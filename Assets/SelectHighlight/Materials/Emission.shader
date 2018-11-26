Shader "Unlit/Emission"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
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
				float linearDepth : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
			};

			sampler2D_float _CameraDepthTexture;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.screenPos = ComputeScreenPos(o.vertex);
				o.linearDepth = -(UnityObjectToViewPos(v.vertex).z * _ProjectionParams.w);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 c = fixed4(0, 0, 0, 1);
				// decode depth texture info
				float2 uv = i.screenPos.xy / i.screenPos.w; // normalized screen-space pos
				float camDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
				camDepth = Linear01Depth (camDepth); // converts z buffer value to depth value from 0..1

                float diff = saturate(i.linearDepth - camDepth);
                if(diff < 0.001)
                    c = float4(1, 0, 0, 1);

                return c;
			}
			ENDCG
		}
	}
}
