using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;


[ExecuteInEditMode]
public class ShaderLightSetting : MonoBehaviour
{
    [Range(0.01f,5.0f)]
    public float RoleLightStrength = 1;
    
    #region 实时阴影
    // public bool useRealTimeShadow = false;
    // private Camera pcfCam;
    // private bool isInit = false;
    // [SerializeField]
    // [Tooltip("渲染资产.")]
    // public ForwardRendererData data;
    #endregion

    private void OnEnable()
    {
        //  实时阴影
        // isInit = false;
        Refresh();
    }

    void Start ()
    {
        SetShaderLight();
        // checkRealTimeShadow();
    }

    private void Refresh() 
    {
        SetShaderLight();
        // checkRealTimeShadow();
    }

    void LateUpdate ()
    {
        Refresh();
    }
    
    /// <summary>
    /// 设置角色光照方向
    /// </summary>
    void SetShaderLight()
    {
        Vector3 vecDir = -gameObject.transform.forward;
        Shader.SetGlobalVector("_DirectionLightPos", vecDir.normalized);
        Shader.SetGlobalFloat("_DirectionLightStrength",RoleLightStrength);
    }

    #region 实时阴影
        /// <summary>
    /// 检查是否需要开启实时阴影
    /// </summary>
    // private void checkRealTimeShadow() 
    // {
    //     if (useRealTimeShadow)
    //     {
    //         SetShadowState(true);
    //     }
    //     else
    //     {
    //         SetShadowState(false);
    //     }
    // }

    /// <summary>
    /// 设置实时阴影
    /// </summary>
    /// <param name="state"></param>
    // private void SetShadowState(bool state)
    // {
    //     if (state)
    //     {
    //         if(!data)
    //             data = ShaderUtils.GetGlobalPostprocessingData();
    //     
    //         if (data != null && !data.rendererFeatures[(int)ShaderIDs.RendererFeatureIndex.CharacterShadow].isActive)
    //         {
    //             data.rendererFeatures[(int)ShaderIDs.RendererFeatureIndex.CharacterShadow].SetActive(true);
    //         }
    //         
    //         InitPcfCamera();
    //         SetPcfCamera();
    //         if(pcfCam.enabled == true)
    //             pcfCam.enabled = false;
    //         Shader.SetGlobalFloat(ShaderIDs.useRealTimeShadow,1);
    //     }
    //     else
    //     {
    //         if (pcfCam)
    //         {
    //             DestroyImmediate(this.GetComponent<UniversalAdditionalCameraData>());
    //             DestroyImmediate(pcfCam);
    //             isInit = false;
    //         }
    //         Shader.SetGlobalFloat(ShaderIDs.useRealTimeShadow,0);
    //         
    //         if (data != null && data.rendererFeatures[(int)ShaderIDs.RendererFeatureIndex.CharacterShadow].isActive)
    //         {
    //             data.rendererFeatures[(int)ShaderIDs.RendererFeatureIndex.CharacterShadow].SetActive(false);   
    //         }
    //     }
    // }

    /// <summary>
    /// 初始化pcf相机和rt
    /// </summary>
    // private void InitPcfCamera()
    // {
    //     if (!pcfCam)
    //     {
    //         pcfCam = this.gameObject.GetComponent<Camera>();
    //         if(!pcfCam)
    //             pcfCam = this.gameObject.AddComponent<Camera>();
    //     }
    // }

    /// <summary>
    /// 设置pcf相机
    /// </summary>
    // private void SetPcfCamera()
    // {
    //     if (!isInit)
    //     {
    //         if(!pcfCam)
    //             return;
    //     
    //         //设置为正交相机
    //         pcfCam.orthographic = true;
    //
    //         pcfCam.nearClipPlane = 0.03f;
    //         pcfCam.farClipPlane = 123;
    //         pcfCam.orthographicSize = 1.5f;
    //         pcfCam.cullingMask = LayerMask.GetMask("Role", "RoleHair");
    //         pcfCam.clearFlags = CameraClearFlags.SolidColor;
    //         pcfCam.backgroundColor = Color.black;
    //         
    //         UniversalAdditionalCameraData cameraData = pcfCam.GetComponent<UniversalAdditionalCameraData>();
    //         if (cameraData)
    //         {
    //             cameraData.requiresDepthTexture = false;
    //             cameraData.requiresColorTexture = false;   
    //         }
    //
    //         isInit = true;
    //     }
    // }
    #endregion
}
