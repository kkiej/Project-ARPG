using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LZ.Editor
{
    /// <summary>
    /// 从 ER 参数表解析每件武器的命中框数据（<see cref="WeaponHitboxData"/> SO）。
    /// <para/>
    /// 数据链（已由 CSV 证实）：
    /// <list type="number">
    /// <item>EquipParamWeapon：equipModelId → behaviorVariationId（如模型 200 → varId 200）。</item>
    /// <item>BehaviorParam_PC：variationId 相同、refType=0 的一批行，其 refId = 一批 AtkParam ID（如 200000/200010/…）。</item>
    /// <item>AtkParam_Pc：每个 AtkParam 的 hit0..hit15 = (DmyPoly1, DmyPoly2, Radius)，即一段段命中胶囊
    ///       （DmyPoly2 = -1 表示单点球）。</item>
    /// </list>
    /// 生成的 SO 同时保存「逐攻击命中段」(perAttack) 与「并集刀刃段」(unionSegments，同 dummy 对取最大半径)。
    /// 之后用 <c>WeaponColliderAutoFitter</c> 在武器 FBX 的 dummy 点之间按 union 段生成子胶囊。
    /// <para/>
    /// 菜单：Tools → Weapon → Build Hitbox Data (from AtkParam)
    /// </summary>
    public class WeaponHitboxDataBuilder : EditorWindow
    {
        private const int HitCount = 16;

        [SerializeField] private string _equipParamPath = "Assets/_ELDENRING_REF/EquipParamWeapon.csv";
        [SerializeField] private string _behaviorParamPath = "Assets/_ELDENRING_REF/BehaviorParam_PC.csv";
        [SerializeField] private string _atkParamPath = "Assets/_ELDENRING_REF/AtkParam_Pc.csv";
        [SerializeField] private string _outputFolder = "Assets/Data/WeaponHitboxes";
        [SerializeField] private string _modelIds = "200";

        private string _report;
        private Vector2 _scroll;

        [MenuItem("Tools/Weapon/Build Hitbox Data (from AtkParam)")]
        public static void ShowWindow() => GetWindow<WeaponHitboxDataBuilder>("Weapon Hitbox Data");

        private void OnGUI()
        {
            GUILayout.Label("武器命中框数据解析器", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "EquipParamWeapon(behaviorVariationId) → BehaviorParam_PC(refId=AtkParam) → AtkParam_Pc(hitN dummy对+半径)。\n" +
                "为每个模型号生成一个 WeaponHitboxData.asset（含逐攻击段 + 并集刀刃段）。\n" +
                "生成后用 Tools/Weapon/Auto-Fit Damage Colliders 的 HitboxData 模式在武器上生成多段胶囊。",
                MessageType.Info);

            GUILayout.Space(6);
            _equipParamPath = EditorGUILayout.TextField("EquipParamWeapon.csv", _equipParamPath);
            _behaviorParamPath = EditorGUILayout.TextField("BehaviorParam_PC.csv", _behaviorParamPath);
            _atkParamPath = EditorGUILayout.TextField("AtkParam_Pc.csv", _atkParamPath);
            _outputFolder = EditorGUILayout.TextField("输出文件夹", _outputFolder);
            _modelIds = EditorGUILayout.TextField(new GUIContent("模型号(逗号分隔)", "WP_A_0200 → 200。可填多个：200,210,1000"), _modelIds);

            GUILayout.Space(8);
            if (GUILayout.Button("生成 / 更新命中框数据", GUILayout.Height(30)))
                Build();

            if (!string.IsNullOrEmpty(_report))
            {
                GUILayout.Space(8);
                _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(180));
                EditorGUILayout.TextArea(_report, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        private void Build()
        {
            var modelIds = ParseModelIds(_modelIds);
            if (modelIds.Count == 0) { _report = "请填至少一个模型号（如 200）。"; return; }

            // 1) EquipParamWeapon：modelId → behaviorVariationId
            if (!TryLoadEquipParam(out var modelToVar, out string err1)) { _report = err1; return; }
            // 2) BehaviorParam_PC：variationId → atkIds（refType==0）
            if (!TryLoadBehaviorParam(out var varToAtkIds, out string err2)) { _report = err2; return; }

            // 收集所有需要的 atkId，供 AtkParam 单次扫描
            var neededAtk = new HashSet<int>();
            var perModelAtk = new Dictionary<int, List<int>>();
            var sb = new StringBuilder();
            foreach (int model in modelIds)
            {
                if (!modelToVar.TryGetValue(model, out int varId))
                {
                    sb.AppendLine($"[跳过] 模型 {model}：EquipParamWeapon 里找不到 equipModelId。");
                    continue;
                }
                if (!varToAtkIds.TryGetValue(varId, out var atkIds) || atkIds.Count == 0)
                {
                    sb.AppendLine($"[跳过] 模型 {model}(varId={varId})：BehaviorParam_PC 里没有对应 refType=0 的行。");
                    continue;
                }
                perModelAtk[model] = atkIds;
                foreach (int a in atkIds) neededAtk.Add(a);
            }

            if (perModelAtk.Count == 0) { _report = "没有可生成的模型。\n\n" + sb; return; }

            // 3) AtkParam_Pc：atkId → segments
            if (!TryLoadAtkParam(neededAtk, out var atkToSegs, out string err3)) { _report = err3; return; }

            EnsureFolder(_outputFolder);

            int okCount = 0;
            foreach (var kv in perModelAtk)
            {
                int model = kv.Key;
                int varId = modelToVar[model];
                var perAttack = new List<WeaponAttackHitboxes>();
                foreach (int atkId in kv.Value)
                {
                    if (!atkToSegs.TryGetValue(atkId, out var segs) || segs.Count == 0) continue;
                    perAttack.Add(new WeaponAttackHitboxes { atkParamId = atkId, segments = segs.ToArray() });
                }

                if (perAttack.Count == 0)
                {
                    sb.AppendLine($"[跳过] 模型 {model}(varId={varId})：对应 AtkParam 都没有有效命中段。");
                    continue;
                }

                var union = BuildUnion(perAttack);
                string assetPath = $"{_outputFolder}/WeaponHitbox_{model}.asset";
                var so = AssetDatabase.LoadAssetAtPath<WeaponHitboxData>(assetPath);
                bool created = so == null;
                if (created) so = CreateInstance<WeaponHitboxData>();

                so.weaponModelId = model;
                so.behaviorVariationId = varId;
                so.perAttack = perAttack.ToArray();
                so.unionSegments = union;

                if (created) AssetDatabase.CreateAsset(so, assetPath);
                else EditorUtility.SetDirty(so);

                okCount++;
                sb.AppendLine($"[OK] 模型 {model}(varId={varId})：攻击 {perAttack.Count} 个，并集段 {union.Length} 段 → {Path.GetFileName(assetPath)}");
                sb.AppendLine("      并集段：" + DescribeSegments(union));
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _report = $"完成：生成/更新 {okCount} 个 WeaponHitboxData。\n输出：{_outputFolder}\n\n" + sb;
            Debug.Log(_report);
        }

        // ─────────────────────────────── CSV 解析 ───────────────────────────────

        private bool TryLoadEquipParam(out Dictionary<int, int> modelToVar, out string error)
        {
            modelToVar = new Dictionary<int, int>();
            error = null;
            if (!TryOpen(_equipParamPath, out var reader, out error)) return false;
            using (reader)
            {
                var cols = reader.ReadLine()?.Split(',');
                if (cols == null) { error = "EquipParamWeapon.csv 为空。"; return false; }
                int iModel = Array.IndexOf(cols, "equipModelId");
                int iVar = Array.IndexOf(cols, "behaviorVariationId");
                if (iModel < 0 || iVar < 0) { error = "EquipParamWeapon.csv 缺少 equipModelId / behaviorVariationId 列。"; return false; }

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var f = line.Split(',');
                    if (f.Length <= Math.Max(iModel, iVar)) continue;
                    if (!TryInt(f[iModel], out int model)) continue;
                    if (!TryInt(f[iVar], out int varId)) continue;
                    if (!modelToVar.ContainsKey(model)) modelToVar[model] = varId; // 同模型取第一条
                }
            }
            return true;
        }

        private bool TryLoadBehaviorParam(out Dictionary<int, List<int>> varToAtkIds, out string error)
        {
            varToAtkIds = new Dictionary<int, List<int>>();
            error = null;
            if (!TryOpen(_behaviorParamPath, out var reader, out error)) return false;
            using (reader)
            {
                var cols = reader.ReadLine()?.Split(',');
                if (cols == null) { error = "BehaviorParam_PC.csv 为空。"; return false; }
                int iVar = Array.IndexOf(cols, "variationId");
                int iRefType = Array.IndexOf(cols, "refType");
                int iRefId = Array.IndexOf(cols, "refId");
                if (iVar < 0 || iRefType < 0 || iRefId < 0) { error = "BehaviorParam_PC.csv 缺少 variationId / refType / refId 列。"; return false; }

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var f = line.Split(',');
                    int maxI = Math.Max(iVar, Math.Max(iRefType, iRefId));
                    if (f.Length <= maxI) continue;
                    if (!TryInt(f[iRefType], out int refType) || refType != 0) continue; // 0 = refId 指向 AtkParam
                    if (!TryInt(f[iVar], out int varId)) continue;
                    if (!TryInt(f[iRefId], out int refId)) continue;

                    if (!varToAtkIds.TryGetValue(varId, out var list)) { list = new List<int>(); varToAtkIds[varId] = list; }
                    if (!list.Contains(refId)) list.Add(refId);
                }
            }
            return true;
        }

        private bool TryLoadAtkParam(HashSet<int> needed, out Dictionary<int, List<WeaponHitSegment>> atkToSegs, out string error)
        {
            atkToSegs = new Dictionary<int, List<WeaponHitSegment>>();
            error = null;
            if (!TryOpen(_atkParamPath, out var reader, out error)) return false;
            using (reader)
            {
                var cols = reader.ReadLine()?.Split(',');
                if (cols == null) { error = "AtkParam_Pc.csv 为空。"; return false; }

                int iId = Array.IndexOf(cols, "ID");
                var iRad = new int[HitCount];
                var iD1 = new int[HitCount];
                var iD2 = new int[HitCount];
                int maxI = iId;
                for (int h = 0; h < HitCount; h++)
                {
                    iRad[h] = Array.IndexOf(cols, $"hit{h}_Radius");
                    iD1[h] = Array.IndexOf(cols, $"hit{h}_DmyPoly1");
                    iD2[h] = Array.IndexOf(cols, $"hit{h}_DmyPoly2");
                    maxI = Math.Max(maxI, Math.Max(iRad[h], Math.Max(iD1[h], iD2[h])));
                }
                if (iId < 0) { error = "AtkParam_Pc.csv 缺少 ID 列。"; return false; }

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var f = line.Split(',');
                    if (f.Length <= iId) continue;
                    if (!TryInt(f[iId], out int atkId) || !needed.Contains(atkId)) continue;
                    if (f.Length <= maxI) continue;

                    var segs = new List<WeaponHitSegment>();
                    for (int h = 0; h < HitCount; h++)
                    {
                        if (iD1[h] < 0) continue;
                        if (!TryInt(f[iD1[h]], out int d1) || d1 < 0) continue;         // DmyPoly1 无效 → 该段不存在
                        if (!TryFloat(f[iRad[h]], out float r) || r <= 0f) continue;    // 半径 <=0 → 忽略
                        TryInt(f[iD2[h]], out int d2);                                  // 可能 -1（单点球）
                        segs.Add(new WeaponHitSegment { dmyA = d1, dmyB = d2, radius = r });
                    }
                    if (segs.Count > 0) atkToSegs[atkId] = segs;
                }
            }
            return true;
        }

        // ─────────────────────────────── 并集 / 辅助 ───────────────────────────────

        // 跨所有攻击去重：同 (dmyA,dmyB) 段合并，半径取最大（覆盖最广，作生成胶囊与回退用）。
        private static WeaponHitSegment[] BuildUnion(List<WeaponAttackHitboxes> perAttack)
        {
            var map = new Dictionary<long, WeaponHitSegment>();
            var order = new List<long>();
            foreach (var atk in perAttack)
            {
                if (atk.segments == null) continue;
                foreach (var s in atk.segments)
                {
                    if (map.TryGetValue(s.Key, out var existing))
                    {
                        if (s.radius > existing.radius) { existing.radius = s.radius; map[s.Key] = existing; }
                    }
                    else { map[s.Key] = s; order.Add(s.Key); }
                }
            }
            var result = new WeaponHitSegment[order.Count];
            for (int i = 0; i < order.Count; i++) result[i] = map[order[i]];
            return result;
        }

        private static string DescribeSegments(WeaponHitSegment[] segs)
        {
            var sb = new StringBuilder();
            foreach (var s in segs)
            {
                if (sb.Length > 0) sb.Append("  ");
                sb.Append(s.IsSphere ? $"[{s.dmyA}]球 r{s.radius:F2}" : $"[{s.dmyA}→{s.dmyB}] r{s.radius:F2}");
            }
            return sb.ToString();
        }

        private static List<int> ParseModelIds(string raw)
        {
            var result = new List<int>();
            if (string.IsNullOrWhiteSpace(raw)) return result;
            foreach (var part in raw.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries))
                if (TryInt(part.Trim(), out int v) && !result.Contains(v)) result.Add(v);
            return result;
        }

        private static bool TryOpen(string path, out StreamReader reader, out string error)
        {
            reader = null; error = null;
            string abs = Path.IsPathRooted(path) ? path : Path.Combine(Directory.GetCurrentDirectory(), path);
            if (!File.Exists(abs)) { error = $"找不到文件：{abs}"; return false; }
            reader = new StreamReader(abs);
            return true;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), folder));
            AssetDatabase.Refresh();
        }

        private static bool TryInt(string s, out int v) =>
            int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v);

        private static bool TryFloat(string s, out float v) =>
            float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
    }
}
