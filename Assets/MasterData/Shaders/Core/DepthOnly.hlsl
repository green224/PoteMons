//
// DepthOnlyパスで使用する共通コード
//


#include "OverridableVertProcMacro.hlsl"



struct Attributes
{
    float4 positionOS	: POSITION;
#if defined(_USETRIPLANAR_ON)
	float3 normalOS     : NORMAL;
#else
    float2 texcoord		: TEXCOORD0;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
	ADDITIONAL_ATTRIBUTES
};

struct Varyings
{
	float3 positionWS		: TEXCOORD0;
#if defined(USE_ALPHA_CLIP) || defined(USE_ALPHA_DITHER)
	#if defined(_USETRIPLANAR_ON)
		float3 normalWS			: TEXCOORD1;
	#else
		float2 uv               : TEXCOORD1;
	#endif
#endif

    float4 positionCS   : SV_POSITION;
	ADDITIONAL_VARYINGS
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings DepthOnlyVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);

	VertexPositionInputs vertexInput = GET_VERTEX_POSITION_INPUTS(input);

	output.positionWS = vertexInput.positionWS;
#if defined(USE_ALPHA_CLIP) || defined(USE_ALPHA_DITHER)
	#if defined(_USETRIPLANAR_ON)
		float3 normalWS = GET_WORLD_NORMAL(input);
		output.normalWS = normalWS;
	#else
		output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
	#endif
#endif

    output.positionCS = vertexInput.positionCS;
    return output;
}

half4 DepthOnlyFragment(Varyings input) : SV_TARGET
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
    return 0;
}


