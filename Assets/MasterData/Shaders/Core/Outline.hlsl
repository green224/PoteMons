//
// Outlineパスで使用する共通コード
//


#include "OverridableVertProcMacro.hlsl"

// アウトライン色をAlbedo色に何をかけて生成するかのパラメータ
#if !defined(OUTLINE_COL_RATE)
	#define OUTLINE_COL_RATE 0.2
#endif
// アウトライン色のアルファ値
#if !defined(OUTLINE_A_RATE)
	#define OUTLINE_A_RATE 0.5
#endif
// アウトライン色の計算式本体
#if !defined(OUTLINE_CALC_COL_BODY)
	// アウトラインのカラーは適当な値で加工する
	#define OUTLINE_CALC_COL_BODY(albedo) \
		half4(albedo.rgb*OUTLINE_COL_RATE, albedo.a*OUTLINE_A_RATE)
#endif
// アウトラインのZオフセット値
#if !defined(OUTLINE_Z_OFS)
	#define OUTLINE_Z_OFS 0.03
#endif


struct Attributes
{
	float4 positionOS   : POSITION;
	float3 normalOS     : NORMAL;
//	float4 tangentOS    : TANGENT;
#if !defined(_USETRIPLANAR_ON)
	float2 texcoord     : TEXCOORD0;
#endif
	ADDITIONAL_ATTRIBUTES
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
	float3 positionWS		: TEXCOORD0;
#if defined(_USETRIPLANAR_ON)
	float3 normalWS			: TEXCOORD1;
#else
	float2 uv               : TEXCOORD1;
#endif
	half fogFactor			: TEXCOORD6;
	float4 positionCS       : SV_POSITION;
	ADDITIONAL_VARYINGS
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

void InitializeInputData(Varyings input, out InputData inputData)
{
	inputData = (InputData)0;
	inputData.fogCoord = input.fogFactor;
	inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
}

Varyings OutlinePassVertex(Attributes input)
{
	Varyings output = (Varyings)0;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);

	VertexPositionInputs vertexInput = GET_VERTEX_POSITION_INPUTS(input);

	// normalWS and tangentWS already normalize.
	// this is required to avoid skewing the direction during interpolation
	// also required for per-vertex lighting and SH evaluation
//	VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
	float3 normalWS = GET_WORLD_NORMAL(input);

	half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);

	output.positionWS = vertexInput.positionWS;
#if defined(_USETRIPLANAR_ON)
	output.normalWS = normalWS;
#else
	output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
#endif

	// スクリーン幅などのパラメータから、押し出し量を計算
	float3 wPos = vertexInput.positionWS;
	// tan(FOV/2) : https://answers.unity.com/questions/770838/how-can-i-extract-the-fov-information-from-the-pro.html
	// これは環境によってはマイナス値になるので、absをかけておく
	float tanHFov = abs(1 / UNITY_MATRIX_P._m11);
	float c2oLen = -dot( UNITY_MATRIX_V._m20_m21_m22_m23, float4(wPos,1) );
	float w = _ScreenParams.w - 1;		// 1/スクリーン高さ
	float push = w*2 * tanHFov*c2oLen;
//push *= 1.6;

	// 押し出し結果を反映
	wPos += normalWS * push;
	wPos += OUTLINE_Z_OFS * normalize(wPos-_WorldSpaceCameraPos);	// Zオフセット
	output.positionCS = mul( UNITY_MATRIX_VP, float4(wPos,1) );


	output.fogFactor = computeFogFactor(vertexInput.positionWS);
//				output.positionCS = vertexInput.positionCS;

	return output;
}

// Used in Standard (Physically Based) shader
half4 OutlinePassFragment(Varyings input) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(input);

	half4 albedo = SAMPLE_ALBEDO_APLHA_BY_TRIPLANAR_OR_NOT(input, _BaseMap, sampler_BaseMap);
	albedo = albedo*_BaseColor;

	InputData inputData;
	InitializeInputData(input, inputData);


#if defined(USE_ALPHA_CLIP)
	// αを指定値でクリップする
	clip(albedo.a - _Cutoff);
#elif defined(USE_ALPHA_DITHER)
	// ディザでクリップする
	clipByDither(inputData.normalizedScreenSpaceUV, albedo.a);
#endif


	// アウトラインのカラーを計算
	half4 color = OUTLINE_CALC_COL_BODY(albedo);

	// Fogをかけつつ、Fog強度でアルファを減衰させる
#if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
	real fogIntensity = ComputeFogIntensity(inputData.fogCoord);
	color.rgb = lerp(unity_FogColor.rgb, color.rgb, fogIntensity);
	color.a = lerp(0, color.a, pow(fogIntensity,4));
#endif

	return color;
}
