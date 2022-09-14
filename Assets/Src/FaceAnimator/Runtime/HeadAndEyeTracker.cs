using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace FaceAnimator {

/** ヘッドトラッキングとアイトラッキングを同時に行うためのモジュール */
[AddComponentMenu("FaceAnimator/HeadAndEyeTracker")]
[ExecuteAlways]
public sealed class HeadAndEyeTracker : MonoBehaviour {
	//--------------------------- インスペクタに公開しているフィールド ---------------------------

	// ヘッドトラッキング用フィールド
	[SerializeField] HeadTracker _headTracker = null;			//!< 本体モジュール
	[Range(0,1)][SerializeField] float _headTrackPow = 0.8f;	//!< 可動範囲の適応率

	// アイトラッキング用フィールド
	[SerializeField] Core.SimpleController _coreCtrl = null;	//!< 本体モジュール
	[SerializeField] Transform _eyeTrackBase = null;			//!< 頭のTrasnform（目の位置・姿勢の参照元）。Zを顔前方方向へ向けたTransform
	[Range(0,1)][SerializeField] float _eyeTrackRange_x = 0.4f;			//!< 可動範囲X
	[Range(0,1)][SerializeField] float _eyeTrackRange_py = 0.4f;		//!< 可動範囲+Y
	[Range(0,1)][SerializeField] float _eyeTrackRange_my = 0.4f;		//!< 可動範囲-Y

	[Range(0,1)][SerializeField] float _eyeTrackRange_margin = 0.1f;	//!< 可動範囲のマージン率
	[Range(0,1)][SerializeField] float _eyeTrackPow = 0.8f;				//!< 可動範囲の適応率


	//-------------------------------------- public メンバ ---------------------------------------

	public Vector3 lookTgtPos;		//!< ワールド座標での視線位置
	public float3 lookTgtDir;		//!< ワールド座標での視線方向
	[Range(0,1)] public float lookPosDirRate = 0;	//!< 1でDir100%、0でPos100%になる
	[Range(0,1)] public float effectRate = 1;		//!< 影響度


	//-------------------------------------- private メンバ --------------------------------------

	void LateUpdate() {
#if UNITY_EDITOR
		// Editor停止中は、こちらのUpdate経由では更新しない
		if (!Application.isPlaying) return;
#endif
		update(Time.deltaTime);
	}

	/** 更新処理 */
	void update(float dt) {

		{// まずヘッドトラッキングの更新
			_headTracker.lookTgtPos = lookTgtPos;
			_headTracker.lookTgtDir = lookTgtDir;
			_headTracker.lookPosDirRate = lookPosDirRate;
			_headTracker.effect = _headTrackPow * effectRate;
			_headTracker.autoUpdate = false;
			_headTracker.update(dt);
		}

		{// アイトラッキングの更新

			// 頭（視線のベース）ローカル座標系での、視線の向きを決定する
			var etcW2L = _eyeTrackBase.worldToLocalMatrix;
			var lkDir = (float3)etcW2L.MultiplyVector( lookTgtDir );
			if (lookPosDirRate < 0.999f) {
				var dir = etcW2L.MultiplyPoint( lookTgtPos );
				lkDir = lerp(dir, lkDir, lookPosDirRate);
			}

			// 視線の反映レンジを計算
			var maxX = _eyeTrackRange_x * (1f-_eyeTrackRange_margin);
			var maxY = _eyeTrackRange_py * (1f-_eyeTrackRange_margin);
			var minY = _eyeTrackRange_my * (1f-_eyeTrackRange_margin);
			var maxX2 = _eyeTrackRange_x * (1f+_eyeTrackRange_margin);
			var maxY2 = _eyeTrackRange_py * (1f+_eyeTrackRange_margin);
			var minY2 = _eyeTrackRange_my * (1f+_eyeTrackRange_margin);

			// 視線方向から反映値を計算
			lkDir = normalize(lkDir);
			lkDir.x = smoothRange(-lkDir.x, -_eyeTrackRange_x, _eyeTrackRange_x, _eyeTrackRange_margin);
			lkDir.y = smoothRange(lkDir.y, -_eyeTrackRange_my, _eyeTrackRange_py, _eyeTrackRange_margin);

			// コアモジュールへ反映
			_coreCtrl.autoUpdate = false;
			_coreCtrl.eyeTrackPos = lkDir.xy * _eyeTrackPow * effectRate;
			_coreCtrl.update(dt);
		}
	}

	/** スムース領域を持つ-1~1領域へ値を変換する。min<0, 0<max であること */
	static float smoothRange(float value, float min, float max, float marginRate) {
		var min2 = min * (1f+marginRate);
		var min1 = min * (1f-marginRate);
		var max1 = max * (1f-marginRate);
		var max2 = max * (1f+marginRate);
		if (value < min2)	return -1;
		if (value < min1)	return -(1f-marginRate) + (value-min1) / (-min*2);
		if (value < 0)		return value / -min;
		if (value < max1)	return value / max;
		if (value < max2)	return  (1f-marginRate) + (value-max1) / (max*2);
							return 1;
	}


	//--------------------------------------------------------------------------------------------
#if UNITY_EDITOR
	void editorUpdate() {
		if (Application.isPlaying) return;
		if (_headTracker==null || _coreCtrl==null) return;
		update(0);
	}

	void OnEnable() {
		if (!Application.isPlaying)
			UnityEditor.EditorApplication.update += editorUpdate;
	}

	void OnDisable() {
		if (!Application.isPlaying)
			UnityEditor.EditorApplication.update -= editorUpdate;
	}

	[CustomEditor(typeof(HeadAndEyeTracker))]
	sealed class CustomInspector : Editor {
		public override void OnInspectorGUI() {
			serializedObject.Update();

			using (new SubBox("HeadTrackモジュール")) {
				drawProp("本体モジュール","_headTracker");
				drawProp("可動範囲の適応率","_headTrackPow");
			}
			using (new SubBox("EyeTrackモジュール")) {
				drawProp("本体モジュール","_coreCtrl");
				drawProp("頭のTrasnform","_eyeTrackBase", "目の位置・姿勢の参照元。Zを顔前方方向へ向けたTransform");
				drawProp("可動範囲X","_eyeTrackRange_x");
				drawProp("可動範囲+Y","_eyeTrackRange_py");
				drawProp("可動範囲-Y","_eyeTrackRange_my");
				drawProp("可動範囲のﾏｰｼﾞﾝ率","_eyeTrackRange_margin");
				drawProp("可動範囲の適応率","_eyeTrackPow");
			}
			using (new SubBox("メインパラメータ(アニメーション可能)")) {
				drawProp("視線位置","lookTgtPos");
				drawProp("視線方向","lookTgtDir");
				drawProp("視線位置↔方向の割合","lookPosDirRate","1でDir100%、0でPos100%になる");
				drawProp("影響度","effectRate");
			}

			serializedObject.ApplyModifiedProperties();
		}

		sealed class SubBox : IDisposable {
			public SubBox(string label) {
				using (new GUILayout.VerticalScope("Toolbar")) {
					EditorGUILayout.LabelField(label);
				}

				_boxScope = new GUILayout.VerticalScope("box");
				++EditorGUI.indentLevel;
			}
			public void Dispose() {
				--EditorGUI.indentLevel;
				_boxScope.Dispose();
			}
			IDisposable _boxScope;
		}

		/** プロパティフィールドGUIを描画 */
		void drawProp(string label, string propName, string tooltip=null) {
			var prop = serializedObject.FindProperty(propName);
			EditorGUILayout.PropertyField(prop,new GUIContent(label,tooltip));
		}

		void OnSceneGUI() {
			serializedObject.Update();
			
			var etb = (target as HeadAndEyeTracker)?._eyeTrackBase;
			if (etb == null) return;

			var prop = serializedObject.FindProperty("lookTgtPos");
			var ltp = prop.vector3Value;
			var rot = etb.rotation;
			Handles.TransformHandle(ref ltp, ref rot);
			prop.vector3Value = ltp;

			serializedObject.ApplyModifiedProperties();
		}
	}
#endif
}
}
