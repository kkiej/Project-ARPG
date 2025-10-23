using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace LZ
{
    public class EventTriggeWakeNearbyCharacters : MonoBehaviour
    {
        [SerializeField] float awakenRadius = 8;

        private void OnTriggerEnter(Collider other)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            PlayerManager player = other.GetComponent<PlayerManager>();

            if (player == null)
                return;

            //  TODO CHECK FOR FRIENDLY (NON INVADER)

            //  TODO CHECK FOR SNEAKING/HIDDEN

            Collider[] creaturesInRadius = Physics.OverlapSphere(transform.position, awakenRadius, WorldUtilityManager.Instance.GetCharacterLayers());
            List<AICharacterManager> creaturesToWake = new List<AICharacterManager>();

            for (int i = 0; i < creaturesInRadius.Length; i++)
            {
                AICharacterManager aiCharacter = creaturesInRadius[i].GetComponentInParent<AICharacterManager>();

                if (aiCharacter == null)
                    continue;

                if (aiCharacter.isDead.Value)
                    continue;

                if (aiCharacter.aiCharacterNetworkManager.isAwake.Value)
                    continue;

                if (!creaturesToWake.Contains(aiCharacter))
                    creaturesToWake.Add(aiCharacter);
            }

            for (int i = 0; i < creaturesToWake.Count; i++)
            {
                creaturesToWake[i].aiCharacterCombatManager.SetTarget(player);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, awakenRadius);
        }
    }
}
