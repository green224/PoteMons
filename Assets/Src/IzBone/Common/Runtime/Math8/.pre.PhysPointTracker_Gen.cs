#if false	//#Ignore  IDE状でエラー表示させ無くするために無効にしておく
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
	//#		var lengthFunc = i==0 ? "abs" : "length";
	//#		for (int j=0; j<2; ++j) {
	//#			var iType = j==0 ? iType0 : iType1;
	//#			var iTypeUp = j==0 ? iTypeUp0 : iTypeUp1;
	/** 指定位置を指定した物理パラメータで追跡する挙動を再現するモジュール */
	public struct PhysPointTracker_【iTypeUp】 {

		public 【iType】 target, value, v;	//!< 目標値・現在値・現在速度
		public float fadeHL;			//!< 目標値へのフェードで近づく際の半減期
		public float a, vMax;			//!< 加速度と最高速度

		/** 更新処理。使用する間は、これを毎フレーム呼んで更新する */
		public void update(float dt) {
			if (dt==0) return;

			// fadeHLで近づくスピード以下になるように目標速度を設定する
			var v2t = target - value;
			var len = 【lengthFunc】(v2t);
			【iType】 tgtV = default;
			if (len!=0) tgtV = v2t * min( vMax/len, (1f-calcHL(fadeHL, dt))/dt );

			// 目標速度へ向けて加速する
			var diffV = tgtV - v;
			var diffVLen = 【lengthFunc】(diffV);
			if (diffVLen!=0) {
				var dltV = diffV * min(1, a*dt / diffVLen);
				v += dltV;
			}

			// 速度を位置へ反映
			value += v*dt;
		}
	}
	//#		}
	//#	}

} }

#endif	//#Ignore
