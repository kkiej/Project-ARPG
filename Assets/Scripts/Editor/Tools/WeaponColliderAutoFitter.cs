using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LZ.Editor
{
    /// <summary>
    /// 武器命中碰撞体自动拟合器。
    /// <para/>
    /// ER 命中模型（已由 AtkParam_Pc.csv 证实）：每个攻击最多 16 段，
    /// 每段 = <c>hitN_DmyPoly1</c> → <c>hitN_DmyPoly2</c> 两个 dummy 点之间连一根胶囊，半径 = <c>hitN_Radius</c>。
    /// 武器 FBX 里带 dummy 节点，命名如 <c>WP_A_0200 Dummy&lt;5&gt; [300]</c>，方括号内即 ER dummy poly ID。
    /// <c>EquipParamWeapon.traceDmyIdHead0/Tail0</c> 是刀光拖尾（≈刀刃两端），可作单根胶囊的两端。
    /// <para/>
    /// 两种拟合模式：
    /// <list type="bullet">
    /// <item><b>WeaponDummies（推荐）</b>：读 FBX 的 dummy 点，按 head/tail dummy ID 在两点间摆一根定向胶囊。
    ///   head/tail 可手填，或留 -1 由 EquipParamWeapon 按模型号自动取 traceDmyIdHead0/Tail0。</item>
    /// <item><b>MeshBounds（回退）</b>：无 dummy 的武器（如空手 0000）或盾牌，用网格包围盒沿最长轴拟合。</item>
    /// </list>
    /// 判定「时机」不在此处——沿用已导出的 TAE 判定窗（MovesetData.damageWindowStart/End →
    /// PlayerAttackState → PlayerEquipmentManager.OpenDamageCollider → meleeDamageCollider.EnableDamageCollider）。
    /// 半径当前为可调参数；将来可从 AtkParam_Pc 的 hitN_Radius 精确回填。
    /// <para/>
    /// 菜单：Tools → Weapon → Auto-Fit Damage Colliders
    /// </summary>
    public class WeaponColliderAutoFitter : EditorWindow
    {
        private const string ColliderChildName = "Damage Collider";

        private enum FitMode { HitboxData, WeaponDummies, MeshBounds }
        private enum AxisMode { Auto, X, Y, Z }

        // ── 通用 ──
        [SerializeField] private FitMode _fitMode = FitMode.HitboxData;
        [SerializeField] private int _colliderLayer = 10;
        [SerializeField] private bool _selectionOnly = true;
        [SerializeField] private string _prefabFolder = "Assets/Prefabs/Items/Weapons";
        [SerializeField] private string _wrapperOutputFolder = "Assets/Prefabs/Items/Weapons/ER";

        // ── HitboxData 模式（推荐：AtkParam 逐攻击多段胶囊）──
        [SerializeField] private string _hitboxDataFolder = "Assets/Data/WeaponHitboxes";

        // ── WeaponDummies 模式 ──
        [SerializeField] private int _dummyHeadId = -1;   // -1 = 从 EquipParamWeapon 自动取 traceDmyIdHead0
        [SerializeField] private int _dummyTailId = -1;   // -1 = 从 EquipParamWeapon 自动取 traceDmyIdTail0
        [SerializeField] private float _dummyRadius = 0.06f;
        [SerializeField] private string _equipParamPath = "Assets/_ELDENRING_REF/EquipParamWeapon.csv";

        // ── MeshBounds 模式 ──
        [SerializeField] private AxisMode _axisMode = AxisMode.Auto;
        [SerializeField] private float _radiusScale = 1.0f;
        [SerializeField] private float _gripTrimFraction = 0.12f;

        // dummy 节点名：如 "WP_A_0200 Dummy<5> [300]"，取方括号内 ID
        private static readonly Regex DummyRe = new Regex(@"Dummy\s*<\d+>\s*\[(\d+)\]", RegexOptions.Compiled);
        // 模型号：如 "WP_A_0200" → 200
        private static readonly Regex ModelIdRe = new Regex(@"WP_A_0*(\d+)", RegexOptions.Compiled);

        // EquipParamWeapon: equipModelId → (head, tail)。首次用到时构建。
        private Dictionary<int, (int head, int tail)> _traceMap;

        private string _report;
        private Vector2 _scroll;

        [MenuItem("Tools/Weapon/Auto-Fit Damage Colliders")]
        public static void ShowWindow() => GetWindow<WeaponColliderAutoFitter>("Weapon Collider Fitter");

        private void OnGUI()
        {
            GUILayout.Label("武器命中碰撞体自动拟合器", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "HitboxData（推荐）：读 WeaponHitboxData（AtkParam 解析结果），在 dummy 对之间生成多段胶囊，运行期逐攻击精确切换。\n" +
                "WeaponDummies：读 FBX 的 dummy 点(如 [300]/[301])，在 head/tail 两点间摆单根定向胶囊。\n" +
                "MeshBounds：无 dummy 的武器/盾牌，用网格包围盒沿最长轴拟合。\n" +
                "判定时机沿用已导出的 TAE 窗，本工具只负责碰撞体形状与连线。",
                MessageType.Info);

            GUILayout.Space(6);
            _fitMode = (FitMode)EditorGUILayout.EnumPopup("拟合模式", _fitMode);
            _colliderLayer = EditorGUILayout.IntField(new GUIContent("碰撞体 Layer", "与旧武器预制体一致，默认 10。"), _colliderLayer);

            GUILayout.Space(4);
            if (_fitMode == FitMode.HitboxData)
            {
                GUILayout.Label("HitboxData 参数", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("先用 Tools/Weapon/Build Hitbox Data 生成 WeaponHitbox_<模型号>.asset。\n" +
                    "本工具按模型号自动匹配对应资产，在其并集段的 dummy 对之间生成子胶囊。", MessageType.None);
                _hitboxDataFolder = EditorGUILayout.TextField("HitboxData 文件夹", _hitboxDataFolder);
            }
            else if (_fitMode == FitMode.WeaponDummies)
            {
                GUILayout.Label("WeaponDummies 参数", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Head/Tail 填 -1 = 从 EquipParamWeapon 按模型号自动取 traceDmyIdHead0/Tail0（刀光两端）。", MessageType.None);
                _dummyHeadId = EditorGUILayout.IntField(new GUIContent("Head Dummy ID", "-1=自动"), _dummyHeadId);
                _dummyTailId = EditorGUILayout.IntField(new GUIContent("Tail Dummy ID", "-1=自动"), _dummyTailId);
                _dummyRadius = EditorGUILayout.Slider(new GUIContent("胶囊半径(米)", "ER 每段有各自 hitN_Radius；此处为统一近似值，后续可从 AtkParam 精确回填。"), _dummyRadius, 0.01f, 0.3f);
                _equipParamPath = EditorGUILayout.TextField("EquipParamWeapon.csv", _equipParamPath);
            }
            else
            {
                GUILayout.Label("MeshBounds 参数", EditorStyles.boldLabel);
                _axisMode = (AxisMode)EditorGUILayout.EnumPopup(new GUIContent("刀身长轴", "Auto=取包围盒最长的本地轴"), _axisMode);
                _radiusScale = EditorGUILayout.Slider("半径系数", _radiusScale, 0.2f, 2f);
                _gripTrimFraction = EditorGUILayout.Slider(new GUIContent("握把裁剪比例", "从靠近原点(握把)一端裁掉的长度比例"), _gripTrimFraction, 0f, 0.6f);
            }

            GUILayout.Space(10);
            GUILayout.Label("① 处理已有武器预制体", EditorStyles.boldLabel);
            _selectionOnly = EditorGUILayout.Toggle(new GUIContent("只处理选中项", "勾选=处理选中的预制体；不勾=扫描下面文件夹。"), _selectionOnly);
            using (new EditorGUI.DisabledScope(_selectionOnly))
                _prefabFolder = EditorGUILayout.TextField("预制体文件夹", _prefabFolder);
            if (GUILayout.Button("拟合 / 更新碰撞体", GUILayout.Height(28)))
                ProcessPrefabs();

            GUILayout.Space(12);
            GUILayout.Label("② 从选中的 WP FBX 生成武器预制体（含碰撞体）", EditorStyles.boldLabel);
            _wrapperOutputFolder = EditorGUILayout.TextField("输出文件夹", _wrapperOutputFolder);
            if (GUILayout.Button("从选中 FBX 生成武器预制体", GUILayout.Height(28)))
                GenerateWrappersFromSelection();

            if (!string.IsNullOrEmpty(_report))
            {
                GUILayout.Space(8);
                _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(140));
                EditorGUILayout.TextArea(_report, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        // ─────────────────────────────── ① 处理已有预制体 ───────────────────────────────

        private void ProcessPrefabs()
        {
            _traceMap = null; // 每次运行重建缓存，确保读到最新 CSV
            var paths = CollectPrefabPaths();
            if (paths.Count == 0)
            {
                _report = "没有找到可处理的预制体（.prefab）。请选中预制体或填对文件夹。";
                return;
            }

            var sb = new StringBuilder();
            int ok = 0, skip = 0;
            foreach (string path in paths)
            {
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if (BuildColliderOnRoot(root, out string msg))
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                        ok++;
                        sb.AppendLine($"[OK] {Path.GetFileName(path)} — {msg}");
                    }
                    else { skip++; sb.AppendLine($"[跳过] {Path.GetFileName(path)} — {msg}"); }
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _report = $"处理完成：成功 {ok}，跳过 {skip}\n\n" + sb;
            Debug.Log(_report);
        }

        private List<string> CollectPrefabPaths()
        {
            var result = new List<string>();
            if (_selectionOnly)
            {
                foreach (Object obj in Selection.objects)
                {
                    string p = AssetDatabase.GetAssetPath(obj);
                    if (!string.IsNullOrEmpty(p) && p.EndsWith(".prefab")) result.Add(p);
                }
            }
            else
            {
                if (!AssetDatabase.IsValidFolder(_prefabFolder)) return result;
                foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { _prefabFolder }))
                    result.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            return result;
        }

        // ─────────────────────────────── ② 从 FBX 生成包装预制体 ───────────────────────────────

        private void GenerateWrappersFromSelection()
        {
            _traceMap = null;
            var models = new List<GameObject>();
            foreach (Object obj in Selection.objects)
            {
                string p = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(p) && (p.EndsWith(".fbx") || p.EndsWith(".FBX")))
                {
                    GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                    if (go != null) models.Add(go);
                }
            }

            if (models.Count == 0)
            {
                _report = "没有选中任何 FBX 模型。请在 Project 里选中 WP_A_xxxx.fbx。";
                return;
            }

            if (!AssetDatabase.IsValidFolder(_wrapperOutputFolder))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), _wrapperOutputFolder));
                AssetDatabase.Refresh();
            }

            var sb = new StringBuilder();
            int ok = 0, skip = 0;
            foreach (GameObject fbx in models)
            {
                string outPath = $"{_wrapperOutputFolder}/{fbx.name}.prefab";
                var root = new GameObject(fbx.name);
                try
                {
                    var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
                    modelInstance.transform.SetParent(root.transform, false);
                    modelInstance.transform.localPosition = Vector3.zero;
                    modelInstance.transform.localRotation = Quaternion.identity;
                    modelInstance.transform.localScale = Vector3.one;

                    if (BuildColliderOnRoot(root, out string msg))
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, outPath);
                        ok++;
                        sb.AppendLine($"[OK] {fbx.name}.prefab — {msg}");
                    }
                    else { skip++; sb.AppendLine($"[跳过] {fbx.name} — {msg}"); }
                }
                finally { DestroyImmediate(root); }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _report = $"生成完成：成功 {ok}，跳过 {skip}\n输出目录：{_wrapperOutputFolder}\n\n" + sb;
            Debug.Log(_report);
        }

        // ─────────────────────────────── 核心：在 root 上建/更新碰撞体 ───────────────────────────────

        private bool BuildColliderOnRoot(GameObject root, out string message)
        {
            var wm = root.GetComponent<WeaponManager>();
            if (wm == null) wm = root.AddComponent<WeaponManager>();

            Transform dcTf = root.transform.Find(ColliderChildName);
            GameObject dcGo = dcTf != null ? dcTf.gameObject : null;
            if (dcGo == null)
            {
                dcGo = new GameObject(ColliderChildName);
                dcGo.transform.SetParent(root.transform, false);
            }
            dcGo.layer = _colliderLayer;

            if (_fitMode == FitMode.HitboxData)
                return BuildHitboxSegments(root, dcGo, wm, out message);

            // 单碰撞体路径：清掉本物体上的旧碰撞体，以及可能残留的多段 Seg_* 子物体。
            foreach (Collider old in dcGo.GetComponents<Collider>())
                DestroyImmediate(old);
            ClearSegmentChildren(dcGo);

            message = "";
            bool built = false;
            string dummyMsg = null;
            if (_fitMode == FitMode.WeaponDummies)
            {
                built = TryBuildFromDummies(root, dcGo, out dummyMsg);
                message = dummyMsg;
            }

            //  WeaponDummies 失败（无 dummy / 未解析到 head-tail）时回退到网格包围盒
            if (!built)
            {
                built = BuildFromMeshBounds(root, dcGo, out string boundsMsg);
                message = string.IsNullOrEmpty(dummyMsg) ? boundsMsg : $"{dummyMsg}；已回退 MeshBounds：{boundsMsg}";
            }

            if (!built) return false;

            var cap = dcGo.GetComponent<CapsuleCollider>();
            if (cap != null) { cap.isTrigger = true; cap.enabled = false; }

            var mwdc = dcGo.GetComponent<MeleeWeaponDamageCollider>();
            if (mwdc == null) mwdc = dcGo.AddComponent<MeleeWeaponDamageCollider>();
            mwdc.damageCollider = dcGo.GetComponent<Collider>();
            mwdc.hitboxData = null;                                    // 单碰撞体模式：清掉多段引用
            mwdc.segmentColliders = System.Array.Empty<WeaponHitboxSegmentCollider>();
            wm.meleeDamageCollider = mwdc;
            return true;
        }

        // ─────────────────────────────── HitboxData：多段胶囊 ───────────────────────────────

        // 按 WeaponHitboxData 的并集段，在武器 FBX 的 dummy 对之间各生成一根定向子胶囊。
        // 每段挂 WeaponHitboxSegmentCollider，运行期由 MeleeWeaponDamageCollider 按当前攻击选择性启用并按 hitN_Radius 定半径。
        private bool BuildHitboxSegments(GameObject root, GameObject dcGo, WeaponManager wm, out string message)
        {
            int modelId = ParseModelId(root.name);
            if (modelId < 0) { message = $"无法从名字解析模型号：{root.name}"; return false; }

            WeaponHitboxData data = LoadHitboxData(modelId);
            if (data == null) { message = $"找不到 WeaponHitbox_{modelId}.asset（先跑 Tools/Weapon/Build Hitbox Data）"; return false; }
            if (data.unionSegments == null || data.unionSegments.Length == 0) { message = $"WeaponHitbox_{modelId} 没有并集段"; return false; }

            var dummies = CollectDummies(root.transform, dcGo.transform);
            if (dummies.Count == 0) { message = "FBX 里没有 dummy 节点"; return false; }

            // 复位 dcGo 到 identity：子物体 localPos/Rot 即等于 root 本地坐标
            dcGo.transform.localPosition = Vector3.zero;
            dcGo.transform.localRotation = Quaternion.identity;
            dcGo.transform.localScale = Vector3.one;

            foreach (Collider old in dcGo.GetComponents<Collider>())
                DestroyImmediate(old);
            ClearSegmentChildren(dcGo);

            var mwdc = dcGo.GetComponent<MeleeWeaponDamageCollider>();
            if (mwdc == null) mwdc = dcGo.AddComponent<MeleeWeaponDamageCollider>();
            mwdc.damageCollider = null;     // 多段模式：命中框在子物体上，本物体不放碰撞体
            mwdc.hitboxData = data;

            var built = new List<WeaponHitboxSegmentCollider>();
            var missing = new HashSet<int>();
            foreach (var seg in data.unionSegments)
            {
                if (!dummies.TryGetValue(seg.dmyA, out Transform tA)) { missing.Add(seg.dmyA); continue; }
                Vector3 p0 = root.transform.InverseTransformPoint(tA.position);

                Vector3 p1; float baseDist;
                if (seg.IsSphere) { p1 = p0; baseDist = 0f; }
                else
                {
                    if (!dummies.TryGetValue(seg.dmyB, out Transform tB)) { missing.Add(seg.dmyB); continue; }
                    p1 = root.transform.InverseTransformPoint(tB.position);
                    baseDist = Vector3.Distance(p0, p1);
                }

                var segGo = new GameObject(seg.IsSphere ? $"Seg_{seg.dmyA}" : $"Seg_{seg.dmyA}_{seg.dmyB}");
                segGo.transform.SetParent(dcGo.transform, false);
                segGo.layer = _colliderLayer;
                segGo.transform.localPosition = (p0 + p1) * 0.5f;
                segGo.transform.localRotation = baseDist > 1e-5f
                    ? Quaternion.FromToRotation(Vector3.up, (p1 - p0).normalized)
                    : Quaternion.identity;
                segGo.transform.localScale = Vector3.one;

                var cap = segGo.AddComponent<CapsuleCollider>();
                cap.direction = 1; // 本地 Y 已对齐 dmyA→dmyB
                cap.center = Vector3.zero;
                cap.radius = seg.radius;
                cap.height = baseDist + 2f * seg.radius;
                cap.isTrigger = true;
                cap.enabled = false;

                var sc = segGo.AddComponent<WeaponHitboxSegmentCollider>();
                sc.dmyA = seg.dmyA;
                sc.dmyB = seg.dmyB;
                sc.baseDistance = baseDist;
                sc.defaultRadius = seg.radius;
                sc.owner = mwdc;
                built.Add(sc);
            }

            if (built.Count == 0)
            {
                message = $"并集段的 dummy 在 FBX 里都找不到（缺 {string.Join(",", missing)}；现有 {string.Join(",", dummies.Keys)}）";
                return false;
            }

            mwdc.segmentColliders = built.ToArray();
            wm.meleeDamageCollider = mwdc;

            string miss = missing.Count > 0 ? $"；缺 dummy {string.Join(",", missing)}" : "";
            message = $"模型 {modelId} varId={data.behaviorVariationId}：生成 {built.Count} 段胶囊{miss}";
            return true;
        }

        private WeaponHitboxData LoadHitboxData(int modelId)
        {
            string direct = $"{_hitboxDataFolder}/WeaponHitbox_{modelId}.asset";
            var so = AssetDatabase.LoadAssetAtPath<WeaponHitboxData>(direct);
            if (so != null) return so;

            foreach (string guid in AssetDatabase.FindAssets("t:WeaponHitboxData"))
            {
                var d = AssetDatabase.LoadAssetAtPath<WeaponHitboxData>(AssetDatabase.GUIDToAssetPath(guid));
                if (d != null && d.weaponModelId == modelId) return d;
            }
            return null;
        }

        private static void ClearSegmentChildren(GameObject dcGo)
        {
            for (int i = dcGo.transform.childCount - 1; i >= 0; i--)
            {
                Transform c = dcGo.transform.GetChild(i);
                if (c.name.StartsWith("Seg_") || c.GetComponent<WeaponHitboxSegmentCollider>() != null)
                    DestroyImmediate(c.gameObject);
            }
        }

        // ── 按 dummy 点在 head↔tail 间摆一根定向胶囊 ──
        private bool TryBuildFromDummies(GameObject root, GameObject dcGo, out string message)
        {
            var dummies = CollectDummies(root.transform, dcGo.transform);
            if (dummies.Count == 0) { message = "未找到 dummy 节点"; return false; }

            int head = _dummyHeadId, tail = _dummyTailId;
            if (head < 0 || tail < 0)
            {
                int modelId = ParseModelId(root.name);
                if (modelId >= 0 && TryGetTrace(modelId, out int h, out int t))
                {
                    if (head < 0) head = h;
                    if (tail < 0) tail = t;
                }
            }
            if (head < 0 || tail < 0) { message = "未解析到 head/tail dummy（可手填或检查 EquipParamWeapon）"; return false; }
            if (!dummies.TryGetValue(head, out Transform hT) || !dummies.TryGetValue(tail, out Transform tT))
            {
                message = $"dummy [{head}] 或 [{tail}] 在模型里不存在（现有：{string.Join(",", dummies.Keys)}）";
                return false;
            }

            //  转到 root 本地空间（dcGo 是 root 直子，localPos/Rot 即 root 空间量）
            Vector3 p0 = root.transform.InverseTransformPoint(hT.position);
            Vector3 p1 = root.transform.InverseTransformPoint(tT.position);
            Vector3 dir = p1 - p0;
            float dist = dir.magnitude;
            if (dist < 1e-4f) { message = $"dummy [{head}] 与 [{tail}] 重合，无法构成胶囊"; return false; }

            dcGo.transform.localPosition = (p0 + p1) * 0.5f;
            dcGo.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir.normalized);
            dcGo.transform.localScale = Vector3.one;

            var cap = dcGo.AddComponent<CapsuleCollider>();
            cap.direction = 1; // 本地 Y 已对齐 head→tail
            cap.center = Vector3.zero;
            cap.radius = _dummyRadius;
            cap.height = dist + 2f * _dummyRadius; // 端帽落在两 dummy 上

            message = $"dummy [{head}]→[{tail}] 长度={dist:F3} 半径={_dummyRadius:F3}";
            return true;
        }

        // ── 网格包围盒回退 ──
        private bool BuildFromMeshBounds(GameObject root, GameObject dcGo, out string message)
        {
            if (!TryComputeLocalBounds(root.transform, dcGo.transform, out Bounds bounds))
            {
                message = "找不到可用网格（MeshFilter/SkinnedMeshRenderer）";
                return false;
            }

            dcGo.transform.localPosition = Vector3.zero;
            dcGo.transform.localRotation = Quaternion.identity;
            dcGo.transform.localScale = Vector3.one;

            int axis = ChooseAxis(bounds.size);
            ComputeCapsuleFromBounds(bounds, axis, out Vector3 center, out float height, out float radius);

            var cap = dcGo.AddComponent<CapsuleCollider>();
            cap.direction = axis;
            cap.center = center;
            cap.radius = radius;
            cap.height = Mathf.Max(height, radius * 2f);

            string axisName = axis == 0 ? "X" : axis == 1 ? "Y" : "Z";
            message = $"轴={axisName} 长度={height:F3} 半径={radius:F3}";
            return true;
        }

        // ─────────────────────────────── 辅助 ───────────────────────────────

        private static Dictionary<int, Transform> CollectDummies(Transform root, Transform exclude)
        {
            var map = new Dictionary<int, Transform>();
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (exclude != null && t.IsChildOf(exclude)) continue;
                Match m = DummyRe.Match(t.name);
                if (m.Success && int.TryParse(m.Groups[1].Value, out int id))
                    map[id] = t; // 同 ID 以第一个为准
            }
            return map;
        }

        private static int ParseModelId(string name)
        {
            Match m = ModelIdRe.Match(name);
            return m.Success && int.TryParse(m.Groups[1].Value, out int id) ? id : -1;
        }

        private bool TryGetTrace(int modelId, out int head, out int tail)
        {
            if (_traceMap == null) BuildTraceMap();
            if (_traceMap != null && _traceMap.TryGetValue(modelId, out var v))
            {
                head = v.head; tail = v.tail;
                return head >= 0 && tail >= 0;
            }
            head = tail = -1;
            return false;
        }

        private void BuildTraceMap()
        {
            _traceMap = new Dictionary<int, (int, int)>();
            string abs = Path.IsPathRooted(_equipParamPath)
                ? _equipParamPath
                : Path.Combine(Directory.GetCurrentDirectory(), _equipParamPath);
            if (!File.Exists(abs))
            {
                Debug.LogWarning($"[WeaponColliderAutoFitter] 找不到 EquipParamWeapon.csv：{abs}（Head/Tail 自动取值将不可用，可手填）。");
                return;
            }

            using var reader = new StreamReader(abs);
            string header = reader.ReadLine();
            if (header == null) return;
            var cols = header.Split(',');
            int iModel = System.Array.IndexOf(cols, "equipModelId");
            int iHead = System.Array.IndexOf(cols, "traceDmyIdHead0");
            int iTail = System.Array.IndexOf(cols, "traceDmyIdTail0");
            if (iModel < 0 || iHead < 0 || iTail < 0)
            {
                Debug.LogWarning("[WeaponColliderAutoFitter] EquipParamWeapon.csv 缺少 equipModelId/traceDmyIdHead0/traceDmyIdTail0 列。");
                return;
            }

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var f = line.Split(',');
                if (f.Length <= iTail) continue;
                if (!int.TryParse(f[iModel], NumberStyles.Integer, CultureInfo.InvariantCulture, out int model)) continue;
                if (_traceMap.ContainsKey(model)) continue; // 同模型多条，取第一条
                int.TryParse(f[iHead], NumberStyles.Integer, CultureInfo.InvariantCulture, out int h);
                int.TryParse(f[iTail], NumberStyles.Integer, CultureInfo.InvariantCulture, out int t);
                _traceMap[model] = (h, t);
            }
        }

        private static bool TryComputeLocalBounds(Transform root, Transform exclude, out Bounds bounds)
        {
            var acc = new Bounds();
            bool has = false;

            void Accumulate(Mesh mesh, Transform t)
            {
                if (mesh == null) return;
                Bounds mb = mesh.bounds;
                Vector3 c = mb.center, e = mb.extents;
                for (int i = 0; i < 8; i++)
                {
                    var corner = new Vector3(
                        c.x + (((i & 1) == 0) ? -e.x : e.x),
                        c.y + (((i & 2) == 0) ? -e.y : e.y),
                        c.z + (((i & 4) == 0) ? -e.z : e.z));
                    Vector3 local = root.InverseTransformPoint(t.TransformPoint(corner));
                    if (!has) { acc = new Bounds(local, Vector3.zero); has = true; }
                    else acc.Encapsulate(local);
                }
            }

            foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (exclude != null && mf.transform.IsChildOf(exclude)) continue;
                Accumulate(mf.sharedMesh, mf.transform);
            }
            foreach (var smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (exclude != null && smr.transform.IsChildOf(exclude)) continue;
                Accumulate(smr.sharedMesh, smr.transform);
            }

            bounds = acc;
            return has;
        }

        private int ChooseAxis(Vector3 size)
        {
            switch (_axisMode)
            {
                case AxisMode.X: return 0;
                case AxisMode.Y: return 1;
                case AxisMode.Z: return 2;
                default:
                    if (size.x >= size.y && size.x >= size.z) return 0;
                    if (size.y >= size.x && size.y >= size.z) return 1;
                    return 2;
            }
        }

        private void ComputeCapsuleFromBounds(Bounds b, int axis, out Vector3 center, out float height, out float radius)
        {
            float lo = b.min[axis], hi = b.max[axis], len = hi - lo;
            float trim = _gripTrimFraction * len;
            if (Mathf.Abs(lo) <= Mathf.Abs(hi)) lo += trim; else hi -= trim;
            height = Mathf.Max(hi - lo, 0.001f);

            int a1 = (axis + 1) % 3, a2 = (axis + 2) % 3;
            float half = 0.5f * Mathf.Max(b.size[a1], b.size[a2]);
            radius = Mathf.Max(half * _radiusScale, 0.005f);

            center = b.center;
            Vector3 c = center; c[axis] = (lo + hi) * 0.5f; center = c;
        }
    }
}
