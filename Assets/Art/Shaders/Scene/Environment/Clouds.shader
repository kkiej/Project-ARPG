Shader "Ro/Clouds"
{
    Properties
    {
        _CloudTex ("Cloud Tex", 2D) = "white" {}
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimColor1("Rim Color1", Color) = (1, 1, 1, 1)
        _RimRadius("Rim Radius", Range(0,50)) = 0
        _RimStrength("Rim Strength", Range(0,1)) = 1
        _ColorLerpSize("Color LerpSize", Range(0,1)) = 1
        _ColorLerpStrength("Color LerpStrength", Range(0, 1)) = 1
        _BrightColor("Bright Color", Color) = (1, 1, 1, 1)
        _BrightColor1("Bright Color1", Color) = (1, 1, 1, 1)
        _DarkColor("Dark Color", Color) = (1, 1, 1, 1)
        _DarkColor1("Dark Color1", Color) = (1, 1, 1, 1)
        _CloudEdgeNoise("Cloud Edge Noise", 2D) = "white" {}
        _LerpTimeOffset("Lerp Time Offset", Float) = 0
        _LerpSpeed("Lerp Speed", Float) = 0
        _LerpCtrl("Lerp Ctrl", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
        half4 _RimColor;
        half4 _RimColor1;
        half _RimRadius;
        half _RimStrength;
        half _ColorLerpSize;
        half _ColorLerpStrength;
        half4 _BrightColor;
        half4 _BrightColor1;
        half4 _DarkColor;
        half4 _DarkColor1;
        float4 _CloudEdgeNoise_ST;
        float4 _CloudTex_ST;
        half _LerpTimeOffset;
        half _LerpSpeed;
        half _LerpCtrl;
        
        half4 _BrightC1;
        half4 _BrightC2;
        half4 _BrightC3;
        half4 _BrightC4;
        half4 _ShadowC1;
        half4 _ShadowC2;
        half4 _ShadowC3;
        half4 _ShadowC4;
        half _AlbedoControl;
        half _BloomRadius;
        half _BloomIntensity;

        CBUFFER_END

        ENDHLSL

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            
            HLSLPROGRAM
            
            #pragma multi_compile_fog
            
            #pragma vertex vert
            #pragma fragment frag
            
            struct a2v
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float3 normalOS : TEXCOORD2;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float4 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float fogCoord : TEXCOORD2;
            };

            TEXTURE2D(_CloudEdgeNoise);
            SAMPLER(sampler_CloudEdgeNoise);
            TEXTURE2D(_CloudTex);
            SAMPLER(sampler_CloudTex);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);
            float4x4 _CloudLToW;

            v2f vert(a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv.xy = v.uv;
                o.uv.zw = v.uv1;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.fogCoord = ComputeFogFactor(o.positionCS.z);
            
                return o;
            }
            
            float computeCloudSize(float SDF, float4 cloudTex, half _LerpCtrl)
            {   
                half cloudStep = 1 - _LerpCtrl;
                half cloudLerp = smoothstep(0.95, 1, _LerpCtrl);
                half alpha = smoothstep(saturate(cloudStep - 0.1), cloudStep, SDF);
                
                return lerp(alpha, cloudTex.a, cloudLerp);
            }
            
            half4 frag(v2f i):SV_Target
            {                
                //距离太阳范围
                #define PI 3.1415926
                
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 normalWS = normalize(i.normalWS);
                float3 objLightDir = normalize(mul(mainLight.direction, _CloudLToW));
                float2 cosin = objLightDir.xz;
                
                float theta = acos(cosin.x) * step(0, cosin.y) + (2 * PI - acos(cosin.x)) * step(cosin.y, 0);
                theta /= 2 * PI;
                
                float temp = abs(i.uv.z - theta);
                float xDis = temp * step(temp, 0.5) + (1 - temp) * step(1 - temp, 0.5);
                float yDis = abs(i.uv.w - (objLightDir.y * 0.37 + 0.49));
                //根据距离计算边缘光
                float RimMask = (1 - smoothstep(0, 0.008 * _RimRadius, xDis * xDis + yDis * yDis)) * _RimStrength;
                _RimColor += RimMask;
                //根据距离太阳位置进行颜色的lerp
                float colorLerp = smoothstep(0, 0.1 * _ColorLerpSize, xDis * xDis + yDis * yDis) * _ColorLerpStrength;
                
                _BrightColor = lerp(_BrightColor, _BrightColor1, colorLerp);
                _DarkColor = lerp(_DarkColor, _DarkColor1, colorLerp);
                //_RimColor = lerp(_RimColor, _RimColor1, colorLerp);
            
                half4 cloudEdgeNoise = SAMPLE_TEXTURE2D(_CloudEdgeNoise, sampler_CloudEdgeNoise, i.uv.xy * _CloudEdgeNoise_ST.xy + _CloudEdgeNoise_ST.zw * _Time.x);
                float2 cloudNoiseUV = i.uv.xy + float2(cloudEdgeNoise.x - 1, cloudEdgeNoise.y - 0.25) * 0.02;
                cloudNoiseUV = cloudNoiseUV * _CloudTex_ST.xy + _CloudTex_ST.zw;

                half noiseTex = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv.xy * 5 + _Time.y / 30).r;
                half4 cloudTex = SAMPLE_TEXTURE2D(_CloudTex, sampler_CloudTex, cloudNoiseUV);
                //half4 cloudTex = SAMPLE_TEXTURE2D(_CloudTex, sampler_CloudTex, i.uv.xy + noiseTex / 40);
                //half4 cloudLerpTex = SAMPLE_TEXTURE2D(_CloudLerpTex, sampler_CloudLerpTex, cloudNoiseUV);
                float SDF = cloudTex.z;
                                
                half finalAlpha =  computeCloudSize(SDF, cloudTex, lerp(sin((_Time.x + _LerpTimeOffset) * _LerpSpeed) * 0.78 + 0.78, 1, _LerpCtrl));
                
                half3 finalColor = _BrightColor * cloudTex.r
                                    + _DarkColor * (1 - cloudTex.r)
                                    + (_RimColor - 0.7) * (cloudTex.g * 0.5 + 1 - smoothstep(0.5, 1, finalAlpha));
                half3 aaa = ((_RimColor - 0.7) * (cloudTex.g * 0.5 + 1 - smoothstep(0.5, 1, finalAlpha))).xyz;
                //finalColor.xyz = MixFog(finalColor.xyz, i.fogCoord);
                
                return half4(finalColor.xyz, finalAlpha);

                half3 lDir = (normalize(mainLight.direction) + 1) * 0.5;
                half4 dayBrightColor = lerp(lerp(_BrightC1, _BrightC2, saturate(smoothstep(0, 0.7, lDir.z) * 8)), _BrightC3, smoothstep(0, 0.8, smoothstep(0.7, 1, lDir.z) * 0.8)) * step(0.5, lDir.y);
                half4 nightBrightColor = lerp(_BrightC1, lerp(_BrightC4, _BrightC3, smoothstep(0.5, 1, lDir.z)), saturate(smoothstep(0, 0.5, lDir.z) * 9)) * step(-0.5, -lDir.y);
                half4 dayShadowColor = lerp(lerp(_ShadowC1, _ShadowC2, saturate(smoothstep(0, 0.7, lDir.z) * 8)), _ShadowC3, smoothstep(0, 0.8, smoothstep(0.7, 1, lDir.z) * 0.8)) * step(0.5, lDir.y);
                half4 nightShadowColor = lerp(_ShadowC1, lerp(_ShadowC4, _ShadowC3, smoothstep(0.5, 1, lDir.z)), saturate(smoothstep(0, 0.5, lDir.z) * 9)) * step(-0.5, -lDir.y);
                half bloomNL = (dot(lightDir, normalWS) + 1) * 0.5;
                half cloudBloom = smoothstep(_BloomRadius, 1, bloomNL) * _BloomIntensity;
                half3 albedo = lerp(dayShadowColor + nightShadowColor, dayBrightColor + nightBrightColor, cloudTex.r * _AlbedoControl) + cloudBloom;
                half3 b = lerp(nightShadowColor, nightBrightColor, cloudTex.r * _AlbedoControl);
                half NdotL = (dot(lightDir, normalWS) + 1) / 2;
                half rimArea = smoothstep(_RimRadius, 1, NdotL);
                half rimDayCtrl = lerp(lerp(lerp(0, 0.5, smoothstep(0, 0.1, lDir.z)), 0.5, smoothstep(0.1, 0.9, lDir.z)), 0, smoothstep(0.9, 1, lDir.z)) * step(0.5, lDir.y);
                half rimNightCtrl = lerp(0, lerp(0, lerp(0.4, lerp(0, 0, smoothstep(0.9, 1, lDir.z)), smoothstep(0.5, 0.9, lDir.z)), smoothstep(0.1, 0.5, lDir.z)), smoothstep(0, 0.1, lDir.z)) * step(-0.5, -lDir.y);
                half3 rimColor = rimArea * cloudTex.g * (rimDayCtrl + rimNightCtrl + dayBrightColor + nightBrightColor);
                
                return half4(albedo + rimColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}
