using UnityEngine;

[ExecuteAlways]
public class ClusterDebug : MonoBehaviour
{
    ClusterLight clusterLight;

    void Update()
    {
        clusterLight ??= new ClusterLight();

        // 更新光源
        var lights = FindObjectsOfType(typeof(Light)) as Light[];
        clusterLight.UpdateLightBuffer(lights);

        // 划分 cluster
        Camera m_camera = Camera.main;
        clusterLight.ClusterGenerate(m_camera);

        // 分配光源
        clusterLight.LightAssign();
        
        // 传递参数
        clusterLight.SetShaderParameters();
        
       // clusterLight.ReleaseCB();

        // Debug
        clusterLight.DebugCluster();
        clusterLight.DebugLightAssign();
    }
}
