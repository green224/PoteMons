

// その他のStatic機能を定義
using System;
using Unity.Mathematics;
using System.Runtime.CompilerServices;
using MI = System.Runtime.CompilerServices.MethodImplAttribute; 
using MO = System.Runtime.CompilerServices.MethodImplOptions; 


namespace IzBone.Common {
static public partial class Math8  {

	// 指定の範囲内で、数値を繰り返す
	static public int wrap( int x, int min, int max ) {
		if ( min < x )	return ( (x-min) % (max-min) ) + min;
		else			return max - ( (max-x) % (max-min) );
	}
	static public uint wrap( uint x, uint min, uint max ) {
		if ( min < x )	return ( (x-min) % (max-min) ) + min;
		else			return max - ( (max-x) % (max-min) );
	}
	static public long wrap( long x, long min, long max ) {
		if ( min < x )	return ( (x-min) % (max-min) ) + min;
		else			return max - ( (max-x) % (max-min) );
	}
	static public ulong wrap( ulong x, ulong min, ulong max ) {
		if ( min < x )	return ( (x-min) % (max-min) ) + min;
		else			return max - ( (max-x) % (max-min) );
	}
	static public float wrap( float x, float min, float max ) {
		if ( min < x )	return ( (x-min) % (max-min) ) + min;
		else			return max - ( (max-x) % (max-min) );
	}
	static public double wrap( double x, double min, double max ) {
		if ( min < x )	return ( (x-min) % (max-min) ) + min;
		else			return max - ( (max-x) % (max-min) );
	}
	[MI(MO.AggressiveInlining)] static public int4 wrap(int4 x,int4 min,int4 max) => new int4(wrap(x.x,min.x,max.x),wrap(x.y,min.y,max.y),wrap(x.z,min.z,max.z),wrap(x.w,min.w,max.w));
	[MI(MO.AggressiveInlining)] static public int3 wrap(int3 x,int3 min,int3 max) => new int3(wrap(x.x,min.x,max.x),wrap(x.y,min.y,max.y),wrap(x.z,min.z,max.z));
	[MI(MO.AggressiveInlining)] static public int2 wrap(int2 x,int2 min,int2 max) => new int2(wrap(x.x,min.x,max.x),wrap(x.y,min.y,max.y));
	[MI(MO.AggressiveInlining)] static public uint4 wrap(uint4 x,uint4 min,uint4 max) => new uint4(wrap(x.x,min.x,max.x),wrap(x.y,min.y,max.y),wrap(x.z,min.z,max.z),wrap(x.w,min.w,max.w));
	[MI(MO.AggressiveInlining)] static public uint3 wrap(uint3 x,uint3 min,uint3 max) => new uint3(wrap(x.x,min.x,max.x),wrap(x.y,min.y,max.y),wrap(x.z,min.z,max.z));
	[MI(MO.AggressiveInlining)] static public uint2 wrap(uint2 x,uint2 min,uint2 max) => new uint2(wrap(x.x,min.x,max.x),wrap(x.y,min.y,max.y));
	[MI(MO.AggressiveInlining)] static public float4 wrap(float4 x,float4 min,float4 max) => new float4(wrap(x.x,min.x,max.x),wrap(x.y,min.y,max.y),wrap(x.z,min.z,max.z),wrap(x.w,min.w,max.w));
	[MI(MO.AggressiveInlining)] static public float3 wrap(float3 x,float3 min,float3 max) => new float3(wrap(x.x,min.x,max.x),wrap(x.y,min.y,max.y),wrap(x.z,min.z,max.z));
	[MI(MO.AggressiveInlining)] static public float2 wrap(float2 x,float2 min,float2 max) => new float2(wrap(x.x,min.x,max.x),wrap(x.y,min.y,max.y));
	[MI(MO.AggressiveInlining)] static public double4 wrap(double4 x,double4 min,double4 max) => new double4(wrap(x.x,min.x,max.x),wrap(x.y,min.y,max.y),wrap(x.z,min.z,max.z),wrap(x.w,min.w,max.w));
	[MI(MO.AggressiveInlining)] static public double3 wrap(double3 x,double3 min,double3 max) => new double3(wrap(x.x,min.x,max.x),wrap(x.y,min.y,max.y),wrap(x.z,min.z,max.z));
	[MI(MO.AggressiveInlining)] static public double2 wrap(double2 x,double2 min,double2 max) => new double2(wrap(x.x,min.x,max.x),wrap(x.y,min.y,max.y));

	// 角度を正規化する
	[MI(MO.AggressiveInlining)] static public float wrapDegree(float x) => wrap(x, -180, 180);
	[MI(MO.AggressiveInlining)] static public float wrapRadian(float x) => wrap(x, -(float)Unity.Mathematics.math.PI, (float)Unity.Mathematics.math.PI);
	[MI(MO.AggressiveInlining)] static public double wrapDegree(double x) => wrap(x, -180, 180);
	[MI(MO.AggressiveInlining)] static public double wrapRadian(double x) => wrap(x, -(double)Unity.Mathematics.math.PI, (double)Unity.Mathematics.math.PI);
	[MI(MO.AggressiveInlining)] static public float4 wrapDegree(float4 x) => new float4(wrapDegree(x.x),wrapDegree(x.y),wrapDegree(x.z),wrapDegree(x.w));
	[MI(MO.AggressiveInlining)] static public float3 wrapDegree(float3 x) => new float3(wrapDegree(x.x),wrapDegree(x.y),wrapDegree(x.z));
	[MI(MO.AggressiveInlining)] static public float2 wrapDegree(float2 x) => new float2(wrapDegree(x.x),wrapDegree(x.y));
	[MI(MO.AggressiveInlining)] static public double4 wrapDegree(double4 x) => new double4(wrapDegree(x.x),wrapDegree(x.y),wrapDegree(x.z),wrapDegree(x.w));
	[MI(MO.AggressiveInlining)] static public double3 wrapDegree(double3 x) => new double3(wrapDegree(x.x),wrapDegree(x.y),wrapDegree(x.z));
	[MI(MO.AggressiveInlining)] static public double2 wrapDegree(double2 x) => new double2(wrapDegree(x.x),wrapDegree(x.y));
	[MI(MO.AggressiveInlining)] static public float4 wrapRadian(float4 x) => new float4(wrapRadian(x.x),wrapRadian(x.y),wrapRadian(x.z),wrapRadian(x.w));
	[MI(MO.AggressiveInlining)] static public float3 wrapRadian(float3 x) => new float3(wrapRadian(x.x),wrapRadian(x.y),wrapRadian(x.z));
	[MI(MO.AggressiveInlining)] static public float2 wrapRadian(float2 x) => new float2(wrapRadian(x.x),wrapRadian(x.y));
	[MI(MO.AggressiveInlining)] static public double4 wrapRadian(double4 x) => new double4(wrapRadian(x.x),wrapRadian(x.y),wrapRadian(x.z),wrapRadian(x.w));
	[MI(MO.AggressiveInlining)] static public double3 wrapRadian(double3 x) => new double3(wrapRadian(x.x),wrapRadian(x.y),wrapRadian(x.z));
	[MI(MO.AggressiveInlining)] static public double2 wrapRadian(double2 x) => new double2(wrapRadian(x.x),wrapRadian(x.y));

	// 整数に変更する
	[MI(MO.AggressiveInlining)] static public int4 round2int(float4 x) => new int4(round2int(x.x),round2int(x.y),round2int(x.z),round2int(x.w));
	[MI(MO.AggressiveInlining)] static public int3 round2int(float3 x) => new int3(round2int(x.x),round2int(x.y),round2int(x.z));
	[MI(MO.AggressiveInlining)] static public int2 round2int(float2 x) => new int2(round2int(x.x),round2int(x.y));
	[MI(MO.AggressiveInlining)] static public int4 round2int(double4 x) => new int4(round2int(x.x),round2int(x.y),round2int(x.z),round2int(x.w));
	[MI(MO.AggressiveInlining)] static public int3 round2int(double3 x) => new int3(round2int(x.x),round2int(x.y),round2int(x.z));
	[MI(MO.AggressiveInlining)] static public int2 round2int(double2 x) => new int2(round2int(x.x),round2int(x.y));

	// ベクトルから指定の方向の成分を取り除く
	[MI(MO.AggressiveInlining)] static public float4 rmvDir(float4 src, float4 dir) => src - math.dot(src,dir)*dir;
	[MI(MO.AggressiveInlining)] static public float3 rmvDir(float3 src, float3 dir) => src - math.dot(src,dir)*dir;
	[MI(MO.AggressiveInlining)] static public float2 rmvDir(float2 src, float2 dir) => src - math.dot(src,dir)*dir;
	[MI(MO.AggressiveInlining)] static public double4 rmvDir(double4 src, double4 dir) => src - math.dot(src,dir)*dir;
	[MI(MO.AggressiveInlining)] static public double3 rmvDir(double3 src, double3 dir) => src - math.dot(src,dir)*dir;
	[MI(MO.AggressiveInlining)] static public double2 rmvDir(double2 src, double2 dir) => src - math.dot(src,dir)*dir;

	// smoothstepの直線版
	[MI(MO.AggressiveInlining)] static public float linearStep(float a, float b, float x) => math.saturate((x-a)/(b-a));
	[MI(MO.AggressiveInlining)] static public double linearStep(double a, double b, double x) => math.saturate((x-a)/(b-a));
	[MI(MO.AggressiveInlining)] static public float4 linearStep(float4 a,float4 b,float4 x) => math.saturate((x-a)/(b-a));
	[MI(MO.AggressiveInlining)] static public float3 linearStep(float3 a,float3 b,float3 x) => math.saturate((x-a)/(b-a));
	[MI(MO.AggressiveInlining)] static public float2 linearStep(float2 a,float2 b,float2 x) => math.saturate((x-a)/(b-a));
	[MI(MO.AggressiveInlining)] static public double4 linearStep(double4 a,double4 b,double4 x) => math.saturate((x-a)/(b-a));
	[MI(MO.AggressiveInlining)] static public double3 linearStep(double3 a,double3 b,double3 x) => math.saturate((x-a)/(b-a));
	[MI(MO.AggressiveInlining)] static public double2 linearStep(double2 a,double2 b,double2 x) => math.saturate((x-a)/(b-a));

} }


