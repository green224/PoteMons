//
// 深度周りで使用する処理をまとめたもの
//

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

// 深度をみて、ソフトパーティクル処理を行った結果のアルファ値を返す。
float SoftParticles(float near, float far, float4 projection)
{
	float fade = 1;
	if (near > 0.0 || far > 0.0)
	{
		float rawDepth = SampleSceneDepth(projection.xy / projection.w);
		float sceneZ = LinearEyeDepth(rawDepth, _ZBufferParams);
		float thisZ = LinearEyeDepth(projection.z / projection.w, _ZBufferParams);
		fade = saturate(far * ((sceneZ - near) - thisZ));
	}
	return fade;
}

// 深度をみて、デカール処理を行った結果のアルファ値を返す。
// 内部的にはソフトパーティクル処理を逆にしただけ
float Decal(float near, float far, float4 projection) {
	float fade = 1;
	if (near > 0.0 || far > 0.0)
	{
		float rawDepth = SampleSceneDepth(projection.xy / projection.w);
		float sceneZ = LinearEyeDepth(rawDepth, _ZBufferParams);
		float thisZ = LinearEyeDepth(projection.z / projection.w, _ZBufferParams);
		fade = 1 - saturate(far * ((sceneZ - near) - thisZ));
	}
	return fade;
}

