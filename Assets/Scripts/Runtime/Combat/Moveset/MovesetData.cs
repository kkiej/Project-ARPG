using System;
using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 一个攻击节点 = 连招中的一击。
    /// 通过 nextOnLight / nextOnHeavy 指向同一 <see cref="HandMoveset.nodes"/> 数组里的下一节点，
    /// 形成数据驱动的 N 段连招图（支持轻/重分叉、终结技）。-1 表示该方向无后续（连招在此结束）。
    /// </summary>
    [Serializable]
    public struct AttackNode
    {
        [Tooltip("编辑器可读标签，如 R1_1 / R2_charge。仅用于阅读，不参与逻辑。")]
        public string label;

        public AnimationClip clip;

        [Tooltip("ER 角色内唯一动画 ID = wepMotionCategory*1_000_000 + (baseSlot+suffix)，例 a023_030000 → 23030000。" +
                 "由 MovesetAutoFiller 回填，供 CharacterAnimationLibrary 按 id 解析与未来 RPC 按 id 同步（档位 A）。0=未回填。")]
        public int animId;

        [Tooltip("用于伤害计算的攻击类型（沿用现有 AttackType 枚举）。")]
        public AttackType attackType;

        public bool applyRootMotion;

        [Header("连招开窗（秒，来自 ER TAE 的 Cancel-R1Attack(115)∪Cancel-RHAttack(4) 执行窗；缺失回退 Input-Common(87)）")]
        [Tooltip("可接出下一段的窗口起点（秒，按 clip 播放时间）。由 MovesetAutoFiller 从权威 SO(c0000_TAE.asset) 按 animId+flag 回填。" +
                 "输入缓冲由 ActionRequest 缓冲处理，故此处取「可执行」窗而非偏早的输入窗。")]
        public float comboWindowStart;
        [Tooltip("可输入下一段的窗口终点（秒）。<=0 表示无 TAE 数据，PlayerAttackState 回退到归一化 [0.35,0.95]。")]
        public float comboWindowEnd;

        [Header("伤害判定窗（秒，来自 ER TAE 的 AttackBehavior(type1)；由 MovesetAutoFiller 回填）")]
        [Tooltip("命中框开启时点（秒，按 clip 播放时间）。运行期到此开 OpenDamageCollider，替代 ER clip 缺失的动画事件。")]
        public float damageWindowStart;
        [Tooltip("命中框关闭时点（秒）。<=0 表示无 TAE 数据 → 不由窗口驱动（旧 clip 仍走自带动画事件）。")]
        public float damageWindowEnd;
        [Tooltip("跳攻触地攻 070 的命中框开启时点（秒）。仅跳攻节点用。")]
        public float landingDamageWindowStart;
        [Tooltip("跳攻触地攻 070 的命中框关闭时点（秒）。<=0 表示无。仅跳攻节点用。")]
        public float landingDamageWindowEnd;

        [Header("蓄力（重击）")]
        [Tooltip("勾选后此节点为可蓄力攻击。ER 接法：长按播蓄满 clip(本节点 clip=0500，前期有较长蓄力)；短按切直接出手 clip(quickAttackClip=0505)。")]
        public bool canCharge;
        [Tooltip("短按/未蓄满的直接出手 clip（ER 0505/0515）。空则没蓄满也用 clip(0500)。")]
        public AnimationClip quickAttackClip;
        [Tooltip("最短按住判定时长（秒）：起手后此时间内不判松手，给手柄 Hold 交互留识别窗，避免误判短按。")]
        public float chargeMinHoldTime;
        [Tooltip("蓄满提交时点（秒，按 0500 播放时间）：到此仍按住=蓄满；此前松手=短按切 0505。")]
        public float chargeCommitTime;
        [Tooltip("满蓄力释放时使用的攻击类型。")]
        public AttackType chargedAttackType;

        [Header("蓄力（旧三段链，已弃用，保留以兼容旧资产；canCharge 走上面 ER 接法）")]
        public AnimationClip chargeHold;
        public AnimationClip chargeRelease;
        public AnimationClip chargeFullRelease;

        [Header("连招转移（开窗内按输入跳转，-1=无后续）")]
        public int nextOnLight;
        public int nextOnHeavy;

        [Header("跳跃攻击落地融合（ER：空中攻 03x030 → 触地攻 03x070 → 落地恢复 03x071，由落地检测驱动）")]
        [Tooltip("空中维持 clip（a023_03x060，可空）：空中攻 clip 播完仍在高处时循环/下落维持，落地再切触地攻。空则回退通用 jumpIdle。")]
        public AnimationClip airHoldClip;
        [Tooltip("触地攻 clip（a023_03x070）：空中攻播放中/播完检测到快触地时切入，带自己的命中框。空则落地走旧通用 jumpEnd。")]
        public AnimationClip landingAttackClip;
        [Tooltip("触地攻的攻击类型（命中框伤害）。")]
        public AttackType landingAttackType;
        [Tooltip("落地恢复 clip（a023_03x071，可空）：短距离落地 + 前序未到尾声时的收招段，无命中框。也是其余落地恢复缺省时的回退。空则触地攻播完直接收。")]
        public AnimationClip landingRecoveryClip;
        [Tooltip("落地恢复 clip（a023_03x072，可空）：长距离落地 + 前序未到尾声。空则回退 071。")]
        public AnimationClip landingRecoveryLongClip;
        [Tooltip("落地恢复 clip（a023_03x081，可空）：短距离落地 + 前序已接近尾声（更短的快速收招）。空则回退 071。")]
        public AnimationClip landingRecoveryFastClip;
        [Tooltip("落地恢复 clip（a023_03x082，可空）：长距离落地 + 前序已接近尾声。空则回退 072→071。")]
        public AnimationClip landingRecoveryFastLongClip;
        [Tooltip("落地下落高度阈值（米）：本次跳攻从空中最高点到落地点的下落高度 ≥ 此值视为「落得高/远」→ 选 072/082，否则「落得低/近」→ 选 071/081。<=0 时一律按低。")]
        public float landingLongFallThreshold;
        [Tooltip("前序动画快慢阈值（归一化 0~1）：触地时空中攻已播进度 ≥ 此值（或空中攻已播完进入维持）视为接近尾声，选快速恢复 081/082；否则选 071/072。<=0 时一律按非快速。")]
        public float landingFastProgressThreshold;
        [Tooltip("快触地预判距离（米）：离地小于此值即视为触地、提前切触地攻让冲击对齐。0=只用 isGrounded。")]
        public float landingLookahead;
    }

    /// <summary>
    /// 某一持武状态（单手/双手/双持）的完整招式图。
    /// nodes 是扁平节点数组；openers 与上下文攻击用 index 指向 nodes，-1 表示无该招。
    /// </summary>
    [Serializable]
    public class HandMoveset
    {
        public AttackNode[] nodes = Array.Empty<AttackNode>();

        [Header("起手节点（index 进入 nodes，-1=无）")]
        public int lightOpener = -1;
        public int heavyOpener = -1;

        [Header("上下文一次性攻击（index 进入 nodes，-1=无）")]
        public int runAttack = -1;
        public int rollAttack = -1;
        public int backstepAttack = -1;
        public int jumpLight = -1;
        public int jumpHeavy = -1;
        public int crouchAttack = -1;

        public bool HasNode(int index) => index >= 0 && nodes != null && index < nodes.Length;

        /// <summary>按 index 安全取节点；越界返回 false。</summary>
        public bool TryGetNode(int index, out AttackNode node)
        {
            if (HasNode(index)) { node = nodes[index]; return true; }
            node = default;
            return false;
        }
    }

    /// <summary>
    /// 数据驱动的武器招式集（ER 风格 moveset），替代 <see cref="WeaponAnimationSet"/> 的强类型攻击字段。
    /// 每个 HandState 一张 <see cref="HandMoveset"/> 图；移动动画沿用现有 <see cref="ClipOverride"/> 覆盖范式。
    /// <para/>
    /// 通常多把同类武器共用一个 MovesetData（如所有直剑）。
    /// </summary>
    [CreateAssetMenu(menuName = "ARPG/Moveset Data")]
    public class MovesetData : ScriptableObject
    {
        [Tooltip("角色基础动画数据（idle/移动混合树等回退来源）。")]
        public CharacterAnimationData baseAnimData;

        [Header("各持武状态招式图")]
        public HandMoveset oneHandRight = new HandMoveset();
        public HandMoveset twoHand = new HandMoveset();
        public HandMoveset dualWield = new HandMoveset();

        [Header("移动动画覆盖（沿用现有 locomotion override 范式）")]
        public ClipOverride[] locomotionOverrides = Array.Empty<ClipOverride>();

        /// <summary>按持武状态取对应招式图。</summary>
        public HandMoveset GetMoveset(HandState handState)
        {
            switch (handState)
            {
                case HandState.TwoHand: return twoHand;
                case HandState.DualWield: return dualWield;
                default: return oneHandRight;
            }
        }
    }
}
