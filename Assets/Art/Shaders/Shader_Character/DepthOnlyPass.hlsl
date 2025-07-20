/***********************************************************************************************
***                     B A R D S H A D E R  ---  B A R D  S T U D I O S                    ***
***********************************************************************************************
*                                                                                             *
*                                      Project Name : BARD                                    *
*                                                                                             *
*                               File Name : DepthOnlyPass.hlsl                                *
*                                                                                             *
*                                    Programmer : Zhu Han                                     *
*                                                                                             *
*                                      Date : 2021/1/20                                       *
*                                                                                             *
* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

#ifndef UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED
#define UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


uniform TEXTURE2D(_BlendTex);                                                       uniform SAMPLER(sampler_BlendTex);

CBUFFER_START(UnityPerMaterial)
uniform half _DissolveCut;                                                          uniform half _UseDissolveCut;
CBUFFER_END

/// <summary>
/// DEPTH ONLY
/// </summary>
struct Attributes
{
    float4 position     															: POSITION;
    float2 texcoord     															: TEXCOORD0;
};

struct Varyings
{
    float2 uv           															: TEXCOORD0;
    float4 positionCS   															: SV_POSITION;
    float4 positionOS   															: TEXCOORD2;
    float2 texcoord     															: TEXCOORD1;
};

Varyings DepthOnlyVertex(Attributes input)
{
    Varyings output																	= (Varyings)0;
															
    output.positionOS																= input.position;
    output.texcoord																	= input.texcoord;
    output.positionCS																= TransformObjectToHClip(input.position.xyz);
    return output;
}

half4 DepthOnlyFragment_Clip(Varyings input) : SV_TARGET
{
    half blendA                                 									= SAMPLE_TEXTURE2D(_BlendTex,sampler_BlendTex,input.texcoord).a;
    blendA                                      									= clamp( pow( blendA, 0.7), 0, 1);
		
    clip(blendA - 0.3);
		
    return 0;
}

half4 DepthOnlyFragment(Varyings input) : SV_TARGET
{	
    return 0;
}

#endif
