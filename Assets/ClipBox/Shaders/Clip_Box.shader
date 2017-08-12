Shader "Clip/Box" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Main Texture", 2D) = "white" {}
		_NormalMap ("Normal Map", 2D) = "bump" {}
		_Smoothness ("Smoothness", Range(0, 1)) = 0.0
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		Cull Off
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		
		struct Input {
			float2 uv_MainTex;
			float2 uv_NormalMap;
			float3 worldPos;
		};
		
		half _Smoothness;
		half _Metallic;
		
		// Initalized by Shader.SetGlobalVector
		float3 _Origin;
		float3 _BoxSize;
		float3 _BoxRotation;
		
		fixed4 _Color;
		sampler2D _MainTex;
		sampler2D _NormalMap;
		
		void surf (Input IN, inout SurfaceOutputStandard o) {

			float3 dir = IN.worldPos - _Origin;
			// instead rotate the box, we inverse rotate the point
			float3 rads = float3(-radians(_BoxRotation.x), -radians(_BoxRotation.y), -radians(_BoxRotation.z));

			// unity rotate object with order: z, x, y, so we invert the order,
			// and invert the angles to rotate the dir, then we can detect point
			// whether in the box on a no rotating coordinate system

			float sinx = sin(rads.x), cosx = cos(rads.x);
			float siny = sin(rads.y), cosy = cos(rads.y);
			float sinz = sin(rads.z), cosz = cos(rads.z);
			/*
			float3x3 matz = {cosz, -sinz, 0,
							 sinz, cosz, 0,
							 0, 0, 1};
			float3x3 matx = {1, 0, 0,
							 0, cosx, -sinx,
							 0, sinx, cosx};
			float3x3 maty = {cosy, 0, siny,
							 0, 1, 0,
							 -siny, 0, cosy};
			*/
			float3x3 mat = {cosz*cosy-sinz*sinx*siny, -sinz*cosx, cosz*siny+sinz*sinx*cosy,
							sinz*cosy+cosz*sinx*siny, cosz*cosx, sinz*siny-cosz*sinx*cosy,
							-cosx*siny, sinx, cosx*cosy};
			dir = mul(mat, dir);
			half3 dist = half3(
				abs(dir.x), // no negatives
				abs(dir.y), // no negatives
				abs(dir.z)  // no negatives
			);


			dist.x = dist.x - _BoxSize.x * 0.5;
			dist.y = dist.y - _BoxSize.y * 0.5;
			dist.z = dist.z - _BoxSize.z * 0.5;

			// all need to be less than size
			half t = dist.x;
			t = max(t, dist.y);
			t = max(t, dist.z);

			clip(-t); 
			
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			
			o.Albedo = c.rgb;
			o.Normal = UnpackNormal (tex2D (_NormalMap, IN.uv_NormalMap));
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
}