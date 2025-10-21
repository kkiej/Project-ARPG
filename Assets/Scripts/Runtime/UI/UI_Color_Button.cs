using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LZ
{
    public class UI_Color_Button : MonoBehaviour
    {
        [Header("Colors")]
        private float redValue;
        private float greenValue;
        private float blueValue;

        [SerializeField] Image colorImage;

        private void Awake()
        {
            redValue = colorImage.color.r * 255;
            greenValue = colorImage.color.g * 255;
            blueValue = colorImage.color.b * 255;
        }

        public void SetSliderValuesToColor()
        {
            TitleScreenManager.instance.SetRedColorSlider(redValue);
            TitleScreenManager.instance.SetGreenColorSlider(greenValue);
            TitleScreenManager.instance.SetBlueColorSlider(blueValue);
            TitleScreenManager.instance.PreviewHairColor();
        }

        public void ConfirmColor()
        {
            TitleScreenManager.instance.CloseChooseHairColorSubMenu();
        }
    }
}
