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
 * 各種テクスチャGeneratorの基底クラスとなるクラス。
 * ただし、これから直接派生せずに、GeneratorBaseTから派生する事。
 */
abstract class GeneratorBase : MonoBehaviour {
	// ------------------------------------- public メンバ --------------------------------------------

	[Serializable] public abstract class TexParameterBase {
		public string name = "";
//		[HideInInspector] public Texture resultTex = null;

		/** 整合性チェック処理。派生先で追加処理を実装する事 */
		abstract public bool validate();
	}


	// ------------------------------------- private メンバ --------------------------------------------

#if UNITY_EDITOR
	abstract protected TexParameterBase[] TexParameters {get;}



	// --------------------------------------------------------------------------------------------------
	/** 設定されているパラメータからテクスチャを全再生成する */
	void regenerateTex() {

		var thisPath = AssetDatabase.GetAssetPath(gameObject);
		if (string.IsNullOrEmpty(thisPath)) {
			Debug.LogError("RegenerateはPrefab化してから行ってください");
			return;
		}

		{// 一旦ここでバリデーションをかける
			var names = new HashSet<string>();
			foreach (var i in TexParameters) {
				if ( names.Contains(i.name) ) {
					Debug.LogError("名前が重複しています："+i.name);
					return;
				}
				if ( !i.validate() ) return;
			}
		}

//		// 現在の生成済みテクスチャを全削除。
//		foreach (var i in rampList) {
//			if (i.resultTex == null) return;
//			DestroyImmediate(i.resultTex);
//		}

		// テクスチャを全生成
		var dirPath = System.IO.Path.GetDirectoryName(thisPath);
		foreach (var i in TexParameters) {
			var tex = generateTex(i);
			var dstPath = dirPath + "/" + i.name;
			saveTexAsset(dstPath, tex);
//			i.resultTex = AssetDatabase.LoadAssetAtPath<Texture2D>(dstPath);
		}
	}

	/** 指定の位置に、指定のテクスチャを保存する */
	void saveTexAsset(string dstPath, Texture tex) {
		if (tex is Texture2D) {
			// Texture2Dを保存する場合
			dstPath += ".png";
			var isOverwrite = System.IO.File.Exists(dstPath);

			System.IO.File.WriteAllBytes(dstPath, ( (Texture2D)tex ).EncodeToPNG());
			AssetDatabase.ImportAsset(dstPath);

			if (!isOverwrite) {
				var importer = (TextureImporter)TextureImporter.GetAtPath(dstPath);
				importer.wrapMode = TextureWrapMode.Clamp;
				importer.npotScale = TextureImporterNPOTScale.None;
				importer.mipmapEnabled = false;
				importer.textureCompression = TextureImporterCompression.Uncompressed;
				importer.SaveAndReimport();
			}

		} else if (tex is Cubemap) {
			// Cubemapを保存する場合
			dstPath += ".asset";
			var isOverwrite = System.IO.File.Exists(dstPath);

			if (isOverwrite) {
				AssetDatabase.CreateAsset(tex, dstPath+".tmp");
				FileUtil.ReplaceFile(dstPath+".tmp", dstPath);
				AssetDatabase.DeleteAsset(dstPath+".tmp");
				AssetDatabase.ImportAsset(dstPath);
			} else {
				AssetDatabase.CreateAsset(tex, dstPath);
			}
		}
	}

	/** テクスチャ生成のパラメータ一つ分を使用して、テクスチャを一つ生成する */
	abstract protected Texture generateTex( TexParameterBase src );

	[CustomEditor(typeof(GeneratorBase), true)]
	sealed class CustomInspector : Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			EditorGUILayout.Space();
			if (GUILayout.Button("テクスチャを生成")) {
				((GeneratorBase)target).regenerateTex();
			}
		}
	}
#endif
}


/**
 * 各種テクスチャGeneratorの直接の派生元となるクラス。
 * Editor拡張がGenerics非対応なので仕方なくNonGenericsクラスを基底に持っている。
 */
abstract class GeneratorBaseT<TexPrmImpl> : GeneratorBase
	where TexPrmImpl : GeneratorBase.TexParameterBase, new()
{
#if UNITY_EDITOR
	public TexPrmImpl[] texParams = new [] {new TexPrmImpl()};

	override protected TexParameterBase[] TexParameters => texParams;

	/** テクスチャ生成のパラメータ一つ分を使用して、テクスチャを一つ生成する */
	override protected Texture generateTex( TexParameterBase src )
		=> generateTex( (TexPrmImpl)src );
	abstract protected Texture generateTex( TexPrmImpl src );
#endif
}




}