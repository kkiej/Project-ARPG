using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

namespace LZ
{
    public class WorldSceneManager : NetworkBehaviour
    {
        public static WorldSceneManager instance;

        //  LOADED SCENES
        public List<Scene> loadedScenes = new List<Scene>();

        //  QUED SCENES
        private List<string> quedSceneIDs = new List<string>();
        private int quedScenesToLoad = 0;
        private Coroutine loadingAdditiveScenesCoroutine;

        //  LOADING STATUS
        private bool sceneIsLoading = false;
        private bool sceneIsUnloading = false;

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

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;        
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;

            //  UNLOAD ALL SCENES
        }

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            if (!NetworkManager.IsServer)
                return;

            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.Load:
                    sceneIsLoading = true;
                    break;
                case SceneEventType.Unload:
                    sceneIsUnloading = true;
                    break;
                case SceneEventType.Synchronize:
                    break;
                case SceneEventType.ReSynchronize:
                    break;
                case SceneEventType.LoadEventCompleted:
                    break;
                case SceneEventType.UnloadEventCompleted:
                    sceneIsUnloading = false;
                    break;
                case SceneEventType.LoadComplete:

                    //  CALLED WHEN THE SCENE IS FINISHED LOADING, ADDS OUR SCENE TO OUR LOADED SCENES LIST
                    loadedScenes.Add(sceneEvent.Scene);
                    
                    //  CLEAR THE LIST IDS IF THE SCENES TO LOAD COUNT IS 0
                    if (quedScenesToLoad <= 0)
                        quedSceneIDs.Clear();

                    //  DOUBLE CHECK LOADED SCENES TO MAKE SURE THEY ARE LOADED, IF NOT REMOVE THEM FROM THE LOADED LIST
                    for (int i = 0; i < loadedScenes.Count; i++)
                    {
                        if (!loadedScenes[i].isLoaded)
                            loadedScenes.RemoveAt(i);
                    }

                    sceneIsLoading = false;
                    break;
                case SceneEventType.UnloadComplete:
                    break;
                case SceneEventType.SynchronizeComplete:
                    break;
                case SceneEventType.ActiveSceneChanged:
                    break;
                case SceneEventType.ObjectSceneChanged:
                    break;
                default:
                    break;
            }
        }

        //  USED TO LOAD OUR MAIN WORLD SCENE
        public void LoadWorldScene(int buildIndex)
        {
            //  1. ACTIVATE OUR LOADING SCREEN
            PlayerUIManager.instance.playerUILoadingScreenManager.ActivateLoadingScreen();

            //  2. GET WORLD SCENE, AND LOAD IT
            string worldScene = SceneUtility.GetScenePathByBuildIndex(buildIndex);
            NetworkManager.Singleton.SceneManager.LoadScene(worldScene, LoadSceneMode.Single);

            //  3. LOAD OUR PLAYER SAVE DATA
            PlayerUIManager.instance.localPlayer.LoadGameDataFromCurrentCharacterData(ref WorldSaveGameManager.instance.currentCharacterData);
        }

        //  USED TO LOAD ADDITIVE SCENES IN OUR MAIN WORLD SCENE
        private void LoadAdditiveScene(string sceneName)
        {
            //  1. MAKE SURE THE SCENE IS NOT ALREADY LOADED
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                //  IF THE SCENE IN THE LIST IS NULL, CONTINUE TO LOOK AT OTHER SCENES
                if (loadedScenes[i] == null)
                    continue;

                //  IF THE SCENE IS ALREADY LOADED, ABORT
                if (loadedScenes[i].name == sceneName && loadedScenes[i].isLoaded)
                    return;
            }

            //  2. LOAD THE SCENE
            var loadSceneStatus = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

            //  3. LOAD EXTRAS IN THE SCENE (SUCH AS MONSTERS OR BREAKABLE OBJECTS)
        }

        //  USED TO LOAD MULTIPLE ADDITIVE SCENES AT ONCE WHEN ENTERING NEW AREA
        public void LoadAdditiveScenes(List<string> scenesToLoad)
        {
            if (!NetworkManager.IsServer)
                return;

            //  PASS ALL OF OUR SCENES TO LOAD TO OUR QUED SCENE LIST
            for (int i = 0; i < scenesToLoad.Count; i++)
            {
                quedSceneIDs.Add(scenesToLoad[i]);
            }

            quedScenesToLoad = quedSceneIDs.Count;

            if (loadingAdditiveScenesCoroutine != null)
                StopCoroutine(loadingAdditiveScenesCoroutine);

            loadingAdditiveScenesCoroutine = StartCoroutine(LoadAdditiveScenesCoroutine());
        }

        private IEnumerator LoadAdditiveScenesCoroutine()
        {
            // 1. CHECK TO SEE IF A SCENE IS CURRENTLY BEING LOADED/UNLOADED AND IF IT IS, WAIT
            for (int i = 0; i < quedSceneIDs.Count; i++)
            {
                while (sceneIsLoading || sceneIsUnloading)
                {
                    yield return null;
                }

                if (quedSceneIDs[i] == null)
                    continue;

                // 2. SORT THROUGH A "QUED" LIST OF SCENES, AND LOAD THEM ONE BY ONE
                LoadAdditiveScene(quedSceneIDs[i]);
                quedScenesToLoad--;

                yield return new WaitForFixedUpdate();
            }

            quedScenesToLoad = 0;
            loadingAdditiveScenesCoroutine = null;

            yield return null;
        }
    }
}
