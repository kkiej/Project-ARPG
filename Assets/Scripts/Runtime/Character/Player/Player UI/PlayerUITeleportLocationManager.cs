using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LZ
{
    public class PlayerUITeleportLocationManager : PlayerUIMenu
    {
        [Header("Teleport Locations")]
        [SerializeField] GameObject[] teleportLocations;

        public override void OpenMenu()
        {
            base.OpenMenu();

            CheckForUnlockedTeleports();
        }

        private void CheckForUnlockedTeleports()
        {
            bool hasFirstSelectedButton = false;

            for (int i = 0; i < teleportLocations.Length; i++)
            {
                for (int s = 0; s < WorldObjectManager.instance.sitesOfGrace.Count; s++)
                {
                    if (WorldObjectManager.instance.sitesOfGrace[s].siteOfGraceID == i)
                    {
                        if (WorldObjectManager.instance.sitesOfGrace[s].isActivated.Value)
                        {
                            teleportLocations[i].SetActive(true);

                            if (!hasFirstSelectedButton)
                            {
                                hasFirstSelectedButton = true;
                                teleportLocations[i].GetComponent<Button>().Select();
                                teleportLocations[i].GetComponent<Button>().OnSelect(null);
                            }
                        }
                        else
                        {
                            teleportLocations[i].SetActive(false);
                        }
                    }
                }
            }
        }

        public void TeleportToSiteOfGrace(int siteID)
        {
            for (int i = 0; i < WorldObjectManager.instance.sitesOfGrace.Count; i++)
            {
                if (WorldObjectManager.instance.sitesOfGrace[i].siteOfGraceID == siteID)
                {
                    WorldObjectManager.instance.sitesOfGrace[i].TeleportToSiteOfGrace();
                    return;
                }
            }
        }
    }
}
