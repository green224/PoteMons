using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using Unity.Collections;

namespace IzBone.IzBCollider.RawCollider {

	/** 直方体コライダ */
	public unsafe struct Box : ICollider {
		public float3 pos;			//!< 中心位置
		public float3 xAxis;		//!< X軸方向
		public float3 yAxis;		//!< Y軸方向
		public float3 zAxis;		//!< Z軸方向
		public float3 r;			//!< 各ローカル軸方向の半径

		/** 指定の球がぶつかっていた場合、引き離しベクトルを得る */
		public bool solve(Sphere* s, float3* oColN, float* oColDepth) {
			var d = s->pos - pos;
			var pr = s->r;

			// まずはバウンダリー球で衝突判定
			var dSqLen = lengthsq(d);
			var sumR_x = r.x + pr;
			var sumR_y = r.y + pr;
			var sumR_z = r.z + pr;
			if (sumR_x*sumR_x + sumR_y*sumR_y + sumR_z*sumR_z < dSqLen) return false;

			// 各軸ごとに、内側かどうか、境目からの距離を計算
			var a = new float3(
				dot(d, xAxis),
				dot(d, yAxis),
				dot(d, zAxis)
			) + r;
			bool xInner = false, yInner = false, zInner = false;
			if (0 < a.x) { a.x -= r.x*2; xInner = a.x<0; }
			if (0 < a.y) { a.y -= r.y*2; yInner = a.y<0; }
			if (0 < a.z) { a.z -= r.z*2; zInner = a.z<0; }

			// 内側で衝突
			if (xInner && yInner && zInner) {
				// 押し出し量を計算
				a.x = (a.x < -r.x ? (-2*r.x-a.x - pr) : (-a.x + pr));
				a.y = (a.y < -r.y ? (-2*r.y-a.y - pr) : (-a.y + pr));
				a.z = (a.z < -r.z ? (-2*r.z-a.z - pr) : (-a.z + pr));

				// 最も押し出し量が小さい方向に押し出し
				var absEx = abs(a.x);
				var absEy = abs(a.y);
				var absEz = abs(a.z);
				if (absEy < absEx) {
					if (absEz < absEy) {
						// z方向に押し出し
						*oColN = a.z < 0 ? -zAxis : zAxis;
						*oColDepth = absEz;
						return true;
					}
					// y方向に押し出し
					*oColN = a.y < 0 ? -yAxis : yAxis;
					*oColDepth = absEy;
					return true;
				}
				if (absEz < absEx) {
					// z方向に押し出し
					*oColN = a.z < 0 ? -zAxis : zAxis;
					*oColDepth = absEz;
					return true;
				}
				// x方向に押し出し
				*oColN = a.x < 0 ? -xAxis : xAxis;
				*oColDepth = absEx;
				return true;
			}
			
			// 頂点、辺、面で衝突の可能性
			if (xInner) a.x=0;
			if (yInner) a.y=0;
			if (zInner) a.z=0;
			var sqA = lengthsq(a);
			if ( pr*pr < sqA ) return false;

			var aLen = sqrt(sqA);
			*oColN = (a.x*xAxis + a.y*yAxis + a.z*zAxis) / (aLen + 0.0000001f);
			*oColDepth = pr - aLen;

			return true;
		}
	}

}


