using UnityEngine;
using Unity.Netcode;

namespace LZ
{
    public class BeaconDetector : MonoBehaviour
    {
        //  Q: WHAT IS A BEACON?
        //  A: A beacon is a trigger that will enable or disable objects/characters as it enters or leaves its radius
        //  this is especially useful for toggling AI characters that are not close to any player

        public PlayerManager player;

        private void OnTriggerEnter(Collider other)
        {
            
        }

        private void OnTriggerExit(Collider other)
        {
            AICharacterManager aiCharacter = other.GetComponent<AICharacterManager>();

            if (aiCharacter != null)
                aiCharacter.DeactivateCharacter(player);
        }
    }
}
