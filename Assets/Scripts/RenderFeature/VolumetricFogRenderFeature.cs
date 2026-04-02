using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class VolumetricFogRenderFeature : ScriptableRendererFeature
{
    [Serializable]
    public class VolumetricFogSettings
    {
        public bool enable = true;
        [Header("分辨率缩放，效果可接受范围内越低越好")] [Range(0.01f, 1f)]
        public float scale = 0.6f;
        [HideInInspector] [Range(2, 100)] public int stepCount = 8;
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingTransparents;
        [Header("是否启用噪声滤波")] public bool spatialFilter = true;
        public EFilterMode spatialFilterMode = EFilterMode.Bilateral;
        public bool useNoiseTexture;
        [HideInInspector] [Range(0f, 2f)] public float noiseScale = 1f;
        [Header("是否启用TAA滤波")] public bool temporalFilter = true;
        [HideInInspector] [Range(0f, 1f)] public float jitterScale = 1f;
        [Range(0f, 1f)] public float historyWeight = 0.91f;
        [Range(0f, 10f)] public float depthClampThreshold = 2f;
        [Min(0)] public float depthClampMaxDistance = 30f;
        [Header("提高步进次数")]
        public bool highQuality = false;
        [Header("Shaders")]
        public Shader filterShader;
        public Shader composeShader;
        public Shader copyDepthShader;
    }
    
    public enum EFilterMode
    {
        Point,
        Gaussian,
        Bilateral,
        Box4X4
    }
    
    public VolumetricFogSettings settings;
    VolumetricFogRenderPass _volumetricFogRenderPass;

    public override void Create()
    {
        if (settings == null) return;
        _volumetricFogRenderPass?.Dispose();
        _volumetricFogRenderPass = new VolumetricFogRenderPass(settings)
        {
            renderPassEvent = settings.Event
        };
    }

    protected override void Dispose(bool disposing)
    {
        _volumetricFogRenderPass?.Dispose();
        _volumetricFogRenderPass = null;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings == null || !settings.enable) return;
        if (_volumetricFogRenderPass == null) return;
        renderer.EnqueuePass(_volumetricFogRenderPass);
    }

    private class VolumetricFogRenderPass : ScriptableRenderPass
    {
        ProfilingSampler _profilingSampler = new ProfilingSampler("VolumetricFogPass_RayMarch");
        readonly VolumetricFogSettings settings;
        Material _composeMaterial;
        Material _filterMaterial;
        Material _copyDepthMaterial;
        
        private Dictionary<int, FogCameraData> _cacheData = new Dictionary<int, FogCameraData>();
    
        private class FogCameraData
        {
            public RTHandle[] rts = new RTHandle[2];
            public RTHandle lastDepth;
            public int curRtIndex = -1;
            public Matrix4x4 lastViewProj;
            public Vector3 lastCameraPos;
            public bool hasLastDepth;
            public bool hasLastColor;

            public void Dispose()
            {
                for (var i = 0; i < rts.Length; i++)
                {
                    rts[i]?.Release();
                    rts[i] = null;
                }

                lastDepth?.Release();
                lastDepth = null;

                curRtIndex = -1;
                hasLastDepth = false;
                hasLastColor = false;
            }
        }

        public VolumetricFogRenderPass(VolumetricFogSettings settings)
        {
            this.settings = settings;
        }

        private class RayMarchData
        {
            public RendererListHandle rendererList;
            public Vector4 fogTextureSize;
            public Matrix4x4 invViewProjMatrix;
            public bool noiseEnable;
            public bool highQuality;
            public Vector4 fogParam;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();
            var renderingData = frameData.Get<UniversalRenderingData>();
            var cameraData = frameData.Get<UniversalCameraData>();
            
            if (cameraData == null || cameraData.camera == null) return;
            if (!cameraData.postProcessEnabled) return;

            var fogRTDesc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
            fogRTDesc.width = (int)(fogRTDesc.width * settings.scale);
            fogRTDesc.height = (int)(fogRTDesc.height * settings.scale);
            fogRTDesc.name = "FogTexture";
            TextureHandle fogRT = renderGraph.CreateTexture(fogRTDesc);
            
            if (!fogRT.IsValid()) return;

            var camera = cameraData.camera;
            int cameraId = camera.GetInstanceID();
            if (!_cacheData.TryGetValue(cameraId, out var fogData))
            {
                fogData = new FogCameraData();
                _cacheData.Add(cameraId, fogData);
            }
            
            var desc = cameraData.cameraTargetDescriptor;
            desc.colorFormat = RenderTextureFormat.Depth;
            desc.graphicsFormat = GraphicsFormat.None;
            desc.depthBufferBits = 32;
            desc.msaaSamples = 1;

            var createNew = RenderingUtils.ReAllocateHandleIfNeeded(ref fogData.lastDepth, desc, FilterMode.Point,
                TextureWrapMode.Clamp, name: "FogDepthTexture");

            fogData.hasLastDepth &= !createNew;

            desc.depthBufferBits = 0;
            desc.width = (int)(desc.width * settings.scale);
            desc.height = (int)(desc.height * settings.scale);
            desc.colorFormat = RenderTextureFormat.ARGBHalf;
            var rts = fogData.rts;
            if (fogData.curRtIndex == -1)
            {
                fogData.curRtIndex = 0;
                fogData.hasLastColor = false;
            }

            RenderingUtils.ReAllocateHandleIfNeeded(ref rts[fogData.curRtIndex], desc, FilterMode.Bilinear,
                TextureWrapMode.Clamp, name: FogTexNames[fogData.curRtIndex]);
            
            ShaderTagId shaderTagID = new ShaderTagId("VolumetricFog");

            var rld = new RendererListDesc(shaderTagID, renderingData.cullResults, cameraData.camera)
            {
                sortingCriteria = SortingCriteria.CommonTransparent,
                rendererConfiguration = PerObjectData.None,
                renderQueueRange = RenderQueueRange.transparent
            };

            var rendererListHandle = renderGraph.CreateRendererList(rld);

            // --- RayMarch pass ---
            Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
            Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
            Matrix4x4 viewProjMatrix = projMatrix * viewMatrix;
            Matrix4x4 invViewProjMatrix = Matrix4x4.Inverse(viewProjMatrix);
            var jitter = settings.temporalFilter ? Halton(Time.frameCount) * settings.jitterScale : 1f;

            using (var builder = renderGraph.AddRasterRenderPass<RayMarchData>("VolumetricFog Pass", out var passData))
            {
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true);
                
                builder.UseRendererList(rendererListHandle);

                passData.rendererList = rendererListHandle;
                passData.fogTextureSize = new Vector4(desc.width, desc.height, 0, 0);
                passData.invViewProjMatrix = invViewProjMatrix;
                passData.noiseEnable = settings.spatialFilter && settings.useNoiseTexture;
                passData.highQuality = settings.highQuality;
                passData.fogParam = new Vector4(jitter, settings.stepCount, settings.noiseScale, 0f);

                builder.SetRenderAttachment(fogRT, 0);

                builder.SetRenderFunc((RayMarchData data, RasterGraphContext context) =>
                {
                    var cmd = context.cmd;
                    using (new ProfilingScope(cmd, _profilingSampler))
                    {
                        cmd.SetGlobalVector(FogTextureSize, data.fogTextureSize);
                        cmd.SetGlobalMatrix(MyInvViewProjMatrix, data.invViewProjMatrix);
                        
                        if (data.noiseEnable)
                            cmd.EnableKeyword(GlobalKeyword.Create("_NoiseEnable"));
                        else
                            cmd.DisableKeyword(GlobalKeyword.Create("_NoiseEnable"));
    
                        if (data.highQuality)
                            cmd.EnableKeyword(GlobalKeyword.Create("_HighQuality"));
                        else
                            cmd.DisableKeyword(GlobalKeyword.Create("_HighQuality"));
    
                        cmd.SetGlobalVector(FogParam, data.fogParam);
                        
                        cmd.DrawRendererList(data.rendererList);
                    }
                });
            }
            
            // --- Filter pass ---
            if (_filterMaterial == null && settings.filterShader != null)
                _filterMaterial = CoreUtils.CreateEngineMaterial(settings.filterShader);
    
            if (_filterMaterial == null) return;

            switch (settings.spatialFilter ? settings.spatialFilterMode : EFilterMode.Point)
            {
                case EFilterMode.Point:
                    _filterMaterial.DisableKeyword("_FilterMode_Gaussian");
                    _filterMaterial.DisableKeyword("_FilterMode_Bilateral");
                    _filterMaterial.DisableKeyword("_FilterMode_Box4x4");
                    break;
                case EFilterMode.Gaussian:
                    _filterMaterial.DisableKeyword("_FilterMode_Bilateral");
                    _filterMaterial.DisableKeyword("_FilterMode_Box4x4");
                    _filterMaterial.EnableKeyword("_FilterMode_Gaussian");
                    break;
                case EFilterMode.Bilateral:
                    _filterMaterial.DisableKeyword("_FilterMode_Gaussian");
                    _filterMaterial.DisableKeyword("_FilterMode_Box4x4");
                    _filterMaterial.EnableKeyword("_FilterMode_Bilateral");
                    break;
                case EFilterMode.Box4X4:
                    _filterMaterial.DisableKeyword("_FilterMode_Gaussian");
                    _filterMaterial.DisableKeyword("_FilterMode_Bilateral");
                    _filterMaterial.EnableKeyword("_FilterMode_Box4x4");
                    break;
            }
            
            var finalRTs = fogData.rts;
    
            if (settings.temporalFilter && fogData.hasLastColor)
            {
                Matrix4x4 proj = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrix(), false);
                var viewProj = proj * cameraData.GetViewMatrix();
                _filterMaterial.SetMatrix(ClipToLastClip, fogData.lastViewProj * viewProj.inverse);
                fogData.lastViewProj = viewProj;
                var historyIndex = (fogData.curRtIndex + finalRTs.Length - 1) % finalRTs.Length;
                _filterMaterial.SetTexture(HistoryFogTexture, finalRTs[historyIndex]);
                _filterMaterial.SetVector(TemporalFilterParam,
                    new Vector4(settings.historyWeight, fogData.lastCameraPos.x, fogData.lastCameraPos.y,
                        fogData.lastCameraPos.z));

                if (fogData.hasLastDepth && settings.depthClampThreshold >= 0 && settings.depthClampMaxDistance > 0.0001f)
                {
                    _filterMaterial.SetTexture(LastDepthTexture, fogData.lastDepth);
                    _filterMaterial.EnableKeyword("_DepthClamp");
                    var textureDesc = fogData.lastDepth.rt.descriptor;
                    _filterMaterial.SetVector(DepthClampParam,
                        new Vector4(settings.depthClampThreshold, settings.depthClampMaxDistance, textureDesc.width,
                            textureDesc.height));
                }
                else
                {
                    _filterMaterial.DisableKeyword("_DepthClamp");
                }
            }
            else
            {
                _filterMaterial.SetVector(TemporalFilterParam, Vector4.zero);
            }

            TextureHandle finalFogTexture = renderGraph.ImportTexture(finalRTs[fogData.curRtIndex]);
            
            if (!finalFogTexture.IsValid()) return;

            var filterBlitParams = new RenderGraphUtils.BlitMaterialParameters(fogRT, finalFogTexture, _filterMaterial, 0);
            renderGraph.AddBlitPass(filterBlitParams, "VolumetricFog_Filter");
    
            // --- Copy depth (if temporal) ---
            if (settings.temporalFilter)
            {
                if (_copyDepthMaterial == null && settings.copyDepthShader != null)
                    _copyDepthMaterial = CoreUtils.CreateEngineMaterial(settings.copyDepthShader);
    
                if (_copyDepthMaterial != null)
                {
                    TextureHandle srcDepthHandle = resourceData.cameraDepth;
                    TextureHandle lastDepthTexture = renderGraph.ImportTexture(fogData.lastDepth);
                    if (srcDepthHandle.IsValid() && lastDepthTexture.IsValid())
                    {
                        var blitParams = new RenderGraphUtils.BlitMaterialParameters(srcDepthHandle, lastDepthTexture, _copyDepthMaterial, 0);
                        renderGraph.AddBlitPass(blitParams, "VolumetricFog_CopyDepth");
                        fogData.hasLastDepth = true;
                        fogData.lastCameraPos = camera.transform.position;
                    }
                }
            }
            else
            {
                fogData.hasLastDepth = false;
            }
    
            // --- Compose pass ---
            if (_composeMaterial == null && settings.composeShader != null)
                _composeMaterial = CoreUtils.CreateEngineMaterial(settings.composeShader);
    
            if (_composeMaterial != null)
            {
                if (finalFogTexture.IsValid() && resourceData.activeColorTexture.IsValid())
                {
                    var blitParams = new RenderGraphUtils.BlitMaterialParameters(finalFogTexture, resourceData.activeColorTexture, _composeMaterial, 0);
                    renderGraph.AddBlitPass(blitParams, "VolumetricFog_Compose");
                }
            }
    
            if (settings.temporalFilter)
            {
                fogData.curRtIndex = (fogData.curRtIndex + 1) % fogData.rts.Length;
                fogData.hasLastColor = true;
            }
            else
            {
                fogData.hasLastColor = false;
            }
        }
        
        private float Halton(int index)
        {
            float res = 0.0f;
            float fraction = 0.5f;
            while (index > 0)
            {
                res += (index & 1) * fraction;
                index >>= 1;
                fraction *= 0.5f;
            }
            return res;
        }
        
        public void Dispose()
        {
            foreach (var fogData in _cacheData.Values)
            {
                fogData?.Dispose();
            }
            _cacheData.Clear();
            
            CoreUtils.Destroy(_filterMaterial);
            CoreUtils.Destroy(_composeMaterial);
            CoreUtils.Destroy(_copyDepthMaterial);
            _filterMaterial = null;
            _composeMaterial = null;
            _copyDepthMaterial = null;
        }
        
        // Shader property IDs
        private static readonly int FogParam = Shader.PropertyToID("_FogParam");
        private static readonly int FogTextureSize = Shader.PropertyToID("_FogTextureSize");
        private static readonly int ClipToLastClip = Shader.PropertyToID("_ClipToLastClip");
        private static readonly int HistoryFogTexture = Shader.PropertyToID("_HistoryFogTexture");
        private static readonly int TemporalFilterParam = Shader.PropertyToID("_TemporalFilterParam");
        private static readonly string[] FogTexNames = {"FogFinalTexture1", "FogFinalTexture2"};
        private static readonly int LastDepthTexture = Shader.PropertyToID("_LastDepthTexture");
        private static readonly int DepthClampParam = Shader.PropertyToID("_DepthClampParam");
        private static readonly int MyInvViewProjMatrix = Shader.PropertyToID("_MyInvViewProjMatrix");
    }
}
