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
    /// 批量填充 WeaponItem 的 ER 挂点字段（DSAS 数据驱动挂载）。
    /// 数据链路：EquipParamWeapon.absorpParamId → WepAbsorpPosParam 行 → right_0/left_0/both_0/hang/isSkeletonBind。
    ///  - 按 WeaponItem.erRowId 匹配 EquipParamWeapon 行；
    ///  - 幂等：只写挂点相关字段(wepAbsorpPosId/isSkeletonBind/dummy*)，不碰其它手工字段；
    ///  - hang(收纳)值 ≥1000 表示挂点在“武器模型自身”上(DSAS 的 %1000 分支)，不在角色骨架，
    ///    这里按原值写入，运行时若在骨架里找不到该 refID 会自动回退，不会出错。
    /// </summary>
    public class ERWeaponMountImporter : EditorWindow
    {
        private const string DefaultWeaponCsv = "Assets/_ELDENRING_REF/Param/_named/EquipParamWeapon_named.csv";
        private const string DefaultPosCsv = "Assets/_ELDENRING_REF/Param/WepAbsorpPosParam.csv";

        private string weaponCsvPath = DefaultWeaponCsv;
        private string posCsvPath = DefaultPosCsv;
        private bool onlyFillEmpty = false;   // true=仅当 dummy 字段还是默认(-1)时才写，保护手工微调

        private Vector2 scroll;
        private string lastReport = "";

        [MenuItem("Tools/ER Import/Weapon Mount (WepAbsorpPos)...")]
        public static void Open()
        {
            var w = GetWindow<ERWeaponMountImporter>("ER 武器挂点导入");
            w.minSize = new Vector2(480, 340);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("批量填充武器挂点 (WepAbsorpPosParam → WeaponItem)", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            weaponCsvPath = EditorGUILayout.TextField(new GUIContent("武器表 CSV", "EquipParamWeapon_named.csv，提供 absorpParamId"), weaponCsvPath);
            posCsvPath = EditorGUILayout.TextField(new GUIContent("挂点表 CSV", "WepAbsorpPosParam.csv，提供 right_0/left_0/both_0/hang"), posCsvPath);
            onlyFillEmpty = EditorGUILayout.Toggle(new GUIContent("仅填空(保护手工微调)", "只在 dummy 字段仍为默认 -1 时写入"), onlyFillEmpty);

            EditorGUILayout.Space(6);
            GUI.backgroundColor = new Color(0.9f, 0.6f, 0.3f);
            if (GUILayout.Button("填充所有 ER 武器挂点", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("ER 武器挂点导入",
                        "按 erRowId 匹配工程内所有 WeaponItem，从 WepAbsorpPosParam 填挂点字段。\n幂等，不覆盖非挂点字段。继续？", "开始", "取消"))
                    Run();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "写入字段：wepAbsorpPosId、isSkeletonBind、dummyRightHand/LeftHand/BothHand/RightHang/LeftHang。\n" +
                "匹配依据：WeaponItem.erRowId == EquipParamWeapon.ID。未设 erRowId 的武器会被跳过。",
                MessageType.Info);

            if (!string.IsNullOrEmpty(lastReport))
            {
                EditorGUILayout.Space(4);
                scroll = EditorGUILayout.BeginScrollView(scroll);
                EditorGUILayout.TextArea(lastReport, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        private void Run()
        {
            string absWeapon = Path.GetFullPath(weaponCsvPath);
            string absPos = Path.GetFullPath(posCsvPath);
            if (!File.Exists(absWeapon)) { Fail($"找不到武器表 CSV：\n{weaponCsvPath}"); return; }
            if (!File.Exists(absPos)) { Fail($"找不到挂点表 CSV：\n{posCsvPath}"); return; }

            Dictionary<int, int> weaponToPos;          // 武器ID → absorpParamId(WepAbsorpPos 行)
            Dictionary<int, AbsorpPos> posById;        // WepAbsorpPos 行 → 挂点
            try
            {
                weaponToPos = LoadWeaponAbsorpPos(absWeapon);
                posById = LoadAbsorpPosTable(absPos);
            }
            catch (Exception e) { Fail($"解析 CSV 失败：\n{e.Message}"); return; }

            string[] guids = AssetDatabase.FindAssets("t:WeaponItem");
            int filled = 0, skippedNoRow = 0, skippedNoPos = 0, skippedProtected = 0;
            var log = new StringBuilder();

            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var item = AssetDatabase.LoadAssetAtPath<WeaponItem>(path);
                    if (item == null) continue;

                    if (item.erRowId <= 0)
                    {
                        skippedNoRow++;
                        continue;
                    }

                    if (!weaponToPos.TryGetValue(item.erRowId, out int posId) || posId < 0 || !posById.TryGetValue(posId, out AbsorpPos ap))
                    {
                        log.AppendLine($"[无挂点] {item.itemName}(row {item.erRowId})：absorpParamId 未指向有效 WepAbsorpPos 行");
                        skippedNoPos++;
                        continue;
                    }

                    if (onlyFillEmpty && HasMountData(item))
                    {
                        skippedProtected++;
                        continue;
                    }

                    item.wepAbsorpPosId = posId;
                    item.isSkeletonBind = ap.isSkeletonBind;
                    item.dummyRightHand = ap.right0;
                    item.dummyLeftHand = ap.left0;
                    item.dummyBothHand = ap.both0;
                    item.dummyRightHang = ap.rightHang0;
                    item.dummyLeftHang = ap.leftHang0;
                    EditorUtility.SetDirty(item);
                    filled++;

                    log.AppendLine($"[填充] {item.itemName}(row {item.erRowId}) pos={posId} " +
                                   $"R={ap.right0} L={ap.left0} 2H={ap.both0} " +
                                   $"hangR={ap.rightHang0} hangL={ap.leftHang0}" +
                                   (ap.isSkeletonBind ? " [skeletonBind]" : ""));
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            lastReport = $"填充 {filled}，跳过(无 erRowId) {skippedNoRow}，跳过(无挂点行) {skippedNoPos}，跳过(已有数据) {skippedProtected}。\n\n{log}";
            Debug.Log($"[ERWeaponMountImporter] 完成：填充 {filled}，跳过 {skippedNoRow + skippedNoPos + skippedProtected}。");
        }

        private static bool HasMountData(WeaponItem it) =>
            it.dummyRightHand >= 0 || it.dummyLeftHand >= 0 || it.dummyBothHand >= 0;

        private void Fail(string msg)
        {
            EditorUtility.DisplayDialog("ER 武器挂点导入", msg, "OK");
        }

        // ───────────────────────── CSV 解析 ─────────────────────────

        private struct AbsorpPos
        {
            public int right0, left0, both0, rightHang0, leftHang0;
            public bool isSkeletonBind;
        }

        /// <summary>武器表：ID → absorpParamId（指向 WepAbsorpPosParam 行）。</summary>
        private static Dictionary<int, int> LoadWeaponAbsorpPos(string absPath)
        {
            var (rows, col) = LoadCsv(absPath);
            if (!col.ContainsKey("ID")) throw new Exception("武器表缺列：ID");
            if (!col.ContainsKey("absorpParamId")) throw new Exception("武器表缺列：absorpParamId（指向 WepAbsorpPosParam）");
            int idCol = col["ID"], posCol = col["absorpParamId"];

            var map = new Dictionary<int, int>();
            foreach (var f in rows)
            {
                if (idCol >= f.Length || posCol >= f.Length) continue;
                if (!int.TryParse(f[idCol], out int id)) continue;
                if (!int.TryParse(f[posCol], out int pos)) pos = -1;
                map[id] = pos;
            }
            return map;
        }

        /// <summary>挂点表：行ID → 各姿态 dummy refID + isSkeletonBind。</summary>
        private static Dictionary<int, AbsorpPos> LoadAbsorpPosTable(string absPath)
        {
            var (rows, col) = LoadCsv(absPath);
            foreach (string need in new[] { "ID", "right_0", "left_0", "both_0", "rightHang_0", "leftHang_0", "isSkeletonBind" })
                if (!col.ContainsKey(need)) throw new Exception($"挂点表缺列：{need}");

            int idc = col["ID"], r0 = col["right_0"], l0 = col["left_0"], b0 = col["both_0"];
            int rh = col["rightHang_0"], lh = col["leftHang_0"], sb = col["isSkeletonBind"];

            var map = new Dictionary<int, AbsorpPos>();
            foreach (var f in rows)
            {
                if (idc >= f.Length) continue;
                if (!int.TryParse(f[idc], out int id)) continue;
                map[id] = new AbsorpPos
                {
                    right0 = ParseInt(f, r0, -1),
                    left0 = ParseInt(f, l0, -1),
                    both0 = ParseInt(f, b0, -1),
                    rightHang0 = ParseInt(f, rh, -1),
                    leftHang0 = ParseInt(f, lh, -1),
                    isSkeletonBind = ParseInt(f, sb, 0) != 0,
                };
            }
            return map;
        }

        private static int ParseInt(string[] f, int i, int def) =>
            (i >= 0 && i < f.Length && int.TryParse(f[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out int v)) ? v : def;

        /// <returns>(去表头后的行列表, 列名→索引)。兼容任意换行符。</returns>
        private static (List<string[]>, Dictionary<string, int>) LoadCsv(string absPath)
        {
            string text = File.ReadAllText(absPath, Encoding.UTF8);
            string[] rawLines = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            if (rawLines.Length < 2) throw new Exception("CSV 内容为空");

            string[] header = SplitCsv(rawLines[0]);
            var col = new Dictionary<string, int>();
            for (int i = 0; i < header.Length; i++)
                col[header[i].Trim()] = i;

            var rows = new List<string[]>();
            for (int i = 1; i < rawLines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(rawLines[i])) continue;
                rows.Add(SplitCsv(rawLines[i]));
            }
            return (rows, col);
        }

        //  逗号分隔，支持双引号包裹（"" 转义）。ER 的 [a|b|c] 用 | 分隔不含逗号，安全。
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
    }
}
