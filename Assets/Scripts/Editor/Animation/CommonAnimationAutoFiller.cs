using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LZ.Editor
{
    /// <summary>
    /// ER 通用动画（<c>a000_</c>）自动回填器：读 <see cref="CommonAnimationConvention"/> 的基址，
    /// 按"基址 + 负重组×10 + 方向"枚举 ID，在指定文件夹查同名 <c>a000_xxxxxx</c> clip，回填进 <see cref="CommonAnimationSet"/>。
    /// <para/>
    /// 与 <see cref="MovesetAutoFiller"/> 同范式：手填上万通用动画不现实，这里按约定一键扫出当前角色用得到的子集。
    /// 菜单：Tools → Moveset → Common Animation Filler。
    /// </summary>
    public class CommonAnimationAutoFiller : EditorWindow
    {
        [SerializeField] private CommonAnimationConvention _convention;
        [SerializeField] private CommonAnimationSet _target;
        [SerializeField] private string _outputPath = "Assets/Data/Animation/CommonAnimationSet_c0000.asset";
        [SerializeField] private string _searchFolder = "Assets/_ELDENRING_REF/Animations";
        // 负重仅 3 档（轻=0/中=10/重=20），权威表 §6.0。
        [SerializeField] private int _maxLoadGroup = CommonAnimationConvention.LoadHeavy;
        // 要扫描的姿态前缀（aXXX 类别号）。缺失文件自动跳过，多列无害。
        [SerializeField] private int[] _stances =
        {
            (int)CommonStance.OneHandLight,  // a000
            (int)CommonStance.OneHandHeavy,  // a002
            (int)CommonStance.OneHandLong,   // a003
            (int)CommonStance.TwoHandLight,  // a010
            (int)CommonStance.TwoHandHeavy,  // a012
            (int)CommonStance.TwoHandLong,   // a013
        };

        private Vector2 _scroll;
        private string _report;

        [MenuItem("Tools/Moveset/Common Animation Filler")]
        public static void ShowWindow() => GetWindow<CommonAnimationAutoFiller>("Common Anim Filler");

        private void OnGUI()
        {
            GUILayout.Label("通用动画自动回填器", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "按 [姿态前缀 aXXX] × [负重 0轻/1中/2重] × [方向 0-3] 枚举动画 ID，扫文件夹回填到 CommonAnimationSet。\n" +
                "翻滚固定 a000 前缀；后撤步 / locomotion 随姿态前缀变。缺失的 ID 自动跳过。",
                MessageType.Info);
            GUILayout.Space(6);

            _convention = (CommonAnimationConvention)EditorGUILayout.ObjectField("通用动画约定表", _convention, typeof(CommonAnimationConvention), false);
            _searchFolder = EditorGUILayout.TextField("搜索文件夹", _searchFolder);
            _maxLoadGroup = EditorGUILayout.IntField("最大负重档 (含，2=重)", _maxLoadGroup);

            var so = new SerializedObject(this);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(_stances)), new GUIContent("扫描的姿态前缀 (aXXX)"), true);
            so.ApplyModifiedProperties();

            GUILayout.Space(4);
            GUILayout.Label("输出目标（二选一）", EditorStyles.boldLabel);
            _target = (CommonAnimationSet)EditorGUILayout.ObjectField("已有 Set(就地填充)", _target, typeof(CommonAnimationSet), false);
            using (new EditorGUI.DisabledScope(_target != null))
            {
                _outputPath = EditorGUILayout.TextField("或新建到路径", _outputPath);
            }

            GUILayout.Space(10);
            using (new EditorGUI.DisabledScope(_convention == null))
            {
                if (GUILayout.Button("生成 / 填充 CommonAnimationSet", GUILayout.Height(30)))
                    Generate();
            }

            if (!string.IsNullOrEmpty(_report))
            {
                GUILayout.Space(8);
                GUILayout.Label("结果", EditorStyles.boldLabel);
                _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(200));
                EditorGUILayout.TextArea(_report, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        private void Generate()
        {
            var sb = new StringBuilder();
            var entries = new List<CommonAnimationSet.Entry>();
            var seen = new HashSet<int>();
            int found = 0, missing = 0;

            int[] stances = (_stances != null && _stances.Length > 0)
                ? _stances
                : new[] { (int)CommonStance.OneHandLight };

            // ── 随姿态前缀变的动作：locomotion + 后撤步 ──
            foreach (int stance in stances)
            {
                string p = $"a{stance:000}";

                // idle：单锚点（a000_000000，仅负重组 0、无方向）。idleId 合法可为 0，用 IdleUnset(-1) 判未配。
                if (_convention.idleId != CommonAnimationConvention.IdleUnset)
                    AddGroups(entries, seen, sb, $"Idle[{p}]", stance, _convention.idleId, 0, false, ref found, ref missing);

                // 走 / 慢跑：四向 × 负重
                AddGroups(entries, seen, sb, $"Walk[{p}]", stance, _convention.walkBase, _maxLoadGroup, true, ref found, ref missing);
                AddGroups(entries, seen, sb, $"Jog[{p}]", stance, _convention.jogBase, _maxLoadGroup, true, ref found, ref missing);
                // 奔跑：仅前向 × 负重
                AddGroups(entries, seen, sb, $"Run[{p}]", stance, _convention.runBase, _maxLoadGroup, false, ref found, ref missing);
                // 刹停（§6.4）：快走四向 / 奔跑前向 × 负重
                AddGroups(entries, seen, sb, $"JogStop[{p}]", stance, _convention.jogStopBase, _maxLoadGroup, true, ref found, ref missing);
                AddGroups(entries, seen, sb, $"RunStop[{p}]", stance, _convention.runStopBase, _maxLoadGroup, false, ref found, ref missing);
                // 后撤步：单向 × 负重（权威表 §6.2：随姿态前缀，无方向）
                AddGroups(entries, seen, sb, $"Backstep[{p}]", stance, _convention.backstepBase, _maxLoadGroup, false, ref found, ref missing);
            }

            // ── 固定 a000 前缀的动作（权威表 §6.3 翻滚；蹲走 / 前手翻当前仅见 a000）──
            int a000 = CommonAnimationConvention.RollStance;
            // 翻滚：四向 × 负重
            AddGroups(entries, seen, sb, "Roll", a000, _convention.rollBase, _maxLoadGroup, true, ref found, ref missing);
            // 蹲走（旧 021000 慢走）：四向，单档
            AddGroups(entries, seen, sb, "CrouchWalk", a000, _convention.crouchWalkBase, 0, true, ref found, ref missing);
            // 下蹲（ER a000_3xxxxx）：idle 单锚点 / 移动四向 / 进入 / 站起
            AddGroups(entries, seen, sb, "CrouchIdle", a000, _convention.crouchIdleBase, 0, false, ref found, ref missing);
            AddGroups(entries, seen, sb, "CrouchMove", a000, _convention.crouchMoveBase, 0, true, ref found, ref missing);
            AddGroups(entries, seen, sb, "CrouchEnter", a000, _convention.crouchEnterId, 0, false, ref found, ref missing);
            AddGroups(entries, seen, sb, "CrouchStandup", a000, _convention.crouchStandupId, 0, false, ref found, ref missing);
            // 前手翻：四向，单档
            AddGroups(entries, seen, sb, "Handspring", a000, _convention.handspringBase, 0, true, ref found, ref missing);

            // ── P3 通用动作（固定 a000，§6.6）：换武 / 喝药 / 无道具 ──
            // 换武（单 id；变体范围 290xx 由具体武器决定，先收基址，后续按需细分）。
            AddGroups(entries, seen, sb, "SwapRight", a000, _convention.weaponSwapRightBase, 0, false, ref found, ref missing);
            AddGroups(entries, seen, sb, "SwapLeft", a000, _convention.weaponSwapLeftBase, 0, false, ref found, ref missing);
            // 喝药 drink：四向枚举 d0-3 顺带覆盖 50110/111/112/113（缺失自动跳过）。
            if (_convention.flaskDrinkBase != CommonAnimationConvention.IdleUnset)
                AddGroups(entries, seen, sb, "FlaskDrink", a000, _convention.flaskDrinkBase, 0, true, ref found, ref missing);
            // 无道具 / 空瓶 50050（单 id）。
            if (_convention.noItemUseBase != CommonAnimationConvention.IdleUnset)
                AddGroups(entries, seen, sb, "NoItemUse", a000, _convention.noItemUseBase, 0, false, ref found, ref missing);

            CommonAnimationSet set = _target;
            bool isNew = set == null;
            if (isNew)
                set = CreateInstance<CommonAnimationSet>();

            set.entries = entries;

            if (isNew)
            {
                string full = ToAbsolute(_outputPath);
                Directory.CreateDirectory(Path.GetDirectoryName(full));
                AssetDatabase.CreateAsset(set, _outputPath);
            }
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            sb.Insert(0, $"[Common Animation Filler] 命中 {found}，跳过(缺失) {missing}，共 {entries.Count} 条。\n" +
                         $"目标 {(isNew ? _outputPath : AssetDatabase.GetAssetPath(set))}\n\n");
            _report = sb.ToString();
            Debug.Log(_report);
            Selection.activeObject = set;
        }

        private void AddGroups(
            List<CommonAnimationSet.Entry> entries, HashSet<int> seen, StringBuilder sb,
            string label, int stance, int baseId, int maxGroup, bool fourDirections, ref int found, ref int missing)
        {
            for (int g = 0; g <= maxGroup; g++)
            {
                int dirCount = fourDirections ? 4 : 1;
                for (int d = 0; d < dirCount; d++)
                {
                    int animId = CommonAnimationConvention.ComposeId(stance, baseId, g, d);
                    int suffix = CommonAnimationConvention.ComposeSuffix(baseId, g, d);
                    AddSlot(entries, seen, sb, $"{label}_g{g}_d{d}", stance, animId, suffix, ref found, ref missing);
                }
            }
        }

        private void AddSlot(
            List<CommonAnimationSet.Entry> entries, HashSet<int> seen, StringBuilder sb,
            string label, int stance, int animId, int suffix, ref int found, ref int missing)
        {
            if (!seen.Add(animId)) return;

            string clipName = $"a{stance:000}_{suffix:000000}";
            AnimationClip clip = ResolveClip(clipName);
            if (clip == null)
            {
                missing++;
                return;
            }

            entries.Add(new CommonAnimationSet.Entry { label = label, animId = animId, clip = clip });
            found++;
            sb.AppendLine($"  [ok] {label,-16} {clipName} (animId {animId})");
        }

        /// <summary>按名字解析工程内 AnimationClip（优先同名 fbx 内的主 clip）。与 MovesetAutoFiller 一致。</summary>
        private AnimationClip ResolveClip(string clipName)
        {
            string[] guids = string.IsNullOrEmpty(_searchFolder)
                ? AssetDatabase.FindAssets(clipName)
                : AssetDatabase.FindAssets(clipName, new[] { _searchFolder });

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
    }
}
