using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using Unity.Collections;

namespace IzBone.IzBCollider.RawCollider {

	/** カプセル形状コライダ */
	public unsafe struct Capsule : ICollider {
		public float3 pos;		//!< 中央位置
		public float r_s;		//!< 横方向の半径
		public float3 dir;		//!< 縦方向の向き
		public float r_h;		//!< 縦方向の長さ

		/** 指定の球がぶつかっていた場合、引き離しベクトルを得る */
		public bool solve(Sphere* s, float3* oColN, float* oColDepth) {
			var d = s->pos - pos;

			// まずはバウンダリー球で衝突判定
			var dSqLen = lengthsq(d);
			var sumR_s = r_s + s->r;
			var sumR_h = r_h + s->r;
			if (sumR_s*sumR_s < dSqLen && sumR_h*sumR_h < dSqLen) return false;

			// 縦方向の位置により距離を再計算
			var len_h = dot(d, dir);
//			if (len_h < -sumR_h || sumR_h < len_h) return false;		// バウンダリー球で判定する場合はこれはいらない
			if (len_h < -r_h+r_s)		d += dir * (r_h-r_s);			// 下側の球との衝突可能性がある場合
			else if (len_h < r_h-r_s)	d -= dir * len_h;				// 中央との衝突可能性がある場合
			else						d += dir * (r_s-r_h);			// 上側の球との衝突可能性がある場合

			// 球vs球の衝突判定
			dSqLen = lengthsq(d);
			if ( sumR_s*sumR_s < dSqLen ) return false;

			var dLen = sqrt(dSqLen);
			*oColN = d / (dLen + 0.0000001f);
			*oColDepth = sumR_s - dLen;

			return true;
		}
	}

}


