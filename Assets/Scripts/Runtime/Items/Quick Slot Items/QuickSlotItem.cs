using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class QuickSlotItem : Item
    {
        [Header("Item Model")]
        [SerializeField] protected GameObject itemModel;

        [Header("Animation")]
        [SerializeField] protected string useItemAnimation;

        //  NOT ALL QUICK SLOT ITEMS ARE CONSUMABLES
        [Header("Consumable")]
        public bool isConsumable = true;

        public virtual void AttemptToUseItem(PlayerManager player)
        {
            if (!CanIUseThisItem(player))
                return;

            player.playerAnimatorManager.PlayTargetActionAnimation(useItemAnimation, true);
        }

        public virtual void SuccessfullyUseItem(PlayerManager player)
        {

        }

        public virtual bool CanIUseThisItem(PlayerManager player)
        {
            return true;
        }

        public virtual int GetCurrentAmount(PlayerManager player)
        {
            return 0;
        }
    }
}
