Shader "NamTV/Zen/Metallic" {
	Properties {
		_DiffuseColor ("DiffuseColor", Vector) = (1,1,1,1)
		_DiffuseColorForce ("DiffuseColorForce", Range(0, 3)) = 1
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[Toggle(_METALLIC_ON)] _UseMetallic ("Use Metallic", Float) = 0
		_Glossiness ("Smoothness", Range(0, 1)) = 0.9
		_Metallic ("Metallic", Range(0, 1)) = 0.25
		[Toggle(_EMISSION_ON)] _UseEmission ("Use Emission", Float) = 0
		_EmissionTexture ("EmissionTexture", 2D) = "white" {}
		[HDR] _EmissionColor ("EmissionColor", Vector) = (1,1,1,1)
		_EmissionForce ("EmissionForce", Range(0, 3)) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
}