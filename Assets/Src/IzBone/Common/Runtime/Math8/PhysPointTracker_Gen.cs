using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace IzBone.Common {
static public partial class Math8  {

	/** 指定位置を指定した物理パラメータで追跡する挙動を再現するモジュール */
	public struct PhysPointTracker_Float {

		public float target, value, v;	//!< 目標値・現在値・現在速度
		public float fadeHL;			//!< 目標値へのフェードで近づく際の半減期
		public float a, vMax;			//!< 加速度と最高速度

		/** 更新処理。使用する間は、これを毎フレーム呼んで更新する */
		public void update(float dt) {
			if (dt==0) return;

			// fadeHLで近づくスピード以下になるように目標速度を設定する
			var v2t = target - value;
			var len = length(v2t);
			float tgtV = default;
			if (len!=0) tgtV = v2t * min( vMax/len, (1f-calcHL(fadeHL, dt))/dt );

			// 目標速度へ向けて加速する
			var diffV = tgtV - v;
			var diffVLen = length(diffV);
			if (diffVLen!=0) {
				var dltV = diffV * min(1, a*dt / diffVLen);
				v += dltV;
			}

			// 速度を位置へ反映
			value += v*dt;
		}
	}
	/** 指定位置を指定した物理パラメータで追跡する挙動を再現するモジュール */
	public struct PhysPointTracker_Double {

		public double target, value, v;	//!< 目標値・現在値・現在速度
		public float fadeHL;			//!< 目標値へのフェードで近づく際の半減期
		public float a, vMax;			//!< 加速度と最高速度

		/** 更新処理。使用する間は、これを毎フレーム呼んで更新する */
		public void update(float dt) {
			if (dt==0) return;

			// fadeHLで近づくスピード以下になるように目標速度を設定する
			var v2t = target - value;
			var len = length(v2t);
			double tgtV = default;
			if (len!=0) tgtV = v2t * min( vMax/len, (1f-calcHL(fadeHL, dt))/dt );

			// 目標速度へ向けて加速する
			var diffV = tgtV - v;
			var diffVLen = length(diffV);
			if (diffVLen!=0) {
				var dltV = diffV * min(1, a*dt / diffVLen);
				v += dltV;
			}

			// 速度を位置へ反映
			value += v*dt;
		}
	}
	/** 指定位置を指定した物理パラメータで追跡する挙動を再現するモジュール */
	public struct PhysPointTracker_Float2 {

		public float2 target, value, v;	//!< 目標値・現在値・現在速度
		public float fadeHL;			//!< 目標値へのフェードで近づく際の半減期
		public float a, vMax;			//!< 加速度と最高速度

		/** 更新処理。使用する間は、これを毎フレーム呼んで更新する */
		public void update(float dt) {
			if (dt==0) return;

			// fadeHLで近づくスピード以下になるように目標速度を設定する
			var v2t = target - value;
			var len = length(v2t);
			float2 tgtV = default;
			if (len!=0) tgtV = v2t * min( vMax/len, (1f-calcHL(fadeHL, dt))/dt );

			// 目標速度へ向けて加速する
			var diffV = tgtV - v;
			var diffVLen = length(diffV);
			if (diffVLen!=0) {
				var dltV = diffV * min(1, a*dt / diffVLen);
				v += dltV;
			}

			// 速度を位置へ反映
			value += v*dt;
		}
	}
	/** 指定位置を指定した物理パラメータで追跡する挙動を再現するモジュール */
	public struct PhysPointTracker_Double2 {

		public double2 target, value, v;	//!< 目標値・現在値・現在速度
		public float fadeHL;			//!< 目標値へのフェードで近づく際の半減期
		public float a, vMax;			//!< 加速度と最高速度

		/** 更新処理。使用する間は、これを毎フレーム呼んで更新する */
		public void update(float dt) {
			if (dt==0) return;

			// fadeHLで近づくスピード以下になるように目標速度を設定する
			var v2t = target - value;
			var len = length(v2t);
			double2 tgtV = default;
			if (len!=0) tgtV = v2t * min( vMax/len, (1f-calcHL(fadeHL, dt))/dt );

			// 目標速度へ向けて加速する
			var diffV = tgtV - v;
			var diffVLen = length(diffV);
			if (diffVLen!=0) {
				var dltV = diffV * min(1, a*dt / diffVLen);
				v += dltV;
			}

			// 速度を位置へ反映
			value += v*dt;
		}
	}
	/** 指定位置を指定した物理パラメータで追跡する挙動を再現するモジュール */
	public struct PhysPointTracker_Float3 {

		public float3 target, value, v;	//!< 目標値・現在値・現在速度
		public float fadeHL;			//!< 目標値へのフェードで近づく際の半減期
		public float a, vMax;			//!< 加速度と最高速度

		/** 更新処理。使用する間は、これを毎フレーム呼んで更新する */
		public void update(float dt) {
			if (dt==0) return;

			// fadeHLで近づくスピード以下になるように目標速度を設定する
			var v2t = target - value;
			var len = length(v2t);
			float3 tgtV = default;
			if (len!=0) tgtV = v2t * min( vMax/len, (1f-calcHL(fadeHL, dt))/dt );

			// 目標速度へ向けて加速する
			var diffV = tgtV - v;
			var diffVLen = length(diffV);
			if (diffVLen!=0) {
				var dltV = diffV * min(1, a*dt / diffVLen);
				v += dltV;
			}

			// 速度を位置へ反映
			value += v*dt;
		}
	}
	/** 指定位置を指定した物理パラメータで追跡する挙動を再現するモジュール */
	public struct PhysPointTracker_Double3 {

		public double3 target, value, v;	//!< 目標値・現在値・現在速度
		public float fadeHL;			//!< 目標値へのフェードで近づく際の半減期
		public float a, vMax;			//!< 加速度と最高速度

		/** 更新処理。使用する間は、これを毎フレーム呼んで更新する */
		public void update(float dt) {
			if (dt==0) return;

			// fadeHLで近づくスピード以下になるように目標速度を設定する
			var v2t = target - value;
			var len = length(v2t);
			double3 tgtV = default;
			if (len!=0) tgtV = v2t * min( vMax/len, (1f-calcHL(fadeHL, dt))/dt );

			// 目標速度へ向けて加速する
			var diffV = tgtV - v;
			var diffVLen = length(diffV);
			if (diffVLen!=0) {
				var dltV = diffV * min(1, a*dt / diffVLen);
				v += dltV;
			}

			// 速度を位置へ反映
			value += v*dt;
		}
	}

} }

