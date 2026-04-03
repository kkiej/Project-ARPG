using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace vietlabs.fr2
{
    [InitializeOnLoad]
    internal class FR2_CacheHelper : AssetPostprocessor
    {
        [NonSerialized] private static HashSet<string> scenes;
        [NonSerialized] private static HashSet<string> guidsIgnore;
        [NonSerialized] internal static bool inited = false; 
        
        static FR2_CacheHelper()
        {
            try
            {
                EditorApplication.update -= InitHelper;
                EditorApplication.update += InitHelper;
            }
            catch (Exception e)
            {
                FR2_LOG.LogWarning(e);
            }
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (FR2_SettingExt.disable) return;
            FR2_Cache.MarkAssetPathContentDirty(importedAssets);
            FR2_Cache.MarkAssetPathChanged(movedAssets);
            FR2_Cache.MarkAssetPathDeleted(deletedAssets);
            
            if (FR2_Cache.autoRefresh) FR2_Cache.IncrementalRefresh();
            FR2_LOG.Log($"AutoRefresh = {FR2_Cache.autoRefresh} => {nameof(OnPostprocessAllAssets)}: imported = {importedAssets.Length}, deleted = {deletedAssets.Length}, moved = {movedFromAssetPaths.Length}" );
        }
        
        internal static void InitHelper()
        {
            if (FR2_Unity.isEditorCompiling || FR2_Unity.isEditorUpdating) return;
            if (!FR2_Cache.isReady) return;
            EditorApplication.update -= InitHelper;
            
            inited = true;
            InitListScene();
            InitIgnore();
            CheckGitStatus(false);
            
#if UNITY_2018_1_OR_NEWER
            EditorBuildSettings.sceneListChanged -= InitListScene;
            EditorBuildSettings.sceneListChanged += InitListScene;
#endif

            #if UNITY_2022_1_OR_NEWER
            EditorApplication.projectWindowItemInstanceOnGUI -= OnGUIProjectInstance;
            EditorApplication.projectWindowItemInstanceOnGUI += OnGUIProjectInstance;
            #else
            EditorApplication.projectWindowItemOnGUI -= OnGUIProjectItem;
            EditorApplication.projectWindowItemOnGUI += OnGUIProjectItem;
            #endif

            InitIgnore();
            // force repaint all project panels
            EditorApplication.RepaintProjectWindow();
        }
        
        private static void CheckGitStatus(bool force)
        {
            if (FR2_SettingExt.gitIgnoreAdded && !force) return;
            FR2_SettingExt.isGitProject = FR2_GitUtil.IsGitProject();
            if (!FR2_SettingExt.isGitProject) return;
            FR2_SettingExt.gitIgnoreAdded = FR2_GitUtil.CheckGitIgnoreContainsFR2Cache();
        }
        
        public static void InitIgnore()
        {
            guidsIgnore = new HashSet<string>();
            foreach (string item in FR2_Setting.IgnoreAsset)
            {
                string guid = AssetDatabase.AssetPathToGUID(item);
                guidsIgnore.Add(guid);
            }
            
            // Debug.Log($"Init Ignore: {guidsIgnore.Count} items");
        }

        private static void InitListScene()
        {
            scenes = new HashSet<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                var sce = AssetDatabase.AssetPathToGUID(scene.path);
                if (!string.IsNullOrEmpty(sce)) scenes.Add(sce);
            }
        }

        private static string lastGUID;
        private static readonly Dictionary<int, GUIContent> _countContentCache = new Dictionary<int, GUIContent>();
        private static GUIContent _plusContent;

        private static void OnGUIProjectInstance(int instanceID, Rect selectionRect)
        {
            if (FR2_SettingExt.disable) return;
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj == null) return;
            var (guid, localId) = FR2_SelectionManager.GetCachedGuidAndLocalId(obj);
            if (string.IsNullOrEmpty(guid)) return;

            bool isMainAsset = guid != lastGUID;
            lastGUID = guid;

            if (isMainAsset)
            {
                DrawProjectItem(guid, selectionRect);
                return;
            }
            
            if (!FR2_Cache._inst._setting.showSubAssetFileId) return;
            var rect2 = selectionRect;
            
            // Cache the localId GUIContent to avoid repeated creation
            if (!_countContentCache.TryGetValue((int)localId, out GUIContent label))
            {
                label = new GUIContent(localId.ToString());
                _countContentCache[(int)localId] = label;
            }
            
            rect2.xMin = rect2.xMax - EditorStyles.miniLabel.CalcSize(label).x;

            using (FR2_Scope.GUIColor(new Color(.5f, .5f, .5f, 0.5f)))
            {
                GUI.Label(rect2, label, EditorStyles.miniLabel);
            }
        }

        private static void OnGUIProjectItem(string guid, Rect rect)
        {
            if (FR2_SettingExt.disable) return;
            bool isMainAsset = guid != lastGUID;
            lastGUID = guid;
            if (isMainAsset) DrawProjectItem(guid, rect);
        }

        private static void DrawProjectItem(string guid, Rect rect)
        {
            var r = new Rect(rect.x, rect.y, 1f, 16f);
            if (scenes.Contains(guid))
            {
                EditorGUI.DrawRect(r, GUI2.Theme(new Color32(72, 150, 191, 255), Color.blue));
            }
            else if (guidsIgnore.Contains(guid))
            {
                var ignoreRect = new Rect(rect.x + 3f, rect.y + 6f, 2f, 2f);
                EditorGUI.DrawRect(ignoreRect, GUI2.darkRed);
            }
            
            if (!FR2_Cache.isReady) return; // not ready
            if (!FR2_Setting.ShowReferenceCount) return;

            var api = FR2_Cache._inst;
            if (FR2_Cache._map == null) FR2_Cache.Check4Changes(false);
            if (!FR2_Cache._map.TryGetValue(guid, out FR2_Asset item)) return;

            if (item == null) return;
            if (item.UsedByMap.Count > 0)
            {
                // Cache GUIContent to avoid allocation
                int count = item.UsedByMap.Count;
                if (!_countContentCache.TryGetValue(count, out GUIContent content))
                {
                    content = FR2_GUIContent.FromString(count.ToString());
                    _countContentCache[count] = content;
                }
                
                r.width = 0f;
                r.xMin -= 100f;
                GUI.Label(r, content, GUI2.miniLabelAlignRight);
            }
            else if (item.forcedIncludedInBuild)
            {
                using (FR2_Scope.GUIColor(GUI.color.Alpha(0.2f)))
                {
                    // Cache plus content
                    if (_plusContent == null) _plusContent = FR2_GUIContent.FromString("+");
                    
                    r.width = 0f;
                    r.xMin -= 100f;
                    GUI.Label(r, _plusContent, GUI2.miniLabelAlignRight);
                }
            }
        }
    }
}
