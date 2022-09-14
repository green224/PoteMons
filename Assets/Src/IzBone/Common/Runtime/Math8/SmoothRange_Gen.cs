// 両端にスムーズに減衰するマージンを持つ範囲情報。
// Burst対応するために、全てStructで定義している
using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;

using MIA = System.Runtime.CompilerServices.MethodImplAttribute;
using MIO = System.Runtime.CompilerServices.MethodImplOptions;



namespace IzBone.Common {
static public partial class Math8  {

	public struct SmoothRange_Float {

		public float lclVal;		//!< ローカル座標

		public float min {[MIA(MIO.AggressiveInlining)] get=>_min;}			//!< 最小値
		public float max {[MIA(MIO.AggressiveInlining)] get=>_max;}			//!< 最大値
		public float marginMin {[MIA(MIO.AggressiveInlining)] get=>_mgnMin;}	//!< 最小値側の補間マージン幅
		public float marginMax {[MIA(MIO.AggressiveInlining)] get=>_mgnMax;}	//!< 最大値側の補間マージン幅
		public float localMin {[MIA(MIO.AggressiveInlining)] get=>_min-_mgnMin;}	//!< 補間前の内部ローカル値の最小値
		public float localMax {[MIA(MIO.AggressiveInlining)] get=>_max+_mgnMax;}	//!< 補間前の内部ローカル値の最大値

		/** 範囲を初期化する */
		public void reset(float min, float max, float margin) => reset(min,max,margin,margin);
		public void reset(float min, float max, float marginMin, float marginMax) {
			_min=min; _max=max; _mgnMin=marginMin; _mgnMax=marginMax;
		}

		// 補間後のグローバル値を取得・設定
		public float getValue() => local2global( lclVal );
		public void setValue(float value) => lclVal = global2local( value );

		// 補間前のローカル値とグローバル値の相互変換
		public float local2global(float val) {
			var val01 = clamp(val, _min-_mgnMin, _min+_mgnMin);
			var val23 = clamp(val, _max-_mgnMax, _max+_mgnMax);
			return clamp(
				val,	// 直線補間部分
				_min + (val01*val01 + localMin*(localMin-2*val01) )/(4*_mgnMin),		// 最小値側のマージン
				_max + (val23*val23 + localMax*(localMax-2*val23) )/(4*-_mgnMax)		// 最大値側のマージン
			);
		}
		public float global2local(float val) {
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

		float _min, _max;
		float _mgnMin, _mgnMax;
	}

	public struct SmoothRange_Float2 {

		public float2 lclVal;		//!< ローカル座標

		public float2 min {[MIA(MIO.AggressiveInlining)] get=>_min;}			//!< 最小値
		public float2 max {[MIA(MIO.AggressiveInlining)] get=>_max;}			//!< 最大値
		public float marginMin {[MIA(MIO.AggressiveInlining)] get=>_mgnMin;}	//!< 最小値側の補間マージン幅
		public float marginMax {[MIA(MIO.AggressiveInlining)] get=>_mgnMax;}	//!< 最大値側の補間マージン幅
		public float2 localMin {[MIA(MIO.AggressiveInlining)] get=>_min-_mgnMin;}	//!< 補間前の内部ローカル値の最小値
		public float2 localMax {[MIA(MIO.AggressiveInlining)] get=>_max+_mgnMax;}	//!< 補間前の内部ローカル値の最大値

		/** 範囲を初期化する */
		public void reset(float2 min, float2 max, float margin) => reset(min,max,margin,margin);
		public void reset(float2 min, float2 max, float marginMin, float marginMax) {
			_min=min; _max=max; _mgnMin=marginMin; _mgnMax=marginMax;
		}

		// 補間後のグローバル値を取得・設定
		public float2 getValue() => local2global( lclVal );
		public void setValue(float2 value) => lclVal = global2local( value );

		// 補間前のローカル値とグローバル値の相互変換
		public float2 local2global(float2 val) {
			var val01 = clamp(val, _min-_mgnMin, _min+_mgnMin);
			var val23 = clamp(val, _max-_mgnMax, _max+_mgnMax);
			return clamp(
				val,	// 直線補間部分
				_min + (val01*val01 + localMin*(localMin-2*val01) )/(4*_mgnMin),		// 最小値側のマージン
				_max + (val23*val23 + localMax*(localMax-2*val23) )/(4*-_mgnMax)		// 最大値側のマージン
			);
		}
		public float2 global2local(float2 val) {
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

		float2 _min, _max;
		float _mgnMin, _mgnMax;
	}

	public struct SmoothRange_Float3 {

		public float3 lclVal;		//!< ローカル座標

		public float3 min {[MIA(MIO.AggressiveInlining)] get=>_min;}			//!< 最小値
		public float3 max {[MIA(MIO.AggressiveInlining)] get=>_max;}			//!< 最大値
		public float marginMin {[MIA(MIO.AggressiveInlining)] get=>_mgnMin;}	//!< 最小値側の補間マージン幅
		public float marginMax {[MIA(MIO.AggressiveInlining)] get=>_mgnMax;}	//!< 最大値側の補間マージン幅
		public float3 localMin {[MIA(MIO.AggressiveInlining)] get=>_min-_mgnMin;}	//!< 補間前の内部ローカル値の最小値
		public float3 localMax {[MIA(MIO.AggressiveInlining)] get=>_max+_mgnMax;}	//!< 補間前の内部ローカル値の最大値

		/** 範囲を初期化する */
		public void reset(float3 min, float3 max, float margin) => reset(min,max,margin,margin);
		public void reset(float3 min, float3 max, float marginMin, float marginMax) {
			_min=min; _max=max; _mgnMin=marginMin; _mgnMax=marginMax;
		}

		// 補間後のグローバル値を取得・設定
		public float3 getValue() => local2global( lclVal );
		public void setValue(float3 value) => lclVal = global2local( value );

		// 補間前のローカル値とグローバル値の相互変換
		public float3 local2global(float3 val) {
			var val01 = clamp(val, _min-_mgnMin, _min+_mgnMin);
			var val23 = clamp(val, _max-_mgnMax, _max+_mgnMax);
			return clamp(
				val,	// 直線補間部分
				_min + (val01*val01 + localMin*(localMin-2*val01) )/(4*_mgnMin),		// 最小値側のマージン
				_max + (val23*val23 + localMax*(localMax-2*val23) )/(4*-_mgnMax)		// 最大値側のマージン
			);
		}
		public float3 global2local(float3 val) {
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

		float3 _min, _max;
		float _mgnMin, _mgnMax;
	}

	public struct SmoothRange_Float4 {

		public float4 lclVal;		//!< ローカル座標

		public float4 min {[MIA(MIO.AggressiveInlining)] get=>_min;}			//!< 最小値
		public float4 max {[MIA(MIO.AggressiveInlining)] get=>_max;}			//!< 最大値
		public float marginMin {[MIA(MIO.AggressiveInlining)] get=>_mgnMin;}	//!< 最小値側の補間マージン幅
		public float marginMax {[MIA(MIO.AggressiveInlining)] get=>_mgnMax;}	//!< 最大値側の補間マージン幅
		public float4 localMin {[MIA(MIO.AggressiveInlining)] get=>_min-_mgnMin;}	//!< 補間前の内部ローカル値の最小値
		public float4 localMax {[MIA(MIO.AggressiveInlining)] get=>_max+_mgnMax;}	//!< 補間前の内部ローカル値の最大値

		/** 範囲を初期化する */
		public void reset(float4 min, float4 max, float margin) => reset(min,max,margin,margin);
		public void reset(float4 min, float4 max, float marginMin, float marginMax) {
			_min=min; _max=max; _mgnMin=marginMin; _mgnMax=marginMax;
		}

		// 補間後のグローバル値を取得・設定
		public float4 getValue() => local2global( lclVal );
		public void setValue(float4 value) => lclVal = global2local( value );

		// 補間前のローカル値とグローバル値の相互変換
		public float4 local2global(float4 val) {
			var val01 = clamp(val, _min-_mgnMin, _min+_mgnMin);
			var val23 = clamp(val, _max-_mgnMax, _max+_mgnMax);
			return clamp(
				val,	// 直線補間部分
				_min + (val01*val01 + localMin*(localMin-2*val01) )/(4*_mgnMin),		// 最小値側のマージン
				_max + (val23*val23 + localMax*(localMax-2*val23) )/(4*-_mgnMax)		// 最大値側のマージン
			);
		}
		public float4 global2local(float4 val) {
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

		float4 _min, _max;
		float _mgnMin, _mgnMax;
	}

	public struct SmoothRange_Double {

		public double lclVal;		//!< ローカル座標

		public double min {[MIA(MIO.AggressiveInlining)] get=>_min;}			//!< 最小値
		public double max {[MIA(MIO.AggressiveInlining)] get=>_max;}			//!< 最大値
		public double marginMin {[MIA(MIO.AggressiveInlining)] get=>_mgnMin;}	//!< 最小値側の補間マージン幅
		public double marginMax {[MIA(MIO.AggressiveInlining)] get=>_mgnMax;}	//!< 最大値側の補間マージン幅
		public double localMin {[MIA(MIO.AggressiveInlining)] get=>_min-_mgnMin;}	//!< 補間前の内部ローカル値の最小値
		public double localMax {[MIA(MIO.AggressiveInlining)] get=>_max+_mgnMax;}	//!< 補間前の内部ローカル値の最大値

		/** 範囲を初期化する */
		public void reset(double min, double max, double margin) => reset(min,max,margin,margin);
		public void reset(double min, double max, double marginMin, double marginMax) {
			_min=min; _max=max; _mgnMin=marginMin; _mgnMax=marginMax;
		}

		// 補間後のグローバル値を取得・設定
		public double getValue() => local2global( lclVal );
		public void setValue(double value) => lclVal = global2local( value );

		// 補間前のローカル値とグローバル値の相互変換
		public double local2global(double val) {
			var val01 = clamp(val, _min-_mgnMin, _min+_mgnMin);
			var val23 = clamp(val, _max-_mgnMax, _max+_mgnMax);
			return clamp(
				val,	// 直線補間部分
				_min + (val01*val01 + localMin*(localMin-2*val01) )/(4*_mgnMin),		// 最小値側のマージン
				_max + (val23*val23 + localMax*(localMax-2*val23) )/(4*-_mgnMax)		// 最大値側のマージン
			);
		}
		public double global2local(double val) {
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

		double _min, _max;
		double _mgnMin, _mgnMax;
	}

	public struct SmoothRange_Double2 {

		public double2 lclVal;		//!< ローカル座標

		public double2 min {[MIA(MIO.AggressiveInlining)] get=>_min;}			//!< 最小値
		public double2 max {[MIA(MIO.AggressiveInlining)] get=>_max;}			//!< 最大値
		public double marginMin {[MIA(MIO.AggressiveInlining)] get=>_mgnMin;}	//!< 最小値側の補間マージン幅
		public double marginMax {[MIA(MIO.AggressiveInlining)] get=>_mgnMax;}	//!< 最大値側の補間マージン幅
		public double2 localMin {[MIA(MIO.AggressiveInlining)] get=>_min-_mgnMin;}	//!< 補間前の内部ローカル値の最小値
		public double2 localMax {[MIA(MIO.AggressiveInlining)] get=>_max+_mgnMax;}	//!< 補間前の内部ローカル値の最大値

		/** 範囲を初期化する */
		public void reset(double2 min, double2 max, double margin) => reset(min,max,margin,margin);
		public void reset(double2 min, double2 max, double marginMin, double marginMax) {
			_min=min; _max=max; _mgnMin=marginMin; _mgnMax=marginMax;
		}

		// 補間後のグローバル値を取得・設定
		public double2 getValue() => local2global( lclVal );
		public void setValue(double2 value) => lclVal = global2local( value );

		// 補間前のローカル値とグローバル値の相互変換
		public double2 local2global(double2 val) {
			var val01 = clamp(val, _min-_mgnMin, _min+_mgnMin);
			var val23 = clamp(val, _max-_mgnMax, _max+_mgnMax);
			return clamp(
				val,	// 直線補間部分
				_min + (val01*val01 + localMin*(localMin-2*val01) )/(4*_mgnMin),		// 最小値側のマージン
				_max + (val23*val23 + localMax*(localMax-2*val23) )/(4*-_mgnMax)		// 最大値側のマージン
			);
		}
		public double2 global2local(double2 val) {
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

		double2 _min, _max;
		double _mgnMin, _mgnMax;
	}

	public struct SmoothRange_Double3 {

		public double3 lclVal;		//!< ローカル座標

		public double3 min {[MIA(MIO.AggressiveInlining)] get=>_min;}			//!< 最小値
		public double3 max {[MIA(MIO.AggressiveInlining)] get=>_max;}			//!< 最大値
		public double marginMin {[MIA(MIO.AggressiveInlining)] get=>_mgnMin;}	//!< 最小値側の補間マージン幅
		public double marginMax {[MIA(MIO.AggressiveInlining)] get=>_mgnMax;}	//!< 最大値側の補間マージン幅
		public double3 localMin {[MIA(MIO.AggressiveInlining)] get=>_min-_mgnMin;}	//!< 補間前の内部ローカル値の最小値
		public double3 localMax {[MIA(MIO.AggressiveInlining)] get=>_max+_mgnMax;}	//!< 補間前の内部ローカル値の最大値

		/** 範囲を初期化する */
		public void reset(double3 min, double3 max, double margin) => reset(min,max,margin,margin);
		public void reset(double3 min, double3 max, double marginMin, double marginMax) {
			_min=min; _max=max; _mgnMin=marginMin; _mgnMax=marginMax;
		}

		// 補間後のグローバル値を取得・設定
		public double3 getValue() => local2global( lclVal );
		public void setValue(double3 value) => lclVal = global2local( value );

		// 補間前のローカル値とグローバル値の相互変換
		public double3 local2global(double3 val) {
			var val01 = clamp(val, _min-_mgnMin, _min+_mgnMin);
			var val23 = clamp(val, _max-_mgnMax, _max+_mgnMax);
			return clamp(
				val,	// 直線補間部分
				_min + (val01*val01 + localMin*(localMin-2*val01) )/(4*_mgnMin),		// 最小値側のマージン
				_max + (val23*val23 + localMax*(localMax-2*val23) )/(4*-_mgnMax)		// 最大値側のマージン
			);
		}
		public double3 global2local(double3 val) {
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

		double3 _min, _max;
		double _mgnMin, _mgnMax;
	}

	public struct SmoothRange_Double4 {

		public double4 lclVal;		//!< ローカル座標

		public double4 min {[MIA(MIO.AggressiveInlining)] get=>_min;}			//!< 最小値
		public double4 max {[MIA(MIO.AggressiveInlining)] get=>_max;}			//!< 最大値
		public double marginMin {[MIA(MIO.AggressiveInlining)] get=>_mgnMin;}	//!< 最小値側の補間マージン幅
		public double marginMax {[MIA(MIO.AggressiveInlining)] get=>_mgnMax;}	//!< 最大値側の補間マージン幅
		public double4 localMin {[MIA(MIO.AggressiveInlining)] get=>_min-_mgnMin;}	//!< 補間前の内部ローカル値の最小値
		public double4 localMax {[MIA(MIO.AggressiveInlining)] get=>_max+_mgnMax;}	//!< 補間前の内部ローカル値の最大値

		/** 範囲を初期化する */
		public void reset(double4 min, double4 max, double margin) => reset(min,max,margin,margin);
		public void reset(double4 min, double4 max, double marginMin, double marginMax) {
			_min=min; _max=max; _mgnMin=marginMin; _mgnMax=marginMax;
		}

		// 補間後のグローバル値を取得・設定
		public double4 getValue() => local2global( lclVal );
		public void setValue(double4 value) => lclVal = global2local( value );

		// 補間前のローカル値とグローバル値の相互変換
		public double4 local2global(double4 val) {
			var val01 = clamp(val, _min-_mgnMin, _min+_mgnMin);
			var val23 = clamp(val, _max-_mgnMax, _max+_mgnMax);
			return clamp(
				val,	// 直線補間部分
				_min + (val01*val01 + localMin*(localMin-2*val01) )/(4*_mgnMin),		// 最小値側のマージン
				_max + (val23*val23 + localMax*(localMax-2*val23) )/(4*-_mgnMax)		// 最大値側のマージン
			);
		}
		public double4 global2local(double4 val) {
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

		double4 _min, _max;
		double _mgnMin, _mgnMax;
	}



} }


