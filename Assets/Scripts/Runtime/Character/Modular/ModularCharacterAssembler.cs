using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 模块化角色装配器（阶段 0/1）。
    /// 思路（参考 Modular Rig Builder）：把部件的 SkinnedMeshRenderer **重绑定到 c0000 通用骨架**——
    /// 共享骨按名指向通用骨架；部件自带、通用骨架没有的"附加骨"（护甲片/布料/挂点）嫁接到通用骨架对应骨之下；
    /// 最后把不再被引用的部件自带骨架（如 'BD_M_1350 Armature'）清理掉。
    /// SkinnedMeshRenderer 的形变只取决于 bones[] 数组，与其所在 Transform 父级无关，
    /// 因此重绑定后部件即随通用骨架（动画）运动。
    ///
    /// 阶段 0/1 用直接 prefab 引用（同步 Instantiate/Destroy）；后续可无痛替换为 Addressables 异步加载。
    /// </summary>
    public class ModularCharacterAssembler : MonoBehaviour
    {
        [Tooltip("c0000 通用骨架映射。留空则在子物体中自动查找。")]
        [SerializeField] private SkeletonBoneMap skeleton;

        [Tooltip("提取出的部件网格(SMR)挂到这里。留空默认挂到 c0000（骨架的父级）下。")]
        [SerializeField] private Transform meshAttachRoot;

        [Tooltip("装配后审计每个 SMR 的 bones[]：统计 null 骨、不在骨架根下的骨，并打印骨名。\n" +
                 "用于排查'顶点固定在世界原点/不随骨架移动'的问题。定位后可关闭。")]
        [SerializeField] private bool debugAuditBones = false;

        // 每个槽位已装配的内容：提取到 c0000 下的 SMR 对象 + 嫁接到通用骨架下的附加骨
        private class EquippedPart
        {
            public readonly List<GameObject> rendererObjects = new();
            public readonly List<Transform> graftedBones = new();
        }

        private readonly Dictionary<BodySlot, EquippedPart> equipped = new();

        public SkeletonBoneMap Skeleton => skeleton;

        private void Awake()
        {
            if (skeleton == null)
                skeleton = GetComponentInChildren<SkeletonBoneMap>(true);

            ResolveMeshAttachRoot();
        }

        private void ResolveMeshAttachRoot()
        {
            if (meshAttachRoot != null) return;
            // 默认挂到 c0000（即骨架 'c0000 Armature' 的父级）；再退到本物体
            meshAttachRoot = (skeleton != null && skeleton.transform.parent != null)
                ? skeleton.transform.parent
                : transform;
        }

        /// <summary>
        /// 装配某槽位的部件。传入 null 视为卸下该槽。
        /// 流程：实例化(临时) → 附加骨嫁接到 c0000 → 重绑定 → 把 SMR 提取到 c0000 下 → 销毁整个临时实例。
        /// 结果：场景里 c0000 下只多出部件网格(SMR)，不留外层包装与部件自带骨架。
        /// </summary>
        /// <returns>提取到 c0000 下的第一个 SMR 对象（无则 null）。</returns>
        public GameObject EquipPart(BodySlot slot, GameObject partPrefab)
        {
            UnequipPart(slot);

            if (partPrefab == null) return null;

            if (skeleton == null)
            {
                Debug.LogError("[ModularCharacterAssembler] 缺少 SkeletonBoneMap，无法重绑定。", this);
                return null;
            }

            skeleton.Build();
            ResolveMeshAttachRoot();

            // 临时实例化（先不挂到角色下，处理完销毁）
            GameObject temp = Instantiate(partPrefab);
            temp.name = partPrefab.name;

            var renderers = temp.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (renderers.Length == 0)
            {
                Debug.LogWarning($"[ModularCharacterAssembler] 部件 '{partPrefab.name}' 没有 SkinnedMeshRenderer。", this);
                DestroyObj(temp);
                return null;
            }

            //  [审计] 实例化后、嫁接前：部件源头的 bones[] 是否已有 null（=导入阶段就丢骨 → 顶点会塌到原点）
            if (debugAuditBones)
                AuditSourceBones(partPrefab.name, renderers);

            var record = new EquippedPart();

            // 1) 嫁接附加骨到 c0000（保证所有被 SMR 引用的附加骨都移出 temp，使 temp 可安全销毁）
            GraftExtraBones(temp, renderers, record.graftedBones);

            // 2) 重绑定每个 SMR
            int totalShared = 0, totalBones = 0;
            foreach (SkinnedMeshRenderer smr in renderers)
            {
                RebindToSkeleton(smr, out int shared, out int total);
                totalShared += shared;
                totalBones += total;
            }

            if (totalShared == 0)
            {
                Debug.LogError($"[ModularCharacterAssembler] 部件 '{partPrefab.name}' 没有任何共享骨匹配到 c0000（{totalBones} 根全未命中）。\n{BuildMismatchReport(renderers)}", this);
                DestroyObj(temp);
                return null;
            }

            // 3) 把 SMR 对象提取到 c0000 下（SkinnedMesh 形变只取决于 bones[]，与父级无关；归零本地 TRS）
            foreach (SkinnedMeshRenderer smr in renderers)
            {
                Transform t = smr.transform;
                t.SetParent(meshAttachRoot, false);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
                record.rendererObjects.Add(smr.gameObject);
            }

            // 4) 销毁临时实例（此时只剩外层包装 + 部件自带骨架，已无被引用对象）
            DestroyObj(temp);

            //  [审计] 装配完成后：最终 bones[] 里的 null 骨 / 不在骨架根下的骨（=不随动画移动 → 视觉上固定在原点）
            if (debugAuditBones)
                AuditAssembledBones(partPrefab.name, record);

            equipped[slot] = record;
            return record.rendererObjects.Count > 0 ? record.rendererObjects[0] : null;
        }

        // ───────────────────────────────────────── 骨骼审计（调试） ─────────────────────────────────────────

        /// <summary>实例化后、嫁接前审计：统计并打印部件源头 SMR 的 null 骨（导入阶段丢骨的铁证）。</summary>
        private void AuditSourceBones(string partName, SkinnedMeshRenderer[] renderers)
        {
            for (int r = 0; r < renderers.Length; r++)
            {
                SkinnedMeshRenderer smr = renderers[r];
                Transform[] bones = smr.bones;
                int nullCount = 0;
                var nullIndices = new List<int>();
                for (int i = 0; i < bones.Length; i++)
                    if (bones[i] == null) { nullCount++; if (nullIndices.Count < 30) nullIndices.Add(i); }

                Mesh mesh = smr.sharedMesh;
                int bindposes = mesh != null ? mesh.bindposes.Length : -1;
                string msg = $"[Audit-源] 部件 '{partName}' SMR#{r} '{smr.name}'：bones={bones.Length}，bindposes={bindposes}，" +
                             $"rootBone={(smr.rootBone ? smr.rootBone.name : "<null>")}，源头 null 骨={nullCount}";
                if (nullCount > 0)
                {
                    msg += $"\n  → null 骨索引: {string.Join(",", nullIndices)}（这些槽有权重的顶点会塌到网格原点=世界原点）。" +
                           "\n  这属于【导入阶段丢骨】，与嫁接无关。多半是 FBX 导入的 optimizeBones/名字冲突所致。";
                    Debug.LogError(msg, smr);
                }
                else
                {
                    Debug.Log(msg + "（源头无 null，问题不在导入丢骨）", smr);
                }
            }
        }

        /// <summary>装配后审计：最终 bones[] 里 null 骨 + 不在骨架根下的骨 + 停在世界原点附近却有权重的骨（=原点尖刺元凶）。</summary>
        private void AuditAssembledBones(string partName, EquippedPart record)
        {
            Transform skelRoot = skeleton != null ? skeleton.transform : null;
            foreach (GameObject go in record.rendererObjects)
            {
                var smr = go.GetComponent<SkinnedMeshRenderer>();
                if (smr == null) continue;
                AuditRenderer(partName, smr, skelRoot);
            }
        }

        /// <summary>
        /// 单个 SMR 的深度审计：null 骨 / 骨架外骨 / 停在世界原点附近的【带权重】骨。
        /// 世界原点附近且有权重的骨会把它影响的顶点拉向 (0,0,0)，形成"固定在原点"的尖刺。
        /// </summary>
        private void AuditRenderer(string partName, SkinnedMeshRenderer smr, Transform skelRoot)
        {
            Transform[] bones = smr.bones;
            Mesh mesh = smr.sharedMesh;

            //  统计每根骨的总权重，判断"有效骨"（有权重才会影响顶点/造成尖刺）。
            //  注意：mesh 若 isReadable=0，boneWeights 可能为空 → 退化为"不按权重过滤"，标出所有近原点骨。
            var boneWeightSum = new float[bones.Length];
            bool haveWeights = false;
            if (mesh != null)
            {
                var bw = mesh.boneWeights;
                haveWeights = bw != null && bw.Length > 0;
                foreach (var w in bw)
                {
                    if (w.boneIndex0 >= 0 && w.boneIndex0 < bones.Length) boneWeightSum[w.boneIndex0] += w.weight0;
                    if (w.boneIndex1 >= 0 && w.boneIndex1 < bones.Length) boneWeightSum[w.boneIndex1] += w.weight1;
                    if (w.boneIndex2 >= 0 && w.boneIndex2 < bones.Length) boneWeightSum[w.boneIndex2] += w.weight2;
                    if (w.boneIndex3 >= 0 && w.boneIndex3 < bones.Length) boneWeightSum[w.boneIndex3] += w.weight3;
                }
            }

            int nullCount = 0, outsideCount = 0, atOriginWeighted = 0;
            var outsideNames = new List<string>();
            var originNames = new List<string>();
            for (int i = 0; i < bones.Length; i++)
            {
                Transform b = bones[i];
                if (b == null) { nullCount++; continue; }
                if (skelRoot != null && !b.IsChildOf(skelRoot) && b != skelRoot)
                {
                    outsideCount++;
                    if (outsideNames.Count < 40 && !outsideNames.Contains(b.name)) outsideNames.Add(b.name);
                }
                //  停在世界原点附近（<5cm），且有权重（或权重不可读时一律标出）→ 会把顶点拉向原点
                if (b.position.sqrMagnitude < 0.0025f && (!haveWeights || boneWeightSum[i] > 0.0001f))
                {
                    atOriginWeighted++;
                    if (originNames.Count < 40)
                    {
                        string wtxt = haveWeights ? $"w={boneWeightSum[i]:0.###}" : "w=?";
                        originNames.Add($"{b.name}({wtxt}, parent={(b.parent ? b.parent.name : "?")})");
                    }
                }
            }

            //  【硬证据】烘焙当前形变后的顶点，逐顶点找真正落在世界原点的（不受 isReadable 限制）。
            int stuckVerts = 0, totalVerts = 0;
            Vector3 sampleWorld = Vector3.zero;
            try
            {
                var baked = new Mesh();
                smr.BakeMesh(baked);
                Vector3[] bv = baked.vertices;
                totalVerts = bv.Length;
                Matrix4x4 l2w = smr.transform.localToWorldMatrix;
                for (int i = 0; i < bv.Length; i++)
                {
                    Vector3 world = l2w.MultiplyPoint3x4(bv[i]);
                    if (world.sqrMagnitude < 0.0025f) //  距世界原点 <5cm
                    {
                        stuckVerts++;
                        if (stuckVerts == 1) sampleWorld = world;
                    }
                }
                Destroy(baked);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Audit] BakeMesh 失败：{e.Message}", smr);
            }

            string wnote = haveWeights ? "" : "（权重不可读:isReadable=0，原点骨未按权重过滤）";
            string msg = $"[Audit-终] 部件 '{partName}' SMR '{smr.name}'：bones={bones.Length}，" +
                         $"null={nullCount}，骨架外={outsideCount}，原点附近骨={atOriginWeighted}{wnote}，" +
                         $"【烘焙落在原点的顶点】={stuckVerts}/{totalVerts}";
            bool bad = nullCount > 0 || outsideCount > 0 || atOriginWeighted > 0 || stuckVerts > 0;
            if (outsideNames.Count > 0)
                msg += $"\n  → 骨架外的骨: {string.Join(", ", outsideNames)}";
            if (originNames.Count > 0)
                msg += $"\n  → 停在世界原点附近的带权重骨: {string.Join("; ", originNames)}";
            if (stuckVerts > 0)
                msg += $"\n  → 有 {stuckVerts} 个顶点被解算到世界原点(样例 {sampleWorld})。骨结构正常但顶点仍塌原点 → 属【绑定姿势/静止姿势不一致】：" +
                       "部件蒙皮所依据的骨骼静止姿势与运行时 c0000 骨架当前姿势不符（bindpose 与 bone 当前世界矩阵对不上）。";

            if (bad) Debug.LogError(msg, smr);
            else Debug.Log(msg + "（结构/位置/烘焙均正常）", smr);
        }

        /// <summary>
        /// 扫描整个角色（本组件所在层级下）的【所有】SkinnedMeshRenderer 并逐个审计。
        /// 用于确认"固定在原点的顶点"到底出自哪个网格——含基础身体/头发等非模块化网格。
        /// 运行时右键组件菜单调用，或代码里调。
        /// </summary>
        [ContextMenu("Audit All Renderers On Character")]
        public void AuditWholeCharacter()
        {
            if (skeleton != null) skeleton.Build();
            Transform skelRoot = skeleton != null ? skeleton.transform : null;

            Transform charRoot = transform.root;
            var all = charRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Debug.Log($"[Audit-全角色] 共 {all.Length} 个 SkinnedMeshRenderer，逐个审计：", this);
            foreach (var smr in all)
                AuditRenderer("(角色)", smr, skelRoot);
        }

        /// <summary>卸下并销毁某槽位的部件（提取到 c0000 下的网格 + 嫁接的附加骨）。</summary>
        public void UnequipPart(BodySlot slot)
        {
            if (!equipped.TryGetValue(slot, out EquippedPart part)) return;

            foreach (GameObject go in part.rendererObjects)
                DestroyObj(go);
            foreach (Transform grafted in part.graftedBones)
                DestroyObj(grafted ? grafted.gameObject : null);

            equipped.Remove(slot);
        }

        public void UnequipAll()
        {
            foreach (BodySlot slot in equipped.Keys.ToList())
                UnequipPart(slot);
        }

        public GameObject GetEquipped(BodySlot slot) =>
            equipped.TryGetValue(slot, out EquippedPart p) && p.rendererObjects.Count > 0
                ? p.rendererObjects[0] : null;

        // ───────────────────────────────────────── 核心实现 ─────────────────────────────────────────

        /// <summary>
        /// 把部件自带、通用骨架里没有的"附加骨"嫁接到通用骨架，并保证所有被 SMR 引用的附加骨都移出 temp。
        /// Pass1：附加骨链顶（父骨为共享骨）→ 重挂到通用对应骨下，保留本地 TRS（同为 c0000 绑定姿势，数值通用）。
        /// Pass2（兜底）：仍留在 temp 内、且被 SMR 引用的附加骨 → 重挂到通用根骨下，保留世界变换，避免随 temp 被销毁。
        /// </summary>
        private void GraftExtraBones(GameObject instance, SkinnedMeshRenderer[] renderers, List<Transform> graftedOut)
        {
            // Pass1：链顶嫁接
            var pass1 = new List<(Transform bone, Transform parent)>();
            foreach (Transform bone in instance.GetComponentsInChildren<Transform>(true))
            {
                if (skeleton.HasBone(bone.name)) continue;       // 共享骨跳过
                Transform parent = bone.parent;
                if (parent == null) continue;

                Transform universalParent = skeleton.GetBone(parent.name);
                if (universalParent != null)
                    pass1.Add((bone, universalParent));
            }
            foreach (var (bone, parent) in pass1)
            {
                bone.SetParent(parent, false);
                graftedOut.Add(bone);
            }

            // Pass2：兜底——任何仍在 temp 内、被 SMR 引用、且非共享的骨，挂到通用根骨下（保留世界变换）
            Transform root = skeleton.RootBone != null ? skeleton.RootBone : skeleton.transform;
            int orphan = 0;
            foreach (SkinnedMeshRenderer smr in renderers)
            {
                foreach (Transform b in smr.bones)
                {
                    if (b == null) continue;
                    if (!b.IsChildOf(instance.transform)) continue; // 已被移出
                    if (skeleton.HasBone(b.name)) continue;         // 共享骨：重绑定阶段会指向通用骨架
                    b.SetParent(root, true);
                    graftedOut.Add(b);
                    orphan++;
                }
            }
            if (orphan > 0)
                Debug.Log($"[ModularCharacterAssembler] {orphan} 根附加骨无对应基础父骨，已兜底挂到根骨 '{root.name}' 下（位置保留，但不随具体基础骨运动）。", this);
        }

        /// <summary>
        /// 把 SMR 的 bones / rootBone 按骨骼名重映射到通用骨架。保持数组长度与顺序不变（对应 bindposes）。
        /// 共享骨 → 通用骨架；附加骨（通用没有）→ 保留自身（已嫁接到通用骨架下）。
        /// </summary>
        public void RebindToSkeleton(SkinnedMeshRenderer smr, out int sharedMatched, out int totalNonNull)
        {
            sharedMatched = 0;
            totalNonNull = 0;
            if (smr == null) return;

            Transform[] src = smr.bones;
            Transform[] dst = new Transform[src.Length];

            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == null) { dst[i] = null; continue; }
                totalNonNull++;

                Transform target = skeleton.GetBone(src[i].name);
                if (target != null)
                {
                    dst[i] = target;
                    sharedMatched++;
                }
                else
                {
                    dst[i] = src[i]; // 附加骨，保留（已嫁接）
                }
            }

            smr.bones = dst;

            if (smr.rootBone != null)
            {
                Transform mappedRoot = skeleton.GetBone(smr.rootBone.name);
                if (mappedRoot != null) smr.rootBone = mappedRoot;
            }
        }

        /// 兼容旧签名：返回匹配到的共享骨数量。
        public int RebindToSkeleton(SkinnedMeshRenderer smr)
        {
            RebindToSkeleton(smr, out int shared, out _);
            return shared;
        }

        private static void DestroyObj(GameObject go)
        {
            if (go == null) return;
            if (Application.isPlaying) Destroy(go);
            else DestroyImmediate(go);
        }

        // ───────────────────────────────────────── 诊断 / 校验 ─────────────────────────────────────────

        /// <summary>失败时生成对照报告：部件骨骼名 vs 通用骨架骨骼名。</summary>
        private string BuildMismatchReport(SkinnedMeshRenderer[] renderers)
        {
            var partNames = renderers
                .SelectMany(r => r.bones)
                .Where(b => b != null)
                .Select(b => b.name)
                .Distinct()
                .Take(20)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"  通用骨架 '{skeleton.name}' 共 {skeleton.BoneCount} 根，示例：");
            sb.AppendLine($"    {skeleton.SampleBoneNames(20)}");
            sb.AppendLine($"  部件 SMR 引用的骨骼名示例：");
            sb.AppendLine($"    {string.Join(", ", partNames)}");
            sb.AppendLine("  → 若两边名字风格不同（如大小写/前缀/空格），即为不匹配原因。");
            return sb.ToString();
        }

        /// <summary>
        /// 诊断某部件 prefab 的骨骼与通用骨架的匹配情况（不实例化、无副作用），结果打到 Console。
        /// </summary>
        public void Diagnose(GameObject partPrefab)
        {
            if (partPrefab == null || skeleton == null)
            {
                Debug.LogWarning("[ModularCharacterAssembler] Diagnose: 缺少 partPrefab 或 skeleton。", this);
                return;
            }
            skeleton.Build();

            var renderers = partPrefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            int matched = 0, missing = 0;
            var missingNames = new List<string>();
            foreach (SkinnedMeshRenderer smr in renderers)
            {
                foreach (Transform b in smr.bones)
                {
                    if (b == null) continue;
                    if (skeleton.HasBone(b.name)) matched++;
                    else { missing++; if (!missingNames.Contains(b.name)) missingNames.Add(b.name); }
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine($"[Diagnose] 部件 '{partPrefab.name}'：SMR×{renderers.Length}，匹配 {matched} / 缺失 {missing}");
            sb.AppendLine($"  通用骨架 '{skeleton.name}' 共 {skeleton.BoneCount} 根，示例：{skeleton.SampleBoneNames(15)}");
            if (renderers.Length > 0)
            {
                var sample = renderers.SelectMany(r => r.bones).Where(b => b).Select(b => b.name).Distinct().Take(15);
                sb.AppendLine($"  部件骨骼名示例：{string.Join(", ", sample)}");
            }
            if (matched == 0 && missing > 0)
                Debug.LogError(sb.ToString(), this);
            else
                Debug.Log(sb.ToString(), this);
        }

        /// <summary>校验部件 prefab 的全部骨骼名是否都能在通用骨架找到，返回缺失列表。</summary>
        public List<string> ValidateBones(GameObject partPrefab)
        {
            var missing = new List<string>();
            if (partPrefab == null || skeleton == null) return missing;

            foreach (SkinnedMeshRenderer smr in partPrefab.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                foreach (Transform bone in smr.bones)
                    if (bone != null && !skeleton.HasBone(bone.name) && !missing.Contains(bone.name))
                        missing.Add(bone.name);

            return missing;
        }

        public enum MissingBoneCategory
        {
            ClothSim,
            Dummy,
            Face,
            RigidArmorOrOther
        }

        public static MissingBoneCategory ClassifyMissingBone(string boneName)
        {
            if (string.IsNullOrEmpty(boneName)) return MissingBoneCategory.RigidArmorOrOther;

            if (boneName.StartsWith("[cloth]") || boneName.EndsWith("_sim") || boneName.Contains("Collidable"))
                return MissingBoneCategory.ClothSim;

            if (boneName.Contains("Dmy") || boneName.Contains("Storage") || boneName.Contains("格納"))
                return MissingBoneCategory.Dummy;

            if (boneName.StartsWith("Face_"))
                return MissingBoneCategory.Face;

            return MissingBoneCategory.RigidArmorOrOther;
        }
    }
}
