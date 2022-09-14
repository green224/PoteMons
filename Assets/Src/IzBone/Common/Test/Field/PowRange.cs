using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;

using NUnit.Framework;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;


namespace IzBone.Common.Field {


static class PowRange
{

	// SrcValueとPowValueの相互変換が正常に行われているかどうかのチェック
	[Test] public static void checkSrcEqualDst() {

		static void checkOne(float baseNum, float srcVal, bool usePowLR) {
			var showVal = usePowLR
				? PowRangeAttribute.srcValue2showValue(srcVal, baseNum, 0.001, 100, false)
				: PowRangeAttribute.srcValue2showValue(srcVal, baseNum, -1, 1, true);
			var dstVal = usePowLR
				? PowRangeAttribute.showValue2srcValue(showVal, baseNum, 0.001, 100, false)
				: PowRangeAttribute.showValue2srcValue(showVal, baseNum, -1, 1, true);

			Assert.IsTrue(
				abs(srcVal - dstVal) < 0.00001f,
				"invalid: src="+srcVal+", dst="+dstVal+", base="+baseNum
			);
		}

		for (int i=0; i<100; ++i) {
			var baseNum =
				UnityEngine.Random.Range(0,2) == 0
				? UnityEngine.Random.Range(0.0001f,0.9999f)
				: UnityEngine.Random.Range(1.0001f,10000);

			var srcVal = UnityEngine.Random.Range(0f,1f);

			checkOne( baseNum, srcVal, UnityEngine.Random.Range(0,2)==0 );
		}
	}


}


}
