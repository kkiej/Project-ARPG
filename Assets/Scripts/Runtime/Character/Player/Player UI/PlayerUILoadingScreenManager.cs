using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace LZ
{
    public class PlayerUILoadingScreenManager : MonoBehaviour
    {
        [SerializeField] GameObject loadingScreen;
        [SerializeField] CanvasGroup canvasGroup;
        private Coroutine fadeLoadingScreenCoroutine;

        private void Start()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnSceneChanged(Scene arg0, Scene arg1)
        {
            DeactivateLoadingScreen();
        }

        public void ActivateLoadingScreen()
        {
            //  IF THE LOADING SCREEN OBJECT IS ALREADY ACTIVE, RETURN
            if (loadingScreen.activeSelf)
                return;

            //  TODO IF THE LOADING SCREEN IS IN THE PROCESS OF DEACTIVATING, CANCEL IT

            canvasGroup.alpha = 1;
            loadingScreen.SetActive(true);
        }

        public void DeactivateLoadingScreen(float delay = 1)
        {
            //  IF THE LOADING SCREEN IS NOT ACTIVE, RETURN
            if (!loadingScreen.activeSelf)
                return;

            //  IF WE ARE ALREADY FADING AWAY THE LOADING SCREEN RETURN
            if (fadeLoadingScreenCoroutine != null)
                return;

            //  THE DURATION IS HOW LONG THE FADE WILL TAKE, THE DELAY IS THE WAIT IN SECONDS BEFORE THE FADE BEGINS
            fadeLoadingScreenCoroutine = StartCoroutine(FadeLoadingScreen(1, delay));
        }

        private IEnumerator FadeLoadingScreen(float duration, float delay)
        {
            while (WorldAIManager.instance.isPerformingLoadingOperation)
            {
                yield return null;
            }

            loadingScreen.SetActive(true);

            if (duration > 0)
            {
                while (delay > 0)
                {
                    delay -= Time.deltaTime;
                    yield return null;
                }

                canvasGroup.alpha = 1;
                float elapsedTime = 0;

                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / duration);
                    yield return null;
                }
            }

            canvasGroup.alpha = 0;
            loadingScreen.SetActive(false);
            fadeLoadingScreenCoroutine = null;
            yield return null;
        }
    }
}
