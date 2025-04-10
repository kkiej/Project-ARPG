#ifndef VolumeLighting_Include
#define VolumeLighting_Include

void GetCenterAndRange(out float3 center, out float3 scale)
{
    float4x4 objectToWorld = GetObjectToWorldMatrix();
    center = float3(objectToWorld._14, objectToWorld._24, objectToWorld._34);
    scale = float3(objectToWorld._11, objectToWorld._22, objectToWorld._33);
}

float3 CalculatePos01(float3 pos, float3 center, float3 scale)
{
    return saturate((pos - center) / scale + 0.5);
}

float Remap(float original_value, float original_min, float original_max, float new_min, float new_max)
{
    return new_min + (original_value - original_min) / (original_max - original_min) * (new_max - new_min);
}

float RemapClamped(float original_value, float original_min, float original_max, float new_min,
                   float new_max)
{
    return new_min + (saturate((original_value - original_min) / (original_max - original_min)) * (new_max -
        new_min));
}

half CalculateDamp(float3 pos, float3 pos01)
{
    half damp = 1;
    #if _UseBorderTransition
    {
        #if 0 //局部空间需要矩阵运算比较费
        {
            float3 distanceToBorder = 0.5 - abs(TransformWorldToObject(pos));
            float minDis = min(distanceToBorder.x, distanceToBorder.z);
            damp *= smoothstep(0, _BorderTransition * 0.5, minDis);
        }
        #else
        {
            // 使用模型矩阵计算，矩阵不能带旋转
            float2 distanceToBorder = 0.5 - abs(pos01.xz - 0.5);
            float minDis = min(distanceToBorder.x, distanceToBorder.y);
            damp *= smoothstep(0, _BorderTransition * 0.5, minDis);
        }
        #endif
    }
    #endif

    #if _UseNearDamp
    {
        float distance = length(GetCameraPositionWS() - pos);
        damp *= saturate((distance - _FogStartDistance) / _DampDistance);
    }
    #endif

    return damp;
}

float CalculateFixedHeight01(RayMarchData data, float3 pos, float height01)
{
    float fixHeight01 = height01;
    #if _HEIGHTTRANSITIONENABLE_MULTIPLY | _HEIGHTTRANSITIONENABLE_SUBTRACT
    {
        float min = _MinHeight;
        #if _UseHeightMap
        {
            float2 heightUv = (pos.xz - data.center.xz) / max(data.range.x, data.range.z) + 0.5;
            float sample01 = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, heightUv, 1).r;
            min += sample01;
            // 低处削弱 0是底部
            //volumeNoise *= RemapClamped(sample01, 0, _LowReduceStart, 1 - _LowReduce, 1);
        }
        #endif

        fixHeight01 = (height01 - min) / max(0.01, _MaxHeight - _MinHeight);
        fixHeight01 = 1 - pow(1 - saturate(fixHeight01), _HeightPower);
    }
    #endif

    return fixHeight01;
}

void UpdatePosInfo(float3 pos, RayMarchData data, out float3 pos01, out float fixedHeight01)
{
    pos01 = CalculatePos01(pos, data.center, data.range);
    fixedHeight01 = CalculateFixedHeight01(data, pos, pos01.y);
}

half CalculateNoise(float3 pos, float3 pos01, float height01)
{
    half volumeNoise = 1;
    float3 posUv = pos;
    #if _UseDetailNoise
    {
        float3 detailUV = posUv * _DetailNoiseMapScale + _Time.y * -_DetailNoiseSpeed;
        half3 detailNoise = SAMPLE_TEXTURE3D(_DetailNoiseMap, sampler_DetailNoiseMap, detailUV).xyz;
        detailNoise = lerp(0.5, detailNoise, _DetailNoiseIntensity * 3) - 0.5;
        posUv += detailNoise;
    }
    #endif

    #if _VOLUMEMAPENABLE_3DTEXTURE
        posUv *= _VolumeMapSpeedScale;
        float3 time = -_Time.y * _VolumeMapSpeedAll.xyz;
        float3 volumeUV = time * _VolumeMapSpeed.xyz + posUv * _VolumeMapScale;
        volumeNoise = SAMPLE_TEXTURE3D(_VolumeMap, sampler_VolumeMap, volumeUV).x;
        volumeNoise = pow(volumeNoise, _VolumeMapPow) * _VolumeMapIntensity;
        
        if(_NoiseLayerCount > 0.5)
        {
            float3 volumeUV2 = time * _VolumeMapSpeed2.xyz + posUv * _VolumeMapScale2;
            half volumeNoise2 = SAMPLE_TEXTURE3D(_VolumeMap, sampler_VolumeMap, volumeUV2).y;
            volumeNoise2 = pow(volumeNoise2, _VolumeMapPow2) * _VolumeMapIntensity2;
            volumeNoise += volumeNoise2;
        }

        half volemeNoiseSmall = 0;
        if(_NoiseLayerCount > 1.5)
        {
            float3 volumeUV3 = time * _VolumeMapSpeed3.xyz + posUv * _VolumeMapScale3;
            half volumeNoise3 = SAMPLE_TEXTURE3D(_VolumeMap, sampler_VolumeMap, volumeUV3).z;
            volumeNoise3 = pow(volumeNoise3, _VolumeMapPow3) * _VolumeMapIntensity3;
            volemeNoiseSmall += volumeNoise3;
        }

        if(_NoiseLayerCount > 2.5)
        {
            float3 volumeUV4 = time * _VolumeMapSpeed4.xyz + posUv * _VolumeMapScale4;
            half volumeNoise4 = SAMPLE_TEXTURE3D(_VolumeMap, sampler_VolumeMap, volumeUV4).z;
            volumeNoise4 = pow(volumeNoise4, _VolumeMapPow4) * _VolumeMapIntensity4;
            volemeNoiseSmall += volumeNoise4;
        }

        volumeNoise += lerp(0, volemeNoiseSmall, pow(height01, _VolumeMapSpeedDown));

    #elif _VOLUMEMAPENABLE_2DTEXTURE
        float2 volumeUV2 = _Time.y * -_VolumeMapSpeed.xz + posUv.xz * _VolumeMapScale;
        half4 sampleTex = SAMPLE_TEXTURE2D(_CloudNoiseMap, sampler_CloudNoiseMap, volumeUV2);
        // float2 uv3 = _Time.y * _VolumeMapSpeed.y + pos.xz * _VolumeMapScale * _VolumeMapSpeed.w;
        // half smallNoise = SAMPLE_TEXTURE3D(_CloudNoiseMap, sampler_CloudNoiseMap, uv3).a;
        volumeNoise = sampleTex.r;
        volumeNoise = lerp(1, volumeNoise, _VolumeMapIntensity);
        volumeNoise *= 1 - saturate(pos01.y / (sampleTex.g * _VolumeHeight));
    #endif

    return volumeNoise;
}

half GetDensity(RayMarchData data, half density, float3 pos, float3 pos01, float fixedHeight01)
{
    half volumeNoise = CalculateNoise(pos, pos01, fixedHeight01);

    #if _HEIGHTTRANSITIONENABLE_MULTIPLY
        volumeNoise *= 1 - fixedHeight01;
    #elif _HEIGHTTRANSITIONENABLE_SUBTRACT
        volumeNoise = saturate(saturate(volumeNoise) - fixedHeight01);// 减法不会降低对比度，保留更多细节
    #endif

    density *= volumeNoise;

    density *= CalculateDamp(pos, pos01);

    return density;
}

half CalculateShadow(Light light, RayMarchData data)
{
    half shadow = light.shadowAttenuation * light.distanceAttenuation;
    shadow *= MainLightRealtimeShadow(TransformWorldToShadowCoord(data.curJitterPos));
    //shadow = step(0.98, shadow); // 有的项目会修改阴影强度导致体积光不够锐利
    shadow = LerpWhiteTo(shadow, _ShadowIntensity);
    #if _UseSelfShadow //自阴影多算一次密度，挺费
    {
        float3 shadowPos = data.curPos + light.direction * _SelfShadowOffset;
        float3 shadowPos01;
        float fixedHeight01;
        UpdatePosInfo(shadowPos, data, shadowPos01, fixedHeight01);
        half shadowDensity = GetDensity(data, data.density, shadowPos, shadowPos01, fixedHeight01);
        //selfShadow *= saturate(1 - shadowDensity * _SelfShadowIntensity);
        shadow *= exp(- shadowDensity * _SelfShadowIntensity * 30);
    }
    #endif
    return shadow;
}

void CalculateLighting(RayMarchData data, half curDensity, Light light, inout half4 accuColor)
{
    half shadow = CalculateShadow(light, data);
    half3 fogColor = _FogColor;
    if (_ColorGradientEnable > 0)
    {
        fogColor = lerp(_FogColorBottom, _FogColor, saturate(data.fixedHeight01 * (1 - _FogColorBottomPower) * 4));
        curDensity *= lerp(1 - _BottomShadow, 1 - _TopShadow, data.fixedHeight01);
        // fogColor = lerp(_FogColorBottom, _FogColor, saturate(data.fixedHeight01 * _FogColorBottomPower));
        // curDensity *= lerp(1 - _BottomShadow, 1, saturate(data.fixedHeight01 * _FogColorBottomPower));
    }
    // https://www.ea.com/frostbite/news/physically-based-unified-volumetric-rendering-in-frostbite
    // lighting = light * shadow * density
    // sliceLightIntegral = lighting * (1 - exp(-density * stepDistance)) / density
    //                    = light * shadow * (1 - exp(-density * stepDistance))
    half lighting = _FogColorIntensity * shadow;
    float sliceTransmittance = exp(-curDensity * data.stepDistance);
    float sliceLightIntegral = lighting;
    sliceLightIntegral *= (1.0 - sliceTransmittance);
    accuColor.a *= sliceTransmittance;
    accuColor.rgb += sliceLightIntegral * accuColor.a * fogColor; // 乘上一层的透射率
}

#endif
