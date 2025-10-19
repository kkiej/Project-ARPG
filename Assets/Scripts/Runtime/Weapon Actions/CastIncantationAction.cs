using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Incantation Action")]
    public class CastIncantationAction : WeaponItemAction
    {
        public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

            if (!playerPerformingAction.IsOwner)
                return;

            //  IF WE ARE USING AN ITEM, DO NOT PROCEED
            if (playerPerformingAction.playerCombatManager.isUsingItem)
                return;

            if (playerPerformingAction.playerNetworkManager.currentStamina.Value <= 0)
                return;

            if (!playerPerformingAction.characterLocomotionManager.isGrounded)
                return;

            if (playerPerformingAction.playerInventoryManager.currentSpell == null)
                return;

            if (playerPerformingAction.playerInventoryManager.currentSpell.SpellClass != SpellClass.Incantation)
                return;

            if (playerPerformingAction.IsOwner)
                playerPerformingAction.playerNetworkManager.isAttacking.Value = true;

            CastIncantation(playerPerformingAction, weaponPerformingAction);
        }

        private void CastIncantation(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
        {
            playerPerformingAction.playerInventoryManager.currentSpell.AttemptToCastSpell(playerPerformingAction);
        }
    }
}
