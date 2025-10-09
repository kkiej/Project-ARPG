using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace LZ
{
    public class AICharacterInventoryManager : CharacterInventoryManager
    {
        AICharacterManager aiCharacter;

        [Header("Loot Chance")]
        public int dropItemChance = 10;
        [SerializeField] Item[] droppableItems;

        protected override void Awake()
        {
            base.Awake();

            aiCharacter = GetComponent<AICharacterManager>();
        }

        public void DropItem()
        {
            if (!aiCharacter.IsOwner)
                return;

            //  THE STATUS OF IF THIS CHARACTER WILL DROP AN ITEM
            bool willDropItem = false;

            //  RANDOM NUMBER ROLLED FROM 0 - 100 (SO IN THIS EXAMPLE A "dropItemChance" VALUE OF 10 WOULD BE A 10% CHANCE TO RECEIVE AN ITEM
            int itemChanceRoll = Random.Range(0, 100);

            //  IF THE NUMBER IS EQUAL TO OR LOWER THAN THE ITEM DROP CHANCE, WE PASS THE CHECK AND DROP THE ITEM
            if (itemChanceRoll <= dropItemChance)
                willDropItem = true;

            if (!willDropItem)
                return;

            Item generatedItem = droppableItems[Random.Range(0, droppableItems.Length)];

            if (generatedItem == null)
                return;

            GameObject itemPickUpInteractableGameObject = Instantiate(WorldItemDatabase.Instance.pickUpItemPrefab);
            PickUpItemInteractable pickUpInteractable = itemPickUpInteractableGameObject.GetComponent<PickUpItemInteractable>();
            itemPickUpInteractableGameObject.GetComponent<NetworkObject>().Spawn();
            pickUpInteractable.itemID.Value = generatedItem.itemID;
            pickUpInteractable.networkPosition.Value = transform.position;
            pickUpInteractable.droppingCreatureID.Value = aiCharacter.NetworkObjectId;
        }
    }
}
