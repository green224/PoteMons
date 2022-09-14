using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace TexGenerator8 {
using Core;

/**
 * 各面一色のCubemapテクスチャを生成するツール。
 * プレファブでデータを格納するので、Editor拡張にはしない(できない)
 */
[AddComponentMenu("TexGenerator8/TexGenerator_Cube")]
sealed class Cube : GeneratorBase_Cubemap<Cube.OneCube> {
	// ------------------------------------- public メンバ --------------------------------------------

	[Serializable] public sealed class OneCube : TexParameterBase {

		public Color
			col_PositiveX,
			col_NegativeX,
			col_PositiveY,
			col_NegativeY,
			col_PositiveZ,
			col_NegativeZ;

		/** 整合性チェック処理 */
		override public bool validate() {
			size = 1;	// サイズを強制的に1にする

			return base.validate();
		}

		/** テクスチャのカラーを構築する */
		override public Color[] buildPixels(CubemapFace cubemapFace) {
			var col = cubemapFace switch {
				CubemapFace.PositiveX => col_PositiveX,
				CubemapFace.NegativeX => col_NegativeX,
				CubemapFace.PositiveY => col_PositiveY,
				CubemapFace.NegativeY => col_NegativeY,
				CubemapFace.PositiveZ => col_PositiveZ,
				CubemapFace.NegativeZ => col_NegativeZ,
				_ => throw new ArgumentException("" + cubemapFace),
			};

			var ret = new Color[size*size];
			for (int i=0; i<ret.Length; ++i) ret[i] = col;

			return ret;
		}
	}


	// ------------------------------------- private メンバ --------------------------------------------

	// --------------------------------------------------------------------------------------------------
}

}