Shader "Custom/Grass"
{
    Properties
    {
        _TopColor("Top Color", Color) = (0, 0.6, 0, 1)
        _BottomColor("Bottom Color", Color) = (0, 0.3, 0, 1)
        _ColorThreshold("Color Threshold", Range(0, 1)) = 0
        _SpecularColor("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularScale("Specular Scale", Range(0, 10)) = 3
        _WindNoise("Wind Noise", 2D) = "white"{}
        _WaveTillingAndSpeed("Wave Tilling And Speed", Vector) = (1, 1, 0.3, 0.3)
        _WindIntensity("Wind Intensity", Range(0, 1)) = 0.5
        _NoiseColor("Noise Color", Color) = (1, 1, 1, 1)
        _ColorNoiseScale("Color Noise Scale", Float) = 1
        _ColorNoiseIntensity("Color Noise Intensity", Range(0, 2)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
        half4 _TopColor;
        half4 _BottomColor;
        half _ColorThreshold;
        half4 _WaveTillingAndSpeed;
        half _WindIntensity;
        half _SpecularScale;
        half4 _SpecularColor;
        half4 _NoiseColor;
        half _ColorNoiseScale;
        half _ColorNoiseIntensity;
        CBUFFER_END

        TEXTURE2D(_WindNoise);
        SAMPLER(sampler_WindNoise);
        
        struct a2v
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
            float3 normalOS : NORMAL;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 normalWS : TEXCOORD1;
            float3 positionWS : TEXCOORD2;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        ENDHLSL
        
        Pass
        {
            Cull Off
            
            HLSLPROGRAM

            #pragma multi_compile_instancing
            
            #pragma vertex vert
            #pragma fragment frag
            
            v2f vert (a2v v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS.xyz);
                o.normalWS = normalInputs.normalWS;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                float3 positionWS = positionInputs.positionWS;
                
                float2 sampleUV = float2(positionWS.x / _WaveTillingAndSpeed.x, positionWS.z / _WaveTillingAndSpeed.y);
                sampleUV.x += _Time.x * _WaveTillingAndSpeed.z;
                sampleUV.y += _Time.x * _WaveTillingAndSpeed.w;
                float waveSample = SAMPLE_TEXTURE2D_LOD(_WindNoise, sampler_WindNoise, sampleUV, 0).r;

                waveSample = (waveSample * 2 - 1) * _WindIntensity;
                positionWS.x += sin(waveSample) * v.uv.y;
                positionWS.z += sin(waveSample) * v.uv.y;
                o.positionWS = positionWS;
                o.positionCS = TransformWorldToHClip(positionWS);
                o.uv = v.uv;
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                Light main = GetMainLight();
                half3 viewDirWS = GetCameraPositionWS() - i.positionWS;
                half3 halfDir = normalize(viewDirWS + main.direction);
                half3 specular = max(dot(normalize(i.normalWS), halfDir), 0);
                specular = lerp(0, specular, i.uv.y);
                specular = pow(specular, _SpecularScale);
                half3 color = lerp(_BottomColor, _TopColor, i.uv.y + _ColorThreshold).rgb + specular * _SpecularColor.rgb;

                float2 colorSampleUV = i.positionWS.xz / _ColorNoiseScale;
                half colorNoise = SAMPLE_TEXTURE2D(_WindNoise, sampler_WindNoise, colorSampleUV).r;
                colorNoise = saturate(colorNoise * _ColorNoiseIntensity);
                color = lerp(_NoiseColor.rgb, color, colorNoise);
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
