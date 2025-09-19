using System;
using Unity.VisualScripting;
using UnityEngine;

public class HeadVectorGenerator : MonoBehaviour
{
    public Transform headBoneTransform;
    public Transform headForwardTransform;
    public Transform headRightTransform;

    private Renderer[] allRenderers;

    private int headForwardID = Shader.PropertyToID("_HeadForward");
    private int headRightID = Shader.PropertyToID("_HeadRight");

#if UNITY_EDITOR
    private void OnValidate()
    {
        LateUpdate();
    }
#endif

    private void LateUpdate()
    {
        if (allRenderers == null)
        {
            allRenderers = GetComponentsInChildren<Renderer>(true);
        }

        for (int i = 0; i < allRenderers.Length; i++)
        {
            Renderer r = allRenderers[i];
            foreach (var mat in r.sharedMaterials)
            {
                if (mat.shader)
                {
                    if (mat.shader.name == "LiteRenderPipeline/Character/SRUniversal")
                    {
                        var position = headBoneTransform.position;
                        mat.SetVector(headForwardID, headForwardTransform.position - position);
                        mat.SetVector(headRightID, headRightTransform.position - position);
                    }
                }
            }
        }
    }
}