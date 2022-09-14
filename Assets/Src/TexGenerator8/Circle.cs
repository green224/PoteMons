using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace TexGenerator8 {
using Core;

/**
 * 円形状のテクスチャを生成するツール。
 * プレファブでデータを格納するので、Editor拡張にはしない(できない)
 */
[AddComponentMenu("TexGenerator8/TexGenerator_Circle")]
sealed class Circle : GeneratorBase_Tex2D<Circle.OneCircle> {
	// ------------------------------------- public メンバ --------------------------------------------

	[Serializable] public sealed class OneCircle : TexParameterBase {
		[Serializable] public sealed class Layer {
			public string name = "layer1";			// レイヤー名。単なるメモ
			[Range(0,1)] public float alpha = 1;	// レイヤーの不透明度

			public ColorGrad colorGrad_R = new ColorGrad();
			public ColorGrad colorGrad_Theta = new ColorGrad();
			public AnimationCurve rPerThetaCurve =		// 角度ごとの半径の大きさを定義するカーブ
				new AnimationCurve(new []{
					new Keyframe(0, 1),
					new Keyframe(1, 1),
				});

			[Range(0,1)] public float thetaOffset = 0;
			[Vec2Range(0,1)] public Vector2 rRange = new Vector2(0,1);
			[Min(1)] public int armCnt = 1;
			public bool isSymmetry = false;

			public enum BlendMode {Alpha, Add, MultValue,}
			public BlendMode blendMode = BlendMode.Alpha;

			/** 整合性チェック処理 */
			public bool validate() {
				if ( colorGrad_R==null || colorGrad_Theta==null || rPerThetaCurve==null ) {
					Debug.LogError("データが未初期化です");
					return false;
				}
				return true;
			}

			/** 指定座標のテクスチャのカラーを計算する */
			public Color calcPixel(float theta, float r) {
				theta += 1.5f*PI + thetaOffset*2*PI;
				var cycleLen = 2*PI / max(1,armCnt);
				if (isSymmetry) cycleLen/=2;

				var c = (int)(theta / cycleLen);
				var t = (theta % cycleLen) / cycleLen;
				if (isSymmetry && c%2==1) t = 1f-t;
				var rMax = lerp(
					rRange.x,
					rRange.y,
					max( 0, rPerThetaCurve.Evaluate(t) )
				);
				var ret =
					colorGrad_Theta.evaluate( t ) *
					colorGrad_R.evaluate( r / (rMax+0.000001f) );
				ret.a *= alpha;
				return ret;
			}
		}

		public Layer[] layers = new []{ new Layer() };

		/** 整合性チェック処理 */
		override public bool validate() {
			if (!base.validate()) return false;

			if ( layers == null ) {
				Debug.LogError("データが未初期化です");
				return false;
			}

			foreach (var i in layers) if (!i.validate()) return false;

			return true;
		}

		/** テクスチャのカラーを構築する */
		override public Color[] buildPixels() {

			// 指定位置を極座標で表したものを得る
			static void calcThetaR(
				float2 pos, int2 size,
				out float theta,
				out float r
			) {
				var maxWidth = max(size.x, size.y);

				var posRate = float2(
					(pos.x - size.x/2) / maxWidth * 2,
					(pos.y - size.y/2) / maxWidth * 2
				);
				theta = atan2(posRate.y, posRate.x);
				r = length(posRate);
			}

			// 全てのピクセルについて、ピクセル内の各位置について計算して平均化
			var cols = new Color[size.x * size.y];
			for (int phase=0; phase<5; ++phase) {
				float2 offset = phase switch {
					0 => float2(0, 0),
					1 => float2(1, 0),
					2 => float2(0, 1),
					3 => float2(1, 1),
					_ => float2(0.5f, 0.5f),
				};

				for (int y=0, i=0; y<size.y; ++y)
				for (int x=0; x<size.x; ++x, ++i) {

					calcThetaR(float2(x,y)+offset, size, out var theta, out var r);

					Color c = Color.clear;
					foreach (var j in layers) {
						// レイヤーのカラーを計算
						var lc = j.calcPixel(theta, r);


						// ブレンドモードによって、合成方法を変える

						// srcのαとdstのαが共に非常に小さいピクセルで、
						// srcのカラーが反映されてしまうのが結構アーティファクトとして出てしまうので、
						// srcのαが小さい場合は適応しないようにするための係数。
						float thrA = 1 - lc.a;
						thrA = thrA*thrA*thrA;
						var rgb_dst = float3( c.r, c.g, c.b );
						var rgb_src = float3( lc.r, lc.g, lc.b );

						// srcとdstのα比での加重平均
						var rgb_avr = saturate( float3(
							c.r * c.a + lc.r * lc.a,
							c.g * c.a + lc.g * lc.a,
							c.b * c.a + lc.b * lc.a
						) ) / (c.a+lc.a+0.0000001f);

						switch (j.blendMode) {
						case Layer.BlendMode.Alpha : {
							// 基本的には加重平均からのlerpでよいが、
							// srcのαが小さい場合は適応しないようにする
							var rgb = lerp(
								lerp( rgb_avr, rgb_dst, thrA ),
								rgb_src,
								lc.a
							);
							c = new Color(
								rgb.x, rgb.y, rgb.z,
								lerp(c.a, 1, lc.a)
							);
							}break;
						case Layer.BlendMode.Add : {
							// 基本的には加重平均との加算でよいが、
							// srcのαが小さい場合は適応しないようにする
							var rgb = saturate(
								lerp( rgb_avr, rgb_dst, thrA )
								+ rgb_src * lc.a / 2	// 平均との加算は輝きすぎるので半分で
							);
							c = new Color(
								rgb.x, rgb.y, rgb.z,
								lerp(c.a, 1, lc.a)
							);
							}break;
						case Layer.BlendMode.MultValue :
							c = new Color(
								c.r * lc.r,
								c.g * lc.g,
								c.b * lc.b,
								c.a * lc.a
							);
							break;
						}
					}

					cols[i] += c;
				}
			}
			for (int y=0, i=0; y<size.y; ++y)
			for (int x=0; x<size.x; ++x, ++i) cols[i] /= 5;

			
			return cols;
		}
	}


	// ------------------------------------- private メンバ --------------------------------------------

	// --------------------------------------------------------------------------------------------------
}

}