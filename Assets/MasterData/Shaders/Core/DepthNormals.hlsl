//
// DepthNormalsパスで使用する共通コード
//


#include "OverridableVertProcMacro.hlsl"



struct Attributes
{
	float4 positionOS     : POSITION;
	float3 normalOS       : NORMAL;
#if !defined(_USETRIPLANAR_ON) && defined(USE_ALPHA_CLIP) || defined(USE_ALPHA_DITHER)
	float2 texcoord     : TEXCOORD0;
#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
	ADDITIONAL_ATTRIBUTES
};

struct Varyings
{
	float3 positionWS		: TEXCOORD0;
#if defined(USE_ALPHA_CLIP) || defined(USE_ALPHA_DITHER)
	#if defined(_USETRIPLANAR_ON)
	#else
		float2 uv               : TEXCOORD01;
	#endif
#endif

	float4 positionCS   : SV_POSITION;
	float3 normalWS     : TEXCOORD2;
	ADDITIONAL_VARYINGS
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings DepthNormalsVertex(Attributes input)
{
	Varyings output = (Varyings)0;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);

	VertexPositionInputs vertexInput = GET_VERTEX_POSITION_INPUTS(input);

	output.positionWS = vertexInput.positionWS;
#if defined(USE_ALPHA_CLIP) || defined(USE_ALPHA_DITHER)
	#if defined(_USETRIPLANAR_ON)
	#else
		output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
	#endif
#endif

//	VertexNormalInputs normalInput = GetVertexNormalInputs(input.normal, input.tangentOS);
//	output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
	float3 normalWS = GET_WORLD_NORMAL(input);
	output.normalWS = normalWS;
    output.positionCS = vertexInput.positionCS;

	return output;
}

float4 calcFragmentReturnOnly(Varyings input) {
	return float4(PackNormalOctRectEncode(TransformWorldToViewDir(input.normalWS, true)), 0.0, 0.0);
}

float4 DepthNormalsFragment(Varyings input) : SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(input);

#if defined(USE_ALPHA_CLIP)
	// αを指定値でクリップする
	CLIP_BY_CUTOFF(input);
#elif defined(USE_ALPHA_DITHER)
	// ディザでクリップする
	CLIP_BY_DITHER(input);
#endif

//    Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
	return calcFragmentReturnOnly(input);
}
