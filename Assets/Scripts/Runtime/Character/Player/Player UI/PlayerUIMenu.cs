using UnityEngine;
using System.Collections;

namespace LZ
{
    public class PlayerUIMenu : MonoBehaviour
    {
        [Header("Menu")]
        [SerializeField] GameObject menu;

        public virtual void OpenMenu()
        {
            PlayerUIManager.instance.menuWindowIsOpen = true;
            menu.SetActive(true);
        }
        
        public virtual void OpenMenuAfterFixedFrame()
        {
            if (menu.activeInHierarchy)
                return;

            StartCoroutine(WaitThenOpenMenu());
        }
        
        protected virtual IEnumerator WaitThenOpenMenu()
        {
            yield return new WaitForFixedUpdate();

            OpenMenu();
        }

        //  THIS IS FINE, BUT IF YOU'RE USING THE "A" BUTTON TO CLOSE MENUS YOU WILL JUMP AS YOU CLOSE THE MENU
        public virtual void CloseMenu()
        {
            PlayerUIManager.instance.menuWindowIsOpen = false;
            menu.SetActive(false);
        }

        public virtual void CloseMenuAfterFixedFrame()
        {
            if (!menu.activeInHierarchy)
                return;

            StartCoroutine(WaitThenCloseMenu());
        }

        protected virtual IEnumerator WaitThenCloseMenu()
        {
            yield return new WaitForFixedUpdate();

            CloseMenu();
        }
    }
}
