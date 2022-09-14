//#CsPP
//#	public partial class Program {
//#
//#		static string[] make4Vec( bool icludeInt, Func< string, string, string[], Func< string, Func< string, string >, string >, string > makeLineProc ) {
//#			var ret = new List<string>();
//#			var axisName = new [] { "x", "y", "z", "w" };
//#			var innerTypeList = icludeInt ? new [] { "int", "uint", "float", "double", } : new [] { "float", "double", };
//#			foreach ( var innerType in innerTypeList ) {
//#				for ( int axisNum = 4; axisNum != 1; --axisNum ) {
//#					var selfCN = innerType + axisNum;
//#					var selfAxises = new string[ axisNum ];
//#					for ( int i = 0; i < axisNum; ++i ) selfAxises[ i ] = axisName[ i ];
//#					Func< string, Func< string, string >, string > axisJoin =
//#						(sep,proc) => String.Join( sep, selfAxises.Select(proc).ToArray() );
//#
//#					var lines = makeLineProc( innerType, selfCN, selfAxises, axisJoin ).Split('\n').Select(i=>i.Trim()).ToArray();
//#					foreach ( var line in lines ) ret.Add( line );
//#				}
//#			}
//#			return ret.ToArray();
//#		}
//#
//#		public static void Main() {

#if false	//#Ignore  IDE状でエラー表示させ無くするために無効にしておく

// その他のStatic機能を定義
using System;
using Unity.Mathematics;
using System.Runtime.CompilerServices;
using MI = System.Runtime.CompilerServices.MethodImplAttribute; 
using MO = System.Runtime.CompilerServices.MethodImplOptions; 


namespace IzBone.Common {
static public partial class Math8 {

	// 指定の範囲内で、数値を繰り返す
	//#	{
	//#		var innerTypeList = new [] { "int", "uint", "long", "ulong", "float", "double", };
	//#		foreach ( var innerType in innerTypeList ) {
	static public 【innerType】 wrap( 【innerType】 x, 【innerType】 min, 【innerType】 max ) {
		if ( min < x )	return ( (x-min) % (max-min) ) + min;
		else			return max - ( (max-x) % (max-min) );
	}
	//# 	}
	//# }
	//# foreach ( var j in make4Vec( true, (it, sCN, sa, axisJoin) =>
	//# 	"[MI(MO.AggressiveInlining)] static public "+sCN+" wrap("+sCN+" x,"+sCN+" min,"+sCN+" max) => new "+sCN+"("+axisJoin( ",", i=>"wrap(x."+i+",min."+i+",max."+i+")" )+");"
	//#	) ) {
	【j】
	//#	}

	// 角度を正規化する
	//#	{
	//#		var innerTypeList = new [] { "float", "double", };
	//#		foreach ( var innerType in innerTypeList ) {
	[MI(MO.AggressiveInlining)] static public 【innerType】 wrapDegree(【innerType】 x) => wrap(x, -180, 180);
	[MI(MO.AggressiveInlining)] static public 【innerType】 wrapRadian(【innerType】 x) => wrap(x, -(【innerType】)Unity.Mathematics.math.PI, (【innerType】)Unity.Mathematics.math.PI);
	//# 	}
	//# }
	//# foreach ( var j in make4Vec( false, (it, sCN, sa, axisJoin) =>
	//# 	"[MI(MO.AggressiveInlining)] static public "+sCN+" wrapDegree("+sCN+" x) => new "+sCN+"("+axisJoin( ",", i=>"wrapDegree(x."+i+")" )+");"
	//#	) ) {
	【j】
	//#	}
	//# foreach ( var j in make4Vec( false, (it, sCN, sa, axisJoin) =>
	//# 	"[MI(MO.AggressiveInlining)] static public "+sCN+" wrapRadian("+sCN+" x) => new "+sCN+"("+axisJoin( ",", i=>"wrapRadian(x."+i+")" )+");"
	//#	) ) {
	【j】
	//#	}

	// 整数に変更する
	//# foreach ( var j in make4Vec( false, (it, sCN, sa, axisJoin) => {
	//# 	var intVec = "int" + sCN.Substring(sCN.Length-1);
	//# 	return "[MI(MO.AggressiveInlining)] static public "+intVec+" round2int("+sCN+" x) => new "+intVec+"("+axisJoin( ",", i=>"round2int(x."+i+")" )+");";
	//#	} ) ) {
	【j】
	//#	}

	// ベクトルから指定の方向の成分を取り除く
	//# foreach ( var j in make4Vec( false, (it, sCN, sa, axisJoin) => {
	//# 	return "[MI(MO.AggressiveInlining)] static public "+sCN+" rmvDir("+sCN+" src, "+sCN+" dir) => src - math.dot(src,dir)*dir;";
	//#	} ) ) {
	【j】
	//#	}

	// smoothstepの直線版
	//#	{
	//#		var innerTypeList = new [] { "float", "double", };
	//#		foreach ( var innerType in innerTypeList ) {
	[MI(MO.AggressiveInlining)] static public 【innerType】 linearStep(【innerType】 a, 【innerType】 b, 【innerType】 x) => math.saturate((x-a)/(b-a));
	//# 	}
	//# }
	//# foreach ( var j in make4Vec( false, (it, sCN, sa, axisJoin) =>
	//# 	"[MI(MO.AggressiveInlining)] static public "+sCN+" linearStep("+sCN+" a,"+sCN+" b,"+sCN+" x) => math.saturate((x-a)/(b-a));"
	//#	) ) {
	【j】
	//#	}

} }

#endif	//#Ignore

//# } }