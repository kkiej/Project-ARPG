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

        [Header("连招开窗（秒，来自 ER TAE 的 Input-Common(flag87) 窗口）")]
        [Tooltip("可输入下一段的窗口起点（秒，按 clip 播放时间）。由 MovesetAutoFiller 从 a23.json 回填。")]
        public float comboWindowStart;
        [Tooltip("可输入下一段的窗口终点（秒）。<=0 表示无 TAE 数据，PlayerAttackState 回退到归一化 [0.35,0.95]。")]
        public float comboWindowEnd;

        [Header("蓄力（重击）")]
        [Tooltip("勾选后此节点为可蓄力攻击，使用下面三个 clip 组成蓄力链。")]
        public bool canCharge;
        public AnimationClip chargeHold;
        public AnimationClip chargeRelease;
        public AnimationClip chargeFullRelease;
        [Tooltip("满蓄力释放时使用的攻击类型。")]
        public AttackType chargedAttackType;

        [Header("连招转移（开窗内按输入跳转，-1=无后续）")]
        public int nextOnLight;
        public int nextOnHeavy;
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
