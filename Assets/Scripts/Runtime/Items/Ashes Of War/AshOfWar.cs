using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class AshOfWar : Item
    {
        [Header("Ash Of War Information")]
        public WeaponClass[] usableWeaponClasses;

        [Header("Costs")]
        public int focusPointCost = 20;
        public int staminaCost = 20;

        //  THE FUNCTION ATTEMPTING TO PERFORM THE ASH OF WAR
        public virtual void AttemptToPerformAction(PlayerManager playerPerformingAction)
        {
            Debug.Log("PERFORMED!");
        }

        //  A HELPER FUNCTION USED TO DETERMINE IF WE CAN IN THIS MOMENT USE THIS ASH OF WAR
        public virtual bool CanIUseThisAbility(PlayerManager playerPerformingAction)
        {
            return false;
        }

        protected virtual void DeductStaminaCost(PlayerManager playerPerformingAction)
        {
            playerPerformingAction.playerNetworkManager.currentStamina.Value -= staminaCost;
        }

        protected virtual void DeductFocusPointCost(PlayerManager playerPerformingAction)
        {

        }
    }
}
