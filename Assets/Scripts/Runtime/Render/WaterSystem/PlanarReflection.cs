//#if UNIVERSAL_RENDERER
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

[ExecuteAlways, DisallowMultipleComponent, AddComponentMenu("Water System/Planar Reflections")]
public class PlanarReflection : MonoBehaviour
{
    [Range(0.01f, 1f)]
    public float renderScale = 0.5f;
    public LayerMask reflectionLayer = -1;
    public bool reflectSkybox;
    public GameObject reflectionTarget;
    [Range(-2f, 3f)]
    public float reflectionPlaneOffset;
    public bool hideReflectionCamera;
    
    private static Camera reflectionCamera;
    private UniversalAdditionalCameraData cameraData;
    private static RenderTexture reflectionTexture;
    private RenderTextureDescriptor previousDescriptor;
    private readonly int planarReflectionTextureId = Shader.PropertyToID("_PlanarReflectionTexture");
    public static event Action<ScriptableRenderContext, Camera> OnBeginPlanarReflection;

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += DoPlanarReflection;
        reflectionLayer = ~(1 << 4);
    }

    void OnDisable()
    {
        CleanUp();
        RenderPipelineManager.beginCameraRendering -= DoPlanarReflection;
    }

    void OnDestroy()
    {
        CleanUp();
        RenderPipelineManager.beginCameraRendering -= DoPlanarReflection;
    }

    void CleanUp()
    {
        if (reflectionCamera)
        {
            reflectionCamera.targetTexture = null;
            SafeDestroyObject(reflectionCamera.gameObject);
        }

        if (reflectionTexture)
            RenderTexture.ReleaseTemporary(reflectionTexture);

    }

    void SafeDestroyObject(UnityEngine.Object obj)
    {
        if (Application.isEditor)
            DestroyImmediate(obj);
        else
            Destroy(obj);
    }

    private void UpdateReflectionCamera(Camera realCamera)
    {
        if (reflectionCamera == null)
            reflectionCamera = InitializeReflectionCamera();

        Vector3 pos = Vector3.zero;
        Vector3 normal = Vector3.up;

        if (reflectionTarget != null)
        {
            pos = reflectionTarget.transform.position + Vector3.up * reflectionPlaneOffset;
            normal = reflectionTarget.transform.up;
        }

        CopyCamera(realCamera, reflectionCamera);
        reflectionCamera.gameObject.hideFlags = hideReflectionCamera ? HideFlags.HideAndDontSave : HideFlags.DontSave;
#if UNITY_EDITOR
        EditorApplication.DirtyHierarchyWindowSorting();
#endif

        var d = -Vector3.Dot(normal, pos);
        var reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        var reflection = Matrix4x4.identity;
        reflection *= Matrix4x4.Scale(new Vector3(1, -1, 1));

        CalculateReflectionMatrix(ref reflection, reflectionPlane);
        var oldPosition = realCamera.transform.position - new Vector3(0, pos.y * 2, 0);
        var newPosition = new Vector3(oldPosition.x, -oldPosition.y, oldPosition.z);
        reflectionCamera.transform.forward = Vector3.Scale(realCamera.transform.forward, new Vector3(1, -1, 1));
        reflectionCamera.worldToCameraMatrix = realCamera.worldToCameraMatrix * reflection;

        var clipPlane = CameraSpacePlane(reflectionCamera, pos - Vector3.up * 0.1f, normal, 1.0f);
        var projection = realCamera.CalculateObliqueMatrix(clipPlane);
        reflectionCamera.projectionMatrix = projection;
        reflectionCamera.cullingMask = reflectionLayer;
        reflectionCamera.transform.position = newPosition;
    }

    private void CopyCamera(Camera src, Camera dest)
    {
        if (dest == null)
            return;

        dest.CopyFrom(src);
        dest.useOcclusionCulling = false;

        if (dest.gameObject.TryGetComponent(out UniversalAdditionalCameraData camData))
        {
            camData.renderShadows = false;
            if (reflectSkybox)
            {
                dest.clearFlags = CameraClearFlags.Skybox;
            }
            else
            {
                dest.clearFlags = CameraClearFlags.SolidColor;
                dest.backgroundColor = Color.black;
            }
        }
    }

    private Camera InitializeReflectionCamera()
    {
        var go = new GameObject("", typeof(Camera));
        go.name = "Reflection Camera [" + go.GetInstanceID() + "]";
        var camData = go.AddComponent(typeof(UniversalAdditionalCameraData)) as UniversalAdditionalCameraData;

        camData.requiresColorOption = CameraOverrideOption.Off;
        camData.requiresDepthOption = CameraOverrideOption.Off;
        camData.SetRenderer(0);

        var t = transform;
        var reflectionCamera = go.GetComponent<Camera>();
        reflectionCamera.transform.SetPositionAndRotation(t.position, t.rotation);
        reflectionCamera.depth = -10;
        reflectionCamera.enabled = false;

        return reflectionCamera;
    }

    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        var m = cam.worldToCameraMatrix;
        var cameraPosition = m.MultiplyPoint(pos);
        var cameraNormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cameraNormal.x, cameraNormal.y, cameraNormal.z, -Vector3.Dot(cameraPosition, cameraNormal));
    }

    RenderTextureDescriptor GetDescriptor(Camera camera, float pipelineRenderScale)
    {
        var width = (int)Mathf.Max(camera.pixelWidth * pipelineRenderScale * renderScale);
        var height = (int)Mathf.Max(camera.pixelHeight * pipelineRenderScale * renderScale);
        var hdr = camera.allowHDR;
        var renderTextureFormat = hdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;

        return new RenderTextureDescriptor(width, height, renderTextureFormat, 16)
        {
            autoGenerateMips = true,
            useMipMap = true
        };
    }

    private void CreateReflectionTexture(Camera camera)
    {
        var descriptor = GetDescriptor(camera, UniversalRenderPipeline.asset.renderScale);

        if (reflectionTexture == null)
        {
            reflectionTexture = RenderTexture.GetTemporary(descriptor);
            previousDescriptor = descriptor;
        }
        else if (!descriptor.Equals(previousDescriptor))
        {
            if (reflectionTexture)
                RenderTexture.ReleaseTemporary(reflectionTexture);

            reflectionTexture = RenderTexture.GetTemporary(descriptor);
            previousDescriptor = descriptor;
        }
        reflectionCamera.targetTexture = reflectionTexture;
    }

    private void DoPlanarReflection(ScriptableRenderContext context, Camera camera)
    {
        /* If reference position and proxy position is exactly the same, we end up in some degeneracies triggered
           by engine code when computing culling parameters. This is an extremely rare case, but can happen
           in editor when focusing on the planar probe. So if that happens, we offset them 0.1 mm apart.
          if(Vector3.Distance(result.proxyPosition, referencePosition) < 1e-4f)
          {
              referencePosition += new Vector3(1e-4f, 1e-4f, 1e-4f);
          }*/

        if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
            return;
        if (!reflectionTarget)
            return;

        UpdateReflectionCamera(camera);
        CreateReflectionTexture(camera);

        var data = new PlanarReflectionSettingData();
        data.Set();
        OnBeginPlanarReflection?.Invoke(context, reflectionCamera);
        if (reflectionCamera.WorldToViewportPoint(reflectionTarget.transform.position).z < 100000)
            UniversalRenderPipeline.RenderSingleCamera(context, reflectionCamera);
        data.Restore();
        Shader.SetGlobalTexture(planarReflectionTextureId, reflectionTexture);
    }

    class PlanarReflectionSettingData
    {
        private readonly bool fog;
        private readonly int maximumLODLevel;
        private readonly float lodBias;

        public PlanarReflectionSettingData()
        {
            fog = RenderSettings.fog;
            maximumLODLevel = QualitySettings.maximumLODLevel;
            lodBias = QualitySettings.lodBias;
        }

        public void Set()
        {
            GL.invertCulling = true;
            RenderSettings.fog = false;
            QualitySettings.maximumLODLevel = 1;
            QualitySettings.lodBias = lodBias * 0.5f;
        }

        public void Restore()
        {
            GL.invertCulling = false;
            RenderSettings.fog = fog;
            QualitySettings.maximumLODLevel = maximumLODLevel;
            QualitySettings.lodBias = lodBias;
        }
    }

    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMatrix, Vector4 plane)
    {
        reflectionMatrix.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMatrix.m01 = (-2F * plane[0] * plane[1]);
        reflectionMatrix.m02 = (-2F * plane[0] * plane[2]);
        reflectionMatrix.m03 = (-2F * plane[3] * plane[0]);

        reflectionMatrix.m10 = (-2F * plane[1] * plane[0]);
        reflectionMatrix.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMatrix.m12 = (-2F * plane[1] * plane[2]);
        reflectionMatrix.m13 = (-2F * plane[3] * plane[1]);

        reflectionMatrix.m20 = (-2F * plane[2] * plane[0]);
        reflectionMatrix.m21 = (-2F * plane[2] * plane[1]);
        reflectionMatrix.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMatrix.m23 = (-2F * plane[3] * plane[2]);

        reflectionMatrix.m30 = 0F;
        reflectionMatrix.m31 = 0F;
        reflectionMatrix.m32 = 0F;
        reflectionMatrix.m33 = 1F;
    }
}

//#endif