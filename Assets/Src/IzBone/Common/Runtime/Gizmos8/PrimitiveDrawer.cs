
// OnDrawGizmosで使用できるように、いつでもインクルード可能にしているが、
// Editorでのみ有効である必要があるので、ここで有効無効を切り替える
#if UNITY_EDITOR

using Unity.Mathematics;
using static Unity.Mathematics.math;

using System;
using UnityEditor;
using UnityEngine;

namespace IzBone.Common {
static internal partial class Gizmos8 {

	// Gizmoで描くか、Handleで描くか
	public enum DrawMode {Gizmos, Handle,}
	public static DrawMode drawMode = DrawMode.Gizmos;

	// 色
	public static Color color = Color.white;


	/** ラインを表示 */
	public static void drawLine(float3 a, float3 b) {
		if (drawMode == DrawMode.Gizmos) {
			var lastCol = Gizmos.color;
			Gizmos.color = color;
			Gizmos.DrawLine(a, b);
			Gizmos.color = lastCol;
		} else {
			var lastCol = Handles.color;
			Handles.color = color;
			Handles.DrawLine(a, b);
			Handles.color = lastCol;
		}
	}

	/** 球を表示 */
	public static void drawSphere(float3 pos, float r) {
		if (drawMode == DrawMode.Gizmos) {
			var lastCol = Gizmos.color;
			Gizmos.color = color;
			Gizmos.DrawSphere(pos, r);
			Gizmos.color = lastCol;
		} else {
			var lastCol = Handles.color;
			Handles.color = color;
			Handles.SphereHandleCap(0, pos, Quaternion.identity, r*2, EventType.Repaint);
			Handles.color = lastCol;
		}
	}
	public static void drawWireSphere(float3 center, float r) {
		var mtx = Unity.Mathematics.float3x3.identity;
		drawWireCircle(center, r, mtx);
		var c0 = mtx.c0;
		var c1 = mtx.c1;
		var c2 = mtx.c2;
		mtx.c0 = c2;
		mtx.c1 = c0;
		mtx.c2 = c1;
		drawWireCircle(center, r, mtx);
		mtx.c0 = c1;
		mtx.c1 = c2;
		mtx.c2 = c0;
		drawWireCircle(center, r, mtx);
	}

	/** 直方体を表示 */
	public static void drawWireCube(float3 trans, quaternion rot, float3 size) =>
		drawWireCube( Unity.Mathematics.float4x4.TRS(trans, rot, size/2) );
	public static void drawWireCube(float4x4 trs) {
		var ppp = mul(trs, float4( 1, 1, 1, 1)).xyz;
		var ppm = mul(trs, float4( 1, 1,-1, 1)).xyz;
		var pmp = mul(trs, float4( 1,-1, 1, 1)).xyz;
		var pmm = mul(trs, float4( 1,-1,-1, 1)).xyz;
		var mpp = mul(trs, float4(-1, 1, 1, 1)).xyz;
		var mpm = mul(trs, float4(-1, 1,-1, 1)).xyz;
		var mmp = mul(trs, float4(-1,-1, 1, 1)).xyz;
		var mmm = mul(trs, float4(-1,-1,-1, 1)).xyz;

		drawLine(ppp, ppm);
		drawLine(ppp, mpp);
		drawLine(ppm, mpm);
		drawLine(mpp, mpm);

		drawLine(pmp, pmm);
		drawLine(pmp, mmp);
		drawLine(pmm, mmm);
		drawLine(mmp, mmm);

		drawLine(ppp, pmp);
		drawLine(mpp, mmp);
		drawLine(ppm, pmm);
		drawLine(mpm, mmm);
	}

	/** カプセルを表示 */
	public static void drawWireCapsule(float3 pos, float3 upDir, float r_s, float r_h) {
		float3 x,y,z;
		y = upDir;
		x = y.Equals(float3(1,0,0)) ?
			normalize( cross(upDir, float3(0,1,0)) ) :
			normalize( cross(upDir, float3(1,0,0)) );
		z = cross(x, y);

		drawWireCylinder(pos, upDir, r_s, r_h);

		var t =  y*r_h + pos;
		var b = -y*r_h + pos;
		drawWireCircle(t, r_s, float3x3(z,x,y), 0, 0.5f);
		drawWireCircle(t, r_s, float3x3(x,z,y), 0, 0.5f);
		drawWireCircle(b, r_s, float3x3(z,x,y), 0.5f, 1);
		drawWireCircle(b, r_s, float3x3(x,z,y), 0.5f, 1);
	}

	/** 円を描画 */
	public static void drawWireCircle(
		float3 center,
		float r,
		float3x3 rot,
		float from = 0,
		float to = 1
	) {
		int segNum = 30;
		Func<int,float> calcTheta = i => (PI*2)*( from + (to-from)*i/segNum );

		for (int i=0; i<segNum; ++i) {
			var theta0 = calcTheta( i );
			var theta1 = calcTheta( i+1 );
			var p0 = mul( rot, float3(cos(theta0),0,sin(theta0)) ) * r + center;
			var p1 = mul( rot, float3(cos(theta1),0,sin(theta1)) ) * r + center;
			drawLine(p0, p1);
		}
	}

	/** シリンダーを描画 */
	public static void drawWireCylinder(float3 pos, float3 upDir, float r_s, float r_h) {
		float3 x,y,z;
		y = upDir;
		x = y.Equals(float3(1,0,0)) ?
			normalize( cross(upDir, float3(0,1,0)) ) :
			normalize( cross(upDir, float3(1,0,0)) );
		z = cross(x, y);

		drawWireCircle( pos, r_s, float3x3(x,y,z), 0, 1 );
		var t =  y*r_h + pos;
		var b = -y*r_h + pos;
		drawWireCircle(t, r_s, float3x3(x,y,z), 0, 1);
		drawWireCircle(b, r_s, float3x3(x,y,z), 0, 1);
		drawLine( b + x*r_s, t + x*r_s );
		drawLine( b + z*r_s, t + z*r_s );
		drawLine( b - x*r_s, t - x*r_s );
		drawLine( b - z*r_s, t - z*r_s );
	}

	/** コーンを描画 */
	public static void drawCone(float3 btmPos, float3 topPos, float r_s) {

		var b2t = normalizesafe(topPos - btmPos);

		// 各種ローカル軸方向を求めて、回転行列を生成
		var x = normalize( cross(
			b2t,
			0.99999f < dot(b2t, float3(1,0,0)) ? float3(0,1,0) : float3(1,0,0)
		) );
		var z = normalize( cross(b2t, x) );
		var y = cross( z, x );
		var rotMtx = float3x3(x,y,z);

		// 底部分のサークルを描画
		drawWireCircle(btmPos, r_s, rotMtx);

		// 壁面のラインを描画
		drawLine(
			mul(rotMtx, float3(1,0,0)) + btmPos,
			topPos
		);
		drawLine(
			mul(rotMtx, float3(-1,0,0)) + btmPos,
			topPos
		);
		drawLine(
			mul(rotMtx, float3(0,0,1)) + btmPos,
			topPos
		);
		drawLine(
			mul(rotMtx, float3(0,0,-1)) + btmPos,
			topPos
		);
	}

	/** 角度範囲表示用コーンを描画。angleはDegree */
	public static void drawAngleCone(float3 pos, quaternion rot, float scl, float2 angle) =>
		drawAngleCone( Unity.Mathematics.float4x4.TRS(pos,rot,scl), angle );
	public static void drawAngleCone(float4x4 trs, float2 angle) {

		// ローカル座標でDrawLineする
		static void line(float4x4 trs, float3 lPos0, float3 lPos1) {
			drawLine(
				mul( trs, float4(lPos0,1) ).xyz,
				mul( trs, float4(lPos1,1) ).xyz
			);
		}

		var aglRad = radians(angle);
		var sAgl = sin(aglRad);
		var cAgl = cos(aglRad);

		// 媒介変数Θから、角度範囲の境界線部分のローカル座標を得る
		float3 calcEdgePosFromTheta(float theta) {
			var cTheta = cos(theta);
			var sTheta = sin(theta);

			return float3(
				sAgl.x*cTheta,
				(cAgl.x*cTheta + cAgl.y*sTheta) / (cTheta + sTheta),
				sAgl.y*sTheta
			);
		}

		// まず角度範囲の境界部分を描く
		int segNum = 60;
		for (int i=0; i<segNum; ++i) {
			var pos0 = calcEdgePosFromTheta( PI*2 * i/segNum );
			var pos1 = calcEdgePosFromTheta( PI*2 * (i+1)/segNum );
			line(trs, pos0, pos1);
		}

		// 中心位置から角度範囲境界への側面を描く
		line( trs, 0, calcEdgePosFromTheta(0) );
		line( trs, 0, calcEdgePosFromTheta(PI*0.5f) );
		line( trs, 0, calcEdgePosFromTheta(PI) );
		line( trs, 0, calcEdgePosFromTheta(PI*1.5f) );

		// 角度制限範囲内の球面補助線を描く
		int segNum2 = 30;
		for (int i=0; i<segNum2; ++i) {

			static float2 calcCS(float aglRad, float rate) {
				var theta = lerp( aglRad, -aglRad, rate );
				return float2( cos(theta), sin(theta) );
			}

			var rate0 = (float)i / segNum2;
			var rate1 = (float)(i+1) / segNum2;
			var csA0 = calcCS(aglRad.x, rate0);
			var csA1 = calcCS(aglRad.x, rate1);
			var csB0 = calcCS(aglRad.y, rate0);
			var csB1 = calcCS(aglRad.y, rate1);

			line( trs, float3(csA0.y, csA0.x, 0), float3(csA1.y, csA1.x, 0) );
			line( trs, float3(0, csB0.x, csB0.y), float3(0, csB1.x, csB1.y) );
		}
	}

} }
#endif
