using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LZ.EditorTools
{
    /// <summary>
    /// 把 dump_c0000_dummies.py 生成的 <c>Assets/_ELDENRING_REF/er_mount_data.json</c>
    /// 导入为运行时用的 <see cref="ERMountData"/> 资产，输出到 Resources 便于 <c>Resources.Load</c>。
    /// </summary>
    public static class ERMountDataImporter
    {
        private const string JsonPath = "Assets/_ELDENRING_REF/er_mount_data.json";
        private const string OutDir = "Assets/_ELDENRING_REF/Resources";
        private const string OutPath = OutDir + "/ERMountData.asset";

        [Serializable] private class DummyDTO
        {
            public int referenceId;
            public string spaceBone;
            public string attachBone;
            public bool followsAttach;
            public float posX, posY, posZ;
            public float rightX, rightY, rightZ;
            public float upX, upY, upZ;
            public float fwdX, fwdY, fwdZ;
        }

        [Serializable] private class CalibDTO { public string name; public float x, y, z; }

        [Serializable] private class RootDTO { public DummyDTO[] dummies; public CalibDTO[] calibrationBones; }

        [MenuItem("Tools/ER/导入挂载数据 (er_mount_data.json)")]
        public static void Import()
        {
            string full = Path.GetFullPath(JsonPath);
            if (!File.Exists(full))
            {
                EditorUtility.DisplayDialog("ER 挂载数据导入",
                    $"找不到 JSON:\n{JsonPath}\n\n请先运行 dump_c0000_dummies.py 生成。", "确定");
                return;
            }

            string json = File.ReadAllText(full);
            RootDTO root = JsonUtility.FromJson<RootDTO>(json);
            if (root == null || root.dummies == null)
            {
                EditorUtility.DisplayDialog("ER 挂载数据导入", "JSON 解析失败。", "确定");
                return;
            }

            if (!Directory.Exists(OutDir))
                Directory.CreateDirectory(OutDir);

            var asset = AssetDatabase.LoadAssetAtPath<ERMountData>(OutPath);
            bool isNew = asset == null;
            if (isNew) asset = ScriptableObject.CreateInstance<ERMountData>();

            asset.dummies.Clear();
            foreach (var d in root.dummies)
            {
                asset.dummies.Add(new ERDummyFrame
                {
                    referenceId = d.referenceId,
                    spaceBone = d.spaceBone,
                    attachBone = d.attachBone,
                    followsAttach = d.followsAttach,
                    pos = new Vector3(d.posX, d.posY, d.posZ),
                    right = new Vector3(d.rightX, d.rightY, d.rightZ),
                    up = new Vector3(d.upX, d.upY, d.upZ),
                    fwd = new Vector3(d.fwdX, d.fwdY, d.fwdZ),
                });
            }

            asset.calibrationBones.Clear();
            if (root.calibrationBones != null)
                foreach (var c in root.calibrationBones)
                    asset.calibrationBones.Add(new ERCalibBone { name = c.name, flverPos = new Vector3(c.x, c.y, c.z) });

            if (isNew)
                AssetDatabase.CreateAsset(asset, OutPath);
            else
                EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ERMountDataImporter] 导入完成：dummies={asset.dummies.Count}, calibrationBones={asset.calibrationBones.Count} → {OutPath}");
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}
