/***********************************************************************************************
 ***                     B A R D S H A D E R  ---  B A R D  S T U D I O S                    ***
 ***********************************************************************************************
 *                                                                                             *
 *                                      Project Name : BARD                                    *
 *                                                                                             *
 *                               File Name : bard_character_body.shader                        *
 *                                                                                             *
 *                                    Programmer : Zhu Han                                     *
 *                                                                                             *
 *                                      Date : 2020/8/24                                       *
 *                                                                                             *
 * - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
shader "bard/role/body"
{
    properties 
    {
        //Texture Settings 
		_MainColor("Base Color", Color) = (1,1,1,1)
		_MainTex("Base Texture", 2D) = "white" {}
    	_CelTex("Cel Texture", 2D) = "white" {}
		_BlendTex("Blend Texture", 2D) = "white" {}
    	
        //Light Settings   
        _LightColor("Light Color",Color) = (1,1,1,1)
        _UseAddLight("AddLight Shadow Set",float) = 0
		_addShadowRangeStep("AddLight Shadow Range Step",Range(0,1)) = 0.8
		_addShadowFeather("AddLight Shadow Feather",Range(0.001,1)) = 0.3
    	_ToonLightStrength("Light Strength",float) = 1
    	
    	//OutLine Setting
		_UseSmoothNormal("使用法线平均化",float) = 1
		_OutlineColor ("OutLine Color", Color) = (0.106,0.0902,0.0784,1)
        _OutlineSkinColor("OutLine Skin Color",Color) = (1,1,1,1)
		_OutlineWidth("Outline Width", Range (0, 1)) = .07
    	_Offset_Z("OutLine Offset Z", float) = 0
    	
    	//Shadow Setting
        _ShadowRangeStep("Shadow Range Step",Range(0,1)) = 0.8
        _ShadowFeather("Shadow Feather",Range(0.001,1)) = 0.3
    	_defaultShadowStrength("Default Shadow Strength",Range(0,1)) = 1
    	_UseDarkLuminance("Use Dark Luminance", float) = 0
    	_DarkLuminance("Dark Luminance",Range(-1,1)) = 0
    	
        //RimLight Setting
		_RimColor("Rim Color",Color) = (1,1,1,1)
	    _RimRangeStep("Shadow Range Step", Range(0, 1)) = 0.3
		_RimFeather("Rim Feather", Range(0.001,1)) = 0.3
    	_UseSSRim("Screen Space Rim Light",float) = 0
    	_SSRimScale("SS Rim Scale", Range(0,2)) = 0.5
    	
    	//Emission Setting
    	_EmissionTex("Emission Tex",2D) = "black"{}
    	[HDR]_EmissionColor("Emission Color",color) = (1,1,1,1)
    	
        //Specular Setting
    	_SpecColor("Spec Color",Color) = (1,1,1,1)
    	_SpecFalloff("Spec Falloff", Range(0, 1)) = 0.4
    	
		_MetalScale("Metal Scale",Range(0,1)) = 0.0
		_SmoothScale("Smooth Scale",Range(0,1)) = 0.0
	    _Shiness("Shiness",Range(0,1)) = 0.5
		_CheckLine("CheckLine",Float) =  12 
		_SpecStep("Spec Step",Float) = 5
    	
    	//HUE Setting
    	_UseHSV("Use HSV", float) = 0
		_Hue("Hue", Range(0, 1)) = 0
		_Saturation("Saturation", Range(-1, 1)) = 0
		_Value("Value", Range(-1, 1)) = 0
    	
    	//Stocking
    	_StockingTex("Stocking Tex",2D) = "white"{}
    	_First_Fresnel_Shadow_Step("First Fresnel Shadow Step",Range(0,1)) = 0.5
    	_First_Fresnel_Shadow_Feather("First Fresnel Shadow Feather",Range(0.00001,1)) = 0.5
     	_First_Fresnel_Shadow_Color("First Fresnel Shadow Color",Color) = (1,1,1,1)
    	
    	_Second_Fresnel_Shadow_Step("First Fresnel Shadow Step",Range(0,1)) = 0.5
    	_Second_Fresnel_Shadow_Feather("First Fresnel Shadow Feather",Range(0.00001,1)) = 0.5
     	_Second_Fresnel_Shadow_Color("First Fresnel Shadow Color",Color) = (1,1,1,1)
    	
    	_Fresnel_Light_Step("Fresnel Light Step",Range(0,1)) = 0.5
    	_Fresnel_Light_Feather("Fresnel Light Feather",Range(0.00001,1)) = 0.5
     	_Fresnel_Light_Color("Fresnel Light Color",Color) = (1,1,1,1)
    	
    	
    	[HideInInspector] [HDR]_StencilOutLineColor("Stencil OutLine Color", Color) = (0,2.180392,4,1)
    	[HideInInspector] _StencilOutlineWidth("Stencil OutLine Width", Range(0, 1)) = 0.8
        
    	//流光
    	[HideInInspector]_UseAdditionFresnel("Use Addition Fresnel",float) = 0
		[HideInInspector][HDR]_FresnelColor("Fresnel Color",color) = (1,1,1,1)
		[HideInInspector]_FresnelRange("Fresnel Range", Range(0,1)) = 0.3
		[HideInInspector]_FresnelFeather("Fresnel Feather", Range(0,1)) = 0.3
		[HideInInspector]_FresnelFlowRange("Fresnel Flow Range", Range(0,1)) = 0.
        
    	//本地坐标溶解
		[HideInInspector]_PosWDissolveScale("PosW Dissolve Scale",Range(0,2)) = 0.5
		[HideInInspector]_PosWDissolveWidth ("PosW Dissolve Width", Range(0,0.3)) = 0
		[HideInInspector][HDR]_PosWDissolveColor ("PosW Dissolve Color", color) = (1,1,1,1)
		[HideInInspector]_PosWNoiseScale ("PosW Noise Scale", Range(0,0.5)) = 0
    	[HideInInspector]_Top2Bottom("Top2Bottom",float) = 0

    	//死亡溶解
		[HideInInspector]_NoiseTex("NoiseTex",2D) = "white"{}
    	[HideInInspector]_UseDissolve("Use Dissolve",float) = 0
		[HideInInspector]_DissolveScale ("DissolveScale", Range(0,1)) = 0
		[HideInInspector]_EdgeWidth ("Edge Width", Range(0,0.5)) = 0
		[HideInInspector][HDR]_EdgeColor ("Edge Color", color) = (1,1,1,1)
        
    	[HideInInspector]_UseDissolveCut("Use Dissolve Cut",float) = 0
		[HideInInspector]_DissolveCut ("Dissolve Cut", Range(0,1)) = 0
		[HideInInspector]_DissolveWidth ("Dissolve Width", Range(0,0.3)) = 0
		[HideInInspector][HDR]_DissolveColor ("Dissolve Color", color) = (1,1,1,1)
        
    	//冰冻效果
        [HideInInspector]_UseIce("Use Ice",float) = 0
    	[HideInInspector]_IceVFXColor("Ice Color",Color) = (1,1,1,1)
		[HideInInspector]_NormalMap("NormalMap",2D) = "bump"{}
    	[HideInInspector]_IceNormalScale("IceNormalScale",Range(0,1)) = 1
        [HideInInspector]_IceRimColor("Ice Rim Color",Color) = (1,1,1,1)
    	[HideInInspector]_IceTexture("Ice Tex",2D) = "White"{}
        [HideInInspector]_Ice_Fresnel_Feather("Fresnel Feather",Range(0,1)) = 0.5
        [HideInInspector]_Ice_Fresnel_Step("Fresnel Step",Range(0,1)) = 0.5
    	[HideInInspector]_IceSpecColor("Spec Color",Color) = (1,1,1,1)
    	[HideInInspector]_IceSpecPower("Spec Power",float) = 1

    	
        [HideInInspector]_UseCameraLight("使用相机灯光",float) = 0
    	
    	[HideInInspector]_ScreenUV("ScreenUV",vector) = (7,3,0,0)
        
		[HideInInspector]_Transparency("Transparency(点正化半透强度)",Range(0,1)) = 1
		[HideInInspector]_UseTransparentMask("Use Transparency Clip",float) = 1
    	
		[HideInInspector]_UseUnityFog("Use Unity Fog",float) = 1
    	
    	[HideInInspector]_FaceForwardVector("FaceForwardVector",vector) = (1,1,1,1)
    	[HideInInspector]_ToonLightDirection("ToonLightDirection",vector) = (1,1,1,1)
    	
        _ShadowInvLen("Shadow InvLen",Range(0,1)) = 1
        _ShadowOffset("Shadow Offset(Y)",Range(-3,3)) = 0
    	_ShadowColor("Shadow Color",Color) = (1,1,1,1)
    	_WorldPos("WorldPos",vector) = (1,1,1,1)
    	_ShadowPlane("ShadowPlane",vector) = (1,1,1,1)
        _PlaneShadowColor("Plane Shadow Color",color) = (0,0,0,1)
        _ShadowFadeParams("ShadowFadeParams",vector) = (1,1,1,1)
    	
    	_RimLightColor("Rim Light Color",Color) = (1,1,1,1)
    	[HDR]_FlowColor("Flow Color",Color) = (1,1,1,1)
    	_EyeShadowColor("Eye Shadow Color",Color) = (1,1,1,1)
		_EmissionStrength("Emission Strength", Range(0,5)) = 1
        _HairShadowDistace ("HairShadowDistance", Float) = 0.05
    	_FirstShadowSet("Flow Range Step",Range(0,1)) = 0.8
    	_FirstShadowFeather("Flow Feather",Range(0.001,1)) = 0.3
    	_UseAlphaClip("Alpha Clip",float) = 0
    	_AddObjectY("Set Flow Pos",float) = 0
    	
    	
        [Header(Shader Feature)]
        [Space][Space][Space]
        [Toggle(NIGHT_ON)]_NIGHTON("切换夜间贴图",int) = 0
        [Toggle(BLACK_WHITE_ON)]_UseRoleFlickerEffect("使用黑白闪烁特效",int) = 0
        [Toggle(ROLE_POSW_DISSOLVE)]_UseRolePosWDissolve("使用角色世界坐标溶解",int) = 0
    }
	
    subshader
    {
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
    	
    	Stencil
	    {
	        Ref 2
	        Comp Always
	        Pass Replace 
	    }

	    pass
    	{
		    Stencil
			{
			    Ref 2
			    Comp NotEqual
			}
    		
    		Name "ShineOutLine"
    		Tags{"LightMode" = "ShineOutLine"}
            Cull Front
            HLSLPROGRAM
			#include "character_outline.hlsl"
			#pragma vertex						vert_stencilOutline
			#pragma fragment					frag_stencilOutline
            ENDHLSL
        }
    	
        Pass
        {
			Name "DepthAdditional"
			Tags{"LightMode"					= "DepthAdditional"}
			ZWrite On	
			ColorMask 0	
			HLSLPROGRAM	
			#pragma vertex						DepthOnlyVertex
			#pragma fragment					DepthOnlyFragment_Clip 
			#include "DepthOnlyPass.hlsl"
			ENDHLSL
        }
    	
	    pass
        {
            Name "OutLine"
            Tags{"LightMode"					= "OutLine"}
            Cull Front
            HLSLPROGRAM
			#include "character_outline.hlsl"
			#pragma multi_compile_fog
            #pragma multi_compile _ ROLE_POSW_DISSOLVE
            // #pragma multi_compile _ RECEIVE_SHADOW
            #pragma multi_compile _ WORLD_SPACE_SAMPLE_SHADOW
            // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma vertex						vert_outline
			#pragma fragment					frag_skin_outline
            ENDHLSL
        }
 
        pass
        {
            Tags{"LightMode"					= "DelayForward"}
            Cull Back
            HLSLPROGRAM
			#include "character.hlsl"
			#pragma multi_compile_fog
            #pragma multi_compile _ NIGHT_ON
            #pragma multi_compile _ BLACK_WHITE_ON 
            #pragma multi_compile _ ROLE_POSW_DISSOLVE
            // #pragma multi_compile _ RECEIVE_SHADOW
            #pragma multi_compile _ WORLD_SPACE_SAMPLE_SHADOW
	        // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma shader_feature _ USE_STOCKING
			#pragma vertex						VS_Body
			#pragma fragment					PS_Body  
            ENDHLSL
        }
    	
	    Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode"					= "ShadowCaster"}

            HLSLPROGRAM
            #pragma vertex						VS_Shadow
            #pragma fragment					PS_Shadow 
			#include "character.hlsl"
            ENDHLSL
        }
    }
	
	CustomEditor "BardToonBodyShaderGUI"
}
