using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace TexGenerator8 {

/**
 * Rampテクスチャを生成するツール。
 * プレファブでデータを格納するので、Editor拡張にはしない(できない)
 */
[AddComponentMenu("TexGenerator8/TexGenerator_Ramp")]
sealed class Ramp : Core.GeneratorBase_Tex2D<Ramp.OneRamp> {
	// ------------------------------------- public メンバ --------------------------------------------

	[Serializable] public sealed class OneRamp : TexParameterBase {
		public Gradient[] colorGrad = null;
		public AnimationCurve yMapCurve = null;		//!< Y軸方向のグラデーションの混ぜ具合をどうするかを定義するカーブ

		public OneRamp() {
			size = int2(100,1);
		}

		/** 整合性チェック処理 */
		override public bool validate() {
			if (!base.validate()) return false;

			if ( colorGrad == null ) {
				Debug.LogError("データが未初期化です");
				return false;
			}

			return true;
		}

		/** テクスチャのカラーを構築する */
		override public Color[] buildPixels() {
			var cols = new Color[size.x * size.y];
			for (int y=0, i=0; y<size.y; ++y)
			for (int x=0; x<size.x; ++x, ++i) {
				var xRate = (float)x / size.x;
				if (colorGrad.Length == 1) {
					cols[i] = colorGrad[0].Evaluate(xRate);
				} else {
					var yRate = clamp(
						yMapCurve.Evaluate( (float)y / size.y * colorGrad.Length ),
						0, colorGrad.Length-1
					);
					cols[i] = Color.Lerp(
						colorGrad[ clamp( (int)yRate, 0, colorGrad.Length-1 ) ].Evaluate(xRate),
						colorGrad[ clamp( (int)yRate+1, 0, colorGrad.Length-1 ) ].Evaluate(xRate),
						yRate - floor(yRate)
					);
				}
			}
			return cols;
		}
	}


	// ------------------------------------- private メンバ --------------------------------------------

	// --------------------------------------------------------------------------------------------------
}

}