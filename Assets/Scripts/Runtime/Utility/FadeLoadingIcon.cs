using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LZ
{
    public class FadeLoadingIcon : MonoBehaviour
    {
        [SerializeField] Image fadeImage;
        private Coroutine fadeCoroutine;

        private void OnEnable()
        {
            FadeUIImage();
        }

        private void OnDisable()
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
        }

        public void FadeUIImage()
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeCoroutine(true));
        }

        private IEnumerator FadeCoroutine(bool fadeAway)
        {
            if (fadeAway)
            {
                for (float i = 1; i >= 0; i -= Time.unscaledDeltaTime)
                {
                    fadeImage.color = new Color(1, 1, 1, i);
                    yield return null;
                }

                fadeCoroutine = StartCoroutine(FadeCoroutine(false));
            }
            else
            {
                for (float i = 0; i <= 1; i += Time.unscaledDeltaTime)
                {
                    fadeImage.color = new Color(1, 1, 1, i);
                    yield return null;
                }

                fadeCoroutine = StartCoroutine(FadeCoroutine(true));
            }
        }
    }
}
