using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Items/Consumeables/Flask")]
    public class FlaskItem : QuickSlotItem
    {
        [Header("Empty Item")]
        [SerializeField] GameObject emptyFlaskItem;

        public override void AttemptToUseItem(PlayerManager player)
        {
            if (!CanIUseThisItem(player))
                return;

            player.playerEffectsManager.activeQuickSlotItemFX = Instantiate(itemModel, player.playerEquipmentManager.rightHandWeaponSlot.transform);

            player.playerAnimatorManager.PlayTargetActionAnimation(useItemAnimation, true, false, true, true, false);
        }
    }
}
