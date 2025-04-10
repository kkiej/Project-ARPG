using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MaterialPass
{
    public Material material;
    public List<bool> passEnabled;
}

public class MaterialPassManager : MonoBehaviour
{
    public List<MaterialPass> materials = new List<MaterialPass>();

    public void ApplyPassSettings()
    {
        foreach (var matPass in materials)
        {
            if (matPass.material != null)
            {
                for (int i = 0; i < matPass.passEnabled.Count; i++)
                {
                    matPass.material.SetShaderPassEnabled(matPass.material.GetPassName(i), !matPass.passEnabled[i]);
                }
            }
        }
    }
}