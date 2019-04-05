Shader "Custom/MoveIntoPosition"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		_Speed("MoveSpeed", Range(1,50)) = 10 // speed of the swaying
		[Toggle(In_Edit)] _Edit("In edit mode?", Int) = 1
        [Toggle(DOWN_ON)] _DOWN("Move Down?", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
		#pragma multi_compile_instancing
		#pragma shader_feature DOWN_ON
		#pragma shader_feature In_Edit

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		float _Speed;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
			UNITY_DEFINE_INSTANCED_PROP(float, _Moved)
        UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata_full v)
		{
#if !In_Edit
		    v.vertex.xyz *= UNITY_ACCESS_INSTANCED_PROP(Props, _Moved);
#if DOWN_ON
            v.vertex.y += _Speed-UNITY_ACCESS_INSTANCED_PROP(Props, _Moved * _Speed);
#else
            v.vertex.y -= _Speed-UNITY_ACCESS_INSTANCED_PROP(Props, _Moved * _Speed);
#endif
#endif
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
