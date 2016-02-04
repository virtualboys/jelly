Shader "Toon/Lit" {
	Properties {
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_LeftPole ("Left Pole", Vector) = (0,0,0)
		_RightPole ("Right Pole", Vector)= (1,1,1)
		_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {} 
		_WaveAmp ("Wave Amplitude", Float) = 1
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
CGPROGRAM
#pragma surface surf ToonRamp fullforwardshadows addshadow vertex:vert

sampler2D _Ramp;

// custom lighting function that uses a texture ramp based
// on angle between light direction and normal
#pragma lighting ToonRamp exclude_path:prepass 
inline half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half atten)
{
	#ifndef USING_DIRECTIONAL_LIGHT
	lightDir = normalize(lightDir);
	#endif
	
	half d = dot (s.Normal, lightDir)*0.5+0.5;
	half3 ramp = tex2D (_Ramp, float2(d,0)).rgb;
	
	half4 c;
//	if(atten < 1)
//		c.rgb = s.Albedo * _LightColor0.rgb * (atten * 2);
//	else
	c.rgb = s.Albedo * _LightColor0.rgb * ramp;// * (atten * 2);
	c.a = 0;
	return c;
}


sampler2D _MainTex;
float4 _Color;
float4 _LeftPole;
float4 _RightPole;
float _WaveAmp;


struct Input {
	float2 uv_MainTex : TEXCOORD0;
};

void vert (inout appdata_full v) {
	float3 left = float3(_LeftPole.x, _LeftPole.y, _LeftPole.z);
	float3 right = float3(_RightPole.x, _RightPole.y, _RightPole.z);

	float3 axis = right - left;
	float d = dot(v.vertex.xyz - left, axis) / dot(axis, axis);
	//float noise = cos(v.texcoord.y*2000) + sin(v.texcoord.x*2000);
    //v.vertex.xyz += .05*noise* v.normal;
    v.vertex.y += .05*_WaveAmp*sin(_Time.y +d * 3.14*6);
}

void surf (Input IN, inout SurfaceOutput o) {
	half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG

	} 

	Fallback "Diffuse"
}
