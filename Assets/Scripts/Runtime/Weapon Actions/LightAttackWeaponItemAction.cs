using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Light Attack Action")]
    public class LightAttackWeaponItemAction : WeaponItemAction
    {
        //  MAIN HAND
        [Header("Light Attacks")]
        [SerializeField] string light_Attack_01 = "Main_Light_Attack_01";
        [SerializeField] string light_Attack_02 = "Main_Light_Attack_02";

        [Header("Running Attacks")]
        [SerializeField] string run_Attack_01 = "Main_Run_Attack_01";

        [Header("Rolling Attacks")]
        [SerializeField] string roll_Attack_01 = "Main_Roll_Attack_01";

        [Header("Backstep Attacks")]
        [SerializeField] string backstep_Attack_01 = "Main_Backstep_Attack_01";

        //  TWO HAND
        [Header("Light Attacks")]
        [SerializeField] string th_light_Attack_01 = "TH_Light_Attack_01";
        [SerializeField] string th_light_Attack_02 = "TH_Light_Attack_02";

        [Header("Running Attacks")]
        [SerializeField] string th_run_Attack_01 = "TH_Run_Attack_01";

        [Header("Rolling Attacks")]
        [SerializeField] string th_roll_Attack_01 = "TH_Roll_Attack_01";

        [Header("Backstep Attacks")]
        [SerializeField] string th_backstep_Attack_01 = "TH_Backstep_Attack_01";

        public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

            if (!playerPerformingAction.IsOwner)
                return;
            
            if (playerPerformingAction.playerNetworkManager.currentStamina.Value <= 0)
                return;

            if (!playerPerformingAction.characterLocomotionManager.isGrounded)
                return;

            if (playerPerformingAction.IsOwner)
                playerPerformingAction.playerNetworkManager.isAttacking.Value = true;

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

        private void PerformLightAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            if (playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
            {
                PerformTwoHandLightAttack(playerPerformingAction, weaponPerformingAction);
            }
            else
            {
                PerformMainHandLightAttack(playerPerformingAction, weaponPerformingAction);
            }
        }

        private void PerformMainHandLightAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            //  IF WE ARE ATTACKING CURRENTLY, AND WE CAN COMBO, PERFORM THE COMBO ATTACK
            if (playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon && playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon = false;

                //  PERFORM AN ATTACK BASED ON THE PREVIOUS ATTACK WE JUST PLAYED
                if (playerPerformingAction.characterCombatManager.lastAttackAnimationPerformed == light_Attack_01)
                {
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponPerformingAction, AttackType.LightAttack02, light_Attack_02, true);
                }
                else
                {
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponPerformingAction, AttackType.LightAttack01, light_Attack_01, true);
                }
            }
            // 否则，只播放常规攻击
            else if (!playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponPerformingAction, AttackType.LightAttack01, light_Attack_01, true);
            }
        }

        private void PerformTwoHandLightAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            //  IF WE ARE ATTACKING CURRENTLY, AND WE CAN COMBO, PERFORM THE COMBO ATTACK
            if (playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon && playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon = false;

                //  PERFORM AN ATTACK BASED ON THE PREVIOUS ATTACK WE JUST PLAYED
                if (playerPerformingAction.characterCombatManager.lastAttackAnimationPerformed == th_light_Attack_01)
                {
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponPerformingAction, AttackType.LightAttack02, th_light_Attack_02, true);
                }
                else
                {
                    playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponPerformingAction, AttackType.LightAttack01, th_light_Attack_01, true);
                }
            }
            //  OTHERWISE, IF WE ARE NOT ALREADY ATTACKING JUST PERFORM A REGULAR ATTACK
            else if (!playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponPerformingAction, AttackType.LightAttack01, th_light_Attack_01, true);
            }
        }
        
        private void PerformRunningAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            if (playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponPerformingAction, AttackType.RunningAttack01, th_run_Attack_01, true);
            }
            else
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponPerformingAction, AttackType.RunningAttack01, run_Attack_01, true);
            }
        }

        private void PerformRollingAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            // 若双手持武，则执行双手奔跑攻击（待实现）
            // 否则执行单手奔跑攻击
            playerPerformingAction.playerCombatManager.canPerformRollingAttack = false;

            if (playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponPerformingAction, AttackType.RollingAttack01, th_roll_Attack_01, true);
            }
            else
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponPerformingAction, AttackType.RollingAttack01, roll_Attack_01, true);
            }
        }

        private void PerformBackstepAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            // 若双手持武，则执行双手奔跑攻击（待实现）
            // 否则执行单手奔跑攻击
            playerPerformingAction.playerCombatManager.canPerformBackstepAttack = false;

            if (playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponPerformingAction, AttackType.BackstepAttack01, th_backstep_Attack_01, true);
            }
            else
            {
                playerPerformingAction.playerAnimatorManager.PlayTargetAttackActionAnimation(weaponPerformingAction, AttackType.BackstepAttack01, backstep_Attack_01, true);
            }
        }
    }
}