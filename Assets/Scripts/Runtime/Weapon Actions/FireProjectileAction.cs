using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Fire Projectile Action")]
    public class FireProjectileAction : WeaponItemAction
    {
        [SerializeField] ProjectileSlot projectileSlot;

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

            RangedProjectileItem projectileItem = null;

            //  1. Define which projectile we are using (Main projectile slot, Secondary projectile slot)
            switch (projectileSlot)
            {
                case ProjectileSlot.Main: 
                    projectileItem = playerPerformingAction.playerInventoryManager.mainProjectile;
                    break;
                case ProjectileSlot.Secondary:
                    projectileItem = playerPerformingAction.playerInventoryManager.secondaryProjectile;
                    break;
                default:
                    break;
            }

            //  2. If that projectile == null, return
            if (projectileItem == null)
                return;

            if (!playerPerformingAction.IsOwner)
                return;

            //  3. If the player is not two handing the weapon, make them two hand it now (Weapon must be two handed to fire projectile)
            if (!playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
            {
                if (playerPerformingAction.playerNetworkManager.isUsingRightHand.Value)
                {
                    playerPerformingAction.playerNetworkManager.isTwoHandingRightWeapon.Value = true;
                }
                else if (playerPerformingAction.playerNetworkManager.isUsingLeftHand.Value)
                {
                    playerPerformingAction.playerNetworkManager.isTwoHandingLeftWeapon.Value = true;
                }
            }

            //  4. If the player does not have an arrow notched, do so now
            if (!playerPerformingAction.playerNetworkManager.hasArrowNotched.Value)
            {
                playerPerformingAction.playerNetworkManager.hasArrowNotched.Value = true;

                bool canIDrawAProjectile = CanIFireThisProjectile(weaponPerformingAction, projectileItem);

                if (!canIDrawAProjectile)
                    return;

                if (projectileItem.currentAmmoAmount <= 0)
                {
                    //  PLAY A SILLY ANIMATION HERE THAT INDICATES THEY ARE OUT OF AMMO
                    playerPerformingAction.playerAnimatorManager.PlayTargetActionAnimation("Out_Of_Ammo_01", true);
                    return;
                }

                playerPerformingAction.playerCombatManager.currentProjectileBeingUsed = projectileSlot;
                playerPerformingAction.playerAnimatorManager.PlayTargetActionAnimation("Bow_Draw_01", true);
                playerPerformingAction.playerNetworkManager.NotifyServerOfDrawnProjectileServerRpc(projectileItem.itemID);
            }
        }

        private bool CanIFireThisProjectile(WeaponItem weaponPerformingAction, RangedProjectileItem projectileItem)
        {
            //  CHECK FOR CROSS BOW GREAT BOWS ECT AND COMPARE AMMO TO GIVE RESULT

            return true;
        }
    }
}
