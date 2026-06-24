using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LZ.Editor
{
    /// <summary>
    /// 自动回填器：读 <see cref="MovesetSlotConvention"/> 通用约定表 + 一个武器类别号(wepMotionCategory)，
    /// 按 ER 动画 ID 约定解析出每个槽位对应的 AnimationClip，自动填进一份 <see cref="MovesetData"/>。
    /// <para/>
    /// 因为槽位约定在所有同类武器间通用，换 category 号即可为任意武器一键生成 moveset，
    /// 不必逐个动画在 DSAS 里对照。
    /// 菜单：Tools → Moveset → Auto Filler
    /// </summary>
    public class MovesetAutoFiller : EditorWindow
    {
        [SerializeField] private MovesetSlotConvention _convention;
        [SerializeField] private CharacterAnimationData _baseAnimData;
        [SerializeField] private int _wepMotionCategory = 23;
        [SerializeField] private string _outputPath = "Assets/Data/Moveset/Moveset_a023_StraightSword.asset";
        [SerializeField] private bool _skipMissingClips = true;
        [SerializeField] private MovesetData _target;

        [Tooltip("TAE JSON：用于回填连招开窗（Input-Common 窗）。留空则不填窗，运行时回退到归一化窗口。")]
        [SerializeField] private string _taeJsonPath = "Assets/_ELDENRING_REF/TAE_Events/a23.json";

        // ER ChrActionFlag id：87 = Input - Common（可输入下一段动作的窗口）。
        private const int Flag_InputCommon = 87;

        private Vector2 _scroll;
        private string _report;

        [MenuItem("Tools/Moveset/Auto Filler")]
        public static void ShowWindow() => GetWindow<MovesetAutoFiller>("Moveset Auto Filler");

        private void OnGUI()
        {
            GUILayout.Label("Moveset 自动回填器", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "读通用槽位约定表 + 武器类别号，按 ER 动画 ID 约定自动解析 clip 并生成 MovesetData。\n" +
                "clip 名 = a{category:000}_{baseSlot+suffix:000000}，例：cat=23 → a023_030000。",
                MessageType.Info);
            GUILayout.Space(6);

            _convention = (MovesetSlotConvention)EditorGUILayout.ObjectField("槽位约定表", _convention, typeof(MovesetSlotConvention), false);
            _baseAnimData = (CharacterAnimationData)EditorGUILayout.ObjectField("基础动画数据(可空)", _baseAnimData, typeof(CharacterAnimationData), false);
            _wepMotionCategory = EditorGUILayout.IntField("武器类别号 (wepMotionCategory)", _wepMotionCategory);
            _skipMissingClips = EditorGUILayout.Toggle("跳过缺失 clip 的槽位", _skipMissingClips);
            _taeJsonPath = EditorGUILayout.TextField("TAE JSON(连招开窗,可空)", _taeJsonPath);

            GUILayout.Space(4);
            GUILayout.Label("输出目标（二选一）", EditorStyles.boldLabel);
            _target = (MovesetData)EditorGUILayout.ObjectField("已有 MovesetData(就地填充)", _target, typeof(MovesetData), false);
            using (new EditorGUI.DisabledScope(_target != null))
            {
                _outputPath = EditorGUILayout.TextField("或新建到路径", _outputPath);
            }

            GUILayout.Space(10);
            using (new EditorGUI.DisabledScope(_convention == null))
            {
                if (GUILayout.Button("生成 / 填充 MovesetData", GUILayout.Height(30)))
                    Generate();
            }

            if (!string.IsNullOrEmpty(_report))
            {
                GUILayout.Space(8);
                GUILayout.Label("结果", EditorStyles.boldLabel);
                _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(160));
                EditorGUILayout.TextArea(_report, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        // 节点生成顺序：连招链在前，便于阅读；用 slot→index 映射做连线。
        private static readonly AttackSlot[] NodeOrder =
        {
            AttackSlot.R1_1, AttackSlot.R1_2, AttackSlot.R1_3, AttackSlot.R1_4, AttackSlot.R1_5,
            AttackSlot.R2_1, AttackSlot.R2_2,
            AttackSlot.RunAttack, AttackSlot.RollAttack, AttackSlot.BackstepAttack,
            AttackSlot.JumpAttack, AttackSlot.GuardCounter,
        };

        private void Generate()
        {
            var sb = new StringBuilder();

            MovesetData data = _target;
            bool isNew = data == null;
            if (isNew)
            {
                data = ScriptableObject.CreateInstance<MovesetData>();
            }

            if (_baseAnimData != null) data.baseAnimData = _baseAnimData;

            Dictionary<int, Vector2> comboWindows = LoadComboWindows(sb);

            int totalNodes = 0, totalMissing = 0;
            foreach (var hand in _convention.hands)
            {
                if (!hand.enabled) continue;
                var moveset = BuildHandMoveset(hand, comboWindows, sb, out int nodeCount, out int missing);
                totalNodes += nodeCount;
                totalMissing += missing;

                switch (hand.handState)
                {
                    case HandState.OneHandRight: data.oneHandRight = moveset; break;
                    case HandState.TwoHand:      data.twoHand = moveset; break;
                    case HandState.DualWield:    data.dualWield = moveset; break;
                }
            }

            if (isNew)
            {
                string full = ToAbsolute(_outputPath);
                Directory.CreateDirectory(Path.GetDirectoryName(full));
                AssetDatabase.CreateAsset(data, _outputPath);
            }
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            sb.Insert(0, $"[Moveset Auto Filler] category a{_wepMotionCategory:000}\n" +
                         $"节点总数 {totalNodes}，缺失 clip {totalMissing}，目标 {(isNew ? _outputPath : AssetDatabase.GetAssetPath(data))}\n\n");
            _report = sb.ToString();
            Debug.Log(_report);
            Selection.activeObject = data;
        }

        private HandMoveset BuildHandMoveset(MovesetSlotConvention.HandBaseEntry hand, Dictionary<int, Vector2> comboWindows, StringBuilder sb, out int nodeCount, out int missing)
        {
            sb.AppendLine($"── {hand.handState} (base {hand.baseSlot}) ──");

            var nodes = new List<AttackNode>();
            var slotToIndex = new Dictionary<AttackSlot, int>();
            missing = 0;

            foreach (var slot in NodeOrder)
            {
                if (!_convention.TryGetSlot(slot, out var entry)) continue;

                int rawId = hand.baseSlot + entry.suffix;
                string clipName = MovesetSlotConvention.ComposeClipName(_wepMotionCategory, hand.baseSlot, entry.suffix);
                AnimationClip clip = ResolveClip(clipName);
                bool found = clip != null;
                if (!found) missing++;

                if (!found && _skipMissingClips)
                {
                    sb.AppendLine($"  [skip] {slot,-14} {clipName} (clip 未找到)");
                    continue;
                }

                Vector2 window = comboWindows != null && comboWindows.TryGetValue(rawId, out var w) ? w : Vector2.zero;

                var node = new AttackNode
                {
                    label = slot.ToString(),
                    clip = clip,
                    animId = MovesetSlotConvention.ComposeAnimId(_wepMotionCategory, hand.baseSlot, entry.suffix),
                    attackType = entry.attackType,
                    applyRootMotion = entry.applyRootMotion,
                    comboWindowStart = window.x,
                    comboWindowEnd = window.y,
                    canCharge = entry.canCharge,
                    chargedAttackType = entry.canCharge ? AttackType.ChargedAttack01 : entry.attackType,
                    nextOnLight = -1,
                    nextOnHeavy = -1,
                };

                slotToIndex[slot] = nodes.Count;
                nodes.Add(node);
                string win = window.y > 0f ? $"  win[{window.x:0.00}~{window.y:0.00}]" : "  win[-]";
                sb.AppendLine($"  [{(found ? "ok" : "null")}] {slot,-14} {clipName}{win}");
            }

            // ── 连线 ──
            var arr = nodes.ToArray();
            // R1 轻击链：R1_1→R1_2→...→R1_5
            LinkLight(arr, slotToIndex, AttackSlot.R1_1, AttackSlot.R1_2);
            LinkLight(arr, slotToIndex, AttackSlot.R1_2, AttackSlot.R1_3);
            LinkLight(arr, slotToIndex, AttackSlot.R1_3, AttackSlot.R1_4);
            LinkLight(arr, slotToIndex, AttackSlot.R1_4, AttackSlot.R1_5);
            // R2 重击链：R2_1→R2_2
            LinkHeavy(arr, slotToIndex, AttackSlot.R2_1, AttackSlot.R2_2);

            var ms = new HandMoveset
            {
                nodes = arr,
                lightOpener     = IndexOf(slotToIndex, AttackSlot.R1_1),
                heavyOpener     = IndexOf(slotToIndex, AttackSlot.R2_1),
                runAttack       = IndexOf(slotToIndex, AttackSlot.RunAttack),
                rollAttack      = IndexOf(slotToIndex, AttackSlot.RollAttack),
                backstepAttack  = IndexOf(slotToIndex, AttackSlot.BackstepAttack),
                jumpLight       = IndexOf(slotToIndex, AttackSlot.JumpAttack),
                jumpHeavy       = -1,
            };

            nodeCount = arr.Length;
            return ms;
        }

        private static void LinkLight(AttackNode[] nodes, Dictionary<AttackSlot, int> map, AttackSlot from, AttackSlot to)
        {
            if (map.TryGetValue(from, out int fi) && map.TryGetValue(to, out int ti))
                nodes[fi].nextOnLight = ti;
        }

        private static void LinkHeavy(AttackNode[] nodes, Dictionary<AttackSlot, int> map, AttackSlot from, AttackSlot to)
        {
            if (map.TryGetValue(from, out int fi) && map.TryGetValue(to, out int ti))
                nodes[fi].nextOnHeavy = ti;
        }

        private static int IndexOf(Dictionary<AttackSlot, int> map, AttackSlot slot)
            => map.TryGetValue(slot, out int i) ? i : -1;

        /// <summary>按名字解析工程内 AnimationClip（优先同名 fbx 内的主 clip）。</summary>
        private static AnimationClip ResolveClip(string clipName)
        {
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
                        return clip;
                }
            }
            return null;
        }

        private static string ToAbsolute(string assetPath)
        {
            string rel = assetPath.Replace("Assets/", "").Replace("Assets\\", "");
            return Path.Combine(Application.dataPath, rel);
        }

        /// <summary>
        /// 解析 TAE JSON，提取每个动画的连招开窗（Input-Common / flag 87 的 [start,end]）。
        /// 返回 rawId → (start, end) 秒。JSON 路径为空或解析失败时返回 null（运行时回退到归一化窗口）。
        /// </summary>
        private Dictionary<int, Vector2> LoadComboWindows(StringBuilder sb)
        {
            if (string.IsNullOrEmpty(_taeJsonPath))
                return null;

            string full = ToAbsolute(_taeJsonPath);
            if (!File.Exists(full))
            {
                sb.AppendLine($"[warn] TAE JSON 未找到，跳过连招开窗回填：{_taeJsonPath}");
                return null;
            }

            TaeRoot root;
            try { root = JsonUtility.FromJson<TaeRoot>(File.ReadAllText(full)); }
            catch (Exception e)
            {
                sb.AppendLine($"[warn] TAE JSON 解析失败，跳过开窗回填：{e.Message}");
                return null;
            }

            var map = new Dictionary<int, Vector2>();
            if (root?.animations == null) return map;

            foreach (var anim in root.animations)
            {
                if (anim.events == null) continue;

                // 取该动画内所有 Input-Common(flag 87) 事件的并集窗口（min start, max end）。
                float start = float.PositiveInfinity, end = float.NegativeInfinity;
                foreach (var ev in anim.events)
                {
                    // 该导出里 ChrActionFlag 被标为 type 0，params[0] 为 flag id。
                    if (ev.type != 0 || ev.@params == null || ev.@params.Length == 0) continue;
                    if (ev.@params[0] != Flag_InputCommon) continue;

                    if (ev.start < start) start = ev.start;
                    float e = ev.end < 0f ? ev.start : ev.end;
                    if (e > end) end = e;
                }

                if (end > start && end > 0f)
                    map[anim.rawId] = new Vector2(Mathf.Max(0f, start), end);
            }

            sb.AppendLine($"[info] 连招开窗回填来源 {_taeJsonPath}，含窗动画 {map.Count} 个。");
            return map;
        }

        // ── TAE JSON DTO（与 MovesetTAEExtractor 一致）──
        [Serializable] private class TaeRoot { public TaeAnim[] animations; }
        [Serializable] private class TaeAnim { public int rawId; public TaeEvent[] events; }
        [Serializable] private class TaeEvent { public int type; public float start; public float end; public int[] @params; }
    }
}
