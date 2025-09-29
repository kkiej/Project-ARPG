using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class PlayerUICharacterMenuManager : MonoBehaviour
    {
        [Header("Menu")]
        [SerializeField] GameObject menu;

        public void OpenCharacterMenu()
        {
            PlayerUIManager.instance.menuWindowIsOpen = true;
            menu.SetActive(true);
        }

        //  THIS IS FINE, BUT IF YOU'RE USING THE "A" BUTTON TO CLOSE MENUS YOU WILL JUMP AS YOU CLOSE THE MENU
        public void CloseCharacterMenu()
        {
            PlayerUIManager.instance.menuWindowIsOpen = false;
            menu.SetActive(false);
        }

        public void CloseCharacterMenuAfterFixedFrame()
        {
            StartCoroutine(WaitThenCloseMenu());
        }

        private IEnumerator WaitThenCloseMenu()
        {
            yield return new WaitForFixedUpdate();

            PlayerUIManager.instance.menuWindowIsOpen = false;
            menu.SetActive(false);
        }
    }
}