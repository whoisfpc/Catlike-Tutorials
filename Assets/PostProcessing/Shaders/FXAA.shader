Shader "Hidden/FXAA"
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
		
		sampler2D _MainTex;

		float4 Sample (float2 uv)
		{
			return tex2D(_MainTex, uv);
		}

		float SampleLuminance (float2 uv)
		{
			#if defined(LUMINANCE_GREEN)
				return Sample(uv).g;
			#else
				return Sample(uv).a;
			#endif
		}
		
		float4 ApplyFXAA (float2 uv)
		{
			return SampleLuminance(uv);
		}
	ENDCG
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass // 0 luminancePass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4 frag (v2f i) : SV_Target
			{
				float4 sample = tex2D(_MainTex, i.uv);
				sample.a = LinearRgbToLuminance(saturate(sample.rgb));
				return sample;
			}
			ENDCG
		}

		Pass // 1 fxaaPass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ LUMINANCE_GREEN

			float4 frag (v2f i) : SV_Target
			{
				return ApplyFXAA(i.uv);
			}
			ENDCG
		}
	}
}
