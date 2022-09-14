using System;
using Unity.Mathematics;
using System.Runtime.CompilerServices;
using MI = System.Runtime.CompilerServices.MethodImplAttribute; 
using MO = System.Runtime.CompilerServices.MethodImplOptions; 


namespace IzBone.Common {
static public partial class Math8 {

	/** 半減期による減少を計算する。時刻0のときの値を1とする */
	[MI(MO.AggressiveInlining)]
	static public double calcHL( double hl, double dt ) => math.pow( 2.0, -dt / hl );

	/** 半減期による減少を計算する。時刻0のときの値を1とする */
	[MI(MO.AggressiveInlining)]
	static public float calcHL( float hl, float dt ) => math.pow( 2.0f, -dt / hl );

	/** 半減期によって減少する値の積分を計算する。時刻0のときの値を1とする */
	[MI(MO.AggressiveInlining)]
	static public double calcIntegralHL( double hl, double dt ) =>
		1.44269504089 * hl * ( 1.0 - math.pow( 2.0, -dt / hl ) );		// 1/ln(2) ≃ 1.442

	/** 半減期によって減少する値の積分を計算する。時刻0のときの値を1とする */
	[MI(MO.AggressiveInlining)]
	static public float calcIntegralHL( float hl, float dt ) =>
		1.44269504089f * hl * ( 1.0f - math.pow( 2.0f, -dt / hl ) );		// 1/ln(2) ≃ 1.442

}
}

