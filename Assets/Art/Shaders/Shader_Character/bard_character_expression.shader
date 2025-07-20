shader "bard/role/expression"
{
    Properties
    {
        _MainTex("MainTex",2D) = "White"{}
        _UseDissolve("Use Dissolve",float) = 0
        [HideInInspector]_ToonLightStrength("Light Strength",float) = 1
        [HideInInspector]_LightColor("Light Color",Color) = (1,1,1,1)

        [HideInInspector] [Toggle(ROLE_POSW_DISSOLVE)]_UseRolePosWDissolve("使用角色世界坐标溶解",int) = 0
        [HideInInspector] _PosWDissolveScale("PosW Dissolve Scale",Range(0,2)) = 0.5
        [HideInInspector] [Toggle]_Top2Bottom("Top2Bottom",float) = 0

        [HideInInspector] _UseTransparentMask("Use Transparency Clip",float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"
        }

        Stencil
        {
            Ref 2
            Comp Always
            Pass Replace
        }

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma multi_compile _ ROLE_POSW_DISSOLVE
            #pragma vertex      VS
            #pragma fragment    PS

            struct VertexIn
            {
                float4 PosL : POSITION;
                float2 TexC : TEXCOORD0;
            };

            struct VertexOut
            {
                float4 PosH : SV_POSITION;
                float2 TexC : TEXCOORD0;
                float3 PosW : TEXCOORD1;
                float4 PosS : TEXCOORD2;
            };

            uniform TEXTURE2D(_MainTex);
            uniform SAMPLER(sampler_MainTex);

            uniform TEXTURE2D(_SenceShadowMaskTexture);
            uniform SAMPLER(sampler_SenceShadowMaskTexture);
            
            uniform half _UseWhiteBalance;
            uniform half3 _kRGB;
            uniform half _Transparency;
            uniform half _DirectionLightStrength;
            uniform float4 _WorldPos;

            #if defined WORLD_SPACE_SAMPLE_SHADOW
            // 场景阴影碰撞检测
            uniform     half    _UseSenceShadow;
            uniform     half2   _SenceShadowOffset;
            uniform     half    _SenceShadowScale;
            #endif
            CBUFFER_START(UnityPerMaterial)
            uniform half _UseDissolve;
            uniform half _PosWDissolveScale;
            uniform half _Top2Bottom;
            uniform half _UseTransparentMask;

            uniform half _ToonLightStrength;
            uniform half4 _LightColor;
            CBUFFER_END

            VertexOut VS(VertexIn vin)
            {
                VertexOut vout;
                vout.PosH = TransformObjectToHClip(vin.PosL);
                vout.TexC = vin.TexC;
                vout.PosW = TransformObjectToWorld(vin.PosL);
                vout.PosS = ComputeScreenPos(vout.PosH);
                return vout;
            }

            inline void CHARACTER_MASK_TRANSPARENT_SET(half UseTransparentMask, half Transparency, float4 PosSS)
            {
                UNITY_BRANCH
                if (UseTransparentMask < 0.5)
                    return;

                PosSS.xy = PosSS.xy / PosSS.w;
                PosSS.xy *= _ScreenParams.xy;

                //阈值矩阵
                float4x4 thresholdMatrix = {
                    1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
                    13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
                    4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
                    16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
                };

                //单位矩阵
                float4x4 _RowAccess = {
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1
                };
                #if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
			    clip(Transparency - thresholdMatrix[fmod(PosSS.x, 4)] * _RowAccess[fmod(PosSS.y, 4)]);
                #else
                clip(Transparency - thresholdMatrix[fmod(PosSS.x, 4.00055)] * _RowAccess[fmod(PosSS.y, 4.00055)]);
                #endif
            }

            inline void SetCharacterWhiteBalance(half UseWhiteBalance, half3 kRGB, inout half4 color)
            {
                UNITY_BRANCH
                if (UseWhiteBalance < 0.5)
                    return;

                color.r *= kRGB.r;
                color.g *= kRGB.g;
                color.b *= kRGB.b;
            }

            inline void GET_CHARACTER_LIGHT_COLOR(half4 LightColor, half ToonLightStrength, inout half4 color)
            {
                half GlobalLightStrength = step(0.001, _DirectionLightStrength) * _DirectionLightStrength + (1 - step(
                    0.001, _DirectionLightStrength));
                color.rgb = color.rgb * LightColor.rgb * ToonLightStrength * GlobalLightStrength;
            }

            inline half GetCharacterSenceShadow()
            {
                #if defined WORLD_SPACE_SAMPLE_SHADOW
                float2 set_uv = _WorldPos.xz - _SenceShadowOffset;
                set_uv = set_uv / _SenceShadowScale;
                half shadow = SAMPLE_TEXTURE2D(_SenceShadowMaskTexture, sampler_SenceShadowMaskTexture, set_uv).r;
                shadow = step(0.9, shadow.r);
                return shadow;
                #else
                return 1;
                #endif
            }

            inline void SetCharacterSenceShadow(half shadow, inout half4 color, half3 shadowColor)
            {
                #if defined WORLD_SPACE_SAMPLE_SHADOW
                color.rgb                                       = lerp(shadowColor,color,shadow.r).rgb;
                #endif
            }

            half4 PS(VertexOut pin) : SV_Target
            {
                CHARACTER_MASK_TRANSPARENT_SET(_UseTransparentMask, _Transparency, pin.PosS);

                #if defined ROLE_POSW_DISSOLVE
	            half show = step(_PosWDissolveScale,pin.PosW.y);
	            show = (1 - show) * _Top2Bottom + (1 - _Top2Bottom) * show;
	            clip(show - 0.5);
                #endif

                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pin.TexC);

                GET_CHARACTER_LIGHT_COLOR(_LightColor, _ToonLightStrength, color);

                SetCharacterWhiteBalance(_UseWhiteBalance, _kRGB, color);

                //---------- 进入阴影 ------
                half shadow = GetCharacterSenceShadow();
                SetCharacterSenceShadow(shadow, color, half3(0.5,0.5,0.5) * color);

                UNITY_BRANCH
                if (_UseDissolve > 0.5)
                {
                    clip(color.a - 2);
                }

                return color;
            }
            ENDHLSL
        }
    }
}