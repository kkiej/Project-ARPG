using UnityEngine;

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
        public CharacterDialogue farewellDialogue;
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

            currentDialogue = WorldSaveGameManager.instance.GetCharacterDialogueByEnum(characterDialogueID);
            farewellDialogue = WorldSaveGameManager.instance.GetCharacterFarewellDialogueByEnum(characterDialogueID);
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

        //  GENERIC FAREWELL DIALOGUE THAT CAN BE CHANGED WITH DIFFERENT FAREWELL SETS
        public void PlayFarewellDialogueEvent()
        {
            if (farewellDialogue == null)
                return;

            if (!dialogueIsPlaying)
            {
                farewellDialogue.PlayDialogueEvent(aiCharacter);
            }
            else
            {
                PlayerUIManager.instance.playerUIPopUpManager.SendNextDialoguePopUpInIndex(farewellDialogue, aiCharacter);
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
