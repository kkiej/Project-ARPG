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
        CrouchAttack,                   // 下蹲攻击
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

            [Header("蓄力重击（ER：长按播 0500 蓄满，短按切 0505 直接出手；canCharge 时生效）")]
            [Tooltip("短按/未蓄满的直接出手 clip 后缀（如重击1 = 505 → a023_030505，重击2 = 515）。0=无，则没蓄满也用蓄满 clip。")]
            public int chargeQuickSuffix;
            [Tooltip("最短按住判定时长（秒）：起手后此时间内不判定松手，给手柄 Hold 交互留出识别窗，避免误判成短按。")]
            public float chargeMinHoldTime;
            [Tooltip("蓄满提交时点（秒，按蓄满 clip 0500 播放时间）：到此仍按住=蓄满（继续 0500）；此前松手=短按（切 0505）。")]
            public float chargeCommitTime;

            [Header("跳跃攻击落地融合（仅 JumpAttack 槽用；后缀 >0 才生效）")]
            [Tooltip("空中维持后缀（ER 03x060）：空中攻 clip 播完仍在高处时的维持段。0=无。")]
            public int airHoldSuffix;
            [Tooltip("触地攻后缀（ER 03x070）：快触地时切入、带自己命中框的落地攻。0=无，落地走通用收招。")]
            public int landingSuffix;
            [Tooltip("落地恢复后缀（ER 03x071）：短距离 + 前序未到尾声的收招段，无命中框，也是其余落地恢复的回退。0=无。")]
            public int landingRecoverySuffix;
            [Tooltip("落地恢复后缀（ER 03x072）：长距离 + 前序未到尾声。0=无。")]
            public int landingRecoveryLongSuffix;
            [Tooltip("落地恢复后缀（ER 03x081）：短距离 + 前序接近尾声（快速收招）。0=无。")]
            public int landingRecoveryFastSuffix;
            [Tooltip("落地恢复后缀（ER 03x082）：长距离 + 前序接近尾声。0=无。")]
            public int landingRecoveryFastLongSuffix;
            [Tooltip("落地下落高度阈值（米）：从空中最高点到落地点的下落高度 ≥ 此值视为落得高/远（072/082）。<=0 一律按低。")]
            public float landingLongFallThreshold;
            [Tooltip("前序动画快慢阈值（归一化 0~1）：触地时空中攻进度 ≥ 此值（或已进入维持）视为接近尾声 → 快速恢复。<=0 一律按非快速。")]
            public float landingFastProgressThreshold;
            [Tooltip("触地攻的攻击类型（命中框伤害倍率）。")]
            public AttackType landingAttackType;
            [Tooltip("快触地预判距离（米）：离地小于此值即切触地攻，让冲击对齐。0=只用 isGrounded。")]
            public float landingLookahead;

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

                // ── 重击 / 蓄力（ER：长按 0500 蓄满 / 短按 0505 直接出手；0510/0515 同理） ──
                new SlotEntry { slot = AttackSlot.R2_1, suffix = 500, attackType = AttackType.HeavyAttack01, applyRootMotion = true, canCharge = true,
                    chargeQuickSuffix = 505, chargeMinHoldTime = 0.2f, chargeCommitTime = 0.6f, note = "R2_1：长按 030500 蓄满 / 短按 030505 直接出手" },
                new SlotEntry { slot = AttackSlot.R2_2, suffix = 510, attackType = AttackType.HeavyAttack02, applyRootMotion = true, canCharge = true,
                    chargeQuickSuffix = 515, chargeMinHoldTime = 0.2f, chargeCommitTime = 0.6f, note = "R2_2：长按 030510 蓄满 / 短按 030515 直接出手" },

                // ── 上下文攻击（中置信：建议在 DSAS 抽查确认） ──
                new SlotEntry { slot = AttackSlot.RunAttack,      suffix = 200, attackType = AttackType.RunningAttack01,      applyRootMotion = true, note = "冲刺攻击（中置信，建议 DSAS 核对）" },
                new SlotEntry { slot = AttackSlot.RollAttack,     suffix = 210, attackType = AttackType.RollingAttack01,      applyRootMotion = true, note = "翻滚攻击（中置信，有 poise 标记）" },
                new SlotEntry { slot = AttackSlot.BackstepAttack, suffix = 300, attackType = AttackType.BackstepAttack01,     applyRootMotion = true, note = "后撤步攻击（中置信）" },
                // ── 跳跃攻击（ER 落地融合：空中攻 03x030 → 维持 03x060 → 触地攻 03x070 → 恢复 03x071） ──
                // suffix=1030 → base 30000 时得 a0xx_031030；触地攻/恢复同理由各 suffix 组出。
                new SlotEntry { slot = AttackSlot.JumpAttack,     suffix = 1030, attackType = AttackType.LightJumpingAttack01, applyRootMotion = true,
                    airHoldSuffix = 1060, landingSuffix = 1070, landingRecoverySuffix = 1071,
                    landingRecoveryLongSuffix = 1072, landingRecoveryFastSuffix = 1081, landingRecoveryFastLongSuffix = 1082,
                    landingLongFallThreshold = 2f, landingFastProgressThreshold = 0.7f,
                    landingAttackType = AttackType.LightJumpingAttack01, landingLookahead = 0.4f,
                    note = "空中攻 031030→维持 031060→触地攻 031070→恢复 071/072/081/082（短长×快慢四选一）" },
                // ── 下蹲轻攻击（a023_030310）──
                new SlotEntry { slot = AttackSlot.CrouchAttack,   suffix = 310, attackType = AttackType.LightAttack01,        applyRootMotion = true, note = "下蹲轻攻击 030310" },
                new SlotEntry { slot = AttackSlot.GuardCounter,   suffix = 700, attackType = AttackType.HeavyAttack01,        applyRootMotion = true, note = "盾反 / 防御反击（待确认）" },
            };
        }
    }
}
