Shader "Hidden/DepthOfField"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	CGINCLUDE
		#include "UnityCG.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
		};

		v2f vert (appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;
			return o;
		}
		
		sampler2D _MainTex, _CameraDepthTexture;
		float4 _MainTex_TexelSize;

		float _BokehRadius, _FocusDistance, _FocusRange;
	ENDCG

	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass // 0 circleOfConfusionPass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			half frag (v2f i) : SV_Target
			{
				float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				depth = LinearEyeDepth(depth);
				float coc = (depth - _FocusDistance) / _FocusRange;
				coc = clamp(coc, -1, 1);
				return coc;
			}
			ENDCG
		}

		Pass // 1 bokehPass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#define BOKEH_KERNEL_MEDIUM

			// From https://github.com/Unity-Technologies/PostProcessing/
			// blob/v2/PostProcessing/Shaders/Builtins/DiskKernels.hlsl
			#if defined(BOKEH_KERNEL_SMALL)
				static const int kernelSampleCount = 16;
				static const float2 kernel[kernelSampleCount] = {
					float2(0, 0),
					float2(0.54545456, 0),
					float2(0.16855472, 0.5187581),
					float2(-0.44128203, 0.3206101),
					float2(-0.44128197, -0.3206102),
					float2(0.1685548, -0.5187581),
					float2(1, 0),
					float2(0.809017, 0.58778524),
					float2(0.30901697, 0.95105654),
					float2(-0.30901703, 0.9510565),
					float2(-0.80901706, 0.5877852),
					float2(-1, 0),
					float2(-0.80901694, -0.58778536),
					float2(-0.30901664, -0.9510566),
					float2(0.30901712, -0.9510565),
					float2(0.80901694, -0.5877853),
				};
			#elif defined(BOKEH_KERNEL_MEDIUM)
				static const int kernelSampleCount = 22;
				static const float2 kernel[kernelSampleCount] = {
					float2(0, 0),
					float2(0.53333336, 0),
					float2(0.3325279, 0.4169768),
					float2(-0.11867785, 0.5199616),
					float2(-0.48051673, 0.2314047),
					float2(-0.48051673, -0.23140468),
					float2(-0.11867763, -0.51996166),
					float2(0.33252785, -0.4169769),
					float2(1, 0),
					float2(0.90096885, 0.43388376),
					float2(0.6234898, 0.7818315),
					float2(0.22252098, 0.9749279),
					float2(-0.22252095, 0.9749279),
					float2(-0.62349, 0.7818314),
					float2(-0.90096885, 0.43388382),
					float2(-1, 0),
					float2(-0.90096885, -0.43388376),
					float2(-0.6234896, -0.7818316),
					float2(-0.22252055, -0.974928),
					float2(0.2225215, -0.9749278),
					float2(0.6234897, -0.7818316),
					float2(0.90096885, -0.43388376),
				};
			#endif

			half4 frag (v2f i) : SV_Target
			{
				half3 color = 0;
				for (int k = 0; k < kernelSampleCount; k++) {
					float2 o = kernel[k];
					o *= _MainTex_TexelSize.xy * _BokehRadius;
					color += tex2D(_MainTex, i.uv + o).rgb;
				}
				color *= 1.0 / kernelSampleCount;
				return half4(color, 1);
			}
			ENDCG
		}

		Pass // 2 postFilterPass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			half4 frag (v2f i) : SV_Target
			{
				float4 o = _MainTex_TexelSize.xyxy * float2(-0.5, 0.5).xxyy;
				half4 s =
					tex2D(_MainTex, i.uv + o.xy) +
					tex2D(_MainTex, i.uv + o.zy) +
					tex2D(_MainTex, i.uv + o.xw) +
					tex2D(_MainTex, i.uv + o.zw);
				return s * 0.25;
			}
			ENDCG
		}
	}
}
