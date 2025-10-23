using UnityEngine;

namespace LZ
{
    public class AIActivationBeacon : MonoBehaviour
    {
        [SerializeField] AICharacterManager beaconOwner;

        public void SetOwnerOfBeacon(AICharacterManager aiCharacter)
        {
            beaconOwner = aiCharacter;
        }

        public void ReactivateAICharacter(PlayerManager player)
        {
            if (beaconOwner == null)
                return;

            beaconOwner.ActivateCharacter(player);
        }

        private void OnTriggerEnter(Collider other)
        {
            BeaconDetector detector = other.GetComponent<BeaconDetector>();

            if (detector == null)
                return;

            ReactivateAICharacter(detector.player);
        }
    }
}
