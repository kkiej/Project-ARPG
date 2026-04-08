using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Items/Ash Of War/Parry")]
    public class ParryAshOfWar : AshOfWar
    {
        public override void AttemptToPerformAction(PlayerManager playerPerformingAction)
        {
            base.AttemptToPerformAction(playerPerformingAction);

            if (!CanIUseThisAbility(playerPerformingAction))
                return;

            DeductStaminaCost(playerPerformingAction);
            DeductFocusPointCost(playerPerformingAction);
            PerformParryTypeBasedOnWeapon(playerPerformingAction);
        }

        public override bool CanIUseThisAbility(PlayerManager playerPerformingAction)
        {
            if (playerPerformingAction.isPerformingAction)
            {
                Debug.Log("CANNOT PERFORM ASH OF WAR: YOU ARE ALREADY PERFORMING AN ACTION");
                return false;
            }

            if (playerPerformingAction.playerNetworkManager.isJumping.Value)
            {
                Debug.Log("CANNOT PERFORM ASH OF WAR: YOU ARE JUMPING");
                return false;
            }

            if (!playerPerformingAction.playerLocomotionManager.isGrounded)
            {
                Debug.Log("CANNOT PERFORM ASH OF WAR: YOU ARE NOT GROUNDED");
                return false;
            }

            if (playerPerformingAction.playerNetworkManager.currentStamina.Value <= 0)
            {
                Debug.Log("CANNOT PERFORM ASH OF WAR: OUT OF STAMINA");
                return false;
            }

            return true;
        }

        //  SMALLER WEAPONS PERFORM FASTER PARRIES
        private void PerformParryTypeBasedOnWeapon(PlayerManager playerPerformingAction)
        {
            WeaponItem weaponBeingUsed = playerPerformingAction.playerCombatManager.currentWeaponBeingUsed;

            var ad = playerPerformingAction.playerAnimatorManager.animData;

            switch (weaponBeingUsed.weaponClass)
            {
                case WeaponClass.StraightSword:
                    break;
                case WeaponClass.Spear:
                    break;
                case WeaponClass.MediumShield:
                    if (ad != null && ad.slowParry != null)
                        playerPerformingAction.playerAnimatorManager.PlayTargetActionAnimation(ad.slowParry, true);
                    else
                        Debug.LogWarning($"{playerPerformingAction.name}: slowParry clip 未配置", playerPerformingAction);
                    break;
                case WeaponClass.Fist:
                    break;
                case WeaponClass.LightShield:
                    if (ad != null && ad.fastParry != null)
                        playerPerformingAction.playerAnimatorManager.PlayTargetActionAnimation(ad.fastParry, true);
                    else
                        Debug.LogWarning($"{playerPerformingAction.name}: fastParry clip 未配置", playerPerformingAction);
                    break;
                default:
                    break;
            }
        }
    }
}