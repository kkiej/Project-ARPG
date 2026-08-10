using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LZ.EditorTools
{
    /// <summary>
    /// 从 EquipParamWeapon_named.csv 导入武器数据到 WeaponItem SO。
    /// 只填 ER 可确定的“数据”字段（名称/说明/重量/需求/基础伤害/格挡吸收/稳定度/削韧），
    /// 并按命名约定挂接武器模型；绝不覆盖手工创作字段（moveset/动作/动画/连招倍率/weaponClass）。
    /// 幂等：重复运行只更新数据字段，保留你手工配置。
    /// </summary>
    public class ERWeaponImporter : EditorWindow
    {
        private const string DefaultCsv = "Assets/_ELDENRING_REF/Param/_named/EquipParamWeapon_named.csv";
        private const string DefaultOut = "Assets/Data/Items/Weapons/ER";
        private const string ModelRoot = "Assets/Prefabs/Items/Weapons/ER";

        private string csvPath = DefaultCsv;
        private string outputRoot = DefaultOut;
        private string modelPrefix = "WP_A_";     // ER 武器模型命名前缀，最终名为 前缀 + 4位modelId
        private string rowIdsText = "";
        private bool requireModelExists = true;    // 找不到对应模型 prefab 时跳过
        private bool assignModelIfEmpty = true;    // 仅在 weaponModel 为空时挂接，避免覆盖手工微调
        private bool setPoiseFromParam = true;     // 用 saWeaponDamage 填 poiseDamage

        private readonly Dictionary<string, int> usedPaths = new();
        private Vector2 scroll;
        private string lastReport = "";

        [MenuItem("Tools/ER Import/Weapon From Param...")]
        public static void Open()
        {
            var w = GetWindow<ERWeaponImporter>("ER 武器导入");
            w.minSize = new Vector2(480, 380);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("从 EquipParamWeapon 导入武器数据 → WeaponItem", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            csvPath = EditorGUILayout.TextField("CSV 路径", csvPath);
            outputRoot = EditorGUILayout.TextField("输出目录", outputRoot);
            modelPrefix = EditorGUILayout.TextField(new GUIContent("模型前缀", $"模型 prefab 命名：{modelPrefix}<4位modelId>，位于 {ModelRoot}"), modelPrefix);
            requireModelExists = EditorGUILayout.Toggle(new GUIContent("要求模型存在", "找不到对应武器模型 prefab 时跳过该件"), requireModelExists);
            assignModelIfEmpty = EditorGUILayout.Toggle(new GUIContent("仅空槽挂接模型", "只在 weaponModel 为空时赋值，保留已手工设置的模型/偏移"), assignModelIfEmpty);
            setPoiseFromParam = EditorGUILayout.Toggle(new GUIContent("用 saWeaponDamage 填削韧", "以 ER 招架/削韧值填 poiseDamage（可能需要按你的数值体系再调）"), setPoiseFromParam);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("行 ID（逗号/空格/换行分隔）");
            rowIdsText = EditorGUILayout.TextArea(rowIdsText, GUILayout.Height(48));

            EditorGUILayout.Space(6);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("导入所列行", GUILayout.Height(28)))
                    RunImport(ParseRowIds(rowIdsText));
                GUI.backgroundColor = new Color(0.9f, 0.6f, 0.3f);
                if (GUILayout.Button("全量导入（CSV 全部武器）", GUILayout.Height(28)))
                {
                    if (EditorUtility.DisplayDialog("ER 武器全量导入",
                            "读取 CSV 全部武器行并生成/更新 SO（幂等，不覆盖手工字段）。\n继续？", "开始", "取消"))
                        RunImport(null);
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "只填 ER 数据字段与模型引用；weaponClass / moveset / 动作 / 动画 / 攻击倍率等手工字段保持不变。\n" +
                "itemID 由 WorldItemDatabase 运行时分配。导入后记得登记进数据库并手配 moveset。",
                MessageType.Info);

            if (!string.IsNullOrEmpty(lastReport))
            {
                EditorGUILayout.Space(4);
                scroll = EditorGUILayout.BeginScrollView(scroll);
                EditorGUILayout.TextArea(lastReport, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        // ───────────────────────── 导入主流程 ─────────────────────────

        /// <param name="rowIds">null=导入 CSV 全部行；否则仅导入所列行。</param>
        private void RunImport(List<int> rowIds)
        {
            string absCsv = Path.GetFullPath(csvPath);
            if (!File.Exists(absCsv))
            {
                EditorUtility.DisplayDialog("ER 武器导入", $"找不到 CSV：\n{csvPath}", "OK");
                return;
            }

            Dictionary<int, string[]> rowsById;
            Dictionary<string, int> col;
            try { (rowsById, col) = LoadCsv(absCsv); }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("ER 武器导入", $"解析 CSV 失败：\n{e.Message}", "OK");
                return;
            }

            IEnumerable<int> ids = rowIds;
            if (ids == null)
            {
                var all = new List<int>(rowsById.Keys);
                all.Sort();
                ids = all;
            }

            EnsureFolder(outputRoot);
            usedPaths.Clear();

            int created = 0, updated = 0, skipped = 0;
            var log = new StringBuilder();

            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (int id in ids)
                {
                    if (!rowsById.TryGetValue(id, out string[] f))
                    {
                        log.AppendLine($"[跳过] {id}：CSV 中无此行"); skipped++; continue;
                    }

                    var r = new Row(f, col);
                    if (string.IsNullOrEmpty(r.Name))
                    {
                        log.AppendLine($"[跳过] {id}：无名称（占位行）"); skipped++; continue;
                    }

                    GameObject model = FindModel(r.EquipModelId);
                    if (requireModelExists && model == null)
                    {
                        log.AppendLine($"[跳过] {id} {r.Name}：找不到模型 {modelPrefix}{r.EquipModelId:D4}.prefab"); skipped++; continue;
                    }

                    bool didCreate = UpsertItem(r, model, out string path, out bool modelSet);
                    if (didCreate) created++; else updated++;
                    log.AppendLine($"[{(didCreate ? "新建" : "更新")}] {id} {r.Name} → {path}" +
                                   $"  (modelId={r.EquipModelId}, phys={r.PhysDamage}{(modelSet ? ", 已挂模型" : "")})");
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            lastReport = $"新建 {created}，更新 {updated}，跳过 {skipped}。\n{log}";
            Debug.Log($"[ERWeaponImporter] 完成：{lastReport}");
        }

        /// <returns>true=新建，false=更新。</returns>
        private bool UpsertItem(Row r, GameObject model, out string assetPath, out bool modelSet)
        {
            modelSet = false;
            string baseName = SanitizeFileName(r.Name);
            assetPath = $"{outputRoot}/{baseName}.asset";
            if (usedPaths.TryGetValue(assetPath, out int owner) && owner != r.Id)
                assetPath = $"{outputRoot}/{baseName}_{r.Id}.asset";
            usedPaths[assetPath] = r.Id;

            var existing = AssetDatabase.LoadAssetAtPath<WeaponItem>(assetPath);
            bool created = existing == null;
            WeaponItem item = existing != null ? existing : ScriptableObject.CreateInstance<WeaponItem>();

            // 基础信息（Item 基类）
            item.itemName = r.Name;
            item.itemDescription = r.Description;
            item.maxItemAmount = 1;
            item.currentItemAmount = 1;

            // ER 数值字段
            item.itemWeight = r.Weight;
            item.strengthREQ = r.StrReq;
            item.dexREQ = r.DexReq;
            item.intREQ = r.IntReq;
            item.faithREQ = r.FaiReq;

            item.physicalDamage = r.PhysDamage;
            item.magicDamage = r.MagicDamage;
            item.fireDamage = r.FireDamage;
            item.lightningDamage = r.LightningDamage;
            item.holyDamage = r.HolyDamage;

            if (setPoiseFromParam)
                item.poiseDamage = r.SaWeaponDamage;

            item.physicalBaseDamageAbsorption = r.PhysGuard;
            item.magicBaseDamageAbsorption = r.MagicGuard;
            item.fireBaseDamageAbsorption = r.FireGuard;
            item.lightningBaseDamageAbsorption = r.LightningGuard;
            item.holyBaseDamageAbsorption = r.HolyGuard;
            item.stability = r.StaminaGuardDef;

            item.erRowId = r.Id;

            // 模型挂接（不覆盖手工设置）
            if (model != null && (!assignModelIfEmpty || item.weaponModel == null))
            {
                item.weaponModel = model;
                modelSet = true;
            }

            //  weaponClass / weaponModelType / moveset / weaponAnimationSet / 各种 Action /
            //  攻击与耐力倍率 / SFX 均为手工字段，导入器不触碰。

            if (created)
                AssetDatabase.CreateAsset(item, assetPath);
            else
                EditorUtility.SetDirty(item);

            return created;
        }

        private GameObject FindModel(int modelId)
        {
            string path = $"{ModelRoot}/{modelPrefix}{modelId:D4}.prefab";
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        // ───────────────────────── CSV 解析 ─────────────────────────

        private static (Dictionary<int, string[]>, Dictionary<string, int>) LoadCsv(string absPath)
        {
            string[] lines = File.ReadAllLines(absPath, Encoding.UTF8);
            if (lines.Length < 2) throw new Exception("CSV 内容为空");

            string[] header = SplitCsv(lines[0]);
            var col = new Dictionary<string, int>();
            for (int i = 0; i < header.Length; i++)
                col[header[i].Trim()] = i;

            foreach (string need in new[]
            {
                "ID", "Name", "weight",
                "properStrength", "properAgility", "properMagic", "properFaith",
                "attackBasePhysics", "attackBaseMagic", "attackBaseFire", "attackBaseThunder", "attackBaseDark",
                "saWeaponDamage",
                "physGuardCutRate", "magGuardCutRate", "fireGuardCutRate", "thunGuardCutRate", "darkGuardCutRate",
                "staminaGuardDef", "equipModelId"
            })
            {
                if (!col.ContainsKey(need)) throw new Exception($"CSV 缺少列：{need}");
            }

            int idCol = col["ID"];
            var byId = new Dictionary<int, string[]>();
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                string[] ff = SplitCsv(lines[i]);
                if (idCol >= ff.Length) continue;
                if (!int.TryParse(ff[idCol], NumberStyles.Integer, CultureInfo.InvariantCulture, out int id)) continue;
                byId[id] = ff;
            }
            return (byId, col);
        }

        //  逗号分隔，支持双引号包裹（"" 转义）。ER 的 [a|b|c] 用 | 分隔，不含逗号，安全。
        private static string[] SplitCsv(string line)
        {
            var res = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                        else inQuotes = false;
                    }
                    else sb.Append(c);
                }
                else
                {
                    if (c == '"') inQuotes = true;
                    else if (c == ',') { res.Add(sb.ToString()); sb.Clear(); }
                    else sb.Append(c);
                }
            }
            res.Add(sb.ToString());
            return res.ToArray();
        }

        private sealed class Row
        {
            public readonly int Id;
            public readonly string Name;
            public readonly string Description;
            public readonly float Weight;
            public readonly int EquipModelId;
            public readonly int StrReq, DexReq, IntReq, FaiReq;
            public readonly int PhysDamage, MagicDamage, FireDamage, LightningDamage, HolyDamage;
            public readonly float SaWeaponDamage;
            public readonly float PhysGuard, MagicGuard, FireGuard, LightningGuard, HolyGuard, StaminaGuardDef;

            public Row(string[] f, Dictionary<string, int> col)
            {
                Id = (int)GetF(f, col, "ID");
                Name = Get(f, col, "Name").Trim();
                Description = GetOpt(f, col, "Caption").Replace("\\n", "\n");
                Weight = GetF(f, col, "weight");
                EquipModelId = (int)GetF(f, col, "equipModelId");

                StrReq = (int)GetF(f, col, "properStrength");
                DexReq = (int)GetF(f, col, "properAgility");
                IntReq = (int)GetF(f, col, "properMagic");
                FaiReq = (int)GetF(f, col, "properFaith");

                PhysDamage = (int)GetF(f, col, "attackBasePhysics");
                MagicDamage = (int)GetF(f, col, "attackBaseMagic");
                FireDamage = (int)GetF(f, col, "attackBaseFire");
                LightningDamage = (int)GetF(f, col, "attackBaseThunder");
                HolyDamage = (int)GetF(f, col, "attackBaseDark"); // ER 内部 Dark = 神圣

                SaWeaponDamage = GetF(f, col, "saWeaponDamage");

                //  武器格挡吸收为 0-100 百分值，直接映射
                PhysGuard = GetF(f, col, "physGuardCutRate");
                MagicGuard = GetF(f, col, "magGuardCutRate");
                FireGuard = GetF(f, col, "fireGuardCutRate");
                LightningGuard = GetF(f, col, "thunGuardCutRate");
                HolyGuard = GetF(f, col, "darkGuardCutRate");
                StaminaGuardDef = GetF(f, col, "staminaGuardDef");
            }
        }

        private static string Get(string[] f, Dictionary<string, int> col, string name)
        {
            int i = col[name];
            return i < f.Length ? f[i] : "";
        }

        private static string GetOpt(string[] f, Dictionary<string, int> col, string name)
        {
            return col.TryGetValue(name, out int i) && i < f.Length ? f[i] : "";
        }

        private static float GetF(string[] f, Dictionary<string, int> col, string name)
        {
            string s = Get(f, col, name);
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : 0f;
        }

        private static List<int> ParseRowIds(string text)
        {
            var res = new List<int>();
            if (string.IsNullOrWhiteSpace(text)) return res;
            foreach (string tok in text.Split(new[] { ',', ' ', '\t', '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries))
                if (int.TryParse(tok.Trim(), out int v) && !res.Contains(v))
                    res.Add(v);
            return res;
        }

        private static void EnsureFolder(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder)) return;
            string[] parts = assetFolder.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }

        private static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
