using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class PlayerInventoryManager : CharacterInventoryManager
    {
        [Header("Weapons")]
        public WeaponItem currentRightHandWeapon;
        public WeaponItem currentLeftHandWeapon;
        public WeaponItem currentTwoHandWeapon;

        [Header("Quick Slots")]
        public WeaponItem[] weaponsInRightHandSlots = new WeaponItem[3];
        public int rightHandWeaponIndex = 0;
        public WeaponItem[] weaponsInLeftHandSlots = new WeaponItem[3];
        public int leftHandWeaponIndex = 0;
        public SpellItem currentSpell;
        public QuickSlotItem[] quickSlotItemsInQuickSlots = new QuickSlotItem[3];
        public int quickSlotItemIndex = 0;
        public QuickSlotItem currentQuickSlotItem;

        [Header("Armor")]
        public HeadEquipmentItem headEquipment;
        public BodyEquipmentItem bodyEquipment;
        public LegEquipmentItem legEquipment;
        public HandEquipmentItem handEquipment;

        [Header("Projectiles")]
        public RangedProjectileItem mainProjectile;
        public RangedProjectileItem secondaryProjectile;

        [Header("Inventory")]
        public List<Item> itemsInInventory;

        public void AddItemToInventory(Item item)
        {
            itemsInInventory.Add(item);
        }

        public void RemoveItemFromInventory(Item item)
        {
            //  TO DO: CREATE AN RPC HERE THAT SPAWNS ITEM ON NETWORK WHEN DROPPED
            bool isStackable = item.maxItemAmount > 1;

            //  IF THE ITEM IS STACKABLE, ATTEMPT TO REMOVE FROM THE STACK FIRST
            if (isStackable)
            {
                for (int i = itemsInInventory.Count - 1; i > -1; i--)
                {
                    if (itemsInInventory[i].itemID == item.itemID)
                    {
                        itemsInInventory[i].currentItemAmount -= item.currentItemAmount;
                        
                        if (itemsInInventory[i].currentItemAmount <= 0)
                            itemsInInventory.Remove(item);
                    }
                }
            }
            // OTHERWISE SIMPLY REMOVE IT FROM THE INVENTORY
            else
            {
                itemsInInventory.Remove(item);
            }

            // CHECKS FOR NULL LIST SLOTS AND REMOVES THEM
            for (int i = itemsInInventory.Count - 1; i > -1; i--)
            {
                if (itemsInInventory[i] == null)
                {
                    itemsInInventory.RemoveAt(i);
                }
            }
        }
    }
}