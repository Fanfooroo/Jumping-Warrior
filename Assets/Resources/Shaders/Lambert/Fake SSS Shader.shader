Shader "NlabShader/Fake SSS Shader" {
	Properties {
		_MainTex ("Texture (RGB)", 2D) = "white" {}
		//_BumpMap ("Bump (RGB)", 2D) = "bump" {}
		_RimColor ("Rim Color(RGB)", Color) = (0.26,0.19,0.16,0.0)
		_RimPower ("Rim Power", Range(0.5, 8.0)) = 3.0
		_ScatterColor ("Scatter Color", Color) = (0.15, 0.0, 0.0, 1.0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf FakeSkinShader
		
		float4 _ScatterColor;
		
		half4 LightingFakeSkinShader (SurfaceOutput s, half3 lightDir, half atten) {
			float wrap = 0.2;
  			float scatterWidth = 0.3;
  			//float4 scatterColor = float4(0.15, 0.0, 0.0, 1.0);
  			float shininess = 40.0;
  			
			float NdotL = max(0,dot(normalize(s.Normal), normalize(lightDir)));
			
			NdotL = NdotL * 0.5 + 0.5;
			NdotL = pow(NdotL, 2);
			
			float NdotL_wrap = (NdotL + wrap) / (1 + wrap);
			
			float scatter = smoothstep(0.0, scatterWidth, NdotL_wrap) * smoothstep(scatterWidth * 2.0, scatterWidth, NdotL_wrap);
			
			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten * 2) + scatter * _ScatterColor;
			c.a = s.Alpha;
			return c;
		}

		sampler2D _MainTex;
		//sampler2D _BumpMap;
		float4 _RimColor;
		float _RimPower;

		struct Input {
			float2 uv_MainTex;
			//float2 uv_BumpMap;
			float3 viewDir;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			//o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
			
			half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
         	o.Emission = _RimColor.rgb * pow (rim, _RimPower);
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
