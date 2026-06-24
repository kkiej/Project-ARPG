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
    /// 只读工具：解析某个武器招式集的 TAE JSON（如 a23.json），
    /// 筛出"带命中框的攻击槽位"，列出 clip 名、时长、命中窗、连招/取消窗、无敌帧、根运动，
    /// 导出成 CSV（供在 DSAS 里对照预览后给每个槽位贴 R1_1 / R2 等逻辑动作标签）。
    /// <para/>
    /// 不修改任何现有逻辑；只读取 JSON + 查找工程内 AnimationClip 以确认存在性与真实时长。
    /// 菜单：Tools → Moveset → TAE Attack Extractor
    /// </summary>
    public class MovesetTAEExtractor : EditorWindow
    {
        // ── 事件类型常量（实测自 a23.json，README 那张表与本导出不符） ──
        private static readonly HashSet<int> HitboxTypes = new HashSet<int> { 112, 113, 116 }; // Hitbox_DummyPoly/BodyNode/DamageHitbox
        private const int Type_InvokeAttackBehavior = 0;
        private const int Type_SetPlayerInput = 129; // 瞬时输入标记（非连续开窗，仅计数参考）
        private const int Type_Poise = 145;          // SuperArmor_Poise（霸体/削韧保护，category=iframe）
        private const int Type_RootMotionMult = 605; // RootMotion_Mult
        private static readonly HashSet<int> WeaponTrailTypes = new HashSet<int> { 700, 712 }; // WeaponTrail / SpawnFFX_WeaponBoth（刀光，可辅助识别"真挥砍"）

        [SerializeField] private string _jsonPath = "Assets/_ELDENRING_REF/TAE_Events/a23.json";
        [SerializeField] private string _clipPrefix = "a023";
        [SerializeField] private string _outputCsv = "Assets/_ELDENRING_REF/a023_moveset_candidates.csv";
        [SerializeField] private bool _onlyWithHitbox = true;
        [SerializeField] private bool _resolveClipLength = true;

        [MenuItem("Tools/Moveset/TAE Attack Extractor")]
        public static void ShowWindow()
        {
            GetWindow<MovesetTAEExtractor>("TAE Attack Extractor");
        }

        private void OnGUI()
        {
            GUILayout.Label("TAE 攻击槽位提取器（只读）", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "解析某武器招式集的 TAE JSON，筛出攻击槽位并导出 CSV。\n" +
                "clip 名 = clipPrefix + '_' + rawId(6位)。例：a23.json + 前缀 a023 → a023_030000。",
                MessageType.Info);
            GUILayout.Space(6);

            _jsonPath = EditorGUILayout.TextField("TAE JSON 路径", _jsonPath);
            _clipPrefix = EditorGUILayout.TextField("Clip 前缀", _clipPrefix);
            _outputCsv = EditorGUILayout.TextField("输出 CSV", _outputCsv);
            _onlyWithHitbox = EditorGUILayout.Toggle("仅含命中框的槽位", _onlyWithHitbox);
            _resolveClipLength = EditorGUILayout.Toggle("查找 Clip 真实时长", _resolveClipLength);

            GUILayout.Space(10);
            if (GUILayout.Button("提取 → CSV + Console", GUILayout.Height(30)))
            {
                Extract();
            }
        }

        private void Extract()
        {
            string fullJson = ToAbsolute(_jsonPath);
            if (!File.Exists(fullJson))
            {
                EditorUtility.DisplayDialog("Error", $"找不到 JSON:\n{fullJson}", "OK");
                return;
            }

            TaeRoot root;
            try
            {
                root = JsonUtility.FromJson<TaeRoot>(File.ReadAllText(fullJson));
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"解析 JSON 失败:\n{e.Message}", "OK");
                return;
            }

            if (root?.animations == null || root.animations.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "JSON 无 animations 数据。", "OK");
                return;
            }

            var rows = new List<Row>();
            foreach (var anim in root.animations)
            {
                var row = BuildRow(anim);
                if (_onlyWithHitbox && !row.hasHitbox) continue;
                rows.Add(row);
            }

            rows.Sort((a, b) => a.rawId.CompareTo(b.rawId));

            WriteCsv(rows, root);
            PrintConsole(rows, root);

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("提取完成",
                $"TAE: {root.tae} (taeId {root.taeId})\n攻击槽位: {rows.Count}\n输出: {_outputCsv}", "OK");
        }

        private Row BuildRow(TaeAnim anim)
        {
            var row = new Row
            {
                rawId = anim.rawId,
                clip = $"{_clipPrefix}_{anim.rawId:000000}",
                hitboxStart = float.NaN,
                hitboxEnd = float.NaN,
                recoveryStart = float.NaN,
                recoveryEnd = float.NaN,
            };

            float maxFiniteEnd = 0f;

            if (anim.events != null)
            {
                foreach (var ev in anim.events)
                {
                    if (ev.end >= 0f && ev.end > maxFiniteEnd) maxFiniteEnd = ev.end;

                    if (HitboxTypes.Contains(ev.type))
                    {
                        row.hasHitbox = true;
                        row.hitboxCount++;
                        Expand(ref row.hitboxStart, ref row.hitboxEnd, ev.start, ev.end);
                    }
                    else if (ev.type == Type_InvokeAttackBehavior)
                    {
                        row.invokeAttackCount++;
                    }

                    if (ev.type == Type_SetPlayerInput) row.inputMarkerCount++;
                    if (ev.type == Type_Poise) row.hasPoise = true;
                    if (ev.type == Type_RootMotionMult) row.hasRootMotion = true;
                    if (WeaponTrailTypes.Contains(ev.type)) row.hasWeaponTrail = true;
                }
            }

            row.eventBasedDuration = maxFiniteEnd;

            if (_resolveClipLength)
            {
                if (TryResolveClip(row.clip, out float len))
                {
                    row.clipFound = true;
                    row.clipLength = len;
                }
            }

            // 推导"可取消恢复区"：命中框结束 → clip 结束（无 clip 时用事件时长兜底）。
            // 这是连招开窗的合理默认起点，设计师再微调，而非 TAE 精确数据。
            if (row.hasHitbox && !float.IsNaN(row.hitboxEnd) && !float.IsPositiveInfinity(row.hitboxEnd))
            {
                row.recoveryStart = row.hitboxEnd;
                row.recoveryEnd = row.clipFound ? row.clipLength : row.eventBasedDuration;
            }

            return row;
        }

        private static void Expand(ref float min, ref float max, float start, float end)
        {
            if (float.IsNaN(min) || start < min) min = start;
            // end<0 表示持续到动画结束；用一个大值占位以便排序显示
            float e = end < 0f ? float.PositiveInfinity : end;
            if (float.IsNaN(max) || e > max) max = e;
        }

        private static bool TryResolveClip(string clipName, out float length)
        {
            length = 0f;
            // 按名字找资源；优先匹配同名 fbx
            string[] guids = AssetDatabase.FindAssets(clipName);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                if (!path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.Equals(Path.GetFileNameWithoutExtension(path), clipName, StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (obj is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                    {
                        length = clip.length;
                        return true;
                    }
                }
            }
            return false;
        }

        private void WriteCsv(List<Row> rows, TaeRoot root)
        {
            var sb = new StringBuilder();
            sb.AppendLine("clip,rawId,clipFound,clipLength,hitboxStart,hitboxEnd,hitboxCount,invokeAttackCount,recoveryStart,recoveryEnd,hasPoise,hasWeaponTrail,hasRootMotion,inputMarkers,eventDuration,label");
            foreach (var r in rows)
            {
                sb.Append(r.clip).Append(',')
                  .Append(r.rawId).Append(',')
                  .Append(r.clipFound ? "1" : "0").Append(',')
                  .Append(F(r.clipFound ? r.clipLength : 0f)).Append(',')
                  .Append(F(r.hitboxStart)).Append(',')
                  .Append(F(r.hitboxEnd)).Append(',')
                  .Append(r.hitboxCount).Append(',')
                  .Append(r.invokeAttackCount).Append(',')
                  .Append(F(r.recoveryStart)).Append(',')
                  .Append(F(r.recoveryEnd)).Append(',')
                  .Append(r.hasPoise ? "1" : "0").Append(',')
                  .Append(r.hasWeaponTrail ? "1" : "0").Append(',')
                  .Append(r.hasRootMotion ? "1" : "0").Append(',')
                  .Append(r.inputMarkerCount).Append(',')
                  .Append(F(r.eventBasedDuration)).Append(',')
                  .Append(""); // label：留空，供 DSAS 对照后手填 R1_1 / R2 ...
                sb.AppendLine();
            }

            string full = ToAbsolute(_outputCsv);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            File.WriteAllText(full, sb.ToString(), new UTF8Encoding(false));
        }

        private static void PrintConsole(List<Row> rows, TaeRoot root)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[TAE Attack Extractor] {root.tae} (taeId {root.taeId}) — 攻击槽位 {rows.Count} 个");
            sb.AppendLine("clip            | len   | hitbox        | 可取消区       | poise trail rootM | invoke");
            foreach (var r in rows)
            {
                string len = r.clipFound ? r.clipLength.ToString("0.00", CultureInfo.InvariantCulture) : "  -  ";
                sb.AppendLine(
                    $"{r.clip,-15} | {len,5} | {Win(r.hitboxStart, r.hitboxEnd),-13} | {Win(r.recoveryStart, r.recoveryEnd),-13} | " +
                    $"{(r.hasPoise ? "Y" : "-"),-5} {(r.hasWeaponTrail ? "Y" : "-"),-5} {(r.hasRootMotion ? "Y" : "-"),-4} | {r.invokeAttackCount}");
            }
            Debug.Log(sb.ToString());
        }

        private static string Win(float s, float e)
        {
            if (float.IsNaN(s)) return "-";
            string es = float.IsPositiveInfinity(e) ? "end" : e.ToString("0.00", CultureInfo.InvariantCulture);
            return $"{s.ToString("0.00", CultureInfo.InvariantCulture)}~{es}";
        }

        private static string F(float v)
        {
            if (float.IsNaN(v)) return "";
            if (float.IsPositiveInfinity(v)) return "end";
            return v.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static string ToAbsolute(string assetPath)
        {
            string rel = assetPath.Replace("Assets/", "").Replace("Assets\\", "");
            return Path.Combine(Application.dataPath, rel);
        }

        private struct Row
        {
            public int rawId;
            public string clip;
            public bool clipFound;
            public float clipLength;
            public bool hasHitbox;
            public int hitboxCount;
            public int invokeAttackCount;
            public float hitboxStart, hitboxEnd;
            public int inputMarkerCount;
            public bool hasPoise;
            public bool hasWeaponTrail;
            public bool hasRootMotion;
            public float recoveryStart, recoveryEnd; // 推导的可取消区（连招开窗默认值）
            public float eventBasedDuration;
        }

        // ── JSON DTO（与 TAEEventImporter 的格式一致） ──
        [Serializable]
        private class TaeRoot
        {
            public string tae;
            public int taeId;
            public TaeAnim[] animations;
        }

        [Serializable]
        private class TaeAnim
        {
            public string animId;
            public int rawId;
            public TaeEvent[] events;
        }

        [Serializable]
        private class TaeEvent
        {
            public int type;
            public float start;
            public float end;
            public string name;
            public string category;
            public int[] @params;
        }
    }
}
