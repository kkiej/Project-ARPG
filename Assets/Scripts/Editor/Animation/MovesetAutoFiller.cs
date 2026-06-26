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

        [Tooltip("权威 TAE 数据（c0000_TAE.asset，由 TAEDumpImporter 生成）：用于回填连招开窗。留空则不填窗，运行时回退到归一化窗口。")]
        [SerializeField] private TAEEventData _taeData;

        // ER ChrActionFlag id（DSAS 核实）：
        //   87  = Input-Common      —— 输入「缓冲」窗（何时能按下并记住下一击，偏早，范围宽）
        //   115 = Cancel-R1Attack   —— 可取消接「下一段 R1」的执行窗（连招真正开窗起点）
        //   4   = Cancel-RHAttack   —— 可取消接「右手攻击」的执行窗（通常衔接在 115 之后到 clip 尾）
        // 连招开窗取「可执行」窗 = (115 ∪ 4)；缺这俩时回退到 87（输入缓冲窗）。
        private const int Flag_InputCommon = 87;
        private const int Flag_CancelR1Attack = 115;
        private const int Flag_CancelRHAttack = 4;

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
            _taeData = (TAEEventData)EditorGUILayout.ObjectField("权威 TAE 数据(连招开窗,可空)", _taeData, typeof(TAEEventData), false);

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

                int fullId = MovesetSlotConvention.ComposeAnimId(_wepMotionCategory, hand.baseSlot, entry.suffix);
                string clipName = MovesetSlotConvention.ComposeClipName(_wepMotionCategory, hand.baseSlot, entry.suffix);
                AnimationClip clip = ResolveClip(clipName);
                bool found = clip != null;
                if (!found) missing++;

                if (!found && _skipMissingClips)
                {
                    sb.AppendLine($"  [skip] {slot,-14} {clipName} (clip 未找到)");
                    continue;
                }

                Vector2 window = comboWindows != null && comboWindows.TryGetValue(fullId, out var w) ? w : Vector2.zero;

                var node = new AttackNode
                {
                    label = slot.ToString(),
                    clip = clip,
                    animId = fullId,
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
        /// 从权威 TAE 数据（c0000_TAE.asset）提取本武器类别每个动画的连招开窗，键为完整 animId（category*1e6+slot）。
        /// 「可执行」窗 = Cancel-R1Attack(115) ∪ Cancel-RHAttack(4)；缺失才回退「缓冲」窗 Input-Common(87)。
        /// _taeData 为空时返回 null（运行时回退到归一化窗口）。
        /// </summary>
        private Dictionary<int, Vector2> LoadComboWindows(StringBuilder sb)
        {
            if (_taeData == null)
            {
                sb.AppendLine("[warn] 未指定权威 TAE 数据（c0000_TAE.asset），跳过连招开窗回填，运行时回退归一化窗口。");
                return null;
            }
            if (_taeData.animations == null) return new Dictionary<int, Vector2>();

            int catPrefix = _wepMotionCategory * 1_000_000;
            var map = new Dictionary<int, Vector2>();
            int exCount = 0, bufCount = 0;

            foreach (var anim in _taeData.animations)
            {
                // 只取本武器类别（rawId 高位 = category）。
                if (anim.rawId / 1_000_000 != _wepMotionCategory) continue;

                bool hasR1 = TAEEventQuery.TryGetActionFlagWindow(anim, Flag_CancelR1Attack, out float r1s, out float r1e);
                bool hasRH = TAEEventQuery.TryGetActionFlagWindow(anim, Flag_CancelRHAttack, out float rhs, out float rhe);
                bool hasBuf = TAEEventQuery.TryGetActionFlagWindow(anim, Flag_InputCommon, out float bs, out float be);

                float exStart = Mathf.Min(hasR1 ? r1s : float.PositiveInfinity, hasRH ? rhs : float.PositiveInfinity);
                float exEnd   = Mathf.Max(hasR1 ? r1e : float.NegativeInfinity, hasRH ? rhe : float.NegativeInfinity);

                if (hasR1 || hasRH)
                {
                    map[anim.rawId] = new Vector2(Mathf.Max(0f, exStart), exEnd);
                    exCount++;
                }
                else if (hasBuf)
                {
                    map[anim.rawId] = new Vector2(Mathf.Max(0f, bs), be);
                    bufCount++;
                }
            }

            sb.AppendLine($"[info] 连招开窗回填来源 {_taeData.name}（类别 a{_wepMotionCategory:000}），含窗动画 {map.Count} 个" +
                          $"（执行窗 115/4：{exCount}，回退缓冲窗 87：{bufCount}）。");
            return map;
        }
    }
}
