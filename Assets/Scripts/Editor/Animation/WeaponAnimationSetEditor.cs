using System.Collections.Generic;
using Animancer;
using LZ;
using UnityEditor;
using UnityEngine;

namespace LZEditor
{
    [CustomEditor(typeof(WeaponAnimationSet))]
    public class WeaponAnimationSetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var baseAnimDataProp = serializedObject.FindProperty("baseAnimData");
            EditorGUILayout.PropertyField(baseAnimDataProp);

            var overridesProp = serializedObject.FindProperty("locomotionClipOverrides");

            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Sync");

                GUI.enabled = baseAnimDataProp.objectReferenceValue != null;
                if (GUILayout.Button("Sync Override List From Animation Data", GUILayout.Height(24)))
                {
                    SyncOverrideList((WeaponAnimationSet)target);
                }
                GUI.enabled = true;
            }

            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(overridesProp, true);

            EditorGUILayout.Space(8);

            DrawPropertiesExcluding(serializedObject,
                "m_Script", "baseAnimData", "locomotionClipOverrides");

            serializedObject.ApplyModifiedProperties();
        }

        private static void SyncOverrideList(WeaponAnimationSet set)
        {
            Undo.RecordObject(set, "Sync Override List");

            var baseAnimData = set.baseAnimData;
            if (baseAnimData == null) return;

            var clips = new List<AnimationClip>();

            AddIfNotNull(clips, baseAnimData.idle1H);
            CollectMixerClips(clips, baseAnimData.locomotion1H);
            AddIfNotNull(clips, baseAnimData.idle2H);
            CollectMixerClips(clips, baseAnimData.locomotion2H);
            AddIfNotNull(clips, baseAnimData.blockingIdle1H);
            CollectMixerClips(clips, baseAnimData.blockingLocomotion1H);
            AddIfNotNull(clips, baseAnimData.blockingIdle2H);
            CollectMixerClips(clips, baseAnimData.blockingLocomotion2H);

            var existing = new Dictionary<AnimationClip, AnimationClip>();
            if (set.locomotionClipOverrides != null)
                foreach (var e in set.locomotionClipOverrides)
                    if (e.original != null && e.replacement != null)
                        existing[e.original] = e.replacement;

            set.locomotionClipOverrides = new ClipOverride[clips.Count];
            for (int i = 0; i < clips.Count; i++)
            {
                set.locomotionClipOverrides[i].original = clips[i];
                existing.TryGetValue(clips[i], out set.locomotionClipOverrides[i].replacement);
            }

            EditorUtility.SetDirty(set);
            Debug.Log($"[{set.name}] 已同步 {clips.Count} 个 locomotion clip", set);
        }

        private static void AddIfNotNull(List<AnimationClip> list, AnimationClip clip)
        {
            if (clip != null) list.Add(clip);
        }

        private static void CollectMixerClips(List<AnimationClip> list, MixerTransition2D mixer)
        {
            if (mixer?.Animations == null) return;
            foreach (var obj in mixer.Animations)
                if (obj is AnimationClip clip)
                    list.Add(clip);
        }
    }
}
