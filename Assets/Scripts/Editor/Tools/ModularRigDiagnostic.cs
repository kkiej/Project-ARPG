using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LZ.EditorTools
{
    public class ModularRigDiagnostic : EditorWindow
    {
        GameObject skeletonFbx;
        GameObject partFbx;

        [MenuItem("Tools/Modular Rig/Diagnostic")]
        public static void Open() => GetWindow<ModularRigDiagnostic>("Rig Diag");

        void OnGUI()
        {
            skeletonFbx = (GameObject)EditorGUILayout.ObjectField("Skeleton FBX", skeletonFbx, typeof(GameObject), false);
            partFbx     = (GameObject)EditorGUILayout.ObjectField("Part FBX",     partFbx,     typeof(GameObject), false);

            using (new EditorGUI.DisabledScope(skeletonFbx == null || partFbx == null))
                if (GUILayout.Button("Compare", GUILayout.Height(28))) Compare();
        }

        void Compare()
        {
            var skelPath = AssetDatabase.GetAssetPath(skeletonFbx);
            var partPath = AssetDatabase.GetAssetPath(partFbx);
            var skelImp = AssetImporter.GetAtPath(skelPath) as ModelImporter;
            var partImp = AssetImporter.GetAtPath(partPath) as ModelImporter;

            Debug.Log($"=== Import Settings ===");
            Debug.Log($"Skeleton: globalScale={skelImp.globalScale}  useFileScale={skelImp.useFileScale}  fileScale={skelImp.fileScale}  bakeAxisConversion={skelImp.bakeAxisConversion}");
            Debug.Log($"Part:     globalScale={partImp.globalScale}  useFileScale={partImp.useFileScale}  fileScale={partImp.fileScale}  bakeAxisConversion={partImp.bakeAxisConversion}");

            var skel = (GameObject)PrefabUtility.InstantiatePrefab(skeletonFbx);
            var part = (GameObject)PrefabUtility.InstantiatePrefab(partFbx);
            try
            {
                Debug.Log($"Skeleton root lossyScale: {skel.transform.lossyScale}");
                Debug.Log($"Part root lossyScale:     {part.transform.lossyScale}");

                var skelBones = skel.GetComponentsInChildren<Transform>(true)
                    .GroupBy(t => t.name).ToDictionary(g => g.Key, g => g.First());

                int match = 0, mismatchPos = 0, mismatchScale = 0;
                float maxPosDelta = 0, maxScaleRatio = 1;
                var samples = new List<string>();

                foreach (var smr in part.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (smr.sharedMesh == null) continue;
                    var bones = smr.bones;
                    for (int i = 0; i < bones.Length; i++)
                    {
                        var pBone = bones[i];
                        if (pBone == null) continue;
                        if (!skelBones.TryGetValue(pBone.name, out var sBone)) continue;

                        Vector3 pPos = pBone.position, sPos = sBone.position;
                        Vector3 pScale = pBone.lossyScale, sScale = sBone.lossyScale;
                        float posDelta = Vector3.Distance(pPos, sPos);
                        float scaleRatio = sScale.x / Mathf.Max(1e-6f, pScale.x);

                        if (posDelta > 0.0005f) mismatchPos++;
                        if (Mathf.Abs(scaleRatio - 1f) > 0.01f) mismatchScale++;
                        if (posDelta <= 0.0005f && Mathf.Abs(scaleRatio - 1f) <= 0.01f) match++;

                        maxPosDelta = Mathf.Max(maxPosDelta, posDelta);
                        if (Mathf.Abs(Mathf.Log(scaleRatio)) > Mathf.Abs(Mathf.Log(maxScaleRatio))) maxScaleRatio = scaleRatio;

                        if (samples.Count < 5 && (posDelta > 0.0005f || Mathf.Abs(scaleRatio - 1f) > 0.01f))
                            samples.Add($"[{pBone.name}] partWorldPos={pPos:F4} skelWorldPos={sPos:F4} dist={posDelta:F4} scaleRatio(skel/part)={scaleRatio:F3}");
                    }
                }

                Debug.Log($"=== Bind-time Bone Comparison ===");
                Debug.Log($"match={match}, posMismatch={mismatchPos}, scaleMismatch={mismatchScale}, maxPosDelta={maxPosDelta:F4}, maxScaleRatio(skel/part)={maxScaleRatio:F3}");
                foreach (var s in samples) Debug.LogWarning(s);
            }
            finally
            {
                DestroyImmediate(skel);
                DestroyImmediate(part);
            }
        }
    }
}