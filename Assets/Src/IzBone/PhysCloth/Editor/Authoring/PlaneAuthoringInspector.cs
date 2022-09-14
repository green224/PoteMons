using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Linq;


namespace IzBone.PhysCloth.Authoring {
using Common;

[CustomEditor(typeof(PlaneAuthoring))]
[CanEditMultipleObjects]
sealed class PlaneAuthoringInspector : BaseAuthoringInspector
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
		var tgt = (PlaneAuthoring)target;

		var tob = tgt._topOfBones?.Where(i=>i!=null)?.ToArray();

		{// 正確なパラメータが指定されているか否かをチェック
			if (tob == null || tob.Length == 0) return;

			int errCnt = 0;
			var rootTrans = tob[0].parent;
			if (rootTrans == null) {
				errCnt = 1;
			} else foreach (var i in tgt._topOfBones) {
				if (i==null) {errCnt = 10; break;}
				if (i.parent != rootTrans) {errCnt = 20; break;}
				if (i.childCount == 0) {errCnt = 30; break;}
			}

			static void showWarn(string msg) => EditorGUILayout.HelpBox(msg, MessageType.Error);
			switch (errCnt) {
			case 1: showWarn("TopOfBonesには共通の親Transformが必要です"); return;
			case 10: showWarn("TopOfBonesにNullが指定されています"); return;
			case 20: showWarn("TopOfBonesには共通の親Transformが必要です"); return;
			case 30: showWarn("TopOfBonesに1つも子供が存在しないTransformが指定されています"); return;
			}
		}

		{// ギズモを描画
			drawPtcl( tob[0].parent, true, 0, 0, 0 );
			var tLst0 = tob.Select(i=>i.parent).ToArray();
			var tLst1 = tob.ToArray();
			for (int dIdx = 0;; ++dIdx) {

				for (int i=0; i<tLst0.Length; ++i) {
					if (tLst1[i] == null) continue;

					var rScl = length(tLst0[i].position - tLst1[i].position);
					drawPtcl( tLst1[i], dIdx==0, tgt.getRadius(dIdx), tgt.getMaxMovableRange(dIdx), rScl );
					drawConnection(tLst0[i], tLst1[i], dIdx==0);

					Transform transL, transR;
					if (i == 0) transL = tgt._isLoopConnect ? tLst1[tLst1.Length-1] : null;
					else transL = tLst1[i-1];
					if (i == tLst1.Length-1) transR = tgt._isLoopConnect ? tLst1[0] : null;
					else transR = tLst1[i+1];

					drawConnection(transR, tLst1[i], dIdx==0);
					drawConnection(transL, tLst1[i], dIdx==0);
				}

				var isAllNull = true;
				for (int i=0; i<tLst0.Length; ++i) {
					tLst0[i] = tLst1[i];
					tLst1[i] = tLst1[i].childCount==0 ? null : tLst1[i].GetChild(0);
					if (tLst1[i] != null) isAllNull = false;
				}
				if (isAllNull) break;
			}
		}

	}
}

}
