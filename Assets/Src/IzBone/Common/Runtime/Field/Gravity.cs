using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.Common.Field {


	/**
	 * 重力を格納する構造体。
	 * 
	 * Physics.gravityを参照しているので、Burst領域には持っていけない。
	 */
	[Serializable]
	public struct Gravity
	{
		public float scale;
		public bool useCustom;
		public float3 customA;

		public Gravity( float scale ) {
			this.scale = scale;
			useCustom = false;
			customA = 0;
		}

		public float3 evaluate() {
			if ( useCustom ) return customA;
			return Physics.gravity * scale;
		}
		public float3 evaluate(float3 defaultG) {
			if ( useCustom ) return customA;
			return defaultG * scale;
		}
	}


}
