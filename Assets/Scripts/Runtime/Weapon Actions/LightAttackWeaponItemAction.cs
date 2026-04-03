using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Light Attack Action")]
    public class LightAttackWeaponItemAction : WeaponItemAction
    {
        public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

            if (!playerPerformingAction.IsOwner)
                return;

            if (playerPerformingAction.playerCombatManager.isUsingItem)
                return;

            if (playerPerformingAction.playerNetworkManager.currentStamina.Value <= 0)
                return;

            //  IF WE ARE IN THE AIR, PERFORM A JUMPING/AERIAL ATTACK
            if (!playerPerformingAction.characterLocomotionManager.isGrounded)
            {
                PerformJumpingLightAttack(playerPerformingAction, weaponPerformingAction);
                return;
            }

            if (playerPerformingAction.playerNetworkManager.isJumping.Value)
                return;

            // 如果我们正在冲刺，播放冲刺攻击
            if (playerPerformingAction.characterNetworkManager.isSprinting.Value)
            {
                PerformRunningAttack(playerPerformingAction, weaponPerformingAction);
                return;
            }
            
            // 如果我们正在翻滚，播放翻滚攻击
            if (playerPerformingAction.characterCombatManager.canPerformRollingAttack)
            {
                PerformRollingAttack(playerPerformingAction, weaponPerformingAction);
                return;
            }
            
            // 如果我们正在后撤步，播放后撤步攻击
            if (playerPerformingAction.characterCombatManager.canPerformBackstepAttack)
            {
                PerformBackstepAttack(playerPerformingAction, weaponPerformingAction);
                return;
            }

            playerPerformingAction.characterCombatManager.AttemptCriticalAttack();

            PerformLightAttack(playerPerformingAction, weaponPerformingAction);
        }

        private void PerformLightAttack(PlayerManager playerPerformingAction, WeaponItem weapon)
        {
            if (playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
                PerformTwoHandLightAttack(playerPerformingAction, weapon);
            else
                PerformMainHandLightAttack(playerPerformingAction, weapon);
        }

        private void PerformMainHandLightAttack(PlayerManager playerPerformingAction, WeaponItem weapon)
        {
            var clips = weapon.weaponAnimationSet;

            //  IF WE ARE ATTACKING CURRENTLY, AND WE CAN COMBO, PERFORM THE COMBO ATTACK
            if (playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon && playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon = false;

                //  PERFORM AN ATTACK BASED ON THE PREVIOUS ATTACK WE JUST PLAYED
                if (playerPerformingAction.characterCombatManager.lastAttackClipPerformed == clips.lightAttack01)
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.LightAttack02, clips.lightAttack02, true);
                else
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.LightAttack01, clips.lightAttack01, true);
            }
            // 否则，只播放常规攻击
            else if (!playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.LightAttack01, clips.lightAttack01, true);
            }
        }

        private void PerformTwoHandLightAttack(PlayerManager playerPerformingAction, WeaponItem weapon)
        {
            var clips = weapon.weaponAnimationSet;

            //  IF WE ARE ATTACKING CURRENTLY, AND WE CAN COMBO, PERFORM THE COMBO ATTACK
            if (playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon && playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon = false;

                //  PERFORM AN ATTACK BASED ON THE PREVIOUS ATTACK WE JUST PLAYED
                if (playerPerformingAction.characterCombatManager.lastAttackClipPerformed == clips.th_lightAttack01)
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.LightAttack02, clips.th_lightAttack02, true);
                else
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.LightAttack01, clips.th_lightAttack01, true);
            }
            //  OTHERWISE, IF WE ARE NOT ALREADY ATTACKING JUST PERFORM A REGULAR ATTACK
            else if (!playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.LightAttack01, clips.th_lightAttack01, true);
            }
        }

        private void PerformRunningAttack(PlayerManager playerPerformingAction, WeaponItem weapon)
        {
            var clips = weapon.weaponAnimationSet;
            if (playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.RunningAttack01, clips.th_runAttack01, true);
            else
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.RunningAttack01, clips.runAttack01, true);
        }

        private void PerformRollingAttack(PlayerManager playerPerformingAction, WeaponItem weapon)
        {
            var clips = weapon.weaponAnimationSet;
            playerPerformingAction.playerCombatManager.canPerformRollingAttack = false;

            if (playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.RollingAttack01, clips.th_rollAttack01, true);
            else
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.RollingAttack01, clips.rollAttack01, true);
        }

        private void PerformBackstepAttack(PlayerManager playerPerformingAction, WeaponItem weapon)
        {
            var clips = weapon.weaponAnimationSet;
            playerPerformingAction.playerCombatManager.canPerformBackstepAttack = false;

            if (playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.BackstepAttack01, clips.th_backstepAttack01, true);
            else
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weapon, AttackType.BackstepAttack01, clips.backstepAttack01, true);
        }

        private void PerformJumpingLightAttack(PlayerManager playerPerformingAction, WeaponItem weapon)
        {
            if (playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
                PerformTwoHandJumpingLightAttack(playerPerformingAction, weapon);
            else
                PerformMainHandJumpingLightAttack(playerPerformingAction, weapon);
        }

        private void PerformMainHandJumpingLightAttack(PlayerManager playerPerformingAction, WeaponItem weapon)
        {
            if (playerPerformingAction.isPerformingAction)
                return;

            var animData = playerPerformingAction.characterAnimatorManager.animData;
            playerPerformingAction.playerAnimatorManager.PlayJumpAttackSequenceAnimation(
                weapon, AttackType.LightJumpingAttack01,
                weapon.weaponAnimationSet.lightJumpAttack01,
                animData != null ? animData.jumpIdle : null,
                animData != null ? animData.jumpEnd : null,
                true);
        }

        private void PerformTwoHandJumpingLightAttack(PlayerManager playerPerformingAction, WeaponItem weapon)
        {
            if (playerPerformingAction.isPerformingAction)
                return;

            var animData = playerPerformingAction.characterAnimatorManager.animData;
            playerPerformingAction.playerAnimatorManager.PlayJumpAttackSequenceAnimation(
                weapon, AttackType.LightJumpingAttack01,
                weapon.weaponAnimationSet.th_lightJumpAttack01,
                animData != null ? animData.jumpIdle : null,
                animData != null ? animData.jumpEnd : null,
                true);
        }
    }
}
