using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LZ.EditorTools
{
    /// <summary>
    /// 从 EquipParamProtector_named.csv 批量生成 / 更新护甲 ScriptableObject（ArmorItem 子类）。
    ///
    /// 数值按 ER 面板显示值精确还原（已用工程内 Knight Helm 校准）：
    ///   itemWeight               = weight
    ///   physicalDamageAbsorption = (1 - neutralDamageCutRate) * 100
    ///   magicDamageAbsorption    = (1 - magicDamageCutRate)   * 100
    ///   fireDamageAbsorption     = (1 - fireDamageCutRate)    * 100
    ///   lightningDamageAbsorption= (1 - thunderDamageCutRate) * 100
    ///   holyDamageAbsorption     = (1 - darkDamageCutRate)    * 100   (ER 的 holy 复用 dark 字段)
    ///   immunity                 = resistPoison   (== resistDisease)
    ///   robustness               = resistBlood    (== resistFreeze)
    ///   focus                    = resistSleep    (== resistMadness)
    ///   vitality                 = resistCurse
    ///   poise                    = round(toughnessCorrectRate * 1000)
    ///   erRowId                  = ID
    ///   modularPartCode          = equipModelId
    ///
    /// 槽位由 protectorCategory 决定：0=头(Head) 1=身(Torso) 2=臂(Arms) 3=腿(Legs)。
    /// 生成幂等：同路径已存在则就地更新字段，不重建资产（不破坏已有引用）。
    /// </summary>
    public class EREquipmentImporter : EditorWindow
    {
        private const string DefaultCsv = "Assets/_ELDENRING_REF/Param/_named/EquipParamProtector_named.csv";
        private const string DefaultOut = "Assets/Data/Items/Armor/ER";
        private const string PartsRoot = "Assets/_ELDENRING_REF/Parts";

        private string csvPath = DefaultCsv;
        private string outputRoot = DefaultOut;
        private string gender = "M";
        private string rowIdsText = "980000, 980100, 980200, 980300";
        private bool requirePartExists = true;
        private bool defaultHelmetIsFull = true;
        //  按套装分文件夹（套装 = equipModelId，同套 4 件共享同一编号）
        private bool groupBySet = true;

        //  本次导入内的占用路径 -> erRowId，用于重名防覆盖
        private readonly Dictionary<string, int> usedPaths = new();
        //  equipModelId -> 套装文件夹名（保证同套所有部件落到同一文件夹）
        private readonly Dictionary<string, string> setFolderByModel = new();
        //  套装文件夹名 -> 首个占用它的 equipModelId（不同套同名时追加编号区分）
        private readonly Dictionary<string, string> folderOwnerModel = new();

        [MenuItem("Tools/ER Import/Armor From Param...")]
        public static void Open()
        {
            var w = GetWindow<EREquipmentImporter>("ER 护甲导入");
            w.minSize = new Vector2(460, 320);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("EquipParamProtector → ArmorItem SO", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            csvPath = EditorGUILayout.TextField("CSV 路径", csvPath);
            outputRoot = EditorGUILayout.TextField("输出目录", outputRoot);
            gender = EditorGUILayout.TextField("模型性别 (M/F)", gender);
            requirePartExists = EditorGUILayout.Toggle(new GUIContent("要求部件存在", "找不到对应 fbx 时跳过该件"), requirePartExists);
            defaultHelmetIsFull = EditorGUILayout.Toggle(new GUIContent("头盔默认全覆盖", "true=FullHelmet(隐藏头发/头), false=Hat(不隐藏)"), defaultHelmetIsFull);
            groupBySet = EditorGUILayout.Toggle(new GUIContent("按套装分文件夹", "同一套装(equipModelId)的部件归到同一子文件夹，文件夹名取自物品名去掉槽位后缀"), groupBySet);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("行 ID（逗号/空格/换行分隔）");
            rowIdsText = EditorGUILayout.TextArea(rowIdsText, GUILayout.Height(60));

            EditorGUILayout.Space(6);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("卡利亚骑士套（980000-980300）"))
                    rowIdsText = "980000, 980100, 980200, 980300";
                if (GUILayout.Button("清空"))
                    rowIdsText = "";
            }

            EditorGUILayout.Space(6);
            if (GUILayout.Button("导入所列行", GUILayout.Height(30)))
                ImportRows(ParseRowIds(rowIdsText));

            EditorGUILayout.Space(2);
            GUI.backgroundColor = new Color(0.9f, 0.6f, 0.3f);
            if (GUILayout.Button("全量导入（CSV 全部护甲）", GUILayout.Height(30)))
                ImportAll();
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "数值按 ER 面板值精确还原；erRowId 存 ER 行号；modularPartCode 取 equipModelId。\n" +
                "生成后请在 EquipmentPartCatalog 上执行 Auto Populate，并把 SO 挂到 WorldItemDatabase。",
                MessageType.Info);
        }

        // ───────────────────────── 导入主流程 ─────────────────────────

        private void ImportRows(List<int> rowIds)
        {
            if (rowIds.Count == 0)
            {
                EditorUtility.DisplayDialog("ER 护甲导入", "没有可导入的行 ID。", "OK");
                return;
            }
            RunImport(rowIds);
        }

        //  全量：读取 CSV 里全部（有名称的）护甲行
        private void ImportAll()
        {
            if (!EditorUtility.DisplayDialog("ER 护甲全量导入",
                    "将读取 CSV 全部护甲行并生成/更新 SO（幂等，可重复执行）。\n继续？", "开始", "取消"))
                return;
            RunImport(null);
        }

        /// <param name="rowIds">null=导入 CSV 全部行；否则仅导入所列行。</param>
        private void RunImport(List<int> rowIds)
        {
            string absCsv = Path.GetFullPath(csvPath);
            if (!File.Exists(absCsv))
            {
                EditorUtility.DisplayDialog("ER 护甲导入", $"找不到 CSV：\n{csvPath}", "OK");
                return;
            }

            Dictionary<int, string[]> rowsById;
            Dictionary<string, int> col;
            try
            {
                (rowsById, col) = LoadCsv(absCsv);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("ER 护甲导入", $"解析 CSV 失败：\n{e.Message}", "OK");
                return;
            }

            //  rowIds 为 null 时导入全部（按行号升序，输出稳定）
            IEnumerable<int> ids = rowIds;
            if (ids == null)
            {
                var all = new List<int>(rowsById.Keys);
                all.Sort();
                ids = all;
            }

            EnsureFolder(outputRoot);
            usedPaths.Clear();
            setFolderByModel.Clear();
            folderOwnerModel.Clear();
            ensuredFolders.Clear();

            int created = 0, updated = 0, skipped = 0;
            var log = new StringBuilder();

            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (int id in ids)
                {
                    if (!rowsById.TryGetValue(id, out string[] f))
                    {
                        log.AppendLine($"[跳过] {id}：CSV 中无此行");
                        skipped++;
                        continue;
                    }

                    var r = new Row(f, col);
                    if (string.IsNullOrEmpty(r.Name))
                    {
                        log.AppendLine($"[跳过] {id}：无名称（占位行）");
                        skipped++;
                        continue;
                    }

                    string prefix = SlotPrefix(r.ProtectorCategory);
                    if (prefix == null)
                    {
                        log.AppendLine($"[跳过] {id} {r.Name}：未知 protectorCategory={r.ProtectorCategory}");
                        skipped++;
                        continue;
                    }

                    string partCode = $"{prefix}_{gender}_{r.EquipModelId}";
                    if (requirePartExists && FindPartAsset(prefix, partCode) == null)
                    {
                        log.AppendLine($"[跳过] {id} {r.Name}：找不到部件 {partCode}.fbx");
                        skipped++;
                        continue;
                    }

                    bool didCreate = UpsertItem(r, prefix, out string path);
                    if (didCreate) created++; else updated++;
                    log.AppendLine($"[{(didCreate ? "新建" : "更新")}] {id} {r.Name} → {path}  (code={r.EquipModelId}, poise={r.Poise})");
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log($"[EREquipmentImporter] 完成：新建 {created}，更新 {updated}，跳过 {skipped}。\n{log}");
            EditorUtility.DisplayDialog("ER 护甲导入", $"新建 {created}，更新 {updated}，跳过 {skipped}。\n详见 Console。", "OK");
        }

        /// <returns>true=新建，false=更新已存在资产。</returns>
        private bool UpsertItem(Row r, string prefix, out string assetPath)
        {
            Type type = SlotType(r.ProtectorCategory);

            string folder = outputRoot;
            if (groupBySet)
            {
                folder = $"{outputRoot}/{ResolveSetFolder(r)}";
                EnsureFolder(folder);
            }

            //  文件名 = 名称；若与本次已生成的另一行重名，追加行号避免互相覆盖
            string baseName = SanitizeFileName(r.Name);
            assetPath = $"{folder}/{baseName}.asset";
            if (usedPaths.TryGetValue(assetPath, out int owner) && owner != r.Id)
                assetPath = $"{folder}/{baseName}_{r.Id}.asset";
            usedPaths[assetPath] = r.Id;

            var existing = AssetDatabase.LoadAssetAtPath<ArmorItem>(assetPath);
            bool created = false;
            ArmorItem item;
            if (existing != null && existing.GetType() == type)
            {
                item = existing;
            }
            else
            {
                item = (ArmorItem)ScriptableObject.CreateInstance(type);
                created = true;
            }

            // Item 基类
            item.itemName = r.Name;
            item.itemDescription = r.Description;
            item.maxItemAmount = 1;
            item.currentItemAmount = 1;

            // ArmorItem 数值（ER 面板值）
            item.itemWeight = r.Weight;
            item.physicalDamageAbsorption = r.PhysAbsorb;
            item.magicDamageAbsorption = r.MagicAbsorb;
            item.fireDamageAbsorption = r.FireAbsorb;
            item.lightningDamageAbsorption = r.LightningAbsorb;
            item.holyDamageAbsorption = r.HolyAbsorb;
            item.immunity = r.Immunity;
            item.robustness = r.Robustness;
            item.focus = r.Focus;
            item.vitality = r.Vitality;
            item.poise = r.Poise;

            // ER 溯源 + 模块化键
            item.erRowId = r.Id;
            item.modularPartCode = r.EquipModelId;

            // 头盔类型（无对应 param 列，用默认策略）
            if (item is HeadEquipmentItem head)
                head.headEquipmentType = defaultHelmetIsFull ? HeadEquipmentType.FullHelmet : HeadEquipmentType.Hat;

            if (created)
                AssetDatabase.CreateAsset(item, assetPath);
            else
                EditorUtility.SetDirty(item);

            return created;
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

            foreach (string need in new[] { "ID", "Name", "weight", "toughnessCorrectRate",
                "neutralDamageCutRate", "magicDamageCutRate", "fireDamageCutRate", "thunderDamageCutRate", "darkDamageCutRate",
                "resistPoison", "resistBlood", "resistSleep", "resistCurse", "protectorCategory", "equipModelId" })
            {
                if (!col.ContainsKey(need)) throw new Exception($"CSV 缺少列：{need}");
            }

            int idCol = col["ID"];
            var byId = new Dictionary<int, string[]>();
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                string[] f = SplitCsv(lines[i]);
                if (idCol >= f.Length) continue;
                if (!int.TryParse(f[idCol], NumberStyles.Integer, CultureInfo.InvariantCulture, out int id)) continue;
                byId[id] = f;
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

        // ───────────────────────── 辅助 ─────────────────────────

        private sealed class Row
        {
            public readonly int Id;
            public readonly string Name;
            public readonly string Description;
            public readonly float Weight;
            public readonly int ProtectorCategory;
            public readonly string EquipModelId;
            public readonly float PhysAbsorb, MagicAbsorb, FireAbsorb, LightningAbsorb, HolyAbsorb;
            public readonly float Immunity, Robustness, Focus, Vitality, Poise;

            public Row(string[] f, Dictionary<string, int> col)
            {
                float Cut(string name) => 1f - GetF(f, col, name);
                float Abs(string name) => Mathf.Round(Cut(name) * 100f * 10f) / 10f;

                Id = (int)GetF(f, col, "ID");
                Name = Get(f, col, "Name").Trim();
                //  Caption 为可选列；导出时换行编码为字面 \n，这里还原
                Description = GetOpt(f, col, "Caption").Replace("\\n", "\n");
                Weight = GetF(f, col, "weight");
                ProtectorCategory = (int)GetF(f, col, "protectorCategory");
                EquipModelId = Get(f, col, "equipModelId").Trim();

                PhysAbsorb = Abs("neutralDamageCutRate");
                MagicAbsorb = Abs("magicDamageCutRate");
                FireAbsorb = Abs("fireDamageCutRate");
                LightningAbsorb = Abs("thunderDamageCutRate");
                HolyAbsorb = Abs("darkDamageCutRate");

                Immunity = GetF(f, col, "resistPoison");
                Robustness = GetF(f, col, "resistBlood");
                Focus = GetF(f, col, "resistSleep");
                Vitality = GetF(f, col, "resistCurse");
                Poise = Mathf.Round(GetF(f, col, "toughnessCorrectRate") * 1000f);
            }
        }

        private static string Get(string[] f, Dictionary<string, int> col, string name)
        {
            int i = col[name];
            return i < f.Length ? f[i] : "";
        }

        //  可选列：列不存在时返回空串
        private static string GetOpt(string[] f, Dictionary<string, int> col, string name)
        {
            return col.TryGetValue(name, out int i) && i < f.Length ? f[i] : "";
        }

        private static float GetF(string[] f, Dictionary<string, int> col, string name)
        {
            string s = Get(f, col, name);
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : 0f;
        }

        private static string SlotPrefix(int protectorCategory) => protectorCategory switch
        {
            0 => "HD",
            1 => "BD",
            2 => "AM",
            3 => "LG",
            _ => null
        };

        //  为一行解析其套装文件夹名：同 equipModelId 复用同一文件夹；
        //  不同套装若去后缀后重名，则追加 equipModelId 区分。
        private string ResolveSetFolder(Row r)
        {
            if (setFolderByModel.TryGetValue(r.EquipModelId, out string cached))
                return cached;

            string name = SanitizeFileName(DeriveSetName(r.Name));
            if (string.IsNullOrEmpty(name)) name = "Misc";

            if (folderOwnerModel.TryGetValue(name, out string owner) && owner != r.EquipModelId)
                name = $"{name}_{r.EquipModelId}";

            folderOwnerModel[name] = r.EquipModelId;
            setFolderByModel[r.EquipModelId] = name;
            return name;
        }

        //  去掉槽位后缀得到套装名："卡利亚骑士头盔"→"卡利亚骑士"；
        //  保留尾部括注（如「（轻装）」）："卡利亚骑士铠甲（轻装）"→"卡利亚骑士（轻装）"。
        private static string DeriveSetName(string itemName)
        {
            string name = itemName.Trim();

            //  分离尾部括注
            string paren = "";
            var m = System.Text.RegularExpressions.Regex.Match(name, @"([（(][^（）()]*[）)])\s*$");
            if (m.Success)
            {
                paren = m.Groups[1].Value;
                name = name.Substring(0, m.Index).TrimEnd();
            }

            //  去掉一个槽位后缀（长的优先，避免「盔」先于「头盔」命中）
            foreach (string suf in SlotSuffixes)
            {
                if (name.Length > suf.Length && name.EndsWith(suf, StringComparison.Ordinal))
                {
                    name = name.Substring(0, name.Length - suf.Length);
                    break;
                }
            }

            return (name + paren).Trim();
        }

        //  常见护甲槽位后缀（务必“长在前”，命中即止）
        private static readonly string[] SlotSuffixes =
        {
            "头盔", "兜帽", "头巾", "头饰", "面甲", "面具", "王冠", "假发", "头冠",
            "胴甲", "铠甲", "大衣", "长袍", "罩袍", "战衣", "战袍", "外套", "披风", "上衣", "法衣",
            "臂甲", "手甲", "护手", "护腕", "腕甲", "手套", "袖套", "手环",
            "腿甲", "胫甲", "护腿", "铁靴", "长靴", "靴子", "裤子", "绑腿",
            "冠", "盔", "帽", "铠", "甲", "服", "装", "衣", "袍", "袖", "靴", "裤",
        };

        private static Type SlotType(int protectorCategory) => protectorCategory switch
        {
            0 => typeof(HeadEquipmentItem),
            1 => typeof(BodyEquipmentItem),
            2 => typeof(HandEquipmentItem),
            3 => typeof(LegEquipmentItem),
            _ => typeof(ArmorItem)
        };

        private static GameObject FindPartAsset(string prefix, string code)
        {
            string path = $"{PartsRoot}/{prefix}/{code}.fbx";
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
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

        //  本次导入内已确保存在的文件夹。批处理(StartAssetEditing)期间
        //  AssetDatabase.IsValidFolder 看不到同批刚建的文件夹，若仅靠它判断会重复
        //  CreateFolder，被 Unity 自动改名成 "xxx 1/2/3"。用此集合保证每个文件夹只建一次。
        private readonly HashSet<string> ensuredFolders = new();

        private void EnsureFolder(string assetFolder)
        {
            if (ensuredFolders.Contains(assetFolder)) return;
            if (AssetDatabase.IsValidFolder(assetFolder)) { ensuredFolders.Add(assetFolder); return; }

            string[] parts = assetFolder.Split('/');
            string cur = parts[0]; // "Assets"
            ensuredFolders.Add(cur);
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{cur}/{parts[i]}";
                if (!ensuredFolders.Contains(next) && !AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                ensuredFolders.Add(next);
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
