﻿Shader "MFPS/4 Rim" {
	Properties{
		 _MainTex("Texture", 2D) = "white" {}
		 _Color("Main Color", Color) = (1, 1, 1, 1)
		 _RimColor("Rim Color", Color) = (0.26,0.19,0.16,0.0)
		 _RimPower("Rim Power", Range(0.5,8.0)) = 3.0
	}
		SubShader{
		  Tags { "RenderType" = "Opaque" }
		  CGPROGRAM
		  #pragma surface surf Lambert
		  fixed4 _Color;
		  struct Input {
			  float2 uv_MainTex;
			  float2 uv_BumpMap;
			  float3 viewDir;
		  };
		  sampler2D _MainTex;
		  sampler2D _BumpMap;
		  float4 _RimColor;
		  float _RimPower;
		  void surf(Input IN, inout SurfaceOutput o) {
			  fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			  fixed4 c = tex * _Color;
				o.Albedo = c.rgb;
			  half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
			  o.Emission = _RimColor.rgb * pow(rim, _RimPower);
		  }
		  ENDCG
		 }
			 Fallback "Diffuse"
}