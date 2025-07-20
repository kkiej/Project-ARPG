/***********************************************************************************************
 ***                     B A R D S H A D E R  ---  B A R D  S T U D I O S                    ***
 ***********************************************************************************************
 *                                                                                             *
 *                                      Project Name : BARD                                    *
 *                                                                                             *
 *                               File Name : bard_shader_lib.hlsl                              *
 *                                                                                             *
 *                                    Programmer : Zhu Han                                     *
 *                                                                                             *
 *                                      Date : 2021/1/20                                       *
 *                                                                                             *
 *---------------------------------------------------------------------------------------------*
 * Functions:                                                                                  *
 *   1.0 GetNdotL                                                                              *
 *   2.0 GetNdotV                                                                              *
 *   3.0 GetHalfDir                                                                  	       *
 *   4.0 GetVdotL                                                                              *
 *   5.0 GetlnLenH                                                                             *
 *   6.0 GetRough                                                        					   *
 *   7.0 GetNdotH                                                                 			   *
 *   8.0 GetRoL                                                                  			   *
 *   9.0 GetMYGGX_InvincibleDragon                                                             *
 *  10.0 GetMYPhongApprox_InvincibleDragon                                                     *
 *  11.0 MYCalcSpecular                                                                        *
 *  12.0 ACESToneMapping                                                                       *
 *  13.0 GetShadowColor                                                                        *
 *  14.0 GetRimColor                                                                           *
 *  15.0 GetRoundingStep                                                                       *
 *  16.0 GetSpec_Model_Toon                                                                    *
 *  17.0 Perlin Noise                                                                          *
 *  18.0 GetWHRatio                                                                            *
 *  19.0 SetMaskTransparency                                                                   *
 *  20.0 RotateAroundYInDegrees                                                                *
 *  21.0 GetTwoDecimal                                                                         *  
 *  22.0 ColorCurvesSet                                                                        *
 *  23.0 TransformViewToProjection                                                             *
 *  23.0 SIMPLE NOISE                                                             			   *
 *  24.0 REMAP																				   *
 *  25.0 SIMPLEX																			   *
 *  26.0 GetUVNosie																			   *
 *  27.0 TransformClipToScreen																   *
 *  28.0 TransformViewToScreen                                                                 *
 *  29.0 linearstep                                                                            *
 * - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
#include "Colorful.hlsl"
#include "bard_shader_Input.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
 
/// <summary>
/// GetNdotL
/// </summary>
inline half3 GetNdotL(half3 lightdir,   half3 nN) 
{
   return																clamp(dot(lightdir, nN), 0, 1) ;	
}

/// <summary>
/// GetNdotV
/// </summary>
inline half GetNdotV(half3 viewdir,   half3 nN)
{
   return																dot(nN,viewdir);	
}

/// <summary>
/// GetHalfDir
/// </summary>
inline half3  GetHalfDir(half3 viewDir, half3 lightDir)
{
   return																normalize(viewDir + lightDir);
}

/// <summary>
/// GetVdotL
/// </summary>
inline half3  GetVdotL(half3 viewDir, half3 lightDir)
{
    return																dot(viewDir , lightDir);
}

/// <summary>
/// GetlnLenH
/// </summary>
inline half3 GetlnLenH(half3 VdotL)
{
     return																1/sqrt( 2 + 2 * VdotL); 
}

/// <summary>
/// GetRough
/// </summary>
inline half3 GetRough(half Shininess, half ilmTexR)
{
     return																(1.0 - Shininess*ilmTexR);
}

/// <summary>
/// GetNdotH
/// </summary>
inline half3 GetNdotH(half3 N,half3 H)
{
    return																max(0,dot(N,H));
}

/// <summary>
/// GetRoL
/// </summary>
inline half3 GetRoL(half3 NdotL, half3 NdotV, half3 VodtL)
{
     return																2 * NdotL * NdotV - VodtL;
}

/// <summary>
/// GetMYGGX_InvincibleDragon
/// </summary>
inline half GetMYGGX_InvincibleDragon(half3 N,   half3 H,   half Rough,   half3 NoH)
{
    float3 NxH															= cross(N, H);
    float OneMinusNoHSqr												= dot(NxH, NxH);
    half a																= Rough * Rough;
    float n 															= NoH * a;
    float p 															= a / (OneMinusNoHSqr + n * n);
    float d 															= p * p;
    return																d;
}

/// <summary>
/// GetMYPhongApprox_InvincibleDragon
/// </summary>
inline half GetMYPhongApprox_InvincibleDragon(half Rough,half RoL)
{
    float a																= Rough * Rough;
    a																	= max(a, 0.008);
    float a2															= a * a;
    float rcp_a2														= 1 / a2;
    float c 															= 0.72134752 * rcp_a2 + 0.39674113;
    float p 															= rcp_a2 * exp2(c * RoL - c);
    return																min(p, rcp_a2);
}

/// <summary>
/// MYCalcSpecular
/// </summary>
inline half MYCalcSpecular(half specFunction,   half MYPhongApprox_InvincibleDragon,  half Roughness,  half  MYGGX_InvincibleDragon)  //SpecModel
{
     if (specFunction == 0)
     {        
          return														MYPhongApprox_InvincibleDragon;
     }
	 return																(Roughness * 0.25 + 0.25) * MYGGX_InvincibleDragon;
}

/// <summary>
/// ACESToneMapping
/// </summary>
inline half3 ACESToneMapping(half3 x)
{
    float a 															= 2.51f;
    float b 															= 0.03f;
    float c 															= 2.43f;
    float d 															= 0.59f;
    float e 															= 0.14f;
    return																saturate((x*(a*x+b))/(x*(c*x+d)+e));
}

/// <summary>
/// GetShadowColor
/// </summary>
inline void GetShadowColor(half sumStep/*step总和*/,half currentStep/*当前step*/,half inputStepNumber/*被step数*/,half stepMask/*step数值*/,half remapStepNumber,half remapStep,out half3 sumColor)
{
	// currentStep = (1 - sumStep) * step(inputStepNumber,stepMask);
	//sumColor = (1 - step(1,stepMask)) * step(0.95,stepMask);
	while(inputStepNumber > 0.001)
	{
		currentStep														= (1 - sumStep) * step(inputStepNumber,stepMask);
		sumStep															+= currentStep;

		UNITY_BRANCH
		if(remapStep <= 0)										
		{										
			remapStep													= 0.01;
		}										
		sumColor														+= currentStep * SAMPLE_TEXTURE2D(_BlendTex, sampler_BlendTex, float2(remapStepNumber,remapStep));
		inputStepNumber													-= 0.05;
		remapStep														-= 0.065;
	}
}

/// <summary>
/// GetRimColor
/// </summary>
// inline void GetRimColor(half sumStep/*step总和*/,half currentStep/*当前step*/,half inputStepNumber/*被step数*/,half stepMask/*step数值*/,half remapStepNumber,half remapStep,out half3 sumColor)
// {
// 	while(inputStepNumber > 0.001)
// 	{
// 		currentStep														= (1 - sumStep) * step(inputStepNumber,stepMask);
// 		sumStep += currentStep;
// 		if(remapStep <= 0)
// 		{
// 			remapStep													= 0.01;
// 		}
// 		sumColor														+= currentStep * SAMPLE_TEXTURE2D(_RimTex, sampler_RimTex, float2(remapStepNumber,remapStep));
// 		inputStepNumber													-= 0.05;
// 		remapStep														-= 0.063;
// 	}
// }

/// <summary>
/// GetRoundingStep
/// </summary>
inline half GetRoundingStep(half InputStep)
{
	InputStep															= floor(InputStep * 100);
	return																InputStep * 0.01;
}

/// <summary>
/// GetSpec_Model_Toon
/// </summary>
inline half GetSpec_Model_Toon(half SpecStep, half SpecModel, half Checkline, half ilmTexB)
{
     return																(floor(smoothstep(0,SpecStep,ilmTexB * SpecModel) * Checkline)/ Checkline);
}

/// <summary>
/// Perlin Noise 
/// </summary>
inline float2 hash22(float2 p)
{
	p																	= float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
	return																-1.0 + 2.0 * frac(sin(p) * 43758.5453123);
}

/// <summary>
/// hash21
/// </summary>
inline float2 hash21(float2 p)
{
	float h																= dot(p, float2(127.1, 311.7));
	return																-1.0 + 2.0 * frac(sin(h) * 43758.5453123);
}

/// <summary>
/// perlin
/// </summary>
inline float perlin_noise(float2 p)
{
	float2 pi															= floor(p);
	float2 pf															= p - pi;
	float2 w															= pf * pf * (3.0 - 2.0 * pf);
	
	return																lerp(lerp(dot(hash22(pi + float2(0.0, 0.0)), pf - float2(0.0, 0.0)),
																		dot(hash22(pi + float2(1.0, 0.0)), pf - float2(1.0, 0.0)), w.x),
																		lerp(dot(hash22(pi + float2(0.0, 1.0)), pf - float2(0.0, 1.0)),
																		dot(hash22(pi + float2(1.0, 1.0)), pf - float2(1.0, 1.0)), w.x), w.y);
}
	
/// <summary>
/// GetWHRatio
/// </summary>
inline float2 GetWHRatio()
{
    return																float2(_ScreenParams.y / _ScreenParams.x, 1);
}

/// <summary>
/// SetMaskTransparency
/// </summary>
inline void SetMaskTransparency(half _Transparency,half2 screenPos)
{
	//阈值矩阵
	float4x4 thresholdMatrix =
	{  
		1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
		13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
		4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
		16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
	};

	//单位矩阵
	float4x4 _RowAccess =
	{ 
		1,  0,  0,  0, 
		0,  1,  0,  0, 
		0,  0,  1,  0, 
		0,  0,  0,  1 
	};
	clip(_Transparency - thresholdMatrix[fmod(screenPos.x, 4)] * _RowAccess[fmod(screenPos.y, 4)]);
}

/// <summary>
/// RotateAroundYInDegrees
/// </summary>
inline float3 RotateAroundYInDegrees(float3 vertex, float degrees)
{
    float alpha															= degrees * PI / 180.0;
    float sina, cosa;
    sincos(alpha, sina, cosa);
    float2x2 m															= float2x2(cosa, -sina, sina, cosa);
    return																float3(mul(m, vertex.xz), vertex.y).xzy;
}

/// <summary>
/// GetTwoDecimal
/// </summary>
inline half GetTwoDecimal(half InputStep)
{
	InputStep															= floor(InputStep * 100);
	return																InputStep * 0.01;
}

/// <summary>
/// ColorCurvesSet
/// </summary>
// inline half3 ColorCurvesSet(half3 inputColor)
// {
// 	inputColor.r 														= SAMPLE_TEXTURE2D(_CurvesTex, sampler_CurvesTex, float2(GetTwoDecimal(inputColor.r),0.5)).r;
// 	inputColor.g 														= SAMPLE_TEXTURE2D(_CurvesTex, sampler_CurvesTex, float2(GetTwoDecimal(inputColor.g),0.5)).r;
// 	inputColor.b 														= SAMPLE_TEXTURE2D(_CurvesTex, sampler_CurvesTex, float2(GetTwoDecimal(inputColor.b),0.5)).r;
// 	return																inputColor;
// }

/// <summary>
/// TransformViewToProjection
/// </summary>
inline float2 TransformViewToProjection (float2 v)
{
    return																mul((float2x2)UNITY_MATRIX_P, v);
}

/// <summary>
/// TransformViewToProjection
/// </summary>
inline float3 TransformViewToProjection (float3 v)
{
    return																mul((float3x3)UNITY_MATRIX_P, v);
}

/// <summary>
/// SIMPLE NOISE 
/// </summary>
inline half2 hash_simple( half2 p ) // replace this by something better
{
	p																	= half2( dot(p,half2(127.1,311.7)), dot(p,half2(269.5,183.3)) );
	return																-1.0 + 2.0*frac(sin(p)*43758.5453123);
}

/// <summary>
/// REMAP
/// </summary>
inline half remap(half x, half t1, half t2, half s1, half s2)
{
	return																(x - t1) / (t2 - t1) * (s2 - s1) + s1;
}

/// <summary>
/// SIMPLEX
/// </summary>
inline half SimpleX_Noise( in half2 p )
{
    const half K1 														= 0.366025404; // (sqrt(3)-1)/2;
    const half K2 														= 0.211324865; // (3-sqrt(3))/6;

	half2  i 															= floor( p + (p.x+p.y)*K1 );
    half2  a 															= p - i + (i.x+i.y)*K2;
    half   m 															= step(a.y,a.x); 
    half2  o 															= half2(m,1.0-m);
    half2  b 															= a - o + K2;
	half2  c 															= a - 1.0 + 2.0*K2;
    half3  h 															= max( 0.5-half3(dot(a,a), dot(b,b), dot(c,c) ), 0.0 );
	half3  n 															= h*h*h*h*half3( dot(a,hash_simple(i+0.0)), dot(b,hash_simple(i+o)), dot(c,hash_simple(i+1.0)));
    return																dot(n, half3(70,70,70) );
}

/// <summary>
/// GetUVNosie
/// </summary>
inline half GetUVNosie(half2 uv)
{
	return																frac(sin(dot(uv.xy, float2(12.9898, 78.233))) * 43758.5453);
}

/// <summary>
/// TransformClipToScreen
/// </summary>
inline float2 TransformClipToScreen(float4 posCS)
{
	float2 posSS														= posCS.xy / posCS.w;
	posSS.y																*= -1;
	posSS																= 0.5*(posSS + 1.0);
	return																posSS;
}

/// <summary>
/// TransformViewToScreen
/// </summary>
inline float2 TransformViewToScreen(float3 posVS)
{
	float4 posCS														= mul(UNITY_MATRIX_P, posVS);
	return																TransformClipToScreen(posCS);
}

/// <summary>
/// linearstep
/// </summary>
inline half linearstep(half edge0, half edge1, half x)
{
	half t																= (x - edge0)/(edge1 - edge0);
	return																clamp(t, 0.0, 1.0);
}

/// <summary>
/// DecodeRGBA
/// </summary>
inline float DecodeRGBA( float4 enc )
{
	float4 kDecodeDot													= float4(1.0, 1/255.0, 1/65025.0, 1/16581375.0);
	return																dot( enc, kDecodeDot );
}

/// <summary>
/// EncodeRGBA
/// </summary>
inline half4 EncodeRGBA(half v )
{
	half4 kEncodeMul                            						= half4(1.0, 255.0, 65025.0, 16581375.0);
	half kEncodeBit                             						= 1.0 / 255.0;
	half4 enc                                   						= kEncodeMul * v;
	enc                                         						= frac (enc);
	enc                                         						-= enc.yzww * kEncodeBit;
	return                                      						enc;
}

/// <summary>
/// SampleNormal
/// </summary>
half3 SampleNormal(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), half scale = 1.0h)
{
	half4 n                                                         = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
	return                                                          UnpackNormalScale(n, scale);
}