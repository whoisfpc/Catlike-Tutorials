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
		float4 _MainTex_TexelSize;
		float _ContrastThreshold, _RelativeThreshold;

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

		float SampleLuminance (float2 uv, float uOffset, float vOffset)
		{
			uv += _MainTex_TexelSize * float2(uOffset, vOffset);
			return SampleLuminance(uv);
		}

		struct LuminanceData {
			float m, n, e, s, w;
			float ne, nw, se, sw;
			float highest, lowest, contrast;
		};

		LuminanceData SampleLuminanceNeighborhood (float2 uv) {
			LuminanceData l;
			l.m = SampleLuminance(uv);
			l.n = SampleLuminance(uv, 0, 1);
			l.e = SampleLuminance(uv, 1, 0);
			l.s = SampleLuminance(uv, 0, -1);
			l.w = SampleLuminance(uv, -1, 0);

			l.ne = SampleLuminance(uv,  1,  1);
			l.nw = SampleLuminance(uv, -1,  1);
			l.se = SampleLuminance(uv,  1, -1);
			l.sw = SampleLuminance(uv, -1, -1);

			l.highest = max(max(max(max(l.n, l.e), l.s), l.w), l.m);
			l.lowest = min(min(min(min(l.n, l.e), l.s), l.w), l.m);
			l.contrast = l.highest - l.lowest;
			return l;
		}

		bool ShouldSkipPixel (LuminanceData l)
		{
			float threshold = max(_ContrastThreshold, _RelativeThreshold * l.highest);
			return l.contrast < threshold;
		}

		float DeterminePixelBlendFactor (LuminanceData l)
		{
			float filter = 2 * (l.n + l.e + l.s + l.w);
			filter += l.ne + l.nw + l.se + l.sw;
			filter *= 1.0 / 12;
			filter = abs(filter - l.m);
			filter = saturate(filter / l.contrast);
			float blendFactor = smoothstep(0, 1, filter);
			return blendFactor * blendFactor;
		}

		float4 ApplyFXAA (float2 uv)
		{
			LuminanceData l = SampleLuminanceNeighborhood(uv);
			if (ShouldSkipPixel(l)) {
				return 0;
			}
			float pixelBlend = DeterminePixelBlendFactor(l);
			return pixelBlend;
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
