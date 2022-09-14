Shader "Pote/Std_Fx"
{
	Properties
	{
		// メインカラー
		[MainTexture]_BaseMap("メインカラー Tex", 2D) = "white" {}
		[HDR]_BaseColor("メインカラー 乗算色", Color) = (1,1,1,1)
		_BaseColor_Ofs("メインカラー ｵﾌｾｯﾄ色", Color) = (0,0,0,0)

		// テクスチャアニメーション
		[Toggle]_UseTexAnim("Texｱﾆﾒｰｼｮﾝを使用", Float) = 0
		_TexAnim_MaskTex("Texｱﾆﾒｰｼｮﾝ ﾏｽｸTex", 2D) = "white" {}
		[Toggle]_TexAnim_MaskOnlyA("Texｱﾆﾒｰｼｮﾝ ﾏｽｸTexをαのみ使用", Float) = 0
		[NORMAL]_TexAnim_DistTex("Texｱﾆﾒｰｼｮﾝ 歪みTex", 2D) = "white" {}
		_TexAnim_DistScl("Texｱﾆﾒｰｼｮﾝ 歪み量", Float) = 0
		_TexAnim_MainSpd("Texｱﾆﾒｰｼｮﾝ ﾒｲﾝ速度", Vector) = (0,0,0,0)
		_TexAnim_MaskSpd("Texｱﾆﾒｰｼｮﾝ ﾏｽｸ速度", Vector) = (0,0,0,0)
		_TexAnim_DistSpd("Texｱﾆﾒｰｼｮﾝ 歪み速度", Vector) = (0.09,0.18,0,0)

		// 環境マッピング
		[KeywordEnum(None,Rim,Sphere,Cube)]_UseEnvMap("環境ﾏｯﾋﾟﾝｸﾞ モード", Float) = 0
		_EnvMap_Rim_Pow("環境ﾏｯﾋﾟﾝｸﾞ Rim累乗係数", Float) = 5
		_EnvMap_Rim_Ofs("環境ﾏｯﾋﾟﾝｸﾞ Rimｵﾌｾｯﾄ値", Range(0,2)) = 1.2
		[HDR]_EnvMap_Rim_Col("環境ﾏｯﾋﾟﾝｸﾞ Rim色", Color) = (1,1,1,1)
		[NoScaleOffset]_EnvMap_SphereTex("環境ﾏｯﾋﾟﾝｸﾞ Spheremap", 2D) = "white" {}
		[NoScaleOffset]_EnvMap_CubeTex("環境ﾏｯﾋﾟﾝｸﾞ Cubemap", Cube) = "white" {}
		_EnvMap_MultAddRate("環境ﾏｯﾋﾟﾝｸﾞ 合成方法 xy乗算,zw加算", Vector) = (0,0,1,0)
		_EnvMap_MultAddRate_MODE("環境ﾏｯﾋﾟﾝｸﾞ 合成方法タイプ(inspector用)", Vector) = (2,0,0,0)

		// α値ｸﾘｯﾋﾟﾝｸﾞ
		[KeywordEnum(None,Cutoff,Dither)]_UseAlphaClip("α値ｸﾘｯﾋﾟﾝｸﾞ モード", Float) = 0
		_AlphaClip_Cutoff("α値ｸﾘｯﾋﾟﾝｸﾞ ｶｯﾄｵﾌ閾値", Range(0.0, 1.0)) = 0.5

		// 頂点カラーの計算方式
		_VColCalcParam_MultAddRate("頂点色の演算方法 xy乗算,zw加算", Vector) = (1,1,0,0)
		_VColCalcParam_MultAddRate_MODE("頂点色の演算方法ﾞ タイプ(inspector用)", Vector) = (1,1,0,0)

		// カラーの点滅アニメーション
		[HDR]_BlinkAnim_Col("点滅表現 カラー", Color) = (0,0,0,0)
		_BlinkAnim_Speed("点滅表現 速度", Float) = 20
		_BlinkAnim_Pivot("点滅表現 ﾋﾟﾎﾞｯﾄ", Range(0,1)) = 0

		// グラデーションマップ
		[Toggle]_UseGradMap("ｸﾞﾗﾃﾞｰｼｮﾝﾏｯﾌﾟを使用", Float) = 0
		[NoScaleOffset]_GradMap_Tex("ｸﾞﾗﾃﾞｰｼｮﾝﾏｯﾌﾟ Tex", 2D) = "white" {}
		[HDR]_GradMap_Col("ｸﾞﾗﾃﾞｰｼｮﾝﾏｯﾌﾟ 乗算色", Color) = (1,1,1,1)

		// フォグを使用するか否か
		[Toggle]_UseFog("フォグを使用", Float) = 1

		// RenderState系パラメータ
		[Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("Blend Op", Float) = 0		// Add
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Blend Src", Float) = 1	// One
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Blend Dst", Float) = 0	// Zero
		[Toggle] _ZWrite("Z Write", Float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Z Test", Float) = 4	// LEqual
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2       		// Back

		// ブレンドモード補佐用の、α値事前合成用パラメータ
		[Toggle]_IsPreCombineAlpha("α値事前合成を使用", Float) = 0
		_PreCombineAlphaPivot("α値事前合成 α=0でのﾌｪｰﾄﾞ先", Range(0,1)) = 0

		// ステンシルパラメータ
		_StencilRef("Stencil Ref", Int) = 0
		_StencilReadMask("Stencil ReadMask", Int) = 0
		_StencilWriteMask("Stencil WriteMask", Int) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comp", Float) = 8		// Always
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilPass("Stencil Pass", Float) = 0		// Keep
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilFail("Stencil Fail", Float) = 0		// Keep
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail("Stencil ZFail", Float) = 0	// Keep
	}

	SubShader
	{
		Tags{
			"RenderType" = "Transparent"
			"RenderPipeline" = "UniversalPipeline"
			"IgnoreProjector" = "True"
			"Queue" = "Transparent"
		}

		HLSLINCLUDE
			#include "Core/Core8.hlsl"

			// αクリッピングを使用するか否か
			#pragma multi_compile_local_fragment _USEALPHACLIP_NORMAL _USEALPHACLIP_CUTOFF _USEALPHACLIP_DITHER

			TEXTURE2D(_BaseMap);			SAMPLER(sampler_BaseMap);
			TEXTURE2D(_GradMap_Tex);		SAMPLER(sampler_GradMap_Tex);
			TEXTURE2D(_TexAnim_MaskTex);	SAMPLER(sampler_TexAnim_MaskTex);
			TEXTURE2D(_TexAnim_DistTex);	SAMPLER(sampler_TexAnim_DistTex);
			TEXTURE2D(_EnvMap_SphereTex);	SAMPLER(sampler_EnvMap_SphereTex);
			TEXTURECUBE(_EnvMap_CubeTex);	SAMPLER(sampler_EnvMap_CubeTex);

			CBUFFER_START(UnityPerMaterial)
			float4 _BaseMap_ST;
			half4 _BaseColor;
			half4 _BaseColor_Ofs;
			float4 _TexAnim_MaskTex_ST;
			half _TexAnim_MaskOnlyA;
			float2 _TexAnim_DistTex_ST;
			float _TexAnim_DistScl;
			float2 _TexAnim_MainSpd;
			float2 _TexAnim_MaskSpd;
			float2 _TexAnim_DistSpd;
			half _EnvMap_Rim_Pow;
			half _EnvMap_Rim_Ofs;
			half4 _EnvMap_Rim_Col;
			half4 _EnvMap_MultAddRate;
			half _AlphaClip_Cutoff;
			half4 _VColCalcParam_MultAddRate;
			half4 _BlinkAnim_Col;
			half _BlinkAnim_Speed;
			half _BlinkAnim_Pivot;
			half4 _GradMap_Col;
			half _UseFog;
			half _IsPreCombineAlpha;
			half _PreCombineAlphaPivot;
			CBUFFER_END
		ENDHLSL

		Pass
		{
			Tags{"LightMode" = "UniversalForward"}

			Stencil {
				Ref [_StencilRef]
				ReadMask [_StencilReadMask]
				WriteMask [_StencilWriteMask]
				Comp [_StencilComp]
				Pass [_StencilPass]
				Fail [_StencilFail]
				ZFail [_StencilZFail]
			}

			Blend[_SrcBlend][_DstBlend]
			BlendOp[_BlendOp]
			ZWrite[_ZWrite]
			ZTest[_ZTest]
			Cull[_Cull]

			HLSLPROGRAM
			#pragma only_renderers gles gles3 glcore d3d11
			#pragma target 2.0

			#pragma multi_compile_fog

			// 環境マップを使用するか否か
			#pragma multi_compile_local _USEENVMAP_NONE _USEENVMAP_RIM _USEENVMAP_SPHERE _USEENVMAP_CUBE

			// テクスチャアニメーションを使用するか否か
			#pragma multi_compile_local _ _USETEXANIM_ON

			// グラデーションマップを使用するか否か
			#pragma multi_compile_local_fragment _ _USEGRADMAP_ON


			#pragma vertex vert
			#pragma fragment frag

			struct Attributes
			{
				float4 positionOS   : POSITION;
				half4 color         : COLOR;
				float3 normalOS     : NORMAL;
				float2 texcoord     : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS   : SV_POSITION;
//				float3 positionWS   : TEXCOORD0;

			#if _USEENVMAP_RIM || _USEENVMAP_SPHERE || _USEENVMAP_CUBE
				float3 viewDirWS	: TEXCOORD0;
			#endif
			#if _USEENVMAP_RIM || _USEENVMAP_CUBE
				float3 normalWS		: TEXCOORD1;
			#endif
			#if _USEENVMAP_SPHERE
				float3 normalEyeS	: TEXCOORD2;
			#endif

				half4 color         : TEXCOORD3;
			#if _USETEXANIM_ON
				float4 uv0          : TEXCOORD4;
				float2 uv1          : TEXCOORD5;
			#else
				float2 uv           : TEXCOORD4;
			#endif
				half fogFactor		: TEXCOORD6;
			};

			Varyings vert(Attributes input)
			{
				Varyings output = (Varyings)0;

				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
				VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
				half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);

				// テクスチャアニメーションを使用するか否かでUVの計算を変えておく
			#if _USETEXANIM_ON
				output.uv0 = float4(
					TRANSFORM_TEX(input.texcoord, _BaseMap)
						+ _Time.y*_TexAnim_MainSpd,
					TRANSFORM_TEX(input.texcoord, _TexAnim_MaskTex)
						+ _Time.y*_TexAnim_MaskSpd
				);
				output.uv1 = input.texcoord * _TexAnim_DistTex_ST.xy	// distTexはuvOffsetしない
					+ _Time.y*_TexAnim_DistSpd;
			#else
				output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
			#endif

				// 各種座標をVeryingsに詰める
			#if _USEENVMAP_RIM || _USEENVMAP_SPHERE || _USEENVMAP_CUBE
				output.viewDirWS = viewDirWS;
			#endif
			#if _USEENVMAP_RIM || _USEENVMAP_CUBE
				output.normalWS = normalInput.normalWS;
			#endif
			#if _USEENVMAP_SPHERE
				output.normalEyeS = mul((float3x3)UNITY_MATRIX_V, normalInput.normalWS);
//				output.normalEyeS.z = min(output.normalEyeS.z, 0);		// 法線が視線より向こう側にならないようにする
				output.normalEyeS = normalize( output.normalEyeS );
			#endif
//				output.positionWS = vertexInput.positionWS;
				output.positionCS = vertexInput.positionCS;

				// 頂点カラー
				output.color = input.color;

				// フォグパラメータ
				output.fogFactor = _UseFog * computeFogFactor(vertexInput.positionWS);
				return output;
			}

			half4 frag(Varyings input) : SV_Target
			{
				// まず各種座標を計算する

				// ポリゴンからピクセルへ向かう方向
				// (View空間の奥行方向とは関係ないので注意)
				// (Unity内部でこういう名前にしているので合わせている)
			#if _USEENVMAP_RIM || _USEENVMAP_SPHERE || _USEENVMAP_CUBE
				half3 viewDirWS = SafeNormalize(input.viewDirWS);
			#endif

				// ワールド法線
			#if _USEENVMAP_RIM || _USEENVMAP_CUBE
				half3 normalWS = NormalizeNormalPerPixel(input.normalWS);
			#endif

				// 画面上での法線
			#if _USEENVMAP_SPHERE
				half3 normalEyeS = SafeNormalize(input.normalEyeS);
				{
					// 画面脇によっても歪まないように補正する。
					// おそらくこの補正は正確ではないが、いい感じに補正される。
					// UnitychanShaderの処理を参考にしているが、原理はちょっとよくわからない。
					// 参考:https://github.com/unity3d-jp/UnityChanToonShaderVer2_Project/blob/release/legacy/2.0/Assets/Toon/Shader/UCTS_DoubleShadeWithFeather.cginc
					//     line:301-307
					// normalizeを使用しない分、愚直に計算するよりは早そう。
					float3 tmp_d = normalEyeS.xyz * float3(-1,-1,1);
					float3 tmp_b = mul( (float3x3)UNITY_MATRIX_V, viewDirWS ) * float3(-1,-1,1) + float3(0,0,1);
					normalEyeS = tmp_b*dot(tmp_b, tmp_d)/tmp_b.z - tmp_d;
				}
			#endif


				// 有効であればテクスチャアニメーションを適応しつつ、
				// メインカラーを読み込み
				float4 albedo;
				{
				#if _USETEXANIM_ON
					float3 dist = SampleNormal(
						input.uv1,
						TEXTURE2D_ARGS(_TexAnim_DistTex, sampler_TexAnim_DistTex)
					);

					float4 mask = SampleAlbedoAlpha(
						input.uv0.zw + dist.xy*_TexAnim_DistScl,
						TEXTURE2D_ARGS(_TexAnim_MaskTex, sampler_TexAnim_MaskTex)
					);
					mask = float4(
						lerp(mask.rgb, 1, _TexAnim_MaskOnlyA),
						mask.a
					);

					albedo = SampleAlbedoAlpha(
						input.uv0.xy + dist.xy*_TexAnim_DistScl,
						TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)
					) * mask;
				#else
					albedo = SampleAlbedoAlpha(
						input.uv,
						TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)
					);
				#endif

					albedo = (albedo + _BaseColor_Ofs)*_BaseColor;
				}

				{// 環境マッピングの適応
				#if !_USEENVMAP_NONE
						half4 envmapCol;
					#if _USEENVMAP_RIM
						// リムライティング
						float rimRate = dot(viewDirWS, normalWS);
						rimRate = pow(
							max( 0, _EnvMap_Rim_Ofs - rimRate ),
							_EnvMap_Rim_Pow
						);
						envmapCol = rimRate * _EnvMap_Rim_Col;
					#elif _USEENVMAP_SPHERE
						// Sphereマッピング
						float2 envDir = normalEyeS.xy*0.5 + 0.5;
						envmapCol = SampleAlbedoAlpha(
							envDir,
							TEXTURE2D_ARGS(_EnvMap_SphereTex, sampler_EnvMap_SphereTex)
						);
					#elif _USEENVMAP_CUBE
						// Cubeマッピング
						float3 envDir = reflect(viewDirWS, normalWS);
						envmapCol = SAMPLE_TEXTURECUBE(_EnvMap_CubeTex, sampler_EnvMap_CubeTex, envDir);
					#endif
						albedo = blendColor(albedo, envmapCol, _EnvMap_MultAddRate);
				#endif
				}

				// 頂点カラーとの演算を実行
				albedo = blendColor(albedo, input.color, _VColCalcParam_MultAddRate);

				{// 点滅表現を入れる
					float4 blinkRate = step( 0.5, frac(_Time.y * _BlinkAnim_Speed) );
					float4 blinkCol = (blinkRate - _BlinkAnim_Pivot) * _BlinkAnim_Col;
					albedo += blinkCol;
				}

				// グラデーションマップを適応
			#if _USEGRADMAP_ON
				{
					half4 gradMappedAlbedo = SampleAlbedoAlpha(
						albedo.a,
						TEXTURE2D_ARGS(_GradMap_Tex, sampler_GradMap_Tex)
					) * _GradMap_Col;
					albedo = half4(
						albedo.rgb * gradMappedAlbedo.rgb,
						gradMappedAlbedo.a
					);
				}
			#endif

				// αでクリップする
			#if defined(_USEALPHACLIP_CUTOFF)
				clip(albedo.a - _AlphaClip_Cutoff);
			#elif defined(_USEALPHACLIP_DITHER)
				clipByDither(GetNormalizedScreenSpaceUV(input.positionCS), albedo.a);
			#endif

				// 出力がバグらないように、ここで0-1に丸める
				// 最後にcolorに対してやるとFog計算後になるが、もしかしたらそちらの方がいいかも？
				albedo = saturate(albedo);

				// Fogを適応
				half3 color = albedo.rgb;
				color = MixFog(color, input.fogFactor);

				// 必要であれば、α値をRGBへ事前反映しておく
				color = lerp(
					color,
					lerp(_PreCombineAlphaPivot, color, albedo.a),
					_IsPreCombineAlpha
				);

				return half4(color, albedo.a);
			}

			ENDHLSL
		}
	}

	FallBack "Hidden/Universal Render Pipeline/FallbackError"
	CustomEditor "App.Editor.ShaderInspector.Std_Fx"
}
