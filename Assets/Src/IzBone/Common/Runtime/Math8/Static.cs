// その他のStatic機能を定義
using System;
using Unity.Mathematics;
using System.Runtime.CompilerServices;



namespace IzBone.Common {
static public partial class Math8 {

// とりあえず不要そうなものはコメントアウトした
//	const double Pi = 3.1415926535897932384626433832795;
//	const double Pi2 = 6.283185307179586476925286766559;
//	const double PiHalf = 1.5707963267948966192313216916398;

	// 三角関数
	static public float2  sincos(float x)  => new float2(  math.sin(x), math.cos(x) );
	static public double2 sincos(double x) => new double2( math.sin(x), math.cos(x) );

	// 正負方向のサイズ1の値に変更する
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public float sign(float x) => 0<x?1:-1;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public double sign(double x) => 0<x?1:-1;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public int sign(int x) => 0<x?1:-1;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public decimal sign(decimal x) => 0<x?1:-1;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public long sign(long x) => 0<x?1:-1;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public short sign(short x) => 0<x?(short)1:(short)-1;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public sbyte sign(sbyte x) => 0<x?(sbyte)1:(sbyte)-1;

	// 絶対値に変更する
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public float abs(float x) => x<0?-x:x;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public double abs(double x) => x<0?-x:x;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public int abs(int x) => x<0?-x:x;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public decimal abs(decimal x) => x<0?-x:x;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public long abs(long x) => x<0?-x:x;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public short abs(short x) => x<0?(short)-x:x;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public sbyte abs(sbyte x) => x<0?(sbyte)-x:x;

	// 整数に変更する
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public int round2int(float x) => (int)math.round(x);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static public int round2int(double x) => (int)math.round(x);

//	// そのほかの標準的な数学関数
//	static public float pow(float x,float p) => Mathf.Pow(x,p);
//	static public double pow(double x,double p) => Math.Pow(x,p);
//	static public float sqrt(float x) => Mathf.Sqrt(x);
//	static public double sqrt(double x) => Math.Sqrt(x);

	// 同次変換行列を使用した位置回転拡縮の変換を行う
	static public float3 trans(float4x4 mtx, float3 src) => math.mul( mtx, new float4(src,1) ).xyz;
	// 同次変換行列を使用した回転拡縮の変換を方向ベクトルに対して行う
	static public float3 transVector(float4x4 mtx, float3 src) => math.mul( mtx, new float4(src,0) ).xyz;
	// 同次変換行列を使用した回転拡縮の変換を法線ベクトルに対して行う。正規化まで行われる
	static public float3 transNormal(float4x4 mtx, float3 src) => math.normalizesafe(transVector(mtx,src));



	/** ベクトルの球面線形補間 */
	static public float3 slerp(float3 src, float3 dst, float rate) {
		var srcLen = math.length(src);
		var dstLen = math.length(dst);
		var srcDir = src / math.max(0.00001f, srcLen);
		var dstDir = dst / math.max(0.00001f, dstLen);

		var agl = math.acos(
			math.clamp( math.dot(srcDir, dstDir), -1, 1 )
		);

		// srcとdstがほぼ同じベクトルであるならば、普通に線形補間する(精度のため)
		if (agl < 0.01f) return math.lerp(src, dst, rate);

		var p0 = math.sin(agl * (1-rate));
		var p1 = math.sin(agl * rate);

//		var ret = (p0*srcDir + p1*dstDir) / math.max(0.0001f, math.sin(agl));
		var ret = p0*srcDir + p1*dstDir;

		return math.normalize(ret) * math.lerp(srcLen, dstLen, rate);
	}

	/**
	 * 逆行列を求める
	 * https://gist.github.com/mattatz/86fff4b32d198d0928d0fa4ff32cf6fa
	 */
	static public float4x4 inverse(float4x4 m) {
		float n11 = m[0][0], n12 = m[1][0], n13 = m[2][0], n14 = m[3][0];
		float n21 = m[0][1], n22 = m[1][1], n23 = m[2][1], n24 = m[3][1];
		float n31 = m[0][2], n32 = m[1][2], n33 = m[2][2], n34 = m[3][2];
		float n41 = m[0][3], n42 = m[1][3], n43 = m[2][3], n44 = m[3][3];

		float t11 = n23 * n34 * n42 - n24 * n33 * n42 + n24 * n32 * n43 - n22 * n34 * n43 - n23 * n32 * n44 + n22 * n33 * n44;
		float t12 = n14 * n33 * n42 - n13 * n34 * n42 - n14 * n32 * n43 + n12 * n34 * n43 + n13 * n32 * n44 - n12 * n33 * n44;
		float t13 = n13 * n24 * n42 - n14 * n23 * n42 + n14 * n22 * n43 - n12 * n24 * n43 - n13 * n22 * n44 + n12 * n23 * n44;
		float t14 = n14 * n23 * n32 - n13 * n24 * n32 - n14 * n22 * n33 + n12 * n24 * n33 + n13 * n22 * n34 - n12 * n23 * n34;

		float det = n11 * t11 + n21 * t12 + n31 * t13 + n41 * t14;
		float idet = 1.0f / det;

		float4x4 ret = default(float4x4);
		ret[0][0] = t11 * idet;
		ret[0][1] = (n24 * n33 * n41 - n23 * n34 * n41 - n24 * n31 * n43 + n21 * n34 * n43 + n23 * n31 * n44 - n21 * n33 * n44) * idet;
		ret[0][2] = (n22 * n34 * n41 - n24 * n32 * n41 + n24 * n31 * n42 - n21 * n34 * n42 - n22 * n31 * n44 + n21 * n32 * n44) * idet;
		ret[0][3] = (n23 * n32 * n41 - n22 * n33 * n41 - n23 * n31 * n42 + n21 * n33 * n42 + n22 * n31 * n43 - n21 * n32 * n43) * idet;

		ret[1][0] = t12 * idet;
		ret[1][1] = (n13 * n34 * n41 - n14 * n33 * n41 + n14 * n31 * n43 - n11 * n34 * n43 - n13 * n31 * n44 + n11 * n33 * n44) * idet;
		ret[1][2] = (n14 * n32 * n41 - n12 * n34 * n41 - n14 * n31 * n42 + n11 * n34 * n42 + n12 * n31 * n44 - n11 * n32 * n44) * idet;
		ret[1][3] = (n12 * n33 * n41 - n13 * n32 * n41 + n13 * n31 * n42 - n11 * n33 * n42 - n12 * n31 * n43 + n11 * n32 * n43) * idet;

		ret[2][0] = t13 * idet;
		ret[2][1] = (n14 * n23 * n41 - n13 * n24 * n41 - n14 * n21 * n43 + n11 * n24 * n43 + n13 * n21 * n44 - n11 * n23 * n44) * idet;
		ret[2][2] = (n12 * n24 * n41 - n14 * n22 * n41 + n14 * n21 * n42 - n11 * n24 * n42 - n12 * n21 * n44 + n11 * n22 * n44) * idet;
		ret[2][3] = (n13 * n22 * n41 - n12 * n23 * n41 - n13 * n21 * n42 + n11 * n23 * n42 + n12 * n21 * n43 - n11 * n22 * n43) * idet;

		ret[3][0] = t14 * idet;
		ret[3][1] = (n13 * n24 * n31 - n14 * n23 * n31 + n14 * n21 * n33 - n11 * n24 * n33 - n13 * n21 * n34 + n11 * n23 * n34) * idet;
		ret[3][2] = (n14 * n22 * n31 - n12 * n24 * n31 - n14 * n21 * n32 + n11 * n24 * n32 + n12 * n21 * n34 - n11 * n22 * n34) * idet;
		ret[3][3] = (n12 * n23 * n31 - n13 * n22 * n31 + n13 * n21 * n32 - n11 * n23 * n32 - n12 * n21 * n33 + n11 * n22 * n33) * idet;

		return ret;
	}		

	/** 指定のfrom方向をto方向に向ける回転を得る。必要であれば角度制限や角度係数を付けることができる */
	static public quaternion fromToRotation(
		float3 from, float3 to,
		float maxAngle=math.PI, float angleScl=1
	) {
		var axis = math.normalizesafe( math.cross(from, to) );
		var theta = math.acos( math.clamp(
			math.dot( math.normalizesafe(from), math.normalizesafe(to) ),
			-1, 1
		) );
		theta = math.min(maxAngle, theta*angleScl);
		var s = math.sin(theta / 2);
		var c = math.cos(theta / 2);
		return math.quaternion(axis.x*s, axis.y*s, axis.z*s, c);
	}


} }

