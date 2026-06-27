using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LZ.Editor
{
    /// <summary>
    /// ER locomotion 动画导入参数统一器：按 ER 后缀(aXXX_YYYYYY 的 YYYYYY)分类，给「移动循环类」clip 套一套统一规则。
    /// <para/>
    /// 规则前提：本项目 locomotion 是<b>代码驱动</b>（KCC 速度来自输入），<see cref="CharacterAnimatorManager"/> 的
    /// OnAnimatorMove 只在 applyRootMotion=true（翻滚/攻击）时消费根运动。所以 locomotion 的根运动应当被
    /// <b>提取出来再丢弃</b>（= Bake Into Pose 全<b>关</b>），让身体原地循环、不滑步；这同时也兼容将来若改成
    /// 根运动驱动的方案（届时只需开始消费 delta，无需再改导入）。
    /// <list type="bullet">
    /// <item><b>循环移动</b>（idle / 走 / 慢跑 / 奔跑 / 蹲走）：Loop Time + Loop Pose 开；三轴 Bake Into Pose 全关。</item>
    /// <item><b>刹停</b>（22xxx，一次性减速）：Loop 关；三轴 Bake Into Pose 全关。</item>
    /// <item><b>其余</b>（攻击 / 翻滚 / 跳跃 / 喝药 / emote…）：<b>不碰</b>，保留各自的根运动与设置。</item>
    /// </list>
    /// 菜单：Tools → Moveset → Locomotion Import Rules
    /// </summary>
    public class LocomotionImportRuleApplier : EditorWindow
    {
        private enum Kind { Skip, LoopLocomotion, OneShotStop }

        [SerializeField] private string _searchFolder = "Assets/_ELDENRING_REF/Animations";
        [Tooltip("同时把 Based Upon 统一成 Original（最利于 loop match 变绿、各 clip 摆位一致）。")]
        [SerializeField] private bool _normalizeBasedUpon = true;
        [Tooltip("也处理刹停 22xxx（设为非循环、原地）。关掉则刹停也跳过。")]
        [SerializeField] private bool _includeStops = true;
        [Tooltip("只预览不写盘。先勾着看报告，确认无误再取消勾选执行。")]
        [SerializeField] private bool _dryRun = true;

        private Vector2 _scroll;
        private string _report;

        [MenuItem("Tools/Moveset/Locomotion Import Rules")]
        public static void ShowWindow() => GetWindow<LocomotionImportRuleApplier>("Locomotion Import Rules");

        private void OnGUI()
        {
            GUILayout.Label("Locomotion 导入参数统一器", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "按 ER 后缀分类，只给「循环移动 / 刹停」clip 套统一规则：\n" +
                "· 循环移动：Loop Time + Loop Pose 开，三轴 Bake Into Pose 全关（提取并丢弃根运动＝原地循环不滑步）\n" +
                "· 刹停 22xxx：Loop 关，三轴 Bake 全关\n" +
                "攻击/翻滚/跳跃/喝药等一律不碰（保留其根运动）。",
                MessageType.Info);
            GUILayout.Space(6);

            _searchFolder = EditorGUILayout.TextField("搜索文件夹", _searchFolder);
            _normalizeBasedUpon = EditorGUILayout.Toggle("统一 Based Upon = Original", _normalizeBasedUpon);
            _includeStops = EditorGUILayout.Toggle("处理刹停 22xxx", _includeStops);
            _dryRun = EditorGUILayout.Toggle("Dry Run（仅预览）", _dryRun);

            GUILayout.Space(8);
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_searchFolder)))
            {
                GUI.backgroundColor = _dryRun ? Color.white : new Color(1f, 0.7f, 0.7f);
                if (GUILayout.Button(_dryRun ? "预览将改动的 clip" : "执行（写入导入设置并重导入）", GUILayout.Height(30)))
                    Run();
                GUI.backgroundColor = Color.white;
            }

            if (!string.IsNullOrEmpty(_report))
            {
                GUILayout.Space(8);
                _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(240));
                EditorGUILayout.TextArea(_report, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        private void Run()
        {
            var sb = new StringBuilder();
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { _searchFolder });
            int loopCount = 0, stopCount = 0, skipped = 0, changed = 0;

            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (!path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;

                    string fileName = Path.GetFileNameWithoutExtension(path);
                    Kind kind = Classify(fileName);
                    if (kind == Kind.Skip) { skipped++; continue; }
                    if (kind == Kind.OneShotStop && !_includeStops) { skipped++; continue; }

                    if (kind == Kind.LoopLocomotion) loopCount++; else stopCount++;

                    if (EditorUtility.DisplayCancelableProgressBar("Locomotion Import Rules", fileName, (float)i / guids.Length))
                        break;

                    if (ApplyTo(path, kind, sb))
                        changed++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (!_dryRun)
                AssetDatabase.Refresh();

            sb.Insert(0, $"[{(_dryRun ? "预览" : "已执行")}] 循环移动 {loopCount}，刹停 {stopCount}，跳过 {skipped}，" +
                         $"{(_dryRun ? "待改动" : "实改动")} {changed} 个 fbx。\n\n");
            _report = sb.ToString();
            Debug.Log(_report);
        }

        /// <summary>按 ER 后缀(YYYYYY)分类。仅认识循环移动与刹停，其余一律 Skip。</summary>
        private static Kind Classify(string fileName)
        {
            // 形如 aXXX_YYYYYY
            int us = fileName.IndexOf('_');
            if (us <= 0 || us + 1 >= fileName.Length) return Kind.Skip;
            if (fileName[0] != 'a' && fileName[0] != 'A') return Kind.Skip;
            if (!int.TryParse(fileName.Substring(us + 1), out int s)) return Kind.Skip;

            // idle 0；走 20000-20099 / 慢跑 20100-20199 / 奔跑 20200-20299；蹲走 21000-21099 → 循环
            if (s == 0) return Kind.LoopLocomotion;
            if (s >= 20000 && s <= 20299) return Kind.LoopLocomotion;
            if (s >= 21000 && s <= 21099) return Kind.LoopLocomotion;
            // 刹停 22000-22299（快走/奔跑刹停）→ 一次性
            if (s >= 22000 && s <= 22299) return Kind.OneShotStop;

            // 27xxx 翻滚/后撤步、202xxx 跳跃、其余动作 → 不碰（需要保留根运动）
            return Kind.Skip;
        }

        private bool ApplyTo(string path, Kind kind, StringBuilder sb)
        {
            var mi = AssetImporter.GetAtPath(path) as ModelImporter;
            if (mi == null) return false;

            var clips = mi.clipAnimations;
            bool fromDefault = clips == null || clips.Length == 0;
            if (fromDefault) clips = mi.defaultClipAnimations;
            if (clips == null || clips.Length == 0) return false;

            bool isLoop = kind == Kind.LoopLocomotion;
            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].loopTime = isLoop;
                clips[i].loopPose = isLoop;
                clips[i].cycleOffset = 0f;

                // 三轴 Bake Into Pose 全关 = 根运动被提取（身体留原地）。
                clips[i].lockRootRotation = false;
                clips[i].lockRootHeightY = false;
                clips[i].lockRootPositionXZ = false;

                if (_normalizeBasedUpon)
                {
                    clips[i].keepOriginalOrientation = true; // Rotation Based Upon = Original
                    clips[i].keepOriginalPositionY = true;   // Y Based Upon = Original
                    clips[i].heightFromFeet = false;
                    clips[i].keepOriginalPositionXZ = true;  // XZ Based Upon = Original
                }
            }

            sb.AppendLine($"  [{(isLoop ? "loop" : "stop")}] {Path.GetFileNameWithoutExtension(path)}" +
                          $"{(fromDefault ? " (默认clip→用户clip)" : "")}");

            if (_dryRun) return true;

            mi.clipAnimations = clips;
            EditorUtility.SetDirty(mi);
            mi.SaveAndReimport();
            return true;
        }
    }
}
