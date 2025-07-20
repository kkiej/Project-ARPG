/***********************************************************************************************
 ***                     B A R D S H A D E R  ---  B A R D  S T U D I O S                    ***
 ***********************************************************************************************
 *                                                                                             *
 *                                      Project Name : BARD                                    *
 *                                                                                             *
 *                               File Name : character_outline.hlsl                            *
 *                                                                                             *
 *                                    Programmer : Zhu Han                                     *
 *                                                                                             *
 *                                      Date : 2021/1/20                                       *
 *                                                                                             *
 *---------------------------------------------------------------------------------------------*/
#include "./bard_function.hlsl" 

struct outline_data 
{
	float4 vertex           							: POSITION;
	float3 normal           							: NORMAL;
	float4 tangent          							: TANGENT;
	float4 color            							: COLOR;
	float2 texcoord0        							: TEXCOORD0;
};

struct v2f_outline 
{
	float4 pos            								: SV_POSITION;
	float4 posWS            							: TEXCOORD0;
	float4 color            							: TEXCOORD1;
	float4 uv0              							: TEXCOORD2;
	float4 screenPos        							: TEXCOORD3;
	float4 PosL											: TEXCOORD4;
	float3 normal										: TEXCOORD5;
	float4 tangent										: TEXCOORD6;
	float4 SHADOW_COORDS								: TEXCOORD7;
};

v2f_outline vert_outline(outline_data v)
{
	v2f_outline o 										= (v2f_outline)0; 
	o.normal											= TransformObjectToWorldNormal(v.normal);
	real sign											= v.tangent.w * GetOddNegativeScale();
	o.tangent											= half4(TransformObjectToWorldDir(v.tangent.xyz).xyz, sign);
	
	// #if defined(UNITY_REVERSED_Z)
	// //v.2.0.4.2 (DX)
	// _Offset_Z = _Offset_Z * -0.01;
	// #else
	// //OpenGL
	// _Offset_Z = _Offset_Z * 0.01;
	// #endif
	
	o.posWS												= mul(unity_ObjectToWorld, v.vertex);
	o.SHADOW_COORDS										= TransformWorldToShadowCoord(o.posWS);
	SetObjectToClipPos(o.pos,v.vertex);
	//-------------    NDC  normal  --------------
	half3 ndcNormal 									= GET_NORMAL_IN_NDC(_UseSmoothNormal,v.tangent, v.normal, o.pos);
	//-------------    角色描边远近控制  --------------
	CHARACTER_OUTLINE_FAR_NEAR_SET(_OutlineWidth,_CameraFOV,v.vertex, ndcNormal, v.texcoord0, v.color, o.pos);
	//-------------    角色描边偏移  --------------
	VertexPosOffsetZ(o.pos.z);
	
	o.screenPos 										= ComputeScreenPos(o.pos);
	o.color 											= v.color;
	o.color.a 											= v.color.x;
	o.uv0.xy 											= v.texcoord0;
	o.uv0.w												= ComputeFogFactor(o.pos.z);
	o.PosL												= v.vertex;
	return o;
}

v2f_outline vert_stencilOutline(outline_data v)
{
	v2f_outline o 										= (v2f_outline)0;
	o.normal											= TransformObjectToWorldNormal(v.normal);
	o.posWS												= mul(unity_ObjectToWorld, v.vertex);
	SetObjectToClipPos(o.pos,v.vertex);
	//-------------    NDC  normal  --------------
	half3 ndcNormal 									= GET_NORMAL_IN_NDC(_UseSmoothNormal,v.tangent, v.normal, o.pos);
	//-------------    角色描边远近控制  --------------
	CHARACTER_STENCIL_OUTLINE_FAR_NEAR_SET( _StencilOutlineWidth, _CameraFOV,v.vertex, ndcNormal, v.color, o.pos);
	
	o.screenPos 										= ComputeScreenPos(o.pos);
	o.color 											= v.color;
	o.color.a 											= v.color.x;
	o.uv0.xy 											= v.texcoord0;
	o.PosL												= v.vertex;
	return o;
}

half4 frag_skin_outline(v2f_outline i) : COLOR
{
	half3 viewDir										= normalize(_WorldSpaceCameraPos.xyz - i.posWS.xyz);
	half4 finalColor 									= i.color;
	half skinmask 										= step(i.color.g, 0.01);
	finalColor.rgb 										= CHARACTER_OUTLINE_COLOR_SET(1, skinmask);

	half blendA                                 		= SAMPLE_TEXTURE2D(_BlendTex,sampler_BlendTex,i.uv0).a;
	blendA                                      		= clamp( pow( blendA, 0.7), 0, 1);
		
	clip(blendA - 0.3);
	
	clip(i.color.a - 0.01);

	//-------------  进入阴影区域  --------------
	// SetCharacterSenceShadow(finalColor, MainLightRealtimeShadow(i.SHADOW_COORDS),finalColor * half3(0.5,0.5,0.5));
	SetCharacterSenceShadow(GetCharacterSenceShadow(), finalColor, finalColor * half3(0.5,0.5,0.5),_SpecFalloff);
	//-------------  角色点阵化  --------------T
	CHARACTER_MASK_TRANSPARENT_SET(_UseTransparentMask,_Transparency,i.screenPos);
	//-------------  获取光照颜色  --------------
	GET_CHARACTER_LIGHT_COLOR(_UseAddColor,_LightColor,_ToonLightStrength,_AddLightColor,finalColor);
	//-------------  角色白平衡  --------------
	SetCharacterWhiteBalance(_UseWhiteBalance,_kRGB,finalColor);
	//-------------  角色冰冻  ---------------
	SetCharacterIce(_UseIce,i.normal, i.tangent, _DirectionLightPos, viewDir,i.uv0,finalColor);
	//-------------  角色溶解  消散 --------------
	UNITY_BRANCH
	if(_UseDissolve > 0.5)
	{
		clip(blendA - 2);
	}

	//---------- 获取自发光 ----------
	WEAPON_DISSOLVE_SET(_UseDissolveCut,_DissolveCut, _DissolveWidth, _DissolveColor,i.color, finalColor);
	
	#if defined ROLE_POSW_DISSOLVE
	// half mask = tex2D(_NoiseTex,TRANSFORM_TEX(i.uv0 * half2(5,5),_NoiseTex)).r;
	// half height = i.PosL.z - mask* _PosWNoiseScale;
	// half show = step(_PosWDissolveScale,height);
	// show = (1 - show) * _Top2Bottom + (1 - _Top2Bottom) * show;
	clip(blendA - 2);
	#endif

	//---------- 获取自发光 ----------
	WEAPON_DISSOLVE_SET(_UseDissolveCut,_DissolveCut, _DissolveWidth, _DissolveColor,i.color, finalColor);
	
	#if defined(_TRANSPARENT)
	SetTransparent(i.screenPos,finalColor);
	#endif

	//---------- 距离雾 ----------
		
	SetCharacterUnityFog(i.uv0.w, finalColor);
	
	return half4(finalColor.rgb , 1);
}

half4 frag_outline(v2f_outline i) : COLOR
{
	half3 viewDir										= normalize(_WorldSpaceCameraPos.xyz - i.posWS.xyz);
	half4 finalColor 									= half4(CHARACTER_OUTLINE_COLOR_SET(0, 0), 1);

	half blendA                                 		= SAMPLE_TEXTURE2D(_BlendTex,sampler_BlendTex,i.uv0).a;
	blendA                                      		= clamp( pow( blendA, 0.7), 0, 1);
		
	clip(blendA - 0.3);
	
	clip(i.color.a - 0.01);

	//-------------  进入阴影区域  --------------
	// SetCharacterSenceShadow(finalColor,MainLightRealtimeShadow(i.SHADOW_COORDS),finalColor * half3(0.5,0.5,0.5));
	SetCharacterSenceShadow(GetCharacterSenceShadow(), finalColor,finalColor * half3(0.5,0.5,0.5),_SpecFalloff);
	//-------------  角色点阵化  --------------
	CHARACTER_MASK_TRANSPARENT_SET(_UseTransparentMask,_Transparency,i.screenPos);
	//-------------  获取光照颜色  --------------
	GET_CHARACTER_LIGHT_COLOR(_UseAddColor,_LightColor,_ToonLightStrength,_AddLightColor,finalColor);
	//-------------  角色白平衡  --------------
	SetCharacterWhiteBalance(_UseWhiteBalance,_kRGB,finalColor);
	//-------------  角色冰冻  ---------------
	SetCharacterIce(_UseIce,i.normal, i.tangent, _DirectionLightPos, normalize(_WorldSpaceCameraPos.xyz - i.posWS.xyz),i.uv0,finalColor);
	//-------------  角色溶解  消散 --------------
	UNITY_BRANCH
	if(_UseDissolve > 0.5)
	{
		clip(blendA - 2);
	}

	//---------- 获取自发光 ----------
	WEAPON_DISSOLVE_SET(_UseDissolveCut,_DissolveCut, _DissolveWidth, _DissolveColor,i.color, finalColor);
	
	#if defined ROLE_POSW_DISSOLVE
	// half mask = tex2D(_NoiseTex,TRANSFORM_TEX(i.uv0 * half2(5,5),_NoiseTex)).r;
	// half height = i.PosL.z - mask* _PosWNoiseScale;
	// half show = step(_PosWDissolveScale,height);
	// show = (1 - show) * _Top2Bottom + (1 - _Top2Bottom) * show;
	clip(blendA - 2);
	#endif

	#if defined(_TRANSPARENT)
	SetTransparent(i.screenPos,finalColor);
	#endif

	//---------- 距离雾 ----------
		
	SetCharacterUnityFog(i.uv0.w, finalColor);
	
	return half4(finalColor.rgb, 1);
}

half4 frag_simpleOutline(v2f_outline i) : COLOR
{
	half4 finalColor 									= half4(CHARACTER_OUTLINE_COLOR_SET(0, 0), 1);
	
	clip(i.color.a - 0.01);
	//-------------  角色点阵化  --------------T
	CHARACTER_MASK_TRANSPARENT_SET(_UseTransparentMask,_Transparency,i.screenPos);
	//-------------  进入阴影区域  --------------
	// SetCharacterSenceShadow(finalColor,finalColor * half3(0.5,0.5,0.5),MainLightRealtimeShadow(i.SHADOW_COORDS));
	SetCharacterSenceShadow(GetCharacterSenceShadow(), finalColor,finalColor * half3(0.5,0.5,0.5),_SpecFalloff);
	//-------------  获取光照颜色  --------------
	GET_CHARACTER_LIGHT_COLOR(_UseAddColor,_LightColor,_ToonLightStrength,_AddLightColor,finalColor);
	//-------------  角色白平衡  --------------
	SetCharacterWhiteBalance(_UseWhiteBalance,_kRGB,finalColor);
	//-------------  角色冰冻  ---------------
	SetCharacterIce(_UseIce,i.normal, i.tangent, _DirectionLightPos, normalize(_WorldSpaceCameraPos.xyz - i.posWS.xyz),i.uv0,finalColor);
	//-------------  角色溶解  消散 --------------
	UNITY_BRANCH
	if(_UseDissolve > 0.5)
	{
		clip(i.color.a - 2);
	}

	#if defined(_TRANSPARENT)
	SetTransparent(i.screenPos,finalColor);
	#endif

	//---------- 距离雾 ----------
		
	SetCharacterUnityFog(i.uv0.w, finalColor);
	
	return half4(finalColor.rgb, 1);
}

half4 frag_stencilOutline(v2f_outline i) : COLOR
{
	half4 finalColor 									= _StencilOutLineColor;
	
	//-------------  角色点阵化  --------------T
	CHARACTER_MASK_TRANSPARENT_SET(_UseTransparentMask,_Transparency,i.screenPos);
	
	i.screenPos.xy										= i.screenPos.xy / i.screenPos.w;

	//	描边流动取消
	// half mask											= saturate(tex2D(_NoiseTex,half2(i.screenPos.x * 10 + _Time.y * 0.5, i.screenPos.y * 10 + _Time.y * 0.5)).r);
	// mask												= step(mask,0.6);
	// clip(mask - 0.5);

	half blendA                                 		= SAMPLE_TEXTURE2D(_BlendTex,sampler_BlendTex,i.uv0).a;
	blendA                                      		= clamp( pow( blendA, 0.7), 0, 1);
		
	clip(blendA - 0.3);
	
	clip(i.color.a - 0.01);

	#if defined(_TRANSPARENT)
	SetTransparent(i.screenPos,finalColor);
	#endif
	
	return half4(finalColor.rgb, 1);
}