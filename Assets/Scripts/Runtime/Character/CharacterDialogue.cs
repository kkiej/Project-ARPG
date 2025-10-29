using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LZ
{
    [CreateAssetMenu(menuName = "A.I/Dialogue")]
    public class CharacterDialogue : ScriptableObject
    {
        [Header("Dialogue Requirements")]
        public int requiredStageID = 0;

        [Header("Greeting Dialogue")]
        [TextArea] public List<string> greetingDialogueString = new List<string>();
        public List<AudioClip> greetingDialogueAudio = new List<AudioClip>();
        private bool greetingHasPlayed = false;

        [Header("Core Dialogue")]
        [TextArea] public List<string> dialogueString = new List<string>();
        public List<AudioClip> dialogueAudio = new List<AudioClip>();
        public int dialogueIndex = 0;

        //  OPTIONAL SETTINGS
        //  FACE CHARACTER
        //  KILL ON CANCEL
        //  OPEN MENU ON CANCEL
        //  ECT

        [Header("End Triggers")]
        [SerializeField] bool setStageIndex = false;    //  THIS WILL BE USED TO "SET STAGE ID" AFTER SETTING AN ID, NEW DIALOGUE WILL BE SELECTED DEPENDING ON ID
        [SerializeField] int stageID = 0;

        public void PlayDialogueEvent(AICharacterManager aiCharacter)
        {
            if (dialogueString.Count != dialogueAudio.Count)
            {
                Debug.Log("AUDIO CLIP COUNT DOES NOT MATCH SUBTITLE COUNT, MISSING FILES");
                return;
            }

            aiCharacter.aiCharacterSoundFXManager.dialogueIsPlaying = true;
            PlayerUIManager.instance.playerUIPopUpManager.SendDialoguePopUp(this, aiCharacter);
        }

        public IEnumerator PlayDialogueCoroutine(AICharacterManager aiCharacter)
        {
            //  PLAY A RANDOM GREETING DIALOGUE, THEN WAIT THE LENGTH OF THAT AUDIO CLIP + A SECOND
            if (greetingDialogueAudio.Count != 0 && !greetingHasPlayed)
            {
                greetingHasPlayed = true;
                int randomGreetingDialogueIndex = Random.Range(0, greetingDialogueAudio.Count);
                PlayerUIManager.instance.playerUIPopUpManager.SetDialoguePopUpSubtitles(greetingDialogueString[randomGreetingDialogueIndex]);
                aiCharacter.aiCharacterSoundFXManager.PlaySoundFX(greetingDialogueAudio[randomGreetingDialogueIndex]);
                yield return new WaitForSeconds(greetingDialogueAudio[randomGreetingDialogueIndex].length + 1);
            }

            while (dialogueIndex < dialogueString.Count)
            {
                PlayerUIManager.instance.playerUIPopUpManager.SetDialoguePopUpSubtitles(dialogueString[dialogueIndex]);
                aiCharacter.aiCharacterSoundFXManager.PlaySoundFX(dialogueAudio[dialogueIndex]);
                dialogueIndex++;
                yield return new WaitForSeconds(dialogueAudio[dialogueIndex].length + 1);
            }

            OnDialogueEnded(aiCharacter);
            PlayerUIManager.instance.playerUIPopUpManager.EndDialoguePopUp();

            yield return null;
        }

        public void OnDialogueEnded(AICharacterManager aiCharacter)
        {
            //  DO STUFF WITH CHARACTER DIALOGUE SCRIPTABLE IF DESIRED
            greetingHasPlayed = false;
            dialogueIndex = 0;

            if (setStageIndex)
                WorldSaveGameManager.instance.SetStageOfDialogue(aiCharacter.aiCharacterSoundFXManager.characterDialogueID, stageID);

            //  DO STUFF WITH AI CHARACTER IF DESIRED
            aiCharacter.aiCharacterSoundFXManager.OnCurrentDialogueEnded();
        }

        public void OnDialogueCancelled(AICharacterManager aiCharacter)
        {

        }
    }
}
