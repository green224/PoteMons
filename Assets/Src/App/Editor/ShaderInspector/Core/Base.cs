using System;
using UnityEngine;
using UnityEditor;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Linq;
using UnityEngine.Rendering;


namespace App.Editor.ShaderInspector.Core {

/** インスペクタ表示を行うクラスの基底クラス */
abstract class Base : ShaderGUI {
	// ------------------------------------- public メンバ ----------------------------------------


	// --------------------------------- private / protected メンバ -------------------------------

	/**
	 * タイトル付きのエリアを表示する処理。内容物を表示するデリゲータを指定する。
	 * 必要であればキーワードトグルでの表示非表示を行うこともできる
	 */
	protected void showPropArea(
		string title,
		Action drawPropProc,
		MtlProps mtlProps = null,
		string toggleName = null
	) {
		using (new EditorGUILayout.VerticalScope("OL Box")) {

			// タイトル部分を表示
			bool isDrawBody;
			using (new EditorGUILayout.VerticalScope("GameViewBackground")) {
				using (new EditorGUILayout.HorizontalScope()) {
					EditorGUILayout.Space(2, false);

					if ( string.IsNullOrEmpty(toggleName) ) {
						// キーワードトグルが不要な場合はそのまま表示する
						EditorGUILayout.LabelField(title);
						isDrawBody = true;

					} else {
						// キーワードトグルが必要な場合は、トグル付きで表示する
						var tglProp = mtlProps[toggleName];
						var value = 0.5f < tglProp.floatValue;

						using (new MixedValueScope(tglProp))
						using (var cc = new EditorGUI.ChangeCheckScope()) {
							EditorGUILayout.ToggleLeft(title, value);
							if ( cc.changed ) {
								value = !value;
								tglProp.floatValue = value ? 1 : 0;
								mtlProps.setKeywordEnable(
									value,
									toggleName.ToUpper()+"_ON"
								);
							}
						}

						isDrawBody = value;
					}
				}
				EditorGUILayout.Space(4, false);
			}

			// 本体部分を表示
			if (isDrawBody) {
				++EditorGUI.indentLevel;
				drawPropProc();
				--EditorGUI.indentLevel;
				EditorGUILayout.Space(2, false);
			}
		}
	}

	// RGB/Aごとに、乗算/加算の係数を指定できるカスタム合成方法パラメータの表示を行う
	protected void showBlendColorField(string propName, string dispName, MtlProps mtlProps) {

		var valProp = mtlProps[propName];
		var modeProp = mtlProps[propName + "_MODE"];

		var val = (float4)valProp.vectorValue;
		var mode = int2(float4(modeProp.vectorValue).xy);

		// タイトルを表示
		EditorGUILayout.LabelField(dispName);

		// 中身を表示する処理
		static (int mode, float2 value) drawOneElem(
			string name,
			(int mode, float2 value) src
		) {
			var dst = src;

			// モード変更ドロップダウン
			if (dst.mode<0 || 3<dst.mode) dst.mode = 4;
			dst.mode = EditorGUILayout.Popup(
				name,
				dst.mode,
				new [] {"影響なし", "乗算", "加算", "減算", "カスタム"}
			);
			if (dst.mode<0 || 3<dst.mode) dst.mode = -1;

			// 各モードごとの表示
			++EditorGUI.indentLevel;
			switch (dst.mode) {
			case 0:		// 影響なし
				dst.value = 0;
				break;
			case 1:		// 乗算
				dst.value = float2(
					EditorGUILayout.Slider("影響度", dst.value.x, 0, 1),
					0
				);
				break;
			case 2:		// 加算
				dst.value = float2(
					0,
					EditorGUILayout.Slider("影響度", dst.value.y, 0, 1)
				);
				break;
			case 3:		// 減算
				dst.value = float2(
					0,
					-EditorGUILayout.Slider("影響度", -dst.value.y, 0, 1)
				);
				break;
			default:	// カスタム
				dst.value = float2(
					EditorGUILayout.FloatField("乗算係数", dst.value.x),
					EditorGUILayout.FloatField("加算係数", dst.value.y)
				);
				break;
			}
			--EditorGUI.indentLevel;

			return dst;
		};

		// 中身を表示
		++EditorGUI.indentLevel;
		using (new MixedValueScope(valProp))
		using (var cc = new EditorGUI.ChangeCheckScope())
		{
			var retRGB = drawOneElem( "RGB", (mode.x, val.xz) );
			var retA   = drawOneElem( "A",   (mode.y, val.yw) );

			if ( cc.changed ) {
				valProp.vectorValue = float4(
					retRGB.value.x,
					retA.value.x,
					retRGB.value.y,
					retA.value.y
				);
				modeProp.vectorValue = float4(
					retRGB.mode,
					retA.mode,
					0,
					0
				);
			}
		}
		--EditorGUI.indentLevel;
	}


	/** RenderState系のプロパティエリアを表示する */
	protected void showRenderStateArea(
		MtlProps mtlProps,
		bool showBlendMode = true,
		bool showZWrite = true,
		bool showZTest = true,
		bool showCull = true,
		bool showUseFog = true,
		bool renderQueue = true
	) {
		showPropArea(
			"RenderState / その他",
			() => {

				if (showBlendMode) {

					// BlendModeの各種プロパティ
					var propBlendOp = mtlProps["_BlendOp"];
					var propSrcBlend = mtlProps["_SrcBlend"];
					var propDstBlend = mtlProps["_DstBlend"];
					var propIsPca = mtlProps["_IsPreCombineAlpha"];
					var propPcaPivot = mtlProps["_PreCombineAlphaPivot"];

					var blendOp = (BlendOp)(int)propBlendOp.floatValue;
					var srcBlend = (BlendMode)(int)propSrcBlend.floatValue;
					var dstBlend = (BlendMode)(int)propDstBlend.floatValue;
					var isPca = 0.5f < propIsPca.floatValue;
					var pcaPivot = propPcaPivot.floatValue;

					{// BlendModeのプリセット用コンボボックスを表示
						var curBMS = BlendModeSet.find(blendOp, srcBlend, dstBlend, isPca, pcaPivot);
						var curBmsIdx = curBMS?.presetIdx ?? BlendModeSet.Presets.Length;

						var bmsNames = BlendModeSet.Presets.Select(i=>i.name);
						if (curBMS==null) bmsNames = bmsNames.Concat(new []{"カスタム"});

						using (new MixedValueScope(propBlendOp, propSrcBlend, propDstBlend))
						using (var cc = new EditorGUI.ChangeCheckScope())
						{
							var newBmsIdx = EditorGUILayout.Popup(curBmsIdx, bmsNames.ToArray());

							if (cc.changed && newBmsIdx!=BlendModeSet.Presets.Length) {
								var val = BlendModeSet.Presets[newBmsIdx];
								propBlendOp.floatValue = (int)val.blendOp;
								propSrcBlend.floatValue = (int)val.srcBlend;
								propDstBlend.floatValue = (int)val.dstBlend;
								propIsPca.floatValue = val.isPreCombineAlpha ? 1 : 0;
								propPcaPivot.floatValue = val.preCombineAlphaPivot;
							}
						}
					}

					// BlendModeの各種設定をそのまま表示
					mtlProps.draw_Prop("_BlendOp");
					mtlProps.draw_Prop("_SrcBlend");
					mtlProps.draw_Prop("_DstBlend");

					// α事前合成用のパラメータを表示
					using (new EditorGUILayout.HorizontalScope()) {

						using (new MixedValueScope(propBlendOp, propSrcBlend, propDstBlend))
						using (var cc = new EditorGUI.ChangeCheckScope())
						{
							isPca = EditorGUILayout.Toggle(isPca, GUILayout.Width(30));
							if ( cc.changed ) {
								propIsPca.floatValue = isPca ? 1 : 0;
							}
						}

						var lastIndentLv = EditorGUI.indentLevel;
						EditorGUI.indentLevel = 0;

						mtlProps.draw_Prop("_PreCombineAlphaPivot", "α値事前合成先");

						EditorGUI.indentLevel = lastIndentLv;
					}
				}

				if ( showBlendMode && (showZWrite||showZTest||showCull) )
					EditorGUILayout.Space();

				// BlendMode以外の設定を表示
				if (showZWrite) mtlProps.draw_Prop("_ZWrite");
				if (showZTest) mtlProps.draw_Prop("_ZTest");
				if (showCull) mtlProps.draw_Prop("_Cull");
				if (renderQueue) mtlProps.editor.RenderQueueField();

				// FOGの設定を表示
				if (showUseFog) {
					EditorGUILayout.Space();
					mtlProps.draw_Prop("_UseFog");
				}
			}
		);
	}

	/** ステンシルパラメータ系のプロパティエリアを表示する */
	protected void showStencilArea( MtlProps mtlProps ) {

		// ステンシルマスクを表示する処理
		static int drawStencilMask( string name, int mask ) {
			using ( new EditorGUILayout.HorizontalScope() ) {
				
				EditorGUILayout.LabelField(name, GUILayout.Width(100), GUILayout.Height(20));

				var lastIndentLv = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				{
					EditorGUILayout.Space();
					for (int i=0; i<8; ++i) {
						int width = i==3 ? 15 : 12;
						var bitIdx = 7 - i;
						var bit = EditorGUILayout.Toggle( ((mask>>bitIdx)&1)!=0, GUILayout.Width(width) );
						if (bit)	mask = mask | (1<<bitIdx);
						else		mask = ( mask & ~(1<<bitIdx) )&0xff;
					}

					EditorGUILayout.Space(2, false);
					mask = EditorGUILayout.IntField( mask, GUILayout.Width(40) );
					mask = clamp(mask, 0, 255);
				}
				EditorGUI.indentLevel = lastIndentLv;

				return mask;
			}
		}
		static void drawStencilMaskByProp( string name, MaterialProperty prop ) {
			using (new MixedValueScope(prop))
			using (var cc = new EditorGUI.ChangeCheckScope())
			{
				var newMask = drawStencilMask(name, (int)prop.floatValue);

				if (cc.changed) {
					prop.floatValue = newMask;
				}
			}
		}

		showPropArea(
			"Stencil",
			() => {
				// 各種プロパティを取得
				var propRef = mtlProps["_StencilRef"];
				var propReadMask = mtlProps["_StencilReadMask"];
				var propWriteMask = mtlProps["_StencilWriteMask"];

				// 中身を表示
				drawStencilMaskByProp("Ref値", propRef);
				drawStencilMaskByProp("Read Mask", propReadMask);
				drawStencilMaskByProp("Write Mask", propWriteMask);
				EditorGUILayout.Space();
				mtlProps.draw_Prop("_StencilComp", "Comp");
				mtlProps.draw_Prop("_StencilPass", "Pass");
				mtlProps.draw_Prop("_StencilFail", "Fail");
				mtlProps.draw_Prop("_StencilZFail", "ZFail");
			}
		);
	}



	// --------------------------------------------------------------------------------------------
}

}
