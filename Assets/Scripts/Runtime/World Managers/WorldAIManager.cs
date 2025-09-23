using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

namespace LZ
{
    public class WorldAIManager : MonoBehaviour
    {
        public static WorldAIManager instance;

        [Header("Characters")]
        [SerializeField] List<AICharacterSpawner> aiCharacterSpawners;
        [SerializeField] private List<AICharacterManager> spawnedInCharacters;
        
        [Header("Bosses")]
        [SerializeField] List<AIBossCharacterManager> spawnedInBosses;

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
        
        public void ResetAllCharacters()
        {
            DespawnAllCharacters();

            foreach (var spawner in aiCharacterSpawners)
            {
                spawner.AttemptToSpawnCharacter();
            }
        }

        private void DespawnAllCharacters()
        {
            foreach (var character in spawnedInCharacters)
            {
                character.GetComponent<NetworkObject>().Despawn();
            }
            
            spawnedInCharacters.Clear();
        }

        private void DisableAllCharacters()
        {
            // TODO:禁用角色游戏对象，在网络中同步禁用状态
            // 如果禁用状态为真，则在客户端连接时禁用游戏对象
            // 可以用来禁用远离玩家的角色以节省内存
            // 注册表可以被分割成区域（AREA_00_，AREA_01，AREA_02）等
        }
    }
}