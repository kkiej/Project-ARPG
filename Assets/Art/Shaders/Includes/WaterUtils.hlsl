#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
float2 Panner(float2 uv, float direction, float speed, float2 offset, float tiling)
{
    direction = direction * 2 - 1;
    float2 d = normalize(float2(cos(3.14 * direction), sin(3.14 * direction)));
    return  (d * _Time.y * speed) + (uv * tiling) + offset;
}

void NormalsUV_half(half2 UV, half2 movement, out half2 UV1, out half2 UV2)
{
    half speed1 = movement.x * -0.5;
    half speed2 = movement.x;

    half tiling1 = movement.y * 0.5;
    half tiling2 = movement.y;

    UV1 = Panner(UV, 1, speed1, 0, 1/tiling1);
    UV2 = Panner(UV, 1, speed2, 0, 1/tiling2);
}

float3 ReconstructWorldPos(float3 viewDirWS, float screenPos_w, float2 screenPos, float2 screenPos_offset, float3 positionVS)
{
    float3 positionWS_Perspective = -viewDirWS.xyz / screenPos_w * LinearEyeDepth(SampleSceneDepth(screenPos_offset), _ZBufferParams) + _WorldSpaceCameraPos;
    
    float sceneDepth = SampleSceneDepth(screenPos);
    sceneDepth = _ProjectionParams.x > 0 ? sceneDepth : 1 - sceneDepth;
    // convert to world units
    sceneDepth = lerp(_ProjectionParams.y, _ProjectionParams.z, sceneDepth);

    float3 positionWS_Ortho = mul(UNITY_MATRIX_I_V, float4(positionVS.xy, -sceneDepth, 1)).xyz;

    float3 positionWS = lerp(positionWS_Perspective, positionWS_Ortho, unity_OrthoParams.w);
    return positionWS;
}

float DepthFade_Exponential(float3 positionWS, float3 viewDirWS, float screenPos_w, float2 screenPos, float2 screenPos_offset, float3 positionVS, float3 normalOS, float Distance)
{
    float3 positionWS_subtract = positionWS - ReconstructWorldPos(viewDirWS, screenPos_w, screenPos, screenPos_offset, positionVS);
    float temp = normalOS.x * positionWS_subtract.x + normalOS.y * positionWS_subtract.y + normalOS.z * positionWS_subtract.z;
    return saturate(exp(-temp / Distance));
}

float DepthFade_Linear(float3 positionWS, float3 viewDirWS, float screenPos_w, float2 screenPos, float2 screenPos_offset, float3 positionVS, float3 normalOS, float Distance)
{
    float3 positionWS_subtract = positionWS - ReconstructWorldPos(viewDirWS, screenPos_w, screenPos, screenPos_offset, positionVS);
    float temp = normalOS.x * positionWS_subtract.x + normalOS.y * positionWS_subtract.y + normalOS.z * positionWS_subtract.z;
    return saturate(temp / Distance);
}

float DepthFade_Cheap(float2 screenPos, float screenPos_w, float Distance)
{
    float depth = 1 - saturate((SampleSceneDepth(screenPos) - screenPos_w) / Distance);
    return depth;
}