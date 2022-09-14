using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;
using static Unity.Mathematics.math;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TexGenerator8.Core {

/**
 * Texture2DのテクスチャGeneratorの基底クラスとなるクラス。
 */
abstract class GeneratorBase_Tex2D<TexPrmImpl> : GeneratorBaseT<TexPrmImpl>
	where TexPrmImpl : GeneratorBase_Tex2D<TexPrmImpl>.TexParameterBase, new()
{
	// ------------------------------------- public メンバ --------------------------------------------

	[Serializable] new public abstract class TexParameterBase : GeneratorBase.TexParameterBase {
		public int2 size = 64;

		/** 整合性チェック処理。派生先で追加処理を実装する事 */
		override public bool validate() {
			if ( size.x<1||2048<size.x || size.y<1||2048<size.y ) {
				Debug.LogError("不正なサイズが指定されています："+size);
				return false;
			}

			return true;
		}

		/** テクスチャのカラーを構築する */
		abstract public Color[] buildPixels();
	}


	// ------------------------------------- private メンバ --------------------------------------------

	// --------------------------------------------------------------------------------------------------
#if UNITY_EDITOR

	/** Ramp用のパラメータ一つ分を使用して、テクスチャを一つ生成する */
	override protected Texture generateTex( TexPrmImpl src ) {
		var tex = new Texture2D(src.size.x, src.size.y);

		var cols = src.buildPixels();

		tex.SetPixels(cols);
		tex.Apply(false, false);
		return tex;
	}

#endif
}





}