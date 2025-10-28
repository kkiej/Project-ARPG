using UnityEngine;

namespace LZ
{
    public class AICharacterSoundFXManager : CharacterSoundFXManager
    {
        [Header("Blocking SFX")]
        [SerializeField] AudioClip[] blockingSFX;

        [Header("Dialogue")]
        //  CHARACTER DIALOGUE ID (WE WILL LOAD POSSIBLE DIALOGUES FROM A DATABASE PER DIALOGUE CHARACTER ID)
        public GameObject interactableDialogueCollider;
        //  CURRENT CHARACTER DIALOGUE (DIALOGUES WILL BE SCRIPTABLE OBJECTS)
        //  OPTIONAL FAREWELL DIALOGUE SET (USED WHEN LEAVING OR ENDING ANY DIALOGUE, CAN BE TRADED OUT DEPENDING ON NPC INTERACTIONS, HAPPY, SAD, ANGRY ECT)
        public bool dialogueIsPlaying = false;
        //  OPTIONAL CONVERSATION TARGET TO LOOK AT WITH IK

        public override void PlayBlockSoundFX()
        {
            if (blockingSFX.Length <= 0)
                return;

            PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(blockingSFX));
        }

        //  DIALOGUE
        public void PlayCurrentDialogueEvent()
        {

        }

        //  GENERIC FAREWELL DIALOGUE THAT CAN BE CHANGED WITH DIFFERENT FAREWELL SETS
        public void PlayFarewellDialogueEvent()
        {

        }

        //  CANCELS CURRENT DIALOGUE EVENT (USED WHEN LEAVING TRIGGER AREA ECT)
        public void CancelCurrentDialogueEvent()
        {

        }

        //  USED FOR SPECIFIC CALLS WHEN A DIALOGUE IS OVER (NPC DIES, SHOP OPENS, ECT)
        public void OnCurrentDialogueEnded()
        {

        }
    }
}
