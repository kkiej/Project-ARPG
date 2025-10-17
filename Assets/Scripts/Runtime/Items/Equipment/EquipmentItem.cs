using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class EquipmentItem : Item
    {
        [Header("Item Weight")]
        public float itemWeight;

        //  BONUS
        //  LIST OF EFFECTS HERE THAT SPECIAL ITEMS CAN HAVE

        //  public void OnItemEquipped(PlayerManager player) - Add all the effects from the list above to the player's effect manager. Call this function when this item equips
        //  public void OnItemUnEquipped(PlayerManager player) - Remove all the effects that were added. Call this function when this item unequips
    }
}