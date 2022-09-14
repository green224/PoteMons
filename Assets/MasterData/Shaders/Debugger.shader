Shader "Pote/Debugger"
{
	Properties
	{
		[KeywordEnum(VColRGB, VColA, Nml, UV)] _Mode("Mode", int) = 0

		// ブレンドステート系パラメータ
		[Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("Blend Op", Float) = 0		// Add
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Blend Src", Float) = 1	// One
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Blend Dst", Float) = 0	// Zero
		[Toggle] _ZWrite("Z Write", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Z Test", Float) = 4	// LEqual
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2       		 // Back
		[Toggle] _ReceiveShadows("Receive Shadows", Float) = 1.0
	}

	SubShader
	{
		Tags{
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
			"UniversalMaterialType" = "Lit"
			"IgnoreProjector" = "True"
		}

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

			#include "Core/Core8.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #pragma multi_compile_local _MODE_VCOLRGB _MODE_VCOLA _MODE_NML _MODE_UV
			#define _MAIN_LIGHT_SHADOWS;
			#define _MAIN_LIGHT_SHADOWS_CASCADE;
			#define _SHADOWS_SOFT
			#define _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_fog

			#pragma vertex vert
			#pragma fragment frag

			struct Attributes
			{
				float4 positionOS	: POSITION;
				float3 normalOS		: NORMAL;
				float4 tangentOS	: TANGENT;
				float2 texcoord		: TEXCOORD0;
				half4 color			: COLOR;
			};

			struct Varyings
			{
				float2 uv				: TEXCOORD0;
				half4 color				: TEXCOORD1;
				float3 positionWS		: TEXCOORD2;
				float3 normalWS			: TEXCOORD3;
				float3 viewDirWS		: TEXCOORD5;
				half fogFactor			: TEXCOORD6;
				float4 positionCS		: SV_POSITION;
			};

			Varyings vert(Attributes input)
			{
				Varyings output = (Varyings)0;

				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
				VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

				half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);

				output.uv = input.texcoord;
				output.color = input.color;
				output.positionWS = vertexInput.positionWS;
				output.normalWS = normalInput.normalWS;
				output.viewDirWS = viewDirWS;
				output.fogFactor = computeFogFactor(vertexInput.positionWS);
				output.positionCS = vertexInput.positionCS;

				return output;
			}

			half4 frag(Varyings input) : SV_Target
			{
				half3 ret=0;

			#if _MODE_VCOLRGB
				ret = input.color.rgb;
			#elif _MODE_VCOLA
				ret = input.color.aaa;
			#elif _MODE_NML
				ret = NormalizeNormalPerPixel(input.normalWS)*.5 + .5;
			#elif _MODE_UV
				ret = half3(saturate(input.uv),0);
			#endif

				return half4(ret,1);
			}

			ENDHLSL
		}

	}

	FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
