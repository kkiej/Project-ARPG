using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MaterialPassManager))]
public class MaterialPassManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MaterialPassManager manager = (MaterialPassManager)target;

        if (GUILayout.Button("Add Material"))
        {
            manager.materials.Add(new MaterialPass());
        }

        for (int i = 0; i < manager.materials.Count; i++)
        {
            var matPass = manager.materials[i];

            EditorGUILayout.BeginVertical("box");
            matPass.material = (Material)EditorGUILayout.ObjectField("Material", matPass.material, typeof(Material), false);

            if (matPass.material != null)
            {
                if (matPass.passEnabled == null || matPass.passEnabled.Count != matPass.material.passCount)
                {
                    matPass.passEnabled = new List<bool>(new bool[matPass.material.passCount]);
                }

                for (int j = 0; j < matPass.material.passCount; j++)
                {
                    string passName = matPass.material.GetPassName(j);
                    matPass.passEnabled[j] = EditorGUILayout.Toggle(passName, matPass.passEnabled[j]);
                }
            }

            if (GUILayout.Button("Remove"))
            {
                manager.materials.RemoveAt(i);
            }

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Apply Pass Settings"))
        {
            manager.ApplyPassSettings();
        }

        EditorUtility.SetDirty(manager);
    }
}