using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class EventTriggerBossFight : MonoBehaviour
    {
        [SerializeField] private int bossID;
        
        private void OnTriggerEnter(Collider other)
        {
            AIBossCharacterManager boss = WorldAIManager.instance.GetBossCharacterByID(bossID);

            if (boss != null)
            {
                boss.WakeBoss();
            }
        }
    }
}