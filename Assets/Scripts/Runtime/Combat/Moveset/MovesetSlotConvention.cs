using System;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 逻辑攻击槽位（与具体武器无关）。ER 的动画 ID 后缀在所有同类武器里含义一致，
    /// 所以这套"槽位 → 逻辑动作"的对应只需定义一次，即可套用到全部武器。
    /// </summary>
    public enum AttackSlot
    {
        None = 0,
        R1_1, R1_2, R1_3, R1_4, R1_5,   // 轻击连段（普通连招）
        R2_1, R2_2,                     // 重击 / 蓄力连段
        RunAttack,                      // 冲刺攻击
        RollAttack,                     // 翻滚攻击
        BackstepAttack,                 // 后撤步攻击
        JumpAttack,                     // 跳跃攻击
        GuardCounter,                   // 盾反 / 防御反击
    }

    /// <summary>
    /// ER 动画 ID 通用约定表：把"持武状态 + 逻辑槽位"翻译成 ER 动画 ID 的数字组成。
    /// <para/>
    /// ER 动画 ID = a{wepMotionCategory:000}_{baseSlot + suffix:000000}。
    /// 例：直剑(cat=23) 单手(base=30000) R1_1(suffix=0) → a023_030000；
    /// 双手(base=32000) R1_1 → a023_032000。
    /// <para/>
    /// 这套约定在所有同类武器间通用，因此自动回填器只需读这一张表，
    /// 换 category 号即可为任意武器生成 <see cref="MovesetData"/>。
    /// 菜单 Assets → Create → ARPG → Moveset Slot Convention 创建后，
    /// 右键资源 → "Fill ER Defaults" 即可播种默认值。
    /// </summary>
    [CreateAssetMenu(menuName = "ARPG/Moveset Slot Convention")]
    public class MovesetSlotConvention : ScriptableObject
    {
        /// <summary>持武状态 → 该状态在 ER 动画 ID 里的基址（6 位槽位的前缀部分）。</summary>
        [Serializable]
        public struct HandBaseEntry
        {
            public HandState handState;
            [Tooltip("ER 槽位基址：单手右手 30000 / 双手 32000 / 双持 35000。")]
            public int baseSlot;
            [Tooltip("是否为该持武状态生成招式图。")]
            public bool enabled;
        }

        /// <summary>逻辑槽位 → 后缀偏移 + 默认攻击属性。</summary>
        [Serializable]
        public struct SlotEntry
        {
            public AttackSlot slot;
            [Tooltip("相对 baseSlot 的后缀。例：R1_1=0, R1_2=10, R2_1=500, 跑攻=200。")]
            public int suffix;
            public AttackType attackType;
            public bool applyRootMotion;
            public bool canCharge;
            [Tooltip("置信度 / 来源备注，仅供阅读，不参与逻辑。")]
            public string note;
        }

        [Header("持武状态 → 槽位基址")]
        public HandBaseEntry[] hands = Array.Empty<HandBaseEntry>();

        [Header("逻辑槽位 → 后缀 + 属性")]
        public SlotEntry[] slots = Array.Empty<SlotEntry>();

        /// <summary>取某槽位定义；不存在返回 false。</summary>
        public bool TryGetSlot(AttackSlot slot, out SlotEntry entry)
        {
            if (slots != null)
            {
                foreach (var s in slots)
                {
                    if (s.slot == slot) { entry = s; return true; }
                }
            }
            entry = default;
            return false;
        }

        /// <summary>取某持武状态的基址；未启用或不存在返回 false。</summary>
        public bool TryGetHandBase(HandState handState, out int baseSlot)
        {
            if (hands != null)
            {
                foreach (var h in hands)
                {
                    if (h.handState == handState && h.enabled) { baseSlot = h.baseSlot; return true; }
                }
            }
            baseSlot = 0;
            return false;
        }

        /// <summary>组合出 ER 动画 ID 字符串，如 a023_030000。</summary>
        public static string ComposeClipName(int wepMotionCategory, int baseSlot, int suffix)
        {
            return $"a{wepMotionCategory:000}_{baseSlot + suffix:000000}";
        }

        /// <summary>
        /// 组合出角色内唯一的整型动画 ID：category*1_000_000 + (baseSlot+suffix)。
        /// 例：a023_030000 → 23030000。用于 CharacterAnimationLibrary 按 id 解析与 RPC 按 id 同步。
        /// </summary>
        public static int ComposeAnimId(int wepMotionCategory, int baseSlot, int suffix)
        {
            return wepMotionCategory * 1_000_000 + (baseSlot + suffix);
        }

        [ContextMenu("Fill ER Defaults")]
        public void FillDefaults()
        {
            hands = new[]
            {
                new HandBaseEntry { handState = HandState.OneHandRight, baseSlot = 30000, enabled = true },
                new HandBaseEntry { handState = HandState.TwoHand,      baseSlot = 32000, enabled = true },
                // 双持(power stance)：a023 仅有 035000-040 这条 R1 链，无独立 R2/跑攻等。
                new HandBaseEntry { handState = HandState.DualWield,    baseSlot = 35000, enabled = true },
            };

            slots = new[]
            {
                // ── 轻击五连（高置信：CSV a023_030000~030040 命中框 + 刀光 + 根运动全对得上） ──
                new SlotEntry { slot = AttackSlot.R1_1, suffix = 0,   attackType = AttackType.LightAttack01, applyRootMotion = true, note = "R1 第一段（高置信）" },
                new SlotEntry { slot = AttackSlot.R1_2, suffix = 10,  attackType = AttackType.LightAttack02, applyRootMotion = true, note = "R1 第二段（高置信）" },
                new SlotEntry { slot = AttackSlot.R1_3, suffix = 20,  attackType = AttackType.LightAttack01, applyRootMotion = true, note = "R1 第三段（高置信）" },
                new SlotEntry { slot = AttackSlot.R1_4, suffix = 30,  attackType = AttackType.LightAttack02, applyRootMotion = true, note = "R1 第四段（高置信）" },
                new SlotEntry { slot = AttackSlot.R1_5, suffix = 40,  attackType = AttackType.LightAttack01, applyRootMotion = true, note = "R1 第五段（高置信）" },

                // ── 重击 / 蓄力（高置信：030500/030510 时长长、命中窗靠后） ──
                new SlotEntry { slot = AttackSlot.R2_1, suffix = 500, attackType = AttackType.HeavyAttack01, applyRootMotion = true, canCharge = true, note = "R2 第一段 / 可蓄力（高置信）" },
                new SlotEntry { slot = AttackSlot.R2_2, suffix = 510, attackType = AttackType.HeavyAttack02, applyRootMotion = true, canCharge = true, note = "R2 第二段 / 可蓄力（高置信）" },

                // ── 上下文攻击（中置信：建议在 DSAS 抽查确认） ──
                new SlotEntry { slot = AttackSlot.RunAttack,      suffix = 200, attackType = AttackType.RunningAttack01,      applyRootMotion = true, note = "冲刺攻击（中置信，建议 DSAS 核对）" },
                new SlotEntry { slot = AttackSlot.RollAttack,     suffix = 210, attackType = AttackType.RollingAttack01,      applyRootMotion = true, note = "翻滚攻击（中置信，有 poise 标记）" },
                new SlotEntry { slot = AttackSlot.BackstepAttack, suffix = 300, attackType = AttackType.BackstepAttack01,     applyRootMotion = true, note = "后撤步攻击（中置信）" },
                new SlotEntry { slot = AttackSlot.JumpAttack,     suffix = 400, attackType = AttackType.LightJumpingAttack01, applyRootMotion = true, note = "跳跃攻击（中置信）" },
                new SlotEntry { slot = AttackSlot.GuardCounter,   suffix = 700, attackType = AttackType.HeavyAttack01,        applyRootMotion = true, note = "盾反 / 防御反击（待确认）" },
            };
        }
    }
}
