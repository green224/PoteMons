using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;



namespace IzBone.IzBCollider {

using Common;

[CanEditMultipleObjects]
[CustomEditor(typeof(BodiesPackAuthoring))]
sealed class BodiesPackAutoringInspector : Editor
{
	void OnSceneGUI() {
		var tgt = (BodiesPackAuthoring)target;

		if (tgt.Bodies != null) foreach (var i in tgt.Bodies) {
			if (i != null) i.DEBUG_drawGizmos();
		}
	}

}

}

