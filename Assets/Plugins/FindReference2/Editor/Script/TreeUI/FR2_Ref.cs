using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vietlabs.fr2
{

    internal class FR2_Ref
    {

        public FR2_Asset addBy;
        
        // Callback delegates for advanced bookmark operations (set by the drawer)
        public System.Action<FR2_Ref> OnCtrlClick;
        public System.Action<FR2_Ref> OnAltClick;
        public System.Action<FR2_Ref> OnShiftClick;
        public FR2_Asset asset;
        public Object component;
        public int depth;
        public string group;
        public int index;

        public bool isSceneRef;
        public int matchingScore;
        public int type;
        public List<SceneRefInfo> sceneReferenceInfo;
        
        // OPTIMIZED: HashSet for O(1) duplicate checking instead of List.Contains O(n)
        internal HashSet<SceneRefInfo> sceneReferenceInfoSet;

        public FR2_Ref()
        { }

        public FR2_Ref(int index, int depth, FR2_Asset asset, FR2_Asset by)
        {
            this.index = index;
            this.depth = depth;

            this.asset = asset;
            if (asset != null) type = FR2_AssetGroupDrawer.GetIndex(asset.extension);

            addBy = by;

            // isSceneRef = false;
        }

        public FR2_Ref(int index, int depth, FR2_Asset asset, FR2_Asset by, string group) : this(index, depth, asset,
            by)
        {
            this.group = group;

            // isSceneRef = false;
        }
        private static int CSVSorter(FR2_Ref item1, FR2_Ref item2)
        {
            int r = item1.depth.CompareTo(item2.depth);
            if (r != 0) return r;

            int t = item1.type.CompareTo(item2.type);
            if (t != 0) return t;

            return item1.index.CompareTo(item2.index);
        }


        public static FR2_Ref[] FromDict(Dictionary<string, FR2_Ref> dict)
        {
            if (dict == null || dict.Count == 0) return null;

            var result = new List<FR2_Ref>();

            foreach (KeyValuePair<string, FR2_Ref> kvp in dict)
            {
                if (kvp.Value == null) continue;
                if (kvp.Value.asset == null) continue;

                result.Add(kvp.Value);
            }

            result.Sort(CSVSorter);


            return result.ToArray();
        }

        public static FR2_Ref[] FromList(List<FR2_Ref> list)
        {
            if (list == null || list.Count == 0) return null;

            list.Sort(CSVSorter);
            var result = new List<FR2_Ref>();
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].asset == null) continue;
                result.Add(list[i]);
            }
            return result.ToArray();
        }

        public override string ToString()
        {
            if (isSceneRef)
            {
                var sr = (FR2_SceneRef)this;
                return sr.scenePath;
            }

            return asset.assetPath;
        }

        
        public string GetSceneObjId()
        {
            if (component == null) return string.Empty;

            return FR2_Cache.GetInstanceIdString(component.GetInstanceID());
        }
        

        public virtual bool isSelected()
        {
            return FR2_Bookmark.Contains(asset.guid);
        }
        public virtual void DrawToogleSelect(Rect r)
        {
            bool s = isSelected();
            r.width = 16f;
            
            Event evt = Event.current;
            bool isMouseOver = r.Contains(evt.mousePosition);
            bool isMouseDown = evt.type == EventType.MouseDown && evt.button == 0 && isMouseOver;
            
            // Handle modifier keys for advanced selection
            if (isMouseDown)
            {
                bool ctrl = Application.platform == RuntimePlatform.OSXEditor ? evt.command : evt.control;
                bool alt = evt.alt;
                bool shift = evt.shift;
                
                if (shift)
                {
                    // Shift+click: Toggle all items in all groups
                    OnShiftClick?.Invoke(this);
                    evt.Use();
                    return;
                }
                else if (alt)
                {
                    // Alt+click: Toggle all siblings in the same group
                    OnAltClick?.Invoke(this);
                    evt.Use();
                    return;
                }
                else if (ctrl)
                {
                    // Cmd+click (Mac) / Ctrl+click (PC): Toggle self and set all siblings to the same new state
                    OnCtrlClick?.Invoke(this);
                    evt.Use();
                    return;
                }
            }
            
            // Normal toggle behavior
            if (!GUI2.Toggle(r, ref s)) return;

            if (s)
            {
                FR2_Bookmark.Add(this);
            } else
            {
                FR2_Bookmark.Remove(this);
            }
        }

        // Removed - now handled via instance callbacks
        
        // Removed - now handled via instance callbacks
        
        // Removed - now handled via instance callbacks
        
        // Removed - now handled via instance callbacks
        


        // public FR2_Ref(int depth, UnityEngine.Object target)
        // {
        // 	this.component = target;
        // 	this.depth = depth;
        // 	// isSceneRef = true;
        // }
        internal List<FR2_Ref> Append(Dictionary<string, FR2_Ref> dict, params string[] guidList)
        {
            var result = new List<FR2_Ref>();
            if (!FR2_Cache.isReady)
            {
                FR2_LOG.LogWarning("Cache not yet ready! Please wait!");
                return result;
            }

            // var excludePackage = !FR2_Cache._inst.setting.showPackageAsset;
            //filter to remove items that already in dictionary
            for (var i = 0; i < guidList.Length; i++)
            {
                string guid = guidList[i];
                if (dict.ContainsKey(guid)) continue;

                FR2_Asset child = FR2_Cache.GetAsset(guid);
                if (child == null) continue;
                // if (excludePackage && child.inPackages) continue;

                var r = new FR2_Ref(dict.Count, depth + 1, child, asset);
                dict.Add(guid, r);
                result.Add(r);
            }

            return result;
        }

        internal void AppendUsedBy(Dictionary<string, FR2_Ref> result, bool deep)
        {
            var h = asset.UsedByMap;
            if (h == null || h.Count == 0) return;
            
            var list = deep ? new List<FR2_Ref>() : null;
            
            // Pre-allocate capacity hint for better performance
            if (deep && list.Capacity < h.Count)
            {
                list.Capacity = h.Count;
            }
            // bool excludePackage = !FR2_Cache._inst.setting.showPackageAsset;

            foreach (KeyValuePair<string, FR2_Asset> kvp in h)
            {
                string guid = kvp.Key;
                if (result.ContainsKey(guid)) continue;

                FR2_Asset child = FR2_Cache.GetAsset(guid);
                if (child == null) continue;
                if (child.IsMissing) continue;
                // if (excludePackage && child.inPackages) continue;

                var r = new FR2_Ref(result.Count, depth + 1, child, asset);
                result.Add(guid, r);

                if (deep) list.Add(r);
            }

            if (!deep || list.Count == 0) return;

            foreach (FR2_Ref item in list)
            {
                item.AppendUsedBy(result, true);
            }
        }

        internal void AppendUsage(Dictionary<string, FR2_Ref> result, bool deep)
        {
            Dictionary<string, HashSet<long>> h = asset.UseGUIDs;
            if (h == null || h.Count == 0) return;
            
            List<FR2_Ref> list = deep ? new List<FR2_Ref>(h.Count) : null;
            // bool excludePackage = !FR2_Cache._inst.setting.showPackageAsset;
            
            foreach (KeyValuePair<string, HashSet<long>> kvp in h)
            {
                string guid = kvp.Key;
                // OPTIMIZED: Use TryAdd pattern to avoid double lookup
                if (result.ContainsKey(guid)) continue;

                FR2_Asset child = FR2_Cache.GetAsset(guid);
                if (child == null || child.IsMissing) continue;
                // if (excludePackage && child.inPackages) continue;

                var r = new FR2_Ref(result.Count, depth + 1, child, asset);
                result.Add(guid, r);

                list?.Add(r);
            }

            if (list == null || list.Count == 0) return;

            // OPTIMIZED: Use for loop instead of foreach to avoid enumerator allocation
            for (var i = 0; i < list.Count; i++)
            {
                list[i].AppendUsage(result, true);
            }
        }

        // --------------------- STATIC UTILS -----------------------


        internal static Dictionary<string, FR2_Ref> FindRefs(string[] guids, bool usageOrUsedBy, bool addFolder)
        {
            if (guids == null || guids.Length == 0) 
                return new Dictionary<string, FR2_Ref>();
            
            // Pre-allocate with reasonable capacity to reduce resizing
            var dict = new Dictionary<string, FR2_Ref>(guids.Length * 10);
            var list = new List<FR2_Ref>(guids.Length);
            var selectedGuids = new HashSet<string>(guids);
            // bool excludePackage = !FR2_Cache._inst.setting.showPackageAsset;

            for (var i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                if (dict.ContainsKey(guid)) continue;

                FR2_Asset asset = FR2_Cache.GetAsset(guid);
                if (asset == null) continue;
                // if (excludePackage && asset.inPackages) continue;

                var r = new FR2_Ref(i, 0, asset, null);
                if (!asset.IsFolder || addFolder) dict.Add(guid, r);

                list.Add(r);
            }

            for (var i = 0; i < list.Count; i++)
            {
                if (usageOrUsedBy)
                {
                    list[i].AppendUsage(dict, true);
                } else
                {
                    list[i].AppendUsedBy(dict, true);
                }
            }

            // OPTIMIZED: Filter in-place to avoid extra dictionary allocation
            var keysToRemove = new List<string>(dict.Count / 4); // Estimate 25% removal
            foreach (KeyValuePair<string, FR2_Ref> kvp in dict)
            {
                if (kvp.Value.depth == 0 || selectedGuids.Contains(kvp.Key))
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            
            // Remove in batch to avoid dictionary resize during enumeration
            for (var i = 0; i < keysToRemove.Count; i++)
            {
                dict.Remove(keysToRemove[i]);
            }

            return dict;
        }


        public static Dictionary<string, FR2_Ref> FindUsage(string[] guids)
        {
            return FindRefs(guids, true, true);
        }

        public static Dictionary<string, FR2_Ref> FindUsedBy(string[] guids)
        {
            return FindRefs(guids, false, true);
        }

        public static Dictionary<string, FR2_Ref> FindUsageScene(GameObject[] objs, bool depth)
        {
            var dict = new Dictionary<string, FR2_Ref>();

            // var list = new List<FR2_Ref>();

            for (var i = 0; i < objs.Length; i++)
            {
                if (objs[i].IsAssetObject()) continue; //only get in scene 

                //add selection
                if (!dict.ContainsKey(objs[i].GetInstanceID().ToString())) dict.Add(objs[i].GetInstanceID().ToString(), new FR2_SceneRef(0, objs[i]));

                foreach (Object item in FR2_Unity.GetAllRefObjects(objs[i]))
                {
                    AppendUsageScene(dict, item);
                }

                if (!depth) continue;
                foreach (GameObject child in objs[i].GetAllChildren(false))
                {
                    foreach (Object item2 in FR2_Unity.GetAllRefObjects(child))
                    {
                        AppendUsageScene(dict, item2);
                    }
                }
            }

            return dict;
        }

        public static Dictionary<string, FR2_Ref> FindUsageSceneWithDetails(GameObject[] objs, bool depth)
        {
            if (!FR2_SceneCache.isReady)
            {
                FR2_LOG.LogWarning("FindUsageSceneWithDetails called when scene cache not ready - this should not happen");
                return new Dictionary<string, FR2_Ref>();
            }
            
            FR2_LOG.Log(nameof(FindUsageSceneWithDetails));
            
            var dict = new Dictionary<string, FR2_Ref>();
            var sceneCache = FR2_SceneCache.Api.cache;

            for (var i = 0; i < objs.Length; i++)
            {
                if (objs[i].IsAssetObject()) continue;

                var instanceId = FR2_SceneCache.GetCachedInstanceIdString(objs[i].GetInstanceID());
                if (!dict.ContainsKey(instanceId)) 
                    dict.Add(instanceId, new FR2_SceneRef(0, objs[i]));

                CollectFromSceneCache(dict, objs[i], sceneCache);

                if (!depth) continue;
                foreach (GameObject child in objs[i].GetAllChildren(false))
                {
                    CollectFromSceneCache(dict, child, sceneCache);
                }
            }

            return dict;
        }

        private static void CollectFromSceneCache(Dictionary<string, FR2_Ref> dict, GameObject gameObject, Dictionary<Component, HashSet<FR2_SceneCache.HashValue>> sceneCache)
        {
            var components = FR2_SceneCache.Api.GetCachedComponents(gameObject);
            foreach (var component in components)
            {
                if (component == null) continue;
                
                if (!sceneCache.TryGetValue(component, out var hashValues)) continue;
                
                foreach (var hashValue in hashValues)
                {
                    if (hashValue.target == null || hashValue.isSceneObject) continue;
                    
                    AppendUsageSceneWithRef(dict, hashValue.target, component, hashValue.propertyPath);
                }
            }
        }

        private static void AppendUsageScene(Dictionary<string, FR2_Ref> dict, Object obj)
        {
            if (obj == null) return;
            
            // OPTIMIZED: Use cached GUID lookup
            if (!objectToGuidCache.TryGetValue(obj, out string guid))
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) return;

                guid = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(guid)) return;
                
                objectToGuidCache[obj] = guid;
            }
            
            if (dict.ContainsKey(guid)) return;

            FR2_Asset asset = FR2_Cache.GetAsset(guid);
            if (asset == null) return;

            // if (!FR2_Cache._inst.setting.showPackageAsset && asset.inPackages) return;
            var r = new FR2_Ref(0, 1, asset, null);
            dict.Add(guid, r);
        }

        // OPTIMIZED: Cache for Object to GUID mapping to avoid repeated AssetDatabase calls
        private static readonly Dictionary<Object, string> objectToGuidCache = new Dictionary<Object, string>();
        
        public static void ClearObjectToGuidCache()
        {
            objectToGuidCache.Clear();
        }
        
        private static void AppendUsageSceneWithRef(Dictionary<string, FR2_Ref> dict, Object obj, Component sourceComponent, string propertyPath)
        {
            // OPTIMIZED: Use cached GUID lookup
            if (!objectToGuidCache.TryGetValue(obj, out string guid))
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) return;

                guid = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(guid)) return;
                
                objectToGuidCache[obj] = guid;
            }

            var asset = FR2_Cache.GetAsset(guid);
            if (asset == null) return;

            if (dict.TryGetValue(guid, out FR2_Ref existingRef))
            {
                // OPTIMIZED: Use HashSet for O(1) duplicate checking instead of List.Contains O(n)
                if (existingRef.sceneReferenceInfo == null)
                {
                    existingRef.sceneReferenceInfo = new List<SceneRefInfo>();
                    existingRef.sceneReferenceInfoSet = new HashSet<SceneRefInfo>();
                }
                
                var refInfo = new SceneRefInfo
                {
                    sourceComponent = sourceComponent,
                    target = obj,
                    propertyPath = propertyPath,
                    isSceneObject = false
                };
                
                // OPTIMIZED: O(1) HashSet.Add instead of O(n) List.Contains + List.Add
                if (existingRef.sceneReferenceInfoSet.Add(refInfo))
                {
                    existingRef.sceneReferenceInfo.Add(refInfo);
                }
                return;
            }

            // Create new regular ref with reference information
            var newRef = new FR2_Ref(0, 1, asset, null);
            var newRefInfo = new SceneRefInfo
            {
                sourceComponent = sourceComponent,
                target = obj,
                propertyPath = propertyPath,
                isSceneObject = false
            };
            
            newRef.sceneReferenceInfo = new List<SceneRefInfo> { newRefInfo };
            newRef.sceneReferenceInfoSet = new HashSet<SceneRefInfo> { newRefInfo };
            
            dict.Add(guid, newRef);
        }
    }


}
