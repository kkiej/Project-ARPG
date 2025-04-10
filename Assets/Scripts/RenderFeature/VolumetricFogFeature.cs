using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumetricFogFeature : ScriptableRendererFeature
{
    [Serializable]
    public class VolumetricFogSetting
    {
        public bool enable = true;

        [Header("分辨率缩放，效果可接受范围内越低越好")] [Range(0.01f, 1f)]
        public float scale = 0.6f; // 这里降采样倍数如果是0.5或者0.25会有噪点的鬼影
                                   // 猜测是深度图没有降采样导致采的深度不准确（正好采在两个像素中间）
                                   // 通过采相邻像素可以解决，另外改降采样倍数也可以

        [HideInInspector] [Range(2, 100)] public int stepCount = 8; // 步进次数，目前shader里写死了
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingTransparents;

        [Header("是否启用噪声滤波")] public bool spatialFilter = true;
        public EFilterMode spatialFilterMode = EFilterMode.Bilateral;

        public Texture2D noiseTexture;

        // public Texture2D[] noiseTextures;
        [HideInInspector] [Range(0f, 2f)] public float noiseScale = 1f;
        [Header("是否启用TAA滤波")] public bool temporalFilter = true;

        [HideInInspector] [Range(0f, 1f)] public float jitterScale = 1f;

        // [Min(2)] public int jitterSequence = 2;
        [Range(0f, 1f)] public float historyWeight = 0.91f;

        [Range(0f, 10f)] public float depthClampThreshold = 2f;
        [Min(0)] public float depthClampMaxDistance = 30f;

        [Header("提高步进次数")] public bool highQuality = false;

        [Header("Shaders")] public Shader filterShader;
        public Shader composeShader;
        public Shader copyDepthShader;
    }

    public enum EFilterMode
    {
        Point,
        Gaussian,
        Bilateral,
        Box4X4,
    }

    public VolumetricFogSetting settings = new VolumetricFogSetting();

    private VolumetricFogPass _volumetricFogPass;

    public override void Create()
    {
        if (settings.filterShader == null)
            settings.filterShader = Resources.Load<Shader>(ShaderIDs.VolumetricFogFilter);
        if (settings.composeShader == null)
            settings.composeShader = Resources.Load<Shader>(ShaderIDs.VolumetricFogCompose);
        if (settings.copyDepthShader == null)
            settings.copyDepthShader = Resources.Load<Shader>(ShaderIDs.VolumetricFogCopyDepth);
#if UNITY_EDITOR
        if (settings.noiseTexture == null)
            settings.noiseTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Packages/com.pwrd.volumetric-fog/Runtime/Textures/BlueNoise.TGA");
#endif

        _volumetricFogPass?.Dispose();
        _volumetricFogPass = new VolumetricFogPass();
    }

    private void OnDestroy()
    {
        _volumetricFogPass?.Dispose();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!settings.enable) return;
        _volumetricFogPass.Setup(settings, renderer);
        renderer.EnqueuePass(_volumetricFogPass);
    }
}

public class VolumetricFogPass : ScriptableRenderPass
{
    const string profilerTag = "VolumetricFogPass";
    ShaderTagId _shaderTagId = new ShaderTagId("VolumetricFog");
    ProfilingSampler _profilingSampler = new ProfilingSampler(profilerTag);
    FilteringSettings _filteringSettings = new FilteringSettings(RenderQueueRange.transparent);
    RenderTargetHandle _fogRT;
    ScriptableRenderer _renderer;
    VolumetricFogFeature.VolumetricFogSetting _settings;
    Material _composeMaterial;
    Material _filterMaterial;
    Material _copyDepthMaterial;

    private Dictionary<Camera, FogCameraData> _cacheData = new Dictionary<Camera, FogCameraData>();

    private class FogCameraData
    {
        public RenderTexture[] rts = new RenderTexture[2];
        public RenderTexture lastDepth;
        public int curRtIndex = -1;
        public Matrix4x4 lastViewProj;
        public Vector3 lastCameraPos;
        public bool hasLastDepth;
        public bool hasLastColor;

        public void Dispose()
        {
            for (var i = 0; i < rts.Length; i++)
            {
                if (rts[i] != null)
                {
                    RenderTexture.ReleaseTemporary(rts[i]);
                    rts[i] = null;
                }
            }

            if (lastDepth != null)
            {
                RenderTexture.ReleaseTemporary(lastDepth);
                lastDepth = null;
            }

            curRtIndex = -1;
            hasLastDepth = false;
            hasLastColor = false;
        }
    }

    public VolumetricFogPass()
    {
        renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        _fogRT.Init("FogTexture");
    }

    public void Setup(VolumetricFogFeature.VolumetricFogSetting setting, ScriptableRenderer renderer)
    {
        _settings = setting;
        _renderer = renderer;
        renderPassEvent = setting.Event;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        ConfigureColorTextureDesc(ref cameraTextureDescriptor, RenderTextureFormat.ARGB32);
        cmd.GetTemporaryRT(_fogRT.id, cameraTextureDescriptor, FilterMode.Point);
        ConfigureTarget(_fogRT.Identifier());
        ConfigureClear(ClearFlag.Color, Color.black);
    }

    private RenderTexture GetRenderTexture(string name, ref RenderTextureDescriptor descriptor, FilterMode filter,
        RenderTexture rt, out bool createNew)
    {
        createNew = false;
        if (rt != null)
        {
            if (rt.width != descriptor.width || rt.height != descriptor.height || rt.filterMode != filter)
            {
                RenderTexture.ReleaseTemporary(rt);
                rt = null;
            }
        }

        if (rt == null)
        {
            rt = RenderTexture.GetTemporary(descriptor);
            rt.filterMode = filter;
            rt.name = name;
            createNew = true;
        }

        return rt;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        //if (renderingData.cameraData.cameraType == CameraType.Game)
        //{
        //    if (renderingData.cameraData.camera.GetUniversalAdditionalCameraData().scriptableRenderer != null)
        //    {
        //        if (!renderingData.cameraData.renderData.UsePostProcessing) return;     
        //    }
        //}
        
        if(renderingData.cameraData.postProcessEnabled == false)
            return;

#if !UNITY_EDITOR && !UNITY_STANDALONE_WIN && !UNITY_STANDALONE_OSX
        return;
#endif
        if(!GlobalConfig.useVolumeFog)
            return;
        
        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        using (new ProfilingScope(cmd, _profilingSampler))
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            Prepare(ref renderingData, cmd);
            DoRayMarchingPass(context, cmd, ref renderingData);
            DoFilterPass(cmd, ref renderingData.cameraData);
            CopyDepthPass(cmd, ref renderingData.cameraData);
            DoComposePass(cmd, ref renderingData.cameraData);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    private void Prepare(ref RenderingData renderingData, CommandBuffer cmd)
    {
        var camera = renderingData.cameraData.camera;
        if (!_cacheData.TryGetValue(camera, out var fogData))
        {
            fogData = new FogCameraData();
            _cacheData.Add(camera, fogData);
        }

        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        ConfigureDepthTextureDesc(ref descriptor);
        fogData.lastDepth = GetRenderTexture(FogDepthTexName, ref descriptor, FilterMode.Point, fogData.lastDepth,
            out var createNew);
        fogData.hasLastDepth &= !createNew;

        ConfigureColorTextureDesc(ref descriptor, RenderTextureFormat.ARGBHalf);
        // cmd.GetTemporaryRT(_fogRT.id, descriptor, FilterMode.Point);
        var rts = fogData.rts;
        if (fogData.curRtIndex == -1)
        {
            fogData.curRtIndex = 0;
            fogData.hasLastColor = false;
        }

        rts[fogData.curRtIndex] = GetRenderTexture(FogTexNames[fogData.curRtIndex], ref descriptor,
            FilterMode.Bilinear, rts[fogData.curRtIndex], out _);
        cmd.SetGlobalVector(FogTextureSize, new Vector4(descriptor.width, descriptor.height));
        
        // Unity提供的VP逆矩阵在不同版本处理不同，这里自己传入
        Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
        Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
        Matrix4x4 viewProjMatrix = projMatrix * viewMatrix;
        Matrix4x4 invViewProjMatrix = Matrix4x4.Inverse(viewProjMatrix);
        Shader.SetGlobalMatrix(MyInvViewProjMatrix, invViewProjMatrix);
    }

    private void DoRayMarchingPass(ScriptableRenderContext context, CommandBuffer cmd, ref RenderingData renderingData)
    {
        DrawingSettings drawingSettings =
            CreateDrawingSettings(_shaderTagId, ref renderingData, SortingCriteria.CommonTransparent);
        if (_settings.spatialFilter)
        {
            Shader.EnableKeyword("_NoiseEnable");
            var noiseTex = _settings.noiseTexture;
            // if (_settings.noiseTextures.Length > 0)
            //     noiseTex = _settings.noiseTextures[Time.frameCount % _settings.noiseTextures.Length];
            cmd.SetGlobalTexture(NoiseMap, noiseTex);
            if (noiseTex != null)
                cmd.SetGlobalVector(NoiseMapTexelSize, new Vector4(noiseTex.width, noiseTex.height));
        }
        else
            Shader.DisableKeyword("_NoiseEnable");

        if (_settings.highQuality)
            Shader.EnableKeyword("_HighQuality");
        else
            Shader.DisableKeyword("_HighQuality");

        var jitter = _settings.temporalFilter
            ? Halton(Time.frameCount) * _settings.jitterScale
            : 1;
        cmd.SetGlobalVector(FogParam,
            new Vector4(jitter, _settings.stepCount, _settings.noiseScale));
        context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings);
    }

    private void DoFilterPass(CommandBuffer cmd, ref CameraData cameraData)
    {
        if (_filterMaterial == null)
            _filterMaterial = new Material(_settings.filterShader);
        var filterMode = _settings.spatialFilter
            ? _settings.spatialFilterMode
            : VolumetricFogFeature.EFilterMode.Point;
        switch (filterMode)
        {
            case VolumetricFogFeature.EFilterMode.Point:
                _filterMaterial.DisableKeyword("_FilterMode_Gaussian");
                _filterMaterial.DisableKeyword("_FilterMode_Bilateral");
                _filterMaterial.DisableKeyword("_FilterMode_Box4x4");
                break;
            case VolumetricFogFeature.EFilterMode.Gaussian:
                _filterMaterial.DisableKeyword("_FilterMode_Bilateral");
                _filterMaterial.DisableKeyword("_FilterMode_Box4x4");
                _filterMaterial.EnableKeyword("_FilterMode_Gaussian");
                break;
            case VolumetricFogFeature.EFilterMode.Bilateral:
                _filterMaterial.DisableKeyword("_FilterMode_Gaussian");
                _filterMaterial.DisableKeyword("_FilterMode_Box4x4");
                _filterMaterial.EnableKeyword("_FilterMode_Bilateral");
                break;
            case VolumetricFogFeature.EFilterMode.Box4X4:
                _filterMaterial.DisableKeyword("_FilterMode_Gaussian");
                _filterMaterial.DisableKeyword("_FilterMode_Bilateral");
                _filterMaterial.EnableKeyword("_FilterMode_Box4x4");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var fogData = _cacheData[cameraData.camera];
        var finalRTs = fogData.rts;
        if (_settings.temporalFilter && fogData.hasLastColor)
        {
            Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrix(), false);
            var viewProj = projMatrix * cameraData.GetViewMatrix();
            _filterMaterial.SetMatrix(ClipToLastClip, fogData.lastViewProj * viewProj.inverse);
            fogData.lastViewProj = viewProj;
            var historyIndex = (fogData.curRtIndex + finalRTs.Length - 1) % finalRTs.Length;
            _filterMaterial.SetTexture(HistoryFogTexture, finalRTs[historyIndex]);
            _filterMaterial.SetVector(TemporalFilterParam,
                new Vector4(_settings.historyWeight, fogData.lastCameraPos.x, fogData.lastCameraPos.y,
                    fogData.lastCameraPos.z));

            if (fogData.hasLastDepth && _settings.depthClampThreshold >= 0 && _settings.depthClampMaxDistance > 0.0001f)
            {
                _filterMaterial.SetTexture(LastDepthTexture, fogData.lastDepth);
                _filterMaterial.EnableKeyword("_DepthClamp");
                var depthTexelSize = fogData.lastDepth.texelSize;
                _filterMaterial.SetVector(DepthClampParam,
                    new Vector4(_settings.depthClampThreshold, _settings.depthClampMaxDistance, depthTexelSize.x,
                        depthTexelSize.y));
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

        cmd.Blit(_fogRT.id, finalRTs[fogData.curRtIndex], _filterMaterial);
    }

    private void DoComposePass(CommandBuffer cmd, ref CameraData cameraData)
    {
        if (_composeMaterial == null)
            _composeMaterial = new Material(_settings.composeShader);
        var fogData = _cacheData[cameraData.camera];
        var finalRTs = fogData.rts;
        cmd.Blit(finalRTs[fogData.curRtIndex], _renderer.cameraColorTarget, _composeMaterial);
        if (_settings.temporalFilter)
        {
            fogData.curRtIndex = (fogData.curRtIndex + 1) % finalRTs.Length;
            fogData.hasLastColor = true;
        }
        else
        {
            fogData.hasLastColor = false;
        }
    }

    private void CopyDepthPass(CommandBuffer cmd, ref CameraData cameraData)
    {
        var fogData = _cacheData[cameraData.camera];
        if (!_settings.temporalFilter)
        {
            fogData.hasLastDepth = false;
            return;
        }

        if (_copyDepthMaterial == null)
            _copyDepthMaterial = new Material(_settings.copyDepthShader);
        cmd.Blit(null, fogData.lastDepth, _copyDepthMaterial);
        fogData.hasLastDepth = true;
        fogData.lastCameraPos = cameraData.camera.transform.position;
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        if (cmd == null)
            throw new ArgumentNullException(nameof(cmd));

        cmd.ReleaseTemporaryRT(_fogRT.id);
    }

    private void ConfigureColorTextureDesc(ref RenderTextureDescriptor descriptor, RenderTextureFormat textureFormat)
    {
        descriptor.msaaSamples = 1;
        descriptor.depthBufferBits = 0;
        descriptor.width = (int) (descriptor.width * _settings.scale);
        descriptor.height = (int) (descriptor.height * _settings.scale);
        descriptor.colorFormat = textureFormat;
    }

    private void ConfigureDepthTextureDesc(ref RenderTextureDescriptor descriptor)
    {
        descriptor.colorFormat = RenderTextureFormat.Depth;
        descriptor.depthBufferBits = 32;
        descriptor.msaaSamples = 1;
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

/*
    private float Halton2(int index, int Base = 2)
    {
        if (Base < 2) Base = 2;
        float res = 0.0f;
        float invBase = 1.0f / Base;
        float fraction = invBase;
        while (index > 0)
        {
            res += (index % Base) * fraction;
            index /= Base;
            fraction *= invBase;
        }

        return res;
    }
*/

    public void Dispose()
    {
        _renderer = null;
        foreach (var fogData in _cacheData.Values)
        {
            fogData?.Dispose();
        }
    }

    private static readonly int FogParam = Shader.PropertyToID("_FogParam");
    private static readonly int FogTextureSize = Shader.PropertyToID("_FogTextureSize");
    private static readonly int ClipToLastClip = Shader.PropertyToID("_ClipToLastClip");
    private static readonly int HistoryFogTexture = Shader.PropertyToID("_HistoryFogTexture");
    private static readonly int TemporalFilterParam = Shader.PropertyToID("_TemporalFilterParam");
    private static readonly int NoiseMap = Shader.PropertyToID("_NoiseMap");
    private static readonly int NoiseMapTexelSize = Shader.PropertyToID("_NoiseMap_TexelSize");
    private static readonly string[] FogTexNames = {"FogFinalTexture1", "FogFinalTexture2"};
    private static readonly int LastDepthTexture = Shader.PropertyToID("_LastDepthTexture");
    private static readonly int DepthClampParam = Shader.PropertyToID("_DepthClampParam");
    private static readonly int MyInvViewProjMatrix = Shader.PropertyToID("_MyInvViewProjMatrix");
    private const string FogDepthTexName = "FogDepthTexture";
}