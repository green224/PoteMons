using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.PhysCloth.Authoring {
	using Common;
	using Common.Field;
	
	public sealed class ConstraintMng {
		public enum Mode {
			Distance,			// 距離で拘束する
			MaxDistance,		// 指定距離未満になるように拘束する(最大距離を指定)
			Axis,				// 移動可能軸を指定して拘束する
		}
		public Mode mode;
		public int srcPtclIdx, dstPtclIdx;

		public float compliance;
		public float4 param;
	}

}
