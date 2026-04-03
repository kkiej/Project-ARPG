using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 每把武器一个，替代 AnimatorOverrideController。
    /// 保存该武器所有攻击动画的 AnimationClip 引用。
    /// </summary>
    [CreateAssetMenu(menuName = "Items/Weapon Animation Set")]
    public class WeaponAnimationSet : ScriptableObject
    {
        [Header("Main Hand — Light Attacks")]
        public AnimationClip lightAttack01;
        public AnimationClip lightAttack02;
        public AnimationClip lightJumpAttack01;
        public AnimationClip runAttack01;
        public AnimationClip rollAttack01;
        public AnimationClip backstepAttack01;

        [Header("Main Hand — Heavy Attacks (含蓄力链)")]
        public AnimationClip heavyAttack01;
        public AnimationClip heavyAttack01Hold;
        public AnimationClip heavyAttack01Release;
        public AnimationClip heavyAttack01FullRelease;
        public AnimationClip heavyAttack02;
        public AnimationClip heavyAttack02Hold;
        public AnimationClip heavyAttack02Release;
        public AnimationClip heavyAttack02FullRelease;

        [Header("Main Hand — Jump Attacks")]
        public AnimationClip heavyJumpAttack01;
        public AnimationClip heavyJumpAttackIdle;
        public AnimationClip heavyJumpAttackEnd;

        [Header("Two Hand — Light Attacks")]
        public AnimationClip th_lightAttack01;
        public AnimationClip th_lightAttack02;
        public AnimationClip th_lightJumpAttack01;
        public AnimationClip th_runAttack01;
        public AnimationClip th_rollAttack01;
        public AnimationClip th_backstepAttack01;

        [Header("Two Hand — Heavy Attacks (含蓄力链)")]
        public AnimationClip th_heavyAttack01;
        public AnimationClip th_heavyAttack01Hold;
        public AnimationClip th_heavyAttack01Release;
        public AnimationClip th_heavyAttack01FullRelease;
        public AnimationClip th_heavyAttack02;
        public AnimationClip th_heavyAttack02Hold;
        public AnimationClip th_heavyAttack02Release;
        public AnimationClip th_heavyAttack02FullRelease;

        [Header("Two Hand — Jump Attacks")]
        public AnimationClip th_heavyJumpAttack01;
        public AnimationClip th_heavyJumpAttackIdle;
        public AnimationClip th_heavyJumpAttackEnd;

        [Header("Dual Wield — Attacks")]
        public AnimationClip dw_Attack01;
        public AnimationClip dw_Attack02;
        public AnimationClip dw_RunAttack01;
        public AnimationClip dw_RollAttack01;
        public AnimationClip dw_BackstepAttack01;

        [Header("Dual Wield — Jump Attacks")]
        public AnimationClip dw_JumpAttack01;
        public AnimationClip dw_JumpAttackIdle;
        public AnimationClip dw_JumpAttackEnd;
    }
}
