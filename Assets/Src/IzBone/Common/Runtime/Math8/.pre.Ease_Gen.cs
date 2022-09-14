#if false	//#Ignore  IDE状でエラー表示させ無くするために無効にしておく
// Easing系の機能群を定義
using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Runtime.CompilerServices;
using MI = System.Runtime.CompilerServices.MethodImplAttribute; 
using MO = System.Runtime.CompilerServices.MethodImplOptions; 


namespace IzBone.Common {
static public partial class Math8 {
	static public class Ease {

		//#	var axisName = new [] { "x", "y", "z", "w" };
		//#	var iTypeList = new [] { "float", "double", };
		//#	foreach ( var iType in iTypeList ) {
		//#
		[MI(MO.AggressiveInlining)]
		public static 【iType】 spring(【iType】 a,【iType】 b,【iType】 x) {
			x = saturate(x);
			x = (
				sin(x * (【iType】)PI * ((【iType】)0.2 + (【iType】)2.5 * x * x * x))
				* pow(1-x, (【iType】)2.2) + x
			) * (1 + ((【iType】)1.2 * (1-x)));
			return a + (b - a) * x;
		}
		[MI(MO.AggressiveInlining)]
		public static 【iType】 halfLife(【iType】 a,【iType】 b,【iType】 hl,【iType】 dt) => lerp(b,a,calcHL(hl,dt));
		//#
		//#		// InOutがちゃんとあるEasingFunctionのコア処理部分の一覧
		//#		var funcs = new []{
		//#			( "Quad",		"(b-a)*x*x + a",							"-(b-a)*x*(x-2) + a" ),
		//#			( "Cubic",		"(b-a)*x*x*x + a",							null ),
		//#			( "Quart",		"(b-a)*x*x*x*x + a",						null ),
		//#			( "Quint",		"(b-a)*x*x*x*x*x + a",						null ),
		//#			( "Sine",		"(a-b) * cos(x*("+iType+")PI/2) + b",		"(b-a) * sin(x*("+iType+")PI/2) + a" ),
		//#			( "Expo",		"(b-a) * pow(2, 10*(x-1)) + a",				"(b-a) * ( 1 - pow(2, -10*x) ) + a" ),
		//#			( "Circ",		"(a-b) * (sqrt(1-x*x) - 1) + a",			"(b-a) * sqrt(-2*x - x*x) + a" ),
		//#			( "Bounce",		null,										{:
			if (x < 1 / 2.75 ) {
				return (b-a) * (【iType】)(7.5625*x*x) + a;
			} else if ( x < 2 / 2.75 ) {
				x -= (【iType】)( 1.5/2.75 );
				return (b-a) * (【iType】)(7.5625*x*x + .75) + a;
			} else if ( x < 2.5 / 2.75 ) {
				x -= (【iType】)( 2.25/2.75 );
				return (b-a) * (【iType】)(7.5625*x*x + .9375) + a;
			} else {
				x -= (【iType】)( 2.625/2.75 );
				return (b-a) * (【iType】)(7.5625*x*x + .984375) + a;
			}
		//#			:} ),
		//#			( "Back",		"(b-a) * x*x * (("+iType+")2.70158*x-("+iType+")1.70158) + a",	null ),
		//#			( "Elastic",	"x<=0 ? a : 1<=x ? b : ( (a-b)*pow(2, 10*(x-1))*sin((x-("+iType+")1.075)*("+iType+")(PI*2/0.3))+a )",	null ),
		//#		};
		//#
		//#		// InOutがちゃんとあるEasingFunctionを生成
		//#		var funcStr1 = "public static " + iType + " ";
		//#		var funcStr2 = "(" + iType + " a," + iType + " b," + iType + " x) ";
		//#		foreach ( var func in funcs ) {
		//#			// Inのイージング処理を生成
		//#			if ( func.Item2 == null ) {
		【funcStr1】in【func.Item1】【funcStr2】=> out【func.Item1】( b, a, 1-x );
		//#			} else {
		//#				var lines = func.Item2.TrimEnd().Split('\n').ToArray();
		//#				if ( lines.Length == 1 ) {
		【funcStr1】in【func.Item1】【funcStr2】=> 【func.Item2】;
		//#				} else {
		【funcStr1】in【func.Item1】【funcStr2】{
		//#					foreach ( var line in lines ) {
【line】
		//#					}
		}
		//#				}
		//#			}
		//#			// Outのイージング処理を生成
		//#			if ( func.Item3 == null ) {
		【funcStr1】out【func.Item1】【funcStr2】=> in【func.Item1】( b, a, 1-x );
		//#			} else {
		//#				var lines = func.Item3.TrimEnd().Split('\n').ToArray();
		//#				if ( lines.Length == 1 ) {
		【funcStr1】out【func.Item1】【funcStr2】=> 【func.Item3】;
		//#				} else {
		【funcStr1】out【func.Item1】【funcStr2】{
		//#					foreach ( var line in lines ) {
【line】
		//#					}
		}
		//#				}
		//#			}
		//#			// InOutのイージング処理を生成
		//#			if ( func.Item2 == null ) {
		【funcStr1】inOut【func.Item1】【funcStr2】=> x<0.5 ? (out【func.Item1】(b-a,0,1-x*2)/2+a) : (out【func.Item1】(a-b,0,x*2-1)/2+b);
		//#			} else {
		【funcStr1】inOut【func.Item1】【funcStr2】=> x<0.5 ? (in【func.Item1】(0,b-a,x*2)/2+a) : (in【func.Item1】(0,a-b,2-x*2)/2+b);
		//#			}
		//#		}
		//#
		//#
		//#		// Vector系のイージング処理を生成
		//#		for ( int axisNum = 4; axisNum != 1; --axisNum ) {
		//#			var selfCN = iType + axisNum;
		//#			var selfAxises = new string[ axisNum ];
		//#			for ( int i = 0; i < axisNum; ++i ) selfAxises[ i ] = axisName[ i ];
		//#			Func< string, Func< string, string >, string > axisJoin =
		//#				(sep,proc) => String.Join( sep, selfAxises.Select(proc).ToArray() );
		//#
		//#			Func<string,string> makeFunc = name => "public static "+selfCN+" "+name+"("+selfCN+" a,"+selfCN+" b,"+iType+" x) => new "+selfCN+"("+axisJoin( ", ", i=>name+"(a."+i+",b."+i+",x)" )+");";
		【makeFunc("spring")】
		public static 【selfCN】 halfLife(【selfCN】 a,【selfCN】 b,【iType】 hl,【iType】 dt) => lerp(b,a,calcHL(hl,dt));
		//#
		//#			foreach ( var func in funcs ) {
		【makeFunc("in"+func.Item1)】
		【makeFunc("out"+func.Item1)】
		【makeFunc("inOut"+func.Item1)】
		//#			}
		//#		}
		//#	}

	}
} }

#endif	//#Ignore
