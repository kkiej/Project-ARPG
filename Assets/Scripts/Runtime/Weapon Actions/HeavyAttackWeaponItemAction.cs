using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Heavy Attack Action")]
    public class HeavyAttackWeaponItemAction : WeaponItemAction
    {
        public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

            if (!playerPerformingAction.IsOwner)
                return;

            if (playerPerformingAction.playerNetworkManager.currentStamina.Value <= 0)
                return;

            //  IF WE ARE USING AN ITEM, DO NOT PROCEED
            if (playerPerformingAction.playerCombatManager.isUsingItem)
                return;

            //  IF WE ARE IN THE AIR, PERFORM A JUMPING/AERIAL ATTACK
            if (!playerPerformingAction.characterLocomotionManager.isGrounded)
            {
                PerformJumpingHeavyAttack(playerPerformingAction, weaponPerformingAction);
                return;
            }

            //  STOPS US FROM STARTING A HEAVY ATTACK JUST AS WE JUMP IF TIMED PERFECTLY
            if (playerPerformingAction.playerNetworkManager.isJumping.Value)
                return;

            PerformHeavyAttack(playerPerformingAction, weaponPerformingAction);
        }

        private void PerformHeavyAttack(PlayerManager playerPerformingAction, WeaponItem weapon)
        {
            if (playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
                PerformTwoHandHeavyAttack(playerPerformingAction, weapon);
            else
                PerformMainHandHeavyAttack(playerPerformingAction, weapon);
        }

        private void PerformMainHandHeavyAttack(PlayerManager playerPerformingAction, WeaponItem weapon)
        {
            var clips = weapon.weaponAnimationSet;
            //  IF WE ARE ATTACKING CURRENTLY, AND WE CAN COMBO, PERFORM THE COMBO ATTACK
            if (playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon && playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon = false;

                //  PERFORM AN ATTACK BASED ON THE PREVIOUS ATTACK WE JUST PLAYED
                if (playerPerformingAction.characterCombatManager.lastAttackClipPerformed == clips.heavyAttack01)
                {
                    playerPerformingAction.playerAnimatorManager.PlayHeavyAttackChainAnimation(
                        weapon, AttackType.HeavyAttack02, AttackType.ChargedAttack02,
                        clips.heavyAttack02, clips.heavyAttack02Hold,
                        clips.heavyAttack02Release, clips.heavyAttack02FullRelease, true);
                }
                else
                {
                    playerPerformingAction.playerAnimatorManager.PlayHeavyAttackChainAnimation(
                        weapon, AttackType.HeavyAttack01, AttackType.ChargedAttack01,
                        clips.heavyAttack01, clips.heavyAttack01Hold,
                        clips.heavyAttack01Release, clips.heavyAttack01FullRelease, true);
                }
            }
            //  OTHERWISE, IF WE ARE NOT ALREADY ATTACKING JUST PERFORM A REGULAR ATTACK
            else if (!playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerAnimatorManager.PlayHeavyAttackChainAnimation(
                    weapon, AttackType.HeavyAttack01, AttackType.ChargedAttack01,
                    clips.heavyAttack01, clips.heavyAttack01Hold,
                    clips.heavyAttack01Release, clips.heavyAttack01FullRelease, true);
            }
        }

        private void PerformTwoHandHeavyAttack(PlayerManager playerPerformingAction, WeaponItem weapon)
        {
            var clips = weapon.weaponAnimationSet;
            //  IF WE ARE ATTACKING CURRENTLY, AND WE CAN COMBO, PERFORM THE COMBO ATTACK
            if (playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon && playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon = false;

                // 基于上一个播放的攻击来决定播放哪个攻击动画
                if (playerPerformingAction.characterCombatManager.lastAttackClipPerformed == clips.th_heavyAttack01)
                {
                    playerPerformingAction.playerAnimatorManager.PlayHeavyAttackChainAnimation(
                        weapon, AttackType.HeavyAttack02, AttackType.ChargedAttack02,
                        clips.th_heavyAttack02, clips.th_heavyAttack02Hold,
                        clips.th_heavyAttack02Release, clips.th_heavyAttack02FullRelease, true);
                }
                else
                {
                    playerPerformingAction.playerAnimatorManager.PlayHeavyAttackChainAnimation(
                        weapon, AttackType.HeavyAttack01, AttackType.ChargedAttack01,
                        clips.th_heavyAttack01, clips.th_heavyAttack01Hold,
                        clips.th_heavyAttack01Release, clips.th_heavyAttack01FullRelease, true);
                }
            }
            // 否则，只播放常规攻击
            else if (!playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerAnimatorManager.PlayHeavyAttackChainAnimation(
                    weapon, AttackType.HeavyAttack01, AttackType.ChargedAttack01,
                    clips.th_heavyAttack01, clips.th_heavyAttack01Hold,
                    clips.th_heavyAttack01Release, clips.th_heavyAttack01FullRelease, true);
            }
        }

        private void PerformJumpingHeavyAttack(PlayerManager playerPerformingAction, WeaponItem weapon)
        {
            if (playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
                PerformTwoHandJumpingHeavyAttack(playerPerformingAction, weapon);
            else
                PerformMainHandJumpingHeavyAttack(playerPerformingAction, weapon);
        }

        private void PerformMainHandJumpingHeavyAttack(PlayerManager playerPerformingAction, WeaponItem weapon)
        {
            if (playerPerformingAction.isPerformingAction)
                return;

            var clips = weapon.weaponAnimationSet;
            playerPerformingAction.playerAnimatorManager.PlayJumpAttackSequenceAnimation(
                weapon, AttackType.HeavyJumpingAttack01,
                clips.heavyJumpAttack01, clips.heavyJumpAttackIdle, clips.heavyJumpAttackEnd, true);
        }

        private void PerformTwoHandJumpingHeavyAttack(PlayerManager playerPerformingAction, WeaponItem weapon)
        {
            if (playerPerformingAction.isPerformingAction)
                return;

            var clips = weapon.weaponAnimationSet;
            playerPerformingAction.playerAnimatorManager.PlayJumpAttackSequenceAnimation(
                weapon, AttackType.HeavyJumpingAttack01,
                clips.th_heavyJumpAttack01, clips.th_heavyJumpAttackIdle, clips.th_heavyJumpAttackEnd, true);
        }
    }
}
