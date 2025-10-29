using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;

namespace LZ
{
    public class WorldGameSessionManager : MonoBehaviour
    {
        public static WorldGameSessionManager instance;
        
        [Header("Active Players In Session")]
        public List<PlayerManager> players = new List<PlayerManager>();

        private Coroutine revivalCoroutien;

        [Header("Active Lobby")]
        public Lobby? currentLobby;
        private FacepunchTransport transport;
        private Coroutine joiningAsClientCoroutine;

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

            transport = GetComponent<FacepunchTransport>();
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
            SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        }

        private void OnDestroy()
        {
            SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
            SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
            SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
        }

        private void OnApplicationQuit()
        {
            DisconnectFromLobby();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene newScene, LoadSceneMode loadMode)
        {
            //  IF WE ARENT ON THE MENU SCENE, ALLOW OTHERS TO JOIN OUR LOBBY
            if (SceneManager.GetActiveScene().buildIndex != 0)
            {
                ToggleLobbyIsJoinable(true);
            }
            else
            {
                ToggleLobbyIsJoinable(false);
            }
        }

        //  FACE PUNCH

        public void ToggleLobbyIsJoinable(bool status)
        {
            currentLobby?.SetJoinable(status);
        }

        //  CALLED WHEN A LOBBY IS CREATED
        private void OnLobbyCreated(Result result, Lobby lobby)
        {
            if (result != Result.OK)
            {
                Debug.LogError($"Lobby could not be created, {result}", this);
                return;
            }

            lobby.SetPublic();
            lobby.SetJoinable(false);   // WE ONLY WANNA SET TO JOINABLE ONCE WE ARE IN THE WORLD
            lobby.SetGameServer(lobby.Owner.Id);
        }

        //  CALLED WHEN YOU ENTER A LOBBY
        private void OnGameLobbyJoinRequested(Lobby joinedLobby, SteamId steamID)
        {
            //  IF WE ARE ON THE MAIN MENU WHEN TRYING TO JOIN, DO NOT ALLOW THE PLAYER TO JOIN UNTIL THEY LOAD INTO THE WORLD
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsClient)
                {
                    Debug.Log("We are not allowed to join another game, we aren't a client or a host. Start the game first.");
                    return;
                }

                //  OPTIONALLY SEND A POP UP LETTING THE PLAYER KNOW THROUGH A UI ELEMENT
            }

            //  SAVE BEFORE JOINING
            WorldSaveGameManager.instance.SaveGame();
            NetworkManager.Singleton.Shutdown();

            Debug.Log($"ATTEMPTING TO JOIN GAME, {joinedLobby.Id}, from {steamID}");
            currentLobby = joinedLobby;

            //  IF WE HAVE A CURRENT LOBBY, JOIN IT
            currentLobby?.Join();
        }

        private void OnLobbyEntered(Lobby lobby)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                return;
            }
            else
            {
                StartGameAsClient(lobby.Owner.Id);
            }
        }

        //public async void StartGameAsHost()
        public void StartGameAsHost()
        {
            NetworkManager.Singleton.StartHost();

            //  IF YOU WANT TO GO BACK TO TESTING WITH A HOST AND CLIENT ON ONE PC, COMMMENT OUT THIS LINE
            //  AS FAR AS I KNOW YOU CANT TEST WITH 2 CLIENTS ON THE SAME PC WITHOUT USING A PROGRAM TO SIMULATE A SECOND PC, AND AN ADDITIONAL STEAM ACCOUNT WITH THE GAME
            //currentLobby = await SteamMatchmaking.CreateLobbyAsync(4);
        }

        public void StartGameAsClient(SteamId id)
        {
            if (PlayerUIManager.instance.localPlayer.isDead.Value)
            {
                //  (OPTIONAL) SEND PLAYER A MESSAGE TO WAIT UNTIL THEY ARE ALIVE BEFORE JOINING
                return;
            }

            if (joiningAsClientCoroutine != null)
                StopCoroutine(joiningAsClientCoroutine);

            joiningAsClientCoroutine = StartCoroutine(AttemptToJoinAsClient(id));
        }

        private IEnumerator AttemptToJoinAsClient(SteamId id)
        {
            //  OPTIONALLY ACTIVATE LOADING SCREEN UNTIL JOINED

            while (transport.targetSteamId != id)
            {
                transport.targetSteamId = id;
                yield return null;
            }

            yield return null;

            NetworkManager.Singleton.StartClient();
        }

        public void DisconnectFromLobby()
        {
            currentLobby?.Leave();
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

            WorldAIManager.instance.ResetAllCharacters();

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