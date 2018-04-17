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

		float _FocusDistance, _FocusRange;
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
	}
}
