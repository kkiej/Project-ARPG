using UnityEngine;
using Unity.Netcode;

namespace LZ
{
    public class DialogueInteractable : Interactable
    {
        AICharacterManager aiCharacter;

        protected override void Awake()
        {
            base.Awake();

            aiCharacter = GetComponentInParent<AICharacterManager>();
        }

        public override void Interact(PlayerManager player)
        {
            if (PlayerUIManager.instance.menuWindowIsOpen)
                return;

            if (aiCharacter.isDead.Value)
            {
                interactableCollider.enabled = false;
                return;
            }

            if (NetworkManager.Singleton.IsServer)
            {
                WorldSaveGameManager.instance.SaveGame();
                //  CLOSE ALL POP UP WINDOWS
            }

            //  1. PLAY CURRENT DIALOGUE
            aiCharacter.aiCharacterSoundFXManager.PlayCurrentDialogueEvent();
            //  2. (OPTIONALLY) USE FACE IK TRACKING TO LOOK AT PLAYER
        }

        public override void OnTriggerEnter(Collider other)
        {
            if (aiCharacter.isDead.Value)
            {
                interactableCollider.enabled = false;

                //  IF THERE IS AN ACTIVE DIALOGUE WITH THIS CHARACTER AND THE PLAYER END IT
                PlayerManager player = other.GetComponent<PlayerManager>();

                if (player != null && player.IsOwner)
                    aiCharacter.aiCharacterSoundFXManager.CancelCurrentDialogueEvent();
            }

            base.OnTriggerEnter(other);
        }

        public override void OnTriggerExit(Collider other)
        {
            base.OnTriggerExit(other);

            PlayerManager player = other.GetComponent<PlayerManager>();

            if (player == null)
                return;

            if (!player.IsOwner)
                return;

            //  CANCEL CURRENT DIALOGUE (IF ANY) WITH THIS CHARACTER WHEN PLAYER LEAVES INTERACTION RADIUS
            aiCharacter.aiCharacterSoundFXManager.CancelCurrentDialogueEvent();

            //  CLOSE ALL MENUS RELATED TO THIS CHARACTER (BLACKSMITH, SHOP, ECT)
            //  RESET HEAD IK TRACKING IF ENABLED          
        }
    }
}
