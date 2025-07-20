/***********************************************************************************************
 ***                     B A R D S H A D E R  ---  B A R D  S T U D I O S                    ***
 ***********************************************************************************************
 *                                                                                             *
 *                                      Project Name : BARD                                    *
 *                                                                                             *
 *                               File Name : bard_function.hlsl                                *
 *                                                                                             *
 *                                    Programmer : Zhu Han                                     *
 *                                                                                             *
 *                                      Date : 2021/1/20                                       *
 *                                                                                             *
 * - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
//  为了支持SRP Batching  请勿在方法里调用全局变量  应当将全局变量由外部传入

#include "./bard_shader_lib.hlsl"

#define RoleLightPos        half3(0.62,2.9,1.8)
#define RoleLightAttention  half4(0.12,2.8,16,-14)
#define RoleSpotDir         half3(0.25,0.69,0.68)

/// <summary>
/// 获取角色材质深度
/// </summary>
inline void Get_CHARACTER_DEPTH(half2 DepthIn, inout half DepthOut)
{
    half depth                                  = DepthIn.x / DepthIn.y;
    #if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
    depth                                       = depth * 0.5 + 0.5;
    #elif defined (UNITY_REVERSED_Z)
    depth                                       = 1 - depth;
    #endif
    DepthOut                                    = LinearEyeDepth(depth,_ZBufferParams);
}

/// <summary>
/// 获取深度纹理
/// </summary>
inline half GetDepth(half2 targetUV)
{
    half rawDepth                              = SampleSceneDepth(targetUV);
    half depth                                 = LinearEyeDepth(rawDepth, _ZBufferParams);
    //#if defined(UNITY_REVERSED_Z)
    //depth = 1.0f - depth;
    //# endif
    return depth;
}

/// <summary>
/// 获取NDC空间法线 来自切线或者法线
/// </summary>
inline half3 GET_NORMAL_IN_NDC(float UseSmoothNormal,float3 tangent, float3 normal, float4 pos)
{
	half3 aimNormal 							= UseSmoothNormal * tangent + normal - UseSmoothNormal * normal;
    half3 viewNormal                            = mul((float3x3)UNITY_MATRIX_IT_MV, aimNormal);
    half3 ndcNormal                             = normalize(TransformViewToProjection(viewNormal)) * pos.w;//将法线变换到NDC空间
    return ndcNormal;
}

/// <summary>
/// 角色描边远近控制
/// </summary>
inline void CHARACTER_OUTLINE_FAR_NEAR_SET(half OutlineWidth,half CameraFOV,float4 vertex, float3 ndcNormal, half2 uv, half4 color, inout float4 pos)
{
    half4 nearUpperRight                    = mul(unity_CameraInvProjection, float4(1, 1, UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));
    half aspect                             = abs(nearUpperRight.y / nearUpperRight.x);//求得屏幕宽高比
    ndcNormal.xy                            *= aspect;
    half dis                                = distance(_WorldSpaceCameraPos.xz,_WorldPos.xz);
    dis                                     = clamp(dis,0,10);
    half distance_weight                    = 0.2 * pow(dis,-0.247);
    half fov_weight                         = 0.245 * pow(CameraFOV, -0.227);
    half outlineScaleBalance                = distance_weight * fov_weight * 1.8;
    pos.xy                                  += OutlineWidth * ndcNormal.xy * color.x * outlineScaleBalance;
}

/// <summary>
/// 角色描边远近控制(ShineOutLine)
/// </summary>
inline void CHARACTER_STENCIL_OUTLINE_FAR_NEAR_SET(half StencilOutlineWidth,half CameraFOV,float4 vertex, float3 ndcNormal,float4 color, inout float4 pos)
{
    half3 posWS 					            = mul(unity_ObjectToWorld,vertex);
    half4 nearUpperRight                        = mul(unity_CameraInvProjection, float4(1, 1, UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));
    half aspect                                 = abs(nearUpperRight.y / nearUpperRight.x);//求得屏幕宽高比
    ndcNormal.xy                                 *= aspect;
    half Dis                                    = distance(_WorldSpaceCameraPos.xyz,posWS);
    half outlineScaleBalance                    = 0.1 * clamp(lerp(1,0.2,clamp((Dis / 7.3),0,1) ),0,0.4) * clamp((45 / CameraFOV), 0.5,2);
    pos.xy                                      += StencilOutlineWidth * ndcNormal.xy * color.x * outlineScaleBalance; 
}

/// <summary>
/// 角色描边颜色控制
/// </summary>
inline half3 CHARACTER_OUTLINE_COLOR_SET(half isBody, half skinmask)
{
    half3 color                                 = (skinmask * _OutlineColor.rgb + (1 - skinmask) * _OutlineSkinColor.rgb) * isBody + _OutlineColor * (1 - isBody);
    return                                      color;
}

/// <summary>
/// 角色描边偏移
/// </summary>
inline void VertexPosOffsetZ(inout float PosZ)
{
    float4 _ClipCameraPos                       = mul(UNITY_MATRIX_VP, float4(_WorldSpaceCameraPos.xyz, 1));
    //v.2.0.7
    #if defined(UNITY_REVERSED_Z)
    //v.2.0.4.2 (DX)
    _Offset_Z                                   = _Offset_Z * -0.01;
    #else
    //OpenGL
    _Offset_Z                                   = _Offset_Z * 0.01;
    #endif
	
    PosZ                                        = PosZ + _Offset_Z * _ClipCameraPos.z;
}

/// <summary>
/// 根据UV获取SimpleX噪声并Step
/// </summary>
inline half SIMPLEX_NOISE_SETP(float2 uv)
{
    half2 noise                                 = half2(uv.x * 30 , uv.y * 30);
    return SimpleX_Noise(noise);
}

/// <summary>
/// 获取角色半兰伯特光照
/// </summary>
void GetCharacterHalfLambert(half3 ToonLightDirection, half UseCameraLight,half3 DirectionLightPos,half3 normalWS, inout half halflambert)
{
    halflambert                                = max(0.0, 0.5 * dot(normalWS, normalize(ToonLightDirection.xyz * UseCameraLight + (1 - UseCameraLight) * DirectionLightPos)) + 0.5);
}

/// <summary>
/// 武器单向流光
/// </summary>
inline void WEAPON_DISSOLVE_SET(half UseDissolveCut,half DissolveCut,half DissolveWidth, half4 DissolveColor,half4 vColor, inout half4 color)
{
    UNITY_BRANCH
    if(UseDissolveCut < 0.5)
        return;
    
   half clipLine                               = saturate(vColor.g);
   half highLightArea                          = 1 - step(clipLine,DissolveCut);
    highLightArea                              = highLightArea * step(clipLine,DissolveCut + DissolveWidth);
    color                                     += highLightArea * DissolveColor;
    
   if (clipLine < DissolveCut)
   {
       discard;
   }
}

/// <summary>
/// 模型空间-->裁剪空间
/// </summary>
inline void SetObjectToClipPos(inout float4 posCS, float4 posOS)
{
    posCS                                       = TransformObjectToHClip(posOS);
}

/// <summary>
/// 获取法线向量、光照向量、视口向量
/// </summary>
inline void GetCharacterNormalizeDir(half3 DirectionLightPos, half3 ToonLightDirection, half UseCameraLight,half3 normal, half3 posWS, inout half3 normalDir, inout half3 lightDir, inout half3 L_World, inout half3 viewDir)
{
    normalDir                                   = normalize(normal);
    L_World                                     = normalize(DirectionLightPos);
    lightDir                                    = normalize(ToonLightDirection.xyz * UseCameraLight + (1 - UseCameraLight) * DirectionLightPos);
    viewDir                                     = normalize(_WorldSpaceCameraPos.xyz - posWS.xyz);
}

/// <summary>
/// 黑白闪特效
/// </summary>
half4 FLICKER_EFFECT_SET(half3 DirectionLightPos,half FlickerShadowRange, half FlickerFresnelRange,half3 normalDir, half3 V)
{
        half Lambert                            = saturate(dot(normalize(normalDir),normalize(DirectionLightPos)));
        half blackWhtie							= step(Lambert,FlickerShadowRange);

        UNITY_BRANCH
        if(_isWhite > 0.5)
        {
            blackWhtie							= step(FlickerShadowRange,Lambert) ;
        }
        half F									= saturate(dot(normalize(normalDir),normalize(V)));
        F										= step(pow(1.0 - max(0, F), 1),FlickerFresnelRange);
        half final								= F * blackWhtie;
        final									= saturate(final + 0.1);
        
        return                                  final.xxxx;
}

/// <summary>
/// 角色基础参数设置
/// </summary>
///参数分别是：uv    固有色   Cel贴图颜色   Blend贴图A通道的灰度图
inline void CharacterBaseParamesSetBody(half2 uv, inout half4 color,inout half4 mainColor, inout half4 celColor, inout half stepMask, inout half Metalic, inout half Stocking)
{
    mainColor                                   = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
    
    celColor                                    = SAMPLE_TEXTURE2D(_CelTex, sampler_CelTex, uv);
    
    stepMask                                    = SAMPLE_TEXTURE2D(_CelTex, sampler_CelTex, uv).a;
    stepMask                                    = clamp( pow( stepMask, 0.7), 0, 1);                      //这里做一下转色彩空间 不然灰度值不对
    
    color                                       = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
    
    half blendA                                 = SAMPLE_TEXTURE2D(_BlendTex,sampler_BlendTex,uv).a;
    
    blendA                                      = clamp( pow( blendA, 0.7), 0, 1);

    Stocking                                    = step(blendA,0.795) * (1 - step(blendA,0.70));
    
    Metalic                                     = (1 - step(blendA,0.4)) * step(blendA,0.6);

    clip(blendA - 0.3);
}

/// <summary>
/// 角色基础参数设置
/// </summary>
///参数分别是：uv    固有色   Cel贴图颜色   Blend贴图A通道的灰度图
inline void CharacterBaseParamesSetFace(half2 uv, inout half4 color,inout half4 mainColor, inout half4 celColor)
{
    mainColor                                   = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
    celColor                                    = SAMPLE_TEXTURE2D(_CelTex, sampler_CelTex, uv);                     //这里做一下转色彩空间 不然灰度值不对
    color                                       = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);                                                                      
}

/// <summary>
/// 角色阴影参数设置
/// </summary>
inline void CharacterShadowParamesSet(half UseCameraLight, half ShadowRangeStep, half ShadowFeather,half Cel_G, half NoL, half NoL_World,inout half ShadowRange, inout half Set_ShadowMask,inout half RampShadowArea, inout half DefaultShadowArea,inout half3 DefaultShadowColor,inout half StayLightArea ,inout half3 RampShadowColor,inout half StepCount)
{
    NoL_World                                   = saturate(NoL_World * 0.5 + 0.5);
    NoL                                         = (1 - UseCameraLight) * NoL + UseCameraLight * saturate(NoL);
    ShadowRange                                 = 1 - Cel_G * (1 - ShadowRangeStep);                    //获取ramp阴影范围
    Set_ShadowMask                              = saturate(((NoL - (ShadowRange-ShadowFeather)) * - 1.0 ) / (ShadowRange - (ShadowRange-ShadowFeather)) + 1 );            //我也不知道为什么这么算.....因为这样写好看
    RampShadowArea                              = step(0.1, Cel_G);                                                                                                         //取得ramp阴影区域
    DefaultShadowArea                           = 1 - step(0.1, Cel_G);                                                                                                     //取得死阴影区域
    DefaultShadowColor                          = half3(1,1,1);                                                                                                             //死阴影颜色
    StayLightArea                               = step(0.99,Cel_G);                                                                                                         //取得长亮区域
    RampShadowColor                             = half3(1,1,1);                                                                                                             //ramp阴影颜色
    StepCount                                   = 0.06;                                                                                                                     //写死的ramp区域间隔值
}

/// <summary>
/// 设置死阴影颜色
/// </summary>
inline void DEFAULT_SHADOW_COLOR_SET(half defaultShadowStrength, half Set_ShadowMask, inout half3 defaultShadowColor)
{
    defaultShadowColor                          = Set_ShadowMask * lerp(defaultShadowColor,defaultShadowColor*defaultShadowColor,defaultShadowStrength) + (1 - Set_ShadowMask) * defaultShadowColor;
}

/// <summary>
/// 设置阴影
/// </summary>
inline void CHARACTER_SHADOW_SET(half3 defaultShadowColor, half3 rampShadowColor, half DefaultShadowArea,inout half4 outColor)
{
    half3 finalShadowColor                      = defaultShadowColor.rgb * DefaultShadowArea + rampShadowColor.rgb * (1 - DefaultShadowArea);
	outColor.rgb                                *= finalShadowColor;
}

/// <summary>
/// CharacterAcesApprox
/// </summary>
inline half3 CharacterAcesApprox(inout half3 v)
{
    v                                           *= 0.6f;
    half a                                      = 2.51f;
    half b                                      = 0.03f;
    half c                                      = 2.43f;
    half d                                      = 0.59f;
    half e                                      = 0.14f;
    return                                      (v*(a*v+b))/(v*(c*v+d)+e);
}

/// <summary>
/// 设置长亮区域
/// </summary>
inline void STAY_LIGHT_SET(inout half4 color, half stayLightArea,half4 mainColor)
{
    color.rgb                                   = color.rgb * (1 - stayLightArea) + stayLightArea * mainColor.rgb;
}

/// <summary>
/// 获取边缘光参数
/// </summary>
inline void GET_RIM_LIGHT_FACTORY(half RimRangeStep, half RimFeather,half NoV, float3 PosW, inout half Set_RimLightMask)
{
    #if defined(_TRANSPARENT)
    
    #else
    UNITY_BRANCH
    if(_UseSSRim > 0.5)
        return;
    #endif
    
    half Dis                                    = distance(_WorldSpaceCameraPos.xyz, PosW);
    half Balance                                = lerp(1.3,0.85,clamp((Dis / 8),0,1));
    RimRangeStep                                *= Balance;
    half rimFactor                              = pow(1.0 - max(0, NoV), 1);
    Set_RimLightMask                            = saturate(((rimFactor - (RimRangeStep -RimFeather)) * - 1.0 ) / (RimRangeStep - (RimRangeStep - RimFeather)) + 1 );
}

/// <summary>
/// 角色边缘光设置  边缘光遮罩  亮光面更加亮 背光面暗
/// </summary>
inline void RIM_LIGHT_SET(half vertexMask, half Set_ShadowMask, half3 RampRimColor, inout half4 color)
{                                                                                           //边缘光遮罩
    RampRimColor                                = step(0.5, Set_ShadowMask) * RampRimColor * 0.3 + (1 - step(0.5, Set_ShadowMask)) * RampRimColor;                          //亮暗面处理
    color.rgb                                   += (RampRimColor * vertexMask);                   
}

/// <summary>
/// 角色屏幕空间深度边缘光
/// </summary>
inline void SCREEN_SPACE_DEPTH_RIM_SET(half4 PosSS, half4 targetPosSS,inout half Set_RimLightMask)
{
    #if defined(_TRANSPARENT)
    return;
    #endif
    
    UNITY_BRANCH
    if(_UseSSRim < 0.5)
        return;

    half2 ssUV                                  = PosSS.xyz / PosSS.w;
    half depth                                  = GetDepth(ssUV);
    
    half depthTarget                            = GetDepth(targetPosSS.xy);
    depthTarget                                += GetDepth(targetPosSS.zw);
    depthTarget                                /= 2;

    half depthDiff                              = depthTarget - depth;
    depthDiff                                   = 1 - saturate(depthDiff);
    Set_RimLightMask                            = pow(depthDiff,5);
}

/// <summary>
/// 设置角色目标屏幕空间坐标
/// </summary>
inline void SetCharacterTargetPosSS(half SSRimScale, half mask,float3 PosW,inout float4 TargetPosSS)
{
    #if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
    float3 posVS                                = TransformWorldToView(PosW) * float3(1,-1,1);
    #elif defined (UNITY_REVERSED_Z)
    float3 posVS                                = TransformWorldToView(PosW);
    #endif

    half Dis                                    = distance(_WorldSpaceCameraPos.xyz,PosW);
    half Balance                                = lerp(0.3,2,clamp((Dis / 8),0,1));
    half fov_weight                             = 0.0317 * pow(_CameraFOV, 0.326) * 12;
    SSRimScale                                  = Balance * SSRimScale * fov_weight;
    SSRimScale                                 *= mask;
    float3 target1                              = posVS + float3(SSRimScale/100, 0, 0);
    float3 target2                              = posVS - float3(SSRimScale/100, 0, 0);
    TargetPosSS.xy                              = TransformViewToScreen(target1);
    TargetPosSS.zw                              = TransformViewToScreen(target2);
}

/// <summary>
/// 高光 普通区域处理
/// </summary>
inline void SPEC_COMMON_SET(half SpecFalloff, half4 CelColor, half colorBlendMask, half3 NoH, inout half3 Spec_Common)
{
    half specmask                               = CelColor.r;                                                                                                               //高光遮罩
	half specpow                                = 1 - CelColor.b;                                                                                                           //高光强度控制
    Spec_Common                                 = saturate(pow(max(0, NoH), specpow * 18) - specpow);                                                                      //计算BlinnPhong模型高光
    Spec_Common                                 = (Spec_Common + ceil(Spec_Common)) * specmask * SpecFalloff;                                                              //乘上遮罩和自定义强度控制
    Spec_Common                                 = Spec_Common * (1 - colorBlendMask) * _SpecColor;                                                                                           //普通区域遮罩
}

/// <summary>
/// 获取角色球谐
/// </summary>
// inline void CHARACTER_SH_COLOR(half3 N, half3 V, half4 mainColor, inout half3 SH)
// {
//     half mip_roughness                          = 1-_SmoothScale;
//     half3 reflectVec                            = reflect(-V, N);
//     half mip                                    = mip_roughness * UNITY_SPECCUBE_LOD_STEPS;
//     half4 rgbm                                  = SAMPLE_TEXTURECUBE_LOD(_skyBox, sampler_skyBox,reflectVec, mip);
//     SH                                          = DecodeHDREnvironment(rgbm, _skyBox_HDR);
//     SH                                          = SH * mainColor.rgb; 
// }

/// <summary>
/// 高光 金属区域处理
/// </summary>
inline void SPEC_METAL_SET(half Shiness, half SpecStep, half CheckLine, half4 CelColor,half3 RoL, half3 N, half3 H, half3 NoH, half3 V,inout half3 SpecMetal, inout half3 SpecMetalSH)
{
    half Rough                                  = GetRough(Shiness,CelColor.b);                                                                        //高光强度获取作为粗糙度
    half MYPhongApprox_InvincibleDragon         = GetMYPhongApprox_InvincibleDragon(Rough,RoL);                                                         //计算改版GGX高光
    half MYGGX_InvincibleDragon                 = GetMYGGX_InvincibleDragon(N,H,Rough,NoH);
    half SpecModel                              = MYCalcSpecular(0,MYPhongApprox_InvincibleDragon,Rough,MYGGX_InvincibleDragon);
    half Spec_Model_Toon                        = GetSpec_Model_Toon(SpecStep,SpecModel,CheckLine,CelColor.b);                                        //金属度遮罩                                                                                //获取天空球颜色
    half3 sh                                    = half4(1,1,1,1);
    
    // CHARACTER_SH_COLOR(N, V, float4(1,1,1,1),sh);

    half specStep                               = step(Spec_Model_Toon,0);
    SpecMetalSH                                 = sh * SpecModel;//saturate((Spec_Model_Toon + specStep * 0.227451) * 4.39655);                          //输出颜色
    SpecMetal                                   = Spec_Model_Toon + Spec_Model_Toon * 0.8;                                             //输出颜色
}

/// <summary>
/// 高光头发
/// </summary>
inline void SPEC_HAIR_SET(half SpecFalloff,half4 CelColor, half3 NoH, half3 Set_ShadowMask, inout half4 color)
{
    half specmask                               = CelColor.r;                                                                                                               //高光遮罩
	half specpow                                = 1 - CelColor.b;                                                                                                           //高光强度控制
    half Spec_Common                            = saturate(pow(max(0, NoH), specpow * 18) - specpow);                                                                      //计算BlinnPhong模型高光
    Spec_Common                                 = (Spec_Common + ceil(Spec_Common)) * specmask * SpecFalloff;                                                              //乘上遮罩和自定义强度控制
    color.rgb                                   += Spec_Common * (1 - Set_ShadowMask) * _SpecColor;
}

/// <summary>
/// 获取高光  普通区域和金属区域
/// </summary>
inline void GET_CHARACTER_SPEC(half MetalScale, half4 CelColor,  half3 SpecCommon, half3 SpecMetal, half3 SpecMetalSH, half3 mainColor, half3 Set_ShadowMask, inout half4 color)
{
    half metalMask                              = CelColor.a;
    SpecCommon                                  *= (mainColor * 0.8 + 0.2);
    half3 baseColor                             = (SpecCommon * (1 - Set_ShadowMask) + color.rgb) * (1 - metalMask);
    half3 metalColor                            = (SpecMetal * mainColor * (1 - Set_ShadowMask) + color.rgb) * metalMask;

    metalColor                                  = lerp(metalColor,SpecMetalSH,MetalScale) * metalMask;;
    color.rgb                                   = baseColor + metalColor;
}

/// <summary>
/// 获取角色自发光
/// </summary>
inline void GET_CHARACTER_EMISSION(half EmissionStrength,half4 mainColor, half2 uv, inout half4 color)
{
    half EmissionMask                           = SAMPLE_TEXTURE2D(_EmissionTex,sampler_EmissionTex,uv).r;
    half3 Emission                              = EmissionMask * mainColor * _EmissionColor;
    color.rgb                                   = color.rgb + Emission;
}

/// <summary>
/// 获取角色脚本光照
/// </summary>
inline void GET_CHARACTER_LIGHT_COLOR(half UseAddColor, half4 LightColor, half ToonLightStrength, half4 AddLightColor,inout half4 color)
{
    half GlobalLightStrength                    = step(0.001, _DirectionLightStrength) * _DirectionLightStrength + (1 - step(0.001, _DirectionLightStrength));
    half3 GetColor                              = color.rgb * LightColor.rgb * ToonLightStrength * GlobalLightStrength;
    color.rgb                                   = UseAddColor * AddLightColor + GetColor;
}

/// <summary>
/// 获取角色Unity投影
/// </summary>
inline void GET_CHARACTER_UNITY_SHADOW(half shadow,half4 mainColor,half4 defaultShadowColor,inout half4 color)
{
    shadow                                      = step(shadow,0.1);
    color.rgb                                   = shadow * (mainColor.rgb * defaultShadowColor.rgb) + (1 - shadow) * mainColor.rgb;
}

/// <summary>
/// ASE_ComputeGrabScreenPos
/// </summary>
inline float4 ASE_ComputeGrabScreenPos( float4 pos )
{
	#if UNITY_UV_STARTS_AT_TOP
	half scale                                  = -1.0;
	#else
	half scale                                  = 1.0;
	#endif
	float4 o                                    = pos;
	o.y                                         = pos.w * 0.5f;
	o.y                                         = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
	return o;
}

/// <summary>
/// 角色点阵化处理
/// </summary>
inline void CHARACTER_MASK_TRANSPARENT_SET(half UseTransparentMask,half Transparency,float4 PosSS)
{
    UNITY_BRANCH
    if(UseTransparentMask < 0.5)
        return;
    
    PosSS.xy                                    = PosSS.xy / PosSS.w;
    PosSS.xy                                    = PosSS.xy * (_ScreenParams.xy) * _RenderScale;

    //阈值矩阵
    float4x4 thresholdMatrix ={  
        1.0 / 17.0,			9.0 / 17.0,			3.0 / 17.0,			11.0 / 17.0,
        13.0 / 17.0,		5.0 / 17.0,			15.0 / 17.0,		7.0 / 17.0,
        4.0 / 17.0,			12.0 / 17.0,		2.0 / 17.0,			10.0 / 17.0,
        16.0 / 17.0,		8.0 / 17.0,			14.0 / 17.0,		6.0 / 17.0
    };

    //单位矩阵
    float4x4 _RowAccess = {
        1,  0,  0,  0, 
        0,  1,  0,  0,
        0,  0,  1,  0, 
        0,  0,  0,  1 
    };
    #if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
    clip(Transparency - thresholdMatrix[fmod(PosSS.x, 4)] * _RowAccess[fmod(PosSS.y, 4)]);
    #else
    clip(Transparency - thresholdMatrix[fmod(PosSS.x, 4.00055)] * _RowAccess[fmod(PosSS.y, 4.00055)]);
    #endif
}
 
/// <summary>
/// 角色顶点运动模糊
/// </summary>
inline void CHARACTER_VERTEXBLUR_FRAG(half NoD, inout half4 color)
{
    #if defined(VERTEXBLUR_ON)
    color.rgb                                   += NoD * color.rgb;
    #endif
}

/// <summary>
/// 角色溶解处理 死亡溶解
/// </summary>
inline void CHARACTER_DISSOLVE_SET(half UseDissolve, half DissolveScale, half EdgeWidth, half4 EdgeColor, half4 uv, inout half4 color)
{
    UNITY_BRANCH
    if(UseDissolve < 0.5)
        return;

    half2 PosS                              = (uv.xy / uv.w) * _ScreenUV;
    half noiseValue                         = saturate(tex2D(_NoiseTex,PosS).r);

    UNITY_BRANCH
    if(noiseValue < DissolveScale)
        discard;

    half EdgeFactor                         = saturate((noiseValue - DissolveScale)/(EdgeWidth * DissolveScale));
    half EdgeArea                           = step(EdgeFactor,0.5);
    color                                   = lerp(color,EdgeColor,EdgeArea);
}

/// <summary>
/// 获取阴影色
/// </summary>
inline void GET_SHADOW_COLOR(half stepMask, half Set_ShadowMask,inout half3 rampShadowColor, inout half3 defaultShadowColor)
{
    #if defined (NIGHT_ON)
		GetShadowColor(0,0,0.95,stepMask,(Set_ShadowMask * 0.5 + 0.5),0.95,rampShadowColor.rgb);
		GetShadowColor(0,0,0.95,stepMask,0.99,0.97,defaultShadowColor.rgb);
	#else
		GetShadowColor(0,0,0.95,stepMask,((1- Set_ShadowMask) * 0.5),0.95,rampShadowColor.rgb);
		GetShadowColor(0,0,0.95,stepMask,0.01,0.97,defaultShadowColor.rgb);
	#endif
}

/// <summary>
/// 设置角色脸部NoL
/// </summary>
inline void CHARACTER_FACEDIR_SET(half3 FaceForwardVector,inout half3 faceLightVectore, inout half3 faceForwardVector, inout half3 faceNoL, half3 L)
{
    faceLightVectore                            = half3(L.x,0,L.z);
	faceForwardVector                           = half3(FaceForwardVector.x,0,FaceForwardVector.z);
	faceForwardVector                           = RotateAroundYInDegrees(faceForwardVector,-90);
	faceNoL                                     = max(0.0, 0.5 * dot(faceForwardVector, normalize(faceLightVectore)) + 0.5);
}

/// <summary>
/// 设置脸部 SDF 阴影
/// </summary>
inline void CHARACTER_FACE_SDF_SET(half UseCameraLight, half2 uv, half3 faceForwardVector, half3 faceLightVectore, half3 faceNoL, inout half sdfShadow)
{
    half faceSDF_Left                           = SAMPLE_TEXTURE2D(_SDFTexture, sampler_SDFTexture, uv).r;
    faceSDF_Left                                = saturate(remap(faceSDF_Left,0.53,1,0.2,1) + 0.2) * UseCameraLight + (1 - UseCameraLight) * faceSDF_Left;
	half faceSDF_Right                          = SAMPLE_TEXTURE2D(_SDFTexture, sampler_SDFTexture, half2(-uv.x, uv.y)).r;
    faceSDF_Right                               = saturate(remap(faceSDF_Right,0.53,1,0.2,1) + 0.2)* UseCameraLight + (1 - UseCameraLight) * faceSDF_Right;
    faceForwardVector                           = RotateAroundYInDegrees(faceForwardVector, -90);
    half faceNolStep                            = step(saturate(dot(faceLightVectore,faceForwardVector)),0);
    
	sdfShadow                                   = saturate(step((faceSDF_Right * faceNolStep + (1 - faceNolStep) * faceSDF_Left), saturate(faceNoL)));
}

/// <summary>
/// 设置脸部阴影
/// </summary>
inline void CHARACTER_FACE_SHADOW_SET(half4 EyeShadowColor,half4 ShadowColor, half defaultShadowStrength, half4 mainColor, half sdfShadow, half4 vColor, inout half4 color)
{
    half eyeArea                                = step(vColor.g,0.5);
	half3 faceShadowColor                       = eyeArea * EyeShadowColor + (1 - eyeArea) * ShadowColor;
    faceShadowColor                             = vColor.b * half3(1,1,1) + (1 - vColor.b) * faceShadowColor;
	half3 shadowCol                             = lerp(faceShadowColor,faceShadowColor * faceShadowColor,defaultShadowStrength);
	color.rgb                                   = (1 - sdfShadow) * mainColor.rgb * shadowCol + mainColor.rgb * sdfShadow;
}

/// <summary>
/// 设置发部投影
/// </summary>
// inline void CHARACTER_HAIR_SHADOW_SET(half2 vertex_depth, half4 PosW, float4 PosSS, half4 mainColor, inout half4 color)
// {
//     UNITY_BRANCH
//     if(_useHairShadow < 0.5)
//         return;
//     
//     half depth                                  = vertex_depth.x / vertex_depth.y;
//     #if defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
//     depth                                       = depth * 0.5 + 0.5;
//     #elif defined (UNITY_REVERSED_Z)
//     depth                                       = 1 - depth;
//     #endif
//
//     half linearEyeDepth                         = LinearEyeDepth(PosW, UNITY_MATRIX_V);
//     
//     float2 screenPos                            = (PosSS.xyz / PosSS.w).xy;
//     half Dis                                    = distance(_WorldSpaceCameraPos.xyz,PosW);
//     half OffsetBalance                          = lerp(1,0.12,saturate(Dis / 6));
//     
//     half3 GetDepth                              = SAMPLE_TEXTURE2D(ShadowMapTexture, sampler_ShadowMapTexture, screenPos + _HairShadowDistace * half2(-0.1,0.1) * OffsetBalance);
//     half rgbSum                                 = saturate(GetDepth.r + GetDepth.g + GetDepth.b);
//     half hairDepth                              = LinearEyeDepth(GetDepth, UNITY_MATRIX_V);
//     hairDepth                                   = hairDepth * rgbSum + (1 - rgbSum) * linearEyeDepth;
//
//     half eyeArea                                = mainColor.a;
//     half3 faceShadowColor                       = eyeArea * _ShadowColor + (1 - eyeArea) * _EyeShadowColor;
//     faceShadowColor                             = mainColor.rgb * faceShadowColor;
//     half depthContrast                          = step(saturate(linearEyeDepth - hairDepth),0);
//
//     color.rgb = lerp(faceShadowColor.rgb,color.rgb,depthContrast);
// }

/// <summary>
/// 角色设置球谐光照
/// </summary>
inline void CHARACTER_SH_SET(half4 SH, half Set_ShadowMask, inout half4 color)
{
    color                                       = lerp(color, SH + color * 0.5, Set_ShadowMask);
}

/// <summary>
/// 角色简单光照阴影
/// </summary>
inline void CHARACTER_SIMPLE_Lighting(half UseAlphaClip, half ShadowRangeStep,half ShadowFeather, half3 N, half3 L,half2 uv ,half4 PosS, inout half cmpShadow, inout half4 color, inout half4 celColor, inout half4 blendColor, inout half4 mainColor)
{   
    #if defined(_USE_MAIN_TEXTURE_LERP)
    half2 screenPos = PosS.xy / PosS.w;
    float noiseValue = SimpleX_Noise(screenPos * _MainTexNoiseTilling) * 0.5 + 0.5;
    half EdgeFactor                         = saturate((noiseValue - _Dissolve_power1)/(_Dissolve_power2 * _Dissolve_power1));

    mainColor = (noiseValue > _Dissolve_power1) ? SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) : SAMPLE_TEXTURE2D(_MainTex02, sampler_MainTex, uv);
    half EdgeArea                           = step(_Dissolve_power1, noiseValue) * step(EdgeFactor, 0.5);
    mainColor                               = _EdgeColor2 * EdgeArea + (1 - EdgeArea) * mainColor;
    #else
    mainColor                               = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
    #endif

    UNITY_BRANCH
    if(UseAlphaClip > 0.5)
    {
        clip(mainColor.a - 0.5);   
    }

    #if defined(_USE_MAIN_TEXTURE_LERP)
    celColor                                    = lerp(SAMPLE_TEXTURE2D(_CelTex, sampler_CelTex, uv),SAMPLE_TEXTURE2D(_CelTex02, sampler_CelTex02, uv),_Dissolve_power1);
    blendColor                                  = lerp(SAMPLE_TEXTURE2D(_BlendTex, sampler_BlendTex, uv),SAMPLE_TEXTURE2D(_BlendTex02, sampler_BlendTex02, uv),_Dissolve_power1);
    #else
    celColor                                    = SAMPLE_TEXTURE2D(_CelTex, sampler_CelTex, uv);
    blendColor                                  = SAMPLE_TEXTURE2D(_BlendTex, sampler_BlendTex, uv);
    #endif

    half NoL                                    = saturate(dot(N, L) * 0.5 + 0.5);

    cmpShadow                                   = 1 - celColor.g * 0.667;
    cmpShadow                                   = 1 - step(cmpShadow, NoL);

    half ShadowRange                            = 1 - (1 - ShadowRangeStep);                    //获取ramp阴影范围
    half Set_ShadowMask                         = saturate(((NoL - (ShadowRange-ShadowFeather)) * - 1.0 ) / (ShadowRange - (ShadowRange-ShadowFeather)) + 1 );            //我也不知道为什么这么算.....因为这样写好看
    half defaultShadow                          = 1 - step(0.1, celColor.g);

    half3 shadowColor                           = lerp(half4(1,1,1,1), blendColor, Set_ShadowMask);
    color.rgb                                   = defaultShadow * blendColor * mainColor + shadowColor * (1 - defaultShadow) * mainColor;
}

/// <summary>
/// 获取角色简单高光
/// </summary>
inline void CHARACTER_SIMPLE_SPEC(half3 N, half3 L, half3 V, half cmpShadow, half4 celColor, inout half4 color)
{
    half specFalloff                            = cmpShadow * 1 + (1- cmpShadow) * _SpecFalloff;
	half specmask                               = celColor.b;
	half specpow                                = 1 - celColor.r;

	half spec                                   = saturate(pow(max(0, dot(N, normalize(V + L))), specpow * 18) - specpow);
	spec                                        = (spec + ceil(spec)) * specmask * specFalloff;
    color.rgb                                   = lerp(color.rgb, half3(1, 1, 1), spec); 
}

/// <summary>
/// 获取简单边缘光
/// </summary>
inline void GET_SIMPLE_RIM_LIGHT(half4 VertexColor, half Set_ShadowMask, half3 RimColor, inout half4 color)
{
    
    RimColor                                    = RimColor * VertexColor.b;
    RimColor                                    = (1 - Set_ShadowMask) * RimColor;
    color.rgb                                   += RimColor;
}

/// <summary>
/// 获取简单自发光
/// </summary>
inline void GET_SIMPLE_EMISSION(half4 EmissionColor, half2 uv, inout half4 color)
{
    #if defined(_USE_MAIN_TEXTURE_LERP)
    half celColorA                                    = lerp(SAMPLE_TEXTURE2D(_CelTex, sampler_CelTex, uv),SAMPLE_TEXTURE2D(_CelTex02, sampler_CelTex02, uv),_Dissolve_power1).a;
    #else
    half celColorA                                    = SAMPLE_TEXTURE2D(_CelTex, sampler_CelTex, uv).a;
    #endif
    
    half4 mainColor                             = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
    half3 Emission                              = EmissionColor * mainColor.rgb * celColorA;
    half3 EyeEmission                           = EmissionColor * mainColor.rgb * (1 - mainColor.a) * saturate(abs(sin(_Time.y * 1.5)));
    color.rgb                                   = color.rgb * (1 - celColorA) + Emission + EyeEmission;
}

/// <summary>
/// 武器的流动光
/// </summary>
inline void GET_WEAPON_FLOWLIGHT(half AddObjectY, half FirstShadowSet, half FirstShadowFeather, half4 FlowColor, half2 uv, half4 PosWS, inout half4 color)
{
    half  PosY                                  = saturate(mul(unity_WorldToObject, PosWS).x + AddObjectY);
    PosY                                        = saturate(abs(frac(PosY)));
    half  MaskDown                              = saturate(PosY * 2.5);
    half  MaskUp                                = saturate((1 - (PosY + 0.2)) * 2.5);
    half  Mask                                  = 1 - MaskDown * MaskUp;
    
    Mask                                        = saturate(((Mask - (FirstShadowSet-FirstShadowFeather)) * - 1.0 ) / (FirstShadowSet - (FirstShadowSet-FirstShadowFeather)) + 1 );
    color                                       = lerp(color, FlowColor + color, Mask);
}

/// <summary>
/// 控制角色暗部亮度
/// </summary>
inline void SetCharacterDarkPartLuminance(half shadow, inout half4 outColor)
{
    UNITY_BRANCH
    if(_UseDarkLuminance < 0.5)
        return;

    half3 hsv                                  = RGBtoHSV(outColor.rgb);
    hsv.b                                      += _DarkLuminance;
    
    outColor.rgb                               = lerp(HSVtoRGB(hsv),outColor.rgb,shadow);
}

/// <summary>
/// 多光源
/// </summary>
inline void Get_CHARACTER_ADDLIGHTS(half UseAddLight, half addShadowRangeStep, half addShadowFeather, half3 N,half3 posW, half4 mainColor, inout half4 color)
{
    UNITY_BRANCH
    if(UseAddLight < 0.5)
        return;

    int addLightsCount                          = GetAdditionalLightsCount();
    half3 addColor                              = half3(0,0,0);
    half shadow = 1;
    for (int i = 0; i < addLightsCount; i++)
    {
        Light addLight                          = GetAdditionalLight(i,posW);
        half3 addLightDir                       = normalize(addLight.direction);
        half NoL                                = max(0, dot(N, addLightDir) * 0.5 + 0.5);
        half Set_ShadowMask                     = 1 - saturate(((NoL - (addShadowRangeStep-addShadowFeather)) * - 1.0 ) / (addShadowRangeStep - (addShadowRangeStep-addShadowFeather)) + 1 );
        shadow                                  = min(shadow, Set_ShadowMask * addLight.distanceAttenuation * addLight.shadowAttenuation);
        addColor                                += Set_ShadowMask * addLight.color * (mainColor * 0.5 + 0.5) * addLight.distanceAttenuation * addLight.shadowAttenuation;
    }
    
    color.rgb                                   += addColor;

    //---------- 控制角色暗部亮度 ----------
    SetCharacterDarkPartLuminance(shadow, color);
}

/// <summary>
/// 角色常驻 假光源
/// </summary>
inline void Get_CHARACTER_ROLE_SPOT_LIGHT(half3 N, half3 PosW, inout half4 color)
{
    float3 lightVector                      = RoleLightPos.xyz - PosW + _WorldPos;
    float distanceSqr                       = max(dot(lightVector, lightVector), HALF_MIN);
    half3 lightDirection                    = half3(lightVector * rsqrt(distanceSqr));
    
    half attenuation                        = DistanceAttenuation(distanceSqr, RoleLightAttention.xy) * AngleAttenuation(RoleSpotDir, lightDirection, RoleLightAttention.zw);
    half3 addLightDir                       = normalize(lightDirection);
    half NoL                                = max(0, dot(N, addLightDir) * 0.5 + 0.5);
    half Set_ShadowMask                     = 1 - saturate(((NoL - 0.2) * - 1.0 ) / 0.3 + 1);
    color.rgb                               += Set_ShadowMask * half3(0.2,0.2,0.2) * attenuation;
}

/// <summary>
/// 角色边特效菲涅尔光
/// </summary>
inline void FRESNEL_SET(half UseAdditionFresnel, half FresnelRange, half FresnelFeather, half4 FresnelColor, half FresnelFlowRange, half NoV, half2 uv, half posZ, inout half4 color)
{
    UNITY_BRANCH
    if(UseAdditionFresnel < 0.5)
        return;
    
    half mask                                   = tex2D(_NoiseTex,TRANSFORM_TEX(uv,_NoiseTex)).r;
    half fresnel                                = pow(1.0 - max(0, NoV), 1);
    fresnel                                     = 1 - saturate( ((fresnel - (FresnelRange - FresnelFeather)) * - 1.0 ) / (FresnelRange - (FresnelRange - FresnelFeather)) + 1 );
    half3 fresnelCol                            = fresnel * FresnelColor * mask;
    half flowMask                               = saturate( ((posZ - (FresnelFlowRange - 0.1)) * - 1.0 ) / (FresnelFlowRange - (FresnelFlowRange - 0.1)) + 1 );
    fresnelCol                                 *= flowMask;
    color.rgb                                  += fresnelCol;
}

/// <summary>
/// 双线性插值
/// </summary>
// inline half texture2DShadowLerp(float2 uv, half sceneDepth, half bias)
// {
//     float2 texelSize                                = _CharacterDepthTexture_TexelSize.xy;
//     half size                                       = _CharacterDepthTexture_TexelSize.z;
//     half2 centroidUV                                = floor(uv * size + 0.5) / size;
//     half2 f                                         = frac(uv * size + 0.5);
//
//     half lb                                         = DecodeRGBA(SAMPLE_TEXTURE2D(_CharacterDepthTexture,sampler_CharacterDepthTexture,centroidUV + texelSize * float2(0.0, 0.0)));
//     lb                                              = sceneDepth - bias > lb ? 1.0 : 0.0;
//     half lt                                         = DecodeRGBA(SAMPLE_TEXTURE2D(_CharacterDepthTexture,sampler_CharacterDepthTexture,centroidUV + texelSize * float2(0.0, 1.0)));
//     lt                                              = sceneDepth - bias > lt ? 1.0 : 0.0;
//     half rb                                         = DecodeRGBA(SAMPLE_TEXTURE2D(_CharacterDepthTexture,sampler_CharacterDepthTexture,centroidUV + texelSize * float2(1.0, 0.0)));
//     rb                                              = sceneDepth - bias > rb ? 1.0 : 0.0;
//     half rt                                         = DecodeRGBA(SAMPLE_TEXTURE2D(_CharacterDepthTexture,sampler_CharacterDepthTexture,centroidUV + texelSize * float2(1.0, 1.0)));
//     rt                                              = sceneDepth - bias > rt ? 1.0 : 0.0;
//     half a                                          = lerp(lb, lt, f.y);
//     half b                                          = lerp(rb, rt, f.y);
//     half c                                          = lerp(a, b, f.x);
//     return c;
// }

/// <summary>
/// PCF 计算
/// </summary>
// inline half PercentCloaerFilter(half3 posVertex,half2 uv , half sceneDepth , half bias)
// {
//     half shadow                                     = 0.0;
//     half2 texelSize                                 = _CharacterDepthTexture_TexelSize.xy;
//
//     for(int x = -_filterSize; x <= _filterSize; ++x)
//     {
//         for(int y = -_filterSize; y <= _filterSize; ++y)
//         {
//             half2 uv_offset                         = half2(x,y) * texelSize;
//             half depth                              = texture2DShadowLerp(uv + uv_offset,sceneDepth,bias);
//             shadow                                  += depth;   
//         }    
//     }
//     half total                                      = (_filterSize * 2 + 1) * (_filterSize * 2 + 1);
//     shadow                                          /= total;
//     return shadow;
// }

/// <summary>
/// 泊松分布
/// </summary>
// inline void BuildPoissonDisk()
// {
//     poissonDisk[0]                                  = half2(-0.94201624, -0.39906216);
//     poissonDisk[1]                                  = half2(0.94558609, -0.76890725);
//     poissonDisk[2]                                  = half2(-0.094184101, -0.92938870);
//     poissonDisk[3]                                  = half2(0.34495938, 0.29387760);
//     poissonDisk[4]                                  = half2(-0.91588581, 0.45771432);
//     poissonDisk[5]                                  = half2(-0.81544232, -0.87912464);
//     poissonDisk[6]                                  = half2(-0.38277543, 0.27676845);
//     poissonDisk[7]                                  = half2(0.97484398, 0.75648379);
//     poissonDisk[8]                                  = half2(0.44323325, -0.97511554);
//     poissonDisk[9]                                  = half2(0.53742981, -0.47373420);
//     poissonDisk[10]                                 = half2(-0.26496911, -0.41893023);
//     poissonDisk[11]                                 = half2(0.79197514, 0.19090188);
//     poissonDisk[12]                                 = half2(-0.24188840, 0.99706507);
//     poissonDisk[13]                                 = half2(-0.81409955, 0.91437590);
//     poissonDisk[14]                                 = half2(0.19984126, 0.78641367);
//     poissonDisk[15]                                 = half2(0.14383161, -0.14100790);
// }

/// <summary>
/// 获取pcf偏移
/// </summary>
// inline half GetPCFShadowBias(half3 lightDir , half3 normal , half maxBias, half baseBias)
// {
//     half cos_val                                    = saturate(dot(lightDir, normal));
//     half sin_val                                    = sqrt(1 - cos_val * cos_val); // sin(acos(L·N))
//     half tan_val                                    = sin_val / cos_val;    // tan(acos(L·N))
//     half bias                                       = baseBias + clamp(tan_val, 0 , maxBias) ;
//     return                                          bias;
// }

/// <summary>
/// 获取pcf阴影
/// </summary>
// inline void GetCharacterPCFShadow(half3 N, half3 L, half3 PosW, half2 uv0, inout half shadowMask, inout half SetShadow)
// {
//     UNITY_BRANCH
//     if(_useRealTimeShadow < 0.5)
//         return;
//     
//     half bias                                       = GetPCFShadowBias(L, N, _maxBias, _baseBias);
//     half4 ndcpos                                    = mul(BARD_SHADOW_MAP_VP, float4(PosW,1));
//     ndcpos.xyz                                      = ndcpos.xyz / ndcpos.w;
//     half3 uvpos                                     = ndcpos.xyz * 0.5 + 0.5;
//     half shadow                                     = PercentCloaerFilter(PosW.xyz, uvpos.xy, uvpos.z, bias);
//     shadowMask                                      = shadow;
//     SetShadow										= max(SetShadow, shadowMask);
// }

/// <summary>
/// 角色白平衡
/// </summary>
inline void SetCharacterWhiteBalance(half UseWhiteBalance, half3 kRGB,inout half4 color)
{
    UNITY_BRANCH
    if (UseWhiteBalance < 0.5)
        return;
    
    color.r                                        *= kRGB.r;
    color.g                                        *= kRGB.g;
    color.b                                        *= kRGB.b;
}

/// <summary>
/// 角色冰冻
/// </summary>
inline void SetCharacterIce(half UseIce, half3 N, half4 T, half3 L, half3 V, half2 uv, inout half4 outColor)
{
    UNITY_BRANCH
    if(UseIce < 0.5)
        return;
    
    half3 normalTS                                                      = SampleNormal(uv, TEXTURE2D_ARGS(_NormalMap, sampler_NormalMap), _IceNormalScale);
    half sgn                                                            = T.w;
    half3 bitangent                                                     = sgn * cross(N.xyz, T.xyz);
    half3 normal                                                        = TransformTangentToWorld(normalTS, half3x3(T.xyz, bitangent.xyz, N));
    normal                                                              = lerp(N,normal,0.5);
    half3 IceBaseColor                                                  = _IceTexture.Sample(sampler_IceTexture,uv) * pow(_IceVFXColor,0.4545454545);
    half fresnel                                                        = saturate( ((saturate(dot(normal,V) - (_Ice_Fresnel_Step - _Ice_Fresnel_Feather)) * - 1.0 ) / (_Ice_Fresnel_Step - (_Ice_Fresnel_Step - _Ice_Fresnel_Feather)) + 1));
    half3 IceRim                                                        = pow(_IceRimColor,0.45454545) * fresnel;
    
    half3 IceSpec                                                       = saturate(pow(saturate(dot(normalize(L + V),normal)),_IceSpecPower)) * pow(_IceSpecColor,0.4545454545);
    half3 IceColor                                                      = IceBaseColor;
    
    half mask                                                           = tex2D(_NoiseTex,TRANSFORM_TEX(uv * half2(2,2),_NoiseTex)).r;

    outColor.rgb                                                        = lerp(IceColor,outColor,mask) + IceSpec + IceRim;;
}

/// <summary>
/// 获取无光照材质
/// </summary>
inline half4 GetUnlitColor(half2 uv, float4 PosS)
{
    half4 unlitColor                                = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv);
    clip(unlitColor.a - 0.5);
    return unlitColor;
}

/// <summary>
/// 角色世界坐标溶解
/// </summary>
inline void SetCharacterPosWDissolve(half4 vColor, half2 uv, inout half4 color)
{
    #if defined ROLE_POSW_DISSOLVE
    half mask                                       = tex2D(_NoiseTex,TRANSFORM_TEX(uv * half2(3,3),_NoiseTex)).r;
    half height                                     = vColor.g - mask * _PosWNoiseScale;
                                        
    half show                                       = step(_PosWDissolveScale,height);
    show                                            = (1 - show) * _Top2Bottom + (1 - _Top2Bottom) * show;
    clip(show - 0.5);
    half edge                                       = _Top2Bottom * (1 - step(vColor.g,(_PosWDissolveScale - _PosWDissolveWidth))) + (1 - _Top2Bottom) * step(vColor.g,(_PosWDissolveScale + _PosWDissolveWidth));
    edge                                            = edge * show;
    color                                           = edge * _PosWDissolveColor + (show * color);
    #endif
}

/// <summary>
/// 视差Eye
/// </summary>
inline void SetCharacterEyeParallax(half Height,half ParallaxScale,half3 T,half3 B, half3 N, half3 V, half2 uv, half mask,inout half4 color)
{
    float3x3 MatrxiTBN                              = float3x3(T.x,B.x,N.x,T.y,B.y,N.y,T.z,B.z,N.z);
    V                                               = mul(MatrxiTBN,V);
    V                                               = V * Height * ParallaxScale;
    half2 setUV                                     = uv - V.xy;
    half eyeArea                                    = 1 - step(mask,0.9);
    half3 parallaxColor                             = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,setUV).rgb;
    color.rgb                                       = parallaxColor * eyeArea + (1 - eyeArea) * color;
}

/// <summary>
/// CharacterHSV
/// </summary>
inline void SetCharacterHSV(half Hue,half Saturation, half Value, inout half4 color)
{
    UNITY_BRANCH
    if(_UseHSV < 0.5)
        return;
    
    half3 hsv                                       = RGBtoHSV(color.rgb);
    hsv.x                                           = hsv.x + Hue;
    hsv.y                                           = hsv.y + Saturation;
    hsv.z                                           = hsv.z + Value;
    color.rgb                                       = HSVtoRGB(hsv);
}

/// <summary>
/// SetCharacterStocking
/// </summary>
inline void SetCharacterStocking(half stocking,half NoV,half ShadowMask, half2 uv, inout half4 outColor)
{
    #if defined USE_STOCKING
    half Drak_1st                                   = saturate( ((saturate(NoV) - (_First_Fresnel_Shadow_Step - _First_Fresnel_Shadow_Feather)) * - 1.0 ) / (_First_Fresnel_Shadow_Step - (_First_Fresnel_Shadow_Step - _First_Fresnel_Shadow_Feather)) + 1 );
    half4 setColor                                  = lerp(outColor,outColor * _First_Fresnel_Shadow_Color,Drak_1st);
    
    half Drak_2nd                                   = saturate( ((saturate(NoV) - (_Second_Fresnel_Shadow_Step - _Second_Fresnel_Shadow_Feather)) * - 1.0 ) / (_Second_Fresnel_Shadow_Step - (_Second_Fresnel_Shadow_Step - _Second_Fresnel_Shadow_Feather)) + 1 );
    setColor                                        = lerp(setColor,outColor * _Second_Fresnel_Shadow_Color,Drak_2nd);
    
    half Bright                                     = 1 - saturate( ((saturate(NoV) - (_Fresnel_Light_Step - _Fresnel_Light_Feather)) * - 1.0 ) / (_Fresnel_Light_Step - (_Fresnel_Light_Step - _Fresnel_Light_Feather)) + 1 );
    setColor                                        += Bright * _Fresnel_Light_Color * outColor;

    half4 silk                                      = SAMPLE_TEXTURE2D(_StockingTex,sampler_StockingTex,uv * _StockingTex_ST.xy + _StockingTex_ST.zw) * 5;
    setColor.rgb                                    = silk * setColor.rgb * 0.1 + setColor.rgb * 0.95;
    outColor                                        = lerp(outColor,setColor,stocking);
    #endif
}

/// <summary>
/// 场景阴影
/// </summary>
// inline void SetCharacterSenceShadow(inout half4 color, half shadow, half3 shadowColor)
// {
//     #if defined RECEIVE_SHADOW
//     color.rgb                                       = lerp(shadowColor,color,saturate(shadow)).rgb;
//     #endif
// }

inline half GetCharacterSenceShadow()
{
    #if defined WORLD_SPACE_SAMPLE_SHADOW
    float2 set_uv                                   = _WorldPos.xz - _SenceShadowOffset;
    set_uv                                          = set_uv / _SenceShadowScale;
    half shadow                                    = SAMPLE_TEXTURE2D(_SenceShadowMaskTexture,sampler_SenceShadowMaskTexture,set_uv).r;
    shadow                                          = step(0.9,shadow.r);
    return                                          shadow;
    #else
    return 1;
    #endif
}

inline void SetCharacterSenceShadow(half shadow, inout half4 color, half3 shadowColor, inout half SpecFalloff)
{
    #if defined WORLD_SPACE_SAMPLE_SHADOW
    SpecFalloff                                     *= shadow;
    color.rgb                                       = lerp(shadowColor,color,shadow.r).rgb;
    #endif
}

/// <summary>
/// 场景阴影(脸部)
/// </summary>
// inline void SetCharacterFaceSenceShadow(inout half4 color, half3 shadowColor,half3 EyeShadowColor, half4 vColor, half shadow)
// {
//     #if defined RECEIVE_SHADOW
//     half eyeArea                                    = step(vColor.g,0.5);
//     half3 faceShadowColor                           = eyeArea * EyeShadowColor + (1 - eyeArea) * shadowColor;
//     color.rgb                                       = lerp(faceShadowColor,color,shadow).rgb;
//     #endif
// }
inline void SetCharacterFaceSenceShadow(inout half4 color, half3 shadowColor,half3 EyeShadowColor, half4 vColor)
{
    #if defined WORLD_SPACE_SAMPLE_SHADOW
    float2 set_uv                                   = _WorldPos.xz - _SenceShadowOffset;
    set_uv                                          = set_uv / _SenceShadowScale;
    half4 shadow                                    = SAMPLE_TEXTURE2D(_SenceShadowMaskTexture,sampler_SenceShadowMaskTexture,set_uv);
    shadow                                          = step(0.9,shadow.r);

    half eyeArea                                    = step(vColor.g,0.5);
    half3 faceShadowColor                           = eyeArea * EyeShadowColor + (1 - eyeArea) * shadowColor;
    color.rgb                                       = lerp(faceShadowColor,color,shadow.r).rgb;
    #endif
}

/// <summary>
/// Transparent
/// </summary>
inline void SetTransparent(float4 PosS, inout half4 color, half alpha = 1)
{
    #if defined(_TRANSPARENT)
    half2 screenUV                                                  = PosS.xy / PosS.w;
    half4 opaqueColor											    = SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,screenUV);
    color.rgb                                                       = lerp(opaqueColor.rgb,color.rgb,_MainColor.a * alpha);
    #endif
}

/// <summary>
/// Voronoi2D
/// </summary>
inline half3 hash3( half2 p )
{
    half3 q                                                         = half3(    dot(p, half2(127.1,311.7)), 
                                                                                dot(p, half2(269.5,183.3)), 
                                                                                dot(p, half2(419.2,371.9)) );
    return                                                          frac(sin(q)*43758.5453);
}

inline half Voronoi2D( in half2 uv)
{
    half2 p                                                         = 0.5 - 0.5 * cos(2 * half2(1.0,0.5) );

    p                                                               = p * p * (3.0 - 2.0 * p);
    p                                                               = p * p * (3.0 - 2.0 * p);
    p                                                               = p * p * (3.0 - 2.0 * p);
    
    half k                                                          = 1.0 + 63.0 * pow(1.0 - p.y, 6.0);
                                                        
    half2 i                                                         = floor(uv);
    half2 f                                                         = frac(uv);
    
    half2 a                                                         = half2(0.0,0.0);
    for( int y=-2; y<=2; y++ )
        for( int x=-2; x<=2; x++ )
        {
            half2  g                                                = half2( x, y );
            half3  o                                                = hash3( i + g ) * half3(p.x,p.x,1.0);
            half2  d                                                = g - f + o.xy;
            half w                                                  = pow( 1.0-smoothstep(0.0,1.414,length(d)), k );
            a                                                       += half2(o.z*w,w);
        }
	
    return                                                          a.x/a.y;
}

/// <summary>
/// 冰冻块面溶解
/// </summary>
inline void IceQuadDissolve(half3 N, half4 T, half3 L, half3 V, half2 uv, inout half4 outColor)
{
    UNITY_BRANCH
    if(_UseFaceIceQuad < 0.5)
        return;
    
    half voronoi                                                    = Voronoi2D(uv * half2(_IceQuadDensity,_IceQuadDensity));
    voronoi                                                         = remap(voronoi,0,1,0,0.9);
    half mask                                                       = voronoi;
    mask                                                            = step(mask,_IceQuadDissolveScale);
    half noiseMask                                                  = tex2D(_NoiseTex,TRANSFORM_TEX(uv * half2(2,2),_NoiseTex)).r;
    noiseMask                                                       = remap(noiseMask,0.85,0.1,1,0.2);

    
    half3 normalTS                                                  = SampleNormal(uv, TEXTURE2D_ARGS(_NormalMap, sampler_NormalMap), _IceNormalScale);
    half sgn                                                        = T.w;
    half3 bitangent                                                 = sgn * cross(N.xyz, T.xyz);
    half3 normal                                                    = TransformTangentToWorld(normalTS, half3x3(T.xyz, bitangent.xyz, N));
    normal                                                          = lerp(N,normal,0.5);
    half3 IceBaseColor                                              = _IceTexture.Sample(sampler_IceTexture,uv) * _IceVFXColor;
    half fresnel                                                    = saturate( ((saturate(dot(normal,V) - (_Ice_Fresnel_Step - _Ice_Fresnel_Feather)) * - 1.0 ) / (_Ice_Fresnel_Step - (_Ice_Fresnel_Step - _Ice_Fresnel_Feather)) + 1));
    half3 IceRim                                                    = _IceRimColor * fresnel;
    
    half3 IceSpec                                                   = pow(saturate(dot(normalize(L + V),normal)),_IceSpecPower) * _IceSpecColor;
    half3 IceColor                                                  = IceBaseColor;

    IceColor                                                        = lerp(IceColor,outColor,noiseMask) + IceSpec + IceRim;
    outColor.rgb                                                    = lerp(outColor,IceColor,mask);
}

/// <summary>
/// 设置角色Unity雾
/// </summary>
inline void SetCharacterUnityFog(half fogCoord, inout half4 outColor)
{
    UNITY_BRANCH
    if(_UseUnityFog < 0.5)
        return;

    outColor.rgb                                                    = MixFog(outColor.rgb, fogCoord);
}