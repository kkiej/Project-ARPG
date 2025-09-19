using UnityEngine;

[ExecuteInEditMode]
public sealed class SingleAtmosphere : MonoBehaviour
{
    public Material _ppMaterial;
    
    [Range(0, 10.0f)]
    public float MieScatterCoef = 1;
    [Range(0, 10.0f)]
    public float MieExtinctionCoef = 1;
    [Range(0.0f, 0.999f)]
    public float MieG = 0.76f;
    
    public float AtmosphereHeight = 80000.0f;
    public float PlanetRadius = 6371000.0f;
    private readonly Vector4 DensityScale = new Vector4(7994.0f, 1200.0f, 0, 0);
    private readonly Vector4 MieSct = new Vector4(2.0f, 2.0f, 2.0f, 0.0f) * 0.00001f;
    
    private static readonly int Height = Shader.PropertyToID("_AtmosphereHeight");
    private static readonly int Radius = Shader.PropertyToID("_PlanetRadius");
    private static readonly int DensityScaleHeight = Shader.PropertyToID("_DensityScaleHeight");
    private static readonly int ScatteringM = Shader.PropertyToID("_ScatteringM");
    private static readonly int ExtinctionM = Shader.PropertyToID("_ExtinctionM");
    private static readonly int G = Shader.PropertyToID("_MieG");
    
    private void Update()
    {
        _SetPPShaderParam();
    }
    
    private void _SetPPShaderParam()
    {
        _SetClipToWorldMatrixToMaterial();
    }

    private void _SetClipToWorldMatrixToMaterial()
    {
        _ppMaterial.SetFloat(Height, AtmosphereHeight);
        _ppMaterial.SetFloat(Radius, PlanetRadius);
        _ppMaterial.SetVector(DensityScaleHeight, DensityScale);

        _ppMaterial.SetVector(ScatteringM, MieSct * MieScatterCoef);
        _ppMaterial.SetVector(ExtinctionM, MieSct * MieExtinctionCoef);
        
        _ppMaterial.SetFloat(G, MieG);
    }
}
