using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using Bard;
using Unity.Mathematics;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
public class ShaderFunctions : MonoBehaviour
{
    //PS曲线数据
    public const double a = -1.16328368895563;
    public const double b = 2.53985820633175;
    public const double c = -0.311015009604814;
    public const double d = 0.0462274924862326;
    public const double e = -0.00339947674473466;
    public const double f = 0.000140838891870456;
    public const double g = -3.4952828816409E-06;
    public const double h = 5.29914548081728E-08;
    public const double i = -4.80264723388739E-10;
    public const double j = 2.38775362814726E-12;
    public const double k = -5.00522708740458E-15;


        
    private const byte k_MaxByteForOverexposedColor = 191;
    public static void DecomposeHdrColor(Color linearColorHdr, out Color baseLinearColor, out float exposure)
    {
        baseLinearColor = linearColorHdr;
        var maxColorComponent = linearColorHdr.maxColorComponent;
        // replicate Photoshops's decomposition behaviour
        if (maxColorComponent == 0f || maxColorComponent <= 1f && maxColorComponent >= 1 / 255f)
        {
            exposure = 0f;
            baseLinearColor.r = (byte)Mathf.RoundToInt(linearColorHdr.r * 255f);
            baseLinearColor.g = (byte)Mathf.RoundToInt(linearColorHdr.g * 255f);
            baseLinearColor.b = (byte)Mathf.RoundToInt(linearColorHdr.b * 255f);
        }
        else
        {
            // calibrate exposure to the max float color component
            var scaleFactor = k_MaxByteForOverexposedColor / maxColorComponent;
            exposure = Mathf.Log(255f / scaleFactor) / Mathf.Log(2f);
            // maintain maximal integrity of byte values to prevent off-by-one errors when scaling up a color one component at a time
            baseLinearColor.r = Math.Min(k_MaxByteForOverexposedColor, (byte)Mathf.CeilToInt(scaleFactor * linearColorHdr.r));
            baseLinearColor.g = Math.Min(k_MaxByteForOverexposedColor, (byte)Mathf.CeilToInt(scaleFactor * linearColorHdr.g));
            baseLinearColor.b = Math.Min(k_MaxByteForOverexposedColor, (byte)Mathf.CeilToInt(scaleFactor * linearColorHdr.b));
        }
    }
    
    /// <summary>
    /// 关闭CharacterStencil
    /// </summary>
    public static async void InitRendererFeature()
    {
        if (ShaderUtils.GlobalPostprocessingData != null)
        {
            ShaderUtils.GlobalPostprocessingData.rendererFeatures[(int)ShaderIDs.RendererFeatureIndex.CharacterStencil].SetActive(false);
        }
    }

    /// <summary>
    /// 获取PS的变化曲线
    /// </summary>
    /// <param name="X"></param>
    /// <returns></returns>
    public static float GetPsAlphaCurves(float X)
    {
        double outAlpha = a + b * X + c * Math.Pow(X, 2) + d * Math.Pow(X, 3) + e * Math.Pow(X, 4) + f * Math.Pow(X, 5) + g * Math.Pow(X, 6) + h * Math.Pow(X, 7) + i * Math.Pow(X, 8) + j * Math.Pow(X, 9) + k * Math.Pow(X, 10);
        return (float)outAlpha;
    }

    /// <summary>
    /// 设置深度法线开启状态
    /// </summary>
    /// <param name="state"></param>
    public static void SetDepthNormalState(bool state)
    {
        if (ShaderUtils.GlobalAsset == null)
            ShaderUtils.GlobalAsset = ShaderUtils.GetGlobalAsset();

        //if (state)
        //    ShaderUtils.GlobalAsset.supportsDepthNormalTexture = true;
        //else
        //    ShaderUtils.GlobalAsset.supportsDepthNormalTexture = false;
    }
    /// <summary>
    /// 设置角色屏幕空间边缘光
    /// </summary>
    /// <param name="state"></param>
    public static void SetCharacterScreenSpaceRimLight(bool state)
    {
        if (state)
            GlobalConfig.useSSRimLight = true;
        else
            GlobalConfig.useSSRimLight = false;
    }

    /// <summary>
    /// 设置角色阴影状态
    /// </summary>
    /// <param name="state"></param>
    public static void SetCharacterShadowState(bool state)
    {
        if (state)
            GlobalConfig.CharacterShadowIndex = 0;
        else
            GlobalConfig.CharacterShadowIndex = 1;
    }

    /// <summary>
    /// 设置角色点阵化开启与否
    /// </summary>
    /// <param name="state"></param>
    public static void SetCharacterTransparentMask(bool state)
    {
        if (state)
            GlobalConfig.useTransparentMask = true;
        else
            GlobalConfig.useTransparentMask = false;
    }

    /// <summary>
    /// 设置实时反射开启与否
    /// </summary>
    /// <param name="state"></param>
    public static void SetRealTimeReflection(bool state)
    {
        if (state)
            GlobalConfig.useRealTimeReflection = true;
        else
            GlobalConfig.useRealTimeReflection = false;
    }
    
    /// <summary>
    /// SetLightingSettings
    /// </summary>
    public static async void SetLightingSettings(LightingData data)
    {
        if(data==null)
            return;
        
        //加载天空盒材质
        Material skyMat;
#if UNITY_EDITOR  
        skyMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(data.skyboxPath);
#else
        skyMat = await AddressablesManager.LoadAssetsAsync<Material>(data.skyboxPath);
#endif
        SetLightingMaterial(data,skyMat);
    }
    public static async void SetLightingMaterial(LightingData data,Material skyMat)
    {
        if (skyMat != null)
        {
            RenderSettings.skybox = skyMat;
        }

        //加载灯光
        GameObject sun = GameObject.Find(data.SunSource);
        if (sun)
        {
            Light sunLight = sun.GetComponent<Light>();
            if (sunLight)
            {
                RenderSettings.sun = sunLight;
            }
        }

        //加载环境光颜色
        RenderSettings.ambientMode = (AmbientMode)data.SourceIndex;
        RenderSettings.ambientIntensity = data.IntensityMultiplie;
        
        RenderSettings.ambientLight = new Color(data.Color.x, data.Color.y, data.Color.z);
        
        Color skyColor = new Color(data.skyColor.x, data.skyColor.y, data.skyColor.z);
        Color EquatorColor = new Color(data.EquatorColor.x, data.EquatorColor.y, data.EquatorColor.z);
        Color GroundColor = new Color(data.GroundColor.x, data.GroundColor.y, data.GroundColor.z);
        RenderSettings.ambientSkyColor = skyColor;
        RenderSettings.ambientEquatorColor = EquatorColor;
        RenderSettings.ambientGroundColor = GroundColor;

        //加载环境反射
        if (data.reflectSourceName.Equals("Custom"))
        {
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        }
        else
        {
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
        }
        RenderSettings.defaultReflectionResolution = (int)data.Resolution;
        RenderSettings.reflectionIntensity = data.RefIntensityMultiplier;
        RenderSettings.reflectionBounces = data.EnvBounces;

        //加载雾效设置
        RenderSettings.fog = data.Fog;
        Color fogColor = new Color(data.color.x, data.color.y, data.color.z);
        RenderSettings.fogColor = fogColor;
        if (data.Mode.Equals("Linear"))
        {
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = data.Start;
            RenderSettings.fogEndDistance = data.End;
        }
        else if (data.Mode.Equals("Exponential"))
        {
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = data.Density;
        }
        else
        {
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = data.Density;
        }
    }

    /// <summary>
    /// 获取投影矩阵
    /// </summary>
    /// <param name="fov"></param>
    /// <param name="aspect"></param>
    /// <param name="NearClip"></param>
    /// <param name="FarClip"></param>
    /// <returns></returns>
    public static Matrix4x4 CreateProjMatrix(float fov, float aspect, float NearClip, float FarClip)
    {
        Matrix4x4 PerspectiveMatrix = GL.GetGPUProjectionMatrix(createPerspectiveCamera(fov, aspect, NearClip, FarClip), false);
        return PerspectiveMatrix;
    }

    /// <summary>
    /// 获取视口矩阵
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="lookAt"></param>
    /// <returns></returns>
    public static Matrix4x4 CreateViewMatrix(Vector3 eye, Vector3 lookAt)
    {
        Vector3 dir = -lookAt.normalized;
        Vector3 N1 = Vector3.Cross(dir, Vector3.up).normalized;
        Vector3 N2 = Vector3.Cross(N1, dir);

        Matrix4x4 ViewMatrix = new Matrix4x4();
        ViewMatrix.m00 = N1.x; ViewMatrix.m01 = N1.y; ViewMatrix.m02 = N1.z; ViewMatrix.m03 = -Vector3.Dot(N1, eye);
        ViewMatrix.m10 = N2.x; ViewMatrix.m11 = N2.y; ViewMatrix.m12 = N2.z; ViewMatrix.m13 = -Vector3.Dot(N2, eye);
        ViewMatrix.m20 = dir.x; ViewMatrix.m21 = dir.y; ViewMatrix.m22 = dir.z; ViewMatrix.m23 = -Vector3.Dot(dir, eye);
        ViewMatrix.m30 = 0; ViewMatrix.m31 = 0; ViewMatrix.m32 = 0; ViewMatrix.m33 = 1;

        return ViewMatrix;
    }

    /// <summary>
    /// 反的投影矩阵
    /// </summary>
    /// <param name="fov"></param>
    /// <param name="aspect"></param>
    /// <param name="near"></param>
    /// <param name="far"></param>
    /// <returns></returns>
    public static float4x4 CreatePerspectiveCameraInv(float fov, float aspect, float near, float far)
    {
        float tanHalfFovy = math.tan(Mathf.Deg2Rad * fov / 2);

        float4x4 mat = new float4x4(
            1 / (aspect * tanHalfFovy), 0, 0, 0,
            0, (1 / tanHalfFovy), 0, 0,
            0, 0, -((far + near) / (far - near)), 2 * far * near / (near - far),
            0, 0, -1, 0
        );
        return mat;
    }

    /// <summary>
    /// 投影矩阵
    /// </summary>
    /// <param name="fov"></param>
    /// <param name="aspect"></param>
    /// <param name="near"></param>
    /// <param name="far"></param>
    /// <returns></returns>
    public static float4x4 createPerspectiveCamera(float fov, float aspect, float near, float far)
    {
        float tanHalfFovy = math.tan(Mathf.Deg2Rad * fov / 2);

        float4x4 mat = new float4x4(
            1 / (aspect * tanHalfFovy), 0, 0, 0,
            0, -(1 / tanHalfFovy), 0, 0,
            0, 0, -((far + near) / (far - near)), 2 * far * near / (near - far),
            0, 0, -1, 0
        );
        return mat;
    }

    /// <summary>
    /// 归一化
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static Vector4 Homogenize(float width, float height, Vector4 pos)
    {
        float rhw = 1 / width;
        Vector4 outPos;
        outPos.x = (pos.x * rhw + 1) * width * 0.5f;
        outPos.y = (1.0f - pos.y * rhw) * height * 0.5f;
        outPos.z = pos.z * rhw;
        outPos.w = 1.0f;
        return outPos;
    }

    /// <summary>
    /// 创建NDC空间矩阵
    /// </summary>
    /// <returns></returns>
    public static Matrix4x4 CreateNDCMatrix(float width, float height)
    {

        float4x4 mat = new float4x4(
            width / 2, 0, 0, width / 2,
            -height / 2, 0, 0, height / 2,
            0, 0, 1, 0,
            0, 0, 0, 0
        );

        return mat;
    }

    /// <summary>
    /// 获取RenderScale
    /// </summary>
    /// <returns></returns>
    public static float GetRenderScale()
    {
        if (ShaderUtils.GlobalAsset == null)
            ShaderUtils.GlobalAsset = ShaderUtils.GetGlobalAsset();

        return ShaderUtils.GlobalAsset.renderScale;
    }

    /// <summary>
    /// 正交相机投影矩阵
    /// </summary>
    /// <param name="ViewLeft"></param>
    /// <param name="ViewRight"></param>
    /// <param name="ViewBottom"></param>
    /// <param name="ViewTop"></param>
    /// <param name="NearZ"></param>
    /// <param name="FarZ"></param>
    /// <returns></returns>
    public static float4x4 XMMatrixOrthographicOffCenterLH(float ViewLeft, float ViewRight, float ViewBottom, float ViewTop, float NearZ, float FarZ)
    {
        float ReciprocalWidth = 1.0f / (ViewRight - ViewLeft);
        float ReciprocalHeight = 1.0f / (ViewTop - ViewBottom);
        float fRange = 1.0f / (FarZ-NearZ);

        float4x4 M = new float4x4(
            ReciprocalWidth + ReciprocalWidth,             0,                                                      0,                    0,
            0,                                             ReciprocalHeight + ReciprocalHeight,                    0,                    0,
            0,                                             0,                                                      fRange,               0,
            -(ViewLeft + ViewRight) * ReciprocalWidth,     -(ViewTop + ViewBottom) * ReciprocalHeight,             -fRange * NearZ,      1
            );

        return M;
    }
    
    /// <summary>
    /// GetRHW
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector4 GetRHW(Vector4 v)
    {
        float rhw = 1 / v.w;
        return new Vector4(v.x * rhw, v.y * rhw, v.z * rhw, 1);
    }
    
    /// <summary>
    /// GetProjectViewport
    /// </summary>
    /// <param name="nc1"></param>
    /// <returns></returns>
    public static Vector4 GetProjectViewport(Vector4 nc1, float ScaleFactory)
    {
        float width = Screen.width / ScaleFactory;
        float height = Screen.height / ScaleFactory;
        
        Vector4 outVector4 = Vector4.one;
        outVector4.x = (nc1.x + 1.0f) * width * 0.5f;
        outVector4.y = height - (1.0f - nc1.y) * height * 0.5f;
        outVector4.z = nc1.z;
        outVector4.w = 1.0f;
        return outVector4;
    }

    /// <summary>
    /// GetOrthScreenPosInPres
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="rect"></param>
    /// <param name="canvas"></param>
    /// <param name="vCam"></param>
    /// <returns></returns>
    /*public static Vector2 GetOrthScreenPosInPres_Battle(VirtualPrespectiveCamera vCam, RectTransform rect, Canvas canvas)
    {
        Vector3 pos = vCam.GetComponent<Camera>().ScreenToWorldPoint(rect.anchoredPosition);
        pos.z = canvas.planeDistance;
        
        Matrix4x4 VP = math.mul(vCam.projec, vCam.view);
        Vector4 pos_projection = math.mul(VP, new Vector4(pos.x,pos.y,pos.z,1));
        
        Vector4 PosRhw = GetRHW(pos_projection);
        Vector2 PosScreen = GetProjectViewport(PosRhw,1);
        return PosScreen;
    }*/

    /// <summary>
    /// 获取Canvas ScaleFactory
    /// </summary>
    /// <param name="mCanvas"></param>
    /// <param name="m_ReferenceResolution"></param>
    /// <param name="m_MatchWidthOrHeight"></param>
    /// <returns></returns>
    public static float GetScaleFactory(Canvas mCanvas, Vector2 m_ReferenceResolution, float m_MatchWidthOrHeight)
    {
        int kLogBase = 2;
        Vector2 screenSize = mCanvas.renderingDisplaySize;
        int displayIndex = mCanvas.targetDisplay;
        if (displayIndex > 0 && displayIndex < Display.displays.Length)
        {
            Display disp = Display.displays[displayIndex];
            screenSize = new Vector2(disp.renderingWidth, disp.renderingHeight);
        }
    
        float scaleFactor = 0;
        float logWidth = Mathf.Log(screenSize.x / m_ReferenceResolution.x, kLogBase);
        float logHeight = Mathf.Log(screenSize.y / m_ReferenceResolution.y, kLogBase);
        float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, m_MatchWidthOrHeight);
        scaleFactor = Mathf.Pow(kLogBase, logWeightedAverage);
        return scaleFactor;
    }
    
    /// <summary>
    /// GetOrthScreenPosInPres
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="rect"></param>
    /// <param name="canvas"></param>
    /// <param name="vCam"></param>
    /// <returns></returns>
    /*public static Vector2 GetOrthScreenPosInPres(VirtualPrespectiveCamera vCam, Vector3 WorldPos, float ScaleFactory = 1)
    {
        Matrix4x4 VP = math.mul(vCam.projec, vCam.view);
        Vector4 pos_projection = math.mul(VP, new Vector4(WorldPos.x,WorldPos.y,WorldPos.z,1));
        
        Vector4 PosRhw = GetRHW(pos_projection);
        Vector2 PosScreen = GetProjectViewport(PosRhw, ScaleFactory);
        return PosScreen;
    }*/
    
    /// <summary>
    /// XMMatrixScaling
    /// </summary>
    /// <param name="ScaleX"></param>
    /// <param name="ScaleY"></param>
    /// <param name="ScaleZ"></param>
    /// <returns></returns>
    public static Matrix4x4 XMMatrixScaling
    (
        float ScaleX,
        float ScaleY,
        float ScaleZ
    )
     {
         Matrix4x4 M = new Matrix4x4(
             new Vector4(0, 0, 0, ScaleX),
             new Vector4(0, 0, ScaleY, 0),
             new Vector4(0, ScaleZ, 0, 0),
             new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
         
         return M;
    }
    
    /// <summary>
    /// XMMatrixTranslation
    /// </summary>
    /// <param name="OffsetX"></param>
    /// <param name="OffsetY"></param>
    /// <param name="OffsetZ"></param>
    /// <returns></returns>
    public static Matrix4x4 XMMatrixTranslation
    (
        float OffsetX,
        float OffsetY,
        float OffsetZ
    )
    {

        Matrix4x4 M = new Matrix4x4(
            new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
            new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            new Vector4(0.0f, 0.0f, 1.0f, 0.0f),
            new Vector4(OffsetX, OffsetY, OffsetZ, 1.0f));

        return M;
    }

    /// <summary>
    /// 获取正交相机的投影矩阵
    /// </summary>
    /// <param name="xMin"></param>
    /// <param name="yMin"></param>
    /// <param name="xMax"></param>
    /// <param name="yMax"></param>
    /// <param name="nearZ"></param>
    /// <param name="farZ"></param>
    /// <returns></returns>
    public static float4x4 CreateOrthographicCameraProjection(float xMin, float yMin, float xMax, float yMax, float nearZ, float farZ)
    {
        float4x4 projection = XMMatrixOrthographicOffCenterLH(xMin, xMax, yMin, yMax, nearZ, farZ);
        return projection;
    }
    
    public static void GetMainCityFaceAngle(RoleShaderManager roleShaderManager, float angle)
    {
        if (roleShaderManager.useCameraLight)
        {
            roleShaderManager.CameraLightRotY = 0;
            
            if(angle >= 165 && angle < 240)                        
            {
                roleShaderManager.CameraLightRotXZ = 0;
            }
            else if(angle >= 240 && angle < 270)                  
            {
                roleShaderManager.CameraLightRotXZ = 45;
            }
            else if(angle >= 270 && angle < 305)                  
            {
                roleShaderManager.CameraLightRotXZ = 85;
            }
            else if(angle >= 305 && angle <= 321)                   
            {
                roleShaderManager.CameraLightRotXZ = 145;
            }
            else if(angle >= 321 && angle <= 360)                     
            {
                roleShaderManager.CameraLightRotXZ = -165;
            }
            else if(angle >= 0 && angle < 96)                     
            {
                roleShaderManager.CameraLightRotXZ = -165;
            }
            else if(angle >= 96 && angle < 115)                  
            {
                roleShaderManager.CameraLightRotXZ = -130;
            }
            else if (angle >= 115 && angle < 147)                    
            {
                roleShaderManager.CameraLightRotXZ = -75;
            }
            else if (angle >= 147 && angle < 165)                    
            {
                roleShaderManager.CameraLightRotXZ = -35;
            }
        }
    }

    // public static T[] GetComponentsInRealChildren<T>(this GameObject go)
    // {
    //     List<T> TList = new List<T>();
    //     TList.AddRange(go.GetComponentInChildren<T>());
    // }
}
