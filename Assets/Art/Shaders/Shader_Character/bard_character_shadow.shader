//--------------------------------------------------------------
//-----------------------writer:zh 2020/8/24--------------------
//--------------------------------------------------------------
shader "bard/role/shadow"
{
    properties
    {
    	_MainTex("MainTex",2D) = "white"{}
    	
        _ShadowInvLen("Shadow InvLen",Range(0,1)) = 1
        _ShadowOffset("Shadow Offset(Y)",Range(-3,3)) = 0
        _PlaneShadowColor("Plane Shadow Color",color) = (0,0,0,1)
    	_WorldPos("WorldPos",vector) = (1,1,1,1)
    	_ShadowPlane("ShadowPlane",vector) = (1,1,1,1)
    	_ShadowFadeParams("ShadowFadeParams",vector) = (1,1,1,1)
 
        [HideInInspector]_UseHSV("Use HSV", float) = 0 
    	[HideInInspector]_UseDissolve("Use Dissolve",float) = 0
    	[HideInInspector]_ToonLightStrength("Light Strength",float) = 1
        [HideInInspector]_LightColor("Light Color",Color) = (1,1,1,1)
        [HideInInspector]_UseAddLight("AddLight Shadow Set",float) = 0
		[HideInInspector]_addShadowRangeStep("AddLight Shadow Range Step",Range(0,1)) = 0.8
		[HideInInspector]_addShadowFeather("AddLight Shadow Feather",Range(0.001,1)) = 0.3
        [HideInInspector]_UseSmoothNormal("使用法线平均化",float) = 1
		[HideInInspector]_OutlineColor ("OutLine Color", Color) = (0.106,0.0902,0.0784,1)
        [HideInInspector]_OutlineSkinColor("OutLine Skin Color",Color) = (1,1,1,1)
		[HideInInspector]_OutlineWidth("Outline Width", Range (0, 1)) = .07
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
        [HideInInspector]_Hue("Hue", Range(0, 1)) = 0
		[HideInInspector]_Saturation("Saturation", Range(-1, 1)) = 0
		[HideInInspector]_Value("Value", Range(-1, 1)) = 0
        [HideInInspector] [HDR]_StencilOutLineColor("Stencil OutLine Color", Color) = (0,2.180392,4,1)
    	[HideInInspector] _StencilOutlineWidth("Stencil OutLine Width", Range(0, 1)) = 0.8
        [HideInInspector]_UseAdditionFresnel("Use Addition Fresnel",float) = 0
		[HideInInspector][HDR]_FresnelColor("Fresnel Color",color) = (1,1,1,1)
		[HideInInspector]_FresnelRange("Fresnel Range", Range(0,1)) = 0.3
		[HideInInspector]_FresnelFeather("Fresnel Feather", Range(0,1)) = 0.3
		[HideInInspector]_FresnelFlowRange("Fresnel Flow Range", Range(0,1)) = 0.
        [HideInInspector]_PosWDissolveScale("PosW Dissolve Scale",Range(0,2)) = 0.5
		[HideInInspector]_PosWDissolveWidth ("PosW Dissolve Width", Range(0,0.3)) = 0
		[HideInInspector][HDR]_PosWDissolveColor ("PosW Dissolve Color", color) = (1,1,1,1)
		[HideInInspector]_PosWNoiseScale ("PosW Noise Scale", Range(0,0.5)) = 0
    	[HideInInspector]_Top2Bottom("Top2Bottom",float) = 0
        [HideInInspector]_DissolveScale ("DissolveScale", Range(0,1)) = 0
		[HideInInspector]_EdgeWidth ("Edge Width", Range(0,0.5)) = 0
		[HideInInspector][HDR]_EdgeColor ("Edge Color", color) = (1,1,1,1)
        [HideInInspector]_UseDissolveCut("Use Dissolve Cut",float) = 0
		[HideInInspector]_DissolveCut ("Dissolve Cut", Range(0,1)) = 0
		[HideInInspector]_DissolveWidth ("Dissolve Width", Range(0,0.3)) = 0
		[HideInInspector][HDR]_DissolveColor ("Dissolve Color", color) = (1,1,1,1)
        [HideInInspector]_UseIce("Use Ice",float) = 0
        [HideInInspector]_UseCameraLight("使用相机灯光",float) = 0
        [HideInInspector]_ScreenUV("ScreenUV",vector) = (7,3,0,0)
        [HideInInspector]_Transparency("Transparency(点正化半透强度)",Range(0,1)) = 1
		[HideInInspector]_UseTransparentMask("Use Transparency Clip",float) = 1
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
    	[HideInInspector]_MainColor("Base Color", Color) = (1,1,1,1)
        [HideInInspector]_LitDirStencilRef ("LitDirStencilRef", Int) = 0
    }
    subshader
    {
        pass
        {
            Name "Shadow"
            Tags{"LightMode" = "Shadow"}

            Blend SrcAlpha  OneMinusSrcAlpha
			ZWrite Off
			Cull Back
			ColorMask RGB

            Stencil
			{
				Ref [_LitDirStencilRef]		
				Comp Equal			
				WriteMask 255		
				ReadMask 255
				//Pass IncrSat
				Pass Invert 
				Fail Keep
				ZFail Keep
			}
 
            HLSLPROGRAM
            #include "character_smooth_shadow.hlsl"
            #pragma vertex vert_planeShadow
			#pragma fragment frag_planeShadow
            ENDHLSL
        }
    }
}
 