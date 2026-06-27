using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LZ.Editor
{
    /// <summary>
    /// 时间窗回填器：从权威 TAE 数据（c0000_TAE.asset，由 <see cref="TAEDumpImporter"/> 生成）按 animId + flag/type，
    /// 把运行期要用的时点烤进会入库的数据资产——与 <see cref="MovesetAutoFiller"/>（连招/取消窗 → MovesetData）同范式，
    /// 因为大 SO 是编辑期派生物、不入库也不在运行时加载。
    /// <list type="bullet">
    /// <item><b>闪避无敌帧</b>：翻滚 a000_027100 / 后撤步 a000_027000 的 <c>type0 flag8 (Flag As Dodging)</c> 窗 → <see cref="CommonAnimationConvention"/>（秒）。</item>
    /// <item><b>喝药消耗时点</b>：饮 a000_050111 的 <c>type65 (ConsumeCurrentGoods)</c> 起点 → 归一化写入 <see cref="CommonAnimationConvention.flaskConsumeNormalizedTime"/>。</item>
    /// <item><b>法术释放时点</b>：每个 <see cref="SpellItem"/> 按其 <c>erCastAnimId</c> 取 <c>type64 (CastHighlightedMagic)</c> 起点（秒）→ <see cref="SpellItem.castReleaseSeconds"/>。</item>
    /// </list>
    /// 菜单：Tools → TAE → Timing Backfiller
    /// </summary>
    public class TAETimingBackfiller : EditorWindow
    {
        [SerializeField] private TAEEventData _taeData;
        [SerializeField] private CommonAnimationConvention _convention;
        [SerializeField] private string _searchFolder = "Assets/_ELDENRING_REF/Animations";
        [SerializeField] private SpellItem[] _spells;

        // ChrActionFlag(type0) 的 flag 号 / 事件 type（见 TAE_Events/README.md §5）。
        private const int Type_ChrActionFlag = 0;
        private const int Flag_FlagAsDodging = 8;   // 翻滚/后撤步无敌帧
        private const int Type_ConsumeGoods  = 65;  // 喝药/道具消耗
        private const int Type_CastMagic     = 64;  // 法术释放

        private Vector2 _scroll;
        private string _report;

        [MenuItem("Tools/TAE/Timing Backfiller")]
        public static void ShowWindow() => GetWindow<TAETimingBackfiller>("TAE Timing Backfiller");

        private void OnGUI()
        {
            GUILayout.Label("TAE 时间窗回填器（权威 SO → 数据资产）", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "从 c0000_TAE.asset 按 animId + flag/type 回填运行期时点：\n" +
                "· 闪避无敌帧(flag8) + 喝药时点(type65) → CommonAnimationConvention\n" +
                "· 法术释放(type64) → 各 SpellItem.castReleaseSeconds",
                MessageType.Info);
            GUILayout.Space(6);

            _taeData = (TAEEventData)EditorGUILayout.ObjectField("权威 TAE 数据", _taeData, typeof(TAEEventData), false);

            GUILayout.Space(8);
            GUILayout.Label("① 闪避无敌帧 + 喝药时点 → Convention", EditorStyles.boldLabel);
            _convention = (CommonAnimationConvention)EditorGUILayout.ObjectField("通用动画约定表", _convention, typeof(CommonAnimationConvention), false);
            _searchFolder = EditorGUILayout.TextField("clip 搜索文件夹(算喝药归一化)", _searchFolder);
            using (new EditorGUI.DisabledScope(_taeData == null || _convention == null))
            {
                if (GUILayout.Button("回填 闪避无敌帧 + 喝药时点", GUILayout.Height(26)))
                    BackfillConvention();
            }

            GUILayout.Space(10);
            GUILayout.Label("② 法术释放时点 → SpellItem", EditorStyles.boldLabel);
            var so = new SerializedObject(this);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(_spells)), new GUIContent("要回填的法术"), true);
            so.ApplyModifiedProperties();
            using (new EditorGUI.DisabledScope(_taeData == null || _spells == null || _spells.Length == 0))
            {
                if (GUILayout.Button("回填 法术释放时点", GUILayout.Height(26)))
                    BackfillSpells();
            }

            if (!string.IsNullOrEmpty(_report))
            {
                GUILayout.Space(8);
                _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(160));
                EditorGUILayout.TextArea(_report, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        private void BackfillConvention()
        {
            var sb = new StringBuilder();

            // 翻滚 a000_027100：flag8 窗（秒）。
            int rollId = CommonAnimationConvention.ComposeId(
                CommonAnimationConvention.RollStance, _convention.rollBase,
                CommonAnimationConvention.LoadLight, CommonAnimationConvention.DirForward);
            if (TryFlagWindow(rollId, Flag_FlagAsDodging, out float rs, out float re))
            {
                _convention.rollIFrameStartSeconds = rs;
                _convention.rollIFrameEndSeconds = re;
                sb.AppendLine($"[ok] 翻滚无敌帧 a{rollId:000000}/{rollId}: flag8 = {rs:0.###}~{re:0.###}s");
            }
            else sb.AppendLine($"[warn] 翻滚 animId {rollId} 未找到 flag8 窗，未改翻滚无敌帧。");

            // 后撤步 a000_027000：flag8 窗（秒）。注：后续帧的 PvE-only(flag143) 不并入（保守只取全无敌段）。
            int backId = CommonAnimationConvention.ComposeId(
                CommonAnimationConvention.RollStance, _convention.backstepBase,
                CommonAnimationConvention.LoadLight, CommonAnimationConvention.DirForward);
            if (TryFlagWindow(backId, Flag_FlagAsDodging, out float bs, out float be))
            {
                _convention.backstepIFrameStartSeconds = bs;
                _convention.backstepIFrameEndSeconds = be;
                sb.AppendLine($"[ok] 后撤步无敌帧 {backId}: flag8 = {bs:0.###}~{be:0.###}s");
            }
            else sb.AppendLine($"[warn] 后撤步 animId {backId} 未找到 flag8 窗，未改后撤步无敌帧。");

            // 喝药饮 a000_050111：type65(ConsumeCurrentGoods) 起点（秒）→ 归一化（÷clip 时长）。
            int drinkId = CommonAnimationConvention.ComposeId(
                CommonAnimationConvention.RollStance, _convention.flaskDrinkBase,
                CommonAnimationConvention.LoadLight, CommonAnimationConvention.DirBackward); // 50110 + dir1 = 50111
            if (TryEventStart(drinkId, Type_ConsumeGoods, out float consumeSec))
            {
                string drinkClip = $"a{drinkId / 1_000_000:000}_{drinkId % 1_000_000:000000}";
                float len = ResolveClipLength(drinkClip);
                if (len > 0f)
                {
                    _convention.flaskConsumeNormalizedTime = Mathf.Clamp01(consumeSec / len);
                    sb.AppendLine($"[ok] 喝药时点 {drinkClip}: type65 起 {consumeSec:0.###}s / clip {len:0.###}s = 归一化 {_convention.flaskConsumeNormalizedTime:0.###}");
                }
                else sb.AppendLine($"[warn] 喝药 {drinkClip} 找不到 clip 算时长（搜索 {_searchFolder}），归一化未改（type65 起 {consumeSec:0.###}s）。");
            }
            else sb.AppendLine($"[warn] 喝药饮 animId {drinkId} 未找到 type65 事件，归一化未改。");

            EditorUtility.SetDirty(_convention);
            AssetDatabase.SaveAssets();

            _report = "[Convention 回填]\n" + sb;
            Debug.Log(_report);
            Selection.activeObject = _convention;
        }

        private void BackfillSpells()
        {
            var sb = new StringBuilder();
            int ok = 0, skip = 0;
            foreach (var spell in _spells)
            {
                if (spell == null) continue;
                if (spell.erCastAnimId <= 0)
                {
                    sb.AppendLine($"[skip] {spell.name}: 未填 erCastAnimId。");
                    skip++;
                    continue;
                }

                if (TryEventStart(spell.erCastAnimId, Type_CastMagic, out float castSec))
                {
                    spell.castReleaseSeconds = castSec;
                    EditorUtility.SetDirty(spell);
                    sb.AppendLine($"[ok] {spell.name}: animId {spell.erCastAnimId} type64 释放 = {castSec:0.###}s");
                    ok++;
                }
                else
                {
                    sb.AppendLine($"[warn] {spell.name}: animId {spell.erCastAnimId} 未找到 type64(CastHighlightedMagic)，未改。");
                    skip++;
                }
            }
            AssetDatabase.SaveAssets();
            _report = $"[Spell 回填] 成功 {ok}，跳过 {skip}\n" + sb;
            Debug.Log(_report);
        }

        // ── helpers ──

        private bool TryFlagWindow(int animId, int flag, out float start, out float end)
        {
            start = 0f; end = 0f;
            var entry = _taeData.GetAnimation(animId);
            if (entry == null) return false;
            return TAEEventQuery.TryGetActionFlagWindow(entry.Value, flag, out start, out end);
        }

        /// <summary>取该 animId 下指定 type 第一个事件的起始时间（秒）。</summary>
        private bool TryEventStart(int animId, int type, out float startSec)
        {
            startSec = 0f;
            var entryN = _taeData.GetAnimation(animId);
            if (entryN == null) return false;
            var entry = entryN.Value;
            if (entry.events == null) return false;

            bool found = false;
            float best = float.PositiveInfinity;
            for (int i = 0; i < entry.events.Length; i++)
            {
                if (entry.events[i].type != type) continue;
                if (entry.events[i].startTime < best) best = entry.events[i].startTime;
                found = true;
            }
            if (found) startSec = Mathf.Max(0f, best);
            return found;
        }

        private float ResolveClipLength(string clipName)
        {
            string[] guids = string.IsNullOrEmpty(_searchFolder)
                ? AssetDatabase.FindAssets(clipName)
                : AssetDatabase.FindAssets(clipName, new[] { _searchFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) || !path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.Equals(Path.GetFileNameWithoutExtension(path), clipName, StringComparison.OrdinalIgnoreCase)) continue;
                foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
                    if (obj is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                        return clip.length;
            }
            return 0f;
        }
    }
}
