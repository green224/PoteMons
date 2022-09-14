using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;

using System.Collections.Generic;


namespace App.Ingame.Editor.Common {

/**
 * URPのデフォルトシェーダを除外する処理を挟むためのモジュール
 *
 * 参考 : https://qiita.com/piti5/items/1f8d3bdfe5a64e478c7e
 */
public class OptimizeShaderPreprocessor : IPreprocessShaders
{

	//IPreprocessShadersの呼び順を制御します。値が小さいほど先に呼ばれます。
	int IOrderedCallback.callbackOrder => default;


	void IPreprocessShaders.OnProcessShader(
		Shader shader,
		ShaderSnippetData snippet,
		IList<ShaderCompilerData> data
	) {
		if ( shader.name.StartsWith(
			"Universal Render Pipeline",
			StringComparison.OrdinalIgnoreCase
		) ) {

			//URP命名に引っかかったら対象から全て外す
			data.Clear();
		}
	}
}

}
