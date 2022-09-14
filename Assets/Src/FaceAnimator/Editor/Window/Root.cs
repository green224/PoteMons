using System;
using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Unity.Mathematics;
using static Unity.Mathematics.math;



namespace FaceAnimator.Window {

using FaceAnimator.Core;

/**
 * コンバート処理を行うウィンドウ本体
 */
sealed class Root : EditorWindow {

	GameObject srcRootAsset;
	string dstAssetName = "[FaceAnim] {0}";		//!< 出力先アセット名のフォーマット文字列

	/** 変換対象のボーンについてのパラメータ */
	[Serializable] sealed class TgtParam {
		public string boneNameFlt;
		public string animNameFlt;
		public TgtParam(string boneNameFlt, string animNameFlt) {
			this.boneNameFlt = boneNameFlt;
			this.animNameFlt = animNameFlt;
		}
	}
	
	TgtParam tp_eyeTrack = new TgtParam("[Ee]ye_[LR]", "EyeTrack_.*");							//!< 視線
	TgtParam tp_facialEye = new TgtParam("(([Ee]ye[a-zA-Z].*)|([Hh]ead_[Ee]ye.*))", "Eye_.*");	//!< 目周りの表情
	TgtParam tp_facialMouth = new TgtParam("(([Mm]outh.*)|([Hh]ead_[Mm]outh.*))", "Mouth_.*");	//!< 口周りの表情

	/** メニューコマンド */
	[MenuItem("Tools/FaceAnimator")]
	static void create() {
		GetWindow<Root>("FaceAnimator");
	}

	/** GUI描画処理 */
	void OnGUI() {

		var isValidParam = srcRootAsset != null;

        using (new EditorGUILayout.VerticalScope("box")) {
			srcRootAsset = EditorGUILayout.ObjectField( srcRootAsset, typeof( GameObject ), false ) as GameObject;
			dstAssetName = EditorGUILayout.TextField(
				new GUIContent(
					"出力先アセット名",
					"出力先アセット名のフォーマット文字列"
				),
				dstAssetName
			);
		}

		// アニメごとの情報
		Func<TgtParam, string, TgtParam> showTgtPrmGUI = (tgtPrm, ttl) => {
			using (new EditorGUILayout.VerticalScope("box")) {
				EditorGUILayout.LabelField(ttl);
				tgtPrm.animNameFlt = EditorGUILayout.TextField(
					new GUIContent(
						"対象アニメ名",
						"対象アニメーション名の正規表現"
					),
					tgtPrm.animNameFlt
				);
				tgtPrm.boneNameFlt = EditorGUILayout.TextField(
					new GUIContent(
						"対象オブジェクト名",
						"アニメーション内の操作先オブジェクト名の正規表現"
					),
					tgtPrm.boneNameFlt
				);
				return tgtPrm;
			}
		};
		showTgtPrmGUI(tp_eyeTrack, "視線アニメーション");
		showTgtPrmGUI(tp_facialEye, "目周りの表情アニメーション");
		showTgtPrmGUI(tp_facialMouth, "口周りの表情アニメーション");

		using (new EditorGUI.DisabledGroupScope( !isValidParam )) {
			if (GUILayout.Button("実行")) build();
		}

		// 結果ログ
		EditorGUILayout.Space();
//		_logViewer.drawGUI();
	}

	/** 出力処理本体 */
	void build() {
//		Core.Log.instance.reset();

		// アセットを読み込み
		var srcPath = AssetDatabase.GetAssetPath( srcRootAsset );
		var srcSubassets = AssetDatabase.LoadAllAssetsAtPath(srcPath);
		var srcAnims = srcSubassets
			.Select(i => i as AnimationClip)
			.Where(i => i != null)
			.ToArray();

		// 生成元アニメーション情報を収集
		var datas_eyeTrack    = procOneAnimType(srcAnims, tp_eyeTrack,    "視線",         float2(0,0.33f));
		var datas_facialEye   = procOneAnimType(srcAnims, tp_facialEye,   "目周りの表情", float2(0.33f,0.66f));
		var datas_facialMouth = procOneAnimType(srcAnims, tp_facialMouth, "口周りの表情", float2(0.66f,1));
		var allAnims = datas_eyeTrack.Concat(datas_facialEye).Concat(datas_facialMouth).ToArray();

		// アニメーションごとに変換処理
		var pathNames = new HashSet<string>();
		foreach (var i in allAnims) foreach (var j in i) pathNames.Add(j.Key);
		
		// 全アニメーションのパスをマージ
		var paths = new List<MasterData.Path>();
		foreach (var i in pathNames) {
			MasterData.CtrlMode ctrlMode = 0;

			foreach (var j in allAnims) {
				if (j.TryGetValue(i, out var d))	ctrlMode |= d.ctrlMode;
			}

			paths.Add(new MasterData.Path(i, ctrlMode));
		}

		// マスターデータのアセットを生成
		var dstPath = getDstPath( srcRootAsset, ".asset" );
		var dstMD = AssetDatabase.LoadAssetAtPath<MasterData>(dstPath);
		if (dstMD == null) {
			dstMD = ScriptableObject.CreateInstance<MasterData>();
			AssetDatabase.CreateAsset(dstMD, dstPath);
		}

		// マスターデータを構築
		dstMD.setup( paths.ToArray() );
		foreach (var i in allAnims) {
			dstMD.addPose(new MasterData.PoseSet(
				i.name,
				paths.Select(j => {
					if (i.TryGetValue(j.name, out var boneData))
						return boneData.data;
					return Unity.Mathematics.float4x4.zero;
				}).ToArray()
			));
		}

		showPrgBar("保存中", 1);
		EditorUtility.SetDirty(dstMD);
		AssetDatabase.SaveAssets();
		hidePrgBar();
	}

	/** 変換途中処理用のデータ。ボーン一つ分の変換情報 */
	struct CnvBoneData {
		public MasterData.CtrlMode ctrlMode;
		public float4x4 data;
	}

	/** 変換途中処理用のデータ。アニメ一つ分の変換情報 */
	sealed class CnvAnimData : Dictionary<string, CnvBoneData> {
		public string name;
		public CnvAnimData(string name) {
			this.name = name;
		}
	}

	List<CnvAnimData> procOneAnimType(
		AnimationClip[] srcAnims,
		TgtParam tgtPrm,
		string logTtl,
		float2 logRange
	) {
		
		// 生成元アニメーションの取得
		var tgtAnimNameReg = new Regex( tgtPrm.animNameFlt );
		srcAnims = srcAnims
			.Where(i => tgtAnimNameReg.IsMatch(i.name))
			.ToArray();

		// 変換対象アニメーションを抽出
		var ret = new List<CnvAnimData>();
		var tgtObjNameReg = new Regex( "(.*/)*" + tgtPrm.boneNameFlt );
		int iCnt=0;
		foreach (var srcAnim in srcAnims) {
			showPrgBar(
				"生成元アニメを収集: " + logTtl + " (" + ++iCnt + "/" + srcAnims.Length + ")",
				logRange.x + (logRange.y-logRange.x) * ((float)iCnt/srcAnims.Length)
			);

			var oneAnim = new CnvAnimData(srcAnim.name);
			foreach (var j in AnimationUtility.GetCurveBindings( srcAnim )) {
				if ( !tgtObjNameReg.IsMatch( j.path ) ) continue;

				// Transformは複数のパラメータに分割されているので、
				// すでに追加されているパスかどうかをチェック。
				// すでに追加されているパスの場合は結合。
				var isAdded = oneAnim.TryGetValue(j.path, out var data);
				if (!isAdded) {
					data.ctrlMode = 0;
//					data.data.c1.w = 1;
				}

				// 値
				var ac = AnimationUtility.GetEditorCurve(srcAnim, j);
				var val = ac.Evaluate(0);

				// パラメータに値を代入
				if (j.type == typeof(Transform)) {
					if (j.propertyName.StartsWith("m_LocalPosition")) {
						if      (j.propertyName.EndsWith(".x")) data.data.c0.x = val;
						else if (j.propertyName.EndsWith(".y")) data.data.c0.y = val;
						else if (j.propertyName.EndsWith(".z")) data.data.c0.z = val;
						data.ctrlMode |= MasterData.CtrlMode.Transform_Pos;
					} else if (j.propertyName.StartsWith("m_LocalRotation")) {
						if      (j.propertyName.EndsWith(".x")) data.data.c1.x = val;
						else if (j.propertyName.EndsWith(".y")) data.data.c1.y = val;
						else if (j.propertyName.EndsWith(".z")) data.data.c1.z = val;
						else if (j.propertyName.EndsWith(".w")) data.data.c1.w = val;
						data.ctrlMode |= MasterData.CtrlMode.Transform_Rot;
					} else if (j.propertyName.StartsWith("m_LocalScale")) {
						if      (j.propertyName.EndsWith(".x")) data.data.c2.x = val;
						else if (j.propertyName.EndsWith(".y")) data.data.c2.y = val;
						else if (j.propertyName.EndsWith(".z")) data.data.c2.z = val;
						data.ctrlMode |= MasterData.CtrlMode.Transform_Scl;
					} else
						Debug.LogError(
							"path:"+j.path+"\n"+
							"prop:"+j.propertyName+"\n"+
							"type:"+j.type
						);
				} else if (
					j.type == typeof(SkinnedMeshRenderer) ||
					j.type == typeof(MeshRenderer)
				) {
					if (j.propertyName.StartsWith("m_Enabled")) {
						data.data.c0.w = val;
						data.ctrlMode |= MasterData.CtrlMode.Renderer_Enable;
					} else {
						Debug.LogError(
							"path:"+j.path+"\n"+
							"prop:"+j.propertyName+"\n"+
							"type:"+j.type
						);
					}
				} else {
					Debug.LogError(
						"path:"+j.path+"\n"+
						"prop:"+j.propertyName+"\n"+
						"type:"+j.type
					);
				}


				oneAnim[j.path] = data;
			}

			// 最後にデータを整形する
			var keys = oneAnim.Keys.ToArray();
			foreach (var i in keys) {
				var d = oneAnim[i];
				// 回転がQuaternionで格納されているので、Eulerに戻す
				if ((d.ctrlMode & MasterData.CtrlMode.Transform_Rot) != 0) {
					d.data.c1.xyz = ((Quaternion)quaternion( d.data.c1 )).eulerAngles;
				}

				// 使用フラグを1に設定
				d.data.c1.w = 1;

				oneAnim[i] = d;
			}

			ret.Add(oneAnim);
		}

		return ret;
	}


	/** 出力先パスを決定する */
	string getDstPath(UnityEngine.Object srcObj, string ext) {
		var srcPath = AssetDatabase.GetAssetPath( srcObj );
		var srcFN = System.IO.Path.GetFileName(srcPath);
		var srcExt = System.IO.Path.GetExtension(srcPath);
		var srcName = srcPath.Substring(
			srcPath.Length - srcFN.Length,
			srcFN.Length - srcExt.Length
		);
		var srcDirName = srcPath.Substring(
			0,
			srcPath.Length - srcFN.Length
		);
		if (AssetDatabase.IsSubAsset(srcObj)) srcName = srcName + "_" + srcObj.name;
		return srcDirName + string.Format(dstAssetName, srcName) + ext;
	}

	// プログレスバーを表示/非表示
	void showPrgBar(string info, float rate) => EditorUtility.DisplayProgressBar("FaceAnimator", info, rate);
	void hidePrgBar() => EditorUtility.ClearProgressBar();
}

}