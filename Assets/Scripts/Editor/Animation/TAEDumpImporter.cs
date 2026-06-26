using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LZ.Editor
{
    /// <summary>
    /// 权威 TAE 导入器：直接解析 DSAS 导出的 c0000 完整 TAE 转储文本
    /// （<c>Assets/_ELDENRING_REF/c0000.anibnd.dcx.txt</c>，13,700 个动画、事件名/type/flag 全对），
    /// 生成单个 <see cref="TAEEventData"/> ScriptableObject。
    /// <para/>
    /// 取代早期「Python 提取 JSON（事件名大量错）→ SO」的链路：本工具的名称、type、flag、帧区间均来自转储，权威可信。
    /// <list type="bullet">
    /// <item>每条动画 <c>animId</c>=如 a023_030000；<c>rawId</c>=角色内唯一整型 = category*1_000_000 + slot（a023_030000 → 23030000）。</item>
    /// <item>时间从「帧」换算为「秒」（÷30）；止帧 <c>M</c> → endTime=-1（持续到动画结束）。</item>
    /// <item><c>ChrActionFlag</c>(type 0) 的 flag 号写入 <c>parameters[0]</c>；<c>category</c> 按语义派生
    ///       （combat / cancel / input / iframe / consume…），使 <see cref="TAEEventQuery"/> 的 Has* 助手开箱即用。</item>
    /// </list>
    /// 菜单：Tools → TAE → Import c0000 Dump → SO
    /// </summary>
    public class TAEDumpImporter : EditorWindow
    {
        [SerializeField] private string _dumpPath = "Assets/_ELDENRING_REF/c0000.anibnd.dcx.txt";
        [SerializeField] private string _outputAsset = "Assets/_ELDENRING_REF/TAE_Events_SO/c0000_TAE.asset";
        [SerializeField] private bool _combatRelevantOnly = true;

        private string _report;
        private Vector2 _scroll;

        // 帧率：ER TAE 帧 ÷ 30 = 秒（与喝药 8/30≈0.2667、翻滚 16/30≈0.533 等实测一致）。
        private const float Fps = 30f;

        // 战斗相关 type 白名单（控体积；其余 SFX/贴花/相机/Wwise/调试等丢弃）。来源见 TAE_Events/README.md §5。
        private static readonly HashSet<int> CombatTypes = new HashSet<int>
        {
            0,    // ChrActionFlag：连招窗/取消窗/输入窗/无敌帧/霸体/受伤修正（全部 flag）
            1,    // AttackBehavior（近战判定）
            2,    // BulletBehavior
            5,    // CommonBehavior
            64,   // CastHighlightedMagic（法术释放时点）
            65,   // ConsumeCurrentGoods（喝药/吃道具回血本体）
            66,   // AddSpEffect_Multiplayer
            67,   // AddSpEffect
            236,  // RootMotionReduction
            304,  // ThrowAttackBehavior
            330,  // WeaponArtFPConsumption
            331,  // AddSpEffect_WeaponArts
            605,  // SetTimeActEditorHavokVariable
            760,  // BoostRootMotionToReachTarget
            795,  // DS3Poise
        };

        // type 0 flag 分类（→ TAEEvent.category）。见 README §5.3。
        private static readonly HashSet<int> IFrameFlags = new HashSet<int> { 8, 24, 67, 94, 132, 134, 143 };
        private static readonly HashSet<int> InputFlags  = new HashSet<int> { 87, 1, 9, 10, 21, 25, 30, 80, 105, 106, 108, 111, 15 };
        private static readonly HashSet<int> CancelFlags = new HashSet<int> { 4, 115, 116, 117, 118, 16, 103, 104, 107, 11, 18, 22, 26, 29, 31, 32, 60, 75, 112 };

        // 表头：真实动画 "aXXX_YYYYYY aXXX_ZZZZZZ.hkt"，别名动画为裸 id "aXXX_YYYYYY"（无 .hkt）→ 允许 id 后直接到行尾。
        private static readonly Regex HeaderRe = new Regex(@"^(a\d{3}_\d{6})(?:$|\s)", RegexOptions.Compiled);
        // 事件行：起/止帧可为负（事件早于第 0 帧），止帧亦可为 M（持续到结尾）；
        // 名称前可有 "[ExePatch]" / "[DS1, Disabled]" 等标记（group3，可空）。
        private static readonly Regex EventRe  = new Regex(@"^\s*(-?\d+)-(-?\d+|M)\s+(?:\[([^\]]*)\]\s*)?([A-Za-z0-9_<>]+)\[(\d+)\]\((.*)\)\s*$", RegexOptions.Compiled);
        // 别名动画：整段事件复用另一动画（自身无事件行），如 "Imports all events from a000_202100."。
        private static readonly Regex AliasRe  = new Regex(@"Imports all events from (a\d{3}_\d{6})", RegexOptions.Compiled);

        [MenuItem("Tools/TAE/Import c0000 Dump → SO")]
        public static void ShowWindow() => GetWindow<TAEDumpImporter>("TAE Dump Importer");

        private void OnGUI()
        {
            GUILayout.Label("权威 TAE 导入器（c0000 转储 → SO）", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "直接解析 c0000.anibnd.dcx.txt（事件名/type/flag 全对），生成单个 TAEEventData SO。\n" +
                "rawId = category*1_000_000 + slot（a023_030000 → 23030000）。时间帧÷30=秒，M→-1。",
                MessageType.Info);
            GUILayout.Space(6);

            _dumpPath = EditorGUILayout.TextField("转储 txt 路径", _dumpPath);
            _outputAsset = EditorGUILayout.TextField("输出 SO 路径", _outputAsset);
            _combatRelevantOnly = EditorGUILayout.Toggle(
                new GUIContent("仅保留战斗相关 type", "勾选只存 ChrActionFlag/判定/喝药/法术/根运动等；关闭则存全部事件（SO 会很大）。"),
                _combatRelevantOnly);

            GUILayout.Space(10);
            if (GUILayout.Button("解析 → 生成 / 更新 SO", GUILayout.Height(30)))
                Import();

            if (!string.IsNullOrEmpty(_report))
            {
                GUILayout.Space(8);
                _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(160));
                EditorGUILayout.TextArea(_report, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        private void Import()
        {
            string full = ToAbsolute(_dumpPath);
            if (!File.Exists(full))
            {
                EditorUtility.DisplayDialog("Error", $"找不到转储文件:\n{full}", "OK");
                return;
            }

            int animCount = 0, eventCount = 0, droppedEvents = 0;
            var entries = new List<TAEAnimationEntry>(14000);
            var aliasMap = new Dictionary<string, string>();   // animId → 复用源 animId

            string curAnimId = null;
            int curRawId = 0;
            List<TAEEvent> curEvents = null;

            void Flush()
            {
                if (curAnimId == null) return;
                entries.Add(new TAEAnimationEntry
                {
                    animId = curAnimId,
                    rawId = curRawId,
                    events = curEvents != null ? curEvents.ToArray() : Array.Empty<TAEEvent>(),
                });
                animCount++;
            }

            try
            {
                using var reader = new StreamReader(full);
                string line;
                int lineNo = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNo++;
                    if (lineNo % 50000 == 0)
                        EditorUtility.DisplayProgressBar("Importing TAE Dump", $"line {lineNo}…", -1f);

                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("----"))
                        continue;

                    Match h = HeaderRe.Match(line);
                    if (h.Success)
                    {
                        Flush();
                        curAnimId = h.Groups[1].Value;
                        curRawId = ToFullId(curAnimId);
                        curEvents = new List<TAEEvent>();
                        continue;
                    }

                    if (curAnimId == null) continue;

                    Match e = EventRe.Match(line);
                    if (!e.Success)
                    {
                        Match al = AliasRe.Match(line);
                        if (al.Success) aliasMap[curAnimId] = al.Groups[1].Value;
                        continue;
                    }

                    // 跳过被禁用的事件（"[..., Disabled]" / "[DS1, Disabled]" 等遗留未激活事件，不应作为生效窗导入）。
                    string tag = e.Groups[3].Value;
                    if (!string.IsNullOrEmpty(tag) && tag.IndexOf("Disabled", StringComparison.OrdinalIgnoreCase) >= 0)
                    { droppedEvents++; continue; }

                    int type = int.Parse(e.Groups[5].Value, CultureInfo.InvariantCulture);
                    if (_combatRelevantOnly && !CombatTypes.Contains(type)) { droppedEvents++; continue; }

                    int startF = int.Parse(e.Groups[1].Value, CultureInfo.InvariantCulture);
                    string endTok = e.Groups[2].Value;
                    float startSec = startF / Fps;
                    float endSec = endTok == "M" ? -1f : int.Parse(endTok, CultureInfo.InvariantCulture) / Fps;

                    string evName = e.Groups[4].Value;
                    int[] prms = ParseParams(e.Groups[6].Value);

                    curEvents.Add(new TAEEvent
                    {
                        type = type,
                        name = evName,
                        category = DeriveCategory(type, prms),
                        startTime = startSec,
                        endTime = endSec,
                        parameters = prms,
                    });
                    eventCount++;
                }
                Flush();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            // ── 解析别名：把「复用源」动画的事件拷给别名动画（自身无事件行）──
            int aliasResolved = 0, aliasMissing = 0;
            if (aliasMap.Count > 0)
            {
                var idx = new Dictionary<string, int>(entries.Count);
                for (int i = 0; i < entries.Count; i++) idx[entries[i].animId] = i;

                foreach (var kv in aliasMap)
                {
                    if (!idx.TryGetValue(kv.Key, out int si)) continue;
                    if (entries[si].events != null && entries[si].events.Length > 0) continue; // 已有自身事件，不覆盖

                    // 顺着 alias 链找到第一个有事件的源（防环：最多跳 8 次）。
                    string target = kv.Value;
                    TAEEvent[] src = null;
                    for (int hop = 0; hop < 8 && target != null; hop++)
                    {
                        if (!idx.TryGetValue(target, out int ti)) break;
                        if (entries[ti].events != null && entries[ti].events.Length > 0) { src = entries[ti].events; break; }
                        target = aliasMap.TryGetValue(target, out string next) ? next : null;
                    }

                    if (src != null)
                    {
                        var ent = entries[si];
                        ent.events = src;       // 共享同一事件数组（只读，安全）
                        entries[si] = ent;
                        aliasResolved++;
                    }
                    else aliasMissing++;
                }
            }

            // 写 / 更新 SO
            var existing = AssetDatabase.LoadAssetAtPath<TAEEventData>(_outputAsset);
            TAEEventData data = existing != null ? existing : ScriptableObject.CreateInstance<TAEEventData>();
            data.tae = "c0000";
            data.taeId = 0;
            data.animations = entries.ToArray();
            data.InvalidateIndex();

            if (existing != null)
            {
                EditorUtility.SetDirty(existing);
            }
            else
            {
                string absOut = ToAbsolute(_outputAsset);
                Directory.CreateDirectory(Path.GetDirectoryName(absOut));
                AssetDatabase.CreateAsset(data, _outputAsset);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _report =
                $"[TAE Dump Importer] 完成\n" +
                $"动画 {animCount} 条，事件 {eventCount} 条" +
                (_combatRelevantOnly ? $"（已丢弃非战斗事件 {droppedEvents} 条）" : "（全量）") + "\n" +
                $"别名复用动画：解析 {aliasResolved} 条" + (aliasMissing > 0 ? $"，源缺失 {aliasMissing} 条" : "") + "\n" +
                $"输出：{_outputAsset}\n" +
                $"示例查询：data.GetAnimation(27100)=翻滚, data.GetAnimation(23030000)=直剑R1一段。";
            Debug.Log(_report);
            Selection.activeObject = data;
        }

        /// <summary>"a023_030000" → 23*1_000_000 + 30000 = 23030000（角色内唯一整型 id）。</summary>
        private static int ToFullId(string animId)
        {
            // 格式固定 aCCC_SSSSSS
            int cat = int.Parse(animId.Substring(1, 3), CultureInfo.InvariantCulture);
            int slot = int.Parse(animId.Substring(5, 6), CultureInfo.InvariantCulture);
            return cat * 1_000_000 + slot;
        }

        /// <summary>
        /// 解析参数串为 int[]：按顶层逗号切分，取每段前导整数
        /// （枚举 "8: Flag As Dodging"→8、位掩码 "1|1"→1、"-1: No Offset"→-1、True→1/False→0；无法解析→0）。
        /// 关键用途：type 0 的 parameters[0] = flag 号。
        /// </summary>
        private static int[] ParseParams(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<int>();

            var segs = SplitTopLevel(raw);
            var result = new int[segs.Count];
            for (int i = 0; i < segs.Count; i++)
                result[i] = LeadingInt(segs[i]);
            return result;
        }

        private static List<string> SplitTopLevel(string s)
        {
            var list = new List<string>();
            int depth = 0, start = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '(') depth++;
                else if (c == ')') { if (depth > 0) depth--; }
                else if (c == ',' && depth == 0)
                {
                    list.Add(s.Substring(start, i - start));
                    start = i + 1;
                }
            }
            list.Add(s.Substring(start));
            return list;
        }

        private static int LeadingInt(string seg)
        {
            string t = seg.Trim();
            if (t.Length == 0) return 0;

            // 截到第一个分隔符（枚举冒号 / 位掩码竖线 / 空格）之前
            int cut = t.Length;
            for (int i = 0; i < t.Length; i++)
            {
                char c = t[i];
                if (c == ':' || c == '|' || c == ' ') { cut = i; break; }
            }
            string head = t.Substring(0, cut);
            if (int.TryParse(head, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
                return v;
            if (head.Equals("True", StringComparison.OrdinalIgnoreCase)) return 1;
            if (head.Equals("False", StringComparison.OrdinalIgnoreCase)) return 0;
            return 0;
        }

        private static string DeriveCategory(int type, int[] prms)
        {
            switch (type)
            {
                case 1: case 2: case 5: case 304: return "combat";   // 判定
                case 65: return "consume";                            // 喝药/道具消耗
                case 64: return "cast";                               // 法术释放
                case 0:
                    if (prms != null && prms.Length > 0)
                    {
                        int f = prms[0];
                        if (IFrameFlags.Contains(f)) return "iframe";
                        if (CancelFlags.Contains(f)) return "cancel";
                        if (InputFlags.Contains(f))  return "input";
                    }
                    return "actionflag";
                default: return "";
            }
        }

        private static string ToAbsolute(string assetPath)
        {
            string rel = assetPath.Replace("Assets/", "").Replace("Assets\\", "");
            return Path.Combine(Application.dataPath, rel);
        }
    }
}
