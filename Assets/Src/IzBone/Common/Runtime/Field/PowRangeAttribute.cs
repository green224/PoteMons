using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.Common.Field {


	/**
	 * floatを対数グラフ的な表示でスライダー表示するための属性。
	 * ShaderのPowerSlider属性と似たような感じ
	 * 
	 * 属性を使わずに、直接処理を使用することがあるので、EditorOnlyにはしていない。
	 */
	internal sealed class PowRangeAttribute : PropertyAttribute
	{
		// 対数の底。0~1,1~∞ が設定可能範囲。0,1は特異点なので指定不可
		public readonly float baseNum;

		// 最大値・最小値
		public readonly float left, right;

		// 表示する値をpoweredValueにするか、もしくは実際の値をpoweredValueにするか
		public readonly bool isShowPowValue;

		public PowRangeAttribute(
			float baseNum, float left, float right,
			bool isShowPowValue = false
		) {
			this.baseNum = baseNum;
			this.left = left;
			this.right = right;
			this.isShowPowValue = isShowPowValue;
		}


		// 表示する値の位置(0～1)と、実際の値(left～right)との相互変換
		public float srcValue2showValue(float srcValue) =>
			(float)srcValue2showValue(srcValue, baseNum, left, right, isShowPowValue);
		public float showValue2srcValue(float showValue) =>
			(float)showValue2srcValue(showValue, baseNum, left, right, isShowPowValue);


		// 表示する値の位置(0～1)と、実際の値(left～right)との相互変換をstaticで行う処理
		static public double srcValue2showValue(
			double srcValue, double baseNum,
			double srcLeft, double srcRight, bool isShowPowValue=false
		) {
			var a = isShowPowValue
				? linValue2powValue(srcValue, baseNum)
				: powValue2linValue(srcValue, baseNum);
			var l = isShowPowValue
				? linValue2powValue(srcLeft, baseNum)
				: powValue2linValue(srcLeft, baseNum);
			var r = isShowPowValue
				? linValue2powValue(srcRight, baseNum)
				: powValue2linValue(srcRight, baseNum);

			// 表示する値は比較的平均的に分布することが期待されるので、この段階で範囲制限を掛ける
			a = (a - l) / (r - l);
			return clamp(a, 0, 1);
		}
		static public double showValue2srcValue(
			double showValue, double baseNum,
			double srcLeft, double srcRight, bool isShowPowValue=false
		) {
			var l = isShowPowValue
				? linValue2powValue(srcLeft, baseNum)
				: powValue2linValue(srcLeft, baseNum);
			var r = isShowPowValue
				? linValue2powValue(srcRight, baseNum)
				: powValue2linValue(srcRight, baseNum);

			// 表示する値は比較的平均的に分布することが期待されるので、この段階で範囲制限を解除する
			var a = clamp(showValue, 0, 1);
			a = lerp( l, r, a );

			return isShowPowValue
				? powValue2linValue(a, baseNum)
				: linValue2powValue(a, baseNum);
		}


		// 対数変化の値(0～∞)と、線形変化の値(-∞～∞)との相互変換。
		static public double linValue2powValue(double linearValue, double baseNum) =>
			pow(baseNum, linearValue);
		static public double powValue2linValue(double poweredValue, double baseNum) =>
			log2(poweredValue) / log2(baseNum);

	}


}
