using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using Unity.Collections;

namespace IzBone.IzBCollider.RawCollider {

	/** 無限平面コライダ */
	public unsafe struct Plane : ICollider {
		public float3 pos;		//!< 平面上の位置
		public float3 dir;		//!< 平面の表方向の向き

		/** 指定の球がぶつかっていた場合、引き離しベクトルを得る */
		public bool solve(Sphere* s, float3* oColN, float* oColDepth) {
			var d = s->pos - pos;

			var dLen = dot(d, dir);
			if (s->r < dLen) return false;

			*oColN = dir;
			*oColDepth = s->r - dLen;

			return true;
		}
	}

}


