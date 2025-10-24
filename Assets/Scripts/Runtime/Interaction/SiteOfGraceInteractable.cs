using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace LZ
{
    public class SiteOfGraceInteractable : Interactable
    {
        [Header("Site Of Grace Info")]
        public int siteOfGraceID;
        public NetworkVariable<bool> isActivated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [Header("VFX")]
        [SerializeField] GameObject activatedParticles;

        [Header("Interaction Text")]
        [SerializeField] string unactivatedInteractionText = "Restore Site Of Grace";
        [SerializeField] string activatedInteractionText = "Rest";

        [Header("Teleport Transform")]
        [SerializeField] Transform teleportTransform;

        protected override void Start()
        {
            base.Start();

            if (IsOwner)
            {
                if (WorldSaveGameManager.instance.currentCharacterData.sitesOfGrace.ContainsKey(siteOfGraceID))
                {
                    isActivated.Value = WorldSaveGameManager.instance.currentCharacterData.sitesOfGrace[siteOfGraceID];
                }
                else
                {
                    isActivated.Value = false;
                }
            }

            if (isActivated.Value)
            {
                interactableText = activatedInteractionText;
            }
            else
            {
                interactableText = unactivatedInteractionText;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // 若加入时状态已变更，则需在连接建立时强制触发OnChange函数
            if (!IsOwner)
                OnIsActivatedChanged(false, isActivated.Value);

            isActivated.OnValueChanged += OnIsActivatedChanged;

            WorldObjectManager.instance.AddSiteOfGraceToList(this);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            isActivated.OnValueChanged -= OnIsActivatedChanged;
        }

        private void RestoreSiteOfGrace(PlayerManager player)
        {
            isActivated.Value = true;

            // 如果存档文件包含此赐福点（Site of Grace）的信息，则将其移除
            if (WorldSaveGameManager.instance.currentCharacterData.sitesOfGrace.ContainsKey(siteOfGraceID))
                WorldSaveGameManager.instance.currentCharacterData.sitesOfGrace.Remove(siteOfGraceID);

            // 然后以值为“true”（表示已激活）重新添加
            WorldSaveGameManager.instance.currentCharacterData.sitesOfGrace.Add(siteOfGraceID, true);

            player.playerAnimatorManager.PlayTargetActionAnimation("Activate_Site_Of_Grace_01", true);
            // 播放动画时若需隐藏武器模型，可在此设置

            PlayerUIManager.instance.playerUIPopUpManager.SendGraceRestoredPopUp("SITE OF GRACE RESTORED");

            StartCoroutine(WaitForAnimationAndPopUpThenRestoreCollider());
        }

        private void RestAtSiteOfGrace(PlayerManager player)
        {
            PlayerUIManager.instance.playerUISiteOfGraceManager.OpenMenu();

            // 临时代码段
            interactableCollider.enabled = true; // 此处临时重新启用碰撞体（待正式菜单添加后将移除），以便实现无限刷怪功能
            player.playerNetworkManager.currentHealth.Value = player.playerNetworkManager.maxHealth.Value;
            player.playerNetworkManager.currentStamina.Value = player.playerNetworkManager.maxStamina.Value;


            // 补充血瓶（待实现）
            // 更新/强制移动任务角色（待实现）
            WorldAIManager.instance.ResetAllCharacters();
        }

        private IEnumerator WaitForAnimationAndPopUpThenRestoreCollider()
        {
            yield return new WaitForSeconds(2); // 此处应预留足够时间以确保动画完整播放且弹出提示开始淡出
            interactableCollider.enabled = true;
        }

        private void OnIsActivatedChanged(bool oldStatus, bool newStatus)
        {
            if (isActivated.Value)
            {
                // 此处可播放特效或启用灯光等效果，以指示该检查点已激活
                activatedParticles.SetActive(true);
                interactableText = activatedInteractionText;
            }
            else
            {
                interactableText = unactivatedInteractionText;
            }
        }

        public override void Interact(PlayerManager player)
        {
            base.Interact(player);

            if (player.isPerformingAction)
                return;

            if (player.playerCombatManager.isUsingItem)
                return;

            WorldSaveGameManager.instance.currentCharacterData.lastSiteOfGraceRestedAt = siteOfGraceID;

            if (player.IsHost)
                player.playerNetworkManager.lastSiteOfGraceUsed.Value = siteOfGraceID;

            if (!isActivated.Value)
            {
                RestoreSiteOfGrace(player);
            }
            else
            {
                RestAtSiteOfGrace(player);
            }
        }

        public void TeleportToSiteOfGrace()
        {
            //  THE PLAYER IS ONLY ABLE TO TELEPORT WHEN NOT IN A CO-OP GAME SO WE CAN GRAB THE LOCAL PLAYER FROM THE NETWORK MANAGER
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  ENABLE LOADING SCREEN
            PlayerUIManager.instance.playerUILoadingScreenManager.ActivateLoadingScreen();

            //  TELEPORT PLAYER
            player.transform.position = teleportTransform.position;

            //  DISABLE LOADING SCREEN
            PlayerUIManager.instance.playerUILoadingScreenManager.DeactivateLoadingScreen(1);
        }
    }
}