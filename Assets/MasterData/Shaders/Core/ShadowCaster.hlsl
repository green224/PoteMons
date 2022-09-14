//
// ShadowCasterパスで使用する共通コード
//


#include "OverridableVertProcMacro.hlsl"


#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

float3 _LightDirection;

struct Attributes
{
	float4 positionOS   : POSITION;
	float3 normalOS     : NORMAL;
#if !defined(_USETRIPLANAR_ON) && defined(USE_ALPHA_CLIP) || defined(USE_ALPHA_DITHER)
	float2 texcoord     : TEXCOORD0;
#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
	ADDITIONAL_ATTRIBUTES
};

struct Varyings
{
#if defined(USE_ALPHA_CLIP) || defined(USE_ALPHA_DITHER)
	#if defined(_USETRIPLANAR_ON)
		float3 positionWS		: TEXCOORD0;
		float3 normalWS			: TEXCOORD1;
	#else
		float2 uv               : TEXCOORD0;
	#endif
#endif

	float4 positionCS   : SV_POSITION;
	ADDITIONAL_VARYINGS
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings ShadowPassVertex(Attributes input)
{
	Varyings output;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);

	VertexPositionInputs vertexInput = GET_VERTEX_POSITION_INPUTS(input);

	// シャドウの設定からゆがめたPositionCSを計算する
	float3 positionWS = vertexInput.positionWS;
	float3 normalWS = GET_WORLD_NORMAL(input);
	float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
#if UNITY_REVERSED_Z
	positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
	positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif


#if defined(USE_ALPHA_CLIP) || defined(USE_ALPHA_DITHER)
	#if defined(_USETRIPLANAR_ON)
		output.positionWS = vertexInput.positionWS;
		output.normalWS = normalWS;
	#else
		output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
	#endif
#endif


	output.positionCS = positionCS;
	return output;
}

half4 ShadowPassFragment(Varyings input) : SV_TARGET
{
#if defined(USE_ALPHA_CLIP)
	// αを指定値でクリップする
	UNITY_SETUP_INSTANCE_ID(input);
	CLIP_BY_CUTOFF(input);
#elif defined(USE_ALPHA_DITHER)
	// ディザでクリップする
	UNITY_SETUP_INSTANCE_ID(input);
	CLIP_BY_DITHER(input);
#endif

//    Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
	return 0;
}

