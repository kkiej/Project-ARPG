/***********************************************************************************************
 ***                     B A R D S H A D E R  ---  B A R D  S T U D I O S                    ***
 ***********************************************************************************************
 *                                                                                             *
 *                                      Project Name : BARD                                    *
 *                                                                                             *
 *                               File Name : bard_shader_Input.hlsl                            *
 *                                                                                             *
 *                                    Programmer : Zhu Han                                     *
 *                                                                                             *
 *                                      Date : 2021/1/20                                       *
 *                                                                                             *
 * - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

//---------- 公共参数 ----------

//灯光
uniform     half3                                               _DirectionLightPos;
uniform     half                                                _DirectionLightStrength;
uniform     half4                                               _AddLightColor;
uniform     half                                                _UseAddColor;

//相机FOV
uniform     half                                                _CameraFOV;

//RenderScale
uniform     half                                                _RenderScale;

uniform     half                                                _isWhite;

//黑白闪
uniform     half                                                _FlickerFresnelRange;
uniform     half                                                _FlickerShadowRange;

//表情
uniform     half                                                uv_x;
uniform     half                                                uv_y;

//白平衡
uniform     half                                                _UseWhiteBalance;
uniform     half3                                               _kRGB;

//头发阴影
uniform     half3                                               worldLightVector;
uniform     float4x4                                            SHADOW_MAP_VP;
uniform     float4x4                                            BARD_SHADOW_MAP_VP;

//眼睛视差
uniform     half                                                _Height;
uniform     half                                                _ParallaxScale;

//脸部冰冻
uniform     half                                                _UseFaceIceQuad;
uniform     half                                                _IceQuadDensity;
uniform     half                                                _IceQuadDissolveScale;

/// <summary>
/// TEXTURE
/// </summary>
uniform TEXTURE2D(_MainTex);                                    uniform SAMPLER(sampler_MainTex);
uniform TEXTURE2D(_CelTex);                                     uniform SAMPLER(sampler_CelTex);
uniform TEXTURE2D(_BlendTex);                                   uniform SAMPLER(sampler_BlendTex);
uniform TEXTURE2D(_SDFTexture);                                 uniform SAMPLER(sampler_SDFTexture);

uniform TEXTURE2D(_EmissionTex);                                uniform SAMPLER(sampler_EmissionTex);

uniform sampler2D _NoiseTex;                                    
uniform TEXTURE2D(_IceTexture);                                 uniform SAMPLER(sampler_IceTexture);

#if defined(_TRANSPARENT)
uniform TEXTURE2D(_TransparentTex);                             uniform SAMPLER(sampler_TransparentTex);
uniform TEXTURE2D(_CameraOpaqueTexture);                        uniform SAMPLER(sampler_CameraOpaqueTexture);
#endif

uniform TEXTURE2D(_SenceShadowMaskTexture);                     uniform SAMPLER(sampler_SenceShadowMaskTexture);
uniform TEXTURE2D(_NormalMap);                                  uniform SAMPLER(sampler_NormalMap);

#if defined(_USE_MAIN_TEXTURE_LERP)
uniform TEXTURE2D(_MainTex02);                                  uniform SAMPLER(sampler_MainTex02);
uniform TEXTURE2D(_CelTex02);                                   uniform SAMPLER(sampler_CelTex02);
uniform TEXTURE2D(_BlendTex02);                                 uniform SAMPLER(sampler_BlendTex02);
#endif

//丝袜材质
#if defined USE_STOCKING
uniform TEXTURE2D(_StockingTex);                                uniform SAMPLER(sampler_StockingTex);
#endif

uniform     float4                                              _MainTex_ST;
uniform     float4                                              _CelTex_ST;
uniform     float4                                              _BlendTex_ST;

#if defined WORLD_SPACE_SAMPLE_SHADOW
// 场景阴影碰撞检测
uniform     half                                                _UseSenceShadow;
uniform     half2                                               _SenceShadowOffset;
uniform     half                                                _SenceShadowScale;
#endif

CBUFFER_START(UnityPerMaterial)

uniform     float4                                              _NoiseTex_ST;

//丝袜材质
uniform     float4                                              _StockingTex_ST;
uniform     half                                                _First_Fresnel_Shadow_Step;
uniform     half                                                _First_Fresnel_Shadow_Feather;
uniform     half4                                               _First_Fresnel_Shadow_Color;

uniform     half                                                _Second_Fresnel_Shadow_Step;
uniform     half                                                _Second_Fresnel_Shadow_Feather;
uniform     half4                                               _Second_Fresnel_Shadow_Color;

uniform     half                                                _Fresnel_Light_Step;
uniform     half                                                _Fresnel_Light_Feather;
uniform     half4                                               _Fresnel_Light_Color;

//平面阴影
uniform     half4                                               _ShadowFadeParams;
uniform     half                                                _ShadowInvLen;
uniform     half                                                _ShadowOffset;
uniform     half4                                               _PlaneShadowColor;
uniform     float4                                              _WorldPos;
uniform     half4                                               _ShadowPlane;

//Texture Settings 
uniform     half4                                               _MainColor;

//Light Settings  
uniform     half4                                               _LightColor;
uniform     half                                                _UseAddLight;
uniform     half                                                _addShadowRangeStep;
uniform     half                                                _addShadowFeather;
uniform     half                                                _ToonLightStrength;

//OutLine Setting
uniform     half                                                _UseSmoothNormal;
uniform     half4                                               _OutlineColor;
uniform     half4                                               _OutlineSkinColor;
uniform     half                                                _OutlineWidth;
uniform     float                                               _Offset_Z;

//Shadow Setting
uniform     half                                                _ShadowRangeStep;
uniform     half                                                _ShadowFeather;
uniform     half                                                _defaultShadowStrength;

//RimLight Setting
uniform     half4                                               _RimColor;
uniform     half                                                _RimRangeStep;
uniform     half                                                _RimFeather;
uniform     half                                                _SSRimScale;
uniform     half                                                _UseSSRim;

//Emission Setting
uniform     half                                                _EmissionStrength;

//Specular Setting
uniform     half                                                _SpecFalloff;
uniform     half4                                               _SpecColor;

uniform     half                                                _MetalScale;
uniform     half                                                _SmoothScale;
uniform     half                                                _Shiness;
uniform     half                                                _CheckLine;
uniform     half                                                _SpecStep;

//HUE Setting
uniform     half                                                _UseHSV;
uniform     half                                                _Hue;
uniform     half                                                _Saturation;
uniform     half                                                _Value;

//流光描边
uniform     half                                                _StencilOutlineWidth;
uniform     half4                                               _StencilOutLineColor;

//流光
uniform     half                                               _UseAdditionFresnel;
uniform     half4                                              _FresnelColor;
uniform     half                                               _FresnelRange;
uniform     half                                               _FresnelFeather;
uniform     half                                               _FresnelFlowRange;

//本地坐标溶解
uniform     half                                                _PosWDissolveScale;
uniform     half                                                _PosWDissolveWidth;
uniform     half4                                               _PosWDissolveColor;
uniform     half                                                _PosWNoiseScale;
uniform     half                                                _Top2Bottom;

//死亡溶解
uniform     half                                                _UseDissolve;
uniform     half                                                _DissolveScale;
uniform     half                                                _EdgeWidth;
uniform     half4                                               _EdgeColor;

uniform     half                                                _UseDissolveCut;
uniform     half4                                               _DissolveColor;
uniform     half                                                _DissolveCut;
uniform     half                                                _DissolveWidth;

//冰冻效果
uniform     half                                                _UseIce;
uniform     half4                                               _IceVFXColor;
uniform     half4                                               _IceRimColor;
uniform     half4                                               _IceSpecColor;
uniform     half                                                _IceSpecPower;
uniform     half                                                _IceNormalScale;
uniform     half                                                _Ice_Fresnel_Feather;
uniform     half                                                _Ice_Fresnel_Step;

//相机灯光
uniform     half                                                _UseCameraLight;
uniform     half3                                               _ToonLightDirection;

//溶解UV
uniform     half2                                               _ScreenUV;

//点阵化透明
uniform     half                                                _Transparency;
uniform     half                                                _UseTransparentMask;

//脸部阴影
uniform     half4                                               _ShadowColor;
uniform     half4                                               _EyeShadowColor;

//头发阴影
uniform     half                                                _HairShadowDistace;

//SIMPLE LIT
uniform     half                                                _UseAlphaClip;
uniform     half4                                               _RimLightColor;
uniform     half4                                               _EmissionColor;

//Metal
uniform     half4                                               _FlowColor;
uniform     half                                                _AddObjectY;
uniform     half                                                _FirstShadowSet;
uniform     half                                                _FirstShadowFeather;

//Unity Fog
uniform     half                                                _UseUnityFog;

//脸部朝向
uniform     half3                                               _FaceForwardVector;

//主纹理插值变化
uniform     half4                                               _MainTexNoiseTilling;
uniform     half                                                _Dissolve_power1;
uniform     half                                                _Dissolve_power2;
uniform     half4                                               _EdgeColor2;

//暗部亮度控制
uniform     half                                                _UseDarkLuminance;
uniform     half                                                _DarkLuminance;

CBUFFER_END