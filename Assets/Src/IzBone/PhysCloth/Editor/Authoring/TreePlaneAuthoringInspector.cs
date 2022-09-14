using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Linq;


namespace IzBone.PhysCloth.Authoring {
using Common;

[CustomEditor(typeof(TreePlaneAuthoring))]
[CanEditMultipleObjects]
sealed class TreePlaneAuthoringInspector : BaseAuthoringInspector
{

	override public void OnInspectorGUI() {
		base.OnInspectorGUI();
	}


	/** １オブジェクトに対するOnSceneGUI。基本的に派生先からはこれを拡張すればOK */
	override protected void OnSceneGUI() {
		base.OnSceneGUI();

		// プレイ中のギズモ表示はBaseのものでOK
		if (Application.isPlaying) return;

		Gizmos8.drawMode = Gizmos8.DrawMode.Handle;
		var tgt = (TreePlaneAuthoring)target;

		var tob = tgt._topOfBones?.Where(i=>i!=null)?.ToArray();

		{// 正確なパラメータが指定されているか否かをチェック
			if (tob == null || tob.Length == 0) return;

			// 指定のTransformの親一覧が、指定のTransform配列の何かを含むか否かのチェック
			static bool checkContainSameTrans(Transform tgt, Transform[] refTrans) {
				for (var j=tgt; j!=null; j=j.parent)
					if ( refTrans.Contains(j) ) return true;
				return false;
			}

			int errCnt = 0;
			foreach (var i in tgt._topOfBones) {
				if (i==null) {errCnt = 10; break;}
				if (i.parent==null) {errCnt = 2; break;}
				if (checkContainSameTrans(i.parent, tob)) {errCnt = 21; break;}
				if (i.childCount == 0) {errCnt = 30; break;}
			}

			static void showWarn(string msg) => EditorGUILayout.HelpBox(msg, MessageType.Error);
			switch (errCnt) {
			case 2: showWarn("TopOfBonesに親のないTransformが指定されています"); return;
			case 10: showWarn("TopOfBonesにNullが指定されています"); return;
			case 21: showWarn("TopOfBonesにTree同士が重なってしまう組み合わせが指定されています"); return;
			case 30: showWarn("TopOfBonesに1つも子供が存在しないTransformが指定されています"); return;
			}
		}

		{// ギズモを描画

			// ルートParticleを表示
			var tLst0 = tob.Select(i=>i.parent).ToList();
			foreach (var i in tLst0) drawPtcl( i, true, 0, 0, 0 );

			// TopParticleを表示
			var tLst1 = tob.ToList();
			for (int i=0; i<tLst0.Count; ++i) {
				if (tLst1[i] == null) continue;

				drawPtcl( tLst1[i], true, 0, 0, 0 );
				drawConnection(tLst0[i], tLst1[i], true);

				if (i != 0) drawConnection(tLst1[i-1], tLst1[i], true);
			}

			// それより下のParticleを表示。
			// とりあえず今は左右のコネクションは表示していない
			for (int dIdx = 1; tLst1.Count!=0; ++dIdx) {
				tLst0.Clear();
				for (int i=0; i<tLst1.Count; ++i) {
					var a = tLst1[i];
					for (int j=0; j<a.childCount; ++j) {
						var b = a.GetChild(j);
						tLst0.Add( b );
						var rScl = length(a.position - b.position);
						drawPtcl( b, false, tgt.getRadius(dIdx), tgt.getMaxMovableRange(dIdx), rScl );
						drawConnection(a, b, false);
					}
				}
				(tLst0, tLst1) = (tLst1, tLst0);
			}
		}

	}
}

}
