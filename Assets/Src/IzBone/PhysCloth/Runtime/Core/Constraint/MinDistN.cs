using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.PhysCloth.Core.Constraint {
	
	/** 指定方向最低距離による拘束条件 */
	public unsafe struct MinDistN : IConstraint {
		public float3 srcPos;	// 距離判定用の起点位置
		public float3 n;		// 押し出し方向
		public float3 pos;
		public float invM;
		public float minDist;	// 最小距離。srcからのn方向の距離がこれ未満の場合は押し出しを行う
		public float compliance;

		public bool isValid() => MinimumM < invM;
		public float solve(float sqDt, float lambda) {

			// XPBDでの拘束条件の解決
			//   P = (x,y,z)
			// とすると、
			//   P・n <  minDist  ならば  Cj = P・n - minDist
			//   P・n >= minDist  ならば  Cj = 0
			// であるので、
			//   P・n <  minDist  ならば  ∇Cj = n
			//   P・n >= minDist  ならば  ∇Cj = 0
			// また
			//   P・n <  minDist  ならば  ∇Cj・∇Cj = 1
			//   P・n >= minDist  ならば  ∇Cj・∇Cj = 0
			var cj = dot( pos - srcPos, n ) - minDist;

			float dlambda;
			if (0 < cj) {
				dlambda = -lambda;		// eq.18

			} else {
				var at = compliance / sqDt;    // a~
				dlambda = (-cj - at * lambda) / (invM + at);	// eq.18
				var correction = dlambda * n;						// eq.17

				pos += invM * correction;
			}

			return dlambda;
		}

		const float MinimumM = 0.00000001f;
	}

}
