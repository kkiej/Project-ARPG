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

        public virtual void AttemptToUseItem(PlayerManager player)
        {
            if (!CanIUseThisItem(player))
                return;

            player.playerAnimatorManager.PlayTargetActionAnimation(useItemAnimation, true);
        }

        public virtual bool CanIUseThisItem(PlayerManager player)
        {
            return true;
        }
    }
}
