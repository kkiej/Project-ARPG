using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

namespace LZ
{
    public class WorldAIManager : MonoBehaviour
    {
        public static WorldAIManager instance;

        [Header("Loading")]
        public bool isPerformingLoadingOperation = false;

        [Header("Characters")]
        [SerializeField] List<AICharacterSpawner> aiCharacterSpawners;
        [SerializeField] List<AICharacterManager> spawnedInCharacters;
        private Coroutine spawnAllCharactersCoroutine;
        private Coroutine despawnAllCharactersCoroutine;
        private Coroutine resetAllCharactersCoroutine;

        [Header("Beacon Prefab")]
        public GameObject beaconGameObject;

        [Header("Bosses")]
        [SerializeField] List<AIBossCharacterManager> spawnedInBosses;

        [Header("Patrol Paths")]
        [SerializeField] List<AIPatrolPath> aiPatrolPaths = new List<AIPatrolPath>(); 

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
        
        public void SpawnCharacter(AICharacterSpawner aiCharacterSpawner)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                aiCharacterSpawners.Add(aiCharacterSpawner);
                aiCharacterSpawner.AttemptToSpawnCharacter();
            }
        }
        
        public void AddCharacterToSpawnedCharactersList(AICharacterManager character)
        {
            if (spawnedInCharacters.Contains(character))
                return;

            spawnedInCharacters.Add(character);

            AIBossCharacterManager bossCharacter = character as AIBossCharacterManager;

            if (bossCharacter != null)
            {
                if (spawnedInBosses.Contains(bossCharacter))
                    return;

                spawnedInBosses.Add(bossCharacter);
            }
        }
        
        public AIBossCharacterManager GetBossCharacterByID(int ID)
        {
            return spawnedInBosses.FirstOrDefault(boss => boss.bossID == ID);
        }

        //  TODO: IF YOU HAVE MORE THAN 25-30 ENEMIES PER AREA, RESET THEIR STATS AND ANIMATIONS INSTEAD OF DESPAWNING AND RESPAWNING
        public void SpawnAllCharacters()
        {
            isPerformingLoadingOperation = true;

            if (spawnAllCharactersCoroutine != null)
                StopCoroutine(spawnAllCharactersCoroutine);

            spawnAllCharactersCoroutine = StartCoroutine(SpawnAllCharactersCoroutine());
        }

        private IEnumerator SpawnAllCharactersCoroutine()
        {
            for (int i = 0; i < aiCharacterSpawners.Count; i++)
            {
                yield return new WaitForFixedUpdate();

                aiCharacterSpawners[i].AttemptToSpawnCharacter();

                yield return null;
            }

            isPerformingLoadingOperation = false;

            yield return null;
        }

        public void ResetAllCharacters()
        {
            isPerformingLoadingOperation = true;

            if (resetAllCharactersCoroutine != null)
                StopCoroutine(resetAllCharactersCoroutine);

            resetAllCharactersCoroutine = StartCoroutine(ResetAllCharactersCoroutine());
        }

        private IEnumerator ResetAllCharactersCoroutine()
        {
            for (int i = 0; i < aiCharacterSpawners.Count; i++)
            {
                yield return new WaitForFixedUpdate();

                aiCharacterSpawners[i].ResetCharacter();

                yield return null;
            }

            isPerformingLoadingOperation = false;

            yield return null;
        }

        private void DespawnAllCharacters()
        {
            isPerformingLoadingOperation = true;

            if (despawnAllCharactersCoroutine != null)
                StopCoroutine(despawnAllCharactersCoroutine);

            despawnAllCharactersCoroutine = StartCoroutine(DespawnAllCharactersCoroutine());
        }

        private IEnumerator DespawnAllCharactersCoroutine()
        {
            for (int i = 0; i < spawnedInCharacters.Count; i++)
            {
                yield return new WaitForFixedUpdate();

                spawnedInCharacters[i].GetComponent<NetworkObject>().Despawn();

                yield return null;
            }

            spawnedInCharacters.Clear();
            isPerformingLoadingOperation = false;

            yield return null;
        }

        private void DisableAllCharacters()
        {
            // TODO:禁用角色游戏对象，在网络中同步禁用状态
            // 如果禁用状态为真，则在客户端连接时禁用游戏对象
            // 可以用来禁用远离玩家的角色以节省内存
            // 注册表可以被分割成区域（AREA_00_，AREA_01，AREA_02）等
        }

        public void DisableAllBossFights()
        {
            for (int i = 0; i < spawnedInBosses.Count; i++)
            {
                if (spawnedInBosses[i] == null)
                    continue;

                spawnedInBosses[i].bossFightIsActive.Value = false;
            }
        }

        //  PATROL PATHS
        public void AddPatrolPathToList(AIPatrolPath patrolPath)
        {
            if (aiPatrolPaths.Contains(patrolPath))
                return;

            aiPatrolPaths.Add(patrolPath);
        }

        public AIPatrolPath GetAIPatrolPathByID(int patrolPathID)
        {
            AIPatrolPath patrolPath = null;

            for (int i = 0; i < aiPatrolPaths.Count; i++)
            {
                if (aiPatrolPaths[i].patrolPathID == patrolPathID)
                    patrolPath = aiPatrolPaths[i];
            }

            return patrolPath;
        }
    }
}