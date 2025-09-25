using UnityEngine;

namespace LZ
{
    public class PlayerSoundFXManager : CharacterSoundFXManager
    {
        PlayerManager player;

        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
        }

        public override void PlayBlockSoundFX()
        {
            PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(player.playerCombatManager.currentWeaponBeingUsed.blocking));
        }
    }
}