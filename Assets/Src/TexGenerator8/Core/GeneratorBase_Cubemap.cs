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
 * CubemapのテクスチャGeneratorの基底クラスとなるクラス。
 */
abstract class GeneratorBase_Cubemap<TexPrmImpl> : GeneratorBaseT<TexPrmImpl>
	where TexPrmImpl : GeneratorBase_Cubemap<TexPrmImpl>.TexParameterBase, new()
{
	// ------------------------------------- public メンバ --------------------------------------------

	[Serializable] new public abstract class TexParameterBase : GeneratorBase.TexParameterBase {
		public int size = 1;

		/** 整合性チェック処理。派生先で追加処理を実装する事 */
		override public bool validate() {
			if ( size<1 || 1024<size ) {
				Debug.LogError("不正なサイズが指定されています："+size);
				return false;
			}

			return true;
		}

		/** テクスチャのカラーを構築する */
		abstract public Color[] buildPixels(CubemapFace cubemapFace);
	}


	// ------------------------------------- private メンバ --------------------------------------------

	// --------------------------------------------------------------------------------------------------
#if UNITY_EDITOR

	/** Ramp用のパラメータ一つ分を使用して、テクスチャを一つ生成する */
	override protected Texture generateTex( TexPrmImpl src ) {
		var tex = new Cubemap(src.size, TextureFormat.RGBA32, 1);

		for (int i=0; i<6; ++i) {
			var cols = src.buildPixels((CubemapFace)i);
			tex.SetPixels(cols, (CubemapFace)i, 0);
		}

		tex.Apply(false, true);
		return tex;
	}

#endif
}





}