using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.PhysCloth.Core.Constraint {
	
	/**	最大距離による拘束条件。指定距離未満になるように拘束する */
	public unsafe struct MaxDistance : IConstraint {
		public float3 srcPos;
		public float3 pos;
		public float invM;
		public float compliance;
		public float maxLen;

		public bool isValid() => MinimumM < invM && 0 <= maxLen;
		public float solve(float sqDt, float lambda) {

			// XPBDでの拘束条件の解決
			//   P = (x,y,z)
			// とすると、Distanceのときと同様に、
			//   Cj = |P| - d
			//   ∇Cj = P / |P|
			// また |P| < d のときは
			//   Cj = ∇Cj = 0
			var p = pos - srcPos;
			var pLen = length(p);
			if ( pLen <= maxLen ) return -lambda;

			var at = compliance / sqDt;    // a~
			var dlambda = (maxLen - pLen - at * lambda) / (invM + at);	// eq.18
			var correction = p * (dlambda / pLen);				// eq.17

			pos += invM * correction;

			return dlambda;
		}

		const float MinimumM = 0.00000001f;
	}

}
