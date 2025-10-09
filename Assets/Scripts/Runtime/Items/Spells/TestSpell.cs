using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Items/Spells/Test Spell")]
    public class TestSpell : SpellItem
    {
        public override void AttemptToCastSpell(PlayerManager player)
        {
            base.AttemptToCastSpell(player);

            if (!CanICastThisSpell(player))
                return;

            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                player.playerAnimatorManager.PlayTargetActionAnimation(mainHandSpellAnimation, true);
            }
            else
            {
                player.playerAnimatorManager.PlayTargetActionAnimation(offHandSpellAnimation, true);
            }
        }

        public override void SuccessfullyCastSpell(PlayerManager player)
        {
            base.SuccessfullyCastSpell(player);

            Debug.Log("CASTED SPELL");
        }

        public override void InstantiateWarmUpSpellFX(PlayerManager player)
        {
            base.InstantiateWarmUpSpellFX(player);

            Debug.Log("INSTANTIATED FX");
        }

        public override void InstantiateReleaseFX(PlayerManager player)
        {
            base.InstantiateReleaseFX(player);

            Debug.Log("INSTANTIATED PROJECTILE");
        }

        public override bool CanICastThisSpell(PlayerManager player)
        {
            if (player.isPerformingAction)
                return false;

            if (player.playerNetworkManager.isJumping.Value)
                return false;

            if (player.playerNetworkManager.currentStamina.Value <= 0)
                return false;

            return true;
        }
    }
}
