Shader "Hidden/Bloom"
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

		sampler2D _MainTex, _SourceTex;
		float4 _MainTex_TexelSize;
		half _Threshold;

		half3 Prefilter (half3 c)
		{
			half brightness = max(c.r, max(c.g, c.b));
			half contribution = max(0, brightness - _Threshold);
			contribution /= max(brightness, 0.00001);
			return c * contribution;
		}

		half3 Sample (float2 uv)
		{
			return tex2D(_MainTex, uv).rgb;
		}

		half3 SampleBox (float2 uv, float delta)
		{
			float4 o = _MainTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
			half3 s =
				Sample(uv + o.xy) + Sample(uv + o.zy) +
				Sample(uv + o.xw) + Sample(uv + o.zw);
			return s * 0.25f;
		}
	ENDCG

	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass // 0
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			half4 frag (v2f i) : SV_Target
			{
				return half4(Prefilter(SampleBox(i.uv, 1)), 1);
			}
			ENDCG
		}

		Pass // 1
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			half4 frag (v2f i) : SV_Target
			{
				return half4(SampleBox(i.uv, 1), 1);
			}
			ENDCG
		}

		Pass // 2
		{
			Blend One One
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			half4 frag (v2f i) : SV_Target
			{
				return half4(SampleBox(i.uv, 0.5), 1);
			}
			ENDCG
		}

		Pass // 3
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				half4 frag (v2f i) : SV_Target
				{
					half4 c = tex2D(_SourceTex, i.uv);
					c.rgb += SampleBox(i.uv, 0.5);
					return c;
				}
			ENDCG
		}

		Pass // 4
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				half4 frag (v2f i) : SV_Target
				{
					return half4(SampleBox(i.uv, 0.5), 1);
				}
			ENDCG
		}
	}
}
