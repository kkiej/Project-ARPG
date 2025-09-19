using UnityEngine;

[ExecuteInEditMode]
public class GetLightLToW : MonoBehaviour
{
    private Matrix4x4 LToW;
    private int _LToWID;

    private void Start()
    {
        _LToWID = Shader.PropertyToID("_LToW");
    }

    void Update()
    {
        LToW = transform.localToWorldMatrix;
        Shader.SetGlobalMatrix(_LToWID, LToW);
    }
}
