using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 每角色作用域的动画解析服务（设计文档 §8.4，档位 A）。
    /// 把"当前可用动画"收拢进角色自己的字典：通用动画(<see cref="CharacterAnimationData"/>) + 当前武器 <see cref="MovesetData"/>。
    /// <para/>
    /// owner 选片、远端 RPC 收片都经此**同一**服务解析：
    /// <list type="bullet">
    /// <item>按角色作用域查找，天然避开跨角色 <c>aXX_YYYYYY</c> 重名；</item>
    /// <item>不依赖全局 <see cref="AnimationClipRegistry"/> 大表（避免上万 clip 全程常驻内存）。</item>
    /// </list>
    /// 档位 B（Boss / 角色专属大集）只需把内部换成 Addressables 异步加载，对外接口不变。
    /// </summary>
    public class CharacterAnimationLibrary
    {
        // clip 名 → clip（兼容现有按 clip.name 走的 RPC 路径）。
        private readonly Dictionary<string, AnimationClip> _byName = new Dictionary<string, AnimationClip>();
        // ER animId → clip（供 FSM 按 id 解析与 RPC 按 id 同步）。
        private readonly Dictionary<int, AnimationClip> _byAnimId = new Dictionary<int, AnimationClip>();
        // clip → ER animId（发送端反查：有 id 走 id RPC，无 id 回退 name RPC）。
        private readonly Dictionary<AnimationClip, int> _clipToAnimId = new Dictionary<AnimationClip, int>();

        /// <summary>"无 ER animId"的哨兵值。用 -1 而非 0，因为 a000_000000(idle) 的 animId 合法为 0。</summary>
        public const int NoAnimId = -1;

        public void Clear()
        {
            _byName.Clear();
            _byAnimId.Clear();
            _clipToAnimId.Clear();
        }

        /// <summary>
        /// 反射注册某数据对象上所有 public 的 <see cref="AnimationClip"/> 字段
        /// （<see cref="CharacterAnimationData"/> / WeaponAnimationSet 等）。这些 clip 无 ER animId，仅按名字索引。
        /// </summary>
        public void RegisterClipFields(object source)
        {
            if (source == null) return;

            foreach (var field in source.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.FieldType != typeof(AnimationClip)) continue;
                if (field.GetValue(source) is AnimationClip clip && clip != null)
                    _byName[clip.name] = clip;
            }
        }

        /// <summary>注册一份 moveset 的所有节点 clip（含蓄力链），按 name + animId 双索引。</summary>
        public void RegisterMoveset(MovesetData moveset)
        {
            if (moveset == null) return;

            RegisterHand(moveset.oneHandRight);
            RegisterHand(moveset.twoHand);
            RegisterHand(moveset.dualWield);
        }

        /// <summary>注册一份通用动画子集（a000_，locomotion / dodge 等），按 name + animId 双索引。</summary>
        public void RegisterCommonSet(CommonAnimationSet set)
        {
            if (set?.entries == null) return;

            foreach (var entry in set.entries)
                Add(entry.animId, entry.clip);
        }

        private void RegisterHand(HandMoveset hand)
        {
            if (hand?.nodes == null) return;

            foreach (var node in hand.nodes)
            {
                Add(node.animId, node.clip);
                // 蓄力链 clip 无独立 animId，仅按名字索引（够远端按名 RPC 解析）。
                Add(NoAnimId, node.chargeHold);
                Add(NoAnimId, node.chargeRelease);
                Add(NoAnimId, node.chargeFullRelease);
            }
        }

        private void Add(int animId, AnimationClip clip)
        {
            if (clip == null) return;
            _byName[clip.name] = clip;
            if (animId >= 0)
            {
                _byAnimId[animId] = clip;
                _clipToAnimId[clip] = animId;
            }
        }

        public bool TryGetByName(string clipName, out AnimationClip clip) => _byName.TryGetValue(clipName, out clip);

        public bool TryGetByAnimId(int animId, out AnimationClip clip) => _byAnimId.TryGetValue(animId, out clip);

        /// <summary>发送端反查 clip 的 ER animId；无 id（如通用 / 旧路径 clip）返回 false。</summary>
        public bool TryGetAnimId(AnimationClip clip, out int animId)
        {
            if (clip != null) return _clipToAnimId.TryGetValue(clip, out animId);
            animId = NoAnimId;
            return false;
        }
    }
}
