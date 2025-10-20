using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class PlayerUISiteOfGraceManager : MonoBehaviour
    {
        [Header("Menu")]
        [SerializeField] GameObject menu;

        public void OpenSiteOfGraceManagerMenu()
        {
            PlayerUIManager.instance.menuWindowIsOpen = true;
            menu.SetActive(true);
        }

        public void CloseSiteOfGraceManagerMenu()
        {
            PlayerUIManager.instance.menuWindowIsOpen = false;
            menu.SetActive(false);
        }

        public void OpenTeleportLocationMenu()
        {
            CloseSiteOfGraceManagerMenu();
            PlayerUIManager.instance.playerUITeleportLocationManager.OpenTeleportLocationManagerMenu();
        }
    }
}
