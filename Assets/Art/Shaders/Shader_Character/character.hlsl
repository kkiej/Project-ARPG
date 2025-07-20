	/***********************************************************************************************
	***                     B A R D S H A D E R  ---  B A R D  S T U D I O S                     ***
	***********************************************************************************************
	*                                                                                             *
	*                                      Project Name : BARD                                    *
	*                                                                                             *
	*									File Name : character.hlsl								  *
	*                                                                                             *
	*                                    Programmer : Zhu Han                                     *
	*                                                                                             *
	*                                      Date : 2020/8/20                                       *
	*                                                                                             *
	* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
	//#include <UnityShaderVariables.cginc>

	#include "./bard_function.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

	/// <summary>
	/// BODY
	/// </summary>
	struct VertexIn_Body 
	{
		float4 PosL           															: POSITION;
		float3 NormalL           														: NORMAL;
		float4 TangentL																	: TANGENT;
		float4 COLOR            														: COLOR;
		float2 TexC        																: TEXCOORD0;
	};

	struct VertexOut_Body 
	{
		float4 PosH              														: SV_POSITION;
		float3 NormalW        															: TEXCOORD0;
		float4 TangentW																	: TEXCOORD1;
		float4 TexC              														: TEXCOORD2;
		float3 PosW         															: TEXCOORD3;
		float4 PosS        																: TEXCOORD4;
		float4 PosL																		: TEXCOORD5;
		float4 COLOR           															: TEXCOORD6;
		float4 TargetPosSS																: TEXCOORD7;
		float4 SHADOW_COORDS															: TEXCOORD8;
	};

	VertexOut_Body VS_Body(VertexIn_Body vin) 
	{
		VertexOut_Body vout																= (VertexOut_Body)0;

		
		// 顶点参数设置
	
		vout.TexC.xy 																	= vin.TexC;
		vout.NormalW																	= TransformObjectToWorldNormal(vin.NormalL);
		real sign																		= vin.TangentL.w * GetOddNegativeScale();
		vout.TangentW																	= half4(TransformObjectToWorldDir(vin.TangentL.xyz).xyz, sign);
		vout.PosW 																		= TransformObjectToWorld(vin.PosL);
		vout.SHADOW_COORDS																= TransformWorldToShadowCoord(vout.PosW);
		vout.PosL																		= vin.PosL;
		GetCharacterHalfLambert(_ToonLightDirection, _UseCameraLight, _DirectionLightPos,vout.NormalW,vout.TexC.z);					
		//GetCharacterHalfLambert(_ToonLightDirection, _UseCameraLight, _WorldSpaceLightPos0,vout.NormalW,vout.TexC.z);					
		SetObjectToClipPos(vout.PosH,vin.PosL);

		vout.TexC.w																		= ComputeFogFactor(vout.PosH.z);
		vout.PosS 																		= ComputeScreenPos(vout.PosH);
		vout.COLOR 																		= vin.COLOR;
		
		SetCharacterTargetPosSS(_SSRimScale,vout.COLOR.b,vout.PosW,vout.TargetPosSS);
		
		return vout;
	}

	half4 PS_Body(VertexOut_Body pin) : SV_Target
	{

		//---------- 角色点阵化 ----------
		
		CHARACTER_MASK_TRANSPARENT_SET(_UseTransparentMask,_Transparency,pin.PosS);

		
		//---------- 获取各种向量 ----------
		
		half3	N,L,V,L_World;
		GetCharacterNormalizeDir( _DirectionLightPos, _ToonLightDirection, _UseCameraLight,pin.NormalW, pin.PosW, N, L, L_World, V);

		half NoL_World																	= GetNdotV(pin.NormalW,L_World);
		half NoV 																		= GetNdotV(V,N);
		half NoL 																		= pin.TexC.z;
		half VoL 																		= GetVdotL(V, L);

		half3 H 																		= GetHalfDir(V, L);
		half NoH 																		= GetNdotH(N,H);
		half RoL 																		= GetRoL(NoL,NoV,VoL);


		//---------- 需要用到的各种参数 ----------
		half stepMask, ShadowRange, Set_ShadowMask, RampShadowArea, DefaultShadowArea, StayLightArea, StepCount, Set_RimLightMask, Metalic, Stocking;
		half3 SpecCommon, SpecMetal, SpecMetalSH, defaultShadowColor, rampShadowColor;
		half4 outColor, mainColor, celColor;
		
		CharacterBaseParamesSetBody(pin.TexC.xy, outColor, mainColor, celColor, stepMask, Metalic, Stocking);

		//---------- 黑白闪 ----------
		
		#if defined BLACK_WHITE_ON
		return FLICKER_EFFECT_SET( _DirectionLightPos, _FlickerShadowRange, _FlickerFresnelRange,N, V);
		#endif
		
		
		//---------- 阴影参数 ----------
		
		CharacterShadowParamesSet(_UseCameraLight,_ShadowRangeStep,_ShadowFeather,celColor.g, NoL, NoL_World, ShadowRange, Set_ShadowMask, RampShadowArea, DefaultShadowArea, defaultShadowColor, StayLightArea , rampShadowColor, StepCount);
		// GetCharacterPCFShadow(N, L, pin.PosWS, pin.uv0, realTimeShadow,Set_ShadowMask);
		GET_SHADOW_COLOR(stepMask, Set_ShadowMask, rampShadowColor, defaultShadowColor);
		
		//---------- 丝袜设置 ----------
		SetCharacterStocking(Stocking, NoV, Set_ShadowMask, pin.TexC.xy, outColor);
		
		//---------- 阴影设置 ----------
		
		DEFAULT_SHADOW_COLOR_SET(_defaultShadowStrength,Set_ShadowMask, defaultShadowColor);
		CHARACTER_SHADOW_SET(defaultShadowColor, rampShadowColor, DefaultShadowArea, outColor);
		
		STAY_LIGHT_SET(outColor, StayLightArea, mainColor);

		//---------- 进入阴影 ------
		// SetCharacterSenceShadow(outColor,MainLightRealtimeShadow(pin.SHADOW_COORDS),defaultShadowColor * mainColor);
		half shadow = GetCharacterSenceShadow();
		SetCharacterSenceShadow(shadow, outColor,defaultShadowColor * mainColor, _SpecFalloff);
		
		//---------- 获取边缘光 ----------
		
		GET_RIM_LIGHT_FACTORY(_RimRangeStep, _RimFeather,NoV, pin.PosW, Set_RimLightMask);
		//SCREEN_SPACE_DEPTH_RIM_SET(pin.PosS, pin.TargetPosSS, Set_RimLightMask);
		RIM_LIGHT_SET(pin.COLOR.b, Set_ShadowMask, _RimColor * (1 - Set_RimLightMask) * saturate(shadow + 0.5), outColor);
		
		//---------- 获取高光 ----------
		
		SPEC_COMMON_SET(_SpecFalloff,celColor, Metalic, NoH, SpecCommon.rgb);
		SPEC_METAL_SET(_Shiness,_SpecStep,_CheckLine,celColor, RoL, N, H, NoH,V, SpecMetal.rgb,SpecMetalSH.rgb);
		GET_CHARACTER_SPEC(_MetalScale,Metalic,SpecCommon.rgb, SpecMetal.rgb, SpecMetalSH.rgb, mainColor, Set_ShadowMask, outColor);
		
		
		//---------- 获取自发光 ----------
		
		GET_CHARACTER_EMISSION(_EmissionStrength,mainColor, pin.TexC.xy,outColor);
		WEAPON_DISSOLVE_SET(_UseDissolveCut,_DissolveCut, _DissolveWidth, _DissolveColor,pin.COLOR, outColor);
		
		
		//---------- 额外菲涅尔 ----------
		
		FRESNEL_SET(_UseAdditionFresnel, _FresnelRange, _FresnelFeather, _FresnelColor, _FresnelFlowRange, NoV, pin.TexC, pin.COLOR.g, outColor);
		
		
		//---------- 获取光照颜色 ----------
		
		GET_CHARACTER_LIGHT_COLOR(_UseAddColor,_LightColor,_ToonLightStrength,_AddLightColor,outColor);
		
		
		//---------- 角色白平衡 ----------
		
		SetCharacterWhiteBalance(_UseWhiteBalance,_kRGB,outColor);
		
		
		//---------- 获取多光源 ----------
		
		Get_CHARACTER_ADDLIGHTS(_UseAddLight, _addShadowRangeStep,_addShadowFeather,N,pin.PosW,mainColor,outColor);
	
		//---------- 常驻假光源----------
		// Get_CHARACTER_ROLE_SPOT_LIGHT(N,pin.PosW,outColor);
		
		//---------- 设置材质HSV ----------
		
		SetCharacterHSV(_Hue,_Saturation,_Value,outColor);
		
		//---------- 角色冰冻效果 ----------
		
		SetCharacterIce(_UseIce,pin.NormalW,pin.TangentW, L, V, pin.TexC, outColor);
		
		//---------- 角色死亡溶解 ----------
		
		CHARACTER_DISSOLVE_SET(_UseDissolve,_DissolveScale,_EdgeWidth,_EdgeColor,pin.PosS, outColor);
		
		//---------- 角色纵向溶解 ----------
		
		SetCharacterPosWDissolve(pin.COLOR,pin.TexC,outColor);
		
		//---------- 半透明处理 ----------
		
		#if defined(_TRANSPARENT)
		SetTransparent(pin.PosS, outColor, mainColor.a);
		#endif
		
		//---------- 距离雾 ----------
		
		SetCharacterUnityFog(pin.TexC.w, outColor);
		
		return half4(outColor.rgb,1);
	}




	/// <summary>
	/// FACE
	/// </summary>
	struct VertexIn_Face
	{
		float4 PosL           														: POSITION;
		float3 NormalL           													: NORMAL;
		float4 TangentL																: TANGENT;
		float4 COLOR            													: COLOR;
		float2 TexC        															: TEXCOORD0;
	};

	struct VertexOut_Face 
	{
		float4 PosH              													: SV_POSITION;
		float3 NormalW        														: TEXCOORD0;
		float4 TangentW																: TEXCOORD1;
		float4 TexC              													: TEXCOORD2;
		float4 PosW         														: TEXCOORD3;
		float4 PosS        															: TEXCOORD4;
		float4 PosL																	: TEXCOORD5;
		float4 COLOR           														: TEXCOORD6;
		float4 SHADOW_COORDS														: TEXCOORD7;
	};

	VertexOut_Face VS_Face(VertexIn_Face vin) 
	{
		VertexOut_Face vout															= (VertexOut_Face)0;

		
		/// 顶点参数设置								
		vout.TexC.xy 																= vin.TexC;
		vout.NormalW																= TransformObjectToWorldNormal(vin.NormalL);
		real sign																	= vin.TangentL.w * GetOddNegativeScale();
		vout.TangentW																= half4(TransformObjectToWorldDir(vin.TangentL.xyz).xyz, sign);
		vout.PosW 																	= mul(unity_ObjectToWorld, vin.PosL);
		vout.SHADOW_COORDS															= TransformWorldToShadowCoord(vout.PosW);
		vout.PosL																	= vin.PosL;
		GetCharacterHalfLambert(_ToonLightDirection, _UseCameraLight, _DirectionLightPos,vout.NormalW,vout.TexC.z);
		SetObjectToClipPos(vout.PosH,vin.PosL);

		vout.TexC.w																	= ComputeFogFactor(vout.PosH.z);
		vout.PosS 																	= ComputeScreenPos(vout.PosH);
		vout.COLOR 																	= vin.COLOR;

		
		return vout;
	}

	half4 PS_Face(VertexOut_Face pin) : SV_Target
	{
		
		//---------- 角色点阵化 ----------
		CHARACTER_MASK_TRANSPARENT_SET(_UseTransparentMask,_Transparency,pin.PosS);


		//---------- 需要用到的各种参数 ----------

		half sdfShadow;
		half4 outColor, mainColor, celColor;
		half3 N, L, L_World, V, faceNoL, faceLightVectore, faceForwardVector;

		GetCharacterNormalizeDir(_DirectionLightPos, _ToonLightDirection, _UseCameraLight,pin.NormalW, pin.PosW, N, L, L_World,V);
		CHARACTER_FACEDIR_SET(_FaceForwardVector,faceLightVectore, faceForwardVector, faceNoL, L);
		CharacterBaseParamesSetFace(pin.TexC.xy, outColor, mainColor, celColor);


		//---------- 黑白闪 ----------

		#if defined BLACK_WHITE_ON
		return FLICKER_EFFECT_SET(_DirectionLightPos, _FlickerShadowRange, _FlickerFresnelRange,N, V);
		#endif


		//---------- 阴影设置 ----------
		CHARACTER_FACE_SDF_SET(_UseCameraLight, pin.TexC.xy, faceForwardVector, faceLightVectore, faceNoL, sdfShadow);
		CHARACTER_FACE_SHADOW_SET(_EyeShadowColor,_ShadowColor,_defaultShadowStrength, mainColor, sdfShadow, pin.COLOR, outColor);
		// CHARACTER_HAIR_SHADOW_SET(pin.depth,pin.PosWS, pin.PosSS, mainColor,outColor);

		//---------- 进入阴影区域 ------
		// SetCharacterFaceSenceShadow(outColor, _ShadowColor * mainColor, _EyeShadowColor * mainColor, pin.COLOR, MainLightRealtimeShadow(pin.SHADOW_COORDS)); 
		SetCharacterFaceSenceShadow(outColor, _ShadowColor * mainColor, _EyeShadowColor * mainColor, pin.COLOR);

		
		//---------- 获取光照颜色 ----------

		GET_CHARACTER_LIGHT_COLOR(_UseAddColor,_LightColor,_ToonLightStrength,_AddLightColor,outColor);


		//---------- 角色白平衡 ----------

		SetCharacterWhiteBalance(_UseWhiteBalance,_kRGB,outColor);
		
		//---------- 额外菲涅尔 ----------
		
		FRESNEL_SET(_UseAdditionFresnel, _FresnelRange, _FresnelFeather, _FresnelColor, _FresnelFlowRange, saturate(dot(N,V)), pin.TexC, pin.COLOR.g, outColor);
		
		//---------- 获取多光源 ----------

		Get_CHARACTER_ADDLIGHTS(_UseAddLight, _addShadowRangeStep, _addShadowFeather,N,pin.PosW, mainColor, outColor);

		//---------- 常驻假光源----------
		// Get_CHARACTER_ROLE_SPOT_LIGHT(N,pin.PosW,outColor);
		
		//---------- 设置材质HSV ----------
		
		SetCharacterHSV(_Hue,_Saturation,_Value,outColor);
		
		
		//---------- 角色冰冻效果 ----------

		SetCharacterIce(_UseIce,pin.NormalW,pin.TangentW, L, V, pin.TexC, outColor);
		
		//---------- 角色死亡溶解 ----------

		CHARACTER_DISSOLVE_SET(_UseDissolve,_DissolveScale,_EdgeWidth,_EdgeColor,pin.PosS, outColor);


		//---------- 角色纵向溶解 ----------
		
		SetCharacterPosWDissolve(pin.PosL,pin.TexC,outColor);

		IceQuadDissolve(pin.NormalW,pin.TangentW, L, V, pin.TexC,outColor);

		//---------- 半透明处理 ----------
		
		#if defined(_TRANSPARENT)
		SetTransparent(pin.PosS,outColor);
		#endif

		//---------- 距离雾 ----------
		
		SetCharacterUnityFog(pin.TexC.w, outColor);
		
		return half4(outColor.rgb,1);
	}





	/// <summary>
	/// HAIR
	/// </summary>
	struct VertexIn_Hair 
	{
		float4 PosL           															: POSITION;
		float3 NormalL           														: NORMAL;
		float4 TangentL																	: TANGENT;
		float4 COLOR            														: COLOR;
		float2 TexC        																: TEXCOORD0;
	};

	struct VertexOut_Hair
	{
		float4 PosH              														: SV_POSITION;
		float3 NormalW        															: TEXCOORD0;
		float4 TangentW																	: TEXCOORD1;
		float4 TexC              														: TEXCOORD2;
		float3 PosW         															: TEXCOORD3;
		float4 PosS        																: TEXCOORD4;
		float4 PosL																		: TEXCOORD5;
		float4 COLOR           															: TEXCOORD6;
		float4 TargetPosSS																: TEXCOORD7;
		float4 SHADOW_COORDS															: TEXCOORD8;
	};

	VertexOut_Hair VS_Hair(VertexIn_Hair vin) 
	{
		VertexOut_Hair vout																= (VertexOut_Hair)0;

		
		/// 顶点参数设置
		vout.TexC.xy 																	= vin.TexC;
		vout.NormalW																	= TransformObjectToWorldNormal(vin.NormalL);
		real sign																		= vin.TangentL.w * GetOddNegativeScale();
		vout.TangentW																	= half4(TransformObjectToWorldDir(vin.TangentL.xyz).xyz, sign);
		vout.PosW 																		= TransformObjectToWorld(vin.PosL);
		vout.SHADOW_COORDS																= TransformWorldToShadowCoord(vout.PosW);
		GetCharacterHalfLambert(_ToonLightDirection, _UseCameraLight, _DirectionLightPos,vout.NormalW,vout.TexC.z);
		SetObjectToClipPos(vout.PosH,vin.PosL);
		
		vout.TexC.w																		= ComputeFogFactor(vout.PosH.z);
		vout.PosS 																		= ComputeScreenPos(vout.PosH);
		vout.COLOR 																		= vin.COLOR;
		
		SetCharacterTargetPosSS(_SSRimScale,vout.COLOR.b,vout.PosW,vout.TargetPosSS);
		return vout;
	}

	half4 PS_Hair(VertexOut_Hair pin) : SV_Target
	{

		//---------- 角色点阵化 ----------
		CHARACTER_MASK_TRANSPARENT_SET(_UseTransparentMask,_Transparency,pin.PosS);

		
		//---------- 获取各种向量 ----------
		
		half NoH, NoL_World, NoV, NoL;
		half3 N,L,V,L_World;
		
		GetCharacterNormalizeDir(_DirectionLightPos, _ToonLightDirection, _UseCameraLight,pin.NormalW, pin.PosW, N, L, L_World,V);

		NoL_World																		= GetNdotV(pin.NormalW,L_World);
		NoV 																			= GetNdotV(V,N);
		NoL 																			= pin.TexC.z;
		half3 H																			= GetHalfDir(V, L);

		NoH 																			= GetNdotH(N,H);

		
		//---------- 需要用到的各种参数 ----------
		
		half stepMask,ShadowRange,Set_ShadowMask,RampShadowArea,DefaultShadowArea,StayLightArea,StepCount,Set_RimLightMask,Metalic,Sock;
		half4 outColor, mainColor, celColor;
		half3 rampShadowColor, rampRimColor,defaultShadowColor;

		CharacterBaseParamesSetBody(pin.TexC.xy, outColor, mainColor, celColor, stepMask, Metalic, Sock);


		//---------- 黑白闪 ----------
		
		#if defined BLACK_WHITE_ON
			return FLICKER_EFFECT_SET(_DirectionLightPos, _FlickerShadowRange, _FlickerFresnelRange,N, V);
		#endif
		
		
		//---------- 阴影设置 ----------
		
		CharacterShadowParamesSet(_UseCameraLight,_ShadowRangeStep,_ShadowFeather,celColor.g, NoL, NoL_World, ShadowRange, Set_ShadowMask, RampShadowArea, DefaultShadowArea, defaultShadowColor, StayLightArea , rampShadowColor.rgb, StepCount);
		GET_SHADOW_COLOR(stepMask, Set_ShadowMask, rampShadowColor, defaultShadowColor);
		DEFAULT_SHADOW_COLOR_SET(_defaultShadowStrength,Set_ShadowMask, defaultShadowColor);
		CHARACTER_SHADOW_SET(defaultShadowColor, rampShadowColor, DefaultShadowArea, outColor);
		// STAY_LIGHT_SET(outColor, StayLightArea, mainColor);

		//---------- 进入阴影区域 ------
		half shadow = GetCharacterSenceShadow();
		SetCharacterSenceShadow(shadow, outColor,defaultShadowColor * mainColor, _SpecFalloff);
		// SetCharacterSenceShadow(outColor,MainLightRealtimeShadow(pin.SHADOW_COORDS),defaultShadowColor * mainColor);

		
		//---------- 获取边缘光 ----------
		
		GET_RIM_LIGHT_FACTORY(_RimRangeStep, _RimFeather,NoV, pin.PosW,Set_RimLightMask);
		//SCREEN_SPACE_DEPTH_RIM_SET(pin.PosS, pin.TargetPosSS, Set_RimLightMask);
		RIM_LIGHT_SET(pin.COLOR.b,Set_ShadowMask, _RimColor * (1 - Set_RimLightMask) * saturate(shadow + 0.5), outColor);
		
		
		//---------- 获取高光 ----------
		
		SPEC_HAIR_SET(_SpecFalloff, celColor, NoH, Set_ShadowMask,outColor);
		
		
		//---------- 获取光照颜色 ----------
		
		GET_CHARACTER_LIGHT_COLOR(_UseAddColor,_LightColor,_ToonLightStrength,_AddLightColor,outColor);
		
		
		//---------- 角色白平衡 ----------
		
		SetCharacterWhiteBalance(_UseWhiteBalance,_kRGB,outColor);
		
		//---------- 额外菲涅尔 ----------
		
		FRESNEL_SET(_UseAdditionFresnel, _FresnelRange, _FresnelFeather, _FresnelColor, _FresnelFlowRange, NoV, pin.TexC, pin.COLOR.g, outColor);
		
		//---------- 获取多光源 ----------
		
		Get_CHARACTER_ADDLIGHTS(_UseAddLight, _addShadowRangeStep, _addShadowFeather,N,pin.PosW,mainColor,outColor);

		//---------- 常驻假光源----------
		// Get_CHARACTER_ROLE_SPOT_LIGHT(N,pin.PosW,outColor);
		
		//---------- 设置材质HSV ----------
		
		SetCharacterHSV(_Hue,_Saturation,_Value,outColor);
		
		
		//---------- 角色冰冻效果 ----------
		
		SetCharacterIce(_UseIce,pin.NormalW,pin.TangentW, L, V, pin.TexC, outColor);
		
		//---------- 角色死亡溶解 ----------
		
		CHARACTER_DISSOLVE_SET(_UseDissolve,_DissolveScale,_EdgeWidth,_EdgeColor,pin.PosS, outColor);
		
		
		//---------- 角色纵向溶解 ----------
		
		SetCharacterPosWDissolve(pin.PosL,pin.TexC,outColor);

		//---------- 半透明处理 ----------
		
		#if defined(_TRANSPARENT)
		SetTransparent(pin.PosS,outColor);
		#endif

		//---------- 距离雾 ----------
		
		SetCharacterUnityFog(pin.TexC.w, outColor);
		
		return half4(outColor.rgb,1);
	}



	/// <summary>
	/// METAL
	/// </summary>
	struct Metal_data 
	{
		float4 PosL		           															: POSITION;
		float3 NormalL           															: NORMAL;
		float4 COLOR            															: COLOR;
		float2 TexC        																	: TEXCOORD0;
	};

	struct v2f_Metal
	{
		float4 PosH              															: SV_POSITION;
		float3 NormalW        																: TEXCOORD0;
		float4 TexC              															: TEXCOORD1;
		float4 PosW         																: TEXCOORD2;
		float4 COLOR           																: TEXCOORD3;
	}; 

	v2f_Metal vert_Metal(Metal_data vin) 
	{
		v2f_Metal vout																		= (v2f_Metal)0;

		
		/// 顶点参数设置
		vout.TexC.xy 																		= vin.TexC;
		vout.NormalW																		= TransformObjectToWorldNormal(vin.NormalL);
		vout.PosW 																			= mul(unity_ObjectToWorld, vin.PosL);

		GetCharacterHalfLambert(_ToonLightDirection, _UseCameraLight, _DirectionLightPos,vout.NormalW,vout.TexC.z);
		SetObjectToClipPos(vout.PosH,vin.PosL);
		
		vout.COLOR 																			= vin.COLOR;

		return vout;
	}

	half4 frag_Metal(v2f_Metal pin) : SV_Target
	{
		half3 N,L,V,L_World;
		
		GetCharacterNormalizeDir(_DirectionLightPos, _ToonLightDirection, _UseCameraLight,pin.NormalW, pin.PosW, N, L, L_World,V);

		half NoL_World,NoV,NoL,VoL,NoH,RoL;

		
		NoL_World																			= GetNdotV(pin.NormalW,L_World);
		NoV 																				= GetNdotV(V,N);
		NoL 																				= pin.TexC.z;
		VoL 																				= GetVdotL(V, L);

		half3 H 																			= GetHalfDir(V, L);
		NoH 																				= GetNdotH(N,H);
		RoL 																				= GetRoL(NoL, NoV,VoL);

		half stepMask,ShadowRange,Set_ShadowMask,RampShadowArea,DefaultShadowArea,StayLightArea,StepCount,colorBlendMask,Metalic,Sock;
		half4 outColor, mainColor, celColor;
		half3 defaultShadowColor, rampShadowColor, SpecCommon, SpecMetal, SpecMetalSH;

		CharacterBaseParamesSetBody(pin.TexC.xy, outColor, mainColor, celColor, stepMask, Metalic, Sock);

		CharacterShadowParamesSet(_UseCameraLight,_ShadowRangeStep,_ShadowFeather,celColor.g, NoL, NoL_World, ShadowRange, Set_ShadowMask, RampShadowArea, DefaultShadowArea, defaultShadowColor, StayLightArea , rampShadowColor.rgb, StepCount);
		GET_SHADOW_COLOR(stepMask, Set_ShadowMask, rampShadowColor, defaultShadowColor);
		DEFAULT_SHADOW_COLOR_SET(_defaultShadowStrength,Set_ShadowMask, defaultShadowColor);
		CHARACTER_SHADOW_SET(defaultShadowColor, rampShadowColor, DefaultShadowArea, outColor);
		STAY_LIGHT_SET(outColor, StayLightArea, mainColor);
		
		
		SPEC_COMMON_SET(_SpecFalloff,celColor, Metalic, NoH, SpecCommon.rgb);
		SPEC_METAL_SET(_Shiness,_SpecStep,_CheckLine,celColor, RoL, N, H, NoH,V, SpecMetal.rgb,SpecMetalSH.rgb);
		GET_CHARACTER_SPEC(_MetalScale,Metalic,SpecCommon.rgb, SpecMetal.rgb, SpecMetalSH.rgb, mainColor, 0, outColor);

		GET_WEAPON_FLOWLIGHT(_AddObjectY, _FirstShadowSet, _FirstShadowFeather, _FlowColor,pin.TexC.xy, pin.PosW, outColor);

		return outColor;
	}





	/// <summary>
	/// TRANSPARENT
	/// </summary>
	struct VertexIn_Transparent
	{
		float4 vertex           														: POSITION;
		float2 texcoord0        														: TEXCOORD0;
		float3 normal           														: NORMAL;
		float4 tangent																	: TANGENT;
	};

	struct VertexOut_Transparent
	{
		float4 pos              														: SV_POSITION;
		float4 uv0              														: TEXCOORD0;
		float4 screenPos        														: TEXCOORD1;
		float3 normal           														: TEXCOORD2;
		float4 tangent																	: TEXCOORD3;
		float3 posWS																	: TEXCOORD4;
		float4 SHADOW_COORDS															: TEXCOORD5;
	}; 

	VertexOut_Transparent VS_Transparent(VertexIn_Transparent vin)
	{
		VertexOut_Transparent vout = (VertexOut_Transparent)0;
		vout.uv0.xy 																	= vin.texcoord0;
		vout.pos 																		= TransformObjectToHClip(vin.vertex);
		vout.uv0.w																		= ComputeFogFactor(vout.pos.z);
		vout.screenPos 																	= ComputeScreenPos(vout.pos);
		vout.normal 																	= TransformObjectToWorldNormal(vin.normal);
		real sign																		= vin.tangent.w * GetOddNegativeScale();
		vout.tangent																	= half4(TransformObjectToWorldDir(vin.tangent.xyz).xyz, sign);
		vout.posWS																		= TransformObjectToWorld(vin.vertex);
		vout.SHADOW_COORDS																= TransformWorldToShadowCoord(vout.posWS);
		return vout;
	}

	half4 PS_Transparent(VertexOut_Transparent pin) : SV_Target
	{
		CHARACTER_MASK_TRANSPARENT_SET(_UseTransparentMask,_Transparency,pin.screenPos);

		half4 mainColor 																= SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,pin.uv0);
		half4 outColor;
		half3 V																			= normalize(_WorldSpaceCameraPos.xyz - pin.posWS.xyz);
		#if defined BLACK_WHITE_ON
			return FLICKER_EFFECT_SET(_DirectionLightPos, _FlickerShadowRange, _FlickerFresnelRange,pin.normal, V);
		#endif
		#if defined(_TRANSPARENT)
		half alpha																		= SAMPLE_TEXTURE2D(_TransparentTex,sampler_TransparentTex,pin.uv0).r;
		half4 opaqueColor																= SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,half2(pin.screenPos.xy / pin.screenPos.w));
		
		outColor																		= half4(lerp(opaqueColor.rgb,mainColor.rgb,alpha),1);
		#else
		outColor																		= mainColor;
		#endif
		GET_CHARACTER_LIGHT_COLOR(_UseAddColor,_LightColor,_ToonLightStrength,_AddLightColor,outColor);

		//------------  额外菲涅尔  ---------------
		FRESNEL_SET( _UseAdditionFresnel, _FresnelRange, _FresnelFeather, _FresnelColor, _FresnelFlowRange,saturate(dot(pin.normal,V)), pin.uv0, 1, outColor);
		
		SetCharacterWhiteBalance(_UseWhiteBalance,_kRGB,outColor);
		SetCharacterIce(_UseIce,pin.normal, pin.tangent, _DirectionLightPos, V,pin.uv0,outColor);
		CHARACTER_DISSOLVE_SET(_UseDissolve,_DissolveScale,_EdgeWidth,_EdgeColor,pin.screenPos, outColor);

		//---------- 距离雾 ----------
		
		SetCharacterUnityFog(pin.uv0.w, outColor);
		
		return outColor;
	}

	/// <summary>
	/// SIMPLE
	/// </summary>
	struct VertexIn_Simple
	{
		float4 vertex           														: POSITION;
		float3 normal           														: NORMAL;
		float4 tangent																	: TANGENT;
		float4 color            														: COLOR;
		float2 texcoord0        														: TEXCOORD0;
	};

	struct VertexOut_Simple
	{
		float4 pos              														: SV_POSITION;
		float4 uv0              														: TEXCOORD0;
		float3 normalDir        														: TEXCOORD1;
		float4 tangentDir																: TEXCOORD2;
		float4 PosWS         															: TEXCOORD3;
		float4 color            														: TEXCOORD4;
		float4 PosS																		: YEXCOORD5;
	}; 

	VertexOut_Simple VS_Simple(VertexIn_Simple vin)
	{
		VertexOut_Simple vout 															= (VertexOut_Simple)0;
		vout.uv0.xy 																	= vin.texcoord0;
		vout.normalDir 																	= TransformObjectToWorldNormal(vin.normal);
		real sign																		= vin.tangent.w * GetOddNegativeScale();
		vout.tangentDir																	= half4(TransformObjectToWorldDir(vin.tangent.xyz).xyz, sign);
		vout.PosWS 																		= mul(unity_ObjectToWorld, vin.vertex);
		vout.pos 																		= TransformObjectToHClip(vin.vertex);
		vout.uv0.w																		= ComputeFogFactor(vout.pos.z);
		vout.color																		= vin.color;
		vout.PosS																		= ComputeScreenPos(vout.pos);
		return vout;
	}

	half4 PS_Simple(VertexOut_Simple pin) : SV_Target
	{
		//-------------  角色点阵化  --------------
		CHARACTER_MASK_TRANSPARENT_SET(_UseTransparentMask,_Transparency,pin.PosS);
		
		half4	VectorGroup_1;															//VectorGroup_1.xyz:N						VectorGroup_1.w:Set_RimLightMask
		half4	VectorGroup_2;															//VectorGroup_2.xyz:L						VectorGroup_2.w:cmpShadow	
		half3	V,L_World;

		GetCharacterNormalizeDir(_DirectionLightPos, _ToonLightDirection, _UseCameraLight,pin.normalDir, pin.PosWS, VectorGroup_1.xyz, VectorGroup_2.xyz, L_World,V);
		half3 NoV 																		= GetNdotV(V,VectorGroup_1.xyz);
		half4 mainColor																	= half4(1,1,1,1);
	    half4 finalColor 																= half4(1,1,1,1);
		half4 celColor																	= half4(1,1,1,1);
		half4 blendColor																= half4(1,1,1,1);
		CHARACTER_SIMPLE_Lighting(_UseAlphaClip,_ShadowRangeStep,_ShadowFeather,VectorGroup_1.xyz, VectorGroup_2.xyz, pin.uv0, pin.PosS, VectorGroup_2.w, finalColor, celColor,blendColor,mainColor);

		SetCharacterSenceShadow(GetCharacterSenceShadow(), finalColor,blendColor * mainColor, _SpecFalloff);
		
		CHARACTER_SIMPLE_SPEC(VectorGroup_1.xyz, VectorGroup_2.xyz, V, VectorGroup_2.w, celColor, finalColor);

		GET_RIM_LIGHT_FACTORY(_RimRangeStep, _RimFeather, NoV, pin.PosWS,VectorGroup_1.w);
		
		GET_SIMPLE_RIM_LIGHT(pin.color, VectorGroup_1.w, _RimLightColor, finalColor);

		GET_SIMPLE_EMISSION(_EmissionColor,pin.uv0, finalColor);

		GET_CHARACTER_LIGHT_COLOR(_UseAddColor,_LightColor,_ToonLightStrength,_AddLightColor,finalColor);

		FRESNEL_SET(_UseAdditionFresnel, _FresnelRange, _FresnelFeather, _FresnelColor, _FresnelFlowRange, NoV, pin.uv0.xy, pin.color.g, finalColor);
		
		Get_CHARACTER_ADDLIGHTS(_UseAddLight, _addShadowRangeStep, _addShadowFeather,pin.normalDir,pin.PosWS,mainColor,finalColor);
		
		SetCharacterIce(_UseIce,pin.normalDir, pin.tangentDir, _DirectionLightPos, V,pin.uv0,finalColor);

		SetCharacterWhiteBalance(_UseWhiteBalance,_kRGB,finalColor);
		
		CHARACTER_DISSOLVE_SET(_UseDissolve,_DissolveScale,_EdgeWidth,_EdgeColor,pin.PosS, finalColor);
		
		#if defined(_TRANSPARENT)
		SetTransparent(pin.PosS,finalColor);
		#endif

		//---------- 距离雾 ----------
		
		SetCharacterUnityFog(pin.uv0.w, finalColor);
		
	    return finalColor;
	}


	struct VertexIn_Shadow
	{
		float4 PosL   																	: POSITION;
		float3 NormalL     																: NORMAL;
		float2 TexC     																: TEXCOORD0;
	};

	struct VertexOut_Shadow
	{
		float2 TexC           															: TEXCOORD0;
		float4 PosH   																	: SV_POSITION;
	};

	float4 GetShadowPositionHClip(VertexIn_Shadow input)
	{
		float3 positionWS																= TransformObjectToWorld(input.PosL.xyz);
		float3 normalWS																	= TransformObjectToWorldNormal(input.NormalL);

		float4 positionCS																= TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _DirectionLightPos));

		#if UNITY_REVERSED_Z
		positionCS.z																	= min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
		#else
		positionCS.z																	= max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
		#endif

		return positionCS;
	}

	VertexOut_Shadow VS_Shadow(VertexIn_Shadow input)
	{
		VertexOut_Shadow output;
		output.TexC 																	= input.TexC;
		output.PosH 																	= GetShadowPositionHClip(input);
		return output;
	}

	half4 PS_Shadow(VertexOut_Shadow input) : SV_TARGET
	{
		half blendA                                 									= SAMPLE_TEXTURE2D(_BlendTex,sampler_BlendTex,input.TexC).a;
		blendA                                      									= clamp( pow( blendA, 0.7), 0, 1);
		clip(blendA - 0.3);
		return 0;
	}

	half4 PS_Shadow_Simple(VertexOut_Shadow input) : SV_TARGET
	{
		return 0;
	}
	

	