using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.PhysCloth.Authoring {
using Common;

abstract class BaseAuthoringInspector : Editor
{
	override public void OnInspectorGUI() {

		// ギズモ表示用のボタンを表示
		using (new EditorGUILayout.HorizontalScope()) {
			EditorGUILayout.Space();
			if ( GUILayout.Button("Gizmos", GUILayout.Width(60)) ) {
				Common.Windows.GizmoOptionsWindow.open();
			}
		}

		base.OnInspectorGUI();
	}

	virtual protected void OnSceneGUI() {
		Gizmos8.drawMode = Gizmos8.DrawMode.Handle;
		var tgt = (BaseAuthoring)target;

		// 登録されているコライダを表示
		if ( Common.Windows.GizmoOptionsWindow.isShowCollider ) {
			if (tgt.Collider!=null) foreach (var i in tgt.Collider.Bodies) {
				if (i == null) continue;
				i.DEBUG_drawGizmos();
			}
		}

		// コンストレイントを描画
		if ( Common.Windows.GizmoOptionsWindow.isShowConnections ) {
			if (tgt._constraints != null) foreach (var i in tgt._constraints) {
				var p0 = tgt._particles[i.srcPtclIdx].DEBUG_curPos;
				var p1 = tgt._particles[i.dstPtclIdx].DEBUG_curPos;
				drawConnection(p0, p1, false);
			}
		}

		// 質点を描画
		if (tgt._particles != null) foreach (var i in tgt._particles) {
			var pos = i.DEBUG_curPos;
			var r = i.radius;
			var v = i.DEBUG_curV;
			var maxMovableRange = i.maxMovableRange;
			var isFixed = i.m < 0.000001f;

			// パーティクルスケールを得る
			Vector3 tailPosCtr = default;
			foreach (var j in i.transTail) tailPosCtr += j.position;
			tailPosCtr /= i.transTail.Length;
			var rScl = length(i.transTail[0].parent.position - tailPosCtr);

			// パーティクル半径・移動可能距離を描画
			drawPtcl(pos, quaternion(0,0,0,1), isFixed, r, maxMovableRange, rScl);

			// TODO : ここ、矢印にする
			if ( !isFixed && Common.Windows.GizmoOptionsWindow.isShowPtclV ) {
				Gizmos8.color = new Color(0,0,1);
				Gizmos8.drawLine( pos, pos+v*0.03f );
			}

//			if ( Common.Windows.GizmoOptionsWindow.isShowPtclNml ) {
//				var nml = ptcl.wNml;
//				Gizmos8.color = new Color(1,0,0);
//				Gizmos8.drawLine( pos, pos+nml*0.03f );
//			}

			// Fixedな親との接続を表示
			if (isFixed) {
				if (i.parent != null && i.parent.m < 0.000001f) {
					drawConnection(i.parent.DEBUG_curPos, pos, true);
				}
			}
		}
	}

	// パーティクル部分を描画する処理
	static protected void drawPtcl(Transform trans, bool isFixed, float r, float movRange, float rScl)
		=> drawPtcl( trans.position, trans.rotation, isFixed, r, movRange, rScl );
	static protected void drawPtcl(float3 pos, quaternion rot, bool isFixed, float r, float movRange, float rScl) {
		// パーティクル半径を描画
		if ( Common.Windows.GizmoOptionsWindow.isShowPtclR ) {
			Gizmos8.color = isFixed
				? Gizmos8.Colors.JointFixed
				: Gizmos8.Colors.JointMovable;

			var viewR = r * rScl;
			if (isFixed) viewR = HandleUtility.GetHandleSize(pos)*0.1f;

			Gizmos8.drawSphere(pos, viewR);
		}

		// 移動可能距離を描画
		if ( Common.Windows.GizmoOptionsWindow.isShowLimitPos ) {
			if (!isFixed && 0 < movRange) {
				Gizmos8.color = Gizmos8.Colors.ShiftLimit;
				Gizmos8.drawWireCube(pos, rot, movRange*2*rScl);
			}
		}
	}

	// パーティクルの接続を描画する処理
	static protected void drawConnection(Transform trans0, Transform trans1, bool isFixed)
		=> drawConnection(
			trans0 == null ? (float3?)null : trans0.position,
			trans1 == null ? (float3?)null : trans1.position,
			isFixed
		);
	static protected void drawConnection(float3? pos0, float3? pos1, bool isFixed) {
		if (
			pos0 != null && pos1 != null &&
			Common.Windows.GizmoOptionsWindow.isShowConnections
		) {
			Gizmos8.color = isFixed ? Gizmos8.Colors.BoneFixed : Gizmos8.Colors.BoneMovable;
			Gizmos8.drawLine( pos0.Value, pos1.Value );
		}
	}

}

}
