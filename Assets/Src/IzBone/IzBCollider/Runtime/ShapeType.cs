using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace IzBone.IzBCollider {

	/** 形状タイプ */
	public enum ShapeType {
		Sphere=0,
		Capsule,
		Box,
		Plane,

		MAX_COUNT
	}

}

