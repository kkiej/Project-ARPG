using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Bard;
//using DG.Tweening;
//using Newtonsoft.Json;
using TMPro;
using Unity.Mathematics;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Sprites;

public class ShaderUtils : ShaderFunctions
{
    /// <summary>
    /// Global Params
    /// </summary>
    public static float ShadowInvLen = 0.25f;                                                                           //角色平面阴影衰减
    public static Color PlanerShadowColor = Color.black;                                                                //角色平面阴影颜色

    public static bool CurrentSkyBoxState = false;                                                                      //当前天空盒状态
    public static float CurrentSkyBoxExposure = 1.92f;                                                                  //当前天空盒曝光度    
                                
    public static float showSceneOutLine = 0.4f;                                                                        //展示界面外勾边宽度
    public static float commonOutLine = 0.4f;                                                                           //普通状态外勾边宽度
    public static float EnemyOutLine = 0.8f;                                                                            //敌人外勾边宽度
    public static float HumanEnemyOutLine = 0.5f;                                                                       //人形敌人外勾边
    public static float ShineOutLine = 0.4f;                                                                            //发光外勾边宽度
    public static float SSRimWidth = 0.4f;                                                                              //SS轮廓光宽度
    public static float SimpleRimWidth = 0.714f;                                                                        //简单轮廓光宽度
                                
    public static int grabTexCount = 0;                                                                                 //grabTex计数器
    public static int grabPassBlurCount = 0;                                                                            //抓取模糊的计数器
    public static UniversalRendererData GlobalPostprocessingData;                                                         //Feature Data
                                    
    public static UniversalRenderPipelineAsset GlobalAsset;                                                             //管线资产
    //public static CameraRenderLevelData GlobalCameraRenderLevelData;                                                    //相机渲染分级Data
    public static VolumeProfile GlobalProcessVolume;                                                                    //后处理Profile
    
    public static int intervalFrame = 5;                                                                                //刷新帧率
                            
    public static string CharacterLight = "CharacterLight";                                                             //角色灯光
    public static Vector3 defaultTransform = new Vector3(10,30,15);                                               //角色灯光Pos
    public static Vector3 defaultEulerAngles = new Vector3(45, -135, 0);                                          //角色灯光Rot

    public static LinkedList<BlurData> GrabScreenTextureList = new LinkedList<BlurData>();                              //抓取当前屏幕图片链表
    public static bool isTraversalCam = false;                                                                          //是否遍历完所有相机的标志
    
    public static Texture2D globalShadowTex = null;                                                                     //场景接触阴影
    
    /// <summary>
    /// 获取RenderData
    /// </summary>
    /// <returns></returns>
    public static UniversalRendererData GetGlobalPostprocessingData()
    {
        if (GlobalPostprocessingData != null)
            return GlobalPostprocessingData;
        else
        {
            return Resources.Load<UniversalRendererData>(ShaderIDs.urpAssetData);
        }
    }

    /// <summary>
    /// 获取URP资产
    /// </summary>
    /// <returns></returns>
    public static UniversalRenderPipelineAsset GetGlobalAsset()
    {
        if (GlobalAsset != null)
            return GlobalAsset;
        else
        {
            return Resources.Load<UniversalRenderPipelineAsset>(ShaderIDs.urpAssetPath);
        }
    }

    /// <summary>
    /// 获取相机渲染分级Data
    /// </summary>
    /// <returns></returns>
    //public static CameraRenderLevelData GetGlobaCameraRenderLevelData()
    //{
    //    if (GlobalCameraRenderLevelData != null)
    //        return GlobalCameraRenderLevelData;
    //    else
    //    {
    //        return Resources.Load<CameraRenderLevelData>(ShaderIDs.cameraRenderData);
    //    }
    //}
    
    /// <summary>
    /// 获取后处理Data
    /// </summary>
    /// <returns></returns>
    public static VolumeProfile GetGlobaVolumeProfile()
    {
        
        if (GlobalProcessVolume == null)
        {
            GlobalProcessVolume = Resources.Load<VolumeProfile>(ShaderIDs.postProcessVolume);
        }
        return GlobalProcessVolume;
    }
    
    /// <summary>
    /// 初始化URP资产
    /// </summary>
    /// <param name="assetPath"></param>
    public static void InitGlobalAsset(string assetPath)
    {
        GlobalAsset = Resources.Load<UniversalRenderPipelineAsset>(assetPath);
    }
    
    /// <summary>
    /// 初始化ForwardRendererData
    /// </summary>
    public static void InitForwardRendererData(string assetPath)
    {
        GlobalPostprocessingData = Resources.Load<UniversalRendererData>(assetPath);
        InitRendererFeature();
    }

    /// <summary>
    /// 初始化相机渲染分级Data
    /// </summary>
    /// <param name="assetPath"></param>
    //public static void InitGlobalCameraRenderLevelData(string assetPath)
    //{
    //    GlobalCameraRenderLevelData = Resources.Load<CameraRenderLevelData>(assetPath);
    //}
    
    /// <summary>
    /// Shader加载
    /// </summary>
    public static async Task<Shader> LoadShaderAtPath(string path)
    {
        #if UNITY_EDITOR  
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(path);
#elif UNITY_STANDALONE_OSX
            return await AddressablesManager.LoadAssetsAsync<Shader>(path);
#elif UNITY_STANDALONE_WIN
            return await AddressablesManager.LoadAssetsAsync<Shader>(path); 
#elif UNITY_IPHONE
            return await AddressablesManager.LoadAssetsAsync<Shader>(path); 
#elif UNITY_ANDROID
            return await AddressablesManager.LoadAssetsAsync<Shader>(path); 
#endif
    }

    /// <summary>
    /// Shader加载
    /// </summary>
    public static async Task<ComputeShader> LoadComputeShaderAtPath(string path)
    {
#if UNITY_EDITOR  
        return UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>(path);
#else
        return await AddressablesManager.LoadAssetsAsync<ComputeShader>(path); 
#endif
    }
    
    /// <summary>
    /// 设置角色渲染队列值
    /// </summary>
    /// <param name="meshRenderers"></param>
    /// <param name="QueueNumber"></param>
    public static void SetCharacterRendererQueue(GameObject character,int QueueNumber)
    {
        MeshRenderer[] meshRenderers = character.GetComponentsInChildren<MeshRenderer>();
        SkinnedMeshRenderer[] skinnedMeshRenderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            foreach (MeshRenderer child in meshRenderers)
            {
                child.sharedMaterial.renderQueue = QueueNumber;
            }
        }

        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            foreach (SkinnedMeshRenderer child in skinnedMeshRenderers)
            {
                child.sharedMaterial.renderQueue = QueueNumber;
            }
        }
    }
    
    /// <summary>
    /// 设置角色黑白闪烁特效开启状态 True打开 / False关闭
    /// </summary>
    /// <param name="materials"></param>
    public static void SetFlickEffect(GameObject character,bool state)
    {
        MeshRenderer[] meshRenderers = character.GetComponentsInChildren<MeshRenderer>();
        SkinnedMeshRenderer[] skinnedMeshRenderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();

        for (int i = 0;i < meshRenderers.Length; i++)
        {
            foreach(MeshRenderer child in meshRenderers)
            { 
                if(state)
                {
                    child.sharedMaterial.EnableKeyword("BLACK_WHITE_ON");
                }
                else
                {
                    child.sharedMaterial.DisableKeyword("BLACK_WHITE_ON");
                }
            }
        }

        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            foreach (SkinnedMeshRenderer child in skinnedMeshRenderers)
            {
                if (state)
                {
                    child.sharedMaterial.EnableKeyword("BLACK_WHITE_ON");
                }
                else
                {
                    child.sharedMaterial.DisableKeyword("BLACK_WHITE_ON");
                }
            }
        }
    }
    
    /// <summary>
    /// 设置角色环境灯光  1.角色传入  2.是否开启叠加色模式  3.传入环境光  4.传入环境光强度（默认是 1）
    /// </summary>
    /// <param name="character"></param>
    /// <param name="useAddColor"></param>
    /// <param name="EnvColor"></param>
    /// <param name="EnvStrength"></param>
    public static void CharacterEnvColorSet(GameObject character, bool useAddColor, Color EnvColor, float EnvStrength = 1)
    {
        RoleShaderManager roleShaderManager = character.GetComponentInChildren<RoleShaderManager>();
        
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.UseAddColor = useAddColor;
        roleShaderManager.LightColor = EnvColor;
        roleShaderManager.LightStrength = EnvStrength;
    }
    
    public static void CharacterEnvColorSet(GameObject character, bool useAddColor, Color EnvColor, float EnvStrength, Color addColor)
    {
        RoleShaderManager roleShaderManager = character.GetComponentInChildren<RoleShaderManager>();
        
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.UseAddColor = useAddColor;
        roleShaderManager.LightColor = EnvColor;
        roleShaderManager.LightStrength = EnvStrength;
        roleShaderManager.AddColor = addColor;
    }

    /// <summary>
    /// 设置角色相机灯光开启状态  True打开 / False关闭
    /// </summary>
    /// <param name="character"></param>
    /// <param name="state"></param>
    public static void CharacterCameraLightSwitch(GameObject character, bool state)
    {
        RoleShaderManager roleShaderManager = character.GetComponentInChildren<RoleShaderManager>();
        
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.useCameraLight = state;
    }

    public static void CharacterCameraLightSwitch(RoleShaderManager roleShaderManager, bool state)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.useCameraLight = state;
    }
    
    /// <summary>
    /// 设置角色平面阴影颜色
    /// </summary>
    /// <param name="character"></param>
    /// <param name="shadowColor"></param>
    /// <param name="ShadowInvLen"></param>
    public static void SetCharacterShadow(Color shadowColor, float ShadowInvLen = 0.25f)
    {
        ShaderUtils.PlanerShadowColor = shadowColor;
        ShaderUtils.ShadowInvLen = ShadowInvLen;
    }

    /// <summary>
    /// 打开角色溶解
    /// </summary>
    public static void SetCharacterDissolveState(RoleShaderManager roleShaderManager, bool state)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.UseDossolve = state;
        roleShaderManager.SetCharacterNoiseTexture();
    }

    /// <summary>
    /// 设置角色溶解特效  溶解强度
    /// </summary>
    public static void SetCharacterDissolve(RoleShaderManager roleShaderManager, float DissolveScale)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.dissolveScale = DissolveScale;
    }

    /// <summary>
    /// 角色溶解颜色
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="color"></param>
    public static void SetCharacterDissolveColor(RoleShaderManager roleShaderManager, Color color)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.edgeColor = color;
    }
    
    /// <summary>
    /// 设置角色溶解特效  灯光强度
    /// </summary>
    public static void SetCharacterLight(RoleShaderManager roleShaderManager, float LightStrength)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.LightStrength = LightStrength;
    }

    /// <summary>
    /// 设置角色平面阴影显示
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="state"></param>
    public static void SetCharacterShadowShowState(RoleShaderManager roleShaderManager, bool state)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.useShadow = state;
    }

    //---------------------获取到遮挡的敌人列表------------------
    /// <summary>
    /// 获取到遮挡的敌人列表
    /// </summary>
    /// <param name="EnemiesList"></param>
    /// <param name="mainCamera"></param>
    /// <returns></returns>
    //public static List<GameObject> GetOcclusionEnemiesList(List<GameObject> EnemiesList, Camera mainCamera)
    //{
    //    return EnemyOcclusion.GetOcclusionEnemyList(EnemiesList, mainCamera);
    //}

    /// <summary>
    /// 设置小猪的表情  index = 0--->默认表情   /    index = 1--->  (>  -  <)   长这样 我也不知道怎么形容    /   index = 2--->  (X  -  X)    长这样 我也不知道怎么形容    /   index = 3--->  (-  。  -)
    /// </summary>
    /// <param name="index"></param>
    public static void LittlePigExpressionSet(GameObject littlePig, int index)
    {
        if (littlePig == null)
            return;

        Renderer[] Renderers = littlePig.GetComponentsInChildren<Renderer>();
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        for (int i = 0; i < Renderers.Length; i++)
        {
            if (Renderers[i].gameObject.name.IndexOf("Eye", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Renderers[i].GetPropertyBlock(propertyBlock);
                switch (index)
                {
                    case 0: propertyBlock.SetFloat(ShaderIDs.UVOffsetX, 0); propertyBlock.SetFloat(ShaderIDs.UVOffsetY, 0); break;
                    case 1: propertyBlock.SetFloat(ShaderIDs.UVOffsetX, 0.5f); propertyBlock.SetFloat(ShaderIDs.UVOffsetY, 0); break;
                    case 2: propertyBlock.SetFloat(ShaderIDs.UVOffsetX, 0); propertyBlock.SetFloat(ShaderIDs.UVOffsetY, 0.5f); break;
                    case 3: propertyBlock.SetFloat(ShaderIDs.UVOffsetX, 0.5f); propertyBlock.SetFloat(ShaderIDs.UVOffsetY, 0.5f); break;
                    case 4: propertyBlock.SetFloat(ShaderIDs.UVOffsetX, 0.5f); propertyBlock.SetFloat(ShaderIDs.UVOffsetY, 0f); break;
                }
                Renderers[i].SetPropertyBlock(propertyBlock);
            }
        }
    }

    /// <summary>
    /// 设置敌人表情
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="index"></param>
    public static void SetEnemyExpression(RoleShaderManager roleShaderManager,int index)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetEnemyEye(index);
    }

    /// <summary>
    /// 给添加相机径向模糊
    /// </summary>
    /// <param name="mainCamera"></param>
    //public static void AddCameraRadblur(GameObject mainCamera)
    //{
    //    mainCamera.AddComponent<RadialBlur>();
    //}

    /// <summary>
    /// 控制Radblur开启关闭
    /// </summary>
    /// <param name="radialBlur"></param>
   //public static void SetCameraRadblurState(RadialBlur radialBlur, bool state)
   //{
   //    radialBlur.enabled = state;
   //}

    /// <summary>
    /// 设置径向模糊参数
    /// </summary>
    /// <param name="radialBlur"></param>
    /// <param name="BlurFactor"></param>
    /// <param name="BlurCount"></param>
    //public static void SetCameraRadblurValue(RadialBlur radialBlur,float BlurFactor, float BlurCount)
    //{
    //    radialBlur.blurCenter = new Vector2(0.5f, 0.5f);
    //    radialBlur.blurFactor = BlurFactor;
    //    radialBlur.blurCount = BlurCount;
    //}

    /// <summary>
    /// 设置GetDark效果
    /// </summary>
    /// <param name="getDark"></param>
    /// <param name="NotDarkArray"></param>
    /// <param name="color"></param>
    //public static void SetDarkEffect(GetDark getDark, GameObject[] NotDarkArray, GameObject[] EffectArray, Color color)
    //{
    //    getDark.NotDarkObjs = NotDarkArray;
    //    getDark.EffectObjs = EffectArray;
    //    getDark.SetColor = color;
    //}
    
    /// <summary>
    /// 设置无光照材质颜色 
    /// </summary>
    /// <param name="color"></param>
    public static void SetUlintColor(Color color)
    {
        Shader.SetGlobalColor(ShaderIDs.UlintColor,color);
    }

    /// <summary>
    /// 还原无光照材质颜色 
    /// </summary>
    /// <param name="color"></param>
    public static void ResetUlintColor()
    {
        Shader.SetGlobalColor(ShaderIDs.UlintColor,Color.white);
    }
    
    /// <summary>
    /// 设置天空球材质颜色
    /// </summary>
    /// <param name="color"></param>
    public static void SetSkyBoxExposure(Material skyboxMt, float exposure)
    {
        if (!CurrentSkyBoxState)
        {
            CurrentSkyBoxExposure = exposure;
            CurrentSkyBoxState = true;
        }
        skyboxMt.SetFloat("_Exposure",exposure);
    }
    
    /// <summary>
    /// 还原天空球材质颜色
    /// </summary>
    /// <param name="color"></param>
    public static void ResetSkyBoxExposure(Material skyboxMt)
    {
        if (CurrentSkyBoxState)
        {
            CurrentSkyBoxState = false;
        }
        skyboxMt.SetFloat("_Exposure",CurrentSkyBoxExposure);
    }
    
    /// <summary>
    /// 设置角色刷新次数
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="FrameCount"></param>
    public static void SetCharacterUpdateFrameConut(int frameCount)
    {
        intervalFrame = frameCount;
    }

    /// <summary>
    /// 设置角色分身透明度
    /// </summary>
    /// <param name="character"></param>
    /// <param name="Alpha"></param>
    public static void SetCharacterGhostAlpha(GameObject character, float Alpha)
    {
        Renderer[] Renderers = character.GetComponentsInChildren<Renderer>();

        for (int i = 0;i < Renderers.Length; i++)
        {
            foreach(Renderer child in Renderers)
            {
                child.sharedMaterial.SetFloat("_Alpha", Alpha);
            }
        }
    }

    /// <summary>
    /// 设置UI颜色变白
    /// </summary>
    /// <param name="image"></param>
    /// <param name="addStrength"></param>
    public static void SetUIAddColor(Image image, float addStrength)
    {
        image.material.SetFloat("_AddSlider", addStrength);
    }

    /// <summary>
    /// 设置角色阴影开启和关闭
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="state"></param>
    public static void SetCharacterShadowState(RoleShaderManager roleShaderManager, bool state)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.useShadow = state;
    }
    
    /// <summary>
    /// 设置角色外勾边
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="state"></param>
    public static void SetCharacterOutLineShowScene(RoleShaderManager roleShaderManager, bool state)
    {
        if(roleShaderManager == null)
            return;
        
        if (state)
            roleShaderManager.isShowScene = true;
        else
            roleShaderManager.isShowScene = false;
    }

    /// <summary>
    /// 使用角色多光源
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void OpenCharacterMultiLight(RoleShaderManager roleShaderManager,bool state)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.UseAdditionLight = state;
    }

    /// <summary>
    /// 使用角色冰冻效果
    /// </summary>
    public static void SetCharacterIce(RoleShaderManager roleShaderManager ,bool state)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.useIceEffect = state;
        if (state)
        {
            Texture2D iceTex = Resources.Load<Texture2D>(ShaderIDs.IceTexPath);
            Shader.SetGlobalTexture(ShaderIDs.IceTex,iceTex);
            Texture2D normal = Resources.Load<Texture2D>(ShaderIDs.IceNormPath);
            Shader.SetGlobalTexture(ShaderIDs.NormalMap,normal);
        }
    }
    
    /// <summary>
    /// 打开角色前发投影
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="state"></param>
    public static void OpenCharacterHairShadow(RoleShaderManager roleShaderManager,bool state)
    {
        if(roleShaderManager == null)
            return;
        
        if (ShaderUtils.GlobalPostprocessingData)
        {
            //FrontShadowFeature feature =
            //    (FrontShadowFeature)ShaderUtils.GlobalPostprocessingData.rendererFeatures[
            //        (int) ShaderIDs.RendererFeatureIndex.FrontShadowFeather];
            //if (state)
            //{
            //    feature.SetActive(true);
            //    roleShaderManager.useHairShadow = true;
            //}
            //else
            //{
            //    feature.SetActive(false);
            //    roleShaderManager.useHairShadow = false;
            //}
        }
    }

    /// <summary>
    /// 设置角色受光强度
    /// </summary>
    /// <param name="RoleShaderManager"></param>
    /// <param name="strength"></param>
    public static void SetCharacterDark(RoleShaderManager RoleShaderManager,float strength)
    {
        if(RoleShaderManager == null)
            return;
        
        RoleShaderManager.LightStrength = strength;
    }

    /// <summary>
    /// 角色RoleShaderManager初始化
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void InitRoleShaderManager(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        //使用叠加颜色 不使用  颜色为白色  强度为1
        roleShaderManager.LightStrength = 1;
        roleShaderManager.LightColor = Color.white;
        roleShaderManager.UseAddColor = false;
        roleShaderManager.AddColor = Color.black;;
        //角色是否在ShowScene
        roleShaderManager.isShowScene = false;
        //多光源 不使用
        roleShaderManager.UseAdditionLight = false;
        roleShaderManager.addShadowFeather = 0.3f;
        roleShaderManager.addShadowRangeStep = 0.5f;
        //相机灯光不开启
        roleShaderManager.useCameraLight = false;
        //不使用角色前发投影
        roleShaderManager.useHairShadow = false;
        //使用阴影
        roleShaderManager.useShadow = true;
        //不使用溶解
        roleShaderManager.UseDossolve = false;
        //关闭黑白闪
        roleShaderManager.SetBlackWhiteFlaskOff();
        //关闭点阵化半透明
        roleShaderManager.PerCharacterMaskTransparentState = false;
        //关闭冰冻效果
        roleShaderManager.useIceEffect = false;
        //关闭流光剔除
        roleShaderManager.UsePosWDissolve = false;
        //关闭额外菲涅尔
        roleShaderManager.UseAdditionFresnel = false;
        //关闭冰冻
        roleShaderManager.useIceEffect = false;
        //关闭脸部冰冻
        roleShaderManager.useFaceIceQuad = false;
        //关闭角色环境光
        roleShaderManager.useEnvironmentRimLight = false;
        //开启Unity雾
        roleShaderManager.useUnityFog = true;
        //设置角色外勾边正常模式
        roleShaderManager.SetRoleOutLineCommon(out roleShaderManager.isSelect);
    }
    
    /// <summary>
    /// 获取方块旋转矩阵
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static float3x3 GetQuadRotatMatrix(GameObject obj, Camera mainCamera)
    {
        Vector3 faceDirection = new Vector3(obj.transform.forward.x,0,obj.transform.forward.z);
        Vector3 cameraDirection = -new Vector3(mainCamera.transform.forward.x,0, mainCamera.transform.forward.z);
            
        float angle = Vector3.Angle (faceDirection, cameraDirection); //求出两向量之间的夹角 
        Vector3 normal = Vector3.Cross (faceDirection,cameraDirection);//叉乘求出法线向量 
        angle *= -Mathf.Sign (Vector3.Dot(normal,obj.transform.up));  //求法线向量与物体上方向向量点乘，结果为1或-1，修正旋转方向 
        angle = angle / 60;
        float3x3 QuadRotatMatrix = new float3x3(
            math.cos(angle),0,-math.sin(angle),
            0,1,0,
            math.sin(angle),0,math.cos(angle)
        );
        return QuadRotatMatrix;
    }

    /// <summary>
    /// 给方块旋转一哈
    /// </summary>
    /// <param name="QuadRotatMatrix"></param>
    /// <param name="pos"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static Vector3 RotatQuad(float3x3 QuadRotatMatrix,Vector3 pos,Vector3 offset)
    {
        float4x4 MoveToZero = new float4x4(
            1,0,0,-pos.x,
            0,1,0,-pos.y,
            0,0,1,-pos.z,
            0,0,0,1
        );
        float4x4 MoveToStart = new float4x4(
            1,0,0,pos.x,
            0,1,0,pos.y,
            0,0,1,pos.z,
            0,0,0,1
        );
        float4 tempPos = math.mul(MoveToZero,new float4(pos,1));
        tempPos.xyz = tempPos.xyz + new float3(offset);
        tempPos.xyz = math.mul(QuadRotatMatrix, tempPos.xyz);
        tempPos = math.mul(MoveToStart, tempPos);
        return tempPos.xyz + new float3(0, 0.85f, 0);
    }

    /// <summary>
    /// 旋转相机灯光
    /// </summary>   
    public static Vector3 RotatCameraLight(float3x3 QuadRotatMatrix,Vector3 pos,Vector3 offset)
    {
        float4x4 MoveToZero = new float4x4(
            1,0,0,-pos.x,
            0,1,0,-pos.y,
            0,0,1,-pos.z,
            0,0,0,1
        );
        float4x4 MoveToStart = new float4x4(
            1,0,0,pos.x,
            0,1,0,pos.y,
            0,0,1,pos.z,
            0,0,0,1
        );
        float4 tempPos = math.mul(MoveToZero,new float4(pos,1));
        tempPos.xyz = tempPos.xyz + new float3(offset);
        tempPos.xyz = math.mul(QuadRotatMatrix, tempPos.xyz);
        tempPos = math.mul(MoveToStart, tempPos);
        return tempPos.xyz;
    }

    /// <summary>
    /// 绘制Debug方块
    /// </summary>
    public static Mesh InitGridMesh(Mesh QuadMesh, GameObject roleLight, Camera camera)
    {
        var vertices = new List<Vector3>();
        var indices = new List<int>();
        
        float3x3 QuadRotatMatrix = GetQuadRotatMatrix(roleLight, camera);
        Vector3 centerPos = roleLight.transform.position;
        
        Vector3 X1 = RotatQuad(QuadRotatMatrix, centerPos, new Vector3(-0.3f, 0.3f, 0));
        
        indices.Add(vertices.Count);
        vertices.Add(X1);
        Vector3 X2 = RotatQuad(QuadRotatMatrix, centerPos, new Vector3(0.3f, 0.3f, 0));
        
        indices.Add(vertices.Count);
        vertices.Add(X2);
        Vector3 X3 = RotatQuad(QuadRotatMatrix, centerPos, new Vector3(0.3f, -0.3f, 0));
        
        indices.Add(vertices.Count);
        vertices.Add(X3);
        Vector3 X4 = RotatQuad(QuadRotatMatrix, centerPos, new Vector3(-0.3f, -0.3f, 0));
        
        indices.Add(vertices.Count);
        vertices.Add(X4);
        
        Mesh newMesh = new Mesh();
        newMesh.hideFlags = HideFlags.DontSave;
        newMesh.SetVertices(vertices);
        newMesh.SetNormals(vertices);
        newMesh.SetIndices(indices.ToArray(), MeshTopology.Quads, 0);
        newMesh.UploadMeshData(true);
        return newMesh;
    }

    /// <summary>
    /// 设置材质图集UV
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="tex_key_word"></param>
    /// <param name="material"></param>
    public static void SetMaterialAtlasUV(Sprite sprite, Material material, string tex_key_word = "_MainTex")
    {
        Vector4 uv = DataUtility.GetOuterUV(sprite);
        Vector2 Tilling = new Vector2(uv[2] - uv[0], uv[3] - uv[1]);
        Vector2 Offset = new Vector2(uv[0], uv[1]);
        material.SetTextureScale(tex_key_word, Tilling);
        material.SetTextureOffset(tex_key_word, Offset);
    }
    
    /// <summary>
    /// 设置发光外勾边开启
    /// </summary>
    public static void SetStencilOutLineON()
    {
        if (GlobalPostprocessingData != null)
        {
            GlobalPostprocessingData.rendererFeatures[(int) ShaderIDs.RendererFeatureIndex.CharacterStencil].SetActive(true);
            GlobalPostprocessingData.rendererFeatures[(int) ShaderIDs.RendererFeatureIndex.ShineOutLine].SetActive(true);
        }
    }
    
    /// <summary>
    /// 设置发光外勾边关闭
    /// </summary>
    public static void SetStencilOutLineOFF()
    {
        if (GlobalPostprocessingData != null)
        {
            GlobalPostprocessingData.rendererFeatures[(int) ShaderIDs.RendererFeatureIndex.CharacterStencil].SetActive(false);
            GlobalPostprocessingData.rendererFeatures[(int) ShaderIDs.RendererFeatureIndex.ShineOutLine].SetActive(false);
        }
    }

    /// <summary>
    /// 角色平面阴影开启关闭
    /// </summary>
    /// <param name="RoleShaderManager"></param>
    /// <param name="state"></param>
    public static void SetCharacterSahdow(RoleShaderManager RoleShaderManager, bool state)
    {
        if(RoleShaderManager == null)
            return;
        
        RoleShaderManager.useShadow = state;
    }

    /// <summary>
    /// 角色Unity阴影开启状态
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="state"></param>
    public static void SetCharacterUnityShadowState(RoleShaderManager roleShaderManager, bool state)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.useShadow = state;
    }
    
    /// <summary>
    /// 初始化grabTexCount
    /// </summary>
    public static void InitGabTexCount()
    {
        grabTexCount = 0;
    }
    
    /// <summary>
    /// grabTexCount增加
    /// </summary>
    public static void AddGrabTexCount()
    {
        grabTexCount++;
    }

    /// <summary>
    /// grabTexCount减少
    /// </summary>
    public static void RemoveGrabTexCount()
    {
        grabTexCount--;
        if (grabTexCount < 0)
            grabTexCount = 0;
    }

    /// <summary>
    /// 初始化grabPassCount
    /// </summary>
    public static void InitGrabPassCount()
    {
        grabPassBlurCount = 0;
    }
    
    /// <summary>
    /// grabTexCount增加
    /// </summary>
    public static void AddGrabPassCount()
    {
        grabPassBlurCount++;
    }

    /// <summary>
    /// grabTexCount减少
    /// </summary>
    public static void RemoveGrabPassCount()
    {
        grabPassBlurCount--;
        if (grabPassBlurCount < 0)
            grabPassBlurCount = 0;
    }
    
    /// <summary>
    /// 角色武器流动出现
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="useWeaponShine"></param>
    /// <param name="showObjectScale"></param>
    /// <param name="showRimScale"></param>
    /// <param name="rimColor"></param>
    public static void SetCharacterWeaponShine(RoleShaderManager roleShaderManager, bool useWeaponShine, float showObjectScale, float showRimScale, float dissolveWdith, Color edgeColor, Color rimColor)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetWeaponShine(useWeaponShine, showRimScale, showObjectScale, rimColor, dissolveWdith, edgeColor);
    }

    /// <summary>
    /// 设置黑夜
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void SetCharacterInNight()
    {
        Shader.EnableKeyword("NIGHT_ON");
    }
    /// <summary>
    /// 设置白天
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void SetCharacterInDay()
    {
        Shader.DisableKeyword("NIGHT_ON");
    }

    /// <summary>
    /// 设置打开蓝色外勾边
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void SetCharacterSelectON(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetRoleSelectON(out roleShaderManager.isSelect);
    }

    /// <summary>
    /// 设置关闭蓝色外勾边
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void SetCharacterSelectOFF(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetRoleSelectOFF(out roleShaderManager.isSelect);
    }

    /// <summary>
    /// 设置颜色压暗
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="mask"></param>
    /// <param name="color"></param>
    public static void SetCharacterLightColor(RoleShaderManager roleShaderManager, Color color)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetLightColor(out roleShaderManager.LightColor,color);
    }

    /// <summary>
    /// 根据环境设置kRGB
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="kRGB"></param>
    public static void SetCharacterEnvironmentColor(RoleShaderManager roleShaderManager,Vector3 kRGB)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.UseWhiteBalance = true;
        Shader.SetGlobalVector(ShaderIDs.kRGB,kRGB);
    }

    /// <summary>
    /// 設置白平衡打開
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void SetCharacterWhiteBalanceStateON(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.UseWhiteBalance = true;
    }

    public static void SetCharacterWhiteBalanceStateOFF(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.UseWhiteBalance = false;
    }

    /// <summary>
    /// 角色点阵化开关
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="state"></param>
    public static void SetCharacterTransparentMaskState(RoleShaderManager roleShaderManager, bool state)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.PerCharacterMaskTransparentState = state;
    }

    /// <summary>
    /// 角色点阵化透明强度
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="MaskTransparent"></param>
    public static void SetCharacterTransparentMaskScale(RoleShaderManager roleShaderManager, float MaskTransparent)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.MaskTransparent = MaskTransparent;
    }
    
    /// <summary>
    /// 設置白平衡顔色
    /// </summary>
    /// <param name="kRGB"></param>
    public static void SetGlobalWhiteBalanceColor(Vector3 kRGB)
    {
        Shader.SetGlobalVector(ShaderIDs.kRGB,kRGB);
    }
    
    /// <summary>
    /// 设置角色场景实时阴影 开关和设置
    /// </summary>
    /// <param name="tex"></param>
    /// <param name="offset"></param>
    /// <param name="scale"></param>
    public static void SetSceneShadowState(bool state)
    {
        // if (state)
        // {
        //     Shader.EnableKeyword("RECEIVE_SHADOW");
        // }
        // else
        // {
        //     Shader.DisableKeyword("RECEIVE_SHADOW");
        // }
        //
        if (state)
        {
            Shader.EnableKeyword("WORLD_SPACE_SAMPLE_SHADOW");
        }
        else
        {
            Shader.DisableKeyword("WORLD_SPACE_SAMPLE_SHADOW");
        }
    }

    /// <summary>
    /// 场景接触阴影
    /// </summary>
    /// <param name="SceneID"></param>
    /// <param name="offset"></param>
    /// <param name="scale"></param>
    public static void SetSceneShadowSettings(object SceneID, Vector2 offset,float scale)
    {
        string texPath = ShaderIDs.senceTexPath + SceneID.ToString();
        globalShadowTex = Resources.Load<Texture2D>(texPath);
        Shader.SetGlobalTexture(ShaderIDs.SenceShadowMaskTexture, globalShadowTex);
        Shader.SetGlobalVector(ShaderIDs.SenceShadowOffset, offset);
        Shader.SetGlobalFloat(ShaderIDs.SenceShadowScale, scale);
    }
    
    /// <summary>
    /// 设置灯光设置
    /// </summary>
    /// <param name="senceID"></param>
    public static async void SetSenceLightingData(string senceID)
    {
        string saveDataPath = ShaderIDs.SaveDataPath + senceID + "_LightingSettings" + ".json";
        
#if UNITY_EDITOR
        if (!File.Exists(saveDataPath))
        {
            Debug.LogError("读取的灯光文件不存在！");
            return;
        }
        //var setting = new JsonSerializerSettings();
        //setting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        //string json = File.ReadAllText(saveDataPath);
        //LightingData data = new LightingData();
        //data = JsonConvert.DeserializeObject<LightingData>(json,setting);
        //if(data!=null)
        //    SetLightingSettings(data);
#else
        TextAsset json = await AddressablesManager.LoadAssetsAsync<TextAsset>(saveDataPath);
        if (!json)
        {
            Debug.LogError("读取的文件不存在！");
            return;
        }

        var setting = new JsonSerializerSettings();
        setting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        LightingData data = new LightingData();
        data = JsonConvert.DeserializeObject<LightingData>(json.text,setting);
        if(data!=null)
             SetLightingSettings(data);
#endif
    }

    /// <summary>
    /// 设置角色身体HSV
    /// </summary>
    public static void SetBodyHSV(RoleShaderManager roleShaderManager,Vector3 HSV)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetBodyHSV(HSV);
    }

    /// <summary>
    /// 设置角色头发HSV
    /// </summary>
    public static void SetHairHSV(RoleShaderManager roleShaderManager,Vector3 HSV)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetHairHSV(HSV);
    }

    /// <summary>
    /// 设置角色脸部HSV
    /// </summary>
    public static void SetFaceHSV(RoleShaderManager roleShaderManager,Vector3 HSV)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetFaceHSV(HSV);
    }

    /// <summary>
    /// 重置HSV
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void ResetHSV(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetBodyHSV(new Vector3(0,0,0));
        roleShaderManager.SetHairHSV(new Vector3(0,0,0));
        roleShaderManager.SetFaceHSV(new Vector3(0,0,0));
    }

    /// <summary>
    /// 设置敌人透明开启
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void SetEnemyTransparentON(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetEnemyTransparentState(true);
    }

    /// <summary>
    /// 设置敌人透明关闭
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void SetEnemyTransparentOFF(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetEnemyTransparentState(false);
    }

    /// <summary>
    /// 设置敌人透明强度
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="AlphaScale"></param>
    public static void SetEnemyAlphaScale(RoleShaderManager roleShaderManager, float AlphaScale)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.ColorAlpha = AlphaScale;
    }

    /// <summary>
    /// 设置角色透明开启
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void SetCharacterTransparentON(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetCharacterTransparentState(true);
    }
    
    /// <summary>
    /// 设置角色透明关闭
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void SetCharacterTransparentOFF(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetCharacterTransparentState(false);
    }
    
    /// <summary>
    /// 设置角色透明强度
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="AlphaScale"></param>
    public static void SetCharacterAlphaScale(RoleShaderManager roleShaderManager, float AlphaScale)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.ColorAlpha = AlphaScale;
    }
    
    /// <summary>
    /// 获取字体颜色矫正
    /// </summary>
    /// <param name="rgbInput"></param>
    /// <returns></returns>
    public static Color SetTextColorCorrect(Color rgbInput)
    {
        float alpha = GetPsAlphaCurves(rgbInput.a);
        rgbInput.r = Mathf.Pow(rgbInput.r, 0.45f);
        rgbInput.g = Mathf.Pow(rgbInput.g, 0.45f);
        rgbInput.b = Mathf.Pow(rgbInput.b, 0.45f);
        rgbInput.a = alpha;
        return rgbInput;
    }

    /// <summary>
    /// 初始化模糊RT单链表
    /// </summary>
    public static void InitGrabScreenTextureLinkList()
    {
        GrabScreenTextureList.Clear();
        GrabScreenTextureList = new LinkedList<BlurData>();
    }

    /// <summary>
    /// 设置模糊材质
    /// </summary>
    public static void SetBlurMaterial(Material material)
    {
        Debug.Log("当前rt数量 " + GrabScreenTextureList.Count);
        if (GrabScreenTextureList.Count > 0)
            material.SetTexture("_ScreenBlurTexture",GrabScreenTextureList.Last.Value.Texture);
    }
    
    /// <summary>
    /// 抓取当前屏幕尾插
    /// </summary>
    /*public static async Task<int> GrabCurrentScreenColor(float BlurRadius = 2,int downSample = 2, int iteration = 2)
    {
        if(GlobalPostprocessingData == null)
            GlobalPostprocessingData = GetGlobalPostprocessingData();
        
        GrabScreenFeature feature = (GrabScreenFeature)GlobalPostprocessingData.rendererFeatures[(int)ShaderIDs.RendererFeatureIndex.GrabScreenColor];
        feature.iteration = iteration;
        feature.downSample = downSample;
        feature.BlurRadius = BlurRadius;
        feature.SetGrabScreenPass();
        
        if (GrabScreenTextureList == null)
            InitGrabScreenTextureLinkList();
        
        //申请新RT用来存当前模糊画面
        RenderTexture TempGrabScreenTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.Default);
        TempGrabScreenTexture.name = "ScreenRT_" + TempGrabScreenTexture.GetInstanceID();
        //新申请RT data尾插
        BlurData newData = new BlurData();
        newData.Texture = TempGrabScreenTexture;
        newData.Index = TempGrabScreenTexture.GetInstanceID();
        GrabScreenTextureList.AddLast(newData);
        //置相机遍历未完成
        isTraversalCam = false;
        //打开模糊当前画面Feature
        if (GlobalPostprocessingData == null)
            GlobalPostprocessingData = GetGlobalPostprocessingData();
        GlobalPostprocessingData.rendererFeatures[(int) ShaderIDs.RendererFeatureIndex.GrabScreenColor].SetActive(true);
        //等待完成
        while (!isTraversalCam)
        {
            await Task.Delay(1);
        }
        return newData.Index;
    }*/
    
    /// <summary>
    /// 等待时间（受到Time.timeScale的影响）
    /// </summary>
    /// <param name="duration">等待时间</param>
    /// <param name="action">等待后执行的函数</param>
    /// <returns></returns>
    public static IEnumerator DelySetTraversalCam(float duration, Action action = null)
    {
        yield return new WaitForSeconds(duration);
        action?.Invoke();
    }
    
    /// <summary>
    /// 释放单链表节点
    /// </summary>
    public static void ReleaseCurrentScreenColor(int Index = -1)
    {
        if(Index == -1)
            return;
        
        if (GrabScreenTextureList.Count >  0)
        {
            BlurData data = GrabScreenTextureList.Single(o => o.Index == Index);
            LinkedListNode<BlurData> currentNode = GrabScreenTextureList.Find(data);
            //释放当前节点RT
            RenderTexture TempGrabScreenTexture = currentNode.Value.Texture;
            RenderTexture.ReleaseTemporary(TempGrabScreenTexture);

            //释放对应Index的节点
            GrabScreenTextureList.Remove(currentNode);
            //关闭模糊当前画面Feature
            GlobalPostprocessingData.rendererFeatures[(int) ShaderIDs.RendererFeatureIndex.GrabScreenColor].SetActive(false);
            isTraversalCam = false;
        }
    }

    /// <summary>
    /// 根据Index获取RT
    /// </summary>
    /// <param name="Index"></param>
    /// <returns></returns>
    public static RenderTexture GetGrabRenderTexture(int Index = -1)
    {
        if(Index == -1)
            return null;

        if (GrabScreenTextureList.Count > 0)
        {
            BlurData data = GrabScreenTextureList.Single(o => o.Index == Index);
            LinkedListNode<BlurData> currentNode = GrabScreenTextureList.Find(data);
            return currentNode.Value.Texture;
        }
        else
            return null;
    }

    /// <summary>
    /// 设置角色额外菲涅尔开启
    /// </summary>
    public static void SetAdditionFresnelON(RoleShaderManager roleShaderManager, Color RimColor,float FresnelRange,float FresnelFeather, float ShowRimScale)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetAdditionFresnel(true, RimColor, FresnelRange, FresnelFeather, ShowRimScale);
    }
    
    /// <summary>
    /// 设置角色额外菲涅尔关闭
    /// </summary>
    public static void SetAdditionFresnelOFF(RoleShaderManager roleShaderManager, Color RimColor,float FresnelRange,float FresnelFeather, float ShowRimScale)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetAdditionFresnel(false, Color.black, 0.5f, 0.5f, 1);
    }

    public static void SetAdditionFresnelOFF(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetAdditionFresnel(false, Color.black, 0.5f, 0.5f, 1);
    }
    
    /// <summary>
    /// 设置角色渲染
    /// </summary>
    /// <param name="state"></param>
    public static void SetCharacterSSRimLight(bool state)
    {
        if(GlobalConfig.CurrentRenderLevel == RenderLevel.Low || GlobalConfig.CurrentRenderLevel == RenderLevel.Middle)
            return;

        SetCharacterScreenSpaceRimLight(state);

        if (GlobalAsset == null)
            GlobalAsset = GetGlobalAsset();
        
        //GlobalAsset.supportDepthAdditional = state;
    }

    /// <summary>
    /// 边缘模糊
    /// </summary>
    /// <param name="state"></param>
    public static void SetRimBlur(bool state)
    {
        if(GlobalPostprocessingData)
            GlobalPostprocessingData = ShaderUtils.GetGlobalPostprocessingData();
        
        if(state)
            GlobalPostprocessingData.rendererFeatures[(int)ShaderIDs.RendererFeatureIndex.RimBlur].SetActive(true);
        else
            GlobalPostprocessingData.rendererFeatures[(int)ShaderIDs.RendererFeatureIndex.RimBlur].SetActive(false);
        
    }

    /// <summary>
    /// 黑白闪普攻
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void ShowBlackWhiteFlash(RoleShaderManager roleShaderManager,int limitFrame = 2)
    {
        if(roleShaderManager == null)
            return;
        
        GameObject Objetc = GameObject.Find("Object");
        //if (Objetc)
        //{
        //    BlackWhiteFlashSetting mBlackWhiteFlashSetting = Objetc.transform.Find("BlackWhiteFlashSetting").GetComponent<BlackWhiteFlashSetting>();
        //    mBlackWhiteFlashSetting.character = roleShaderManager;
        //    mBlackWhiteFlashSetting.frameSum = 0;
//
        //    mBlackWhiteFlashSetting.LimitFrame = limitFrame;
        //}
    }
    
    /// <summary>
    /// 角色变色开启
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="targetColor"></param>
    /// <param name="changeColorSlider"></param>
    public static void SetChangeColorON(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        for (int l = 0; l < roleShaderManager.change_color_renders.Length; l++)
        {
            roleShaderManager.change_color_renders[l].sharedMaterial.SetFloat("_UseChangeColor",1);
        }
    }

    /// <summary>
    /// 角色变色
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="target_color"></param>
    /// <param name="change_color_slider"></param>
    public static void SetChangeColor(RoleShaderManager roleShaderManager, Color target_color, float change_color_slider)
    {
        if(roleShaderManager == null)
            return;
        
        for (int l = 0; l < roleShaderManager.change_color_renders.Length; l++)
        {
            roleShaderManager.change_color_renders[l].sharedMaterial.SetColor("_TargetColor", target_color);
            roleShaderManager.change_color_renders[l].sharedMaterial.SetFloat("_ChangeColorSlider", change_color_slider);
        }
    }
    
    /// <summary>
    /// 角色变色关闭
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void SetChangeColorOFF(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        for (int l = 0; l < roleShaderManager.change_color_renders.Length; l++)
        {
            roleShaderManager.change_color_renders[l].sharedMaterial.SetFloat("_UseChangeColor",0);
        }
    }
    
    /// <summary>
    /// 狼人爪子闪烁
    /// </summary>
    /// <param name="material"></param>
    /// <param name="strength"></param>
    public static void SetEnemyWolfHitEffect(Material material,float strength)
    {
        material.SetFloat("_Float7",strength);
    }

    /// <summary>
    /// 设置角色自发光
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="emission"></param>
    public static void SetEmenyEmissionStrength(RoleShaderManager roleShaderManager, float TargetStrength)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetEnemyEmissionStrength(TargetStrength);
    }

    /// <summary>
    /// 获取当前角色自发光强度
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <returns></returns>
    public static float GetCurrentEnemyEmissionStrength(RoleShaderManager roleShaderManager)
    {
        return roleShaderManager.GetCurrentEnemyEmissionStrength();
    }
    
    /// <summary>
    /// 设置材质使用Dither剔除
    /// </summary>
    /// <param name="meshRendererArray"></param>
    /// <param name="Transparency"></param>
    public static void SetBuildingsDither(bool state, MeshRenderer[] meshRendererArray, float Transparency = 0)
    {
        for (int i = 0; i < meshRendererArray.Length; i++)
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            meshRendererArray[i].GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat("_UseDitherClip",Convert.ToInt32(state));
            propertyBlock.SetFloat("_Transparency",Transparency);
            meshRendererArray[i].SetPropertyBlock(propertyBlock);
        }
    }

    /// <summary>
    /// 设置角色主城脸部阴影
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="lightTransform"></param>
    public static void SetCharacterMainCityFaceShadow(RoleShaderManager roleShaderManager, GameObject lightTransform)
    {
        if(roleShaderManager == null)
            return;
        
        Vector2 faceDir = new Vector2(roleShaderManager.m_RoleFaceObj.transform.forward.x, roleShaderManager.m_RoleFaceObj.transform.forward.z);
        Vector2 lightForward = new Vector2(lightTransform.transform.forward.x, lightTransform.transform.forward.z);
        Vector2 lightLeft = new Vector2(-lightTransform.transform.right.x, -lightTransform.transform.right.z);
        
        float angle = Vector2.Angle(faceDir, lightLeft);
        float dotNum = math.dot(lightForward, faceDir);
        if (dotNum < 0) 
            angle = 360 - angle;
        
        GetMainCityFaceAngle(roleShaderManager, angle);
    }
    
    /// <summary>
    /// 设置延迟渲染Mesh 的Layer
    /// </summary>
    public static void SetDeferredMeshLayer()
    {
        if (ShaderUtils.GetGlobalPostprocessingData().renderingMode == RenderingMode.Deferred)
        {
            foreach (MeshRenderer item in UnityEngine.Object.FindObjectsOfType(typeof(MeshRenderer)))
            {
                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                item.GetPropertyBlock(propertyBlock);
                uint meshLayer = (uint)item.gameObject.layer;
                propertyBlock.SetInt("_MeshRenderingLayer",(int)meshLayer);
                item.SetPropertyBlock(propertyBlock);
            }    
        }
    }
    
    /// <summary>
    /// 设置AMD超分采样开启关闭
    /// </summary>
    /// <param name="state"></param>
    public static void SetFSRState(bool state)
    {
        if (ShaderUtils.GlobalAsset == null)
            ShaderUtils.GlobalAsset = ShaderUtils.GetGlobalAsset();
    }

    /// <summary>
    /// 设置正交相机到透视 状态
    /// </summary>
    /// <param name="state"></param>
    /// <param name="material"></param>
    public static void SetCriOrth2PerspState(bool state, Material material)
    {
        if(state)
            material.EnableKeyword("CRI_APPLY_ORTH2PERSP");
        else
            material.DisableKeyword("CRI_APPLY_ORTH2PERSP");
    }

    /// <summary>
    /// 设置Cri视频伽马矫正
    /// </summary>
    /// <param name="material"></param>
    public static void SetCtiVideoGammaCorrect(bool state, Material material)
    {
        if(state)
            material.EnableKeyword("GAMMA_TO_LINEAR");
        else
            material.DisableKeyword("GAMMA_TO_LINEAR");
    }

    /// <summary>
    /// 设置角色Unity雾开启
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void SetCharacterFogOn(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.useUnityFog = true;
    }
    
    /// <summary>
    /// 设置角色Unity雾关闭
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void SetCharacterFogOff(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.useUnityFog = false;
    }

    /// <summary>
    /// 设置场景材质亮度
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="value"></param>
    public static void SetSceneCrystalValue(Material mat, float value)
    {
        mat.SetFloat("_Value", value);
    }
    
    /// <summary>
    /// 场景变暗效果打开
    /// </summary>
    /// <param name="camera"></param>
    /*public static void SetSkyDarkOn(Camera camera, Color co)
    {
        SkyDarkCtrl mSkyDarkCtrl = camera.GetComponent<SkyDarkCtrl>();
        if (mSkyDarkCtrl == null)
        {
            mSkyDarkCtrl = camera.gameObject.AddComponent<SkyDarkCtrl>();
            mSkyDarkCtrl.Distance = 3;
            mSkyDarkCtrl.oval_X = 0.5f;
            mSkyDarkCtrl.oval_Y = 0.35f;
            mSkyDarkCtrl.ovalWidth = 2;
            mSkyDarkCtrl.ovalHeight = 1.32f;
            mSkyDarkCtrl.Range = 1;
            mSkyDarkCtrl.Smooth = 1;
        }

        if (!mSkyDarkCtrl.enabled)
        {
            mSkyDarkCtrl.enabled = true;
        }
        
        mSkyDarkCtrl.color = co;

    }*/

    /// <summary>
    /// PBR材质溶解打开
    /// </summary>
    /// <param name="materials"></param>
    /// <param name="dissolveScale"></param>
    public static void SetPBRDissolveON(Material[] materials)
    {
        for (int l = 0; l < materials.Length; l++)
        {
            materials[l].SetFloat("_UseDissolve",1);
        }
    }
    
    /// <summary>
    /// PBR材质溶解
    /// </summary>
    /// <param name="materials"></param>
    /// <param name="dissolveScale"></param>
    public static void SetPBRDissolve(Material[] materials, float dissolveScale)
    {
        for (int l = 0; l < materials.Length; l++)
        {
            materials[l].SetFloat(ShaderIDs.DissolveScale, dissolveScale);
        }
    }
    
    /// <summary>
    /// PBR材质溶解关闭
    /// </summary>
    /// <param name="materials"></param>
    /// <param name="dissolveScale"></param>
    public static void SetPBRDissolveOFF(Material[] materials)
    {
        for (int l = 0; l < materials.Length; l++)
        {
            materials[l].SetFloat("_UseDissolve",0);
        }
    }

    /// <summary>
    /// 设置粒子特效最大数量
    /// </summary>
    /// <param name="count"></param>
    /// <param name="mParticleSystemList"></param>
    /// 
    public static void SetParticleSystemCount(int count,ParticleSystem ParticleSystem)
    {
        ParticleSystem.maxParticles = 0;
    }
    
    public static void SetParticleSystemCount(int count,List<ParticleSystem> mParticleSystemList)
    {
        for (int l = 0; l < mParticleSystemList.Count; l++)
        {
            mParticleSystemList[l].maxParticles = 0;
        }
    }
    
    /// <summary>
    /// 设置SDF AO开启
    /// </summary>
    public static void Set_SDF_AO_ON(RoleShaderManager[] roleShaderManagers)
    {
        GlobalConfig.useSdfAO = true;
        Shader.SetGlobalFloat("_ENABLE_SDF_AO",1);

        for (int l = 0; l < roleShaderManagers.Length; l++)
        {
            if(roleShaderManagers[l]!=null)
                roleShaderManagers[l].useSDFAO = true;
        }
    }

    /// <summary>
    /// SDF AO
    /// </summary>
    /// <param name="left_foot"></param>
    /// <param name="right_foot"></param>
    /// <param name="useSdfAO"></param>
    public static void Update_SDF_AO(RoleShaderManager[] roleShaderManagers)
    {
        if(!GlobalConfig.useSdfAO)
            return;
        
        if(roleShaderManagers == null)
            return;
        
        if(roleShaderManagers.Length <=0)
            return;
        
        float[] character_caster_shadow_array = new float[ShaderIDs.SDF_AO_MAX_COUNT];
        Vector4[] character_left_foot_pos_array= new Vector4[ShaderIDs.SDF_AO_MAX_COUNT];
        Vector4[] character_right_foot_pos_array= new Vector4[ShaderIDs.SDF_AO_MAX_COUNT];
        
        if(roleShaderManagers.Length > ShaderIDs.SDF_AO_MAX_COUNT)
        {
            Debug.LogError("超出最大支持个数！最多支持24个角色！");
            return;
        }
        
        for (int l = 0; l < roleShaderManagers.Length; l++)
        {
            if(!roleShaderManagers[l].useSDFAO)
                return;
            
            if(roleShaderManagers[l].left_foot_transform == null || roleShaderManagers[l].right_foot_transform == null)
                return;

            character_caster_shadow_array[l] = (float)Convert.ToInt32(roleShaderManagers[l].useShadow);
            character_left_foot_pos_array[l] = roleShaderManagers[l].left_foot_transform.position;
            character_right_foot_pos_array[l] = roleShaderManagers[l].right_foot_transform.position;
        }

        Shader.SetGlobalFloatArray("_character_caster_shadow_array",character_caster_shadow_array);
        Shader.SetGlobalVectorArray("_character_left_foot_pos_array",character_left_foot_pos_array);
        Shader.SetGlobalVectorArray("_character_right_foot_pos_array",character_right_foot_pos_array);
    }
    
    /// <summary>
    /// 设置SDF AO关闭
    /// </summary>
    /// <param name="roleShaderManagers"></param>
    public static void Set_SDF_AO_OFF(RoleShaderManager[] roleShaderManagers)
    {
        GlobalConfig.useSdfAO = false;
        Shader.SetGlobalFloat("_ENABLE_SDF_AO",0);

        for (int l = 0; l < roleShaderManagers.Length; l++)
        {
            if(roleShaderManagers[l]!=null)
                roleShaderManagers[l].useSDFAO = false;
        } 
    }
    
    /// <summary>
    /// 场景变暗效果关闭
    /// </summary>
    /// <param name="camera"></param>
    /*public static void SetSkyDarkOff(Camera camera)
    {
        SkyDarkCtrl mSkyDarkCtrl = camera.GetComponent<SkyDarkCtrl>();
        if (mSkyDarkCtrl == null)
            mSkyDarkCtrl = camera.gameObject.AddComponent<SkyDarkCtrl>();

        mSkyDarkCtrl.enabled = false;
    }*/

    /// <summary>
    /// 角色自定义边缘光控制
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="width"></param>
    public static void SetCustomRimWidthOn(RoleShaderManager roleShaderManager,float width)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.useCustomRimLightWidth = true;
        roleShaderManager.CustomRimLightWidth = width;
    }
    
    public static void SetCustomRimWidthOn(RoleShaderManager[] roleShaderManagers,float width)
    {
        for (int l = 0; l < roleShaderManagers.Length; l++)
        {
            if (roleShaderManagers[l] != null)
            {
                roleShaderManagers[l].useCustomRimLightWidth = true;
                roleShaderManagers[l].CustomRimLightWidth = width;      
            }
        }
    }
    
    public static void SetCustomRimWidthOff(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.useCustomRimLightWidth = false;
    }
    
    public static void SetCustomRimWidthOff(RoleShaderManager[] roleShaderManagers)
    {
        for (int l = 0; l < roleShaderManagers.Length; l++)
        {
            if(roleShaderManagers[l]!=null)
                roleShaderManagers[l].useCustomRimLightWidth = false;
        }
    }
    
    /// <summary>
    /// 角色自定义描边光控制
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="width"></param>
    public static void SetCustomOutLineWidthOn(RoleShaderManager roleShaderManager,float width)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.useCustomOutLineWidth = true;
        roleShaderManager.CustomOutLineWidth = width;
    }
    
    public static void SetCustomOutLineWidthOn(RoleShaderManager[] roleShaderManagers,float width)
    {
        for (int l = 0; l < roleShaderManagers.Length; l++)
        {
            if (roleShaderManagers[l] != null)
            {
                roleShaderManagers[l].useCustomOutLineWidth = true;
                roleShaderManagers[l].CustomOutLineWidth = width;     
            }
        }
    }
    
    public static void SetCustomOutLineWidthOff(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.useCustomOutLineWidth = false;
    }
    
    public static void SetCustomOutLineWidthOff(RoleShaderManager[] roleShaderManagers)
    {
        for (int l = 0; l < roleShaderManagers.Length; l++)
        {
            if(roleShaderManagers[l] != null)
                roleShaderManagers[l].useCustomOutLineWidth = false;
        }
    }

    /// <summary>
    /// 设置AlphaClip
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="state"></param>
    public static void SetRoleSimpleAlphaClip(RoleShaderManager roleShaderManager, bool state)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetAlphaClip(state);
    }

    /// <summary>
    /// 设置纹理插值变换
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="DissolveStrength"></param>
    /// <param name="EdgeScale"></param>
    /// <param name="EdgeColor"></param>
    public static void SetRoleSimpleTextureLerp(RoleShaderManager roleShaderManager, float DissolveStrength)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.SetMainTextureLerp(DissolveStrength);
    }

    /// <summary>
    /// 设置角色暗部亮度控制打开
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void SetRoleDarkPartLuminanceON(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.UseDarkLuminance = true;
    }
    
    /// <summary>
    /// 设置角色暗部亮度控制关闭
    /// </summary>
    /// <param name="roleShaderManager"></param>
    public static void SetRoleDarkPartLuminanceOFF(RoleShaderManager roleShaderManager)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.UseDarkLuminance = false;
    }
    
    /// <summary>
    /// 设置角色暗部亮度控制值
    /// </summary>
    /// <param name="roleShaderManager"></param>
    /// <param name="value"></param>
    public static void SetRoleDarkPartLuminanceValue(RoleShaderManager roleShaderManager,float value)
    {
        if(roleShaderManager == null)
            return;
        
        roleShaderManager.DarkLuminance = value;
    }

    /// <summary>
    /// 设置3D物体在2DUI下的自适应
    /// </summary>
    /// <param name="root3d"></param>
    /// <param name="root2d"></param>
    /// <param name="UICamera"></param>
    /// <param name="height"></param>
    /// <param name="planeZ"></param>
    /*public static Vector3 Set3DObjectTransformAdaptive(Vector3 root2dPos, Camera mainCamera, Camera UICamera, float planeZ = 1.5f,
        float height = 0.75f)
    {
        Vector2 screenPoint = PositionConvert.UIPointToScreenPoint(new Vector3(root2dPos.x, root2dPos.y, 0), UICamera);
        Vector3 worldPoint = PositionConvert.ScreenPointToWorldPoint(screenPoint, planeZ, mainCamera);

        return new Vector3(worldPoint.x, worldPoint.y - height, worldPoint.z);
    }*/
    
    /// <summary>
    /// 清理TMP资源
    /// </summary>
    /// <param name="font"></param>
    /*public static async void ClearAllFontAssetData(TMP_Text[] texts)
    {
        TMP_FontAsset[] tmpArray = new TMP_FontAsset[4];
        //  加载不明体
        tmpArray[0] = await AddressablesManager.LoadAssetsAsync<TMP_FontAsset>(GlobalConfig.BUMING);
        //  加载FZLTHJ体
        tmpArray[1] = await AddressablesManager.LoadAssetsAsync<TMP_FontAsset>(GlobalConfig.FZLTHS);
        //  加载青柳体
        tmpArray[2] = await AddressablesManager.LoadAssetsAsync<TMP_FontAsset>(GlobalConfig.QinLiuTi);
        //  加载GBK
        tmpArray[3] = await AddressablesManager.LoadAssetsAsync<TMP_FontAsset>(GlobalConfig.GBK);
        
        for (int i = 0; i< tmpArray.Length; i++)
            tmpArray[i].ClearFontAssetData(true);

        for (int l = 0; l < texts.Length; l++)
        {
            texts[l].havePropertiesChanged = true;
        }
    }*/
    
    /// <summary>
    /// 获取时间戳
    /// </summary>
    /// <returns></returns>
    public static int GetTimeStamp()
    {
        TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1);;
        return (int)(ts.TotalMilliseconds);
    }

    //  -----------------------------------------效果控制接口-----------------------------------------
    
    
    /// <summary>
    /// 设置全局RenderScale
    /// </summary>
    /*public static void SetGloablRenderScale(float scale)
    {
        if (ShaderUtils.GlobalAsset == null)
            ShaderUtils.GlobalAsset = ShaderUtils.GetGlobalAsset();

        ShaderUtils.GlobalAsset.renderScale = scale;
        
        ShaderUtils.GlobalCameraRenderLevelData.PerCameraRenderScale = 1.0f;
    }*/
    
    /// <summary>
    /// 设置3D RenderScale
    /// </summary>
    /*public static void SetPerRenderScale(float scale)
    {
        if (ShaderUtils.GlobalCameraRenderLevelData == null)
            ShaderUtils.GlobalCameraRenderLevelData = ShaderUtils.GetGlobaCameraRenderLevelData();
        
        ShaderUtils.GlobalCameraRenderLevelData.PerCameraRenderScale = scale;
    }*/

    /// <summary>
    /// 获取3D RenderScale
    /// </summary>
    /// <returns></returns>
    /*public static float GetPerRenderScale()
    {
        if (ShaderUtils.GlobalCameraRenderLevelData == null)
            ShaderUtils.GlobalCameraRenderLevelData = ShaderUtils.GetGlobaCameraRenderLevelData();

        return ShaderUtils.GlobalCameraRenderLevelData.PerCameraRenderScale;
    }*/
    
    /// <summary>
    /// 设置深度图开启状态
    /// </summary>
    /// <param name="state"></param>
    public static void SetDepthState(bool state)
    {
        if (ShaderUtils.GlobalAsset == null)
            ShaderUtils.GlobalAsset = ShaderUtils.GetGlobalAsset();

        ShaderUtils.GlobalAsset.supportsCameraDepthTexture = state;

    }
    
    /// <summary>
    /// 设置额外深度开启状态
    /// </summary>
    /// <param name="state"></param>
    /*public static void SetDepthAdditionalState(bool state)
    {
        if (ShaderUtils.GlobalAsset == null)
            ShaderUtils.GlobalAsset = ShaderUtils.GetGlobalAsset();

        ShaderUtils.GlobalAsset.supportDepthAdditional = state;
    }*/
    
    /// <summary>
    /// 设置OpaqueColorTexture降采样
    /// </summary>
    public static void SetOpaqueTextureDownSample(Downsampling sample)
    {
        //ShaderUtils.GlobalAsset.opaqueDownsampling = sample;
    }
    
    /// <summary>
    /// 设置ShadowMap质量
    /// </summary>
    /// <param name="level"></param>
    public static void SetShadowMapQuality(int size)
    {
        if (GlobalAsset == null)
            GlobalAsset = GetGlobalAsset();
        
        GlobalAsset.shadowCascadeOption = ShadowCascadesOption.NoCascades;
        //GlobalAsset.mainLightShadowmapResolution = size;
    }
    
    /// <summary>
    /// 设置抗锯齿开关
    /// </summary>
    /// <param name="state"></param>
    /*public static void SetAntiAlisingState(bool state)
    {
        if (GlobalCameraRenderLevelData == null)
            GlobalCameraRenderLevelData = GetGlobaCameraRenderLevelData();

        GlobalCameraRenderLevelData.UseAntiAlising = state;
    }*/
    
    /// <summary>
    /// 设置抗锯齿类型
    /// </summary>
    /// <param name="aa"></param>
    //public static void SetAntiAlisingType(AntiAliasing type)
    //{
    //    if (GlobalCameraRenderLevelData == null)
    //        GlobalCameraRenderLevelData = GetGlobaCameraRenderLevelData();
//
    //    GlobalCameraRenderLevelData.AntiAliasingType = type;
    //}
    
    /// <summary>
    /// 设置后处理开启状态
    /// </summary>
    /// <param name="state"></param>
    /*public static void SetPostProcessingState(bool state)
    {
        if (GlobalCameraRenderLevelData == null)
            GlobalCameraRenderLevelData = GetGlobaCameraRenderLevelData();

        GlobalCameraRenderLevelData.UsePostProcessing = state;
    }*/
    
    /// <summary>
    /// 设置目标帧率
    /// </summary>
    /// <param name="state"></param>
    public static void SetApplicationTargetFrame(int targetFrame)
    {
        Application.targetFrameRate = targetFrame;
    }
    
    /// <summary>
    /// 设置垂直同步数量        VSyncs数值需要在每帧之间传递，使用0为不等待垂直同步。值必须是0，1或2。
    /// 开启了垂直同步的话，会使用平台的默认渲染帧率和vSyncCount来决定最终最大的渲染帧率。比如某平台的默认渲染帧率为60fps,vSyncCount设置为2。那么最终目标渲染帧率为60/2=30fps。
    /// </summary>
    /// <param name="count"></param>
    public static void SetVSyncCount(int count)
    {
        QualitySettings.vSyncCount = count;
    }
    
    /// <summary>
    /// 使用体积雾
    /// </summary>
    /// <param name="state"></param>
    public static void SetVolumeFog(bool state)
    {
        GlobalConfig.useVolumeFog = state;
    }
    
    /// <summary>
    /// 设置Bloom
    /// </summary>
    /// <param name="state"></param>
    public static void SetBloom(bool state)
    {
        VolumeProfile profile = GetGlobaVolumeProfile();
        for (int l = 0; l < profile.components.Count; l++)
        {
            if (profile.components[l].name == "BloomSimple")
            {
                profile.components[l].active = state;
            }
        }
    }

    /// <summary>
    /// 设置实时阴影
    /// </summary>
    /// <param name="state"></param>
    /*public static void SetRealTimeShadowState(bool state)
    {
        if (GlobalAsset == null)
            GlobalAsset = GetGlobalAsset();

        GlobalAsset.supportsMainLightShadows = state;
    }*/

    /// <summary>
    /// 设置贴图整体质量
    /// </summary>
    /// <param name="count"></param>
    public static void SetGloablTextureQuality(int count)
    {
        QualitySettings.globalTextureMipmapLimit = count;
    }
    
    /// <summary>
    /// 设置分辨率
    /// </summary>
    /// <param name="renderLevel"></param>
    public static void SetResolution(Vector2 resolution, bool state = true)
    {
        Screen.SetResolution((int)resolution.x,(int)resolution.y, state);
    }
    
    /// <summary>
    /// 设置后处理渲染等级
    /// </summary>
    /// <param name="level"></param>
    public static void SetGlobalRenderLevel(RenderLevel level)
    {
        GlobalConfig.CurrentRenderLevel = level;
    }

    public static void SetPostProcessLevel(RenderLevel level)
    {
        GlobalConfig.PostProcessLevel = level;
    }
    
    /// <summary>
    /// 设置角色提前深度
    /// </summary>
    /// <param name="state"></param>
    public static void SetCharacterPreDepth(bool state)
    {
        SetCharacterScreenSpaceRimLight(state);
        SetCharacterSSRimLight(state);
    }
}
