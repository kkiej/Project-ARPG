using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 挂在角色的通用骨骼根上（通常是 Armature/Hips 所在的父物体）。
    /// 缓存整套骨骼的 name -> Transform 映射，供 <see cref="ModularCharacterAssembler"/>
    /// 在装配部件时把 SkinnedMeshRenderer 的骨骼重绑定到这套主骨骼。
    /// </summary>
    public class SkeletonBoneMap : MonoBehaviour
    {
        // ELDEN RING / FromSoftware 骨架的根骨候选（无 "Hips"）。
        // 注意：部分 FBX 导出会把 Master 等控制骨"压平"为 c0000 Armature 的直接子级，
        // 导致 Master 没有子节点；真正的形变骨链通常挂在 Pelvis 下。
        // 因此根骨识别优先选"存在且有子节点"的候选（一般是 Pelvis）。
        private static readonly string[] RootBoneCandidates = { "Pelvis", "Master", "Root" };

        [Tooltip("形变根骨，仅作参考（重绑定按骨骼名匹配，不依赖此字段）。留空自动识别 Pelvis/Master/Root。" +
                 "本组件请挂在所有骨骼的父级（如 'c0000 Armature'）上。")]
        [SerializeField] private Transform rootBone;

        private readonly Dictionary<string, Transform> boneLookup = new();
        // 归一化查找（去首尾空格 + 忽略大小写），用于兜底匹配命名差异
        private readonly Dictionary<string, Transform> normalizedLookup = new();
        private bool isBuilt;

        //  基础骨快照：首次构建（尚无任何部件嫁接骨时）记录 c0000 通用骨架的全部骨。
        //  之后 Build() 只从这份快照重建查找表，**绝不收录部件嫁接进来的附加骨**。
        //  否则：A 部件嫁接的附加骨会被重扫进查找表 → B 部件按名字误绑到 A 的骨上 →
        //  卸下 A 时销毁其嫁接骨 → B 的 bones[] 出现 null → 顶点塌世界原点。
        private readonly HashSet<Transform> baseBones = new();
        private bool baseCaptured;

        public Transform RootBone => rootBone;

        private static string Normalize(string n) => string.IsNullOrEmpty(n) ? n : n.Trim().ToLowerInvariant();

        private void Awake()
        {
            Build();
        }

        /// <summary>
        /// 重新扫描骨骼层级并构建查找表。骨骼结构变化后（如换骨架）需手动再调一次。
        /// </summary>
        public void Build() => Build(false);

        /// <summary>
        /// 重建查找表。
        /// <paramref name="recaptureBase"/>=true 时重新快照基础骨（仅在真正更换角色骨架时用；
        /// 此时必须确保层级下没有部件嫁接骨，否则会把附加骨误当基础骨）。
        /// </summary>
        public void Build(bool recaptureBase)
        {
            if (recaptureBase)
            {
                baseBones.Clear();
                baseCaptured = false;
            }

            //  首次构建：此刻尚无任何部件被装配（EquipPart 会先 Build 再嫁接），
            //  故当前层级下的全部骨即纯净的 c0000 基础骨，快照下来作为唯一权威来源。
            if (!baseCaptured)
            {
                baseBones.Clear();
                foreach (Transform bone in GetComponentsInChildren<Transform>(true))
                {
                    if (bone == transform) continue; // 跳过挂载本组件的节点自身
                    baseBones.Add(bone);
                }
                baseCaptured = true;
            }

            boneLookup.Clear();
            normalizedLookup.Clear();

            //  只从基础骨快照重建：任何后来嫁接进来的附加骨都不在快照里，天然被排除，
            //  保证 GetBone/HasBone 永远只解析到永久基础骨 → 各部件只绑到自己的附加骨。
            foreach (Transform bone in baseBones)
            {
                if (bone == null) continue; // 防御：基础骨被意外销毁

                // 同名骨骼以第一个为准（ER 骨骼名通常唯一；若有重名会在 Console 提示）
                if (!boneLookup.TryAdd(bone.name, bone))
                {
                    Debug.LogWarning($"[SkeletonBoneMap] 发现重名骨骼 '{bone.name}'，已忽略后者。重绑定可能不准确。", bone);
                }
                normalizedLookup.TryAdd(Normalize(bone.name), bone);
            }

            // 未手动指定时，自动识别 ER 根骨：
            // 优先选"存在且有子节点"的候选（应对 Master 被压平为叶子的情况），否则退而取存在的第一个。
            if (rootBone == null)
            {
                Transform fallback = null;
                foreach (string candidate in RootBoneCandidates)
                {
                    if (boneLookup.TryGetValue(candidate, out Transform found))
                    {
                        fallback ??= found;
                        if (found.childCount > 0)
                        {
                            rootBone = found;
                            break;
                        }
                    }
                }
                if (rootBone == null) rootBone = fallback;
            }

            isBuilt = true;
        }

        /// <summary>按骨骼名取主骨骼 Transform，找不到返回 null。先精确匹配，再归一化兜底。</summary>
        public Transform GetBone(string boneName)
        {
            if (!isBuilt) Build();
            if (string.IsNullOrEmpty(boneName)) return null;
            if (boneLookup.TryGetValue(boneName, out Transform bone)) return bone;
            return normalizedLookup.TryGetValue(Normalize(boneName), out Transform nb) ? nb : null;
        }

        public bool HasBone(string boneName) => GetBone(boneName) != null;

        public int BoneCount
        {
            get
            {
                if (!isBuilt) Build();
                return boneLookup.Count;
            }
        }

        /// <summary>诊断用：返回前 count 个骨骼名，便于在日志里和部件骨骼名对照。</summary>
        public string SampleBoneNames(int count = 20)
        {
            if (!isBuilt) Build();
            var names = new List<string>(boneLookup.Keys);
            int n = Mathf.Min(count, names.Count);
            return string.Join(", ", names.GetRange(0, n));
        }
    }
}
