using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Aim Action")]
    public class AimAction : WeaponItemAction
    {
        public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

            //  IF WE ARE NOT GROUNDED, DO NOT PROCEED
            if (!playerPerformingAction.playerLocomotionManager.isGrounded)
                return;

            //  IF WE ARE JUMPING, DO NOT PROCEED
            if (playerPerformingAction.playerNetworkManager.isJumping.Value)
                return;

            //  IF WE ARE ROLLING, DO NOT PROCEED
            if (playerPerformingAction.playerLocomotionManager.isRolling)
                return;

            //  IF WE ARE LOCKED ON, DO NOT PROCEED
            if (playerPerformingAction.playerNetworkManager.isLockedOn.Value)
                return;

            //  IF WE ARE USING AN ITEM, DO NOT PROCEED
            if (playerPerformingAction.playerCombatManager.isUsingItem)
                return;

            if (playerPerformingAction.IsOwner)
            {
                //  TWO HAND THE WEAPON (BOW) BEFORE WE AIM
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

                playerPerformingAction.playerNetworkManager.isAiming.Value = true;
            }
        }
    }
}
