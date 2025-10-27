using UnityEngine;
using Unity.Netcode;

namespace LZ
{
    public class EventTriggerLoadScene : MonoBehaviour
    {
        [Header("Area")]
        [SerializeField] WorldSceneLocation area;

        private void OnTriggerEnter(Collider other)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;

            PlayerManager player = other.GetComponent<PlayerManager>();

            if (player == null)
                return;

            AddPlayerToArea(player);
        }

        private void AddPlayerToArea(PlayerManager player)
        {
            WorldSubsceneManager.instance.LoadAreasBasedOnAreaCurrentIn(area, player);
        }
    }
}
