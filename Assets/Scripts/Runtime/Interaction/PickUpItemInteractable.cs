using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace LZ
{
    public class PickUpItemInteractable : Interactable
    {
        public ItemPickUpType pickUpType;

        [Header("Item")]
        [SerializeField] Item item;

        [Header("World Spawn Pick Up")]
        [SerializeField] int itemID;
        [SerializeField] bool hasBeenLooted = false;

        protected override void Start()
        {
            base.Start();

            if (pickUpType == ItemPickUpType.WorldSpawn)
                CheckIfWorldItemWasAlreadyLooted();
        }

        private void CheckIfWorldItemWasAlreadyLooted()
        {
            // 0. IF THE PLAYER ISN'T THE HOST, HIDE THE ITEM
            if (!NetworkManager.Singleton.IsHost)
            {
                gameObject.SetActive(false);
                return;
            }

            // 1. COMPARE THE DATA OF LOOTED ITEMS I.D'S WITH THIS ITEM'S ID
            if (!WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.ContainsKey(itemID))
            {
                WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.Add(itemID, false);
            }

            hasBeenLooted = WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted[itemID];

            // 2. IF IT HAS BEEN LOOTED, HIDE THE GAMEOBJECT
            if (hasBeenLooted)
                gameObject.SetActive(false);
        }

        public override void Interact(PlayerManager player)
        {
            base.Interact(player);

            // 1. PLAY A SFX
            player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.pickUpItemSFX);

            // 2. ADD ITEM TO INVENTORY
            player.playerInventoryManager.AddItemToInventory(item);

            // 3. DISPLAY A UI POP UP SHOWING ITEM'S NAME AND PICTURE
            PlayerUIManager.instance.playerUIPopUpManager.SendItemPopUp(item, 1);

            // 4. SAVE LOOT STATUS IF IT'S A WORLD SPAWN
            if (pickUpType == ItemPickUpType.WorldSpawn)
            {
                if (WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.ContainsKey((int)itemID))
                {
                    WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.Remove(itemID);
                }

                WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.Add(itemID, true);
            }

            // 5. HIDE OR DESTROY GAMEOBJECT
            Destroy(gameObject);
        }
    }
}