//--------------------------------------------------------------
//-----------------------Smooth Shadow Map---------------  -----
//--------------------------------------------------------------
#include "./bard_shader_lib.hlsl"
struct appdata_planeShadow
{
	float4 vertex : POSITION;
    float2 uv     : TEXCOORD0;
};

struct v2f_planeShadow
{
	float4 vertex : SV_POSITION;
	float3 xlv_TEXCOORD0 : TEXCOORD0;
	float3 xlv_TEXCOORD1 : TEXCOORD1;
    float2 uv            : TEXCOORD2;
};

v2f_planeShadow vert_planeShadow(appdata_planeShadow v)
{
	v2f_planeShadow o;
	float3 lightdir = normalize(_DirectionLightPos.xyz);
	float3 worldpos = mul(unity_ObjectToWorld, v.vertex).xyz;
	// _ShadowPlane.w = p0 * n  // 平面的w分量就是p0 * n
	float distance = (_ShadowOffset - dot(_ShadowPlane.xyz, worldpos)) / dot(_ShadowPlane.xyz, lightdir.xyz);
	worldpos = worldpos + distance * lightdir.xyz;
	o.vertex = mul(unity_MatrixVP, float4(worldpos, 1.0));

	o.xlv_TEXCOORD0 = _WorldPos.xyz;
	o.xlv_TEXCOORD1 = worldpos;
    o.uv = v.uv;
	return o;
}

float4 frag_planeShadow(v2f_planeShadow i) : SV_Target
{
    float4 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
    clip(mainColor.a - 0.1);
	float3 posToPlane_2 = (i.xlv_TEXCOORD0 - i.xlv_TEXCOORD1);
	float4 color;
	color.xyz = _PlaneShadowColor.rgb;
	#if defined _TRANSPARENT
	_ShadowInvLen = _ShadowInvLen * (1 - _MainColor.a);
	#endif
	color.w = saturate(pow((1.0 - clamp(((sqrt(dot(posToPlane_2, posToPlane_2)) * _ShadowInvLen) - _ShadowFadeParams.x), 0.0, 1.0)), _ShadowFadeParams.y) * _ShadowFadeParams.z + 0.35);
	return color;
}