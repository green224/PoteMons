// Easing系の機能群を定義
using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Runtime.CompilerServices;
using MI = System.Runtime.CompilerServices.MethodImplAttribute; 
using MO = System.Runtime.CompilerServices.MethodImplOptions; 


namespace IzBone.Common {
static public partial class Math8  {
	static public class Ease {

		[MI(MO.AggressiveInlining)]
		public static float spring(float a,float b,float x) {
			x = saturate(x);
			x = (
				sin(x * (float)PI * ((float)0.2 + (float)2.5 * x * x * x))
				* pow(1-x, (float)2.2) + x
			) * (1 + ((float)1.2 * (1-x)));
			return a + (b - a) * x;
		}
		[MI(MO.AggressiveInlining)]
		public static float halfLife(float a,float b,float hl,float dt) => lerp(b,a,calcHL(hl,dt));
		public static float inQuad(float a,float b,float x) => (b-a)*x*x + a;
		public static float outQuad(float a,float b,float x) => -(b-a)*x*(x-2) + a;
		public static float inOutQuad(float a,float b,float x) => x<0.5 ? (inQuad(0,b-a,x*2)/2+a) : (inQuad(0,a-b,2-x*2)/2+b);
		public static float inCubic(float a,float b,float x) => (b-a)*x*x*x + a;
		public static float outCubic(float a,float b,float x) => inCubic( b, a, 1-x );
		public static float inOutCubic(float a,float b,float x) => x<0.5 ? (inCubic(0,b-a,x*2)/2+a) : (inCubic(0,a-b,2-x*2)/2+b);
		public static float inQuart(float a,float b,float x) => (b-a)*x*x*x*x + a;
		public static float outQuart(float a,float b,float x) => inQuart( b, a, 1-x );
		public static float inOutQuart(float a,float b,float x) => x<0.5 ? (inQuart(0,b-a,x*2)/2+a) : (inQuart(0,a-b,2-x*2)/2+b);
		public static float inQuint(float a,float b,float x) => (b-a)*x*x*x*x*x + a;
		public static float outQuint(float a,float b,float x) => inQuint( b, a, 1-x );
		public static float inOutQuint(float a,float b,float x) => x<0.5 ? (inQuint(0,b-a,x*2)/2+a) : (inQuint(0,a-b,2-x*2)/2+b);
		public static float inSine(float a,float b,float x) => (a-b) * cos(x*(float)PI/2) + b;
		public static float outSine(float a,float b,float x) => (b-a) * sin(x*(float)PI/2) + a;
		public static float inOutSine(float a,float b,float x) => x<0.5 ? (inSine(0,b-a,x*2)/2+a) : (inSine(0,a-b,2-x*2)/2+b);
		public static float inExpo(float a,float b,float x) => (b-a) * pow(2, 10*(x-1)) + a;
		public static float outExpo(float a,float b,float x) => (b-a) * ( 1 - pow(2, -10*x) ) + a;
		public static float inOutExpo(float a,float b,float x) => x<0.5 ? (inExpo(0,b-a,x*2)/2+a) : (inExpo(0,a-b,2-x*2)/2+b);
		public static float inCirc(float a,float b,float x) => (a-b) * (sqrt(1-x*x) - 1) + a;
		public static float outCirc(float a,float b,float x) => (b-a) * sqrt(-2*x - x*x) + a;
		public static float inOutCirc(float a,float b,float x) => x<0.5 ? (inCirc(0,b-a,x*2)/2+a) : (inCirc(0,a-b,2-x*2)/2+b);
		public static float inBounce(float a,float b,float x) => outBounce( b, a, 1-x );
		public static float outBounce(float a,float b,float x) {
			if (x < 1 / 2.75 ) {
				return (b-a) * (float)(7.5625*x*x) + a;
			} else if ( x < 2 / 2.75 ) {
				x -= (float)( 1.5/2.75 );
				return (b-a) * (float)(7.5625*x*x + .75) + a;
			} else if ( x < 2.5 / 2.75 ) {
				x -= (float)( 2.25/2.75 );
				return (b-a) * (float)(7.5625*x*x + .9375) + a;
			} else {
				x -= (float)( 2.625/2.75 );
				return (b-a) * (float)(7.5625*x*x + .984375) + a;
			}
		}
		public static float inOutBounce(float a,float b,float x) => x<0.5 ? (outBounce(b-a,0,1-x*2)/2+a) : (outBounce(a-b,0,x*2-1)/2+b);
		public static float inBack(float a,float b,float x) => (b-a) * x*x * ((float)2.70158*x-(float)1.70158) + a;
		public static float outBack(float a,float b,float x) => inBack( b, a, 1-x );
		public static float inOutBack(float a,float b,float x) => x<0.5 ? (inBack(0,b-a,x*2)/2+a) : (inBack(0,a-b,2-x*2)/2+b);
		public static float inElastic(float a,float b,float x) => x<=0 ? a : 1<=x ? b : ( (a-b)*pow(2, 10*(x-1))*sin((x-(float)1.075)*(float)(PI*2/0.3))+a );
		public static float outElastic(float a,float b,float x) => inElastic( b, a, 1-x );
		public static float inOutElastic(float a,float b,float x) => x<0.5 ? (inElastic(0,b-a,x*2)/2+a) : (inElastic(0,a-b,2-x*2)/2+b);
		public static float4 spring(float4 a,float4 b,float x) => new float4(spring(a.x,b.x,x), spring(a.y,b.y,x), spring(a.z,b.z,x), spring(a.w,b.w,x));
		public static float4 halfLife(float4 a,float4 b,float hl,float dt) => lerp(b,a,calcHL(hl,dt));
		public static float4 inQuad(float4 a,float4 b,float x) => new float4(inQuad(a.x,b.x,x), inQuad(a.y,b.y,x), inQuad(a.z,b.z,x), inQuad(a.w,b.w,x));
		public static float4 outQuad(float4 a,float4 b,float x) => new float4(outQuad(a.x,b.x,x), outQuad(a.y,b.y,x), outQuad(a.z,b.z,x), outQuad(a.w,b.w,x));
		public static float4 inOutQuad(float4 a,float4 b,float x) => new float4(inOutQuad(a.x,b.x,x), inOutQuad(a.y,b.y,x), inOutQuad(a.z,b.z,x), inOutQuad(a.w,b.w,x));
		public static float4 inCubic(float4 a,float4 b,float x) => new float4(inCubic(a.x,b.x,x), inCubic(a.y,b.y,x), inCubic(a.z,b.z,x), inCubic(a.w,b.w,x));
		public static float4 outCubic(float4 a,float4 b,float x) => new float4(outCubic(a.x,b.x,x), outCubic(a.y,b.y,x), outCubic(a.z,b.z,x), outCubic(a.w,b.w,x));
		public static float4 inOutCubic(float4 a,float4 b,float x) => new float4(inOutCubic(a.x,b.x,x), inOutCubic(a.y,b.y,x), inOutCubic(a.z,b.z,x), inOutCubic(a.w,b.w,x));
		public static float4 inQuart(float4 a,float4 b,float x) => new float4(inQuart(a.x,b.x,x), inQuart(a.y,b.y,x), inQuart(a.z,b.z,x), inQuart(a.w,b.w,x));
		public static float4 outQuart(float4 a,float4 b,float x) => new float4(outQuart(a.x,b.x,x), outQuart(a.y,b.y,x), outQuart(a.z,b.z,x), outQuart(a.w,b.w,x));
		public static float4 inOutQuart(float4 a,float4 b,float x) => new float4(inOutQuart(a.x,b.x,x), inOutQuart(a.y,b.y,x), inOutQuart(a.z,b.z,x), inOutQuart(a.w,b.w,x));
		public static float4 inQuint(float4 a,float4 b,float x) => new float4(inQuint(a.x,b.x,x), inQuint(a.y,b.y,x), inQuint(a.z,b.z,x), inQuint(a.w,b.w,x));
		public static float4 outQuint(float4 a,float4 b,float x) => new float4(outQuint(a.x,b.x,x), outQuint(a.y,b.y,x), outQuint(a.z,b.z,x), outQuint(a.w,b.w,x));
		public static float4 inOutQuint(float4 a,float4 b,float x) => new float4(inOutQuint(a.x,b.x,x), inOutQuint(a.y,b.y,x), inOutQuint(a.z,b.z,x), inOutQuint(a.w,b.w,x));
		public static float4 inSine(float4 a,float4 b,float x) => new float4(inSine(a.x,b.x,x), inSine(a.y,b.y,x), inSine(a.z,b.z,x), inSine(a.w,b.w,x));
		public static float4 outSine(float4 a,float4 b,float x) => new float4(outSine(a.x,b.x,x), outSine(a.y,b.y,x), outSine(a.z,b.z,x), outSine(a.w,b.w,x));
		public static float4 inOutSine(float4 a,float4 b,float x) => new float4(inOutSine(a.x,b.x,x), inOutSine(a.y,b.y,x), inOutSine(a.z,b.z,x), inOutSine(a.w,b.w,x));
		public static float4 inExpo(float4 a,float4 b,float x) => new float4(inExpo(a.x,b.x,x), inExpo(a.y,b.y,x), inExpo(a.z,b.z,x), inExpo(a.w,b.w,x));
		public static float4 outExpo(float4 a,float4 b,float x) => new float4(outExpo(a.x,b.x,x), outExpo(a.y,b.y,x), outExpo(a.z,b.z,x), outExpo(a.w,b.w,x));
		public static float4 inOutExpo(float4 a,float4 b,float x) => new float4(inOutExpo(a.x,b.x,x), inOutExpo(a.y,b.y,x), inOutExpo(a.z,b.z,x), inOutExpo(a.w,b.w,x));
		public static float4 inCirc(float4 a,float4 b,float x) => new float4(inCirc(a.x,b.x,x), inCirc(a.y,b.y,x), inCirc(a.z,b.z,x), inCirc(a.w,b.w,x));
		public static float4 outCirc(float4 a,float4 b,float x) => new float4(outCirc(a.x,b.x,x), outCirc(a.y,b.y,x), outCirc(a.z,b.z,x), outCirc(a.w,b.w,x));
		public static float4 inOutCirc(float4 a,float4 b,float x) => new float4(inOutCirc(a.x,b.x,x), inOutCirc(a.y,b.y,x), inOutCirc(a.z,b.z,x), inOutCirc(a.w,b.w,x));
		public static float4 inBounce(float4 a,float4 b,float x) => new float4(inBounce(a.x,b.x,x), inBounce(a.y,b.y,x), inBounce(a.z,b.z,x), inBounce(a.w,b.w,x));
		public static float4 outBounce(float4 a,float4 b,float x) => new float4(outBounce(a.x,b.x,x), outBounce(a.y,b.y,x), outBounce(a.z,b.z,x), outBounce(a.w,b.w,x));
		public static float4 inOutBounce(float4 a,float4 b,float x) => new float4(inOutBounce(a.x,b.x,x), inOutBounce(a.y,b.y,x), inOutBounce(a.z,b.z,x), inOutBounce(a.w,b.w,x));
		public static float4 inBack(float4 a,float4 b,float x) => new float4(inBack(a.x,b.x,x), inBack(a.y,b.y,x), inBack(a.z,b.z,x), inBack(a.w,b.w,x));
		public static float4 outBack(float4 a,float4 b,float x) => new float4(outBack(a.x,b.x,x), outBack(a.y,b.y,x), outBack(a.z,b.z,x), outBack(a.w,b.w,x));
		public static float4 inOutBack(float4 a,float4 b,float x) => new float4(inOutBack(a.x,b.x,x), inOutBack(a.y,b.y,x), inOutBack(a.z,b.z,x), inOutBack(a.w,b.w,x));
		public static float4 inElastic(float4 a,float4 b,float x) => new float4(inElastic(a.x,b.x,x), inElastic(a.y,b.y,x), inElastic(a.z,b.z,x), inElastic(a.w,b.w,x));
		public static float4 outElastic(float4 a,float4 b,float x) => new float4(outElastic(a.x,b.x,x), outElastic(a.y,b.y,x), outElastic(a.z,b.z,x), outElastic(a.w,b.w,x));
		public static float4 inOutElastic(float4 a,float4 b,float x) => new float4(inOutElastic(a.x,b.x,x), inOutElastic(a.y,b.y,x), inOutElastic(a.z,b.z,x), inOutElastic(a.w,b.w,x));
		public static float3 spring(float3 a,float3 b,float x) => new float3(spring(a.x,b.x,x), spring(a.y,b.y,x), spring(a.z,b.z,x));
		public static float3 halfLife(float3 a,float3 b,float hl,float dt) => lerp(b,a,calcHL(hl,dt));
		public static float3 inQuad(float3 a,float3 b,float x) => new float3(inQuad(a.x,b.x,x), inQuad(a.y,b.y,x), inQuad(a.z,b.z,x));
		public static float3 outQuad(float3 a,float3 b,float x) => new float3(outQuad(a.x,b.x,x), outQuad(a.y,b.y,x), outQuad(a.z,b.z,x));
		public static float3 inOutQuad(float3 a,float3 b,float x) => new float3(inOutQuad(a.x,b.x,x), inOutQuad(a.y,b.y,x), inOutQuad(a.z,b.z,x));
		public static float3 inCubic(float3 a,float3 b,float x) => new float3(inCubic(a.x,b.x,x), inCubic(a.y,b.y,x), inCubic(a.z,b.z,x));
		public static float3 outCubic(float3 a,float3 b,float x) => new float3(outCubic(a.x,b.x,x), outCubic(a.y,b.y,x), outCubic(a.z,b.z,x));
		public static float3 inOutCubic(float3 a,float3 b,float x) => new float3(inOutCubic(a.x,b.x,x), inOutCubic(a.y,b.y,x), inOutCubic(a.z,b.z,x));
		public static float3 inQuart(float3 a,float3 b,float x) => new float3(inQuart(a.x,b.x,x), inQuart(a.y,b.y,x), inQuart(a.z,b.z,x));
		public static float3 outQuart(float3 a,float3 b,float x) => new float3(outQuart(a.x,b.x,x), outQuart(a.y,b.y,x), outQuart(a.z,b.z,x));
		public static float3 inOutQuart(float3 a,float3 b,float x) => new float3(inOutQuart(a.x,b.x,x), inOutQuart(a.y,b.y,x), inOutQuart(a.z,b.z,x));
		public static float3 inQuint(float3 a,float3 b,float x) => new float3(inQuint(a.x,b.x,x), inQuint(a.y,b.y,x), inQuint(a.z,b.z,x));
		public static float3 outQuint(float3 a,float3 b,float x) => new float3(outQuint(a.x,b.x,x), outQuint(a.y,b.y,x), outQuint(a.z,b.z,x));
		public static float3 inOutQuint(float3 a,float3 b,float x) => new float3(inOutQuint(a.x,b.x,x), inOutQuint(a.y,b.y,x), inOutQuint(a.z,b.z,x));
		public static float3 inSine(float3 a,float3 b,float x) => new float3(inSine(a.x,b.x,x), inSine(a.y,b.y,x), inSine(a.z,b.z,x));
		public static float3 outSine(float3 a,float3 b,float x) => new float3(outSine(a.x,b.x,x), outSine(a.y,b.y,x), outSine(a.z,b.z,x));
		public static float3 inOutSine(float3 a,float3 b,float x) => new float3(inOutSine(a.x,b.x,x), inOutSine(a.y,b.y,x), inOutSine(a.z,b.z,x));
		public static float3 inExpo(float3 a,float3 b,float x) => new float3(inExpo(a.x,b.x,x), inExpo(a.y,b.y,x), inExpo(a.z,b.z,x));
		public static float3 outExpo(float3 a,float3 b,float x) => new float3(outExpo(a.x,b.x,x), outExpo(a.y,b.y,x), outExpo(a.z,b.z,x));
		public static float3 inOutExpo(float3 a,float3 b,float x) => new float3(inOutExpo(a.x,b.x,x), inOutExpo(a.y,b.y,x), inOutExpo(a.z,b.z,x));
		public static float3 inCirc(float3 a,float3 b,float x) => new float3(inCirc(a.x,b.x,x), inCirc(a.y,b.y,x), inCirc(a.z,b.z,x));
		public static float3 outCirc(float3 a,float3 b,float x) => new float3(outCirc(a.x,b.x,x), outCirc(a.y,b.y,x), outCirc(a.z,b.z,x));
		public static float3 inOutCirc(float3 a,float3 b,float x) => new float3(inOutCirc(a.x,b.x,x), inOutCirc(a.y,b.y,x), inOutCirc(a.z,b.z,x));
		public static float3 inBounce(float3 a,float3 b,float x) => new float3(inBounce(a.x,b.x,x), inBounce(a.y,b.y,x), inBounce(a.z,b.z,x));
		public static float3 outBounce(float3 a,float3 b,float x) => new float3(outBounce(a.x,b.x,x), outBounce(a.y,b.y,x), outBounce(a.z,b.z,x));
		public static float3 inOutBounce(float3 a,float3 b,float x) => new float3(inOutBounce(a.x,b.x,x), inOutBounce(a.y,b.y,x), inOutBounce(a.z,b.z,x));
		public static float3 inBack(float3 a,float3 b,float x) => new float3(inBack(a.x,b.x,x), inBack(a.y,b.y,x), inBack(a.z,b.z,x));
		public static float3 outBack(float3 a,float3 b,float x) => new float3(outBack(a.x,b.x,x), outBack(a.y,b.y,x), outBack(a.z,b.z,x));
		public static float3 inOutBack(float3 a,float3 b,float x) => new float3(inOutBack(a.x,b.x,x), inOutBack(a.y,b.y,x), inOutBack(a.z,b.z,x));
		public static float3 inElastic(float3 a,float3 b,float x) => new float3(inElastic(a.x,b.x,x), inElastic(a.y,b.y,x), inElastic(a.z,b.z,x));
		public static float3 outElastic(float3 a,float3 b,float x) => new float3(outElastic(a.x,b.x,x), outElastic(a.y,b.y,x), outElastic(a.z,b.z,x));
		public static float3 inOutElastic(float3 a,float3 b,float x) => new float3(inOutElastic(a.x,b.x,x), inOutElastic(a.y,b.y,x), inOutElastic(a.z,b.z,x));
		public static float2 spring(float2 a,float2 b,float x) => new float2(spring(a.x,b.x,x), spring(a.y,b.y,x));
		public static float2 halfLife(float2 a,float2 b,float hl,float dt) => lerp(b,a,calcHL(hl,dt));
		public static float2 inQuad(float2 a,float2 b,float x) => new float2(inQuad(a.x,b.x,x), inQuad(a.y,b.y,x));
		public static float2 outQuad(float2 a,float2 b,float x) => new float2(outQuad(a.x,b.x,x), outQuad(a.y,b.y,x));
		public static float2 inOutQuad(float2 a,float2 b,float x) => new float2(inOutQuad(a.x,b.x,x), inOutQuad(a.y,b.y,x));
		public static float2 inCubic(float2 a,float2 b,float x) => new float2(inCubic(a.x,b.x,x), inCubic(a.y,b.y,x));
		public static float2 outCubic(float2 a,float2 b,float x) => new float2(outCubic(a.x,b.x,x), outCubic(a.y,b.y,x));
		public static float2 inOutCubic(float2 a,float2 b,float x) => new float2(inOutCubic(a.x,b.x,x), inOutCubic(a.y,b.y,x));
		public static float2 inQuart(float2 a,float2 b,float x) => new float2(inQuart(a.x,b.x,x), inQuart(a.y,b.y,x));
		public static float2 outQuart(float2 a,float2 b,float x) => new float2(outQuart(a.x,b.x,x), outQuart(a.y,b.y,x));
		public static float2 inOutQuart(float2 a,float2 b,float x) => new float2(inOutQuart(a.x,b.x,x), inOutQuart(a.y,b.y,x));
		public static float2 inQuint(float2 a,float2 b,float x) => new float2(inQuint(a.x,b.x,x), inQuint(a.y,b.y,x));
		public static float2 outQuint(float2 a,float2 b,float x) => new float2(outQuint(a.x,b.x,x), outQuint(a.y,b.y,x));
		public static float2 inOutQuint(float2 a,float2 b,float x) => new float2(inOutQuint(a.x,b.x,x), inOutQuint(a.y,b.y,x));
		public static float2 inSine(float2 a,float2 b,float x) => new float2(inSine(a.x,b.x,x), inSine(a.y,b.y,x));
		public static float2 outSine(float2 a,float2 b,float x) => new float2(outSine(a.x,b.x,x), outSine(a.y,b.y,x));
		public static float2 inOutSine(float2 a,float2 b,float x) => new float2(inOutSine(a.x,b.x,x), inOutSine(a.y,b.y,x));
		public static float2 inExpo(float2 a,float2 b,float x) => new float2(inExpo(a.x,b.x,x), inExpo(a.y,b.y,x));
		public static float2 outExpo(float2 a,float2 b,float x) => new float2(outExpo(a.x,b.x,x), outExpo(a.y,b.y,x));
		public static float2 inOutExpo(float2 a,float2 b,float x) => new float2(inOutExpo(a.x,b.x,x), inOutExpo(a.y,b.y,x));
		public static float2 inCirc(float2 a,float2 b,float x) => new float2(inCirc(a.x,b.x,x), inCirc(a.y,b.y,x));
		public static float2 outCirc(float2 a,float2 b,float x) => new float2(outCirc(a.x,b.x,x), outCirc(a.y,b.y,x));
		public static float2 inOutCirc(float2 a,float2 b,float x) => new float2(inOutCirc(a.x,b.x,x), inOutCirc(a.y,b.y,x));
		public static float2 inBounce(float2 a,float2 b,float x) => new float2(inBounce(a.x,b.x,x), inBounce(a.y,b.y,x));
		public static float2 outBounce(float2 a,float2 b,float x) => new float2(outBounce(a.x,b.x,x), outBounce(a.y,b.y,x));
		public static float2 inOutBounce(float2 a,float2 b,float x) => new float2(inOutBounce(a.x,b.x,x), inOutBounce(a.y,b.y,x));
		public static float2 inBack(float2 a,float2 b,float x) => new float2(inBack(a.x,b.x,x), inBack(a.y,b.y,x));
		public static float2 outBack(float2 a,float2 b,float x) => new float2(outBack(a.x,b.x,x), outBack(a.y,b.y,x));
		public static float2 inOutBack(float2 a,float2 b,float x) => new float2(inOutBack(a.x,b.x,x), inOutBack(a.y,b.y,x));
		public static float2 inElastic(float2 a,float2 b,float x) => new float2(inElastic(a.x,b.x,x), inElastic(a.y,b.y,x));
		public static float2 outElastic(float2 a,float2 b,float x) => new float2(outElastic(a.x,b.x,x), outElastic(a.y,b.y,x));
		public static float2 inOutElastic(float2 a,float2 b,float x) => new float2(inOutElastic(a.x,b.x,x), inOutElastic(a.y,b.y,x));
		[MI(MO.AggressiveInlining)]
		public static double spring(double a,double b,double x) {
			x = saturate(x);
			x = (
				sin(x * (double)PI * ((double)0.2 + (double)2.5 * x * x * x))
				* pow(1-x, (double)2.2) + x
			) * (1 + ((double)1.2 * (1-x)));
			return a + (b - a) * x;
		}
		[MI(MO.AggressiveInlining)]
		public static double halfLife(double a,double b,double hl,double dt) => lerp(b,a,calcHL(hl,dt));
		public static double inQuad(double a,double b,double x) => (b-a)*x*x + a;
		public static double outQuad(double a,double b,double x) => -(b-a)*x*(x-2) + a;
		public static double inOutQuad(double a,double b,double x) => x<0.5 ? (inQuad(0,b-a,x*2)/2+a) : (inQuad(0,a-b,2-x*2)/2+b);
		public static double inCubic(double a,double b,double x) => (b-a)*x*x*x + a;
		public static double outCubic(double a,double b,double x) => inCubic( b, a, 1-x );
		public static double inOutCubic(double a,double b,double x) => x<0.5 ? (inCubic(0,b-a,x*2)/2+a) : (inCubic(0,a-b,2-x*2)/2+b);
		public static double inQuart(double a,double b,double x) => (b-a)*x*x*x*x + a;
		public static double outQuart(double a,double b,double x) => inQuart( b, a, 1-x );
		public static double inOutQuart(double a,double b,double x) => x<0.5 ? (inQuart(0,b-a,x*2)/2+a) : (inQuart(0,a-b,2-x*2)/2+b);
		public static double inQuint(double a,double b,double x) => (b-a)*x*x*x*x*x + a;
		public static double outQuint(double a,double b,double x) => inQuint( b, a, 1-x );
		public static double inOutQuint(double a,double b,double x) => x<0.5 ? (inQuint(0,b-a,x*2)/2+a) : (inQuint(0,a-b,2-x*2)/2+b);
		public static double inSine(double a,double b,double x) => (a-b) * cos(x*(double)PI/2) + b;
		public static double outSine(double a,double b,double x) => (b-a) * sin(x*(double)PI/2) + a;
		public static double inOutSine(double a,double b,double x) => x<0.5 ? (inSine(0,b-a,x*2)/2+a) : (inSine(0,a-b,2-x*2)/2+b);
		public static double inExpo(double a,double b,double x) => (b-a) * pow(2, 10*(x-1)) + a;
		public static double outExpo(double a,double b,double x) => (b-a) * ( 1 - pow(2, -10*x) ) + a;
		public static double inOutExpo(double a,double b,double x) => x<0.5 ? (inExpo(0,b-a,x*2)/2+a) : (inExpo(0,a-b,2-x*2)/2+b);
		public static double inCirc(double a,double b,double x) => (a-b) * (sqrt(1-x*x) - 1) + a;
		public static double outCirc(double a,double b,double x) => (b-a) * sqrt(-2*x - x*x) + a;
		public static double inOutCirc(double a,double b,double x) => x<0.5 ? (inCirc(0,b-a,x*2)/2+a) : (inCirc(0,a-b,2-x*2)/2+b);
		public static double inBounce(double a,double b,double x) => outBounce( b, a, 1-x );
		public static double outBounce(double a,double b,double x) {
			if (x < 1 / 2.75 ) {
				return (b-a) * (double)(7.5625*x*x) + a;
			} else if ( x < 2 / 2.75 ) {
				x -= (double)( 1.5/2.75 );
				return (b-a) * (double)(7.5625*x*x + .75) + a;
			} else if ( x < 2.5 / 2.75 ) {
				x -= (double)( 2.25/2.75 );
				return (b-a) * (double)(7.5625*x*x + .9375) + a;
			} else {
				x -= (double)( 2.625/2.75 );
				return (b-a) * (double)(7.5625*x*x + .984375) + a;
			}
		}
		public static double inOutBounce(double a,double b,double x) => x<0.5 ? (outBounce(b-a,0,1-x*2)/2+a) : (outBounce(a-b,0,x*2-1)/2+b);
		public static double inBack(double a,double b,double x) => (b-a) * x*x * ((double)2.70158*x-(double)1.70158) + a;
		public static double outBack(double a,double b,double x) => inBack( b, a, 1-x );
		public static double inOutBack(double a,double b,double x) => x<0.5 ? (inBack(0,b-a,x*2)/2+a) : (inBack(0,a-b,2-x*2)/2+b);
		public static double inElastic(double a,double b,double x) => x<=0 ? a : 1<=x ? b : ( (a-b)*pow(2, 10*(x-1))*sin((x-(double)1.075)*(double)(PI*2/0.3))+a );
		public static double outElastic(double a,double b,double x) => inElastic( b, a, 1-x );
		public static double inOutElastic(double a,double b,double x) => x<0.5 ? (inElastic(0,b-a,x*2)/2+a) : (inElastic(0,a-b,2-x*2)/2+b);
		public static double4 spring(double4 a,double4 b,double x) => new double4(spring(a.x,b.x,x), spring(a.y,b.y,x), spring(a.z,b.z,x), spring(a.w,b.w,x));
		public static double4 halfLife(double4 a,double4 b,double hl,double dt) => lerp(b,a,calcHL(hl,dt));
		public static double4 inQuad(double4 a,double4 b,double x) => new double4(inQuad(a.x,b.x,x), inQuad(a.y,b.y,x), inQuad(a.z,b.z,x), inQuad(a.w,b.w,x));
		public static double4 outQuad(double4 a,double4 b,double x) => new double4(outQuad(a.x,b.x,x), outQuad(a.y,b.y,x), outQuad(a.z,b.z,x), outQuad(a.w,b.w,x));
		public static double4 inOutQuad(double4 a,double4 b,double x) => new double4(inOutQuad(a.x,b.x,x), inOutQuad(a.y,b.y,x), inOutQuad(a.z,b.z,x), inOutQuad(a.w,b.w,x));
		public static double4 inCubic(double4 a,double4 b,double x) => new double4(inCubic(a.x,b.x,x), inCubic(a.y,b.y,x), inCubic(a.z,b.z,x), inCubic(a.w,b.w,x));
		public static double4 outCubic(double4 a,double4 b,double x) => new double4(outCubic(a.x,b.x,x), outCubic(a.y,b.y,x), outCubic(a.z,b.z,x), outCubic(a.w,b.w,x));
		public static double4 inOutCubic(double4 a,double4 b,double x) => new double4(inOutCubic(a.x,b.x,x), inOutCubic(a.y,b.y,x), inOutCubic(a.z,b.z,x), inOutCubic(a.w,b.w,x));
		public static double4 inQuart(double4 a,double4 b,double x) => new double4(inQuart(a.x,b.x,x), inQuart(a.y,b.y,x), inQuart(a.z,b.z,x), inQuart(a.w,b.w,x));
		public static double4 outQuart(double4 a,double4 b,double x) => new double4(outQuart(a.x,b.x,x), outQuart(a.y,b.y,x), outQuart(a.z,b.z,x), outQuart(a.w,b.w,x));
		public static double4 inOutQuart(double4 a,double4 b,double x) => new double4(inOutQuart(a.x,b.x,x), inOutQuart(a.y,b.y,x), inOutQuart(a.z,b.z,x), inOutQuart(a.w,b.w,x));
		public static double4 inQuint(double4 a,double4 b,double x) => new double4(inQuint(a.x,b.x,x), inQuint(a.y,b.y,x), inQuint(a.z,b.z,x), inQuint(a.w,b.w,x));
		public static double4 outQuint(double4 a,double4 b,double x) => new double4(outQuint(a.x,b.x,x), outQuint(a.y,b.y,x), outQuint(a.z,b.z,x), outQuint(a.w,b.w,x));
		public static double4 inOutQuint(double4 a,double4 b,double x) => new double4(inOutQuint(a.x,b.x,x), inOutQuint(a.y,b.y,x), inOutQuint(a.z,b.z,x), inOutQuint(a.w,b.w,x));
		public static double4 inSine(double4 a,double4 b,double x) => new double4(inSine(a.x,b.x,x), inSine(a.y,b.y,x), inSine(a.z,b.z,x), inSine(a.w,b.w,x));
		public static double4 outSine(double4 a,double4 b,double x) => new double4(outSine(a.x,b.x,x), outSine(a.y,b.y,x), outSine(a.z,b.z,x), outSine(a.w,b.w,x));
		public static double4 inOutSine(double4 a,double4 b,double x) => new double4(inOutSine(a.x,b.x,x), inOutSine(a.y,b.y,x), inOutSine(a.z,b.z,x), inOutSine(a.w,b.w,x));
		public static double4 inExpo(double4 a,double4 b,double x) => new double4(inExpo(a.x,b.x,x), inExpo(a.y,b.y,x), inExpo(a.z,b.z,x), inExpo(a.w,b.w,x));
		public static double4 outExpo(double4 a,double4 b,double x) => new double4(outExpo(a.x,b.x,x), outExpo(a.y,b.y,x), outExpo(a.z,b.z,x), outExpo(a.w,b.w,x));
		public static double4 inOutExpo(double4 a,double4 b,double x) => new double4(inOutExpo(a.x,b.x,x), inOutExpo(a.y,b.y,x), inOutExpo(a.z,b.z,x), inOutExpo(a.w,b.w,x));
		public static double4 inCirc(double4 a,double4 b,double x) => new double4(inCirc(a.x,b.x,x), inCirc(a.y,b.y,x), inCirc(a.z,b.z,x), inCirc(a.w,b.w,x));
		public static double4 outCirc(double4 a,double4 b,double x) => new double4(outCirc(a.x,b.x,x), outCirc(a.y,b.y,x), outCirc(a.z,b.z,x), outCirc(a.w,b.w,x));
		public static double4 inOutCirc(double4 a,double4 b,double x) => new double4(inOutCirc(a.x,b.x,x), inOutCirc(a.y,b.y,x), inOutCirc(a.z,b.z,x), inOutCirc(a.w,b.w,x));
		public static double4 inBounce(double4 a,double4 b,double x) => new double4(inBounce(a.x,b.x,x), inBounce(a.y,b.y,x), inBounce(a.z,b.z,x), inBounce(a.w,b.w,x));
		public static double4 outBounce(double4 a,double4 b,double x) => new double4(outBounce(a.x,b.x,x), outBounce(a.y,b.y,x), outBounce(a.z,b.z,x), outBounce(a.w,b.w,x));
		public static double4 inOutBounce(double4 a,double4 b,double x) => new double4(inOutBounce(a.x,b.x,x), inOutBounce(a.y,b.y,x), inOutBounce(a.z,b.z,x), inOutBounce(a.w,b.w,x));
		public static double4 inBack(double4 a,double4 b,double x) => new double4(inBack(a.x,b.x,x), inBack(a.y,b.y,x), inBack(a.z,b.z,x), inBack(a.w,b.w,x));
		public static double4 outBack(double4 a,double4 b,double x) => new double4(outBack(a.x,b.x,x), outBack(a.y,b.y,x), outBack(a.z,b.z,x), outBack(a.w,b.w,x));
		public static double4 inOutBack(double4 a,double4 b,double x) => new double4(inOutBack(a.x,b.x,x), inOutBack(a.y,b.y,x), inOutBack(a.z,b.z,x), inOutBack(a.w,b.w,x));
		public static double4 inElastic(double4 a,double4 b,double x) => new double4(inElastic(a.x,b.x,x), inElastic(a.y,b.y,x), inElastic(a.z,b.z,x), inElastic(a.w,b.w,x));
		public static double4 outElastic(double4 a,double4 b,double x) => new double4(outElastic(a.x,b.x,x), outElastic(a.y,b.y,x), outElastic(a.z,b.z,x), outElastic(a.w,b.w,x));
		public static double4 inOutElastic(double4 a,double4 b,double x) => new double4(inOutElastic(a.x,b.x,x), inOutElastic(a.y,b.y,x), inOutElastic(a.z,b.z,x), inOutElastic(a.w,b.w,x));
		public static double3 spring(double3 a,double3 b,double x) => new double3(spring(a.x,b.x,x), spring(a.y,b.y,x), spring(a.z,b.z,x));
		public static double3 halfLife(double3 a,double3 b,double hl,double dt) => lerp(b,a,calcHL(hl,dt));
		public static double3 inQuad(double3 a,double3 b,double x) => new double3(inQuad(a.x,b.x,x), inQuad(a.y,b.y,x), inQuad(a.z,b.z,x));
		public static double3 outQuad(double3 a,double3 b,double x) => new double3(outQuad(a.x,b.x,x), outQuad(a.y,b.y,x), outQuad(a.z,b.z,x));
		public static double3 inOutQuad(double3 a,double3 b,double x) => new double3(inOutQuad(a.x,b.x,x), inOutQuad(a.y,b.y,x), inOutQuad(a.z,b.z,x));
		public static double3 inCubic(double3 a,double3 b,double x) => new double3(inCubic(a.x,b.x,x), inCubic(a.y,b.y,x), inCubic(a.z,b.z,x));
		public static double3 outCubic(double3 a,double3 b,double x) => new double3(outCubic(a.x,b.x,x), outCubic(a.y,b.y,x), outCubic(a.z,b.z,x));
		public static double3 inOutCubic(double3 a,double3 b,double x) => new double3(inOutCubic(a.x,b.x,x), inOutCubic(a.y,b.y,x), inOutCubic(a.z,b.z,x));
		public static double3 inQuart(double3 a,double3 b,double x) => new double3(inQuart(a.x,b.x,x), inQuart(a.y,b.y,x), inQuart(a.z,b.z,x));
		public static double3 outQuart(double3 a,double3 b,double x) => new double3(outQuart(a.x,b.x,x), outQuart(a.y,b.y,x), outQuart(a.z,b.z,x));
		public static double3 inOutQuart(double3 a,double3 b,double x) => new double3(inOutQuart(a.x,b.x,x), inOutQuart(a.y,b.y,x), inOutQuart(a.z,b.z,x));
		public static double3 inQuint(double3 a,double3 b,double x) => new double3(inQuint(a.x,b.x,x), inQuint(a.y,b.y,x), inQuint(a.z,b.z,x));
		public static double3 outQuint(double3 a,double3 b,double x) => new double3(outQuint(a.x,b.x,x), outQuint(a.y,b.y,x), outQuint(a.z,b.z,x));
		public static double3 inOutQuint(double3 a,double3 b,double x) => new double3(inOutQuint(a.x,b.x,x), inOutQuint(a.y,b.y,x), inOutQuint(a.z,b.z,x));
		public static double3 inSine(double3 a,double3 b,double x) => new double3(inSine(a.x,b.x,x), inSine(a.y,b.y,x), inSine(a.z,b.z,x));
		public static double3 outSine(double3 a,double3 b,double x) => new double3(outSine(a.x,b.x,x), outSine(a.y,b.y,x), outSine(a.z,b.z,x));
		public static double3 inOutSine(double3 a,double3 b,double x) => new double3(inOutSine(a.x,b.x,x), inOutSine(a.y,b.y,x), inOutSine(a.z,b.z,x));
		public static double3 inExpo(double3 a,double3 b,double x) => new double3(inExpo(a.x,b.x,x), inExpo(a.y,b.y,x), inExpo(a.z,b.z,x));
		public static double3 outExpo(double3 a,double3 b,double x) => new double3(outExpo(a.x,b.x,x), outExpo(a.y,b.y,x), outExpo(a.z,b.z,x));
		public static double3 inOutExpo(double3 a,double3 b,double x) => new double3(inOutExpo(a.x,b.x,x), inOutExpo(a.y,b.y,x), inOutExpo(a.z,b.z,x));
		public static double3 inCirc(double3 a,double3 b,double x) => new double3(inCirc(a.x,b.x,x), inCirc(a.y,b.y,x), inCirc(a.z,b.z,x));
		public static double3 outCirc(double3 a,double3 b,double x) => new double3(outCirc(a.x,b.x,x), outCirc(a.y,b.y,x), outCirc(a.z,b.z,x));
		public static double3 inOutCirc(double3 a,double3 b,double x) => new double3(inOutCirc(a.x,b.x,x), inOutCirc(a.y,b.y,x), inOutCirc(a.z,b.z,x));
		public static double3 inBounce(double3 a,double3 b,double x) => new double3(inBounce(a.x,b.x,x), inBounce(a.y,b.y,x), inBounce(a.z,b.z,x));
		public static double3 outBounce(double3 a,double3 b,double x) => new double3(outBounce(a.x,b.x,x), outBounce(a.y,b.y,x), outBounce(a.z,b.z,x));
		public static double3 inOutBounce(double3 a,double3 b,double x) => new double3(inOutBounce(a.x,b.x,x), inOutBounce(a.y,b.y,x), inOutBounce(a.z,b.z,x));
		public static double3 inBack(double3 a,double3 b,double x) => new double3(inBack(a.x,b.x,x), inBack(a.y,b.y,x), inBack(a.z,b.z,x));
		public static double3 outBack(double3 a,double3 b,double x) => new double3(outBack(a.x,b.x,x), outBack(a.y,b.y,x), outBack(a.z,b.z,x));
		public static double3 inOutBack(double3 a,double3 b,double x) => new double3(inOutBack(a.x,b.x,x), inOutBack(a.y,b.y,x), inOutBack(a.z,b.z,x));
		public static double3 inElastic(double3 a,double3 b,double x) => new double3(inElastic(a.x,b.x,x), inElastic(a.y,b.y,x), inElastic(a.z,b.z,x));
		public static double3 outElastic(double3 a,double3 b,double x) => new double3(outElastic(a.x,b.x,x), outElastic(a.y,b.y,x), outElastic(a.z,b.z,x));
		public static double3 inOutElastic(double3 a,double3 b,double x) => new double3(inOutElastic(a.x,b.x,x), inOutElastic(a.y,b.y,x), inOutElastic(a.z,b.z,x));
		public static double2 spring(double2 a,double2 b,double x) => new double2(spring(a.x,b.x,x), spring(a.y,b.y,x));
		public static double2 halfLife(double2 a,double2 b,double hl,double dt) => lerp(b,a,calcHL(hl,dt));
		public static double2 inQuad(double2 a,double2 b,double x) => new double2(inQuad(a.x,b.x,x), inQuad(a.y,b.y,x));
		public static double2 outQuad(double2 a,double2 b,double x) => new double2(outQuad(a.x,b.x,x), outQuad(a.y,b.y,x));
		public static double2 inOutQuad(double2 a,double2 b,double x) => new double2(inOutQuad(a.x,b.x,x), inOutQuad(a.y,b.y,x));
		public static double2 inCubic(double2 a,double2 b,double x) => new double2(inCubic(a.x,b.x,x), inCubic(a.y,b.y,x));
		public static double2 outCubic(double2 a,double2 b,double x) => new double2(outCubic(a.x,b.x,x), outCubic(a.y,b.y,x));
		public static double2 inOutCubic(double2 a,double2 b,double x) => new double2(inOutCubic(a.x,b.x,x), inOutCubic(a.y,b.y,x));
		public static double2 inQuart(double2 a,double2 b,double x) => new double2(inQuart(a.x,b.x,x), inQuart(a.y,b.y,x));
		public static double2 outQuart(double2 a,double2 b,double x) => new double2(outQuart(a.x,b.x,x), outQuart(a.y,b.y,x));
		public static double2 inOutQuart(double2 a,double2 b,double x) => new double2(inOutQuart(a.x,b.x,x), inOutQuart(a.y,b.y,x));
		public static double2 inQuint(double2 a,double2 b,double x) => new double2(inQuint(a.x,b.x,x), inQuint(a.y,b.y,x));
		public static double2 outQuint(double2 a,double2 b,double x) => new double2(outQuint(a.x,b.x,x), outQuint(a.y,b.y,x));
		public static double2 inOutQuint(double2 a,double2 b,double x) => new double2(inOutQuint(a.x,b.x,x), inOutQuint(a.y,b.y,x));
		public static double2 inSine(double2 a,double2 b,double x) => new double2(inSine(a.x,b.x,x), inSine(a.y,b.y,x));
		public static double2 outSine(double2 a,double2 b,double x) => new double2(outSine(a.x,b.x,x), outSine(a.y,b.y,x));
		public static double2 inOutSine(double2 a,double2 b,double x) => new double2(inOutSine(a.x,b.x,x), inOutSine(a.y,b.y,x));
		public static double2 inExpo(double2 a,double2 b,double x) => new double2(inExpo(a.x,b.x,x), inExpo(a.y,b.y,x));
		public static double2 outExpo(double2 a,double2 b,double x) => new double2(outExpo(a.x,b.x,x), outExpo(a.y,b.y,x));
		public static double2 inOutExpo(double2 a,double2 b,double x) => new double2(inOutExpo(a.x,b.x,x), inOutExpo(a.y,b.y,x));
		public static double2 inCirc(double2 a,double2 b,double x) => new double2(inCirc(a.x,b.x,x), inCirc(a.y,b.y,x));
		public static double2 outCirc(double2 a,double2 b,double x) => new double2(outCirc(a.x,b.x,x), outCirc(a.y,b.y,x));
		public static double2 inOutCirc(double2 a,double2 b,double x) => new double2(inOutCirc(a.x,b.x,x), inOutCirc(a.y,b.y,x));
		public static double2 inBounce(double2 a,double2 b,double x) => new double2(inBounce(a.x,b.x,x), inBounce(a.y,b.y,x));
		public static double2 outBounce(double2 a,double2 b,double x) => new double2(outBounce(a.x,b.x,x), outBounce(a.y,b.y,x));
		public static double2 inOutBounce(double2 a,double2 b,double x) => new double2(inOutBounce(a.x,b.x,x), inOutBounce(a.y,b.y,x));
		public static double2 inBack(double2 a,double2 b,double x) => new double2(inBack(a.x,b.x,x), inBack(a.y,b.y,x));
		public static double2 outBack(double2 a,double2 b,double x) => new double2(outBack(a.x,b.x,x), outBack(a.y,b.y,x));
		public static double2 inOutBack(double2 a,double2 b,double x) => new double2(inOutBack(a.x,b.x,x), inOutBack(a.y,b.y,x));
		public static double2 inElastic(double2 a,double2 b,double x) => new double2(inElastic(a.x,b.x,x), inElastic(a.y,b.y,x));
		public static double2 outElastic(double2 a,double2 b,double x) => new double2(outElastic(a.x,b.x,x), outElastic(a.y,b.y,x));
		public static double2 inOutElastic(double2 a,double2 b,double x) => new double2(inOutElastic(a.x,b.x,x), inOutElastic(a.y,b.y,x));

	}
} }

