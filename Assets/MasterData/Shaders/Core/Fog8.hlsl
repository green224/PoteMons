//
// カスタムFog用のコード
//

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


//float4 _EnvFog_FogParam;
#define _EnvFog_FogParam float4(0,0,0,0)


// Fog用のパラメータを計算する。
// オリジナルのComputeFogFactorはカメラ位置からの正確な距離で取っておらず、
// カメラ向きによってかかり方が変わってしまうので、正確な位置で取るように変更している。
real computeFogFactor(float3 positionWS) {

	// UNITY_Z_0_FAR_FROM_CLIPSPACE の代わりに、カメラからの正確な距離をもとに計算する
	float cam2posLen = length(positionWS - _WorldSpaceCameraPos);

	// これは正確には元コードのclipZ_01とは異なるが、別に正確に元の距離にする必要はない
	// (むしろこちらの方が正確)なので、処理を簡略化するためにこうしている。
//	float clipZ_01 = (cam2posLen-_ProjectionParams.y) / (_ProjectionParams.z-_ProjectionParams.y) * _ProjectionParams.z;
	float clipZ_01 = cam2posLen;

	// カスタムFogの処理を入れる
	clipZ_01 = max( 0, clipZ_01 - _EnvFog_FogParam.x );

	// ここから下はオリジナルのソースと同じ
	// → PackageCache\com.unity.render-pipelines.universal@10.4.0\ShaderLibrary\ShaderVariablesFunctions.hlsl
	//      → real ComputeFogFactor(float z)
	#if defined(FOG_LINEAR)
		// factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
		float fogFactor = saturate(clipZ_01 * unity_FogParams.z + unity_FogParams.w);
		return real(fogFactor);
	#elif defined(FOG_EXP) || defined(FOG_EXP2)
		// factor = exp(-(density*z)^2)
		// -density * z computed at vertex
		return real(unity_FogParams.x * clipZ_01);
	#else
		return 0.0h;
	#endif
}
