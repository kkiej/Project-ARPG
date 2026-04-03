using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace LZ.Editor
{
    [CustomEditor(typeof(AnimationClipRegistry))]
    public class AnimationClipRegistryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var registry = (AnimationClipRegistry)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("工具", EditorStyles.boldLabel);

            if (GUILayout.Button("从指定文件夹的 AnimatorController / OverrideController 收集 Clip"))
            {
                var folder = PickFolder("选择搜索 AnimatorController 的文件夹");
                if (folder != null)
                    CollectClipsFromAnimatorAssets(registry, folder);
            }

            if (GUILayout.Button("从指定文件夹搜索所有 AnimationClip"))
            {
                var folder = PickFolder("选择搜索 AnimationClip 的文件夹");
                if (folder != null)
                    CollectAllAnimationClips(registry, folder);
            }

            EditorGUILayout.Space(6);

            if (GUILayout.Button("移除重复 Clip"))
            {
                RemoveDuplicateClips(registry);
            }

            if (GUILayout.Button("移除空 (None) 条目"))
            {
                RemoveNullEntries(registry);
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "开发阶段可自由增删和排序。\n" +
                "上线后如需删除 clip，建议设为 None 保留索引，以避免网络 ID 错位。",
                MessageType.Info);
        }

        private string PickFolder(string title)
        {
            string absPath = EditorUtility.OpenFolderPanel(title, "Assets", "");
            if (string.IsNullOrEmpty(absPath)) return null;

            // 转换为相对于项目的 Assets 路径
            string projectPath = Application.dataPath; // …/Assets
            if (!absPath.StartsWith(projectPath))
            {
                Debug.LogWarning("[AnimationClipRegistry] 请选择项目 Assets 目录下的文件夹。");
                return null;
            }

            string relative = "Assets" + absPath.Substring(projectPath.Length);
            return relative;
        }

        private void CollectClipsFromAnimatorAssets(AnimationClipRegistry registry, string searchFolder)
        {
            var clips = registry.EditorGetClipList();
            var existingSet = new HashSet<AnimationClip>(clips.Where(c => c != null));
            int addedCount = 0;

            var overrideGuids = AssetDatabase.FindAssets("t:AnimatorOverrideController", new[] { searchFolder });
            foreach (var guid in overrideGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var overrideCtrl = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);
                if (overrideCtrl == null) continue;

                var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideCtrl.overridesCount);
                overrideCtrl.GetOverrides(overrides);

                foreach (var pair in overrides)
                {
                    if (pair.Value != null && existingSet.Add(pair.Value))
                    {
                        clips.Add(pair.Value);
                        addedCount++;
                    }
                    if (pair.Key != null && existingSet.Add(pair.Key))
                    {
                        clips.Add(pair.Key);
                        addedCount++;
                    }
                }
            }

            var controllerGuids = AssetDatabase.FindAssets("t:AnimatorController", new[] { searchFolder });
            foreach (var guid in controllerGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                if (ctrl == null) continue;

                foreach (var layer in ctrl.layers)
                {
                    CollectClipsFromStateMachine(layer.stateMachine, existingSet, clips, ref addedCount);
                }
            }

            SortClips(clips);
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AnimationClipRegistry] 从 \"{searchFolder}\" 中的 Animator 资产收集完毕，新增 {addedCount} 个 clip，总计 {clips.Count} 个。");
        }

        private void CollectClipsFromStateMachine(
            AnimatorStateMachine sm,
            HashSet<AnimationClip> existingSet,
            List<AnimationClip> clips,
            ref int addedCount)
        {
            foreach (var state in sm.states)
            {
                CollectClipsFromMotion(state.state.motion, existingSet, clips, ref addedCount);
            }

            foreach (var sub in sm.stateMachines)
            {
                CollectClipsFromStateMachine(sub.stateMachine, existingSet, clips, ref addedCount);
            }
        }

        private void CollectClipsFromMotion(
            Motion motion,
            HashSet<AnimationClip> existingSet,
            List<AnimationClip> clips,
            ref int addedCount)
        {
            if (motion is AnimationClip clip)
            {
                if (existingSet.Add(clip))
                {
                    clips.Add(clip);
                    addedCount++;
                }
            }
            else if (motion is BlendTree blendTree)
            {
                foreach (var child in blendTree.children)
                {
                    CollectClipsFromMotion(child.motion, existingSet, clips, ref addedCount);
                }
            }
        }

        private void CollectAllAnimationClips(AnimationClipRegistry registry, string searchFolder)
        {
            var clips = registry.EditorGetClipList();
            var existingSet = new HashSet<AnimationClip>(clips.Where(c => c != null));
            int addedCount = 0;

            var guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { searchFolder });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var allAtPath = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var obj in allAtPath)
                {
                    if (obj is AnimationClip c && !c.name.StartsWith("__preview__") && existingSet.Add(c))
                    {
                        clips.Add(c);
                        addedCount++;
                    }
                }
            }

            SortClips(clips);
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AnimationClipRegistry] 从 \"{searchFolder}\" 收集完毕，新增 {addedCount} 个 clip，总计 {clips.Count} 个。");
        }

        private void RemoveDuplicateClips(AnimationClipRegistry registry)
        {
            Undo.RecordObject(registry, "Remove Duplicate Clips");
            var clips = registry.EditorGetClipList();
            var seen = new HashSet<AnimationClip>();
            int removed = 0;

            for (int i = clips.Count - 1; i >= 0; i--)
            {
                if (clips[i] == null) continue;
                if (!seen.Add(clips[i]))
                {
                    clips.RemoveAt(i);
                    removed++;
                }
            }

            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AnimationClipRegistry] 移除了 {removed} 个重复条目，剩余 {clips.Count} 个。");
        }

        private void RemoveNullEntries(AnimationClipRegistry registry)
        {
            Undo.RecordObject(registry, "Remove Null Entries");
            var clips = registry.EditorGetClipList();
            int removed = clips.RemoveAll(c => c == null);
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AnimationClipRegistry] 移除了 {removed} 个空条目，剩余 {clips.Count} 个。");
        }

        private static void SortClips(List<AnimationClip> clips)
        {
            clips.Sort((a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return 1;
                if (b == null) return -1;
                return string.Compare(a.name, b.name, System.StringComparison.Ordinal);
            });
        }
    }
}
