Shader "NamTV/Standard" {
	Properties {
		[Enum(UnityEditor.BlendMode)] _Mode ("Rendering mode", Float) = 1
		_Color ("Color", Vector) = (1,1,1,1)
		_DiffuseColorForce ("Color Force", Range(0, 3)) = 1
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 1
		_MetallicGlossMap ("Metallic (RGB)", 2D) = "white" {}
		_GlossMapScale ("Smoothness", Range(0, 1)) = 0.9
		_Glossiness ("Smoothness", Range(0, 1)) = 0.25
		_Metallic ("Metallic ", Range(0, 1)) = 0.25
		[Enum(UnityEditor.SmoothnessMapChannel)] _SmoothnessTextureChannel ("Source", Float) = 1
		_EmissionMap ("Emission Map", 2D) = "white" {}
		[HDR] _EmissionColor ("EmissionColor", Vector) = (1,1,1,1)
		_EmissionForce ("EmissionForce", Range(0, 3)) = 1
		_BumpMap ("NormalMap Texture", 2D) = "Bump" {}
		[Toggle] _ZWrite ("ZWrite", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "BCShaderGUI"
}