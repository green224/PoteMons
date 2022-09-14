Shader "Pote/Std_Chara"
{
	Properties
	{
		[MainTexture] _BaseMap("Albedo", 2D) = "white" {}
		[HDR] _BaseColor("Color", Color) = (1,1,1,1)
		_Ramp("Ramp", 2D) = "white" {}

		_LighingEffect("LighingEffect", Range(0,1)) = 1		// ライティングの影響をどれだけ受けるか。0だとライティング無効になる
		_ShadowBottomColRate("影部分の明度", Range(0,1)) = 0.33		// シャドウ範囲内の色を、どのくらいの明るさとして計算するか
		_OutlineEffect("OutlineEffect", Range(0,1)) = 1		// アウトラインの透明度。0だとアウトラインなしになる

		// ブレンドステート系パラメータ
		[Space]
		[Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("Blend Op", Float) = 0		// Add
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Blend Src", Float) = 1	// One
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Blend Dst", Float) = 0	// Zero
		[Toggle] _ZWrite("Z Write", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Z Test", Float) = 4	// LEqual
//		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2       		 // Back
		[Toggle] _ReceiveShadows("Receive Shadows", Float) = 1.0
		[HideInInspector] _CullMain("Cull Main", Float) = 2				// Back
		[HideInInspector] _CullOutline("Cull Outline", Float) = 1		// Front
	}

	SubShader
	{
		Tags{
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
			"UniversalMaterialType" = "Lit"
			"IgnoreProjector" = "True"
			"Queue" = "Geometry+300"
		}

		HLSLINCLUDE
			#include "Core/Core8.hlsl"

			#define USE_ALPHA_DITHER

			TEXTURE2D(_BaseMap);    SAMPLER(sampler_BaseMap);
			TEXTURE2D(_Ramp);		SAMPLER(sampler_Ramp);
			CBUFFER_START(UnityPerMaterial)
			float4 _BaseMap_ST;
			half4 _BaseColor;
			half _LighingEffect;
			half _ShadowBottomColRate;
			half _OutlineEffect;
			half4 _WallBackMaskColor;
			half _WallBackMaskDitherAlpha;
			CBUFFER_END
		ENDHLSL

		// ------------------------------------------------------------------
		//  Forward pass. Shades all light in a single pass. GI + emission + Fog
		Pass
		{
			// Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
			// no LightMode tag are also rendered by Universal Render Pipeline
			Name "ForwardLit"
			Tags{"LightMode" = "UniversalForward"}

			Blend[_SrcBlend][_DstBlend]
			BlendOp[_BlendOp]
			ZWrite[_ZWrite]
			ZTest[_ZTest]
			Cull[_CullMain]

			HLSLPROGRAM
			#pragma only_renderers gles gles3 glcore d3d11
			#pragma target 2.0

			// -------------------------------------
			// Material Keywords
//			#define _RECEIVE_SHADOWS_OFF

			// -------------------------------------
			// Universal Pipeline keywords
			#define _MAIN_LIGHT_SHADOWS;
			#define _MAIN_LIGHT_SHADOWS_CASCADE;
//			#define _ADDITIONAL_LIGHTS
			#define _SHADOWS_SOFT

			// -------------------------------------
			// Unity defined keywords
			#pragma multi_compile_fog

			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment


			#define COMBINE_SHADOW_TO_NDOTL(NdotL,shadowAtn) \
				lerp( 1, min(\
					NdotL/2+0.5,\
					lerp(_ShadowBottomColRate,1,shadowAtn)\
				), _LighingEffect)
			#include "Core/LitForward.hlsl"


			half getCustomLightRate(Varyings input, InputData inputData) {
				Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, half4(1,1,1,1));

				half shadowAtn = mainLight.distanceAttenuation * mainLight.shadowAttenuation;

				float NdotL0 = dot(inputData.normalWS, mainLight.direction);

				return COMBINE_SHADOW_TO_NDOTL(NdotL0, shadowAtn);
			}

			half3 getCustomLightRamp(Varyings input, InputData inputData) {
				half lightRate = getCustomLightRate(input, inputData);
				half4 ramp = SampleAlbedoAlpha(float2(lightRate,0), TEXTURE2D_ARGS(_Ramp, sampler_Ramp));
				return ramp.rgb;
			}

			half4 LitPassFragment(Varyings input) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);

				InputData inputData;
				InitializeInputData(input, inputData);

				// albedoを取得
				half4 albedo = SAMPLE_ALBEDO_APLHA_BY_TRIPLANAR_OR_NOT(input, _BaseMap, sampler_BaseMap);
				albedo = albedo*_BaseColor;

				// αをディザでクリップする
				clipByDither(inputData.normalizedScreenSpaceUV, albedo.a);

				half4 col = half4(albedo.rgb * getCustomLightRamp(input, inputData), albedo.a);

				// リムライトの計算
				float rimPow = dot(
					inputData.normalWS,
					// リムライトの方向は適当にずらす
					normalize(inputData.viewDirectionWS + half3(0.1,-0.1,0.1))
				);
				rimPow = pow(1.2-abs(rimPow), 5);		// 適当なカーブに加工
				col.rgb = lerp(col.rgb, albedo.rgb, saturate(rimPow)*0.3);	// 適当な係数で合成

				// フォグを適応
				col.rgb = MixFog(col.rgb, inputData.fogCoord);
				return col;
			}

			ENDHLSL
		}

		Pass
		{
			Name "Outline"
			Tags{"LightMode" = "Outline"}

			ZWrite Off
			ZTest LEqual
			Cull[_CullOutline]
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM
			#pragma only_renderers gles gles3 glcore d3d11
			#pragma target 2.0

			#pragma multi_compile_fog

			#pragma vertex OutlinePassVertex
			#pragma fragment frag
			#define OUTLINE_A_RATE (0.5*_OutlineEffect)
			#include "Core/Outline.hlsl"

			half4 frag(Varyings input) : SV_Target {
				return OutlinePassFragment(input);
			}
			ENDHLSL
		}

//		Pass
//		{
//			Name "ShadowCaster"
//			Tags{"LightMode" = "ShadowCaster"}
//
//			ZWrite On
//			ZTest LEqual
//			ColorMask 0
//			Cull[_CullMain]
//
//			HLSLPROGRAM
//			#pragma only_renderers gles gles3 glcore d3d11
//			#pragma target 2.0
//
//			#pragma vertex ShadowPassVertex
//			#pragma fragment ShadowPassFragment
//			#include "Core/ShadowCaster.hlsl"
//			ENDHLSL
//		}

		Pass
		{
			Name "DepthOnly"
			Tags{"LightMode" = "DepthOnly"}

			ZWrite On
			ColorMask 0
			Cull[_CullMain]

			HLSLPROGRAM
			#pragma only_renderers gles gles3 glcore d3d11
			#pragma target 2.0

			#pragma vertex DepthOnlyVertex
			#pragma fragment frag
			#include "Core/DepthOnly.hlsl"

			half4 frag(Varyings input) : SV_Target {
				return DepthOnlyFragment(input);
			}
			ENDHLSL
		}

		// This pass is used when drawing to a _CameraNormalsTexture texture
		Pass
		{
			Name "DepthNormals"
			Tags{"LightMode" = "DepthNormals"}

			ZWrite On
			Cull[_CullMain]

			HLSLPROGRAM
			#pragma only_renderers gles gles3 glcore d3d11
			#pragma target 2.0

			#pragma vertex DepthNormalsVertex
			#pragma fragment frag
			#include "Core/DepthNormals.hlsl"

			half4 frag(Varyings input) : SV_Target {
				return DepthNormalsFragment(input);
			}
			ENDHLSL
		}
	}

	FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
