using UnityEngine;

namespace LZ
{
    public class PickUpRunesInteractable : Interactable
    {
        public int runeCount = 0;

        public override void Interact(PlayerManager player)
        {
            WorldSaveGameManager.instance.currentCharacterData.hasDeadSpot = false;
            player.playerStatsManager.AddRunes(runeCount);
            Destroy(gameObject);
        }
    }
}
