using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using Unity.Collections;

namespace IzBone.IzBCollider.RawCollider {

	/** 球形コライダ */
	public unsafe struct Sphere : ICollider {
		public float3 pos;
		public float r;

		/** 指定の球がぶつかっていた場合、引き離しベクトルを得る */
		public bool solve(Sphere* s, float3* oColN, float* oColDepth) {
			var d = s->pos - pos;
			var dSqLen = lengthsq(d);
			var sumR = r + s->r;
			if ( sumR*sumR < dSqLen ) return false;

			var dLen = sqrt(dSqLen);
			*oColN = d / (dLen + 0.0000001f);
			*oColDepth = sumR - dLen;
			
			return true;
		}
	}

}


