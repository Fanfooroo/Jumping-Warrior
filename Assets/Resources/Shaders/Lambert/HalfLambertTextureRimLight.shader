Shader "NlabShader/HalfLambertTextureRimLight" {
	Properties {
		_MainTex ("Texture (RGB)", 2D) = "white" {}
		_RimColor ("Rim Color(RGB)", Color) = (0.26,0.19,0.16,0.0)
		_RimPower ("Rim Power", Range(0.5, 8.0)) = 3.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf HalfLambert
		
		half4 LightingHalfLambert (SurfaceOutput s, half3 lightDir, half atten) {
			float NdotL = max(0,dot(normalize(s.Normal), normalize(lightDir)));
			NdotL = NdotL * 0.5 + 0.5;
			NdotL = pow(NdotL, 2);
          	half4 c;
          	c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten * 2);
          	c.a = s.Alpha;
          	return c;
      	}

		sampler2D _MainTex;
		float4 _RimColor;
		float _RimPower;

		struct Input {
			float2 uv_MainTex;
			float3 viewDir;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
         	o.Emission = _RimColor.rgb * pow (rim, _RimPower);
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
