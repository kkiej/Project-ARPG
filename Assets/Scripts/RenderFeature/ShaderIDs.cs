using UnityEngine;

internal static class ShaderIDs
{
    /// <summary>
    /// CONST
    /// </summary>
    internal const int SDF_AO_MAX_COUNT = 24;

    /// <summary>
    /// 资源路径
    /// </summary>
    //  Feature Data 的路径
    internal static string urpAssetData = "URPSettings/UniversalRenderPipelineAsset_Renderer";
    //  Render Asset 的路径
    internal static string urpAssetPath = "URPSettings/UniversalRenderPipelineAsset";
    //  CameraRenderData  的路径
    internal static string cameraRenderData = "URPSettings/BardCameraRenderData";
    //  VolumeData
    internal static string postProcessVolume = "URPSettings/GlobalVolumeProfile";
    //  Shader 路径
    internal static string shaderPathPrefix = "Assets/Art/Bard_3D_Share/Renderer/Shader/";
    
    
    
    /// <summary>
    /// 贴图路径
    /// </summary>
    //  场景接触阴影路径
    internal static string senceTexPath = "RenderResources/SenceShadowTexture/";
    //  灯光参数json存储路径
    internal static string SaveDataPath = "Assets/Art/Bard_3D_Share/ADBundle/Lighting/";
    //  角色冰冻图片存储路径
    internal static string IceTexPath = "RenderResources/CharacterTexture/IceTex";
    //  角色冰冻法线贴图存储路径
    internal static string IceNormPath = "RenderResources/CharacterTexture/IceNorm";
    //  溶解贴图路径
    internal static string dissolveTex = "RenderResources/CharacterTexture/dissolveTex";
    //  渲染遮罩贴图路径
    internal static string RotatMaskTex = "Assets/Art/Bard_3D_Share/01_VFXCommon/Texture/Fx_tex_noise_rotattex.png";

    /// <summary>
    /// Shader名称
    /// </summary>
    
    //  简单半透明材质
    internal static string simple_transparent_name = "bard/role/simple(transparent)";
    //  眼睛半透明材质
    internal static string eye_transparent_name = "bard/role/eye(transparent)";
    //  简单材质
    internal static string simple_name = "bard/role/simple";
    //  眼睛材质
    internal static string eye_name = "bard/role/eye";
    //  角色身体材质
    internal static string bard_character_body_name = "bard/role/body";
    //  角色身体半透明材质
    internal static string bard_character_body_transparent_name = "bard/role/body(transparent)";
    //  角色脸部材质
    internal static string bard_character_face_name = "bard/role/face";
    //  角色脸部半透明材质
    internal static string bard_character_face_transparent_name = "bard/role/face(transparent)";
    //  角色头发材质
    internal static string bard_character_hair_name = "bard/role/hair";
    //  角色头发半透明材质
    internal static string bard_character_hair_transparent_name = "bard/role/hair(transparent)";
    
    
    /// <summary>
    /// Shader路径
    /// </summary>
    //  后处理部分
    
    //  高斯模糊
    internal static string GaussianBlur = "Shader_PostProcessing/GaussianBlur/Shader/GaussianBlur";
    //  径向模糊
    internal static string RadiaBlur = "Shader_PostProcessing/RadiaBlur/Shader/RadiaBlur";
    //  径向模糊
    internal static string AddRadiaBlur = "Shader_PostProcessing/AddRadiaBlur/Shader/AddRadiaBlur";
    //  RGB分离
    internal static string RGBSplit = "Shader_PostProcessing/RGBSplit/Shader/RGBSplit";
    //  后效外勾边
    internal static string OutLinePost = "Shader_PostProcessing/OutLinePost/Shaders/OutLinePostEffect";
    //  后效外勾边遮罩
    internal static string OutlinePrePass = "Shader_PostProcessing/OutLinePost/Shaders/OutlinePrePass";
    //  全局雾效
    internal static string GlobalFog = "Shader_PostProcessing/GlobalFog/Shaders/GlobalFog";
    //  天空盒渐显
    internal static string FadeToSky = "Shader_PostProcessing/GlobalFog/Shaders/FadeToSkybox";
    //  四周压暗
    internal static string BardVignette = "Shader_PostProcessing/Vignette/Vignette";
    //  显示轮廓
    internal static string BehindShadow = "Shader_PostProcessing/BehindShadowEffect/Shader/BehindShadowEffect";
    //  动态模糊
    internal static string ImageMotionBlur = "Shader_PostProcessing/ImageMotionBlur/Shader/ImageMotionBlur";
    //  LUT矫正
    internal static string LutCorrect = "Shader_PostProcessing/LUTCorrect/Shader/LUTCorrect";
    //  抓铺屏幕模糊
    internal static string GrabPassGaussian = "Shader_PostProcessing/GrabPassBlur/Shader/GaussianBlur";
    //  眨眼效果
    internal static string WinkVignette = "Shader_PostProcessing/WinkVignette/Shaders/WinkVignette";
    //  屏幕描边
    internal static string RobertEdgeDetection = "Shader_PostProcessing/RobertEdgeDetection/RobertEdgeDetection";
    //  屏幕波纹
    internal static string RingWave = "Shader_PostProcessing/RingWave/RingWave";
    //  颜色空间
    internal static string Linear2Gamma = "Shader_PostProcessing/UIGammaCorrection/Shader/Linear2Gamma";
    //  高度雾
    internal static string HeightFog = "Shader_PostProcessing/HeightFog/HeightFog";
    //  体积雾过滤
    internal static string VolumetricFogFilter = "Shader_PostProcessing/VolumeFog/VolumetricFogFilter";
    //  体积雾
    internal static string VolumetricFogCompose = "Shader_PostProcessing/VolumeFog/VolumetricFogCompose";
    //  体积雾拷贝深度
    internal static string VolumetricFogCopyDepth = "Shader_PostProcessing/VolumeFog/VolumetricFogCopyDepth";
    //  模糊四周
    internal static string BlurVignette = "Shader_PostProcessing/GrabPassBlur/Shader/BlurVignette";    
    //  波浪抖动
    internal static string WaveJitter = "Shader_PostProcessing/WaveJitter/WaveJitter"; 
    //  速度线
    internal static string SpeedLine = "Shader_PostProcessing/SpeedLine/SpeedLine";
    //  BW黑白闪 第三代
    internal static string BWFlash = "Shader_PostProcessing/BWFlash/BWFlash";
    //  天空变色
    internal static string SkyDark = "Shader_PostProcessing/SkyDark/SkyDark";
    //  SSPR
    internal static string SSPR = "Shader_PostProcessing/ScreenSpacePlaneReflection/MobileSSPRComputeShader";
    //  SSAO
    internal static string SSAO = "Shader_PostProcessing/AmbientOcclusion/SSAO";
    //  HBAO
    internal static string HBAO = "Shader_PostProcessing/AmbientOcclusion/HBAO";
    //  ScreenMask
    internal static string ScreenMask = "Shader_PostProcessing/ScreenMask/ScreenMask";
    //  泛光
    internal static string Glow = "Shader_PostProcessing/Glow/Glow";
    //  扫描出现
    internal static string ScanApperance = "Shader_PostProcessing/ScanApperance/ScanApperance";
    //  扫描遮罩
    internal static string ScanMask = "Shader_PostProcessing/ScanApperance/ScanMask";
    //  球面化
    internal static string Spherize = "Shader_PostProcessing/Spherize/Spherize";
    //  景深
    internal static string DepthOfField = "Shader_PostProcessing/DepthOfField/DepthOfField";
    //  散景模糊
    internal static string Bokeh = "Shader_PostProcessing/Bokeh/Bokeh";
    //  散景模糊
    internal static string ImageOutLinePost = "Shader_PostProcessing/ImageOutLinePost/ImageOutLinePost";

    //UI部分
    //  普通
    internal static string Common = shaderPathPrefix + "Shader_UI/bard_ui_common.shader";
    //  普通 + 描边
    internal static string CommonOutLine = shaderPathPrefix + "Shader_UI/bard_ui_common_outline.shader";
    //  变亮
    internal static string UIaddColor = shaderPathPrefix + "Shader_UI/bard_UI_AddColor.shader";
    //  变亮 + 描边
    internal static string UIaddColorOutLine = shaderPathPrefix + "Shader_UI/bard_UI_AddColor_Outline.shader";
    //  插值
    internal static string UIlerpColor = shaderPathPrefix + "Shader_UI/bard_UI_LerpColor.shader";
    //  插值 + 描边
    internal static string UIlerpColorOutLine = shaderPathPrefix + "Shader_UI/bard_UI_LerpColor_OutLine.shader";
    //  变量 乘
    internal static string UIMultiColor = shaderPathPrefix + "Shader_UI/bard_UI_MultiColor.shader";
    //  变量 乘 + 描边
    internal static string UIMultiColorOutLine = shaderPathPrefix + "Shader_UI/bard_UI_MultiColor_OutLine.shader";
    //  角色小人 
    internal static string UIsmallCharacter = shaderPathPrefix + "Shader_UI/bard_ui_smallCharacter.shader";
    //  透明遮罩
    internal static string UIAlphaMask = shaderPathPrefix + "Shader_UI/bard_ui_alphaMask.shader";
    //  动态遮罩
    internal static string UIDynmicMask = shaderPathPrefix + "Shader_UI/UIDynmicMask.shader";
    //  变灰
    internal static string UIGrey = shaderPathPrefix + "Shader_UI/bard_UI_Grey.shader";
    //  变灰 + 描边
    internal static string UIGreyOutLine = shaderPathPrefix + "Shader_UI/bard_UI_Grey_OutLine.shader";
    //  canvas mask
    internal static string UIDefaultMask = shaderPathPrefix + "Shader_UI/UI-CanvasMask.shader";
    //  mipmap的模糊
    internal static string MipMapBlur = shaderPathPrefix + "Shader_UI/MipMapBlur.shader";
    //  UI抓取屏幕
    internal static string UIGrabScreenBlur = shaderPathPrefix + "Shader_UI/bard_ui_grabScreenBlur.shader";
    //  小地图遮罩生成
    internal static string bard_ui_mini_map_mask = shaderPathPrefix + "Shader_UI/draw_minimap_mask.compute";
    //  小地图遮罩
    internal static string MiniMapMask = shaderPathPrefix + "Shader_UI/MiniMapMask.shader";
    //  小地图遮罩
    internal static string MiniMapMaskSecond = shaderPathPrefix + "Shader_UI/MiniMapMaskSecond.shader";
    //  小地图遮罩绘制
    internal static string MiniMapMaskCreator = shaderPathPrefix + "Shader_UI/MiniMapMaskCreator.shader";
    //  小地图遮罩绘制
    internal static string MiniMapMaskCreatorSecond = shaderPathPrefix + "Shader_UI/MiniMapMaskCreatorSecond.shader";
    //  平移和缩放
    internal static string TillingOffset = shaderPathPrefix + "Shader_UI/bard_ui_tilling-offset.shader";
    //  虚拟透视
    internal static string VirtualPrespective = shaderPathPrefix + "Shader_UI/Orth2Persp.shader";
    //  虚拟透视 + 描边
    internal static string VirtualPrespectiveOutLine = shaderPathPrefix + "Shader_UI/Orth2PerspOutLine.shader";
    //  Image外勾边
    internal static string ImageOutLine = shaderPathPrefix + "Shader_UI/ImageOutLine.shader";

    //Character
    //  黑白闪
    internal static string whiteBlackFlash = shaderPathPrefix + "Shader_Character/bard_character_black_white.shader";
    //  角色深度
    internal static string CharacterDepth = shaderPathPrefix + "Shader_Character/bard_character_depth.shader";
    //  简单半透明材质
    internal static string simple_transparent_path = shaderPathPrefix + "Shader_Character/bard_character_simple_transparent.shader";
    //  简单材质
    internal static string simple_path = shaderPathPrefix + "Shader_Character/bard_character_simple.shader";
    //  眼睛半透明材质
    internal static string eye_transparent_path = shaderPathPrefix + "Shader_Character/bard_character_eye_transparent.shader";
    //  眼睛材质
    internal static string eye_path = shaderPathPrefix + "Shader_Character/bard_character_eye.shader";
    //  角色身体材质
    internal static string bard_character_body_path = shaderPathPrefix + "Shader_Character/bard_character_body.shader";
    //  角色身体半透明材质
    internal static string bard_character_body_transparent_path = shaderPathPrefix + "Shader_Character/bard_character_body_transparent.shader";
    //  角色脸部材质
    internal static string bard_character_face_path = shaderPathPrefix + "Shader_Character/bard_character_face.shader";
    //  角色脸部半透明材质
    internal static string bard_character_face_transparent_path = shaderPathPrefix + "Shader_Character/bard_character_face_transparent.shader";
    //  角色头发材质
    internal static string bard_character_hair_path = shaderPathPrefix + "Shader_Character/bard_character_hair.shader";
    //  角色头发半透明材质
    internal static string bard_character_hair_transparent_path = shaderPathPrefix + "Shader_Character/bard_character_hair_transparent.shader";
    
    
    /// <summary>
    /// 渲染Pass String
    /// </summary>
    //  模板Pass
    internal static string StencilPass = "Stencil";
    //  发光外勾边Pass
    internal static string ShineOutLinePass = "ShineOutLine";
    //  外勾边Pass
    internal static string OutLinePass = "OutLine";
    //  深度Pass
    internal static string DepthOnlyPass = "DepthOnly";

    
    
    /// <summary>
    /// UI ShaderID
    /// </summary>
    //  无光照
    internal static readonly int UlintColor = Shader.PropertyToID("_UlintColor");
    //  add拉条
    internal static readonly int AddSlider = Shader.PropertyToID("_AddSlider");
    //  mul拉条
    internal static readonly int MultiSlider = Shader.PropertyToID("_MultiSlider");
    //  混合颜色
    internal static readonly int BlendColor = Shader.PropertyToID("_BlendColor");
    //  透明度值
    internal static readonly int Alpha = Shader.PropertyToID("_Alpha");
    //  强度值
    internal static readonly int Scale = Shader.PropertyToID("_Scale");
    //  马赛克强度
    internal static readonly int MosaicScale = Shader.PropertyToID("_MosaicScale");
    //  模板值
    internal static readonly int Stencil = Shader.PropertyToID("_Stencil");
    //  模板比较
    internal static readonly int StencilComp = Shader.PropertyToID("_StencilComp");
    //  模板通道
    internal static readonly int StencilOp = Shader.PropertyToID("_StencilOp");


    /// <summary>
    /// 角色ShaderID
    /// </summary>
    //  发光外勾边宽度
    internal static readonly int ShineOutLineWidth = Shader.PropertyToID("_StencilOutlineWidth");
    //  发光外勾边颜色
    internal static readonly int ShineOutLineColor = Shader.PropertyToID("_StencilOutLineColor");
    //  外勾边宽度
    internal static readonly int OutLineWidth = Shader.PropertyToID("_OutlineWidth");
    //  使用相机灯光
    internal static readonly int UseCameraLight = Shader.PropertyToID("_UseCameraLight");
    //  SS边缘光范围
    internal static readonly int SSRimScale = Shader.PropertyToID("_SSRimScale");
    //  普通边缘光范围
    internal static readonly int RimRangeStep = Shader.PropertyToID("_RimRangeStep");
    //  边缘光颜色
    internal static readonly int RimColor = Shader.PropertyToID("_RimColor");
    //  边缘光颜色
    internal static readonly int EnemyRimColor = Shader.PropertyToID("_RimLightColor");
    //  暗部亮度控制开关
    internal static readonly int UseDarkLuminance = Shader.PropertyToID("_UseDarkLuminance");
    //  暗部亮度值
    internal static readonly int DarkLuminance = Shader.PropertyToID("_DarkLuminance");
    //  卡通灯光方向
    internal static readonly int ToonLightDirection = Shader.PropertyToID("_ToonLightDirection");
    //  相机fov
    internal static readonly int CameraFOV = Shader.PropertyToID("_CameraFOV");
    //  卡通灯光强度
    internal static readonly int ToonLightStrength = Shader.PropertyToID("_ToonLightStrength");
    //  角色世界坐标
    internal static readonly int WorldPos = Shader.PropertyToID("_WorldPos");
    //  平面位置
    internal static readonly int ShadowPlane = Shader.PropertyToID("_ShadowPlane");
    //  平面参数
    internal static readonly int ShadowFadeParams = Shader.PropertyToID("_ShadowFadeParams");
    //  平面衰减
    internal static readonly int ShadowInvLen = Shader.PropertyToID("_ShadowInvLen");
    //  平面阴影颜色
    internal static readonly int PlaneShadowColor = Shader.PropertyToID("_PlaneShadowColor");
    //  阴影偏移
    internal static readonly int ShadowOffset = Shader.PropertyToID("_ShadowOffset");
    //  灯光颜色
    internal static readonly int LightColor = Shader.PropertyToID("_LightColor");
    //  额外灯光颜色
    internal static readonly int AddColor = Shader.PropertyToID("_AddLightColor");
    //  使用add颜色
    internal static readonly int UseAddColor = Shader.PropertyToID("_UseAddColor");
    //  使用白平衡
    internal static readonly int UseWhiteBalance = Shader.PropertyToID("_UseWhiteBalance");
    //  脸部朝向
    internal static readonly int FaceForwardVector = Shader.PropertyToID("_FaceForwardVector");
    //  点阵化透明
    internal static readonly int Transparency = Shader.PropertyToID("_Transparency");
    //  渲染等级
    internal static readonly int RenderScale = Shader.PropertyToID("_RenderScale");
    //  使用死亡溶解
    internal static readonly int UseDissolve = Shader.PropertyToID("_UseDissolve");
    //  死亡溶解强度
    internal static readonly int DissolveScale = Shader.PropertyToID("_DissolveScale");
    //  死亡溶解边缘宽度
    internal static readonly int EdgeWidth = Shader.PropertyToID("_EdgeWidth");
    //  死亡溶解边缘颜色
    internal static readonly int EdgeColor = Shader.PropertyToID("_EdgeColor");
    //  使用多光源
    internal static readonly int UseAddLight = Shader.PropertyToID("_UseAddLight");
    //  多光源阴影范围
    internal static readonly int addShadowRangeStep = Shader.PropertyToID("_addShadowRangeStep");
    //  多光源阴影软硬
    internal static readonly int addShadowFeather = Shader.PropertyToID("_addShadowFeather");
    //  使用前发投影
    internal static readonly int UseHairShadow = Shader.PropertyToID("_useHairShadow");
    //  使用方向溶解
    internal static readonly int useDissolveCut = Shader.PropertyToID("_UseDissolveCut");
    //  使用菲涅尔流光
    internal static readonly int fresnelFlowRange = Shader.PropertyToID("_FresnelFlowRange");
    //  使用额外菲涅尔
    internal static readonly int useAdditionFresnel = Shader.PropertyToID("_UseAdditionFresnel");
    //  流光菲涅尔范围
    internal static readonly int FresnelRange = Shader.PropertyToID("_FresnelRange");
    //  流光菲涅尔软硬
    internal static readonly int FresnelFeather = Shader.PropertyToID("_FresnelFeather");
    //  流光剔除
    internal static readonly int dissolveCut = Shader.PropertyToID("_DissolveCut");
    //  流光颜色
    internal static readonly int fresnelColor = Shader.PropertyToID("_FresnelColor");
    //  使用实时阴影
    internal static readonly int useRealTimeShadow = Shader.PropertyToID("_useRealTimeShadow");
    //  使用屏幕空间边缘光
    internal static readonly int useSSRimLight = Shader.PropertyToID("_UseSSRim");
    //  使用点阵化剔除
    internal static readonly int useTransparentMask = Shader.PropertyToID("_UseTransparentMask");
    //  白平衡kRGB
    internal static readonly int kRGB = Shader.PropertyToID("_kRGB");
    //  使用场景接触阴影
    internal static readonly int UseSenceShadow = Shader.PropertyToID("_UseSenceShadow");
    //  场景接触阴影贴图
    internal static readonly int SenceShadowMaskTexture = Shader.PropertyToID("_SenceShadowMaskTexture");
    //  场景阴影贴图offset
    internal static readonly int SenceShadowOffset = Shader.PropertyToID("_SenceShadowOffset");
    //  场景阴影贴图scale
    internal static readonly int SenceShadowScale = Shader.PropertyToID("_SenceShadowScale");
    //  使用角色闪光
    internal static readonly int UseRoleFlickerEffect = Shader.PropertyToID("_UseRoleFlickerEffect");
    //  使用冰冻效果
    internal static readonly int UseIce = Shader.PropertyToID("_UseIce");
    //  冰冻颜色
    internal static readonly int IceVFXColor = Shader.PropertyToID("_IceVFXColor");
    //  冰冻法线贴图
    internal static readonly int NormalMap = Shader.PropertyToID("_NormalMap");
    //  冰冻法线贴图强度
    internal static readonly int IceNormalScale = Shader.PropertyToID("_IceNormalScale");
    //  冰冻边缘光
    internal static readonly int IceRimColor = Shader.PropertyToID("_IceRimColor");
    //  冰冻菲涅尔软硬
    internal static readonly int IceFresnelFeather = Shader.PropertyToID("_Ice_Fresnel_Feather");    
    //  冰冻菲涅尔范围
    internal static readonly int IceFresnelStep = Shader.PropertyToID("_Ice_Fresnel_Step");   
    //  冰冻高光颜色
    internal static readonly int IceSpecColor = Shader.PropertyToID("_IceSpecColor"); 
    //  冰冻高光范围
    internal static readonly int IceSpecPower = Shader.PropertyToID("_IceSpecPower");
    //  使用脸部冰冻效果
    internal static readonly int UseFaceIceQuad = Shader.PropertyToID("_UseFaceIceQuad");
    //  脸部冰冻密度
    internal static readonly int IceQuadDensity = Shader.PropertyToID("_IceQuadDensity");
    //  脸部冰冻溶解强度
    internal static readonly int IceQuadDissolveScale = Shader.PropertyToID("_IceQuadDissolveScale");
    //  使用高度图
    internal static readonly int UseHeightMap = Shader.PropertyToID("_UseHeightMap");
    //  冰冻贴图
    internal static readonly int IceTex = Shader.PropertyToID("_IceTexture");
    //  噪声图
    internal static readonly int NoiseTex = Shader.PropertyToID("_NoiseTex");
    //  世界坐标溶解范围
    internal static readonly int PosWDissolveScale = Shader.PropertyToID("_PosWDissolveScale");
    //  世界坐标溶解宽度
    internal static readonly int PosWDissolveWidth = Shader.PropertyToID("_PosWDissolveWidth");
    //  世界坐标溶解遮罩范围
    internal static readonly int PosWMaskScale = Shader.PropertyToID("_PosWNoiseScale");
    //  世界坐标溶解颜色
    internal static readonly int PosWDissolveColor = Shader.PropertyToID("_PosWDissolveColor");
    //  世界坐标溶解使用翻转
    internal static readonly int UseTop2Bottom = Shader.PropertyToID("_Top2Bottom");
    //  溶解UV
    internal static readonly int DissolveUV = Shader.PropertyToID("_ScreenUV");
    //  使用HSV
    internal static readonly int UseHSV = Shader.PropertyToID("_UseHSV");
    //  色相
    internal static readonly int HUE = Shader.PropertyToID("_Hue");
    //  饱和度
    internal static readonly int Saturation = Shader.PropertyToID("_Saturation");
    //  明度
    internal static readonly int Value = Shader.PropertyToID("_Value");
    //  材质的stencil值
    internal static readonly int LitDirStencilRef = Shader.PropertyToID("_LitDirStencilRef");
    //  使用Unity雾
    internal static readonly int UseUnityFog = Shader.PropertyToID("_UseUnityFog");
    //  角色自发光
    internal static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    //  使用AlphaClip
    internal static readonly int UseAlphaClip = Shader.PropertyToID("_UseAlphaClip");
    //  溶解强度  --MAIN TEXTURE LERP
    internal static readonly int DissolveStrength = Shader.PropertyToID("_Dissolve_power1");
    //  亮边范围  --MAIN TEXTURE LERP
    internal static readonly int EdgeScale= Shader.PropertyToID("_Dissolve_power2");
    //  亮边颜色 --MAIN TEXTURE LERP
    internal static readonly int EdgeColorLerp= Shader.PropertyToID("_EdgeColor2");


    /// <summary>
    /// 消散溶解    ShaderID
    /// </summary>
    //  消散溶解颜色
    internal static readonly int DissolveColor = Shader.PropertyToID("_DissolveColor");
    //  消散溶解剔除
    internal static readonly int DissolveCut = Shader.PropertyToID("_DissolveCut");
    //  消散溶解范围
    internal static readonly int DissolveStep = Shader.PropertyToID("_DissolveStep");
    //  消散溶解软硬
    internal static readonly int DissolveFeather = Shader.PropertyToID("_DissolveFeather");
    //  消散溶解宽度
    internal static readonly int DissolveWidth= Shader.PropertyToID("_DissolveWidth");





    /// <summary>
    /// 消散溶解拉伸  ShaderID
    /// </summary>
    //  拉伸溶解颜色
    internal static readonly int StretchColor = Shader.PropertyToID("_StretchColor");
    //  拉伸溶解剔除
    internal static readonly int StretchCut = Shader.PropertyToID("_StretchCut");
    //  拉伸溶解噪声密度
    internal static readonly int StretchDensity = Shader.PropertyToID("_StretchDensity");
    //  拉伸溶解长度
    internal static readonly int StretchLength = Shader.PropertyToID("_StretchLength");
    //  拉伸溶解速度
    internal static readonly int StretchSpeed = Shader.PropertyToID("_StretchSpeed");
    //  拉伸溶解噪声图
    internal static readonly int StretchNoiseTex = Shader.PropertyToID("_NoiseTex");





    /// <summary>
    /// 角色选中描边后处理   ShaderID
    /// </summary>
    //  描边颜色
    internal static readonly int OutlineCol = Shader.PropertyToID("_OutlineCol");
    //  描边便宜
    internal static readonly int Offsets = Shader.PropertyToID("_offsets");
    //  描边模糊图
    internal static readonly int BlurTex = Shader.PropertyToID("_BlurTex");
    //  描边遮罩图
    internal static readonly int MaskTex = Shader.PropertyToID("_MaskTex");
    //  描边强度
    internal static readonly int OutlineStrength = Shader.PropertyToID("_OutlineStrength");





    /// <summary>
    /// Color Grading后处理    ShaderID
    /// </summary>
    //  曲线图
    internal static readonly int CurvesTex = Shader.PropertyToID("_CurvesTex");
    //  源图
    internal static readonly int source = Shader.PropertyToID("_source");





    /// <summary>
    /// UV偏移值   ShaderID
    /// </summary>
    //  眼睛贴图index
    internal static readonly int EyePicIndex = Shader.PropertyToID("_PicIndex"); 
    //  X偏移
    internal static readonly int UVOffsetX = Shader.PropertyToID("uv_x");
    //  Y偏移
    internal static readonly int UVOffsetY = Shader.PropertyToID("uv_y");





    /// <summary>
    /// 场景变黑    ShaderID
    /// </summary>
    //  场景变黑颜色
    internal static readonly int Color = Shader.PropertyToID("_Color");
    //  黑色遮罩
    internal static readonly int DarkMask = Shader.PropertyToID("_DarkMask");
    //  特效遮罩
    internal static readonly int EffectMask = Shader.PropertyToID("_EffectMask");





    /// <summary>
    /// 黑白闪参数   ShaderID
    /// </summary>
    //  主图
    internal static readonly int MainTex = Shader.PropertyToID("_MainTex");
    //  外部颜色
    internal static readonly int OutColor = Shader.PropertyToID("_OutColor");
    //  描边
    internal static readonly int Outline = Shader.PropertyToID("_Outline");
    //  暗部强度
    internal static readonly int DarkStrength = Shader.PropertyToID("_DarkStrength");
    //  亮部颜色
    internal static readonly int ColorLight = Shader.PropertyToID("_ColorLight");
    //  阴影颜色
    internal static readonly int ColorShadow = Shader.PropertyToID("_ColorShadow");
    //  黑白闪阴影范围
    internal static readonly int FlickerShadowRange = Shader.PropertyToID("_FlickerShadowRange");
    //  黑白闪菲涅尔范围
    internal static readonly int FlickerFresnelRange = Shader.PropertyToID("_FlickerFresnelRange");





    /// <summary>
    /// 实时投影    ShaderID
    /// </summary>
    //  世界转阴影空间矩阵
    internal static readonly int WorldToShadowMatrix = Shader.PropertyToID("BARD_SHADOW_MAP_VP");


    /// <summary>
    /// 管线配置文件 RendererFeatureIndex
    /// </summary>
    public enum RendererFeatureIndex
    {
        OutLine = 0,                            //外勾边
        RenderDistort = 1,                      //渲染扭曲
        TransparentOutLine = 2,                 //半透明描边
        Flicker = 3,                            //黑白闪
        RadiaBlur = 4,                          //径向模糊
        RGBSplit = 5,                           //RGB分离
        BehindShadow = 6,                       //遮挡阴影
        OutLinePost = 7,                        //羽化描边
        Test = 8,                               //测试
        GrabTexture = 9,                        //抓取屏幕
        RenderObjectToTarget = 10,              //渲染物体到RT
        CharacterStencil = 11,                  //渲染角色模板
        FrontShadowFeather = 12,                //角色前发投影
        ShaderDebugFeature = 13,                //shader Debug工具
        ImageMotionBlur = 14,                   //动态模糊
        ShineOutLine = 15,                      //Stencil外描边
        CharacterShadow = 16,                   //角色实时投影
        WinkVignette = 17,                      //眨眼后效
        GaussianBlur = 18,                      //高斯模糊
        GlobalFog = 19,                         //全局雾
        FadeToSky = 20,                         //渐变到天空球
        GrabScreenColor = 21,                   //抓取当前屏幕图片
        BardVignette = 22,                      //屏幕压暗
        RobertEdgeDetection = 23,               //屏幕描边
        OpaquePreZ = 24,                        //不透明PreZ
        WritePosW = 25,                         //写入世界坐标的Y
        HeightFog = 26,                         //高度雾
        RimBlur = 27,                           //边缘模糊
        VolumeFog = 28,                         //体积雾
        OverDraw = 29,                          //Overdraw
        TransparentPreZ=30,                     //半透明PreZ
        RingWave = 31,                          //屏幕水波纹
        AddRadialBlur = 32,                     //叠加径向模糊
        WaveJitter = 33,                        //波浪抖动
        SpeedLine = 34,                         //速度线
        BWFlash = 35,                           //黑白闪第三版
        SkyDark = 36,                           //天空变色
        DrawMiniMap = 37,                       //绘制小地图遮罩
        SSPR = 38,                              //屏幕空间平面反射
        BardAmbientOcclusion = 39,              //AO后处理
        ScreenMask = 40,                        //屏幕遮罩颜色
        MoveEffect = 41,                        //MoveEffect
        Glow = 42,                              //Glow    
        DrawMiniMapSecond = 43,                 //绘制小地图遮罩 02
        DelayForward = 44,                      //等待的正向渲染
        HeartEffect = 45,                       //心形后效
        ScanApperance = 46,                     //扫描显示
        DrawTransparentDepth = 47,              //单独写入半透明深度
        Spherize = 48,                          //球面化
        DepthOfField = 49,                      //景深
        Bokeh = 50,                             //散景模糊
        ImageOutLine = 51,                      //Image外勾边
    }
}

/// <summary>
/// 不透明渲染队列
/// </summary>
public enum BardRenderQueue
{
    character_body = 2001,                      //角色身体材质
    character_face = 2002,                      //角色脸部材质
    character_hair = 2003,                      //角色头发材质
    character_simple = 2004,                    //角色简单材质
    unlit = 2005,                               //无光照不透明
    
    lit_pbr = 1999,                             //场景不透明材质
    lit_plant = 1998,                           //场景树叶不透明
    
    
    lit_pbr_transparent = 3001,                 //场景透明材质
    lit_plant_transparent = 3002,               //植物透明材质
}

/// <summary>
/// 渲染分级枚举
/// </summary>
public enum RenderLevel
{
    Low = 0,                                    //省电        - 低
    Middle = 1,                                 //普通        - 中
    High = 2,                                   //高清        - 高
    Acme = 3,                                   //超清        - 极致
}

/// <summary>
/// 渲染全局配置
/// </summary>
public class GlobalConfig
{
    /// <summary>
    /// 字体路径
    /// </summary>
    internal static string BUMING = "Assets/GameShare/Font/BUMING.asset";
    internal static string FZLTHS = "Assets/GameShare/Font/FZLanTingHeiS-R-GB.asset";
    internal static string QinLiuTi = "Assets/GameShare/Font/QinLiuTi.asset";
    internal static string GBK = "Assets/GameShare/Font/FZLanTingHei-R-GBK.asset";
    
    /// <summary>
    /// 编队界面白平衡配置
    /// </summary>
    //  森林白天 kRGB
    public static Vector3 ForestDay_kRGB = new Vector3(0.9564983f,1.020486f,1.008612f);   
    //  城镇白天kRGB
    public static Vector3 TownDay_kRGB = new Vector3(0.9973374f,1.005095f,0.9809452f);   
    //  城镇夜晚kRGB
    public static Vector3 TownNight_kRGB = new Vector3(0.8377715f,1.029283f,1.274668f);
    //  教堂白天kRGB
    public static Vector3 Auditorium_kRGB = new Vector3(1.058946f,0.9805557f,0.9455279f);
    
    //  编队界面白天kRGB
    public static Vector3 ShowSence_Morning = new Vector3(0.9969863f,1.001074f,1.002363f);
    //  编队界面中午kRGB
    public static Vector3 ShowSence_Noon = new Vector3(1.184019f, 0.9666752f, 0.9666752f);
    //  编队界面夜晚kRGB
    public static Vector3 ShowSence_Evening = new Vector3(0.7995162f, 1.044331f, 1.297671f);
    //  编队界面傍晚kRGB
    public static Vector3 ShowSence_Dusk = new Vector3(1.189398f, 0.9191387f, 0.9193838f);

    /// <summary>
    /// 接触阴影配置
    /// </summary>
    //  城镇白天ST
    public static Vector2 TownDayOffset = new Vector2(34.1f,-13.5f);
    public static float TownDayScale = 31;

    //  森林白天ST
    public static Vector2 ForestDayOffset = new Vector2(-20.4f, -25.4f);
    public static float ForestDayScale = 51.5f;

    //  城镇夜晚ST
    public static Vector2 TownNightOffset = new Vector2(-96.7f, 98.4f);
    public static float TownNightScale = 200;

    //  主城ST
    public static Vector2 MainCityOffset = new Vector2(41.35f, -8.1f);
    public static float MainCityScale = 102.5f;

    //  迷宫ST
    public static Vector2 MiGongOffset = new Vector2(56.6f, 26.4f);
    public static float MiGongScale = 178;
    
    /// <summary>
    /// 分级配置 参数值
    /// </summary>
    /// 
    public static bool useSSRimLight = true;                                                //屏幕空间边缘光 使用与否
    public static bool useTransparentMask = true;                                           //角色点阵化剔除 使用与否
    public static bool useRealTimeReflection = true;                                        //实时反射  使用与否
    public static bool useRealTimeShadow = true;                                            //是否使用实时阴影
    public static bool useSdfAO = false;                                                    //使用SDF AO
    public static bool useAdditionalLight = true;                                           //角色是否使用多光源
    public static bool useCharacterHSV = true;                                              //角色是否使用HSV
    public static bool useVolumeFog = true;                                                 //是否使用体积雾
    public static bool useBloom = true;                                                     //是否使用Bloom

    public static int CharacterShadowIndex = 1;                                             //0：圆片  1：平面阴影
    public static float RadialBlurCount = 6;                                                //径向模糊参数
    public static Vector3 GaussianBlurParames = new Vector3(2, 3, 2);                 //高斯模糊参数
    public static int TargetFrame = 30;                                                     //锁帧45FPS
    public static int LimitFrame = 25;                                                      //高帧率上限
    public static int AcmeLimitFrame = 30;                                                  //大于13的高帧率上限
    
    public static float DieDissolveWidth = 0.35f;                                           //死亡溶解宽度

    //  当前分级
    public static RenderLevel CurrentRenderLevel = RenderLevel.High;                        //  当前分级
    public static RenderLevel PostProcessLevel = RenderLevel.High;                          //  当前后效分级
}

/// <summary>
/// 敌人表情    枚举
/// </summary>
internal enum EnemyExpression
{
    Default = 0,                                                                            //默认
    Attack = 1,                                                                             //攻击
    Dead = 2,                                                                               //死亡
    Play = 3,                                                                               //玩耍
    Hit = 4,                                                                                //受击
}

namespace Bard
{
    public class LightingData
    {
        /// <summary>
        /// Environment
        /// </summary>
        #region 环境部分
        public string skyboxPath;                                                           //天空盒路径
        public string SunSource;                                                            //主光源
    
        public int SourceIndex; //0:skyBox 1:Gradient 2:Color
    
        public float IntensityMultiplie;
    
        public Vector3 Color;
        
        public Vector3 skyColor;                                                            //天空颜色
        public Vector3 EquatorColor;                                                        //赤道颜色
        public Vector3 GroundColor;                                                         //地面颜色

        public string reflectSourceName;                                                    //反射源名称
        public float Resolution;                                                            //质量

        public float RefIntensityMultiplier;                                                //反射强度
        public int EnvBounces;                                                              //Bounces次数
        #endregion
    

        /// <summary>
        /// OtherSettings
        /// </summary>
        #region 其他设置
        public bool Fog;                                                                    //启用雾
        public Vector3 color;                                                               //雾颜色
        public string Mode;                                                                 //雾模式
        public float Start;                                                                 //距离雾Start
        public float End;                                                                   //距离雾End
        public float Density;                                                               //距离雾密度
        #endregion

    }
}

/// <summary>
/// 模糊数据
/// </summary>
public class BlurData
{
    public RenderTexture Texture;                                                           //模糊图
    public int Index;                                                                       //模糊图的Index
}