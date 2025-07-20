using UnityEngine;

[ExecuteInEditMode]
public class FogController : MonoBehaviour
{
    [Header("基础设置")]
    [Tooltip("是否启用雾效")]
    public bool fogEnabled = true;

    [Header("雾效模式")]
    [Tooltip("选择雾效模式")]
    public FogMode fogMode = FogMode.Exponential;

    [Header("颜色设置")]
    [Tooltip("雾效颜色")]
    public Color fogColor = Color.gray;

    [Header("指数模式参数")]
    [Range(0.001f, 0.1f)]
    [Tooltip("雾效密度（指数模式使用）")]
    public float fogDensity = 0.01f;

    [Header("线性模式参数")]
    [Tooltip("雾效起始距离（线性模式使用）")]
    public float fogStartDistance = 0.0f;
    [Tooltip("雾效结束距离（线性模式使用）")]
    public float fogEndDistance = 300.0f;

    void OnEnable()
    {
        ApplyFogSettings();
    }

    void Update()
    {
        ApplyFogSettings();
    }

    // 应用所有雾效设置
    private void ApplyFogSettings()
    {
        RenderSettings.fog = fogEnabled;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = fogMode;

        switch (fogMode)
        {
            case FogMode.Linear:
                RenderSettings.fogStartDistance = fogStartDistance;
                RenderSettings.fogEndDistance = fogEndDistance;
                break;
            case FogMode.Exponential:
                RenderSettings.fogDensity = fogDensity;
                break;
            case FogMode.ExponentialSquared:
                RenderSettings.fogDensity = fogDensity;
                break;
        }
    }

    // 以下为公共方法，可供其他脚本或UI调用 -----------------

    /// <summary>
    /// 切换雾效开关状态
    /// </summary>
    public void ToggleFog()
    {
        fogEnabled = !fogEnabled;
        ApplyFogSettings();
    }

    /// <summary>
    /// 设置雾效密度（仅指数模式有效）
    /// </summary>
    /// <param name="density">新的密度值 (0.001 - 0.1)</param>
    public void SetFogDensity(float density)
    {
        fogDensity = Mathf.Clamp(density, 0.001f, 0.1f);
        ApplyFogSettings();
    }

    /// <summary>
    /// 设置线性模式参数
    /// </summary>
    /// <param name="start">起始距离</param>
    /// <param name="end">结束距离</param>
    public void SetLinearParameters(float start, float end)
    {
        fogStartDistance = start;
        fogEndDistance = end;
        ApplyFogSettings();
    }

    /// <summary>
    /// 设置雾效颜色
    /// </summary>
    /// <param name="newColor">新的颜色值</param>
    public void SetFogColor(Color newColor)
    {
        fogColor = newColor;
        ApplyFogSettings();
    }

    // 在Inspector面板修改值时实时更新（仅在编辑模式下）
    private void OnValidate()
    {
        ApplyFogSettings();
    }
}