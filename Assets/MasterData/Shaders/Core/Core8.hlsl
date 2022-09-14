//
// 共通で最初にインクルードしておくコード
//

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"

#include "Fog8.hlsl"


TEXTURE3D(_DitherMaskLOD);	SAMPLER(sampler_DitherMaskLOD);

half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap)) {
	return SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv);
}
half3 SampleNormal(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap)) {
	half4 n = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
	return UnpackNormal(n);
}

// TriPlanar用のUV変換処理
float2 transformTexTriplanar(float2 uv, float4 st) {
	return float2(
		st.x * uv.x + st.y * uv.y,
		st.z * uv.x + st.w * uv.y
	);
}

// TriPlanerでAlbedoAlphaをサンプリング
half4 SampleAlbedoAlphaByTriPlanar(
	float3 posW,
	float3 nmlW,
	float4 st,
	TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap)
) {
	half4 albedoX = SampleAlbedoAlpha(
		transformTexTriplanar(posW.zy, st),
		TEXTURE2D_ARGS(albedoAlphaMap, sampler_albedoAlphaMap)
	);
	half4 albedoY = SampleAlbedoAlpha(
		transformTexTriplanar(posW.xz, st),
		TEXTURE2D_ARGS(albedoAlphaMap, sampler_albedoAlphaMap)
	);
	half4 albedoZ = SampleAlbedoAlpha(
		transformTexTriplanar(posW.xy, st),
		TEXTURE2D_ARGS(albedoAlphaMap, sampler_albedoAlphaMap)
	);

	float3 blend = nmlW * nmlW;
	blend /= blend.x + blend.y + blend.z;

	return albedoX*blend.x + albedoY*blend.y + albedoZ*blend.z;
}

// _USETRIPLANAR_ONが定義されていたらTriPlanarで、そうでなければ普通にAlbedoAlphaをサンプリングする
#if defined(_USETRIPLANAR_ON)
	#define SAMPLE_ALBEDO_APLHA_BY_TRIPLANAR_OR_NOT(input, albedoAlphaMap, sampler_albedoAlphaMap) \
		SampleAlbedoAlphaByTriPlanar(\
			input.positionWS,\
			input.normalWS,\
			albedoAlphaMap##_ST,\
			TEXTURE2D_ARGS(albedoAlphaMap, sampler_albedoAlphaMap)\
		)
#else
	#define SAMPLE_ALBEDO_APLHA_BY_TRIPLANAR_OR_NOT(input, albedoAlphaMap, sampler_albedoAlphaMap) \
		SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(albedoAlphaMap, sampler_albedoAlphaMap))
#endif


// ディザでクリップする
void clipByDither(half2 normalizedScreenSpaceUV, half alpha) {
	half2 vpos = normalizedScreenSpaceUV * _ScreenParams.xy;
	vpos *= 0.25;				// 4px x 4pxのブロック毎にテクスチャをマッピングする

	// ディザ用テクスチャは4px x 4px x 16px
	clip( SAMPLE_TEXTURE3D(
		_DitherMaskLOD,
		sampler_DitherMaskLOD,
//		float3(vpos.xy, saturate(alpha) * 0.9375)
		float3(vpos.xy, saturate(alpha) * 0.94)		// 0.9375だとギリギリすぎてα1でもディザが出てしまう事があるので、少し大き目にしておく
	).a - 0.5 );
}

// ディザでクリップする
#define CLIP_BY_DITHER(input) \
	clipByDither(\
		GetNormalizedScreenSpaceUV(input.positionCS),\
		SAMPLE_ALBEDO_APLHA_BY_TRIPLANAR_OR_NOT(\
			input, _BaseMap, sampler_BaseMap\
		).a * _BaseColor.a\
	)

// αカットオフ値でクリップする
#define CLIP_BY_CUTOFF(input) \
	clip(\
		SAMPLE_ALBEDO_APLHA_BY_TRIPLANAR_OR_NOT(\
			input, _BaseMap, sampler_BaseMap\
		).a * _BaseColor.a - _Cutoff\
	)

// 指定の2色を、乗算・加算係数を指定して合成する処理。
// 合成方法を色々工夫できるので、頂点カラーの演算方法などに使用する
// lhsに対して、rhsをどう合成するかをblendParamで指定する。
// blendParam.xyが乗算係数。blendParam.zwが加算係数
half4 blendColor(half4 lhs, half4 rhs, half4 blendParam) {
	return lhs
		* lerp(1, rhs, blendParam.xxxy)		// 乗算
		+ rhs * blendParam.zzzw;			// 加算
}


