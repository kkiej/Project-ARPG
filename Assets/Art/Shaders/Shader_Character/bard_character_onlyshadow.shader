Shader "bard/role/only shadow"
{
    Properties
    {
        _Color("Color",Color) = (1,1,1,1)
    }

    SubShader
    {

        Tags{"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        ENDHLSL

        Pass
        {
            Tags{"LightMode" = "UniversalForward"}
            Blend SrcAlpha  OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma vertex VS
            #pragma fragment PS

            struct VertexIn
            {
                float4 PosL : POSITION;
            };

            struct VertexOut
            {
                float4 PosH : SV_POSITION;
                float4 shadowCoord : TEXCOORD0;
                float3 PosW : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
            uniform half4 _Color;
            CBUFFER_END

            VertexOut VS(VertexIn vin)
            {
                VertexOut vout;
                vout.PosH = TransformObjectToHClip(vin.PosL);
                vout.PosW = TransformObjectToWorld(vin.PosL);
                vout.shadowCoord = TransformWorldToShadowCoord(vout.PosW);
                return vout;
            }

            half4 PS(VertexOut pin) : SV_Target
            {
                float4 SHADOW_COORDS = TransformWorldToShadowCoord(pin.PosW);
                half shadow = 1 - saturate(MainLightRealtimeShadow(SHADOW_COORDS));
                return half4(_Color.rgb, _Color.a * shadow);
            }
            ENDHLSL
        }
    }
}