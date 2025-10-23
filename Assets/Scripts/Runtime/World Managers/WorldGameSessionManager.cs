using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class WorldGameSessionManager : MonoBehaviour
    {
        public static WorldGameSessionManager instance;
        
        [Header("Active Players In Session")]
        public List<PlayerManager> players = new List<PlayerManager>();

        private Coroutine revivalCoroutien;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void WaitThenReviveHost()
        {
            if (revivalCoroutien != null)
                StopCoroutine(revivalCoroutien);

            revivalCoroutien = StartCoroutine(ReviveHostCoroutine(5));
        }

        private IEnumerator ReviveHostCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            PlayerUIManager.instance.playerUILoadingScreenManager.ActivateLoadingScreen();

            PlayerUIManager.instance.localPlayer.ReviveCharacter();

            //  TODO SAVE LAST SITE OF GRACE VISITED, AND T.P THERE
            for (int i = 0; i < WorldObjectManager.instance.sitesOfGrace.Count; i++)
            {
                if (WorldObjectManager.instance.sitesOfGrace[i].siteOfGraceID == WorldSaveGameManager.instance.currentCharacterData.lastSiteOfGraceRestedAt)
                {
                    WorldObjectManager.instance.sitesOfGrace[i].TeleportToSiteOfGrace();
                    break;
                }
            }
        }

        public void AddPlayerToActivePlayersList(PlayerManager player)
        {
            // 检查列表，如果不包含玩家就添加他
            if (!players.Contains(player))
            {
                players.Add(player);
            }
            
            // 检查列表有没有空槽位，有就删除
            for (int i = players.Count - 1; i > -1; i--)
            {
                if (players[i] == null)
                {
                    players.RemoveAt(i);
                }
            }
        }

        public void RemovePlayerFromActivePlayersList(PlayerManager player)
        {
            if (players.Contains(player))
            {
                players.Remove(player);
            }
            
            // 检查列表有没有空槽位，有就删除
            for (int i = players.Count - 1; i > -1; i--)
            {
                if (players[i] == null)
                {
                    players.RemoveAt(i);
                }
            }
        }
    }
}