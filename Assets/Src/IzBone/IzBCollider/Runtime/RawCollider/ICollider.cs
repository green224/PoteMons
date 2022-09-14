using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using Unity.Collections;

namespace IzBone.IzBCollider.RawCollider {

	/** コライダのinterface */
	public unsafe interface ICollider {
		/** 指定の球がぶつかっていた場合、引き離しベクトルを得る */
		public bool solve(Sphere* s, float3* oColN, float* oColDepth);
	}

}


