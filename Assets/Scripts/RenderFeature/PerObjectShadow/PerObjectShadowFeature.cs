using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Profiling;
using Unity.Collections;
using Unity.Mathematics;
using Object = UnityEngine.Object;

public class PerObjectShadowFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public LayerMask shadowCastingLayers;
        public int maxShadowCasters = 10;
        public int shadowMapSize = 2048;
        public int tileSize = 512;
        public float maxDistance = 20f;
        public float shadowDistanceBias = 0.1f;
    }

    public Settings settings = new Settings();

    private PerObjectShadowPass m_ShadowPass;
    private HighQualityShadow m_ShadowManager;

    public override void Create()
    {
        m_ShadowManager = new HighQualityShadow(settings);
        m_ShadowPass = new PerObjectShadowPass(m_ShadowManager);
        m_ShadowPass.renderPassEvent = RenderPassEvent.AfterRenderingShadows;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.lightData.mainLightIndex == -1) return;
        
        m_ShadowManager.Update(renderingData);
        renderer.EnqueuePass(m_ShadowPass);
    }

    protected override void Dispose(bool disposing)
    {
        m_ShadowManager?.Dispose();
    }

    class PerObjectShadowPass : ScriptableRenderPass
    {
        private HighQualityShadow m_ShadowManager;
        private readonly ShaderTagId m_ShaderTag = new ShaderTagId("DepthOnly");
        
        private static readonly int _CharacterShadowTexture = Shader.PropertyToID("_CharacterShadowTexture");
        private static readonly int _CharacterShadowMatrix = Shader.PropertyToID("_CharacterShadowMatrix");
        private static readonly int _CharacterUVClamp = Shader.PropertyToID("_CharacterUVClamp");

        public PerObjectShadowPass(HighQualityShadow shadowManager)
        {
            m_ShadowManager = shadowManager;
        }
        
        [Obsolete("Obsolete")]
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (m_ShadowManager.shadowMap == null)
            {
                var desc = new RenderTextureDescriptor(
                    m_ShadowManager.settings.shadowMapSize, 
                    m_ShadowManager.settings.shadowMapSize,
                    RenderTextureFormat.Shadowmap, 24);
                
                m_ShadowManager.shadowMap = RenderTexture.GetTemporary(desc);
                m_ShadowManager.shadowMap.filterMode = FilterMode.Bilinear;
                m_ShadowManager.shadowMap.wrapMode = TextureWrapMode.Clamp;
            }
            
            ConfigureTarget(m_ShadowManager.shadowMap);
            ConfigureClear(ClearFlag.All, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.lightData.mainLightIndex == -1) return;

            CommandBuffer cmd = CommandBufferPool.Get("Per Object Shadow");
            //context.ExecuteCommandBuffer(cmd);
            //cmd.Clear();

            var light = renderingData.lightData.visibleLights[renderingData.lightData.mainLightIndex].light;
            
            m_ShadowManager.UpdateShadowMatrices(light, renderingData.cameraData.camera, m_ShadowManager.tilesPerRow);

            for (int i = 0; i < m_ShadowManager.sliceCount; i++)
            {
                var slice = m_ShadowManager.shadowSlices[i];
                if (slice.isEmpty) continue;

                cmd.SetViewProjectionMatrices(slice.viewMatrix, slice.projectionMatrix);
                cmd.SetViewport(new Rect(slice.offsetX, slice.offsetY, 
                    m_ShadowManager.settings.tileSize, m_ShadowManager.settings.tileSize));
                
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                var drawSettings = CreateDrawingSettings(m_ShaderTag, 
                    ref renderingData, SortingCriteria.CommonOpaque);
                var filterSettings = new FilteringSettings(
                    RenderQueueRange.opaque, m_ShadowManager.settings.shadowCastingLayers);

                context.DrawRenderers(renderingData.cullResults, 
                    ref drawSettings, ref filterSettings);
            }

            cmd.SetGlobalTexture(_CharacterShadowTexture, m_ShadowManager.shadowMap);
            cmd.SetGlobalMatrixArray(_CharacterShadowMatrix, m_ShadowManager.shadowMatrices);
            cmd.SetGlobalVectorArray(_CharacterUVClamp, m_ShadowManager.uvClamps);
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    class HighQualityShadow : System.IDisposable
    {
        public Settings settings;
        public RenderTexture shadowMap;
        public ShadowSliceData[] shadowSlices;
        public Matrix4x4[] shadowMatrices;
        public Vector4[] uvClamps;
        
        public int tilesPerRow => Mathf.FloorToInt(settings.shadowMapSize / (float)settings.tileSize);
        public int sliceCount => tilesPerRow * tilesPerRow;

        private List<Renderer> m_ShadowCasters = new List<Renderer>();
        private Camera m_CurrentCamera;
        private Light m_MainLight;

        public HighQualityShadow(Settings settings)
        {
            this.settings = settings;
            shadowSlices = new ShadowSliceData[sliceCount];
            shadowMatrices = new Matrix4x4[sliceCount];
            uvClamps = new Vector4[sliceCount];
        }

        public void Update(RenderingData renderingData)
        {
            m_CurrentCamera = renderingData.cameraData.camera;
            m_MainLight = renderingData.lightData.visibleLights[renderingData.lightData.mainLightIndex].light;
            
            UpdateCulling();
        }

        private void UpdateCulling()
        {
            // 执行视锥剔除和距离剔除
            var camera = m_CurrentCamera;
            var mainLight = m_MainLight;

            m_ShadowCasters.RemoveAll(r => r == null);
            var candidates = Object.FindObjectsOfType<Renderer>();

            foreach (var renderer in candidates)
            {
                if (m_ShadowCasters.Contains(renderer)) continue;

                //bool isVisible = GeometryUtility.TestPlanesAABB(
                //    GeometryUtility.CalculateFrustumPlanes(camera), 
                //    renderer.bounds);

                bool isVisible = IsVisibleByCamera(renderer.bounds.center, renderer.bounds.extents, camera);

                float distance = Vector3.Distance(
                    renderer.transform.position, camera.transform.position);

                if (isVisible && distance < settings.maxDistance)
                {
                    if (m_ShadowCasters.Count < settings.maxShadowCasters)
                    {
                        m_ShadowCasters.Add(renderer);
                    }
                    else
                    {
                        // 替换最远的对象
                        int farthestIndex = 0;
                        float maxDist = 0;
                        for (int i = 0; i < m_ShadowCasters.Count; i++)
                        {
                            float d = Vector3.Distance(
                                m_ShadowCasters[i].transform.position, 
                                camera.transform.position);
                            if (d > maxDist)
                            {
                                maxDist = d;
                                farthestIndex = i;
                            }
                        }
                        if (distance < maxDist)
                        {
                            m_ShadowCasters[farthestIndex] = renderer;
                        }
                    }
                }
            }
        }
        
        private bool IsVisibleByCamera(float3 c, float3 e, Camera mainCamera)
        {
            var VP = mainCamera.projectionMatrix * mainCamera.worldToCameraMatrix;

            float3 fa = VP.m30 - new float3(VP.m00, VP.m10, VP.m20);
            float3 fb = VP.m31 - new float3(VP.m01, VP.m11, VP.m21);
            float3 fc = VP.m32 - new float3(VP.m02, VP.m12, VP.m22);
            float3 fd = VP.m33 - new float3(VP.m03, VP.m13, VP.m23);

            float3 fe = VP.m30 + new float3(VP.m00, VP.m10, VP.m20);
            float3 ff = VP.m31 + new float3(VP.m01, VP.m11, VP.m21);
            float3 fg = VP.m32 + new float3(VP.m02, VP.m12, VP.m22);
            float3 fh = VP.m33 + new float3(VP.m03, VP.m13, VP.m23);

            float3 d012 = fa * c.x + fb * c.y + fc * c.z + fd;
            float3 d345 = fe * c.x + ff * c.y + fg * c.z + fh;

            float3 r012 = math.abs(fa) * e.x + math.abs(fb) * e.y + math.abs(fc) * e.z;
            float3 r345 = math.abs(fe) * e.x + math.abs(ff) * e.y + math.abs(fg) * e.z;

            float3 f012 = d012 + r012;
            float3 f345 = d345 + r345;

            return math.all(f012 > 0) && math.all(f345 > 0);
        }

        public void UpdateShadowMatrices(Light light, Camera camera, int tiles)
        {
            for (int i = 0; i < shadowSlices.Length; i++)
            {
                if (i >= m_ShadowCasters.Count)
                {
                    shadowSlices[i].isEmpty = true;
                    continue;
                }

                var renderer = m_ShadowCasters[i];
                var bounds = renderer.bounds;

                // 计算光源空间矩阵
                Matrix4x4 viewMatrix = ComputeLightViewMatrix(light, bounds.center);
                var lightSpaceBounds = TransformBoundsToLightSpace(bounds, viewMatrix);
                Matrix4x4 projMatrix = ComputeLightProjectionMatrix(lightSpaceBounds);

                // 计算阴影矩阵
                shadowSlices[i] = new ShadowSliceData
                {
                    viewMatrix = viewMatrix,
                    projectionMatrix = projMatrix,
                    offsetX = (i % tiles) * settings.tileSize,
                    offsetY = (i / tiles) * settings.tileSize,
                    isEmpty = false
                };

                shadowMatrices[i] = GetShadowTransform(projMatrix, viewMatrix);
                uvClamps[i] = CalculateUVClamp(shadowSlices[i]);
            }
        }
        
        private Bounds TransformBoundsToLightSpace(Bounds bounds, Matrix4x4 lightView)
        {
            // 获取物体包围盒的8个顶点
            Vector3[] corners = new Vector3[8];
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;
    
            corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
            corners[1] = center + new Vector3( extents.x, -extents.y, -extents.z);
            corners[2] = center + new Vector3(-extents.x,  extents.y, -extents.z);
            corners[3] = center + new Vector3( extents.x,  extents.y, -extents.z);
            corners[4] = center + new Vector3(-extents.x, -extents.y,  extents.z);
            corners[5] = center + new Vector3( extents.x, -extents.y,  extents.z);
            corners[6] = center + new Vector3(-extents.x,  extents.y,  extents.z);
            corners[7] = center + new Vector3( extents.x,  extents.y,  extents.z);

            // 转换所有顶点到光源空间
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    
            foreach (var corner in corners)
            {
                Vector3 lightSpacePos = lightView.MultiplyPoint(corner);
                min = Vector3.Min(min, lightSpacePos);
                max = Vector3.Max(max, lightSpacePos);
            }

            return new Bounds
            {
                min = min,
                max = max
            };
        }

        private Matrix4x4 ComputeLightViewMatrix(Light light, Vector3 center)
        {
            Vector3 lightDir = -light.transform.forward;
            Vector3 position = center + lightDir * settings.shadowDistanceBias;
            return Matrix4x4.TRS(
                position, 
                Quaternion.LookRotation(lightDir, Vector3.up), 
                Vector3.one).inverse;
        }

        private Matrix4x4 ComputeLightProjectionMatrix(Bounds bounds)
        {
            float near = bounds.min.z - settings.shadowDistanceBias;
            float far = bounds.max.z + settings.shadowDistanceBias;

            return Matrix4x4.Ortho(
                bounds.min.x, 
                bounds.max.x,
                bounds.min.y, 
                bounds.max.y,
                near, 
                far
            );
        }

        private Matrix4x4 GetShadowTransform(Matrix4x4 proj, Matrix4x4 view)
        {
            Matrix4x4 worldToShadow = proj * view;
            if (SystemInfo.usesReversedZBuffer) 
            {
                worldToShadow.m20 *= -1;
                worldToShadow.m21 *= -1;
                worldToShadow.m22 *= -1;
                worldToShadow.m23 *= -1;
            }
    
            var scaleOffset = Matrix4x4.identity;
            scaleOffset.m00 = scaleOffset.m11 = 0.5f;
            scaleOffset.m03 = scaleOffset.m13 = 0.5f;
            return scaleOffset * worldToShadow;
        }

        private Vector4 CalculateUVClamp(ShadowSliceData slice)
        {
            float x = slice.offsetX / (float)settings.shadowMapSize;
            float y = slice.offsetY / (float)settings.shadowMapSize;
            float w = settings.tileSize / (float)settings.shadowMapSize;
            return new Vector4(x, x + w, y, y + w);
        }

        public void Dispose()
        {
            if (shadowMap != null)
                RenderTexture.ReleaseTemporary(shadowMap);
        }
    }

    struct ShadowSliceData
    {
        public Matrix4x4 viewMatrix;
        public Matrix4x4 projectionMatrix;
        public int offsetX;
        public int offsetY;
        public bool isEmpty;
    }
}