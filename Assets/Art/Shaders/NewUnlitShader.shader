Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            TEXTURE2D(_CharacterShadowTexture);
            SAMPLER(sampler_CharacterShadowTexture);
            float4x4 _CharacterShadowMatrix[10];
            float4 _CharacterUVClamp[10];
            
            float SampleCharacterShadow(float3 worldPos, int index)
            {
                if(index >= 10) return 1.0;
                
                float4 shadowCoord = mul(_CharacterShadowMatrix[index], float4(worldPos, 1.0));
                shadowCoord.xyz /= shadowCoord.w;
                
                float4 uvClamp = _CharacterUVClamp[index];
                float2 uv = shadowCoord.xy;
                if(uv.x < uvClamp.x || uv.x > uvClamp.y || 
                   uv.y < uvClamp.z || uv.y > uvClamp.w)
                    return 1.0;
            
                return SAMPLE_TEXTURE2D(_CharacterShadowTexture, sampler_CharacterShadowTexture, uv).x;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.positionWS = TransformObjectToWorld(v.vertex.xyz);
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float4 debugCoord = mul(_CharacterShadowMatrix[0], float4(i.positionWS, 1.0));
                return float4(debugCoord.xyz / debugCoord.w, 1.0);
                half3 col = half3(0.5, 0.5, 0.5);
                half shadow = SampleCharacterShadow(i.positionWS, 0);
                col *= shadow;
                return half4(shadow.xxx, 1);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite [_ZWrite]
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            //#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
}
