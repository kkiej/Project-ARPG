using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
public class AverageNormal : EditorWindow
{
    [MenuItem("Tools/模型平均法线写入顶点颜色")]
    public static void WriteToColor()
    {
        MeshFilter[] meshFilters = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>();
        foreach (var meshFilter in meshFilters)
        {
            Mesh mesh = Instantiate(meshFilter.sharedMesh);
            ToColor(mesh);
            AssetDatabase.CreateAsset(mesh, "Assets/" + meshFilter.name + "New.mesh");
        }

        SkinnedMeshRenderer[] skinMeshRenders = Selection.activeGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var skinMeshRender in skinMeshRenders)
        {
            Mesh mesh = Instantiate(skinMeshRender.sharedMesh);
            ToColor(mesh);
            AssetDatabase.CreateAsset(mesh, "Assets/" + skinMeshRender.name + "New.mesh");
        }
    }
    
    private static void ToColor(Mesh mesh)
    {
        var averageNormalHash = new Dictionary<Vector3, Vector3>();
        for (var j = 0; j < mesh.vertexCount; j++)
        {
            if (!averageNormalHash.ContainsKey(mesh.vertices[j]))
            {
                averageNormalHash.Add(mesh.vertices[j], mesh.normals[j]);
            }
            else
            {
                averageNormalHash[mesh.vertices[j]] =
                    (averageNormalHash[mesh.vertices[j]] + mesh.normals[j]).normalized;
            }
        }

        var averageNormals = new Vector3[mesh.vertexCount];
        for (var j = 0; j < mesh.vertexCount; j++)
        {
            averageNormals[j] = averageNormalHash[mesh.vertices[j]];
            //转到切线空间 并且设置成[0,1]范围
            var mNormal = mesh.normals[j];
            var mTangent = mesh.tangents[j];
            
            var mBinormal = Vector3.Cross(mNormal, new Vector3(mTangent.x, mTangent.y, mTangent.z)) * mTangent.w;
            //构造是按列，此处需要按行
            Matrix4x4 matrix = new Matrix4x4(new Vector3(mTangent.x, mTangent.y, mTangent.z).normalized, mBinormal.normalized, mNormal.normalized, Vector4.zero);
            Matrix4x4 tmatrix = Matrix4x4.Transpose(matrix);
            
            averageNormals[j] = (tmatrix.MultiplyVector(averageNormals[j]).normalized + Vector3.one) / 2.0f;
        }
        
        var colors = new Color[mesh.vertexCount];
        for (var j = 0; j < mesh.vertexCount; j++)
        {
            colors[j] = new Color(averageNormals[j].x, averageNormals[j].y, averageNormals[j].z, 0);
        }
        mesh.colors = colors;
    }
}