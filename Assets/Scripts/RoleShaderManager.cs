using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using DG.Tweening;
using Unity.Mathematics;
//using Newtonsoft.Json;
using System.IO;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[ExecuteInEditMode]
public class RoleShaderManager : RoleShaderFunctions
{
    /// <summary>
    /// 当前类型
    /// </summary>
    [SerializeField] public CharacterType _characterType = CharacterType.Player;
    
    /// <summary>
    /// 灯光部分参数
    [SerializeField] public Color LightColor = Color.white;
    /// </summary>
    [SerializeField] public bool UseWhiteBalance = false;
    [Range(0, 3)]
    [SerializeField] public float LightStrength = 1;
    [SerializeField] public bool UseAddColor = false;
    [SerializeField] public Color AddColor = Color.white;
    
    [SerializeField] public bool UseAdditionLight = false;
    [Range(0, 1)]
    [SerializeField] public float addShadowRangeStep = 0.5f;
    [Range(0, 1)]
    [SerializeField] public float addShadowFeather = 0.3f;

    [SerializeField] public bool UseDarkLuminance = false;
    [Range(-1, 1)]
    [SerializeField] public float DarkLuminance = 0.0f;

    /// <summary>
    /// 阴影部分参数
    /// </summary>
    [SerializeField] public bool useShadow = true;

    /// <summary>
    /// 相机灯光参数
    /// </summary>a
    [SerializeField] public bool useCameraLight;
    [SerializeField] public bool useDebugQuad;
    [SerializeField] private GameObject CameraLight;
    [SerializeField] private Vector3 cameraLightForward;
    [Range(-10, 10)] 
    [SerializeField] public float CameraLightRotY = 5;
    [Range(-180, 180)] 
    [SerializeField] public float CameraLightRotXZ = 30;

    /// <summary>
    /// 点阵化半透明
    /// </summary>
    [SerializeField] public bool PerCharacterMaskTransparentState = false;
    [Range(0, 1)]
    [SerializeField] public float MaskTransparent = 1;

    /// <summary>
    /// 角色溶解参数
    /// </summary> 
    [SerializeField] public bool UseDossolve = false;
    [Range(0,1)]
    [SerializeField] public float dissolveScale = 0;
    [SerializeField] public Vector2 ScreenUV = new Vector2(7, 3); 
    [ColorUsage(true,true)]
    [SerializeField] public Color edgeColor = new Color(2.1185f,0.1552f,0.1552f,1);

    /// <summary>
    /// 角色流光剔除参数
    /// </summary> 
    [SerializeField] public DissolvePart m_dissolvePart = DissolvePart.All;
    [SerializeField] public bool UsePosWDissolve = false;
    [SerializeField] public float PosWDissolveScale = 0;
    [SerializeField] public float PosWDissolveWidth = 0.075f;
    [SerializeField] public float MaskScale = 0.3f;
    [ColorUsage(true,true)]
    [SerializeField] public Color PosWDissolveColor = new Color(1,1,1);
    [SerializeField] public bool useTop2Bottom = false;

    /// <summary>
    /// 是否是展示界面
    /// </summary>
    public bool isShowScene = false;
    
    /// <summary>
    /// 角色前发投影
    /// </summary>
    [SerializeField] public bool useHairShadow;

    /// <summary>
    /// 角色描边宽度
    /// </summary>
    [SerializeField] public bool useCustomOutLineWidth = false;
    [SerializeField] [Range(0,3)]public float CustomOutLineWidth = 0.5f;
    
    /// <summary>
    /// 角色SS边缘光宽度
    /// </summary>
    [SerializeField] public bool useCustomRimLightWidth = false;
    [SerializeField] [Range(0,1)]public float CustomRimLightWidth = 0.5f;

    /// <summary>
    /// 自定义边缘光颜色
    /// </summary>
    [SerializeField] public bool useCustomRimLightColor = false;
    [SerializeField] public Color CustomRimLightColor = new Color(0.35f,0.35f,0.35f,1.0f);
    
    /// <summary>
    /// 使用额外菲涅尔
    /// </summary>
    [SerializeField] public bool TestAdditionFresnel = false;
    [SerializeField] public bool UseAdditionFresnel;
    [Range(0.001f, 1f)]
    [SerializeField] public float FresnelRange = 0.5f;
    [Range(0.001f, 1f)]
    [SerializeField] public float FresnelFeather= 0.5f;
    
    [Range(0, 2f)]
    [SerializeField] public float showRimScale;
    [SerializeField] [ColorUsage(true, true)] public Color rimColor;

    /// <summary>
    /// 冰冻效果
    /// </summary>
    [SerializeField] public bool useIceEffect = false;
    [SerializeField] public Color IceColor = new Color(0.683f,0.919f,1,1);
    [SerializeField] [Range(0,1)] public float IceNormalScale = 1;
    [SerializeField] public Color IceRimColor = new Color(0.64f,0.65f,0.65f,1);
    [SerializeField] [Range(0,1)] public float IceFresnelFeather = 0.2f;
    [SerializeField] [Range(0,1)] public float IceFresnelStep = 0.3f;
    [SerializeField] public Color IceSpecColor = new Color(0.639f, 0.76f, 0.811f, 1);
    [SerializeField] public float IceSpecPower = 5;
    
    
    /// <summary>
    /// 怪物隐身效果
    /// </summary>
    [SerializeField] public bool useInvisible = false;
    [SerializeField] [Range(0,1)]public float ColorAlpha = 1;

    /// <summary>
    /// 角色脸部冰霜效果
    /// </summary>
    [SerializeField] public bool  useFaceIceQuad = false;
    [SerializeField] public float IceQuadDensity = 1;
    [SerializeField] [Range(0,0.99f)]public float IceQuadDissolveScale = 0;

    /// <summary>
    /// 角色环境边缘光
    /// </summary>
    [SerializeField] public bool  useEnvironmentRimLight = false;
    [SerializeField] public Texture2D m_ToonRamp;
    [SerializeField] [ColorUsage(true, true)] public Color EnvColor;
    [SerializeField] [Range(0,10)]public float EnvRimPower = 0;
    [SerializeField] public float EnvRimOffset = 0.24f;

    /// <summary>
    /// Unity距离雾
    /// </summary>
    [SerializeField] public bool useUnityFog = true;

    /// <summary>
    /// SDF AO
    /// </summary>
    [SerializeField] public bool useSDFAO = false;
    [SerializeField] public Transform left_foot_transform;
    [SerializeField] public Transform right_foot_transform;

    /// <summary>
    /// 其他参数
    /// </summary> 
    private GameObject RoleObject;
    [SerializeField] public GameObject m_RoleFaceObj;
    private Camera mainCamera;
    private UniversalAdditionalCameraData mainCameraData;
    
    /// <summary>
    /// 是否可以自动虚化
    /// </summary>
    public bool CanInvisibility = false;
    
    /// <summary>
    /// 是否使用根据相机距离自动虚化的功能
    /// </summary>
    public bool AutoInvisibilityByCamera = false;
    public float distance;
    private MaterialPropertyBlock propertyBlock;
    private Mesh DebugQuadMesh;
    public bool updateState = false;
    public bool isSelect = false;
    public bool StartEnd = false;


    #region 客户端加 K帧 武器出现
    //  使用武器溶解出现
    public bool useWeaponShine = false;
    //  武器溶解范围
    [Range(0,1)]public float showObjectScale= 0;
    //  流光宽度
    [Range(0,0.3f)]public float weaponDissolveWdith = 0;
    [Range(0,1)]public float weaponRimScale= 0;
    //  武器流光颜色
    [ColorUsage(true, true)] public Color weaponEdgeColor = Color.white;
    [ColorUsage(true,true)]public Color weaponRimColor = Color.white;
    #endregion

    #region 变色特效
    public SkinnedMeshRenderer[] change_color_renders = new SkinnedMeshRenderer[0];
    #endregion

#if UNITY_EDITOR
    /// <summary>
    /// OnDrawGizmos
    /// </summary>
    private void OnDrawGizmos()
    {
        if (mainCamera != null && CameraLight != null && useDebugQuad)
        {
            //DebugQuadMesh = ShaderUtils.InitGridMesh(DebugQuadMesh,CameraLight,mainCamera);
            //Gizmos.color = new Color(1.0f, 0, 0, 0.3f);
            //Gizmos.DrawWireMesh(DebugQuadMesh);   
        }
    }
#endif

    private void Awake()
    {
        AutoInvisibilityByCamera = false;
        PerCharacterMaskTransparentState = false;
        updateState = true;
    }

    /// <summary>
    /// Start
    /// </summary>
    private void OnEnable()
    {
        //获取主相机用于相机灯光和FOV描边控制
        GetMainCamera();
        //初始化设置
        InitStartSettings();
        //实例化角色材质
        InitCharacterMaterial();
        //更新角色材质参数
        UpdateRoleShaderParams();
        //设置前发投影
        SetCharacterCelHairShadow(_characterType,useHairShadow);
        //设置角色外勾边正常模式
        SetRoleOutLineCommon(out isSelect);
        StartEnd = true;
    }

    private void Start()
    {
        if(_characterType == CharacterType.Player)
            Shader.DisableKeyword("WORLD_SPACE_SAMPLE_SHADOW");
    }

    /// <summary>
    /// Update
    /// </summary>
    private void Update()
    {
        if (updateState)
        {
            //更新角色常量和图片
            UpdateRoleShaderParams();
            
            
            //设置角色点阵化透明
            if (AutoInvisibilityByCamera)
                SetCharacterMaskTransparentState(ref PerCharacterMaskTransparentState, CanInvisibility, isSelect, mainCamera, ref MaskTransparent);
        }
    }

    /// <summary>
    /// 更新角色shader参数
    /// </summary>
    private void UpdateRoleShaderParams()
    {
        //  判断初始化设置
        JudgeStratSettings();
        
        //  旋转相机灯光
        RotateCameraLight();

        //  更新角色参数
        foreach (CharacterRender child in MaterialList)
        {
            //运行中跑SRP Batching
            if (Application.isPlaying)
            {
                if (child != null)
                {
                    if (_characterType == CharacterType.Enemy || _characterType == CharacterType.HumanEnemy)
                    {
                        // 更新敌人描边宽度
                        UpdateEnemyOutLine(child.sharedMaterials,isShowScene,_characterType,useCustomOutLineWidth,CustomOutLineWidth);
                        // 更新边缘光颜色
                        UpdateEnemyRimColor(useCustomRimLightColor,child.sharedMaterials,CustomRimLightColor);
                        // 更新敌人灯光参数
                        UpdateEnemyLightParams(child.sharedMaterials, LightStrength, LightColor, AddColor, UseWhiteBalance, UseAddColor, UseAdditionLight && GlobalConfig.useAdditionalLight);
                        // 更新冰冻效果
                        UpdateIceVFX(child.sharedMaterials, useIceEffect, IceColor, IceNormalScale, IceRimColor, IceFresnelFeather, IceFresnelStep, IceSpecColor, IceSpecPower);
                        // 更新死亡溶解
                        UpdateDossolve(child.sharedMaterials, UseDossolve, dissolveScale, edgeColor, ScreenUV);
                        // 更新敌人透明
                        UpdateTransparentAlpha(useInvisible, child.sharedMaterials, ColorAlpha);
                        // 更新敌人点阵化透明
                        UpdateMaskTransparent(child.sharedMaterials, PerCharacterMaskTransparentState, MaskTransparent);
                        //  更新FOV控制
                        UpdateCameraFOV(child.sharedMaterials, mainCamera);
                        //  更新Unity雾
                        UpdateUnityFogEnable(child.sharedMaterials, useUnityFog);
                        
                        // 更新脸部朝向
                        if (m_RoleFaceObj != null && _characterType == CharacterType.HumanEnemy)
                        {
                            for (int i = 0; i < child.sharedMaterials.Length; i++)
                            {
                                child.sharedMaterials[i].SetVector(ShaderIDs.FaceForwardVector, m_RoleFaceObj.transform.forward);
                            }
                        }
                    }
                    else
                    {
                        // 更新卡通灯光
                        UpdateToonLight(child.sharedMaterials, useCameraLight, mainCamera, cameraLightForward, LightStrength);
                        // 更新角色灯光
                        UpdateRoleLightParams(child.sharedMaterials, LightColor, AddColor, UseWhiteBalance, UseAddColor, UseAdditionLight && GlobalConfig.useAdditionalLight, addShadowRangeStep, addShadowFeather);
                        // 更新暗部亮度控制
                        UpdateRoleDarkLuminance(child.sharedMaterials,UseDarkLuminance,DarkLuminance);
                        // 更新冰冻效果
                        UpdateIceVFX(child.sharedMaterials, useIceEffect, IceColor, IceNormalScale, IceRimColor, IceFresnelFeather, IceFresnelStep, IceSpecColor, IceSpecPower);
                        //  更新脸部冰冻
                        UpdateFaceIceQuad(child.sharedMaterials, useFaceIceQuad, IceQuadDensity, IceQuadDissolveScale);
                        // 更新角色描边
                        UpdateRoleOutLine(child.isFace,child.sharedMaterials, isShowScene, useCustomOutLineWidth,CustomOutLineWidth);
                        // 更新屏幕空间边缘光
                        UpdateRoleRimScale(useCustomRimLightWidth,child.sharedMaterials,CustomRimLightWidth);
                        // 更新边缘光颜色
                        UpdateRoleRimColor(useCustomRimLightColor,child.sharedMaterials,CustomRimLightColor);
                        // 更新死亡溶解
                        UpdateDossolve(child.sharedMaterials, UseDossolve, dissolveScale, edgeColor, ScreenUV);
                        // 更新世界坐标剔除
                        if(m_dissolvePart == DissolvePart.All)
                            UpdateCharacterClip(child.sharedMaterials, UsePosWDissolve, PosWDissolveScale, PosWDissolveWidth, MaskScale, PosWDissolveColor, useTop2Bottom);
                        else
                        {
                            if (child.isWeapon)
                            {
                                UpdateCharacterClip(child.sharedMaterials,UsePosWDissolve, PosWDissolveScale, PosWDissolveWidth, MaskScale, PosWDissolveColor, useTop2Bottom);
                            }
                        }
                        // 更新脸部朝向
                        if (m_RoleFaceObj != null)
                        {
                            for (int i = 0; i < child.sharedMaterials.Length; i++)
                            {
                                child.sharedMaterials[i].SetVector(ShaderIDs.FaceForwardVector, m_RoleFaceObj.transform.forward);
                            }
                        }
                        // 更新角色点阵化透明
                        UpdateMaskTransparent(child.sharedMaterials, PerCharacterMaskTransparentState, MaskTransparent);
                        // 更新角色透明
                        UpdateTransparentAlpha(useInvisible, child.sharedMaterials, ColorAlpha);
                        //  更新FOV控制
                        UpdateCameraFOV(child.sharedMaterials, mainCamera);
                        //  更新Unity雾
                        UpdateUnityFogEnable(child.sharedMaterials, useUnityFog);
                    }
                }
            }
            else //没有运行 使用propertyBlock  但是这玩意会打断SRP Batching
            {
                if (child != null && propertyBlock != null)
                {
                    child._renderer.GetPropertyBlock(propertyBlock);

                    if (_characterType == CharacterType.Enemy || _characterType == CharacterType.HumanEnemy)
                    {
                        // 更新敌人描边宽度
                        UpdateEnemyOutLine(propertyBlock,useCustomOutLineWidth,CustomOutLineWidth);
                        // 更新边缘光颜色
                        UpdateEnemyRimColor(useCustomRimLightColor,propertyBlock,CustomRimLightColor);
                        // 更新敌人灯光参数
                        UpdateEnemyLightParams(propertyBlock, LightStrength, LightColor, AddColor, UseWhiteBalance, UseAddColor, UseAdditionLight && GlobalConfig.useAdditionalLight);
                        // 更新冰冻效果
                        UpdateIceVFX(propertyBlock, useIceEffect, IceColor, IceNormalScale, IceRimColor, IceFresnelFeather, IceFresnelStep, IceSpecColor, IceSpecPower);
                        // 更新敌人透明
                        UpdateTransparentAlpha(useInvisible, propertyBlock, ColorAlpha);
                        // 更新敌人点阵化透明
                        UpdateMaskTransparent(propertyBlock, PerCharacterMaskTransparentState, MaskTransparent);
                        //  更新FOV控制
                        UpdateCameraFOV(propertyBlock, mainCamera);
                        //  更新Unity雾
                        UpdateUnityFogEnable(propertyBlock, useUnityFog);
                        // 更新脸部朝向
                        if (m_RoleFaceObj != null && _characterType == CharacterType.HumanEnemy)
                        {
                            propertyBlock.SetVector(ShaderIDs.FaceForwardVector, m_RoleFaceObj.transform.forward);
                        }
                        // 提交材质块
                        child._renderer.SetPropertyBlock(propertyBlock);
                    }
                    else
                    {
                        // 更新角色卡通灯光
                        UpdateToonLight(propertyBlock, useCameraLight, mainCamera, cameraLightForward, LightStrength);
                        // 更新角色描边
                        UpdateRoleOutLine(child.isFace,propertyBlock, isShowScene, useCustomOutLineWidth,CustomOutLineWidth);
                        // 更新暗部亮度控制
                        UpdateRoleDarkLuminance(propertyBlock,UseDarkLuminance,DarkLuminance);
                        // 更新屏幕空间边缘光
                        UpdateRoleRimScale(useCustomRimLightWidth,propertyBlock,CustomRimLightWidth);
                        // 更新边缘光颜色
                        UpdateRoleRimColor(useCustomRimLightColor,propertyBlock,CustomRimLightColor);
                        // 更新角色灯光
                        UpdateRoleLightParams(propertyBlock, LightColor, AddColor, UseWhiteBalance, UseAddColor, UseAdditionLight && GlobalConfig.useAdditionalLight, addShadowRangeStep, addShadowFeather);
                        // 更新冰冻效果
                        UpdateIceVFX(propertyBlock, useIceEffect, IceColor, IceNormalScale, IceRimColor, IceFresnelFeather, IceFresnelStep, IceSpecColor, IceSpecPower);
                        //  更新脸部冰冻
                        UpdateFaceIceQuad(propertyBlock, useFaceIceQuad, IceQuadDensity, IceQuadDissolveScale);
                        // 更新世界坐标剔除
                        if(m_dissolvePart == DissolvePart.All)
                            UpdateCharacterClip(child.sharedMaterials,UsePosWDissolve, PosWDissolveScale, PosWDissolveWidth, MaskScale, PosWDissolveColor, useTop2Bottom);
                        else
                        {
                            if (child.isWeapon)
                            {
                                UpdateCharacterClip(child.sharedMaterials,UsePosWDissolve, PosWDissolveScale, PosWDissolveWidth, MaskScale, PosWDissolveColor, useTop2Bottom);
                            }
                        }
                        // 更新流光
                        if(TestAdditionFresnel)
                            SetAdditionFresnelTest(propertyBlock,UseAdditionFresnel,rimColor,FresnelRange,FresnelFeather,showRimScale);
                        // 更新脸部朝向
                        if (m_RoleFaceObj != null)
                        {
                            propertyBlock.SetVector(ShaderIDs.FaceForwardVector, m_RoleFaceObj.transform.forward);
                        }
                        // 更新角色点阵化透明
                        UpdateMaskTransparent(propertyBlock, PerCharacterMaskTransparentState, MaskTransparent);
                        // 更新角色透明
                        UpdateTransparentAlpha(useInvisible, propertyBlock, ColorAlpha);
                        //  更新FOV控制
                        UpdateCameraFOV(propertyBlock, mainCamera);
                        //  更新Unity雾
                        UpdateUnityFogEnable(propertyBlock, useUnityFog);
                        child._renderer.SetPropertyBlock(propertyBlock);
                    }
                }   
            }
        }
        //  更新RenderScale
        UpdatMaskTransparentScale(mainCameraData);
        //  角色前发投影
        SetCharacterCelHairShadow(_characterType,useHairShadow);
        //  更新阴影
        SetShadow(useShadow);

        #region 客户端加 K帧 武器出现
        //由编辑后存储的json数据控制,所以这里注释掉 --WG
        //SetWeaponShine(useWeaponShine, showObjectScale, weaponRimScale,
        //    weaponRimColor, weaponDissolveWdith, weaponEdgeColor);
        #endregion
    }

    /// <summary>
    /// 初始化设置
    /// </summary>
    private void InitStartSettings()
    {
        //关闭实时阴影
        // Shader.DisableKeyword("RECEIVE_SHADOW");
        // Shader.DisableKeyword("WORLD_SPACE_SAMPLE_SHADOW"); 
        
        //  设置角色物体
        RoleObject = this.gameObject;

        //  设置灯光和相机灯光
        if (mainCamera != null)
        {
            SetShaderLight();
            CameraLight.transform.parent = this.transform;
        }
        //  材质表清除
        MaterialList.Clear();
        
        //  Editor下使用MPB
        if (Application.isEditor)
        {
            propertyBlock = new MaterialPropertyBlock();   
        }

        if (this.GetComponent<Renderer>() != null)
        {
            CharacterRender characterRender = new CharacterRender();
            characterRender._renderer = this.GetComponent<Renderer>();
            characterRender.sharedMaterial = this.GetComponent<Renderer>().sharedMaterial;
            characterRender.sharedMaterials = this.GetComponent<Renderer>().sharedMaterials;
            if(this.name.Contains("Body"))
                characterRender.isBody = true;
            else
                characterRender.isBody = false;

            if (this.name.Contains("Face"))
                characterRender.isFace = true;
            else
                characterRender.isFace = false;
                
            if(this.name.Contains("Hair"))
                characterRender.isHair = true;
            else
                characterRender.isHair = false;
                
            if (this.name.Contains("Weapon"))
                characterRender.isWeapon = true;
            else
                characterRender.isWeapon = false;

            if (this.name.Contains("Eye"))
                characterRender.isEye = true;
            else
                characterRender.isEye = false;
                
            MaterialList.Add(characterRender);
        }
        
        //  筛选出每个物体对应的部位
        foreach (Transform child in RoleObject.transform)
        {
            if (child.GetComponent<Renderer>() != null)
            {
                CharacterRender characterRender = new CharacterRender();
                characterRender._renderer = child.GetComponent<Renderer>();
                characterRender.sharedMaterial = child.GetComponent<Renderer>().sharedMaterial;
                characterRender.sharedMaterials = child.GetComponent<Renderer>().sharedMaterials;
                
                if(child.name.Contains("Body"))
                    characterRender.isBody = true;
                else
                    characterRender.isBody = false;

                if (child.name.Contains("Face"))
                    characterRender.isFace = true;
                else
                    characterRender.isFace = false;
                
                if(child.name.Contains("Hair"))
                    characterRender.isHair = true;
                else
                    characterRender.isHair = false;
                
                if (child.name.Contains("Weapon"))
                    characterRender.isWeapon = true;
                else
                    characterRender.isWeapon = false;

                if (child.name.Contains("Eye"))
                    characterRender.isEye = true;
                else
                    characterRender.isEye = false;
                
                MaterialList.Add(characterRender);
            }
        }
        
        //  设置阴影初始化
        SetShadow(useShadow);
    }


    /// <summary>
    /// 更新角色数据前检查
    /// </summary>
    private void JudgeStratSettings()
    {
        if(RoleObject == null)
            RoleObject = this.gameObject;
        
        if(mainCamera == null || mainCamera.gameObject.activeSelf == false)
            GetMainCamera();

        if (CameraLight == null)
        {
            SetShaderLight();
            CameraLight.transform.parent = this.transform;
        }
    }

    /// <summary>
    /// 获取主相机
    /// </summary>
    private void GetMainCamera()
    {
        if (mainCamera == null || mainCamera.gameObject.activeSelf == false)
            mainCamera = Camera.main;

        if (mainCameraData == null && mainCamera != null)
            mainCameraData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
    }

    /// <summary>
    /// 设置角色相机光照
    /// </summary>
    private void SetShaderLight()
    {
        foreach (Transform child in RoleObject.transform)
        {
            if (string.Equals(child.name, "CameraLight"))
            {
                CameraLight = child.gameObject;
                return;
            }
        }
        CameraLight = new GameObject("CameraLight");
    }

    /// <summary>
    /// 更新延迟阴影
    /// </summary>
    private void RotateCameraLight()
    {
        if (mainCamera != null && useCameraLight)
        {
            if (CameraLight != null)
            {
                //CameraLight.transform.eulerAngles = new Vector3(0,CameraLightRotXZ,0);
                //float3x3 QuadRotatMatrix = ShaderUtils.GetQuadRotatMatrix(CameraLight, mainCamera);
                //Vector3 X1 = ShaderUtils.RotatQuad(QuadRotatMatrix, CameraLight.transform.position, new Vector3(0, 0, -1));
                //Vector3 X2 = ShaderUtils.RotatQuad(QuadRotatMatrix, CameraLight.transform.position, new Vector3(0, 0, 1));
                //Tween tweenX = DOTween.To(() => cameraLightForward.x, x => cameraLightForward.x = x, (X2 - X1).normalized.x, 0.9f);
                //Tween tweenY = DOTween.To(() => cameraLightForward.y, x => cameraLightForward.y = x, CameraLightRotY, 0.9f);
                //Tween tweenZ = DOTween.To(() => cameraLightForward.z, x => cameraLightForward.z = x, (X2 - X1).normalized.z, 0.9f);
            }
        }
    }

    /// <summary>
    /// 运行时实例化角色材质
    /// </summary>
    public void InitCharacterMaterial()
    {
        //  设置角色溶解图片
        Texture2D dissolve = Resources.Load<Texture2D>(ShaderIDs.dissolveTex);
        Texture2D iceTex = Resources.Load<Texture2D>(ShaderIDs.IceTexPath);
        Texture2D normal = Resources.Load<Texture2D>(ShaderIDs.IceNormPath);

        if (Application.isPlaying)
        {
            foreach (CharacterRender child in MaterialList)
            {
                if (child._renderer.sharedMaterials.Length > 0)
                {
                    for (int i = 0; i < child._renderer.sharedMaterials.Length; i++)
                    {
                        Material material_new = Instantiate(child._renderer.sharedMaterials[i]);
                        material_new.name = child._renderer.sharedMaterials[i].name;
                        child._renderer.materials[i] = material_new;
                        
                        if(dissolve != null)
                            child._renderer.materials[i].SetTexture(ShaderIDs.NoiseTex,dissolve);
                        
                        if(iceTex != null)
                            child._renderer.materials[i].SetTexture(ShaderIDs.IceTex,iceTex);
                        
                        if(normal != null)
                            child._renderer.materials[i].SetTexture(ShaderIDs.NormalMap,normal);
                    }
                }

                child.sharedMaterial = child._renderer.sharedMaterial;
                child.sharedMaterials = child._renderer.sharedMaterials;
            }
        }
    }

    /// <summary>
    /// 设置角色材质分级    HSV /   SDF AO  /   多光源    /    点阵化透明
    /// </summary>
    public void SetCharacterMaterialLevel()
    {
        switch (GlobalConfig.CurrentRenderLevel)
        {
            //  全开
            case RenderLevel.Acme:
                //--------------    开启      -----------------
                //HSV 
                GlobalConfig.useCharacterHSV = true;
                //SDF AO
                GlobalConfig.useSdfAO = true;
                //多光源
                GlobalConfig.useAdditionalLight = true;
                //点阵化透明
                GlobalConfig.useTransparentMask = true;
                break;
            
            //  HSV控制关闭、SDF AO关闭
            case RenderLevel.High:
                
                //--------------    关闭      -----------------
                //HSV 
                GlobalConfig.useCharacterHSV = false;
                //SDF AO
                useSDFAO = false;
                GlobalConfig.useSdfAO = false;
                
                //--------------    开启      -----------------
                //多光源
                GlobalConfig.useAdditionalLight = true;
                //点阵化透明
                GlobalConfig.useTransparentMask = true;

                break;
            
            //  HSV控制关闭、SDF AO关闭、丝袜材质关闭、多光源关闭、阴影Ramp关闭（使用最后一阶阴影色 类似怪物材质）、边缘光Ramp关闭（使用最后一阶边缘光色）、金属高光换普通高光
            case RenderLevel.Middle:
                
                //--------------    关闭      -----------------
                //HSV
                GlobalConfig.useCharacterHSV = false;
                //SDF AO
                useSDFAO = false;
                GlobalConfig.useSdfAO = false;
                //多光源
                UseAdditionLight = false;
                GlobalConfig.useAdditionalLight = false;

                //--------------    开启      -----------------
                //点阵化透明
                GlobalConfig.useTransparentMask = true;
                break;
            
            //  HSV控制关闭、SDF AO关闭、丝袜材质关闭、多光源关闭、阴影Ramp关闭（使用最后一阶阴影色 类似怪物材质）、边缘光Ramp关闭（使用最后一阶边缘光色）、金属高光换普通高光、接触阴影关闭、点阵化透明关闭、角色冰冻关闭
            case RenderLevel.Low:
                
                //--------------    关闭      -----------------
                //HSV
                GlobalConfig.useCharacterHSV = false;
                //SDF AO
                useSDFAO = false;
                GlobalConfig.useSdfAO = false;
                //多光源
                UseAdditionLight = false;
                GlobalConfig.useAdditionalLight = false;
                //点阵化透明
                PerCharacterMaskTransparentState = false;
                GlobalConfig.useTransparentMask = false;
                break;
        }
    }

    /// <summary>
    /// 设置AlphaClip
    /// </summary>
    /// <param name="state"></param>
    public void SetAlphaClip(bool state)
    {
        foreach (CharacterRender child in MaterialList)
        {
            if (state)
                child.sharedMaterial.SetFloat(ShaderIDs.UseAlphaClip,1);
            else
                child.sharedMaterial.SetFloat(ShaderIDs.UseAlphaClip,0);
        }   
    }
    
    /// <summary>
    /// 设置纹理插值变化
    /// </summary>
    /// <param name="dissolve_strength"></param>
    /// <param name="edge_scale"></param>
    /// <param name="edge_color"></param>
    public void SetMainTextureLerp(float dissolve_strength)
    {
        foreach (CharacterRender child in MaterialList)
        {
            child.sharedMaterial.SetFloat(ShaderIDs.DissolveStrength,dissolve_strength);
        }
    }
    
    /// <summary>
    /// 敌人 透明 加载换shader
    /// </summary>
    public async void SetEnemyTransparentState(bool state)
    {
        //  如果不是敌人就返回
        if (_characterType == CharacterType.Player)
            return;

        //  未完成初始化返回
        //while (!StartEnd)
        //    await new WaitForEndOfFrame();
        
        //  是否开启透明
        if (state)
        {
            //  使用透明
            useInvisible = true;
            
            //  替换材质shader
            foreach (CharacterRender child in MaterialList)
            {
                /*if (child.sharedMaterial.shader.name.Equals(ShaderIDs.simple_name))
                {
                    Shader shader = await ShaderUtils.LoadShaderAtPath(ShaderIDs.simple_transparent_path);
                    child.sharedMaterial.shader = shader;
                }

                if (child.sharedMaterial.shader.name.Equals(ShaderIDs.eye_name))
                {
                    Shader shader = await ShaderUtils.LoadShaderAtPath(ShaderIDs.eye_transparent_path);
                    child.sharedMaterial.shader = shader;
                }*/
            }   
        }
        else
        {
            //  不使用透明
            useInvisible = false;
            
            //  替换材质shader
            foreach (CharacterRender child in MaterialList)
            {
                /*if (child.sharedMaterial.shader.name.Equals(ShaderIDs.simple_transparent_name))
                {
                    Shader shader = await ShaderUtils.LoadShaderAtPath(ShaderIDs.simple_path);
                    child.sharedMaterial.shader = shader;
                }

                if (child.sharedMaterial.shader.name.Equals(ShaderIDs.eye_transparent_name))
                {
                    Shader shader = await ShaderUtils.LoadShaderAtPath(ShaderIDs.eye_path);
                    child.sharedMaterial.shader = shader;
                }*/
            }
        }
    }
    
    /// <summary>
    /// 角色 透明 加载换shader
    /// </summary>
    public async void SetCharacterTransparentState(bool state)
    {
        //  如果不是敌人就返回
        if (_characterType != CharacterType.Player)
            return;
    
        //  未完成初始化返回
        //while (!StartEnd)
        //    await new WaitForEndOfFrame();

        //  是否开启透明
        if (state)
        {
            //  使用透明
            useInvisible = true;
            
            //  替换材质shader
            foreach (CharacterRender child in MaterialList)
            {
                /*if (child.sharedMaterial.shader.name.Equals(ShaderIDs.bard_character_face_name))
                {
                    Shader shader = await ShaderUtils.LoadShaderAtPath(ShaderIDs.bard_character_face_transparent_path);
                    child.sharedMaterial.shader = shader;
                }

                if (child.sharedMaterial.shader.name.Equals(ShaderIDs.bard_character_hair_name))
                {
                    Shader shader = await ShaderUtils.LoadShaderAtPath(ShaderIDs.bard_character_hair_transparent_path);
                    child.sharedMaterial.shader = shader;
                }
 
                if (child.sharedMaterial.shader.name.Equals(ShaderIDs.bard_character_body_name))
                {
                    Shader shader = await ShaderUtils.LoadShaderAtPath(ShaderIDs.bard_character_body_transparent_path);
                    child.sharedMaterial.shader = shader;
                }*/

                SetCharacterTranspatentRimCtrl(state, ref useCustomRimLightWidth, child.sharedMaterial);
            } 
        }
        else
        {
            //  不使用透明
            useInvisible = false;
            
            //  替换材质shader
            foreach (CharacterRender child in MaterialList)
            {
                /*if (child.sharedMaterial.shader.name.Equals(ShaderIDs.bard_character_face_transparent_name))
                {
                    Shader shader = await ShaderUtils.LoadShaderAtPath(ShaderIDs.bard_character_face_path);
                    child.sharedMaterial.shader = shader;
                }

                if (child.sharedMaterial.shader.name.Equals(ShaderIDs.bard_character_hair_transparent_name))
                {
                    Shader shader = await ShaderUtils.LoadShaderAtPath(ShaderIDs.bard_character_hair_path);
                    child.sharedMaterial.shader = shader;
                }

                if (child.sharedMaterial.shader.name.Equals(ShaderIDs.bard_character_body_transparent_name))
                {
                    Shader shader = await ShaderUtils.LoadShaderAtPath(ShaderIDs.bard_character_body_path);
                    child.sharedMaterial.shader = shader;
                }*/
                
                SetCharacterTranspatentRimCtrl(state, ref useCustomRimLightWidth, child.sharedMaterial);
            }
        }
    }
}

