using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.PhysCloth.Core.Constraint {
	
#if true
	/** 角度制限による拘束条件 */
	public unsafe struct Angle : IConstraint {
		public float3 pos0, pos1, pos2;
		public float invM0, invM1, invM2;
		public float compliance;
		public float3 defChildPos;	// 初期姿勢でのchildの位置

		public bool isValid() => MinimumM < invM0 + invM1 + invM2;
		public float solve(float sqDt, float lambda) {

			// XPBDでの拘束条件の解決
			internalProcA(out var cj, out var nblACj, out var nblBCj, out var nblCCj);
			return internalProcB(sqDt, lambda, cj, nblACj, nblBCj, nblCCj);
		}

		// Constraint_AngleWithLimit からも同じ処理を行うので、共通化のために処理を分離しておく
		internal void internalProcA(
			out float cj,
			out float3 nblACj,
			out float3 nblBCj,
			out float3 nblCCj
		) {
			// ここの細かい式の情報：https://qiita.com/green224/items/4c2afa3a2f9b2f4b3abd
			var u = normalizesafe( defChildPos - pos1 );
			var v = normalizesafe( cross( cross(u, pos2 - pos1), u ) );
			var a2 = float2( dot(pos1, u), dot(pos1, v) );
			var b2 = float2( dot(pos2, u), dot(pos2, v) );
			var c2 = float2( dot(pos0, u), dot(pos0, v) );
			var s2 = float2( dot(defChildPos, u), dot(defChildPos, v) );
			var p = b2 - a2;
			var q = c2 - a2;
			var o = s2 - a2;
			var t = p / ( lengthsq(p) + 0.0000001f );
			var r = q / ( lengthsq(q) + 0.0000001f );

			var phiBase = atan2(q.y, q.x);
			var phi0 = atan2(o.y, o.x) - phiBase;
			var phi = atan2(p.y, p.x) - phiBase;

			cj = phi - phi0;
			nblACj = (t.y-r.y)*u + (r.x-t.x)*v;	// ∇_ACj
			nblBCj =      -t.y*u + t.x*v;		// ∇_BCj
			nblCCj =       r.y*u - r.x*v;		// ∇_CCj
		}
		internal float internalProcB(
			float sqDt,
			float lambda,
			float cj,
			float3 nblACj,
			float3 nblBCj,
			float3 nblCCj
		) {
			var at = compliance / sqDt;    // a~
			var dlambda =
				(-cj - at * lambda) / (
					lengthsq(nblACj)*invM1 +
					lengthsq(nblBCj)*invM2 +
					lengthsq(nblCCj)*invM0 +
					at
				);									// eq.18

			pos1   += invM1   * dlambda * nblACj;		// eq.17
			pos2  += invM2  * dlambda * nblBCj;			// eq.17
			pos0 += invM0 * dlambda * nblCCj;			// eq.17
			return dlambda;
		}


		const float MinimumM = 0.00000001f;
	}

	/**
	 * 角度制限による拘束条件。
	 * 通常の角度拘束と、上限差分角度拘束を二つ同時に行うことで、計算負荷を下げたもの
	 */
	public unsafe struct AngleWithLimit {
		public Angle aglCstr;				// 通常の角度拘束
		public float compliance_nutral;		// 常にかかる角度拘束のコンプライアンス値
		public float compliance_limit;		// 制限角度を超えた際にかかる角度拘束のコンプライアンス値
		public float limitAngle;			// 制限角度。ラジアン角

		public void solve(
			float sqDt,
			ref float lambda_nutral,
			ref float lambda_limit
		) {
			// XPBDでの拘束条件の解決
			aglCstr.internalProcA(out var cj, out var nblACj, out var nblBCj, out var nblCCj);

			// 角度制限の上限に達しているか否かでコンプライアンス値とコンストレイント値を変更
			if (cj < -limitAngle || limitAngle < cj) {
				cj -= sign(cj) * limitAngle;
				aglCstr.compliance = compliance_limit;
				var dLambda = aglCstr.internalProcB(sqDt,lambda_limit,cj,nblACj,nblBCj,nblCCj);
//				lambda_nutral = lambda_nutral;
				lambda_limit += dLambda;
			} else {
				aglCstr.compliance = compliance_nutral;
				var dLambda = aglCstr.internalProcB(sqDt,lambda_nutral,cj,nblACj,nblBCj,nblCCj);
				lambda_nutral += dLambda;
				lambda_limit = 0;
			}
		}
	}

#else
	/** 角度制限による拘束条件 */
	public unsafe struct Angle : IConstraint {
		public float3 pos0, pos1, pos2;
		public float invM0, invM1, invM2;
		public float compliance;
		public float3 defChildPos;	// 初期姿勢でのchildの位置

		public bool isValid() => MinimumM < invM0 + invM1 + invM2;
		public float solve(float sqDt, float lambda) {

			// XPBDでの拘束条件の解決
			var at = compliance / sqDt;    // a~

			// ここの細かい式の情報：https://qiita.com/green224/items/4c2afa3a2f9b2f4b3abd
			var u = normalizesafe( defChildPos - pos1 );
			var v = normalizesafe( cross( cross(u, pos2 - pos1), u ) );
			var a2 = float2( dot(pos1, u), dot(pos1, v) );
			var b2 = float2( dot(pos2, u), dot(pos2, v) );
			var c2 = float2( dot(pos0, u), dot(pos0, v) );
			var s2 = float2( dot(defChildPos, u), dot(defChildPos, v) );
			var p = b2 - a2;
			var q = c2 - a2;
			var o = s2 - a2;
			var t = p / ( lengthsq(p) + 0.0000001f );
			var r = q / ( lengthsq(q) + 0.0000001f );

			var phiBase = atan2(q.y, q.x);
			var phi0 = atan2(o.y, o.x) - phiBase;
			var phi = atan2(p.y, p.x) - phiBase;

			var cj = phi - phi0;
			var nblACj = (t.y-r.y)*u + (r.x-t.x)*v;		// ∇_ACj
			var nblBCj =      -t.y*u + t.x*v;			// ∇_BCj
			var nblCCj =       r.y*u - r.x*v;			// ∇_CCj

			var dlambda =
				(-cj - at * lambda) / (
					lengthsq(nblACj)*invM1 +
					lengthsq(nblBCj)*invM2 +
					lengthsq(nblCCj)*invM0 +
					at
				);									// eq.18

			pos1   += invM1   * dlambda * nblACj;			// eq.17
			pos2  += invM2  * dlambda * nblBCj;			// eq.17
			pos0 += invM0 * dlambda * nblCCj;			// eq.17

			return dlambda;
		}

		const float MinimumM = 0.00000001f;
	}
#endif

}
