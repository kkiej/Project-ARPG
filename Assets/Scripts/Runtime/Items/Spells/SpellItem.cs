using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class SpellItem : Item
    {
        [Header("Spell Class")]
        public SpellClass SpellClass;

        [Header("Spell Modifiers")]
        public float fullChargeEffectMultiplier = 2;

        [Header("Spell Costs")]
        public int spellSlotsUsed = 1;
        public int staminaCost = 25;
        public int focusPointCost = 25;

        [Header("Spell FX")]
        [SerializeField] protected GameObject spellCastWarmUpFX;
        [SerializeField] protected GameObject spellChargeFX;
        [SerializeField] protected GameObject spellCastReleaseFX;
        [SerializeField] protected GameObject spellCastReleaseFXFullCharge;
        //  FULL CHARGE VERSION OF FX (TO DO)

        [Header("Animations")]
        [SerializeField] protected string mainHandSpellAnimation;
        [SerializeField] protected string offHandSpellAnimation;

        [Header("Sound FX")]
        public AudioClip warmUpSoundFX;
        public AudioClip releaseSoundFX;

        //  THIS IS WHERE YOU PLAY THE "WARM UP" ANIMATION
        public virtual void AttemptToCastSpell(PlayerManager player)
        {

        }

        //  SPELL FX THAT ARE INSTANTIATED WHEN ATTEMPTING TO CAST THE SPELL
        public virtual void InstantiateWarmUpSpellFX(PlayerManager player)
        {

        }

        //  THIS IS WHERE SPELL PROJECTS/FX ARE ACTIVATED
        public virtual void SuccessfullyCastSpell(PlayerManager player)
        {
            if (player.IsOwner)
            {
                player.playerNetworkManager.currentFocusPoints.Value -= focusPointCost;
                player.playerNetworkManager.currentStamina.Value -= staminaCost;
            }
        }

        public virtual void SuccessfullyChargeSpell(PlayerManager player)
        {

        }

        public virtual void SuccessfullyCastSpellFullCharge(PlayerManager player)
        {
            if (player.IsOwner)
            {
                player.playerNetworkManager.currentFocusPoints.Value -= Mathf.RoundToInt(focusPointCost * fullChargeEffectMultiplier);
                player.playerNetworkManager.currentStamina.Value -= staminaCost * fullChargeEffectMultiplier; ;
            }
        }

        //  HELPER FUNCTION TO CHECK WEATHER OR NOT WE ARE ABLE TO USE THE SPELL WHEN ATTEMPTING TO CAST
        public virtual bool CanICastThisSpell(PlayerManager player)
        {
            if (player.playerNetworkManager.currentFocusPoints.Value <= focusPointCost)
                return false;

            if (player.playerNetworkManager.currentStamina.Value <= staminaCost)
                return false;

            if (player.isPerformingAction)
                return false;

            if (player.playerNetworkManager.isJumping.Value)
                return false;

            return true;
        }
    }
}
