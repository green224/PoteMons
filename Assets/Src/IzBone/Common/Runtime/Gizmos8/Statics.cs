
// OnDrawGizmosで使用できるように、いつでもインクルード可能にしているが、
// Editorでのみ有効である必要があるので、ここで有効無効を切り替える
#if UNITY_EDITOR

using Unity.Mathematics;
using static Unity.Mathematics.math;

using System;
using UnityEditor;
using UnityEngine;

namespace IzBone.Common {
static internal partial class Gizmos8 {

	/** 指定のTransformツリー同士が関係しているか否か */
	public static bool isRelationTransformTrees(Transform a, Transform b) {
		for (var i=a; i!=null; i=i.parent)
			if (b == i) return true;
		for (var i=b; i!=null; i=i.parent)
			if (a == i) return true;
		return false;
	}

	/** 指定のTransformツリーが選択に含まれているか否か */
	public static bool isSelectedTransformTree(UnityEngine.Object[] selection, Transform tree) {
		foreach (var i in selection) {
			var go = i as GameObject;
			if (go == null) continue;
			if (isRelationTransformTrees(go.transform, tree)) return true;
		}
		return false;
	}

	/** 指定の座標リストでボーン繋ぎを描画する */
	public static void drawBones(float3[] posLst) => drawBones(posLst, new Color(1,0,1,0.5f));
	public static void drawBones(float3[] posLst, Color col) {
		using (new ColorScope(col)) {

			// 繋がりの線を描画
			float maxLen = 0;
			for (int i=1; i<posLst.Length; ++i) {
				maxLen = max( maxLen, length(posLst[i-1] - posLst[i]) );
				drawLine(posLst[i-1], posLst[i]);
			}

			// 間接位置を描画
			for (int i=0; i<posLst.Length; ++i)
				drawSphere(posLst[i], 0.1f*maxLen);
		}
	}

} }
#endif
