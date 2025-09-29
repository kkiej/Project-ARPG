using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class PlayerUIToggleHud : MonoBehaviour
    {
        private void OnEnable()
        {
            //  HIDE THE HUD
            PlayerUIManager.instance.playerUIHudManager.ToggleHUD(false);
        }

        private void OnDisable()
        {
            //  BRING THE HUD BACK
            PlayerUIManager.instance.playerUIHudManager.ToggleHUD(true);
        }
    }
}