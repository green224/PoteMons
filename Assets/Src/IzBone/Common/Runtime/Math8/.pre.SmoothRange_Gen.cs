#if false	//#Ignore  IDE状でエラー表示させ無くするために無効にしておく
// 両端にスムーズに減衰するマージンを持つ範囲情報。
// Burst対応するために、全てStructで定義している
using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

using MIA = System.Runtime.CompilerServices.MethodImplAttribute;
using MIO = System.Runtime.CompilerServices.MethodImplOptions;



namespace IzBone.Common {
static public partial class Math8 {

	//#	var iTypeList = new [] { "float", "double", };
	//#	foreach ( var iType in iTypeList ) {
	//#		for (int axisNum=1; axisNum<=4; ++axisNum) {
	//#			var type = iType + (axisNum==1?"":axisNum.ToString());
	//#			var typeUp = char.ToUpper(type[0]) + type.Substring(1);
	//#
	public struct SmoothRange_【typeUp】 {

		public 【type】 lclVal;		//!< ローカル座標

		public 【type】 min {[MIA(MIO.AggressiveInlining)] get=>_min;}			//!< 最小値
		public 【type】 max {[MIA(MIO.AggressiveInlining)] get=>_max;}			//!< 最大値
		public 【iType】 marginMin {[MIA(MIO.AggressiveInlining)] get=>_mgnMin;}	//!< 最小値側の補間マージン幅
		public 【iType】 marginMax {[MIA(MIO.AggressiveInlining)] get=>_mgnMax;}	//!< 最大値側の補間マージン幅
		public 【type】 localMin {[MIA(MIO.AggressiveInlining)] get=>_min-_mgnMin;}	//!< 補間前の内部ローカル値の最小値
		public 【type】 localMax {[MIA(MIO.AggressiveInlining)] get=>_max+_mgnMax;}	//!< 補間前の内部ローカル値の最大値

		/** 範囲を初期化する */
		public void reset(【type】 min, 【type】 max, 【iType】 margin) => reset(min,max,margin,margin);
		public void reset(【type】 min, 【type】 max, 【iType】 marginMin, 【iType】 marginMax) {
			_min=min; _max=max; _mgnMin=marginMin; _mgnMax=marginMax;
		}

		// 補間後のグローバル値を取得・設定
		public 【type】 getValue() => local2global( lclVal );
		public void setValue(【type】 value) => lclVal = global2local( value );

		// 補間前のローカル値とグローバル値の相互変換
		public 【type】 local2global(【type】 val) {
			var val01 = clamp(val, _min-_mgnMin, _min+_mgnMin);
			var val23 = clamp(val, _max-_mgnMax, _max+_mgnMax);
			return clamp(
				val,	// 直線補間部分
				_min + (val01*val01 + localMin*(localMin-2*val01) )/(4*_mgnMin),		// 最小値側のマージン
				_max + (val23*val23 + localMax*(localMax-2*val23) )/(4*-_mgnMax)		// 最大値側のマージン
			);
		}
		public 【type】 global2local(【type】 val) {
			val = clamp(val, min, max);
			return clamp(
				lerp(		// 直線補間部分
					lerp(
						localMin,
						val,
						step(_min+_mgnMin, val)
					),
					localMax,
					step(_max-_mgnMax, val)
				),
				localMin + sqrt(4*_mgnMin * (val-_min)),	// 最小値側のマージン
				localMax - sqrt(4*_mgnMax * (_max-val))	// 最大値側のマージン
			);
		}

		【type】 _min, _max;
		【iType】 _mgnMin, _mgnMax;
	}

	//# 	}
	//# }


} }


#endif	//#Ignore
