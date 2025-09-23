using System.Collections;
using TMPro;
using UnityEngine;

namespace LZ
{
    public class PlayerUIPopUpManager : MonoBehaviour
    {
        [Header("Message Pop Up")]
        [SerializeField] TextMeshProUGUI popUpMessageText;
        [SerializeField] GameObject popUpMessageGameObject;
        
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

        public void CloseAllPopUpWindows()
        {
            popUpMessageGameObject.SetActive(false);

            PlayerUIManager.instance.popUpWindowIsOpen = false;
        }
        
        public void SendPlayerMessagePopUp(string messageText)
        {
            PlayerUIManager.instance.popUpWindowIsOpen = true;
            popUpMessageText.text = messageText;
            popUpMessageGameObject.SetActive(true);
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