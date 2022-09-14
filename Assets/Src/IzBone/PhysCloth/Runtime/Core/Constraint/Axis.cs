using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.PhysCloth.Core.Constraint {

	/** 可動軸方向による拘束条件 */
	public unsafe struct Axis : IConstraint {
		public float3 pos0, pos1;
		public float invM0, invM1;
		public float compliance;
		public float3 axis;

		public bool isValid() => MinimumM < invM0 + invM1;
		public float solve(float sqDt, float lambda) {
			var sumInvM = invM0 + invM1;

			// XPBDでの拘束条件の解決
			var at = compliance / sqDt;    // a~
			//   P = (x,y,z), A:固定軸
			// とすると、
			//   Cj = |P × A| = |B| , B:= P × A
			// であるので、計算すると
			//   ∇Cj = -( B × A ) / |B|
			var p = pos0 - pos1;
			var b = cross( p, axis );
			var bLen = length(b);
			var dCj = -cross(b, axis) / (bLen + 0.0000001f);

			var dlambda = (-bLen - at * lambda) / (dot(dCj,dCj)*sumInvM + at);	// eq.18
			var correction = dlambda * dCj;			// eq.17

			pos0 += +invM0 * correction;
			pos1 += -invM1 * correction;

			return dlambda;
		}

		const float MinimumM = 0.00000001f;
	}

}
