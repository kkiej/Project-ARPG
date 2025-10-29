using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LZ
{
    public class PlayerUIPopUpManager : MonoBehaviour
    {
        [Header("Message Pop Up")]
        [SerializeField] TextMeshProUGUI popUpMessageText;
        [SerializeField] GameObject popUpMessageGameObject;

        [Header("Item Pop Up")]
        [SerializeField] GameObject itemPopUpGameObject;
        [SerializeField] Image itemIcon;
        [SerializeField] TextMeshProUGUI itemName;
        [SerializeField] TextMeshProUGUI itemAmount;

        //  IF YOU PLAN ON MAKING ALL OF THESE POPUPS FUNCTION IDENTICALLY, YOU COULD JUST MAKE 1 POP UP GAMEOBJECT AND CHANGE THE TEXT VALUES AS NEEDED
        //  INSTEAD OF MAKING SEVERAL DIFFERENT GROUPS FOR POP UP FUNCTIONALITY
        [Header("YOU DIED Pop Up")]
        [SerializeField] private GameObject youDiedPopUpGameObject;
        [SerializeField] private TextMeshProUGUI youDiedPopUpBackgroundText;
        [SerializeField] private TextMeshProUGUI youDiedPopUpText;
        [SerializeField] private CanvasGroup youDiedPopUpCanvasGroup; // 允许我们随着时间设置alpha来渐隐
        
        [Header("BOSS DEFEATED Pop Up")]
        [SerializeField] GameObject bossDefeatedPopUpGameObject;
        [SerializeField] TextMeshProUGUI bossDefeatedPopUpBackgroundText;
        [SerializeField] TextMeshProUGUI bossDefeatedPopUpText;
        [SerializeField] CanvasGroup bossDefeatedPopUpCanvasGroup;   // 允许我们随着时间设置alpha来渐隐
        
        [Header("GRACE RESTORED Pop Up")]
        [SerializeField] GameObject graceRestoredPopUpGameObject;
        [SerializeField] TextMeshProUGUI graceRestoredPopUpBackgroundText;
        [SerializeField] TextMeshProUGUI graceRestoredPopUpText;
        [SerializeField] CanvasGroup graceRestoredPopUpCanvasGroup;   // 允许我们随着时间设置alpha来渐隐
        
        [Header("DIALOGUE Pop Up")]
        [SerializeField] GameObject dialoguePopUpGameObject;
        [SerializeField] TextMeshProUGUI dialoguePopUpText;
        [SerializeField] CharacterDialogue currentDialogue;
        private Coroutine dialogueCoroutine;

        public void CloseAllPopUpWindows()
        {
            popUpMessageGameObject.SetActive(false);
            itemPopUpGameObject.SetActive(false);

            PlayerUIManager.instance.popUpWindowIsOpen = false;
        }
        
        public void SendPlayerMessagePopUp(string messageText)
        {
            PlayerUIManager.instance.popUpWindowIsOpen = true;
            popUpMessageText.text = messageText;
            popUpMessageGameObject.SetActive(true);
        }

        public void SendItemPopUp(Item item, int amount)
        {
            itemAmount.enabled = false;
            itemIcon.sprite = item.itemIcon;
            itemName.text = item.itemName;

            if (amount > 0)
            {
                itemAmount.enabled = true;
                itemAmount.text = "x" + amount.ToString();
            }

            itemPopUpGameObject.SetActive(true);
            PlayerUIManager.instance.popUpWindowIsOpen = true;
        }

        public void SendYouDiedPopUp()
        {
            // 激活后处理效果
            
            youDiedPopUpGameObject.SetActive(true);
            youDiedPopUpBackgroundText.characterSpacing = 0;
            StartCoroutine(StretchPopUpTextOverTime(youDiedPopUpBackgroundText, 8, 19));
            StartCoroutine(FadeInPopUpOverTime(youDiedPopUpCanvasGroup, 5));
            StartCoroutine(WaitThenFadeOutPopUpOverTime(youDiedPopUpCanvasGroup, 2, 5));
        }
        
        public void SendBossDefeatedPopUp(string bossDefeatedMessage)
        {
            bossDefeatedPopUpText.text = bossDefeatedMessage;
            bossDefeatedPopUpBackgroundText.text = bossDefeatedMessage;
            bossDefeatedPopUpGameObject.SetActive(true);
            bossDefeatedPopUpBackgroundText.characterSpacing = 0;
            StartCoroutine(StretchPopUpTextOverTime(bossDefeatedPopUpBackgroundText, 8, 19));
            StartCoroutine(FadeInPopUpOverTime(bossDefeatedPopUpCanvasGroup, 5));
            StartCoroutine(WaitThenFadeOutPopUpOverTime(bossDefeatedPopUpCanvasGroup, 2, 5));
        }
        
        public void SendGraceRestoredPopUp(string graceRestoredMessage)
        {
            graceRestoredPopUpText.text = graceRestoredMessage;
            graceRestoredPopUpBackgroundText.text = graceRestoredMessage;
            graceRestoredPopUpGameObject.SetActive(true);
            graceRestoredPopUpBackgroundText.characterSpacing = 0;
            StartCoroutine(StretchPopUpTextOverTime(graceRestoredPopUpBackgroundText, 8, 19));
            StartCoroutine(FadeInPopUpOverTime(graceRestoredPopUpCanvasGroup, 5));
            StartCoroutine(WaitThenFadeOutPopUpOverTime(graceRestoredPopUpCanvasGroup, 2, 5));
        }

        public void SendDialoguePopUp(CharacterDialogue dialogue, AICharacterManager aiCharacter)
        {
            PlayerUIManager.instance.playerUIHudManager.ToggleHUDWithOutPopUps(false);
            currentDialogue = dialogue;

            if (dialogueCoroutine != null)
                StopCoroutine(dialogueCoroutine);

            dialogueCoroutine = StartCoroutine(dialogue.PlayDialogueCoroutine(aiCharacter));
            PlayerUIManager.instance.popUpWindowIsOpen = true;
        }

        public void SendNextDialoguePopUpInIndex(CharacterDialogue dialogue, AICharacterManager aiCharacter)
        {
            currentDialogue = dialogue;

            if (dialogueCoroutine != null)
                StopCoroutine(dialogueCoroutine);

            if (aiCharacter.aiCharacterSoundFXManager.dialogueIsPlaying)
                aiCharacter.aiCharacterSoundFXManager.audioSource.Stop();

            currentDialogue.dialogueIndex++;
            dialogueCoroutine = StartCoroutine(dialogue.PlayDialogueCoroutine(aiCharacter));
        }

        public void SetDialoguePopUpSubtitles(string dialogueText)
        {
            dialoguePopUpGameObject.SetActive(true);
            dialoguePopUpText.text = dialogueText;
        }

        public void EndDialoguePopUp()
        {
            dialoguePopUpGameObject.SetActive(false);
            PlayerUIManager.instance.playerUIHudManager.ToggleHUDWithOutPopUps(true);
        }

        public void CancelDialoguePopUp(AICharacterManager aiCharacter)
        {
            PlayerUIManager.instance.playerUIHudManager.ToggleHUDWithOutPopUps(true);

            if (dialogueCoroutine != null)
                StopCoroutine(dialogueCoroutine);

            if (aiCharacter.aiCharacterSoundFXManager.audioSource.isPlaying)
                aiCharacter.aiCharacterSoundFXManager.audioSource.Stop();

            dialoguePopUpGameObject.SetActive(false);
            currentDialogue.OnDialogueCancelled(aiCharacter);
        }

        private IEnumerator StretchPopUpTextOverTime(TextMeshProUGUI text, float duration, float stretchAmount)
        {
            if (duration > 0f)
            {
                text.characterSpacing = 0;
                float timer = 0;

                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    text.characterSpacing = Mathf.Lerp(text.characterSpacing, stretchAmount, (timer / duration) / 20);
                    yield return null;
                }
            }
        }

        private IEnumerator FadeInPopUpOverTime(CanvasGroup canvas, float duration)
        {
            if (duration > 0)
            {
                canvas.alpha = 0;
                float timer = 0;

                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    canvas.alpha = Mathf.Lerp(0, 1, timer / duration);
                    yield return null;
                }
            }

            canvas.alpha = 1;
        }
        
        private IEnumerator WaitThenFadeOutPopUpOverTime(CanvasGroup canvas, float duration, float delay)
        {
            if (duration > 0)
            {
                // 等待延迟
                while (delay > 0)
                {
                    delay -= Time.deltaTime;
                    yield return null;
                }
        
                canvas.alpha = 1;  // 确保一开始透明度是 1
                float timer = 0;

                // 渐变
                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    canvas.alpha = Mathf.Lerp(1, 0, timer / duration);  // 正确计算渐变进度
                    yield return null;
                }
            }

            canvas.alpha = 0;  // 最终确保透明度为 0
        }
    }
}