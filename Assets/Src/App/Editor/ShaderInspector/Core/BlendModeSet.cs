using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine.Rendering;
using System.Collections.Generic;


namespace App.Editor.ShaderInspector.Core {

/** ブレンドモード情報 */
sealed class BlendModeSet {
	// ------------------------------------- public メンバ ----------------------------------------

	readonly public int presetIdx;	// プリセット番号
	readonly public string name;	// 名前

	// 実際にRenderStateに設定するパラメータ
	readonly public BlendOp blendOp;
	readonly public BlendMode srcBlend;
	readonly public BlendMode dstBlend;
	readonly public bool isPreCombineAlpha;		// RGBへα値を事前に反映しておくか否か
	readonly public float preCombineAlphaPivot;	// RGBへα値を事前に反映しておく際の、α=0でのフェード先

	readonly static public BlendModeSet Opacity;
	readonly static public BlendModeSet Alpha;
	readonly static public BlendModeSet Add;
	readonly static public BlendModeSet Subtract;
	readonly static public BlendModeSet Multiply;
	readonly static public BlendModeSet MultiplyX2;
	readonly static public BlendModeSet Max;
	readonly static public BlendModeSet Min;
	readonly static public BlendModeSet Screen;
	readonly static public BlendModeSet Exclusion;

	// プリセット一覧
	readonly static public BlendModeSet[] Presets;

	// 指定のパラメータに一致するBlendModeSetを得る
	static public BlendModeSet find(
		BlendOp blendOp, BlendMode srcBlend, BlendMode dstBlend,
		bool isPreCombineAlpha, float preCombineAlphaPivot
	) {
		foreach (var i in Presets) {
			if (i.blendOp!=blendOp || i.srcBlend!=srcBlend || i.dstBlend!=dstBlend) continue;
			if (i.isPreCombineAlpha) {
				if (!isPreCombineAlpha || preCombineAlphaPivot!=i.preCombineAlphaPivot) continue;
			} else {
				if (isPreCombineAlpha) continue;
			}

			return i;
		}
		return null;
	}


	// --------------------------------- private / protected メンバ -------------------------------

	static BlendModeSet() {
		var presetLst = new List<BlendModeSet>();
		BlendModeSet addPreset(
			string name,
			BlendOp blendOp,
			BlendMode srcBlend,
			BlendMode dstBlend,
			float? combineAlphaPivot = null
		) {
			var ret = new BlendModeSet(
				presetLst.Count,
				name,
				blendOp, srcBlend, dstBlend,
				combineAlphaPivot.HasValue,
				combineAlphaPivot ?? 0
			);
			presetLst.Add( ret );
			return ret;
		}

		Opacity = addPreset( "不透明",
			BlendOp.Add,
			BlendMode.One, BlendMode.Zero
		);
		Alpha = addPreset( "α合成",
			BlendOp.Add,
			BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha
		);
		Add = addPreset( "加算",
			BlendOp.Add,
			BlendMode.SrcAlpha, BlendMode.One
		);
		Subtract = addPreset( "減算",
			BlendOp.ReverseSubtract,
			BlendMode.SrcAlpha, BlendMode.One
		);
		Multiply = addPreset( "乗算",
			BlendOp.Add,
			BlendMode.DstColor, BlendMode.Zero, 1
		);
		MultiplyX2 = addPreset( "2x乗算",
			BlendOp.Add,
			BlendMode.DstColor, BlendMode.SrcColor, 0.5f
		);
		Max = addPreset( "比較（明）",
			BlendOp.Max,
			BlendMode.One, BlendMode.One, 0
		);
		Min = addPreset( "比較（暗）",
			BlendOp.Min,
			BlendMode.One, BlendMode.One, 1
		);
		Screen = addPreset( "スクリーン",
			BlendOp.Add,
			BlendMode.OneMinusDstColor, BlendMode.One, 0
		);
		Exclusion = addPreset( "除外",
			BlendOp.Add,
			BlendMode.OneMinusDstColor, BlendMode.OneMinusSrcColor, 0
		);

		Presets = presetLst.ToArray();
	}

	BlendModeSet(
		int presetIdx,
		string name,
		BlendOp blendOp,
		BlendMode srcBlend,
		BlendMode dstBlend,
		bool isPreCombineAlpha,
		float combineAlphaPivot
	) {
		this.presetIdx = presetIdx;
		this.name = name;
		this.blendOp = blendOp;
		this.srcBlend = srcBlend;
		this.dstBlend = dstBlend;
		this.isPreCombineAlpha = isPreCombineAlpha;
		this.preCombineAlphaPivot = combineAlphaPivot;
	}


	// --------------------------------------------------------------------------------------------
}

}
