Shader "Car Paint Shading/Car Paint Opaque" {
	Properties {
		[Header(Basic)] _Color ("Tint", Vector) = (1,1,1,1)
		_MainTex ("Albedo", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_Metallic ("Metallic", Range(0, 1)) = 0
		_BumpScale ("Bump Scale", Float) = 1
		[Normal] _BumpMap ("Bump Map", 2D) = "bump" {}
		_EmissionColor ("Emission", Vector) = (0,0,0,1)
		[Header(Flake Bump)] [NoScaleOffset] [Normal] _FlakesBumpMap ("Flakes Bump", 2D) = "bump" {}
		_FlakesBumpMapScale ("Flakes Scale", Float) = 1
		_FlakesBumpStrength ("Flakes Strength", Range(0.001, 1)) = 0.2
		[Header(Flake)] _FlakeColor ("Flakes", Vector) = (1,1,1,1)
		_FlakesColorStrength ("Flakes Strength", Range(0, 10)) = 1
		_FlakesColorCutoff ("Flakes Cutoff", Range(0, 0.95)) = 0.5
		[Header(Fresnel)] _FresnelColor ("Color", Vector) = (1,1,1,1)
		_FresnelPower ("Power", Range(0, 10)) = 1
		[Header(Reflection)] _ReflectionSpecular ("Specular", Vector) = (0.3,0.3,0.3,1)
		_ReflectionGlossiness ("Smoothness", Range(0, 1)) = 1
		[Header(Flake)] _SparklePert ("Flakes Strength", Range(0, 10)) = 1
		_PaintColor0 ("Paint Color 0", Vector) = (0.4,0,0.35,1)
		_PaintColor1 ("Paint Color 1", Vector) = (0.6,0,0,1)
		_PaintColor2 ("Paint Color 2", Vector) = (0,0.35,0,1)
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
	//CustomEditor "CarPaintEditor"
}