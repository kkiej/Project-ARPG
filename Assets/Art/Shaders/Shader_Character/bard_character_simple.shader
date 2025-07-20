Shader "bard/role/simple"
{
    Properties
    {
        [Header(Main Texture)]
    	[Toggle] _UseAlphaClip("Alpha Clip",float) = 0
		_MainTex("Base Texture", 2D) = "white" {}
    	_MainTex02("Base Texture(02)", 2D) = "white"{}
		_CelTex("CelTex", 2D) = "white" {}
    	_CelTex02("Cel Tex(02)",2D) = "white"{}
		_BlendTex("BlendTex", 2D) = "white" {}
		_BlendTex02("BlendTex(02)", 2D) = "white" {}
        [Space][Space][Space]

        [Header(OutLine)]
        [Toggle] _UseSmoothNormal("使用法线平均化",float) = 0
		_OutlineColor ("OutLine Color", Color) = (0.106,0.0902,0.0784,1)
		_OutlineWidth("Outline Width", Range (0, 5)) = .07
    	_Offset_Z("OutLine Offset Z", float) = 0
        [Space][Space][Space]

        [Header(Emission)]
        [HDR]_EmissionColor("Emission Color",color) = (1,1,1,1)
        [Space][Space][Space]

        [Header(RimLight)]
        _RimLightColor("Rim Light Color",Color) = (1,1,1,1)
		_RimRangeStep("Rim Range Step", Range(0, 1)) = 0.3
        _RimFeather("Rim Feather",Range(0.001,1)) = 0.3
        [Space][Space][Space]

        [Header(SpecLight)]
    	_SpecColor("Spec Color",Color) = (1,1,1,1)
    	_SpecFalloff("Spec Falloff", Range(0, 1)) = 0.4

        [Header(Shadow)]
        _ShadowRangeStep("Shadow Range Step", Range(0,1)) = 0.8
        _ShadowFeather("Shadow Feather", Range(0.001,1)) = 0.3
    	
        [Toggle(_USE_MAIN_TEXTURE_LERP)]USE_MAIN_TEXTURE_LERP("使用MainTex插值",int) = 0
        
    	_MainTexNoiseTilling("插值溶解重铺值",Vector) = (32,16,0,0)
		_Dissolve_power1("溶解强度", Range( 0 , 1)) = 0
		_Dissolve_power2("亮边范围", Range( 0 , 1)) = 0.65
		[HDR]_EdgeColor2("亮边颜色", Color) = (0,0.438499,1,1)
    	
    	
        [HideInInspector]_ShadowInvLen("Shadow InvLen",Range(0,1)) = 1
        [HideInInspector]_ShadowOffset("Shadow Offset(Y)",Range(-3,3)) = 0
        [HideInInspector]_PlaneShadowColor("Plane Shadow Color",color) = (0,0,0,1)
    	[HideInInspector]_WorldPos("WorldPos",vector) = (1,1,1,1)
    	[HideInInspector]_ShadowPlane("ShadowPlane",vector) = (1,1,1,1)
    	[HideInInspector]_ShadowFadeParams("ShadowFadeParams",vector) = (1,1,1,1)
        
		[HideInInspector]_UseTransparentMask("Use Transparency Clip",float) = 1
        [HideInInspector]_Transparency("Transparency(点正化半透强度)",Range(0,1)) = 1
    	
    	[Header(Ice)]
    	[HideInInspector][Toggle] _UseIce("Use Ice",float) = 0
    	[HideInInspector]_IceVFXColor("Ice Color",Color) = (1,1,1,1)
		[HideInInspector]_NormalMap("NormalMap",2D) = "bump"{}
    	[HideInInspector]_IceNormalScale("IceNormalScale",Range(0,1)) = 1
        [HideInInspector]_IceRimColor("Ice Rim Color",Color) = (1,1,1,1)
    	[HideInInspector]_IceTexture("Ice Tex",2D) = "White"{}
        [HideInInspector]_Ice_Fresnel_Feather("Fresnel Feather",Range(0,1)) = 0.5
        [HideInInspector]_Ice_Fresnel_Step("Fresnel Step",Range(0,1)) = 0.5
    	[HideInInspector]_IceSpecColor("Spec Color",Color) = (1,1,1,1)
    	[HideInInspector]_IceSpecPower("Spec Power",float) = 1

        [HideInInspector]_LightColor("Light Color",Color) = (1,1,1,1)
    	[HideInInspector]_UseCameraLight("使用相机灯光",float) = 0
	    [HideInInspector]_EdgeWidth ("Edge Width", Range(0,0.5)) = 0
		[HideInInspector]_EdgeColor ("Edge Color", color) = (1,1,1,1)
    	[HideInInspector]_DissolveScale ("DissolveScale", Range(0,1)) = 0
        
    	[HideInInspector] _OutlineSkinColor("OutLine Skin Color",Color) = (1,1,1,1)
    	
	    [HideInInspector]_NoiseTex("NoiseTex",2D) = "white"{}
        [HideInInspector]_UseDissolveCut("Use Dissolve Cut",float) = 0
    	[HideInInspector]_DissolveCut ("Dissolve Cut", Range(0,1)) = 0
    	[HideInInspector]_ScreenUV("ScreenUV",vector) = (7,3,0,0)
        
        

        [HideInInspector]_ShadowColor("Shadow Color",Color) = (1,1,1,1)
        [HideInInspector]_EyeShadowColor("Eye Shadow Color",Color) = (1,1,1,1)
        [HideInInspector]_defaultShadowStrength("Default Shadow Strength",Range(0,1)) = 1
    	[HideInInspector]_HairShadowDistace ("HairShadowDistance", Float) = 0.05
    	
        [HideInInspector]_UseHSV("Use HSV", float) = 0
		[HideInInspector]_Hue("Hue", Range(0, 1)) = 0
		[HideInInspector]_Saturation("Saturation", Range(-1, 1)) = 0
		[HideInInspector]_Value("Value", Range(-1, 1)) = 0
        [HideInInspector][HDR]_StencilOutLineColor("Stencil OutLine Color", Color) = (0,2.180392,4,1)
	    [HideInInspector]_StencilOutlineWidth("Stencil OutLine Width", Range(0, 1)) = 0.8
        
        [HideInInspector]_UseAdditionFresnel("Use Addition Fresnel",float) = 0
	    [HideInInspector][HDR]_FresnelColor("Fresnel Color",color) = (1,1,1,1)
        [HideInInspector]_FresnelRange("Fresnel Range", Range(0,1)) = 0.3
    	[HideInInspector]_FresnelFeather("Fresnel Feather", Range(0,1)) = 0.3
    	[HideInInspector]_FresnelFlowRange("Fresnel Flow Range", Range(0,1)) = 0.5
    	
        [HideInInspector]_PosWDissolveScale("PosW Dissolve Scale",Range(0,2)) = 0.5
		[HideInInspector]_PosWDissolveWidth ("PosW Dissolve Width", Range(0,0.3)) = 0
        [HideInInspector][HDR]_PosWDissolveColor ("PosW Dissolve Color", color) = (1,1,1,1)
		[HideInInspector]_PosWNoiseScale ("PosW Noise Scale", Range(0,0.5)) = 0
		[HideInInspector]_Top2Bottom("Top2Bottom",float) = 0
    	
		[HideInInspector]_UseUnityFog("Use Unity Fog",float) = 1
        [HideInInspector]_UseAddLight("AddLight Shadow Set",float) = 1
        [HideInInspector]_addShadowRangeStep("AddLight Shadow Range Step",Range(0,1)) = 0.8
		[HideInInspector]_addShadowFeather("AddLight Shadow Feather",Range(0.001,1)) = 0.3
    	[HideInInspector]_FaceForwardVector("FaceForwardVector",vector) = (1,1,1,1)
    	[HideInInspector]_ToonLightDirection("ToonLightDirection",vector) = (1,1,1,1)
    	
        [HideInInspector]_UseIce("Use Ice",float) = 0
		[HideInInspector]_RimColor("Rim Color",Color) = (1,1,1,1)
        [HideInInspector]_UseSSRim("Screen Space Rim Light",float) = 0
    	[HideInInspector]_SSRimScale("SS Rim Scale", Range(0,2)) = 0.5
        [HideInInspector]_EmissionStrength("Emission Strength", Range(0,5)) = 1
        [HideInInspector]_MetalScale("Metal Scale",Range(0,1)) = 0.0
		[HideInInspector]_SmoothScale("Smooth Scale",Range(0,1)) = 0.0
	    [HideInInspector]_Shiness("Shiness",Range(0,1)) = 0.5
		[HideInInspector]_CheckLine("CheckLine",Float) =  12
		[HideInInspector]_SpecStep("Spec Step",Float) = 5
        [HideInInspector][HDR]_FlowColor("Flow Color",Color) = (1,1,1,1)
        [HideInInspector]_FirstShadowSet("Flow Range Step",Range(0,1)) = 0.8
    	[HideInInspector]_FirstShadowFeather("Flow Feather",Range(0.001,1)) = 0.3
        [HideInInspector]_AddObjectY("Set Flow Pos",float) = 0
		[HideInInspector]_DissolveWidth ("Dissolve Width", Range(0,0.3)) = 0
		[HideInInspector][HDR]_DissolveColor ("Dissolve Color", color) = (1,1,1,1)
    	[HideInInspector]_MainColor("Base Color", Color) = (1,1,1,1)
    	[HideInInspector]_ToonLightStrength("Light Strength",float) = 1 
    	[HideInInspector]_UseDissolve("Use Dissolve",float) = 0
        [HideInInspector]_LitDirStencilRef ("LitDirStencilRef", Int) = 0
    	[HideInInspector]_UseDarkLuminance("Use Dark Luminance", float) = 0
    	[HideInInspector]_DarkLuminance("Dark Luminance",Range(-1,1)) = 0
    } 
    SubShader
    {
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
    	
        pass
        {
            Name "OutLine"
            Tags{"LightMode" = "OutLine"}
            Cull Front
            HLSLPROGRAM
			#include "character_outline.hlsl"
			#pragma multi_compile_fog
			#pragma vertex					vert_outline
			#pragma fragment				frag_simpleOutline
            ENDHLSL
        }

        pass
        {
            Tags{"LightMode" = "UniversalForward"}
            Cull Back
            HLSLPROGRAM
			#include "character.hlsl"
			#pragma multi_compile_fog
            #pragma multi_compile _ WORLD_SPACE_SAMPLE_SHADOW
            #pragma shader_feature _ _USE_MAIN_TEXTURE_LERP
            #pragma vertex					VS_Simple
            #pragma fragment				PS_Simple 
            ENDHLSL
        }
    	
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode"					= "ShadowCaster"}

            HLSLPROGRAM
            #pragma vertex						VS_Shadow
            #pragma fragment					PS_Shadow_Simple
			#include "character.hlsl"
            ENDHLSL
        }

        Pass
        {
			Name "DepthOnly"
			Tags{"LightMode"					= "DepthOnly"}
			ZWrite On	
			ColorMask 0	
			HLSLPROGRAM	
			#pragma vertex						DepthOnlyVertex
			#pragma fragment					DepthOnlyFragment_Clip 
			#include "DepthOnlyPass.hlsl"
			ENDHLSL
        }
    }
	
	CustomEditor "BardToonSimpleShaderGUI"
}
