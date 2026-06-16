using System.Collections.Generic;
using LZ.ModularRig;
using UnityEditor;
using UnityEngine;

namespace LZ.EditorTools
{
    /// <summary>
    /// 模块化角色拼装面板。把 Skeleton + 各部件 FBX 拖入槽位,点 Build 即可在场景里生成角色。
    /// 菜单: Tools / Modular Rig / Character Builder
    /// </summary>
    public class ModularCharacterBuilderWindow : EditorWindow
    {
        // ---- 输出设置 ----
        string characterName = "TestCharacter";
        Transform parent;
        bool savePrefab = false;
        string prefabFolder = "Assets/_ELDENRING_REF/CharacterPrefabs";
        bool rebakeBindposes = false;  // 默认关: c0000 系所有部件与骨架同名同尺度, bindpose 本就通用, 不该改. 仅当源/骨架 import scale 真的不一致时才开.
        bool resetBounds = false;      // 默认关: c0000 与部件同尺度, 部件导入的 localBounds 已正确紧贴模型, 重算反而会按编辑态姿势/换 rootBone 把盒子算歪飞出去. 仅当源/骨架尺度不一致导致盒子明显不对时才开.
        float boundsExpand = 0.5f;     // 在 ResetBounds 结果上额外扩张多少米, 给大动作 (翻滚/披风/武器拖尾) 留余量

        // ---- 必选 ----
        GameObject skeletonFbx;

        // ---- 身体部件 ----
        GameObject headFbx;     // HD_*
        GameObject faceFbx;     // FC_*
        GameObject hairFbx;     // HR_*
        GameObject bodyFbx;     // BD_*
        GameObject armsFbx;     // AM_*
        GameObject legsFbx;     // LG_*

        // ---- 武器 ----
        GameObject rightHandWeapon;     // 挂 R_Weapon
        GameObject leftHandWeapon;      // 挂 L_Weapon
        GameObject leftHandShield;      // 挂 L_Weapon (Shield 子类)
        GameObject backWeapon;          // 挂 SpineFront 或背部 dummy

        // ---- 骨骼名(法环 c0000 默认值,可改) ----
        string rightHandWeaponBone = "R_Weapon";
        string leftHandWeaponBone = "L_Weapon";
        string backWeaponBone = "SpineFront";

        Vector2 scroll;

        [MenuItem("Tools/Modular Rig/Character Builder")]
        public static void Open()
        {
            var win = GetWindow<ModularCharacterBuilderWindow>("Modular Rig Builder");
            win.minSize = new Vector2(420, 600);
            win.Show();
        }

        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.LabelField("Modular Character Builder", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "拖入 Skeleton.fbx 和各部件 FBX, 点 Build 生成测试角色.\n" +
                "部件 FBX 应已在 Import Settings 中将 Avatar 设为 Copy From Skeleton.",
                MessageType.Info);

            // === Output ===
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            characterName = EditorGUILayout.TextField("Character Name", characterName);
            parent = (Transform)EditorGUILayout.ObjectField("Parent (optional)", parent, typeof(Transform), true);
            savePrefab = EditorGUILayout.Toggle("Save Prefab Variant", savePrefab);
            if (savePrefab)
                prefabFolder = EditorGUILayout.TextField("Prefab Folder", prefabFolder);
            rebakeBindposes = EditorGUILayout.Toggle(
                new GUIContent("Rebake Bindposes",
                    "源 FBX 与 Skeleton 的 Import Scale 不一致时自动重算 bindpose. 会克隆 Mesh."),
                rebakeBindposes);
            resetBounds = EditorGUILayout.Toggle(
                new GUIContent("Reset Bounds After Build",
                    "默认关. 同尺度时部件导入的 localBounds 已正确, 不需重算. 仅当源/骨架尺度不一致、盒子明显不贴模型时才开."),
                resetBounds);
            if (resetBounds)
            {
                EditorGUI.indentLevel++;
                boundsExpand = EditorGUILayout.FloatField(
                    new GUIContent("Bounds Expand (m)",
                        "ResetBounds 算的是当前姿态的包围盒, 这里额外扩张给大动作留余量. 0 = 不扩张."),
                    boundsExpand);
                EditorGUI.indentLevel--;
            }

            // === Skeleton ===
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Skeleton (required)", EditorStyles.boldLabel);
            skeletonFbx = (GameObject)EditorGUILayout.ObjectField("Skeleton FBX", skeletonFbx, typeof(GameObject), false);

            // === Body Parts ===
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Body Parts", EditorStyles.boldLabel);
            headFbx = DrawPartSlot("Head (HD_*)", headFbx, "HD");
            faceFbx = DrawPartSlot("Face (FC_*)", faceFbx, "FC");
            hairFbx = DrawPartSlot("Hair (HR_*)", hairFbx, "HR");
            bodyFbx = DrawPartSlot("Body (BD_*)", bodyFbx, "BD");
            armsFbx = DrawPartSlot("Arms (AM_*)", armsFbx, "AM");
            legsFbx = DrawPartSlot("Legs (LG_*)", legsFbx, "LG");

            // === Weapons ===
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Weapons", EditorStyles.boldLabel);
            DrawWeaponSlot("Right Hand", ref rightHandWeapon, ref rightHandWeaponBone);
            DrawWeaponSlot("Left Hand",  ref leftHandWeapon,  ref leftHandWeaponBone);
            DrawWeaponSlot("Left Shield", ref leftHandShield, ref leftHandWeaponBone);
            DrawWeaponSlot("Back",       ref backWeapon,      ref backWeaponBone);

            // === Buttons ===
            EditorGUILayout.Space(12);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(skeletonFbx == null))
                {
                    if (GUILayout.Button("Build", GUILayout.Height(32)))
                        Build();
                }
                if (GUILayout.Button("Clear", GUILayout.Width(80), GUILayout.Height(32)))
                    Clear();
            }

            if (skeletonFbx == null)
                EditorGUILayout.HelpBox("Skeleton FBX 必填.", MessageType.Warning);

            // === Validation tips ===
            var warns = Validate();
            if (warns.Count > 0)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
                foreach (var w in warns) EditorGUILayout.HelpBox(w, MessageType.Warning);
            }

            EditorGUILayout.EndScrollView();
        }

        // ===== 槽位绘制 =====
        GameObject DrawPartSlot(string label, GameObject current, string expectedPrefix)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var next = (GameObject)EditorGUILayout.ObjectField(label, current, typeof(GameObject), false);
                if (next != null && !string.IsNullOrEmpty(expectedPrefix))
                {
                    if (!next.name.StartsWith(expectedPrefix + "_", System.StringComparison.OrdinalIgnoreCase))
                        GUILayout.Label(new GUIContent("!", $"命名不像 {expectedPrefix}_, 请确认是否拖对了"),
                            GUILayout.Width(16));
                }
                return next;
            }
        }

        void DrawWeaponSlot(string label, ref GameObject weapon, ref string boneName)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                weapon = (GameObject)EditorGUILayout.ObjectField(label, weapon, typeof(GameObject), false);
                boneName = EditorGUILayout.TextField(boneName, GUILayout.Width(120));
            }
        }

        // ===== 校验提示 =====
        List<string> Validate()
        {
            var list = new List<string>();
            if (skeletonFbx != null && !skeletonFbx.name.Equals("Skeleton", System.StringComparison.OrdinalIgnoreCase))
                list.Add($"Skeleton 槽里拖的不是名为 'Skeleton' 的 FBX (当前: {skeletonFbx.name}). 可能不是基础骨架.");

            // gender consistency
            string gender = null;
            foreach (var p in new[] { headFbx, faceFbx, bodyFbx, armsFbx, legsFbx })
            {
                if (p == null) continue;
                if (p.name.Length >= 4 && p.name[2] == '_')
                {
                    var g = p.name[3].ToString();
                    if (g == "M" || g == "F")
                    {
                        if (gender == null) gender = g;
                        else if (gender != g)
                        {
                            list.Add("部件性别不一致(混了 M 和 F), 蒙皮可能错位.");
                            break;
                        }
                    }
                }
            }
            return list;
        }

        void Clear()
        {
            headFbx = faceFbx = hairFbx = bodyFbx = armsFbx = legsFbx = null;
            rightHandWeapon = leftHandWeapon = leftHandShield = backWeapon = null;
        }

        // ===== 真正的拼装逻辑 =====
        void Build()
        {
            if (skeletonFbx == null) return;

            var characterRoot = new GameObject(characterName);
            Undo.RegisterCreatedObjectUndo(characterRoot, "Build Modular Character");
            if (parent != null) characterRoot.transform.SetParent(parent, false);

            // 1. Skeleton (unpack 解除 prefab 连接, 方便后续挂武器/改层级)
            var skeletonInst = InstantiateUnpacked(skeletonFbx);
            skeletonInst.name = "Armature";
            skeletonInst.transform.SetParent(characterRoot.transform, false);

            // 用"整副骨架实例"作为骨骼映射 / 查找的根, 而不是猜一个名为 Master/Root 的子节点.
            // c0000 这类 FromSoft 骨架带 Model_Dmy_Storage 等附属层级, 真正的形变骨不一定全挂在被猜中的子树下;
            // 若只从子树建名字表, 会漏骨 -> 按名重映射整体失败 -> 蒙皮塌掉.
            var skeletonRoot = skeletonInst.transform;
            if (skeletonRoot.childCount == 0)
            {
                Debug.LogError("[Builder] Skeleton 没有骨骼层级.");
                Object.DestroyImmediate(characterRoot);
                return;
            }

            // 2. Meshes 容器
            var meshes = new GameObject("Meshes");
            meshes.transform.SetParent(characterRoot.transform, false);

            // 3. 拼身体部件(每个一个分组节点便于卸装备)
            AttachPart(headFbx, skeletonRoot, meshes.transform, rebakeBindposes);
            AttachPart(faceFbx, skeletonRoot, meshes.transform, rebakeBindposes);
            AttachPart(hairFbx, skeletonRoot, meshes.transform, rebakeBindposes);
            AttachPart(bodyFbx, skeletonRoot, meshes.transform, rebakeBindposes);
            AttachPart(armsFbx, skeletonRoot, meshes.transform, rebakeBindposes);
            AttachPart(legsFbx, skeletonRoot, meshes.transform, rebakeBindposes);

            // 4. 武器(rigid mesh,直接 reparent 到对应骨骼)
            MountWeapon(rightHandWeapon, skeletonRoot, rightHandWeaponBone);
            MountWeapon(leftHandWeapon,  skeletonRoot, leftHandWeaponBone);
            MountWeapon(leftHandShield,  skeletonRoot, leftHandWeaponBone);
            MountWeapon(backWeapon,      skeletonRoot, backWeaponBone);

            // 5. Animator + Avatar (放在 Armature 上)
            var animator = skeletonInst.AddComponent<Animator>();
            animator.avatar = LoadAvatarFromFbx(skeletonFbx);
            if (animator.avatar == null)
                Debug.LogWarning("[Builder] Skeleton.fbx 里没找到 Avatar. 请确认 Rig=Generic 且 Avatar Definition=Create From This Model.");

            // 6. 重算所有 SMR 的 bounds (重绑 bones[] 后 localBounds 还是装备小骨架那套, 要刷新)
            if (resetBounds)
                RefreshAllBounds(characterRoot, boundsExpand);

            // 7. 可选: 保存 Prefab Variant
            if (savePrefab)
            {
                if (!AssetDatabase.IsValidFolder(prefabFolder))
                    CreateFolderRecursive(prefabFolder);
                var path = AssetDatabase.GenerateUniqueAssetPath($"{prefabFolder}/{characterName}.prefab");
                PrefabUtility.SaveAsPrefabAssetAndConnect(characterRoot, path, InteractionMode.UserAction);
                Debug.Log($"[Builder] Prefab saved -> {path}");
            }

            Selection.activeGameObject = characterRoot;
            EditorGUIUtility.PingObject(characterRoot);
            Debug.Log($"[Builder] '{characterName}' 已生成.");
        }

        // ===== Helpers =====
        // 实例化 FBX 并解除 prefab 连接, 否则 prefab 内部 transform 不能被 SetParent 到外面.
        static GameObject InstantiateUnpacked(GameObject fbx)
        {
            if (fbx == null) return null;
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            if (inst == null) return null;
            PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            inst.name = fbx.name;
            return inst;
        }

        static void AttachPart(GameObject fbx, Transform skeletonRoot, Transform meshContainer, bool rebakeBindposes)
        {
            if (fbx == null) return;
            var inst = InstantiateUnpacked(fbx);
            if (inst == null) return;
            var smrs = SkinnedMeshAttacher.Attach(inst, skeletonRoot, meshContainer,
                destroySourceAfter: true, rebakeBindposes: rebakeBindposes);
            if (smrs.Count == 0)
                Debug.LogWarning($"[Builder] {fbx.name} 里没找到 SkinnedMeshRenderer.");
        }

        static void MountWeapon(GameObject fbx, Transform skeletonRoot, string boneName)
        {
            if (fbx == null) return;
            var bone = FindBoneByName(skeletonRoot, boneName);
            if (bone == null)
            {
                Debug.LogWarning($"[Builder] 找不到骨骼 '{boneName}',武器 {fbx.name} 没挂上.");
                return;
            }
            var inst = InstantiateUnpacked(fbx);
            if (inst == null) return;
            inst.transform.SetParent(bone, false);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.identity;
        }

        static Transform FindBoneByName(Transform root, string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }

        static Avatar LoadAvatarFromFbx(GameObject fbx)
        {
            var path = AssetDatabase.GetAssetPath(fbx);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
                if (a is Avatar av) return av;
            return null;
        }

        // 对生成的角色根下所有 SMR 矫正 rootBone 然后调 ResetBounds.
        //
        // 为什么不能只调 ResetBounds:
        //   SMR.bounds = TransformBounds(localBounds, rootBone.localToWorld)
        //   localBounds 是 mesh 顶点在 rootBone 局部空间的范围.
        //   如果 rootBone 指向 Armature 根 (世界原点附近) 而 mesh 实际渲染在远处,
        //   localBounds 会变成一个 "从原点延伸到 mesh" 的超大盒, bounds 跟着飞.
        //
        // 修法: 在 ResetBounds 之前, 把 rootBone 强制指向 mesh 实际依赖的骨头.
        //   首选 "Pelvis" (FROM 角色的标准重心骨), fallback 到 bones[] 第一个 valid 的.
        static void RefreshAllBounds(GameObject root, float expand)
        {
            if (root == null) return;
            var smrs = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            int rootBoneFixed = 0;
            foreach (var smr in smrs)
            {
                if (smr == null || smr.sharedMesh == null) continue;
                Undo.RecordObject(smr, "Refresh SMR Bounds");
        
                // 1. 矫正 rootBone (关键!)
                Transform better = PickBetterRootBone(smr, root.transform);
                if (better != null && better != smr.rootBone)
                {
                    smr.rootBone = better;
                    rootBoneFixed++;
                }
        
                // 2. 重算 localBounds (相对新 rootBone)
                smr.ResetBounds();
        
                // 3. 扩张冗余
                if (expand > 0f)
                {
                    var b = smr.localBounds;
                    b.Expand(expand);
                    smr.localBounds = b;
                }
            }
            if (smrs.Length > 0)
                Debug.Log($"[Builder] Bounds refreshed on {smrs.Length} SMR(s) (rootBone fixed: {rootBoneFixed}, expand={expand:F2}m).");
        }
        
        // 选一个合理的 rootBone:
        //   1. bones[] 里存在 "Pelvis" / "Hips" / "Spine" / "Root", 优先用
        //   2. bones[] 第一个非 null 且在角色 root 层级下的
        //   3. fallback: 角色根自身
        static Transform PickBetterRootBone(SkinnedMeshRenderer smr, Transform characterRoot)
        {
            string[] preferred = { "Pelvis", "Hips", "Hip", "Spine", "Root" };
            Transform[] bones = smr.bones;
            if (bones != null && bones.Length > 0)
            {
                foreach (var name in preferred)
                {
                    foreach (var b in bones)
                        if (b != null && b.name == name && b.IsChildOf(characterRoot))
                            return b;
                }
                foreach (var b in bones)
                    if (b != null && b.IsChildOf(characterRoot))
                        return b;
            }
            return characterRoot;
        }

        static void CreateFolderRecursive(string folder)
        {
            folder = folder.Replace('\\', '/');
            var parts = folder.Split('/');
            var current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
