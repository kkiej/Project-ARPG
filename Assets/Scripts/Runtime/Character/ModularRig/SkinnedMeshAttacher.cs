using System.Collections.Generic;
using UnityEngine;

namespace LZ.ModularRig
{
    /// <summary>
    /// 把"自带骨架副本的 SkinnedMeshRenderer"重绑到一套共享真骨架上。
    /// 法环 c0000 系装备的核心机制:所有部件 FBX 都自带一份同名骨架,运行时全部指向唯一一套真骨架。
    /// </summary>
    public static class SkinnedMeshAttacher
    {
        /// <summary>
        /// 把 sourcePart(部件 FBX 实例)的所有 SMR 重定向到 targetSkeletonRoot,并把网格节点 reparent 到 meshContainer。
        /// 重绑完成后, sourcePart 里那套"副本骨架"就没用了, 可由调用方决定销毁。
        /// </summary>
        /// <param name="sourcePart">部件 FBX 实例化出来的根节点(里面有 SMR + 自带骨架副本)</param>
        /// <param name="targetSkeletonRoot">共享真骨架根(通常是 Skeleton.fbx 实例化出来的 Armature 根节点)</param>
        /// <param name="meshContainer">重绑后的 SMR GameObject 会被 reparent 到这里(便于统一管理 / 卸装备时整体销毁)</param>
        /// <param name="destroySourceAfter">是否销毁 sourcePart 残骸(已经只剩副本骨架,留着浪费)</param>
        /// <returns>重绑成功的 SMR 列表</returns>
        public static List<SkinnedMeshRenderer> Attach(
            GameObject sourcePart,
            Transform targetSkeletonRoot,
            Transform meshContainer,
            bool destroySourceAfter = true,
            bool rebakeBindposes = false)
        {
            var result = new List<SkinnedMeshRenderer>();
            if (sourcePart == null || targetSkeletonRoot == null || meshContainer == null)
            {
                Debug.LogError("[SkinnedMeshAttacher] Null argument.");
                return result;
            }

            // 1. 把目标骨架按名字索引一次(O(N) 一次, 后面 O(1) 查)
            var boneByName = BuildBoneMap(targetSkeletonRoot);

            var smrs = sourcePart.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);

            // 1.5 移植"部件专属骨" —— 如 [cloth] 布料链 / ScaleMail 等动态骨.
            //     这些骨只存在于部件 FBX, 共享骨架(c0000)里没有同名骨.
            //     做法: 把它们从部件骨架嫁接到共享骨架对应父骨下, 保留局部 TRS,
            //     并登记进 boneByName, 这样后面 remap 能找到, 布料蒙皮才不会塌.
            //     (必须在 remap 之前做; 嫁接出去的骨脱离 sourcePart, 销毁残骸时不受影响)
            GraftPartOnlyBones(smrs, boneByName, targetSkeletonRoot);

            // 2. 处理所有 SMR(一个部件 FBX 可能含多个材质分块的 SMR)
            foreach (var smr in smrs)
            {
                if (smr.sharedMesh == null) continue;

                // 2a. 抓取源骨骼引用 (重 parent 之前, 用于 bindpose 补偿)
                var oldBones = smr.bones;
                var oldSmrLocalToWorld = smr.transform.localToWorldMatrix;

                // 2b. 复用 SMR GameObject —— 把它从源父级移到 meshContainer
                smr.transform.SetParent(meshContainer, worldPositionStays: false);

                // 2c. 重映射 bones 数组
                var newBones = new Transform[oldBones.Length];
                int missing = 0;
                var missingNames = new List<string>();
                for (int i = 0; i < oldBones.Length; i++)
                {
                    if (oldBones[i] == null) { missing++; continue; }
                    if (boneByName.TryGetValue(oldBones[i].name, out var target))
                    {
                        newBones[i] = target;
                    }
                    else if (oldBones[i].name.EndsWith("_skeleton"))
                    {
                        // 部件 FBX 自己的骨架根容器节点 (如 "FC_M_0100_skeleton"), 不是形变骨,
                        // 目标骨架里没有同名节点. 回退到目标骨架根, 避免该 index 变成 null.
                        newBones[i] = targetSkeletonRoot;
                    }
                    else
                    {
                        missing++;
                        if (missingNames.Count < 8) missingNames.Add(oldBones[i].name);
                        // 找不到的骨骼: 标记为 null, 蒙皮会塌但不会崩
                        // (法环部件骨架是真骨架的子集, 理论上不该有 miss)
                    }
                }
                smr.bones = newBones;

                // 2d. rootBone 重映射
                if (smr.rootBone != null && boneByName.TryGetValue(smr.rootBone.name, out var newRoot))
                    smr.rootBone = newRoot;
                else
                    smr.rootBone = targetSkeletonRoot;

                // 2e. (可选) 重算 bindposes —— 当源 FBX 与目标骨架的单位/缩放不一致时使用
                //     原理: bindposes[i] = bone[i].worldToLocalMatrix * smr.transform.localToWorldMatrix (绑定时)
                //     用新 bones 的世界矩阵和 SMR 在源 FBX 中的世界矩阵重算, 抵消尺度/旋转差异
                if (rebakeBindposes)
                    RebakeBindposes(smr, oldBones, oldSmrLocalToWorld);

                // 2f. localBounds 保持部件导入时的值 ——
                //     它是相对 rootBone 局部空间、按 bind 姿势算好的, 已紧贴模型.
                //     c0000 与部件同尺度同骨架, rootBone 又映射到同名同朝向的骨, 所以原值依然有效.
                //     千万不要用 sharedMesh.bounds (那是网格局部空间, 坐标系不同) 覆盖, 会让盒子飞出去.

                if (missing > 0)
                {
                    string sample = missingNames.Count > 0 ? $" e.g. [{string.Join(", ", missingNames)}]" : "";
                    Debug.LogWarning($"[SkinnedMeshAttacher] {smr.name}: {missing}/{oldBones.Length} bones 在目标骨架里找不到同名骨{sample}. " +
                                     "若 miss 数≈全部, 说明骨架映射根选错或骨名不一致, 蒙皮会塌.");
                }

                result.Add(smr);
            }

            // 3. 销毁残骸(只剩副本骨架的源 GO)
            if (destroySourceAfter)
            {
                if (Application.isPlaying) Object.Destroy(sourcePart);
                else Object.DestroyImmediate(sourcePart);
            }

            return result;
        }

        /// <summary>
        /// 实例化一个 FBX prefab 并立即重绑到目标骨架,一步到位。
        /// </summary>
        public static List<SkinnedMeshRenderer> Instantiate(
            GameObject fbxPrefab,
            Transform targetSkeletonRoot,
            Transform meshContainer,
            bool rebakeBindposes = false)
        {
            if (fbxPrefab == null)
            {
                Debug.LogError("[SkinnedMeshAttacher] fbxPrefab is null.");
                return new List<SkinnedMeshRenderer>();
            }
            var inst = Object.Instantiate(fbxPrefab);
            inst.name = fbxPrefab.name;
            return Attach(inst, targetSkeletonRoot, meshContainer,
                destroySourceAfter: true, rebakeBindposes: rebakeBindposes);
        }

        /// <summary>
        /// 当源 FBX 与目标骨架 import scale 不一致时, 重算 bindposes 抵消差异.
        /// 注意: 会克隆 Mesh (避免污染 shared asset).
        /// </summary>
        private static void RebakeBindposes(SkinnedMeshRenderer smr, Transform[] oldBones, Matrix4x4 oldSmrLocalToWorld)
        {
            var newBones = smr.bones;
            if (newBones.Length != oldBones.Length) return;

            // 克隆 Mesh, 避免修改 shared asset
            var cloned = Object.Instantiate(smr.sharedMesh);
            cloned.name = smr.sharedMesh.name + " (Rebound)";

            var oldBP = cloned.bindposes;
            var newBP = new Matrix4x4[oldBP.Length];
            for (int i = 0; i < oldBP.Length; i++)
            {
                if (newBones[i] == null || oldBones[i] == null)
                {
                    newBP[i] = oldBP[i];
                    continue;
                }
                // 蒙皮: v_world = Σ wᵢ · (boneᵢ.localToWorld · bindposeᵢ) · v_local
                // 目标: 用新骨骼算出的 skinMatrix 与"源 FBX 里用旧骨骼算出的 skinMatrix"一致, 从而尺度/朝向差被抵消且形变保持.
                //   旧: skinMatrixᵢ = oldBonesᵢ.localToWorld · oldBP[i]
                //   令: newBonesᵢ.localToWorld · newBP[i] == oldBonesᵢ.localToWorld · oldBP[i]
                //   =>  newBP[i] = newBonesᵢ.worldToLocal · oldBonesᵢ.localToWorld · oldBP[i]
                // 注意必须用每根骨自己的 oldBonesᵢ.localToWorld(此时源 FBX 尚未销毁, oldBones 有效),
                // 不能统一用 SMR 的 localToWorld —— 那样会把所有骨塌成刚体, 一动就废.
                newBP[i] = newBones[i].worldToLocalMatrix * oldBones[i].localToWorldMatrix * oldBP[i];
            }
            cloned.bindposes = newBP;
            smr.sharedMesh = cloned;
        }

        // 把部件 SMR 引用、但共享骨架里不存在的骨(布料/动态骨等)嫁接到共享骨架.
        private static void GraftPartOnlyBones(
            SkinnedMeshRenderer[] smrs,
            Dictionary<string, Transform> boneByName,
            Transform targetSkeletonRoot)
        {
            var needed = new HashSet<Transform>();
            foreach (var smr in smrs)
            {
                if (smr == null) continue;
                var bones = smr.bones;
                for (int i = 0; i < bones.Length; i++)
                    if (bones[i] != null) needed.Add(bones[i]);
            }
            foreach (var b in needed)
                EnsureBoneInTarget(b, boneByName, targetSkeletonRoot);
        }

        // 确保 bone 在共享骨架里有对应物:
        //   - 已是共享骨(名字命中) -> 直接返回共享骨, 不动部件骨
        //   - 部件骨架根容器(*_skeleton) / 到顶了 -> 返回共享骨架根
        //   - 部件专属骨 -> 先递归确保其父骨在目标里, 再把自己嫁接到该父骨下(保留局部 TRS)
        private static Transform EnsureBoneInTarget(
            Transform bone,
            Dictionary<string, Transform> boneByName,
            Transform targetSkeletonRoot)
        {
            if (bone == null) return targetSkeletonRoot;
            if (boneByName.TryGetValue(bone.name, out var existing)) return existing;
            if (bone.name.EndsWith("_skeleton")) return targetSkeletonRoot;

            Transform targetParent = EnsureBoneInTarget(bone.parent, boneByName, targetSkeletonRoot);
            bone.SetParent(targetParent, worldPositionStays: false);
            boneByName[bone.name] = bone;
            return bone;
        }

        private static Dictionary<string, Transform> BuildBoneMap(Transform root)
        {
            var dict = new Dictionary<string, Transform>(256);
            var stack = new Stack<Transform>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var t = stack.Pop();

                // 跳过 FromSoft 的 dummy 点存储子树 (Model_Dmy_Storage / *_Dmy_* ).
                // 这些节点常与真实形变骨同名, 会污染按名映射, 导致蒙皮绑到 dummy 点上而错位.
                if (IsDummyStorageNode(t.name)) continue;

                // 第一个出现的名字优先(法环真骨架名唯一, 不会冲突)
                if (!dict.ContainsKey(t.name)) dict.Add(t.name, t);
                for (int i = 0; i < t.childCount; i++) stack.Push(t.GetChild(i));
            }
            return dict;
        }

        private static bool IsDummyStorageNode(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return name.Contains("Dmy_Storage") || name.Contains("Model_Dmy");
        }
    }
}
