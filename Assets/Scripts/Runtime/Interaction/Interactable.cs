using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace LZ
{
    public class Interactable : NetworkBehaviour
    {
        public string interactableText; // 进入交互碰撞体时显示的文本提示（拾取物品、拉杠杆等）
        [SerializeField] protected Collider interactableCollider;   // 用于检测玩家交互的碰撞体
        [SerializeField] protected bool hostOnlyInteractable = true;    // 启用后，该对象将无法被联机玩家交互

        protected virtual void Awake()
        {
            // 检查其是否为空，某些情况下可能需要手动将碰撞体指定为子对象（具体取决于可交互对象的设计）
            if (interactableCollider == null)
                interactableCollider = GetComponent<Collider>();
        }
        protected virtual void Start()
        {

        }

        public virtual void Interact(PlayerManager player)
        {
            if (!player.IsOwner)
                return;

            //  移除玩家身上的交互功能
            interactableCollider.enabled = false;
            player.playerInteractionManager.RemoveInteractionFromList(this);
            PlayerUIManager.instance.playerUIPopUpManager.CloseAllPopUpWindows();

            //  SAVE GAME AFTER INTERACTING
            WorldSaveGameManager.instance.SaveGame();
        }

        public virtual void OnTriggerEnter(Collider other)
        {
            PlayerManager player = other.GetComponent<PlayerManager>();

            if (player != null)
            {
                if (!player.playerNetworkManager.IsHost && hostOnlyInteractable)
                    return;

                if (!player.IsOwner)
                    return;

                //  向玩家传递交互指令
                player.playerInteractionManager.AddInteractionToList(this);
            }
        }

        public virtual void OnTriggerExit(Collider other)
        {
            PlayerManager player = other.GetComponent<PlayerManager>();

            if (player != null)
            {
                if (!player.playerNetworkManager.IsHost && hostOnlyInteractable)
                    return;

                if (!player.IsOwner)
                    return;

                //  移除玩家身上的交互功能
                player.playerInteractionManager.RemoveInteractionFromList(this);
                PlayerUIManager.instance.playerUIPopUpManager.CloseAllPopUpWindows();
            }
        }
    }
}