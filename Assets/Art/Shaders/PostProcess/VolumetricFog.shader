Shader "Athena/VolumetricFog/Fog"
{
    Properties
    {
        [Header(FogColor)]
        _FogColor("雾的颜色", Color) = (1,1,1,1)
        [Toggle]_EnableLightColor("受光照影响", Float) = 1.0
        _FogAlpha("雾的透视度", Range(0.0, 10.0)) = 0.75
        _Density("雾的密度", Range(0.0, 1.0)) = 0.01
        _FogColorIntensity("光照强度", Range(0.0, 20.0)) = 5.0
        _ShadowIntensity("阴影强度", Range(0.0, 1.0)) = 1.0
        _HGFactor("相位系数（大于0逆光更亮）", Range(-0.96, 0.96)) = 0.3
        _MaxLightValue("亮度最大值Clamp", Range(0, 10)) = 10

        [Header(ColorGradient)]
        [Toggle]_ColorGradientEnable("颜色渐变", Float) = 0.0
        _FogColorBottom("底部颜色", Color) = (1,1,1,1)
        _FogColorBottomPower("颜色渐变范围", Range(0.0, 1.0)) = 0.5
        _BottomShadow("底部变暗", Range(0.0, 1.0)) = 0.0
        _TopShadow("顶部变暗", Range(0.0, 1.0)) = 0.0

        [Header(HeightGradient)]
        [KeywordEnum(No, Multiply, Subtract)]_HeightTransitionEnable("密度渐变", Float) = 0.0
        _MinHeight("渐变开始高度", Range(-1, 1)) = 0
        _MaxHeight("渐变结束高度", Range(0, 2)) = 1
        _HeightPower("渐变曲线（值越大前面变化越快）", Range(0.1,10)) = 1

        [Toggle(_UseHeightMap)]_HeightMapEnable("使用高度图", Float) = 0.0
        [NoScaleOffset][SinglelineTexture] _HeightMap("高度图", 2D) = "black" {}
        //        [HideInInspector] _LowReduce("削弱低处雾", Range(0.0, 1.0)) = 0.0
        //        [HideInInspector] _LowReduceStart("削弱低处雾的范围", Range(0.01, 1.0)) = 0.0
        //        [HideInInspector] _HeightMapCenterRange("高度图中心和范围", Vector) = (0,0,0,100)
        //        [HideInInspector] _HeightMapDepth("高度图深度", Float) = 10

        [Header(NoiseMap)]
        [KeywordEnum(NoTexture, 3DTexture, 2DTexture)]_VolumeMapEnable("使用噪声模拟雾的密度和飘动效果", Float) = 0.0
        [NoScaleOffset][SinglelineTexture] _VolumeMap("3D噪声图", 3D) = "white" {}

        _VolumeMapSpeedAll("整体速度", Vector) = (1,1,1,1)
        _VolumeMapSpeedScale("整体Scale", Range(0.0, 2.0)) = 1.0
        _VolumeMapSpeedDown("底部Scale渐变", Range(0.0, 2.0)) = 1.0

        [KeywordEnum(R, RG, RGB, RGBA)] _NoiseLayerCount("噪声层数", Int) = 0

        _VolumeMapScale("噪声Scale(R)", Range(0.0, 0.2)) = 0.1
        _VolumeMapIntensity("噪声影响强度(R)", Range(0.0, 2.0)) = 1.0
        _VolumeMapPow("噪声Pow(R)", Range(0.0, 10.0)) = 1.0
        _VolumeMapSpeed("雾的飘动速度(R)", Vector) = (0.3,0,0,1)

        [ShowIfTwoPropertyCondition(_NoiseLayerCount, Greater, 0)] _VolumeMapScale2("噪声Scale(G)", Range(0.0, 0.2)) = 0.05
        [ShowIfTwoPropertyCondition(_NoiseLayerCount, Greater, 0)] _VolumeMapIntensity2("噪声影响强度(G)", Range(0.0, 2.0)) = 0.4
        [ShowIfTwoPropertyCondition(_NoiseLayerCount, Greater, 0)] _VolumeMapPow2("噪声Pow(G)", Range(0.0, 10.0)) = 1.0
        [ShowIfTwoPropertyCondition(_NoiseLayerCount, Greater, 0)] _VolumeMapSpeed2("雾的飘动速度(G)", Vector) = (0.3,0,0,1)

        [ShowIfTwoPropertyCondition(_NoiseLayerCount, Greater, 1)] _VolumeMapScale3("噪声Scale(B)", Range(0.0, 0.2)) = 0.05
        [ShowIfTwoPropertyCondition(_NoiseLayerCount, Greater, 1)] _VolumeMapIntensity3("噪声影响强度(B)", Range(0.0, 2.0)) = 0.3
        [ShowIfTwoPropertyCondition(_NoiseLayerCount, Greater, 1)] _VolumeMapPow3("噪声Pow(B)", Range(0.0, 10.0)) = 1.0
        [ShowIfTwoPropertyCondition(_NoiseLayerCount, Greater, 1)] _VolumeMapSpeed3("雾的飘动速度(B)", Vector) = (0.3,0,0,1)

        [ShowIfTwoPropertyCondition(_NoiseLayerCount, Greater, 2)] _VolumeMapScale4("噪声Scale(A)", Range(0.0, 0.2)) = 0.05
        [ShowIfTwoPropertyCondition(_NoiseLayerCount, Greater, 2)] _VolumeMapIntensity4("噪声影响强度(A)", Range(0.0, 2.0)) = 0.2
        [ShowIfTwoPropertyCondition(_NoiseLayerCount, Greater, 2)] _VolumeMapPow4("噪声Pow(A)", Range(0.0, 10.0)) = 1.0
        [ShowIfTwoPropertyCondition(_NoiseLayerCount, Greater, 2)] _VolumeMapSpeed4("雾的飘动速度(A)", Vector) = (0.3,0,0,1)

        [NoScaleOffset][SinglelineTexture][ShowIf(_VOLUMEMAPENABLE_2DTEXTURE)] _CloudNoiseMap("2D噪声图", 2D) = "white" {}
        [ShowIfTwoPropertyCondition(_VolumeMapEnable, Equal, 2)] _VolumeHeight("2D噪声高度强度", Range(0.0, 2.0)) = 1

        [Header(DetailNoiseMap)]
        [Toggle(_UseDetailNoise)]_DetailNoiseEnable("使用细节噪声", Float) = 0.0
        [NoScaleOffset][SinglelineTexture]_DetailNoiseMap("细节噪声图", 3D) = "white" {}
        _DetailNoiseMapScale("细节噪声Scale", Range(0.0, 0.1)) = 0.05
        _DetailNoiseIntensity("细节噪声强度", Range(0.0, 10.0)) = 1.0
        _DetailNoiseSpeed("细节噪声速度", Range(0.0, 0.1)) = 0.01

        [Header(MaxDistance)]
        [Toggle(_UseMaxDistance)]_MaxDistanceEnable("体积雾最远距离（体积雾范围过大时开启，提高精度）", Float) = 1.0
        _MaxDistance("体积雾最远距离", Float) = 50

        [Header(BorderTransition)]
        [Toggle(_UseBorderTransition)]_BorderGradientEnable("开启边缘过渡（Cube不能有旋转）", Float) = 0.0
        _BorderTransition("XZ方向过渡程度", Range(0, 1)) = 0

        [Header(NearDamp)]
        [Toggle(_UseNearDamp)]_NearDampEnable("近处没有雾", Float) = 0.0
        _FogStartDistance("没雾的范围", Float) = 10.0
        _DampDistance("渐变范围", Float) = 10.0

        [Header(SelfShadow)][Toggle(_UseSelfShadow)]_SelfShadowEnable("开启自阴影", Float) = 0.0
        _SelfShadowOffset("自阴影偏移距离", Range(0, 5)) = 1
        _SelfShadowIntensity("自阴影强度", Range(0, 3)) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent" "Queue" = "Transparent+1" "RenderPipeline" = "UniversalPipeline" "DisableBatching"="True"
        }
        LOD 300

        Pass
        {
            Tags
            {
                "LightMode" = "VolumetricFog"
            }

            ZTest Always
            ZWrite Off
            Cull Front
            Blend One SrcAlpha, DstAlpha Zero
            // Blend One SrcAlpha, Zero One

            HLSLPROGRAM
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _NoiseEnable
            #pragma multi_compile _ _HighQuality
            #pragma shader_feature_local _VOLUMEMAPENABLE_NOTEXTURE _VOLUMEMAPENABLE_3DTEXTURE _VOLUMEMAPENABLE_2DTEXTURE
            #pragma shader_feature_local _HEIGHTTRANSITIONENABLE_NO _HEIGHTTRANSITIONENABLE_MULTIPLY _HEIGHTTRANSITIONENABLE_SUBTRACT
            #pragma shader_feature_local _UseMaxDistance
            #pragma shader_feature_local _UseBorderTransition
            #pragma shader_feature_local _UseDetailNoise
            #pragma shader_feature_local _UseHeightMap
            #pragma shader_feature_local _UseNearDamp
            #pragma shader_feature_local _UseSelfShadow

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half3 _FogColor;
            half _ColorGradientEnable;
            half3 _FogColorBottom;
            half _FogColorBottomPower;
            half _FogColorIntensity;
            half _ShadowIntensity;
            half _FogAlpha;
            half _EnableLightColor;

            half4 _VolumeMapSpeedAll;
            half _VolumeMapSpeedScale;
            half _VolumeMapSpeedDown;
            half _BottomShadow;
            half _TopShadow;
            int _NoiseLayerCount;
            half _VolumeMapScale;
            half _VolumeMapIntensity;
            half4 _VolumeMapSpeed;
            half _VolumeMapPow;
            half _VolumeMapScale2;
            half _VolumeMapIntensity2;
            half4 _VolumeMapSpeed2;
            half _VolumeMapPow2;
            half _VolumeMapScale3;
            half _VolumeMapIntensity3;
            half4 _VolumeMapSpeed3;
            half _VolumeMapPow3;
            half _VolumeMapScale4;
            half _VolumeMapIntensity4;
            half4 _VolumeMapSpeed4;
            half _VolumeMapPow4;

            half _VolumeHeight;
            half _Density;
            half _HGFactor;
            half _MaxLightValue;

            float _MinHeight;
            float _MaxHeight;
            float _HeightPower;

            // half _LowReduce;
            // half _LowReduceStart;
            // float4 _HeightMapCenterRange;
            // float _HeightMapDepth;
            // half _HeightBottomGradient;

            float _MaxDistance;
            half _BorderTransition;

            half _DetailNoiseMapScale;
            half _DetailNoiseIntensity;
            half _DetailNoiseSpeed;

            float _FogStartDistance;
            float _DampDistance;

            float _SelfShadowOffset;
            half _SelfShadowIntensity;
            CBUFFER_END

            float4 _FogTextureSize;
            float4 _FogParam;
            float4 _NoiseMap_TexelSize;
            float4x4 _MyInvViewProjMatrix; // Unity提供的VP逆矩阵在不同版本处理不同，这里自己传入

            #define _Jitter _FogParam.x
            #define _NoiseScale _FogParam.z
            // #define _Step_MAX uint(_FogParam.y)
            #if _HighQuality
            #define _Step_MAX 64
            #else
            #define _Step_MAX 8
            #endif

            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);
            TEXTURE2D_FLOAT(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            TEXTURE3D(_VolumeMap);
            SAMPLER(sampler_VolumeMap);
            TEXTURE3D(_DetailNoiseMap);
            SAMPLER(sampler_DetailNoiseMap);
            TEXTURE2D_FLOAT(_HeightMap);
            SAMPLER(sampler_HeightMap);
            TEXTURE2D(_CloudNoiseMap);
            SAMPLER(sampler_CloudNoiseMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float3 posWS : TEXCOORD0;
                float4 screenUV : TEXCOORD1;
                float4 posCS : SV_POSITION;
            };

            struct RayMarchData
            {
                float3 stepVec3;
                float stepDistance;
                float3 curJitterPos; //目前只对阴影部分采样使用JitterPos，采噪声容易有噪点，噪声还是太高频了
                float3 curPos;
                float3 center;
                float3 range;
                half density;
                float3 pos01;
                float fixedHeight01;
            };

            #include "SpatialHelper.hlsl"
            #include "VolumeLighting.hlsl"

            half GetNoise(float2 uv)
            {
                #if _NoiseEnable
                    uv = uv * _FogTextureSize.xy / _NoiseMap_TexelSize.xy;
                    return SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, uv).r * _NoiseScale;
                #else
                {
                    return 0;
                }
                #endif
            }

            half PhaseHG(float3 lightDir, float3 viewDir)
            {
                half g = _HGFactor;
                half x = 1 + g * dot(viewDir, lightDir);
                return (1 - g * g) / (x * x);
                // 1.5-exponent is quite costly, use Schlick phase function approximation
                // http://www2.imm.dtu.dk/pubdb/edoc/imm6267.pdf
                //return (1 - g * g) / (pow(1 + g * g - 2 * g * dot(viewDir, lightDir), 1.5));
            }

            RayMarchData InitRayMarchData(float3 startPos, float rayMarchLength, float3 dir, float2 screenUV)
            {
                RayMarchData data;
                float stepDelta = 1.0 / _Step_MAX;
                data.stepDistance = stepDelta * rayMarchLength;
                data.stepVec3 = data.stepDistance * dir;
                data.curPos = startPos;
                data.curJitterPos = startPos + data.stepVec3 * (frac(_Jitter + GetNoise(screenUV)) - 0.5);
                data.density = max(_Density, 0.000001);
                GetCenterAndRange(data.center, data.range);
                UpdatePosInfo(data.curPos, data, data.pos01, data.fixedHeight01);
                return data;
            }

            void UpdateNextStep(inout RayMarchData data)
            {
                data.curJitterPos += data.stepVec3;
                data.curPos += data.stepVec3;
                UpdatePosInfo(data.curPos, data, data.pos01, data.fixedHeight01);
            }

            half4 RayMarching(float3 startPos, float rayMarchLength, float3 dir, float2 screenUV)
            {
                half4 accuColor = half4(0, 0, 0, 1);
                if (rayMarchLength > 0)
                {
                    RayMarchData data = InitRayMarchData(startPos, rayMarchLength, dir, screenUV);
                    Light light = GetMainLight();
                    [unroll(_Step_MAX)]
                    for (uint i = 0; i < _Step_MAX; i++)
                    {
                        half curDensity = GetDensity(data, data.density, data.curPos, data.pos01, data.fixedHeight01);
                        CalculateLighting(data, curDensity, light, accuColor);
                        UpdateNextStep(data);
                    }

                    float phase = PhaseHG(light.direction, -dir);
                    accuColor.rgb = min(_MaxLightValue, phase * accuColor.rgb);
                    if (_EnableLightColor > 0)
                        accuColor.rgb *= light.color;

                    accuColor.a = saturate(1 - (1 - accuColor.a) * _FogAlpha);
                }

                return accuColor;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.posCS = vertexInput.positionCS;
                output.posWS = vertexInput.positionWS;
                output.screenUV = ComputeScreenPos(output.posCS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDirWS = normalize(input.posWS - GetCameraPositionWS());
                float2 screenUV = input.screenUV.xy / input.screenUV.w;
                float4 intersect = IntersectCube(viewDirWS, screenUV);
                half4 res = RayMarching(intersect.xyz, intersect.w, viewDirWS, screenUV);
                if (all(res.rgb == 0))
                {
                    res.a = 1;
                }
                return res;
            }
            ENDHLSL
        }
    }
}