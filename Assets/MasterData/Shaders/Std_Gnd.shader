Shader "Pote/Std_Gnd"
{
	Properties
	{
		[MainTexture] _BaseMap("Albedo", 2D) = "white" {}
		_AddColor("AddColor", Color) = (0,0,0,0)
		[HDR] _BaseColor("Color", Color) = (1,1,1,1)
		_Ramp("Ramp", 2D) = "white" {}
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		// これが有効な場合は、スケーリングが9スライス的に頂点位置に作用する
		[Toggle] _Use9Slice("Use 9-Slice", Float) = 0

		// これが有効な場合は、テクスチャリングをTriPlanarにする
		[Toggle] _UseTriPlanar("Use TriPlanar", Float) = 0

		[Space]
		_GreenRate("GreenRate", Range(0.0, 1.0)) = 0
		_GreenColor("GreenColor", Color) = (1,1,1,1)

		// ブレンドステート系パラメータ
		[Space]
		[Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("Blend Op", Float) = 0		// Add
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Blend Src", Float) = 1	// One
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Blend Dst", Float) = 0	// Zero
		[Toggle] _ZWrite("Z Write", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Z Test", Float) = 4	// LEqual
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2       		 // Back

		[Space]
		_ReceiveShadowRate("影を受ける割合", Range(0, 1)) = 1
		_AORate("AOを使用する割合", Range(0, 1)) = 1

		// アウトラインのパラメータ
		[Space]
		[Toggle] _UseCustomOutlineColor("ｱｳﾄﾗｲﾝにｶｽﾀﾑ色を使用", Float) = 0
		_CustomOutlineColor("ｱｳﾄﾗｲﾝのｶｽﾀﾑ色", Color) = (1,1,1,1)
//		_OutlineWidthRate("ｱｳﾄﾗｲﾝの幅ｽｹｰﾙ", Float) = 1
	}

	SubShader
	{
		Tags{
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
			"UniversalMaterialType" = "Lit"
			"IgnoreProjector" = "True"
		}

		HLSLINCLUDE
			#include "Core/Core8.hlsl"
			#define USE_ALPHA_CLIP
			#define USE_ADD_COLOR

			#pragma multi_compile_local _ _USETRIPLANAR_ON

			TEXTURE2D(_BaseMap);	SAMPLER(sampler_BaseMap);
			TEXTURE2D(_Ramp);		SAMPLER(sampler_Ramp);
			CBUFFER_START(UnityPerMaterial)
			float4 _BaseMap_ST;
			half4 _BaseColor;
			half4 _AddColor;
			half _Cutoff;
			half _GreenRate;
			half4 _GreenColor;
			half _ReceiveShadowRate;
			half _AORate;
			half _UseCustomOutlineColor;
			half4 _CustomOutlineColor;
			CBUFFER_END

			// 影を使用するか否か
			#define USE_SHADOW_RATE _ReceiveShadowRate

			// カスタムの頂点位置変換処理
			#define GET_VERTEX_POSITION_INPUTS(attr) GetCustomVertexPositionInputs(attr.positionOS.xyz)
			VertexPositionInputs GetCustomVertexPositionInputs(float3 positionOS)
			{
				VertexPositionInputs input;
			#if _USE9SLICE_ON
				float4x4 o2w = GetObjectToWorldMatrix();
				float l0 = length(o2w._m00_m10_m20);
				float l1 = length(o2w._m01_m11_m21);
				float l2 = length(o2w._m02_m12_m22);
				o2w._m00_m10_m20 /= l0;
				o2w._m01_m11_m21 /= l1;
				o2w._m02_m12_m22 /= l2;
				float4x4 shiftMtx = float4x4(1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1);
				shiftMtx._m03_m13_m23 = (float3(l0,l1,l2)-1) * (step(0,positionOS)*2-1);
				o2w = mul(o2w, shiftMtx);

				input.positionWS = mul(o2w, float4(positionOS, 1.0)).xyz;
			#else
				input.positionWS = TransformObjectToWorld(positionOS);
			#endif
				input.positionVS = TransformWorldToView(input.positionWS);
				input.positionCS = TransformWorldToHClip(input.positionWS);

				float4 ndc = input.positionCS * 0.5f;
				input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
				input.positionNDC.zw = input.positionCS.zw;

				return input;
			}
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
			Cull[_Cull]

			HLSLPROGRAM
			#pragma only_renderers gles gles3 glcore d3d11
			#pragma target 2.0

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing

			// -------------------------------------
			// Material Keywords
			#pragma multi_compile_local _ _USE9SLICE_ON
			#define ADDITIONAL_ATTRIBUTES half4 color : COLOR;
			#define ADDITIONAL_VARYINGS half4 color : TEXCOORD6;

			// -------------------------------------
			// Universal Pipeline keywords
			#define _MAIN_LIGHT_SHADOWS;
			#define _MAIN_LIGHT_SHADOWS_CASCADE;
			#define _SHADOWS_SOFT
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

			// -------------------------------------
			// Unity defined keywords
			#pragma multi_compile_fog


			#pragma vertex vert
			#pragma fragment LitPassFragment


			#include "Core/LitForward.hlsl"

			Varyings vert(Attributes input) {
				Varyings output = LitPassVertex(input);

				output.color = input.color;
				return output;
			}

			// Used in Standard (Physically Based) shader
			half4 LitPassFragment(Varyings input) : SV_Target
			{
				FragMainData fmd = LitPassFragment_Main(input);

				fmd.col.rgb = lerp(
					fmd.col.rgb,
					fmd.col.rgb * _GreenColor.rgb,
					_GreenRate * pow(saturate(fmd.inp.normalWS.y),3)
				);

			#if defined(_SCREEN_SPACE_OCCLUSION)
				AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(fmd.inp.normalizedScreenSpaceUV);
				fmd.col.rgb *= lerp(
					1,
					aoFactor.directAmbientOcclusion * aoFactor.indirectAmbientOcclusion,
					_AORate
				);
			#endif
				fmd.col *= input.color;

				fmd.col.rgb = MixFog(fmd.col.rgb, fmd.inp.fogCoord);
				return fmd.col;
			}

			ENDHLSL
		}

		Pass
		{
			Name "Outline"
			Tags{"LightMode" = "Outline"}

			ZWrite Off
			ZTest LEqual
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM
			#pragma only_renderers gles gles3 glcore d3d11
			#pragma target 2.0

			#pragma multi_compile_fog
			#pragma multi_compile_local _ _USE9SLICE_ON

			#pragma vertex OutlinePassVertex
			#pragma fragment OutlinePassFragment
			#define OUTLINE_COL_RATE 0.2
			#define OUTLINE_A_RATE 0.8
			#define OUTLINE_CALC_COL_BODY(albedo) \
				lerp(\
					half4(albedo.rgb*OUTLINE_COL_RATE, albedo.a*OUTLINE_A_RATE),\
					_CustomOutlineColor,\
					_UseCustomOutlineColor\
				)
			#include "Core/Outline.hlsl"
			ENDHLSL
		}

		Pass
		{
			Name "ShadowCaster"
			Tags{"LightMode" = "ShadowCaster"}

			ZWrite On
			ZTest LEqual
			ColorMask 0
			Cull[_Cull]

			HLSLPROGRAM
			#pragma only_renderers gles gles3 glcore d3d11
			#pragma target 2.0

			#pragma multi_compile_instancing
			#pragma multi_compile_local _ _USE9SLICE_ON

			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment
			#include "Core/ShadowCaster.hlsl"
			ENDHLSL
		}

		Pass
		{
			Name "DepthOnly"
			Tags{"LightMode" = "DepthOnly"}

			ZWrite On
			ColorMask 0
			Cull[_Cull]

			HLSLPROGRAM
			#pragma only_renderers gles gles3 glcore d3d11
			#pragma target 2.0

			#pragma multi_compile_instancing
			#pragma multi_compile_local _ _USE9SLICE_ON

			// -------------------------------------
			// Material Keywords
			#pragma vertex DepthOnlyVertex
			#pragma fragment DepthOnlyFragment
			#include "Core/DepthOnly.hlsl"
			ENDHLSL
		}

		// This pass is used when drawing to a _CameraNormalsTexture texture
		Pass
		{
			Name "DepthNormals"
			Tags{"LightMode" = "DepthNormals"}

			ZWrite On
			Cull[_Cull]

			HLSLPROGRAM
			#pragma only_renderers gles gles3 glcore d3d11
			#pragma target 2.0

			// -------------------------------------
			// Material Keywords
			#pragma multi_compile_instancing
			#pragma multi_compile_local _ _USE9SLICE_ON

			#pragma vertex DepthNormalsVertex
			#pragma fragment DepthNormalsFragment
			#include "Core/DepthNormals.hlsl"
			ENDHLSL
		}
	}

	FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
