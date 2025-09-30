using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace LZ
{
    public class UI_EquipmentInventorySlot : MonoBehaviour
    {
        public Image itemIcon;
        public Image highlightedIcon;
        [SerializeField] public Item currentItem;

        public void AddItem(Item item)
        {
            if (item == null)
            {
                itemIcon.enabled = false;
                return;
            }

            itemIcon.enabled = true;

            currentItem = item;
            itemIcon.sprite = item.itemIcon;
        }

        public void SelectSlot()
        {
            highlightedIcon.enabled = true;
        }

        public void DeselectSlot()
        {
            highlightedIcon.enabled = false;
        }

        public void EquipItem()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            switch (PlayerUIManager.instance.playerUIEquipmentManager.currentSelectedEquipmentSlot)
            {
                case EquipmentType.RightWeapon01:

                    //  IF OUR CURRENT WEAPON IN THIS SLOT, IS NOT AN UNARMED ITEM, ADD IT TO OUR INVENTORY
                    WeaponItem currentWeapon = player.playerInventoryManager.weaponsInRightHandSlots[0];

                    if (currentWeapon.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                    {
                        player.playerInventoryManager.AddItemToInventory(currentWeapon);
                    }

                    //  THEN REPLACE THE WEAPON IN THAT SLOT WITH OUR NEW WEAPON
                    player.playerInventoryManager.weaponsInRightHandSlots[0] = currentItem as WeaponItem;

                    //  THEN REMOVE THE NEW WEAPON FROM OUR INVENTORY
                    player.playerInventoryManager.RemoveItemFromInventory(currentItem);

                    //  RE-EQUIP NEW WEAPON IF WE ARE HOLDING THE CURRENT WEAPON IN THIS SLOT (IF YOU CHANGE RIGHT WEAPON 3 AND YOU ARE HOLDING RIGHT WEAPON 1 NOTHING WOULD HAPPEN HERE)
                    if (player.playerInventoryManager.rightHandWeaponIndex == 0)
                        player.playerNetworkManager.currentRightHandWeaponID.Value = currentItem.itemID;

                    //  REFRESHES EQUIPMENT WINDOW
                    PlayerUIManager.instance.playerUIEquipmentManager.OpenEquipmentManagerMenu();

                    break;
                case EquipmentType.RightWeapon02:
                    break;
                case EquipmentType.RightWeapon03:
                    break;
                case EquipmentType.LeftWeapon01:
                    break;
                case EquipmentType.LeftWeapon02:
                    break;
                case EquipmentType.LeftWeapon03:
                    break;
                default:
                    break;
            }
        }
    }
}
