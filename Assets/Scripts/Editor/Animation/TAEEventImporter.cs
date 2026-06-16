using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LZ.Editor
{
    public class TAEEventImporter : EditorWindow
    {
        private string _jsonFolderPath = "Assets/_ELDENRING_REF/TAE_Events";
        private string _outputFolder = "Assets/_ELDENRING_REF/TAE_Events_SO";
        private bool _importing;

        [MenuItem("Tools/TAE Event Importer")]
        public static void ShowWindow()
        {
            GetWindow<TAEEventImporter>("TAE Event Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("TAE JSON → ScriptableObject", EditorStyles.boldLabel);
            GUILayout.Space(10);

            _jsonFolderPath = EditorGUILayout.TextField("JSON Folder", _jsonFolderPath);
            _outputFolder = EditorGUILayout.TextField("Output Folder", _outputFolder);

            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(_importing);
            if (GUILayout.Button("Import All TAE JSON → SO", GUILayout.Height(30)))
            {
                ImportAll();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void ImportAll()
        {
            _importing = true;
            try
            {
                string fullJsonPath = Path.Combine(Application.dataPath,
                    _jsonFolderPath.Replace("Assets/", "").Replace("Assets\\", ""));
                string fullOutputPath = Path.Combine(Application.dataPath,
                    _outputFolder.Replace("Assets/", "").Replace("Assets\\", ""));

                if (!Directory.Exists(fullJsonPath))
                {
                    EditorUtility.DisplayDialog("Error", $"JSON folder not found:\n{fullJsonPath}", "OK");
                    return;
                }

                Directory.CreateDirectory(fullOutputPath);

                var jsonFiles = Directory.GetFiles(fullJsonPath, "*.json");
                int success = 0;
                int failed = 0;

                for (int i = 0; i < jsonFiles.Length; i++)
                {
                    string file = jsonFiles[i];
                    string fileName = Path.GetFileNameWithoutExtension(file);

                    if (fileName.StartsWith("_")) continue; // skip _summary.json

                    EditorUtility.DisplayProgressBar("Importing TAE",
                        $"{fileName} ({i + 1}/{jsonFiles.Length})",
                        (float)i / jsonFiles.Length);

                    try
                    {
                        string json = File.ReadAllText(file);
                        var data = ParseTAEJson(json);
                        if (data == null)
                        {
                            failed++;
                            continue;
                        }

                        string assetPath = $"{_outputFolder}/{fileName}.asset";
                        var existing = AssetDatabase.LoadAssetAtPath<TAEEventData>(assetPath);
                        if (existing != null)
                        {
                            existing.tae = data.tae;
                            existing.taeId = data.taeId;
                            existing.animations = data.animations;
                            EditorUtility.SetDirty(existing);
                        }
                        else
                        {
                            AssetDatabase.CreateAsset(data, assetPath);
                        }

                        success++;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to import {fileName}: {e.Message}");
                        failed++;
                    }
                }

                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("TAE Import Complete",
                    $"Success: {success}\nFailed: {failed}\nOutput: {_outputFolder}", "OK");
            }
            finally
            {
                _importing = false;
            }
        }

        private static TAEEventData ParseTAEJson(string json)
        {
            var root = JsonUtility.FromJson<TAEJsonRoot>(json);
            if (root == null || root.animations == null) return null;

            var data = ScriptableObject.CreateInstance<TAEEventData>();
            data.tae = root.tae;
            data.taeId = root.taeId;

            var entries = new List<TAEAnimationEntry>(root.animations.Length);
            foreach (var anim in root.animations)
            {
                var entry = new TAEAnimationEntry
                {
                    animId = anim.animId,
                    rawId = anim.rawId,
                };

                if (anim.events != null && anim.events.Length > 0)
                {
                    var events = new TAEEvent[anim.events.Length];
                    for (int i = 0; i < anim.events.Length; i++)
                    {
                        var src = anim.events[i];
                        events[i] = new TAEEvent
                        {
                            type = src.type,
                            name = src.name ?? "",
                            category = src.category ?? "",
                            startTime = src.start,
                            endTime = src.end,
                            parameters = src.@params ?? Array.Empty<int>(),
                        };
                    }
                    entry.events = events;
                }
                else
                {
                    entry.events = Array.Empty<TAEEvent>();
                }

                entries.Add(entry);
            }

            data.animations = entries.ToArray();
            return data;
        }

        [Serializable]
        private class TAEJsonRoot
        {
            public string tae;
            public int taeId;
            public int animationCount;
            public int eventCount;
            public TAEJsonAnimation[] animations;
        }

        [Serializable]
        private class TAEJsonAnimation
        {
            public string animId;
            public int rawId;
            public TAEJsonEvent[] events;
        }

        [Serializable]
        private class TAEJsonEvent
        {
            public int type;
            public float start;
            public float end;
            public string name;
            public string category;
            public int[] @params;
        }
    }
}
