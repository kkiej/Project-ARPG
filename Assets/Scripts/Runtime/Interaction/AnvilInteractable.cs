using UnityEngine;

namespace LZ
{
    public class AnvilInteractable : Interactable
    {
        public override void Interact(PlayerManager player)
        {
            if (!player.IsOwner)
                return;

            //  移除玩家身上的交互功能
            player.playerInteractionManager.RemoveInteractionFromList(this);
            PlayerUIManager.instance.playerUIPopUpManager.CloseAllPopUpWindows();

            //  SAVE GAME AFTER INTERACTING
            WorldSaveGameManager.instance.SaveGame();
            
            if (player.IsOwner)
                PlayerUIManager.instance.playerUIWeaponUpgradeManager.OpenMenu();
        }

        public override void OnTriggerExit(Collider other)
        {
            PlayerManager player = other.GetComponent<PlayerManager>();

            if (player != null)
            {
                if (!player.playerNetworkManager.IsHost && hostOnlyInteractable)
                    return;

                if (!player.IsOwner)
                    return;
                
                // REMOVE THE INTERACTION FROM THE PLAYER
                player.playerInteractionManager.RemoveInteractionFromList(this);
                PlayerUIManager.instance.playerUIPopUpManager.CloseAllPopUpWindows();
                PlayerUIManager.instance.playerUIWeaponUpgradeManager.CloseMenu();
            }
        }
    }
}