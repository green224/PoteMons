#if false	//#Ignore  IDE状でエラー表示させ無くするために無効にしておく
// 単純なバネシミュレーション用モジュール。
// Burst対応するために、全てStructで定義している
using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.Common {
static public partial class Math8 {

	//#	for (int i=1; i<=3; ++i) {
	//#		var iType0 = "float" + (i==1?"":i.ToString());
	//#		var iType1 = "double" + (i==1?"":i.ToString());
	//#		var iTypeUp0 = char.ToUpper(iType0[0]) + iType0.Substring(1);
	//#		var iTypeUp1 = char.ToUpper(iType1[0]) + iType1.Substring(1);
	/** 単純なバネのシミュレーション。速度優先で、軽い処理で近似する */
	public struct Spring_【iTypeUp0】 {

		public float maxX, maxV;	//!< 位置・速度最大値
		public 【iType0】 x, v;			//!< 位置と速度
		public float kpm, vHL;		//!< バネ係数/質量と速度半減期

		/** 更新処理 */
		public void update(float dt) {
			// バネ振動による加速度と空気抵抗による半減期から、新しい速度を算出
			var a = -x * kpm;
			var newV = (v + a*dt) * calcHL(vHL, dt);

			// 新しい速度に直線的に遷移したと仮定して、位置を更新
			x += (v + newV)/2 * dt;
			v = newV;

			// 範囲情報でクリッピング
			x = clamp(x, -maxX, maxX);
			v = clamp(v, -maxV, maxV);
		}
	}

	/** 単純なバネのシミュレーション。単振動部分を解析的に解くため、少し処理が重い */
	public struct SpringDat_【iTypeUp1】 {

		public double maxX, maxV;	//!< 位置・速度最大値
		public 【iType1】 x, v;			//!< 位置と速度
		public double omg, vHL;		//!< 単振動角速度と速度半減期

		/** 更新処理 */
		public void update(double dt) {
			// 現在の位置、速度から、単振動のためのパラメータを取得
			var omgT0 = atan2( omg*x, v );
			var len = x / sin( omgT0 );

			// dt後の単振動による位置・速度を解析的に解く
			var t = omg*dt + omgT0;
			x = len * sin( t );
			v = omg*len * cos( t );

			// 半減期による減速を行う
			v *= calcHL(vHL, dt);

			// 範囲情報でクリッピング
			x = clamp(x, -maxX, maxX);
			v = clamp(v, -maxV, maxV);
		}
	}

	//#	}

} }

#endif	//#Ignore
