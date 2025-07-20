#ifndef TIARA_SHADERS_UTILITY
#define TIARA_SHADERS_UTILITY

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

half CelRemap(half value, half hardness, half offset, half center = 0.5h)
{
	return hardness * (value + offset - center) + center;
}

half SmoothCelRemap(half value, half hardness, half offset, half center = 0.5h)
{
	half result = CelRemap(value, hardness, offset, center);
	result = smoothstep(0, 1, saturate(result));
	return result;
}

// Lerp from 0 to 1, but 0.5 is value
half LerpBetween01(half value, half alpha)
{
	half result = value * alpha * 2.0h * step(alpha, 0.5h);
	half rate = alpha * 2.0h - 1.0h;
	result += (value * (1 - rate) + rate) * step(0.5h, alpha);
	return result;
}

half InverseLerp(half min, half max, half t)
{
	return (t - min) / (max - min);
}
float InverseLerp(float min, float max, float t)
{
	return (t - min) / (max - min);
}

inline float4 GetPosNDCFromCS(float4 positionCS)
{
	float4 ndc = positionCS * 0.5f;
	float4 positionNDC;
	positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
	positionNDC.zw = positionCS.zw;
	return positionNDC;
}
inline float4 GetPosNDCFromWS(float3 positionWS)
{
	return GetPosNDCFromCS(TransformWorldToHClip(positionWS));
}
inline float3 GetScreenPixelPosFromNDC(float4 positionNDC)
{
	positionNDC = positionNDC / positionNDC.w;
	positionNDC.z = (UNITY_NEAR_CLIP_VALUE >= 0) ? positionNDC.z : positionNDC.z * 0.5 + 0.5;
	float2 screenPosXY = positionNDC.xy * _ScreenParams.xy;
	return float3(screenPosXY, positionNDC.z);
}
inline float3 GetScreenPosFromNDC(float4 positionNDC)
{
	positionNDC = positionNDC / positionNDC.w;
	positionNDC.z = (UNITY_NEAR_CLIP_VALUE >= 0) ? positionNDC.z : positionNDC.z * 0.5 + 0.5;
	float2 screenPosXY = positionNDC.xy * float2((_ScreenParams.w - 1.0) * _ScreenParams.x, 1.0);
	return float3(screenPosXY, positionNDC.z);
}
inline float3 GetScreenPixelPosFromCS(float4 positionCS)
{
	return GetScreenPixelPosFromNDC(GetPosNDCFromCS(positionCS));
}
inline float3 GetScreenPosFromCS(float4 positionCS)
{
	return GetScreenPosFromNDC(GetPosNDCFromCS(positionCS));
}
inline float3 GetScreenPixelPosFromWS(float3 positionWS)
{
	return GetScreenPixelPosFromCS(TransformWorldToHClip(positionWS));
}
inline float3 GetScreenPosFromWS(float3 positionWS)
{
	return GetScreenPosFromCS(TransformWorldToHClip(positionWS));
}

inline half Dither4x4Bayer(int x, int y)
{
	const half dither[16] = {
		1,	9,	3,	11,
		13,	5,	15,	7,
		4,	12,	2,	10,
		16,	8,	14,	6 
	};
	int r = y * 4 + x;
	return dither[r] / 16;
}
inline float Dither8x8Bayer(int x, int y)
{
	const float dither[64] = {
		1, 49, 13, 61,  4, 52, 16, 64,
	   33, 17, 45, 29, 36, 20, 48, 32,
		9, 57,  5, 53, 12, 60,  8, 56,
	   41, 25, 37, 21, 44, 28, 40, 24,
		3, 51, 15, 63,  2, 50, 14, 62,
	   35, 19, 47, 31, 34, 18, 46, 30,
	   11, 59,  7, 55, 10, 58,  6, 54,
	   43, 27, 39, 23, 42, 26, 38, 22};
	int r = y * 8 + x;
	return dither[r] / 64; // same # of instructions as pre-dividing due to compiler magic
}	
inline half SampleDither4x4(float2 screenPixelPos)
{
	return Dither4x4Bayer(fmod(screenPixelPos.x, 4), fmod(screenPixelPos.y, 4));
}
inline half SampleDither4x4(float4 positionNDC)
{
	return SampleDither4x4(GetScreenPixelPosFromNDC(positionNDC).xy);
}
inline half SampleDither8x8(float2 screenPixelPos)
{
	return Dither8x8Bayer(fmod(screenPixelPos.x, 8), fmod(screenPixelPos.y, 8));
}
inline half SampleDither8x8(float4 positionNDC)
{
	return SampleDither8x8(GetScreenPixelPosFromNDC(positionNDC).xy);
}

inline float AlphaToDotWave(float2 screenPos, float density)
{
	float cellDens = 10.0f * density;
	float2 dotCenter = (floor(screenPos * cellDens) + float2(0.5f, 0.5f)) * rcp(cellDens);
	return distance(screenPos, dotCenter) * cellDens * 0.7071067811865f;
}

half ReverseSmoothStep(half x)
{
	return saturate(x * (x * (3.3333h * x - 5.0h) + 2.6667h));
}

float3 ReverseSmoothStep(float3 x)
{
	return saturate(x * (x * (3.3333 * x - 5.0) + 2.6667));
}

// TODO: y轴偏移还需改良
float3 CalcWind(float3 positionWS, float power, float4 windSpeedFreqStr, TEXTURE2D_PARAM( _WindDistoTex, sampler_WindDistoTex))
{
	float2 windFlotUV = positionWS.xz * windSpeedFreqStr.z - _Time.y * windSpeedFreqStr.xy;
	float3 windFlow = float3(0, 0, 0);
	windFlow.xz = SAMPLE_TEXTURE2D_LOD(_WindDistoTex, sampler_WindDistoTex, windFlotUV, 0.0).rg;
	windFlow.xz = ReverseSmoothStep(windFlow).xz * 2 - 0.95;
	windFlow.xz *= normalize(windSpeedFreqStr.xy);
	//windFlow.y = dot(windFlow.xz, windFlow.xz) - 1;
	power = windSpeedFreqStr.w * power;
	windFlow.xz *= power * 10;
	//windFlow.y *= abs(power);
	return windFlow;
}

half CalcSides(float3 positionWS, half3 viewDirWS, float4 positionNDC, half hideSideSHardness)
{
	half3 faceNormal = normalize(cross(ddy(positionWS), ddx(positionWS)));
	half faceSide = (dot(viewDirWS, faceNormal) - 0.5h) * hideSideSHardness + 0.5h;
	faceSide = smoothstep(0, 1, saturate(faceSide));
	return step(SampleDither4x4(positionNDC), faceSide);
}

half OneMinusPow(half n, half p)
{
	return 1.0h - pow(1.0h - n, p);
}
float OneMinusPow(float n, float p)
{
	return 1.0f - pow(1.0f - n, p);
}

float3 CalcVAT(TEXTURE2D_PARAM(_VAT, sampler_VAT), float4 _VAT_TexelSize, uint VertexID, float normalizedTime, float2 Bounds)
{
	float3 vertexOffset = 0;
	float2 uv = float2(normalizedTime, (VertexID + 0.5) * _VAT_TexelSize.y);
	float4 vat = SAMPLE_TEXTURE2D_LOD(_VAT, sampler_VAT, uv, 0.0);
	vertexOffset.x = lerp(Bounds.x, Bounds.y, vat.r);
	vertexOffset.y = lerp(Bounds.x, Bounds.y, vat.g);
	vertexOffset.z = lerp(Bounds.x, Bounds.y, vat.b);
	return vertexOffset;
}

float3 PerturbNormal(float3 surf_pos, float3 surf_norm, float height, float scale )
{
	// "Bump Mapping Unparametrized Surfaces on the GPU" by Morten S. Mikkelsen
	float3 vSigmaS = ddx(surf_pos);
	float3 vSigmaT = ddy(surf_pos);
	float3 vN = surf_norm;
	float3 vR1 = cross(vSigmaT, vN);
	float3 vR2 = cross(vN, vSigmaS);
	float fDet = dot(vSigmaS, vR1);
	float dBs = ddx(height);
	float dBt = ddy(height);
	float3 vSurfGrad = scale * 0.05 * sign(fDet) * (dBs * vR1 + dBt * vR2);
	return normalize(abs(fDet) * vN - vSurfGrad);
}

half FastSin(half x)
{
	half2 core = half2(x, x) * half2(0.6366, 0.6366) + half2(-1, -3);
	half2 result = saturate(-core * core + half2(1, 1));
	return result.x - result.y;
}

inline float2 POM(TEXTURE2D(_HeightMap), SAMPLER(sampler_HeightMap), float2 uvs, float2 dx, float2 dy, float3 normalWS, float3 viewDirWS,
	float3 viewDirTS, int minSamples, int maxSamples, float parallax, float refPlane)
{
	int stepIndex = 0;
	int numSteps = (int)lerp((float)maxSamples, (float)minSamples, saturate(dot(normalWS, viewDirWS)));
	float layerHeight = 1.0 / numSteps;
	float2 plane = parallax * (viewDirTS.xy / viewDirTS.z);
	uvs.xy += refPlane * plane;
	float2 deltaTex = -plane * layerHeight;
	float2 prevTexOffset = 0;
	float prevRayZ = 1.0f;
	float prevHeight = 0.0f;
	float2 currTexOffset = deltaTex;
	float currRayZ = 1.0f - layerHeight;
	float currHeight = 0.0f;
	float intersection = 0;
	float2 finalTexOffset = 0;
	while (stepIndex < numSteps + 1)
	{
		currHeight = SAMPLE_TEXTURE2D_GRAD(_HeightMap, sampler_HeightMap, uvs + currTexOffset, dx, dy).r;
	 	if (currHeight > currRayZ)
	 	{
	 	 	stepIndex = numSteps + 1;
	 	}
	 	else
	 	{
	 	 	stepIndex++;
	 	 	prevTexOffset = currTexOffset;
	 	 	prevRayZ = currRayZ;
	 	 	prevHeight = currHeight;
	 	 	currTexOffset += deltaTex;
	 	 	currRayZ -= layerHeight;
	 	}
	}
	int sectionSteps = 2;
	int sectionIndex = 0;
	float newZ = 0;
	float newHeight = 0;
	while (sectionIndex < sectionSteps)
	{
	 	intersection = (prevHeight - prevRayZ) / (prevHeight - currHeight + currRayZ - prevRayZ);
	 	finalTexOffset = prevTexOffset + intersection * deltaTex;
	 	newZ = prevRayZ - intersection * layerHeight;
	 	newHeight = SAMPLE_TEXTURE2D_GRAD(_HeightMap, sampler_HeightMap, uvs + finalTexOffset, dx, dy).r;
	 	if (newHeight > newZ)
	 	{
	 	 	currTexOffset = finalTexOffset;
	 	 	currHeight = newHeight;
	 	 	currRayZ = newZ;
	 	 	deltaTex = intersection * deltaTex;
	 	 	layerHeight = intersection * layerHeight;
	 	}
	 	else
	 	{
	 	 	prevTexOffset = finalTexOffset;
	 	 	prevHeight = newHeight;
	 	 	prevRayZ = newZ;
	 	 	deltaTex = (1 - intersection) * deltaTex;
	 	 	layerHeight = (1 - intersection) * layerHeight;
	 	}
	 	sectionIndex++;
	}
	return uvs.xy + finalTexOffset;
}

#endif