using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LZ.EditorTools
{
    /// <summary>
    /// 一键把 ER 护甲 SO 批量登记进场景里的 WorldItemDatabase：
    /// 按类型（头/身/腿/手）分类，去重追加到对应的 [SerializeField] 列表。
    /// 通过 SerializedObject 修改，保证脏标记/撤销/持久化正确。
    /// </summary>
    public class ERArmorDatabaseRegistrar : EditorWindow
    {
        private const string DefaultFolder = "Assets/Data/Items/Armor/ER";

        //  数据库上 4 个私有 [SerializeField] 列表的字段名（须与 WorldItemDatabase 保持一致）
        private static readonly (Type type, string prop)[] SlotMap =
        {
            (typeof(HeadEquipmentItem), "headEquipment"),
            (typeof(BodyEquipmentItem), "bodyEquipment"),
            (typeof(LegEquipmentItem),  "legEquipment"),
            (typeof(HandEquipmentItem), "handEquipment"),
        };

        private WorldItemDatabase database;
        private string sourceFolder = DefaultFolder;
        private bool sortByErRowId = true;
        private bool removeMissingRefs = true;
        private bool saveSceneAfter = true;

        private Vector2 scroll;
        private string lastReport = "";

        [MenuItem("Tools/ER Import/Register Armor To Database...")]
        public static void Open()
        {
            var w = GetWindow<ERArmorDatabaseRegistrar>("ER 护甲登记");
            w.minSize = new Vector2(460, 320);
            w.TryAutoFindDatabase();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("把 Armor/ER 下的护甲 SO 批量登记进 WorldItemDatabase", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                database = (WorldItemDatabase)EditorGUILayout.ObjectField(
                    "目标数据库", database, typeof(WorldItemDatabase), true);
                if (GUILayout.Button("自动查找", GUILayout.Width(70)))
                    TryAutoFindDatabase();
            }

            sourceFolder = EditorGUILayout.TextField("源文件夹", sourceFolder);
            sortByErRowId = EditorGUILayout.Toggle(
                new GUIContent("新项按 erRowId 排序", "新登记的项按 ER 行号升序追加（已存在项保持原顺序）"), sortByErRowId);
            removeMissingRefs = EditorGUILayout.Toggle(
                new GUIContent("清理空引用", "登记前先移除列表里已丢失(None)的引用"), removeMissingRefs);
            saveSceneAfter = EditorGUILayout.Toggle(
                new GUIContent("完成后保存场景", ""), saveSceneAfter);

            EditorGUILayout.Space(6);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("预览（只统计）", GUILayout.Height(28)))
                    Run(false);
                GUI.backgroundColor = new Color(0.5f, 0.85f, 0.5f);
                if (GUILayout.Button("扫描并登记", GUILayout.Height(28)))
                    Run(true);
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "itemID 由 WorldItemDatabase.Awake 按列表顺序运行时分配，无需手填。\n" +
                "登记只“追加去重”，不会重排已存在项。", MessageType.Info);

            if (!string.IsNullOrEmpty(lastReport))
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("结果", EditorStyles.boldLabel);
                scroll = EditorGUILayout.BeginScrollView(scroll);
                EditorGUILayout.TextArea(lastReport, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        private void TryAutoFindDatabase()
        {
            //  跨版本：找场景里已加载的实例（排除资产/预制体里的）
            var all = Resources.FindObjectsOfTypeAll<WorldItemDatabase>();
            database = all.FirstOrDefault(d =>
                d != null && d.gameObject.scene.IsValid() && d.gameObject.scene.isLoaded)
                ?? all.FirstOrDefault();
            if (database == null)
                lastReport = "未在已打开的场景中找到 WorldItemDatabase，请手动拖入。";
        }

        private void Run(bool commit)
        {
            if (database == null)
            {
                EditorUtility.DisplayDialog("ER 护甲登记", "请先指定目标 WorldItemDatabase。", "OK");
                return;
            }
            if (!AssetDatabase.IsValidFolder(sourceFolder))
            {
                EditorUtility.DisplayDialog("ER 护甲登记", $"源文件夹不存在：\n{sourceFolder}", "OK");
                return;
            }

            var report = new System.Text.StringBuilder();
            var so = new SerializedObject(database);
            if (commit)
                Undo.RegisterCompleteObjectUndo(database, "Register ER Armor");

            int totalAdded = 0, totalDup = 0, totalCleaned = 0;

            foreach (var (type, propName) in SlotMap)
            {
                SerializedProperty listProp = so.FindProperty(propName);
                if (listProp == null)
                {
                    report.AppendLine($"[警告] 数据库缺少字段 {propName}，跳过。");
                    continue;
                }

                //  当前已登记的引用集合（用于去重）
                var existing = new HashSet<UnityEngine.Object>();
                int cleaned = 0;
                for (int i = listProp.arraySize - 1; i >= 0; i--)
                {
                    var refObj = listProp.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (refObj == null)
                    {
                        if (removeMissingRefs && commit) { listProp.DeleteArrayElementAtIndex(i); cleaned++; }
                        continue;
                    }
                    existing.Add(refObj);
                }
                totalCleaned += cleaned;

                //  扫描源文件夹里该类型的所有 SO
                var found = LoadAssetsOfType(type, sourceFolder);
                if (sortByErRowId)
                    found = found.OrderBy(a => (a as ArmorItem)?.erRowId ?? int.MaxValue)
                                 .ThenBy(a => a.name, StringComparer.Ordinal).ToList();

                int added = 0, dup = 0;
                foreach (var asset in found)
                {
                    if (existing.Contains(asset)) { dup++; continue; }
                    if (commit)
                    {
                        int idx = listProp.arraySize;
                        listProp.InsertArrayElementAtIndex(idx);
                        listProp.GetArrayElementAtIndex(idx).objectReferenceValue = asset;
                    }
                    existing.Add(asset);
                    added++;
                }

                totalAdded += added;
                totalDup += dup;
                report.AppendLine($"{propName,-14} 找到 {found.Count,4}  新增 {added,4}  已存在 {dup,4}" +
                                  (cleaned > 0 ? $"  清理空引用 {cleaned}" : ""));
            }

            if (commit)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(database);
                if (database.gameObject.scene.IsValid())
                {
                    EditorSceneManager.MarkSceneDirty(database.gameObject.scene);
                    if (saveSceneAfter)
                        EditorSceneManager.SaveScene(database.gameObject.scene);
                }
            }

            report.AppendLine();
            report.AppendLine(commit
                ? $"已登记：新增 {totalAdded}，已存在 {totalDup}，清理空引用 {totalCleaned}。"
                  + (saveSceneAfter ? "（场景已保存）" : "（记得 Ctrl+S 保存场景）")
                : $"预览：将新增 {totalAdded}，已存在 {totalDup}。点“扫描并登记”写入。");

            lastReport = report.ToString();
            Debug.Log($"[ERArmorDatabaseRegistrar]\n{lastReport}");
        }

        private static List<UnityEngine.Object> LoadAssetsOfType(Type type, string folder)
        {
            //  用具体类型搜索，避免子类/父类匹配歧义
            string[] guids = AssetDatabase.FindAssets($"t:{type.Name}", new[] { folder });
            var list = new List<UnityEngine.Object>(guids.Length);
            var seen = new HashSet<string>();
            foreach (string g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                if (!seen.Add(path)) continue;
                var obj = AssetDatabase.LoadAssetAtPath(path, type);
                //  精确匹配类型，排除派生/同名干扰
                if (obj != null && obj.GetType() == type)
                    list.Add(obj);
            }
            return list;
        }
    }
}
