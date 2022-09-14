using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Linq;


namespace App.Editor.ShaderInspector {

using Core;

/** Std_Fxのインスペクタ表示 */
sealed class Std_Fx : Base {
	// ------------------------------------- public メンバ ----------------------------------------

	override public void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {

		var mtlProps = new MtlProps(materialEditor, properties);

		showPropArea(
			"メインカラー",
			() => {
				mtlProps.draw_Prop("_BaseMap", "メインTexture");
				mtlProps.draw_Col("_BaseColor", "乗算色", true);
				mtlProps.draw_Col("_BaseColor_Ofs", "ｵﾌｾｯﾄ色");
			}
		);

		EditorGUILayout.Space();

		showPropArea(
			"Texアニメーションを使用",
			() => {
				mtlProps.draw_Prop("_TexAnim_MaskTex", "マスクTexture");
				mtlProps.draw_Prop("_TexAnim_MaskOnlyA", "ﾏｽｸTexをαのみ使用");

				EditorGUILayout.Space();

				mtlProps.draw_Prop("_TexAnim_DistTex", "歪みTexture");
				mtlProps.draw_Prop("_TexAnim_DistScl", "歪み量");

				EditorGUILayout.Space();

				mtlProps.draw_Vector2("_TexAnim_MainSpd", "メインTexture速度");
				mtlProps.draw_Vector2("_TexAnim_MaskSpd", "マスクTexture速度");
				mtlProps.draw_Vector2("_TexAnim_DistSpd", "歪みTexture速度");
			},
			mtlProps,
			"_UseTexAnim"
		);

		EditorGUILayout.Space();

		showPropArea(
			"環境マッピング",
			() => {
				var modeProp = mtlProps.draw_Prop("_UseEnvMap", "モード");
				var mode = (int)modeProp.floatValue;

				if (mode == 0) return;

				switch (mode) {
				case 1:		// Rim
					mtlProps.draw_Prop("_EnvMap_Rim_Pow", "Rim累乗係数");
					mtlProps.draw_Prop("_EnvMap_Rim_Ofs", "Rimｵﾌｾｯﾄ値");
					mtlProps.draw_Col("_EnvMap_Rim_Col", "Rim色", true);
					break;
				case 2:		// Sphere
					mtlProps.draw_Prop("_EnvMap_SphereTex", "Spheremap");
					break;
				case 3:		// Cube
					mtlProps.draw_Prop("_EnvMap_CubeTex", "Cubemap");
					break;
				}

				EditorGUILayout.Space();

				showBlendColorField("_EnvMap_MultAddRate", "合成方法", mtlProps);
			}
		);

		EditorGUILayout.Space();

		showPropArea(
			"α値クリッピング",
			() => {
				var modeProp = mtlProps.draw_KeywordEnum(
					"_UseAlphaClip",
					new []{ "None", "Cutoff", "Dither" },
					new []{ "無し", "カットオフ", "ディザ抜き" },
					"モード"
				);
				var mode = (int)modeProp.floatValue;

				if (mode == 1) {
					mtlProps.draw_Prop("_AlphaClip_Cutoff", "ｶｯﾄｵﾌ閾値");
				}
			}
		);

		EditorGUILayout.Space();

		showPropArea(
			"頂点カラー",
			() => {
				showBlendColorField("_VColCalcParam_MultAddRate", "合成方法", mtlProps);
			}
		);

		EditorGUILayout.Space();

		showPropArea(
			"点滅アニメーション",
			() => {
				mtlProps.draw_Prop("_BlinkAnim_Col", "カラー");
				mtlProps.draw_Prop("_BlinkAnim_Speed", "速度");
				mtlProps.draw_Prop("_BlinkAnim_Pivot", "ピボット");
			}
		);

		EditorGUILayout.Space();

		showPropArea(
			"グラデーションマップを使用",
			() => {
				mtlProps.draw_Prop("_GradMap_Tex", "Texture");
				mtlProps.draw_Prop("_GradMap_Col", "乗算色");
			},
			mtlProps,
			"_UseGradMap"
		);

		EditorGUILayout.Space();
		showRenderStateArea(mtlProps);
		EditorGUILayout.Space();
		showStencilArea(mtlProps);
	}


	// --------------------------------- private / protected メンバ -------------------------------

	// --------------------------------------------------------------------------------------------
}

}
