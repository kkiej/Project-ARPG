using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 模块化装备部件索引（方案 B）。
    /// 按"部件名 code"（= ER 部件 prefab 文件名，如 BD_M_1350）映射到 prefab。
    ///
    /// 设计要点：
    /// - 数据驱动的"权威数据"放在装备 SO 上（ArmorItem.modularPartCode），此处只是 code -> prefab 的资源索引。
    /// - 不用 itemID 做 key（itemID 在 WorldItemDatabase 里按列表顺序运行时分配，会漂移）。
    /// - 直接引用 prefab（打包安全，不依赖 Addressables / 不占 Resources）。
    /// - 条目用编辑器一键扫描文件夹自动填充，无需手动逐个拖拽。
    /// </summary>
    [CreateAssetMenu(menuName = "ARPG/Equipment Part Catalog")]
    public class EquipmentPartCatalog : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            [Tooltip("部件名，等于 prefab 文件名，如 BD_M_1350。")]
            public string code;

            public GameObject prefab;
        }

        [Header("自动填充")]
        [Tooltip("点击右键菜单 'Auto Populate From Folder' 时，扫描此目录（含子目录）下所有含 SkinnedMeshRenderer 的\n" +
                 "GameObject 资产（.fbx / .prefab 均可），以资产名为 code 自动填入 entries。\n" +
                 "项目相对路径，如 Assets/_ELDENRING_REF/Parts")]
        [SerializeField] private string sourceFolder = "Assets/_ELDENRING_REF/Parts";

        [SerializeField] private List<Entry> entries = new();

        //  运行时索引：code(大写) -> prefab，大小写不敏感
        private Dictionary<string, GameObject> lookup;

        private void OnEnable() => lookup = null; // 资产重载后失效，下次访问重建

        private static string Key(string code) => string.IsNullOrEmpty(code) ? code : code.Trim().ToUpperInvariant();

        private void BuildLookup()
        {
            lookup = new Dictionary<string, GameObject>(entries.Count);
            foreach (Entry entry in entries)
            {
                if (string.IsNullOrEmpty(entry.code) || entry.prefab == null) continue;
                lookup[Key(entry.code)] = entry.prefab;
            }
        }

        /// <summary>按部件名 code（如 BD_M_1350）取 prefab，找不到返回 null（大小写不敏感）。</summary>
        public GameObject GetPrefabByCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;
            if (lookup == null) BuildLookup();
            return lookup.TryGetValue(Key(code), out GameObject prefab) ? prefab : null;
        }

        public bool HasCode(string code) => GetPrefabByCode(code) != null;

        public IReadOnlyList<Entry> Entries => entries;

#if UNITY_EDITOR
        /// <summary>
        /// 扫描 sourceFolder 下所有含 SkinnedMeshRenderer 的 GameObject 资产（.fbx / .prefab），
        /// 以资产名为 code 填充 entries（覆盖式重建）。仅编辑器可用。
        /// </summary>
        [ContextMenu("Auto Populate From Folder")]
        public void AutoPopulateFromFolder()
        {
            if (string.IsNullOrEmpty(sourceFolder) || !UnityEditor.AssetDatabase.IsValidFolder(sourceFolder))
            {
                Debug.LogError($"[EquipmentPartCatalog] sourceFolder 无效：'{sourceFolder}'", this);
                return;
            }

            //  t:GameObject 同时覆盖导入的 FBX 模型根与 prefab
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:GameObject", new[] { sourceFolder });
            entries.Clear();

            int skippedNoSmr = 0, skippedDup = 0;
            var seen = new HashSet<string>();

            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) continue;

                //  只收带蒙皮网格的资产，过滤掉无关 GameObject
                if (go.GetComponentInChildren<SkinnedMeshRenderer>(true) == null)
                {
                    skippedNoSmr++;
                    continue;
                }

                string key = Key(go.name);
                if (!seen.Add(key))
                {
                    skippedDup++;
                    Debug.LogWarning($"[EquipmentPartCatalog] 重名部件 '{go.name}'（{path}）已忽略，仅保留第一个。", go);
                    continue;
                }

                entries.Add(new Entry { code = go.name, prefab = go });
            }

            entries.Sort((a, b) => string.CompareOrdinal(a.code, b.code));
            lookup = null;

            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[EquipmentPartCatalog] 已从 '{sourceFolder}' 填充 {entries.Count} 个部件" +
                      $"（跳过无 SMR {skippedNoSmr} 个，重名 {skippedDup} 个）。", this);
        }
#endif
    }
}
