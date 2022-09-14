using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using System.Linq;


namespace IzBone.SimpleRig {
using Common;

[CustomEditor(typeof(Skirt))]
[CanEditMultipleObjects]
sealed class SkirtInspector : Editor
{
	void OnSceneGUI() {
		Gizmos8.drawMode = Gizmos8.DrawMode.Handle;
		var tgt = (Skirt)target;
		if (tgt._boneDatas==null || tgt._rootData==null) return;

		// ルート位置からスカート根本への線を表示
		Gizmos8.color = new Color(1,1,0,0.5f);
		var rootL2W = (float4x4)tgt._rootData.trans.localToWorldMatrix;
		foreach (var bone in tgt._boneDatas) {
			Gizmos8.drawLine(
				rootL2W.c3.xyz,
				bone.transTree[0].trans.position
			);
		}

		// スカートボーンのそれぞれの接続を表示
		Gizmos8.color = new Color(1,0,1);
		foreach (var bone in tgt._boneDatas) {
			for (int i=1; i<bone.transTree.Length; ++i) {
				var p0 = (float3)bone.transTree[i-1].trans.position;
				var p1 = (float3)bone.transTree[i].trans.position;
				Gizmos8.drawLine(p0, p1);

				var r = length(p1-p0) / 15;
				Gizmos8.drawSphere( p0, r );
				Gizmos8.drawSphere( p1, r );
			}
			
//			Handles.Label(
//				bone.transTree[0].trans.position,
//				bone.transTree[0].trans.name
//			);
		}

		// ふとももを表示
		if (tgt._legDatas!=null && tgt._boneDatas!=null && tgt._rootData!=null) {

			// スカートのボーン長さの平均
			float boneLen=default;
			foreach (var bone in tgt._boneDatas) {
				boneLen += length(
					bone.transTree[0].trans.position
					- bone.transTree.Last().trans.position
				);
			}
			boneLen /= tgt._boneDatas.Length;

			// ふとももの姿勢を表示
			Gizmos8.color = new Color(0,0,1);
			foreach (var i in tgt._legDatas) {
				var p0 = i.curL2W.c3.xyz;
				var p1 = p0 + Math8.transVector(i.init2curW, tgt._initSkirtWDir*boneLen);
				Gizmos8.drawLine( p0, p1 );
				var r = length(p1-p0) / 30;
				Gizmos8.drawSphere(p0, r);
				Gizmos8.drawSphere(p1, r);
			}

			// ふとももの影響範囲を表示
			Gizmos8.color = new Color(1,1,0,0.2f);
			var effectPosRange = tgt._effectPosRange
				* tgt._maxPosDistCache
				* length( Math8.transVector(rootL2W, float3(1,0,0)) );
			foreach (var i in tgt._legDatas) {
				var p0 = i.curL2W.c3.xyz;
				var p1 = p0 + Math8.transVector(i.init2curW, tgt._initSkirtWDir*boneLen);

				var ctr = (p0+p1)/2;
				var upDir = normalize(p1-p0);
				var s_h = length(p1-p0)/2;
				Gizmos8.drawWireCapsule( ctr, upDir, effectPosRange.x, s_h);
				Gizmos8.drawWireCapsule( ctr, upDir, effectPosRange.y, s_h);
//Gizmos8.color = new Color(1,0,0,0.2f);
//Gizmos8.drawWireCapsule(ctr, initBoneDir, effectPosRange.x, s_h);
			}
		}



	}
}

}
