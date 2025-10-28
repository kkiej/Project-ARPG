using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LZ
{
    [CreateAssetMenu(menuName = "A.I/Dialogue")]
    public class CharacterDialogue : ScriptableObject
    {
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

        }

        private IEnumerator PlayDialogueCoroutine(AICharacterManager aiCharacter)
        {
            yield return null;
        }

        public void OnDialogueEnded(AICharacterManager aiCharacter)
        {

        }

        public void OnDialogueCancelled(AICharacterManager aiCharacter)
        {

        }
    }
}
