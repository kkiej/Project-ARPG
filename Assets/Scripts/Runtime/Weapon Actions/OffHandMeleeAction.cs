using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Off Hand Melee Action")]
    public class OffHandMeleeAction : WeaponItemAction
    {
        public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

            // 检查是否触发强力姿态动作（双武器攻击）
            if (playerPerformingAction.playerNetworkManager.isUsingLeftHand.Value && !playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
            {
                if (playerPerformingAction.playerInventoryManager.currentRightHandWeapon.weaponClass 
                    == playerPerformingAction.playerInventoryManager.currentLeftHandWeapon.weaponClass)
                {
                    //  PERFORM A POWER STANCE ACTION
                    PerformPowerStanceLeftHandAction(playerPerformingAction, weaponPerformingAction);
                    return;
                }
            }

            // 检查是否可格挡
            if (!playerPerformingAction.playerCombatManager.canBlock)
                return;

            //  IF WE ARE USING AN ITEM, DO NOT PROCEED
            if (playerPerformingAction.playerCombatManager.isUsingItem)
                return;

            //  CHECK FOR ATTACK STATUS
            if (playerPerformingAction.playerNetworkManager.isAttacking.Value)
            {
                // 禁用格挡（使用短矛/中矛时允许在轻攻击期间进行格挡反击。此逻辑由其他动作类处理）
                if (playerPerformingAction.IsOwner)
                    playerPerformingAction.playerNetworkManager.isBlocking.Value = false;

                return;
            }

            if (playerPerformingAction.playerNetworkManager.isBlocking.Value)
                return;

            if (playerPerformingAction.IsOwner)
                playerPerformingAction.playerNetworkManager.isBlocking.Value = true;
        }

        private void PerformPowerStanceLeftHandAction(PlayerManager playerPerformingAction, WeaponItem weapon)
        {
            var clips = weapon.weaponAnimationSet;

            if (playerPerformingAction.playerNetworkManager.currentStamina.Value <= 0)
                return;

            //  PERFORM DUAL WIELD JUMPING ATTACK
            if (!playerPerformingAction.playerLocomotionManager.isGrounded)
            {
                if (playerPerformingAction.isPerformingAction)
                    return;

                if (playerPerformingAction.IsOwner)
                {
                    playerPerformingAction.playerAnimatorManager.PlayJumpAttackSequenceAnimation(
                        weapon, AttackType.DualJumpAttack,
                        clips.dw_JumpAttack01, clips.dw_JumpAttackIdle, clips.dw_JumpAttackEnd, true);
                }

                return;
            }

            if (playerPerformingAction.playerNetworkManager.isJumping.Value)
                return;

            if (playerPerformingAction.playerCombatManager.canPerformRollingAttack)
            {
                playerPerformingAction.playerCombatManager.canPerformRollingAttack = false;

                if (playerPerformingAction.IsOwner)
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.DualRollAttack, clips.dw_RollAttack01, true);

                return;
            }

            if (playerPerformingAction.playerCombatManager.canPerformBackstepAttack)
            {
                playerPerformingAction.playerCombatManager.canPerformBackstepAttack = false;

                if (playerPerformingAction.IsOwner)
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.DualBackstepAttack, clips.dw_BackstepAttack01, true);

                return;
            }

            if (playerPerformingAction.playerNetworkManager.isSprinting.Value)
            {
                if (playerPerformingAction.IsOwner)
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.DualRunAttack, clips.dw_RunAttack01, true);

                return;
            }

            if (playerPerformingAction.playerCombatManager.canComboWithOffHandWeapon && playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerCombatManager.canComboWithOffHandWeapon = false;

                if (playerPerformingAction.characterCombatManager.lastAttackClipPerformed == clips.dw_Attack01)
                {
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.DualAttack02, clips.dw_Attack02, true);
                }
                else if (playerPerformingAction.characterCombatManager.lastAttackClipPerformed == clips.dw_Attack02)
                {
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.DualAttack01, clips.dw_Attack01, true);
                }
                else
                {
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.DualAttack01, clips.dw_Attack01, true);
                }
            }
            else if (!playerPerformingAction.playerCombatManager.canComboWithOffHandWeapon && !playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.DualAttack01, clips.dw_Attack01, true);
            }
        }
    }
}
