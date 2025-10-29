using UnityEngine;
using Unity.Netcode;

namespace LZ
{
    public class AICharacterSoundFXManager : CharacterSoundFXManager
    {
        AICharacterManager aiCharacter;

        [Header("Blocking SFX")]
        [SerializeField] AudioClip[] blockingSFX;

        [Header("Dialogue")]
        public CharacterDialogueID characterDialogueID;
        public GameObject interactableDialogueCollider;
        public CharacterDialogue currentDialogue;
        public GameObject interactableDialogueObject;
        public bool dialogueIsPlaying = false;
        //  OPTIONAL CONVERSATION TARGET TO LOOK AT WITH IK

        protected override void Awake()
        {
            base.Awake();

            aiCharacter = GetComponent<AICharacterManager>();
        }

        protected override void Start()
        {
            base.Start();

            if (characterDialogueID != CharacterDialogueID.NoDialogueID)
            {
                currentDialogue = WorldSaveGameManager.instance.GetCharacterDialogueByEnum(characterDialogueID);
                interactableDialogueObject = Instantiate(WorldAIManager.instance.dialogueInteractable, transform);
                NetworkObject networkObject = interactableDialogueObject.GetComponent<NetworkObject>();
                networkObject.Spawn();
                networkObject.TrySetParent(gameObject, true);
            }
        }

        public override void PlayBlockSoundFX()
        {
            if (blockingSFX.Length <= 0)
                return;

            PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(blockingSFX));
        }

        //  DIALOGUE
        public void PlayCurrentDialogueEvent()
        {
            if (currentDialogue == null)
                return;

            if (!dialogueIsPlaying)
            {
                currentDialogue.PlayDialogueEvent(aiCharacter);
            }
            else
            {
                PlayerUIManager.instance.playerUIPopUpManager.SendNextDialoguePopUpInIndex(currentDialogue, aiCharacter);
            }
        }

        //  CANCELS CURRENT DIALOGUE EVENT (USED WHEN LEAVING TRIGGER AREA ECT)
        public void CancelCurrentDialogueEvent()
        {
            if (dialogueIsPlaying)
            {
                dialogueIsPlaying = false;
                PlayerUIManager.instance.playerUIPopUpManager.CancelDialoguePopUp(aiCharacter);
            }
        }

        //  USED FOR SPECIFIC CALLS WHEN A DIALOGUE IS OVER (NPC DIES, SHOP OPENS, ECT)
        public void OnCurrentDialogueEnded()
        {
            //  GET NEW DIALOGUE BASED ON STAGE ID
            currentDialogue = WorldSaveGameManager.instance.GetCharacterDialogueByEnum(characterDialogueID);
        }
    }
}
