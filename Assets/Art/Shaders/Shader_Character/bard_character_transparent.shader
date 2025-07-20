shader "bard/role/transparent"
{
    Properties
    {
        _MainTex("Albedo (RGB)", 2D) = "white" {}
    	_TransparentTex("Transparent Tex",2D) = "white"{}
        _Transparency("Transparency(点正化半透强度)",Range(0,1)) = 1 
        _LightColor("Light Color",Color) = (1,1,1,1)
        
		_DissolveScale ("DissolveScale", Range(0,1)) = 0
		_EdgeWidth ("Edge Width", Range(0,0.5)) = 0
		[HDR]_EdgeColor ("Edge Color", color) = (1,1,1,1)
        _ScreenUV("ScreenUV",vector) = (7,3,0,0)
    	
    	_PosWDissolveScale("PosW Dissolve Scale",Range(0,2)) = 0.5
		_PosWDissolveWidth ("PosW Dissolve Width", Range(0,0.3)) = 0
        [HDR]_PosWDissolveColor ("PosW Dissolve Color", color) = (1,1,1,1)
		_PosWNoiseScale ("PosW Noise Scale", Range(0,0.5)) = 0
		[Toggle]_Top2Bottom("Top2Bottom",float) = 0
    	
        _ShadowInvLen("Shadow InvLen",Range(0,1)) = 1
        _ShadowOffset("Shadow Offset(Y)",Range(-3,3)) = 0
    	
        [Header(OutLine)]
    	[Toggle]_NoOutLine("No OutLine",float) = 0
    	[Toggle]_UseSmoothNormal("Use Smooth Normal", float) = 1
        _OutlineColor ("Outline Color", Color) = (0.106,0.0902,0.0784,1)
		_OutlineSkinColor ("Outline Skin Color", Color) = (0.458,0.322,0.298,1)
		_OutlineWidth("Outline width", Range (0, 1)) = .07
    	_Offset_Z("OutLine Offset Z", float) = 0
        [Toggle(BLACK_WHITE_ON)]_UseRoleFlickerEffect("使用黑白闪烁特效",int) = 0
        [Toggle(ROLE_POSW_DISSOLVE)]_UseRolePosWDissolve("使用角色世界坐标溶解",int) = 0
    	
        [Header(Ice)]
    	_UseIce("Use Ice",float) = 0
    	_IceVFXColor("Ice Color",Color) = (1,1,1,1)
		_NormalMap("NormalMap",2D) = "bump"{}
    	_IceNormalScale("IceNormalScale",Range(0,1)) = 1
        _IceRimColor("Ice Rim Color",Color) = (1,1,1,1)
    	_IceTexture("Ice Tex",2D) = "White"{}
        _Ice_Fresnel_Feather("Fresnel Feather",Range(0,1)) = 0.5
        _Ice_Fresnel_Step("Fresnel Step",Range(0,1)) = 0.5
    	_IceSpecColor("Spec Color",Color) = (1,1,1,1)
    	_IceSpecPower("Spec Power",float) = 1

		[HideInInspector]_NoiseTex("NoiseTex",2D) = "white"{}
    	[HideInInspector]_UseDissolve("Use Dissolve",float) = 0
        [HideInInspector]_UseTransparentMask("Use Transparency Clip",float) = 1
        [HideInInspector]_UseDissolveCut("Use Dissolve Cut",float) = 0
    	[HideInInspector]_DissolveCut ("Dissolve Cut", Range(0,1)) = 0
        [HideInInspector]_UseAdditionFresnel("Use Addition Fresnel",float) = 0
	    [HideInInspector][HDR]_FresnelColor("Fresnel Color",color) = (1,1,1,1)
        [HideInInspector]_FresnelRange("Fresnel Range", Range(0,1)) = 0.3
    	[HideInInspector]_FresnelFeather("Fresnel Feather", Range(0,1)) = 0.3
    	[HideInInspector]_FresnelFlowRange("Fresnel Flow Range", Range(0,1)) = 0.5
        [HideInInspector]_ShadowColor("Shadow Color",Color) = (1,1,1,1)
    	[HideInInspector]_RimLightColor("Rim Light Color",Color) = (1,1,1,1)
    	[HideInInspector][HDR]_FlowColor("Flow Color",Color) = (1,1,1,1)
    	[HideInInspector]_EyeShadowColor("Eye Shadow Color",Color) = (1,1,1,1)
    	[HideInInspector][HDR]_EmissionColor("Emission Color",color) = (1,1,1,1)
    	[HideInInspector]_HairShadowDistace ("HairShadowDistance", Float) = 0.05
    	[HideInInspector]_FirstShadowSet("Flow Range Step",Range(0,1)) = 0.8
    	[HideInInspector]_FirstShadowFeather("Flow Feather",Range(0.001,1)) = 0.3
    	[HideInInspector]_UseAlphaClip("Alpha Clip",float) = 0
    	[HideInInspector]_AddObjectY("Set Flow Pos",float) = 0
        [HideInInspector]_UseCameraLight("使用相机灯光",float) = 0
    	[HideInInspector] [HDR]_StencilOutLineColor("Stencil OutLine Color", Color) = (0,2.180392,4,1)
    	[HideInInspector] _StencilOutlineWidth("Stencil OutLine Width", Range(0, 1)) = 0.8
        [HideInInspector]_DissolveWidth ("Dissolve Width", Range(0,0.3)) = 0
		[HideInInspector][HDR]_DissolveColor ("Dissolve Color", color) = (1,1,1,1)
        [HideInInspector]_UseAddLight("AddLight Shadow Set",float) = 0
		[HideInInspector]_addShadowRangeStep("AddLight Shadow Range Step",Range(0,1)) = 0.8
		[HideInInspector]_addShadowFeather("AddLight Shadow Feather",Range(0.001,1)) = 0.3
        [HideInInspector]_ShadowRangeStep("Shadow Range Step",Range(0,1)) = 0.8
        [HideInInspector]_ShadowFeather("Shadow Feather",Range(0.001,1)) = 0.3
    	[HideInInspector]_defaultShadowStrength("Default Shadow Strength",Range(0,1)) = 1
        [HideInInspector]_RimColor("Rim Color",Color) = (1,1,1,1)
	    [HideInInspector]_RimRangeStep("Shadow Range Step", Range(0, 1)) = 0.3
		[HideInInspector]_RimFeather("Rim Feather", Range(0.001,1)) = 0.3
    	[HideInInspector]_UseSSRim("Screen Space Rim Light",float) = 0
    	[HideInInspector]_SSRimScale("SS Rim Scale", Range(0,2)) = 0.5
        [HideInInspector]_EmissionStrength("Emission Strength", Range(0,5)) = 1
        [HideInInspector]_SpecFalloff("Spec Falloff", Range(0, 1)) = 0.4
        [HideInInspector]_MetalScale("Metal Scale",Range(0,1)) = 0.0
		[HideInInspector]_SmoothScale("Smooth Scale",Range(0,1)) = 0.0
	    [HideInInspector]_Shiness("Shiness",Range(0,1)) = 0.5
		[HideInInspector]_CheckLine("CheckLine",Float) =  12
		[HideInInspector]_SpecStep("Spec Step",Float) = 5
        [HideInInspector]_UseHSV("Use HSV", float) = 0 
        [HideInInspector]_Hue("Hue", Range(0, 1)) = 0
		[HideInInspector]_Saturation("Saturation", Range(-1, 1)) = 0
		[HideInInspector]_Value("Value", Range(-1, 1)) = 0
    	[HideInInspector]_MainColor("Base Color", Color) = (1,1,1,1)
    	[HideInInspector]_ToonLightStrength("Light Strength",float) = 1
        [HideInInspector]_WorldPos("WorldPos",vector) = (1,1,1,1)
    	[HideInInspector]_ShadowPlane("ShadowPlane",vector) = (1,1,1,1)
        [HideInInspector]_PlaneShadowColor("Plane Shadow Color",color) = (0,0,0,1)
        [HideInInspector]_ShadowFadeParams("ShadowFadeParams",vector) = (1,1,1,1)
		[HideInInspector]_UseUnityFog("Use Unity Fog",float) = 1
    	[HideInInspector]_FaceForwardVector("FaceForwardVector",vector) = (1,1,1,1)
    	[HideInInspector]_ToonLightDirection("ToonLightDirection",vector) = (1,1,1,1)
    	[HideInInspector]_SpecColor("Spec Color",Color) = (1,1,1,1)
    	[HideInInspector]_UseDarkLuminance("Use Dark Luminance", float) = 0
    	[HideInInspector]_DarkLuminance("Dark Luminance",Range(-1,1)) = 0
    }
    SubShader
    {
        Tags{"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}

    	Stencil
	    {
	        Ref 2
	        Comp Always
	        Pass Replace 
	    } 
    	
        pass
        {
            Name "OutLine"
            Tags{"LightMode" = "OutLine"}
            Cull Front
            HLSLPROGRAM
            #define _TRANSPARENT 1
			#include "character_outline.hlsl"
            #pragma multi_compile _ ROLE_POSW_DISSOLVE
			#pragma multi_compile_fog
			#pragma vertex						vert_outline
			#pragma fragment					frag_outline
            ENDHLSL
        } 
    	
        pass
        {
            Tags{"LightMode" = "UniversalForward"}
            Blend SrcAlpha  OneMinusSrcAlpha
            HLSLPROGRAM
            #define _TRANSPARENT 1
			#include "character.hlsl"
			#pragma multi_compile_fog
            #pragma multi_compile _ BLACK_WHITE_ON
            #pragma multi_compile _ ROLE_POSW_DISSOLVE
	        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma vertex						VS_Transparent
			#pragma fragment					PS_Transparent
            ENDHLSL
        }
    }
}
