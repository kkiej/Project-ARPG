using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace LZ
{
    public class FogWallInteractable : Interactable
    {
        [Header("Fog")]
        [SerializeField] GameObject[] fogGameObjects;

        [Header("Collision")]
        [SerializeField] Collider fogWallCollider;
        
        [Header("I.D")]
        public int fogWallID;
        
        [Header("Sound")]
        private AudioSource fogWallAudioSource;
        [SerializeField] AudioClip fogWallSFX;

        [Header("Active")]
        public NetworkVariable<bool> isActive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        protected override void Awake()
        {
            base.Awake();

            fogWallAudioSource = gameObject.GetComponent<AudioSource>();
        }

        public override void Interact(PlayerManager player)
        {
            base.Interact(player);

            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward);
            player.transform.rotation = targetRotation;

            AllowPlayerThroughFogWallCollidersServerRpc(player.NetworkObjectId);
            player.playerAnimatorManager.PlayTargetActionAnimation("Pass_Through_Fog_01", true);
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            OnIsActiveChanged(false, isActive.Value);
            isActive.OnValueChanged += OnIsActiveChanged;
            WorldObjectManager.instance.AddFogWallToList(this);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            isActive.OnValueChanged -= OnIsActiveChanged;
            WorldObjectManager.instance.RemoveFogWallFromList(this);
        }

        private void OnIsActiveChanged(bool oldStatus, bool newStatus)
        {
            if (isActive.Value)
            {
                foreach (var fogObject in fogGameObjects)
                {
                    fogObject.SetActive(true);
                }
            }
            else
            {
                foreach (var fogObject in fogGameObjects)
                {
                    fogObject.SetActive(false);
                }
            }
        }
        
        // 当服务器RPC不要求所有权时，非所有者可触发该功能（客户端玩家不拥有雾墙所有权，因为他们不是主机）
        [ServerRpc(RequireOwnership = false)]
        private void AllowPlayerThroughFogWallCollidersServerRpc(ulong playerObjectID)
        {
            if (IsServer)
            {
                AllowPlayerThroughFogWallCollidersClientRpc(playerObjectID);
            }
        }

        [ClientRpc]
        private void AllowPlayerThroughFogWallCollidersClientRpc(ulong playerObjectID)
        {
            PlayerManager player = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerObjectID].GetComponent<PlayerManager>();

            fogWallAudioSource.PlayOneShot(fogWallSFX);

            if (player != null)
                StartCoroutine(DisableCollisionForTime(player));
        }

        private IEnumerator DisableCollisionForTime(PlayerManager player)
        {
            // 使此函数的执行时长与穿过雾墙的动画时长保持一致
            Physics.IgnoreCollision(player.characterController, fogWallCollider, true);
            yield return new WaitForSeconds(3);
            Physics.IgnoreCollision(player.characterController, fogWallCollider, false);
        }
    }
}