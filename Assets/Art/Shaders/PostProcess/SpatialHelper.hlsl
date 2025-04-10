#ifndef SpatialHelper_Include
#define SpatialHelper_Include

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

float3 ReconstructWorldPos(float2 screenPos, float depth)
{
    #if defined(SHADER_API_GLCORE) || defined (SHADER_API_GLES) || defined (SHADER_API_GLES3)
    depth = depth * 2 - 1;
    #endif
    // #if UNITY_UV_STARTS_AT_TOP
    // {
    //     screenPos.y = 1 - screenPos.y;
    // }
    // #endif
    float4 raw = mul(_MyInvViewProjMatrix, float4(screenPos * 2 - 1, depth, 1));
    float3 worldPos = raw.rgb / raw.a;
    return worldPos;
}

// zBufferParam = { (f-n)/n, 1, (f-n)/n*f, 1/f }
float LinearDepthToZBuffer(float linearDepth, float4 zBufferParam)
{
    return (1.0 / linearDepth - zBufferParam.w) / zBufferParam.z;
}

float GetMaxDepth(float2 screenUV)
{
    float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV);
    #if _UseMaxDistance
    {
        #if UNITY_REVERSED_Z
        {
            depth = max(depth, LinearDepthToZBuffer(_MaxDistance, _ZBufferParams));
        }
        #else
        depth = min(depth, LinearDepthToZBuffer(_MaxDistance, _ZBufferParams));
        #endif
    }

    #endif
    return depth;
}

float3 TransformWorldToObjectDirFloat(float3 dirWS)
{
    return normalize(mul((float3x3)GetWorldToObjectMatrix(), dirWS));
}

float4 IntersectCube(float3 viewDir, float2 screenUV)
{
    float maxDepth = GetMaxDepth(screenUV);
    float3 maxPosWS = ReconstructWorldPos(screenUV, maxDepth);
    float3 localCameraPos = TransformWorldToObject(GetCameraPositionWS());
    float3 localViewDir = TransformWorldToObjectDirFloat(viewDir);
    float3 invLocalViewDir = 1.0 / localViewDir;
    float3 intersect1 = (-0.5 - localCameraPos) * invLocalViewDir;
    float3 intersect2 = (0.5 - localCameraPos) * invLocalViewDir;
    float3 tEnterVec3 = min(intersect1, intersect2);
    float3 tExitVec3 = max(intersect1, intersect2);
    float tEnter = max(max(tEnterVec3.x, tEnterVec3.y), tEnterVec3.z);
    float tExit = min(min(tExitVec3.x, tExitVec3.y), tExitVec3.z);

    float3 localMaxPos = TransformWorldToObject(maxPosWS);
    float tMax = length(localMaxPos - localCameraPos); // / length(localViewDir);
    tEnter = min(tEnter, tMax);
    tExit = min(tExit, tMax);

    tEnter = max(tEnter, 0);
    tExit = max(tEnter, tExit);
    float3 localStartPos = localCameraPos + localViewDir * tEnter;
    float3 localEndPos = localCameraPos + localViewDir * tExit;
    float3 worldStartPos = TransformObjectToWorld(localStartPos);
    float3 worldEndPos = TransformObjectToWorld(localEndPos);
    return float4(worldStartPos, length(worldEndPos - worldStartPos));
}

#endif