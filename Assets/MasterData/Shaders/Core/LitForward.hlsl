//
// ライティングをするメインパス用の共通コード
//

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#include "OverridableVertProcMacro.hlsl"

// Unityのバージョンアップに従って、これを有効にするとバグるようになったので、
// とりあえず無効化。あとでちゃんとUnity内シェーダを読んで、同じように実装を更新する事。
#undef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR


// 影を使用するか否か。
// 特に定義しない場合は常に使用する
#if !defined(USE_SHADOW_RATE)
	#define USE_SHADOW_RATE 1
#endif

// 影影響度とライトからの陰影値から、ライト適応度を計算する処理。
// 物によってここの計算を特殊化したい場合があるので、その場合はこのマクロを定義して上書きする
#if !defined(COMBINE_SHADOW_TO_NDOTL)
	#define COMBINE_SHADOW_TO_NDOTL(NdotL,shadowAtn) (min(NdotL, NdotL*shadowAtn)/2 + 0.5)
#endif



struct Attributes
{
	float4 positionOS	: POSITION;
	float3 normalOS		: NORMAL;
//	float4 tangentOS	: TANGENT;
#if !defined(_USETRIPLANAR_ON)
	float2 texcoord		: TEXCOORD0;
#endif
	ADDITIONAL_ATTRIBUTES
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
#if !defined(_USETRIPLANAR_ON)
	float2 uv				: TEXCOORD0;
#endif

	float3 positionWS		: TEXCOORD1;
	float3 normalWS			: TEXCOORD2;
	float3 viewDirWS		: TEXCOORD3;

	half fogFactor			: TEXCOORD4;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	float4 shadowCoord		: TEXCOORD5;
#endif

	float4 positionCS		: SV_POSITION;
	ADDITIONAL_VARYINGS
	UNITY_VERTEX_INPUT_INSTANCE_ID
};


/**
 * ピクセルシェーダ内で、入力情報から色々なデータを取得する処理。
 * 参考：com.unity.render-pipelines.universal@10.4.0\Shaders\LitForwardPass.hlsl
 */
void InitializeInputData(Varyings input, out InputData inputData) {
	inputData = (InputData)0;

	inputData.positionWS = input.positionWS;

	half3 viewDirWS = SafeNormalize(input.viewDirWS);
	inputData.normalWS = NormalizeNormalPerPixel(input.normalWS);
	inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
	inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
	inputData.shadowCoord = float4(0, 0, 0, 0);
#endif

	inputData.fogCoord = input.fogFactor;
	inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
}

Varyings LitPassVertex(Attributes input)
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
//#if defined(DOUBLE_SIDED)
//	half nmlSign = sign(dot(normalInput.normalWS, viewDirWS));
//	normalInput.normalWS *= nmlSign;
//#endif

	half3 vertexLight = VertexLighting(vertexInput.positionWS, normalWS);

#if !defined(_USETRIPLANAR_ON)
	output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
#endif

	// already normalized from normal transform to WS.
	output.normalWS = normalWS;
	output.viewDirWS = viewDirWS;

//	output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
	output.fogFactor = computeFogFactor(vertexInput.positionWS);

	output.positionWS = vertexInput.positionWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	output.shadowCoord = GetShadowCoord(vertexInput);
#endif

	output.positionCS = vertexInput.positionCS;

	return output;
}


/** LitPassFragment_Mainが返すデータ */
struct FragMainData {
	InputData inp;
	half4 albedo;
	half4 col;		// 計算結果のカラー
	half lightRate;
};

/** ライトの影響度を求める */
half getLightRate(InputData inputData) {
	Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, half4(1,1,1,1));

	half shadowAtn = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
	shadowAtn = lerp(1, shadowAtn, USE_SHADOW_RATE);

	half NdotL = dot(inputData.normalWS, mainLight.direction);
	return COMBINE_SHADOW_TO_NDOTL(NdotL, shadowAtn);
}

/** ライトの影響結果からランプテクスチャの値を求める */
half3 getLightRamp(half lightRate) {
	half4 ramp = SampleAlbedoAlpha(float2(lightRate,0), TEXTURE2D_ARGS(_Ramp, sampler_Ramp));
	return ramp.rgb;
}


/** ピクセルシェーダのメイン処理部分 */
FragMainData LitPassFragment_Main(inout Varyings input) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(input);

	InputData inputData;
	InitializeInputData(input, inputData);

	half4 albedo = SAMPLE_ALBEDO_APLHA_BY_TRIPLANAR_OR_NOT(input, _BaseMap, sampler_BaseMap);
#if defined(USE_ADD_COLOR)
	albedo += _AddColor;
#endif
	albedo = albedo*_BaseColor;


#if defined(USE_ALPHA_CLIP)
	// αを指定値でクリップする
	clip(albedo.a - _Cutoff);
#elif defined(USE_ALPHA_DITHER)
	// αをディザでクリップする
	clipByDither(inputData.normalizedScreenSpaceUV, albedo.a);
#endif


	half lightRate = getLightRate(inputData);
	half3 color = albedo.rgb * getLightRamp(lightRate);

	// 返り値を生成して返す
	FragMainData ret;
	ret.inp = inputData;
	ret.albedo = albedo;
	ret.col = half4(color, albedo.a);
	ret.lightRate = lightRate;
	return ret;
}
