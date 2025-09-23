using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class PlayerInteractionManager : MonoBehaviour
    {
        PlayerManager player;

        private List<Interactable> currentInteractableActions; // 若使用 Unity 2022.3.11f1 版本请勿序列化，该版本在检视窗口中存在显示错误

        private void Awake()
        {
            player = GetComponent<PlayerManager>();
        }

        private void Start()
        {
            currentInteractableActions = new List<Interactable>();
        }

        private void FixedUpdate()
        {
            if (!player.IsOwner)
                return;

            // 当UI菜单未开启且无弹出提示（当前交互信息）时，检测可交互对象
            if (!PlayerUIManager.instance.menuWindowIsOpen && !PlayerUIManager.instance.popUpWindowIsOpen)
                CheckForInteractable();
        }

        private void CheckForInteractable()
        {
            if (currentInteractableActions.Count == 0)
                return;

            if (currentInteractableActions[0] == null)
            {
                currentInteractableActions.RemoveAt(0); // 若位于索引0的可交互项变为空（如被游戏移除），则从列表中删除该索引项
                return;
            }

            // 若存在可交互动作且尚未通知玩家，则在此进行通知
            if (currentInteractableActions[0] != null)
                PlayerUIManager.instance.playerUIPopUpManager.SendPlayerMessagePopUp(currentInteractableActions[0].interactableText);
        }

        private void RefreshInteractionList()
        {
            for (int i = currentInteractableActions.Count - 1; i > -1; i--)
            {
                if (currentInteractableActions[i] == null)
                    currentInteractableActions.RemoveAt(i);
            }
        }

        public void AddInteractionToList(Interactable interactableObject)
        {
            RefreshInteractionList();

            if (!currentInteractableActions.Contains(interactableObject))
                currentInteractableActions.Add(interactableObject);
        }

        public void RemoveInteractionFromList(Interactable interactableObject)
        {
            if (currentInteractableActions.Contains(interactableObject))
                currentInteractableActions.Remove(interactableObject);

            RefreshInteractionList();
        }

        public void Interact()
        {
            if (currentInteractableActions.Count == 0)
                return;

            if (currentInteractableActions[0] != null)
            {
                currentInteractableActions[0].Interact(player);
                RefreshInteractionList();
            }
        }
    }
}