using UnityEngine;

namespace LZ
{
    public class AICharacterSoundFXManager : CharacterSoundFXManager
    {
        [Header("Blocking SFX")]
        [SerializeField] AudioClip[] blockingSFX;

        public override void PlayBlockSoundFX()
        {
            if (blockingSFX.Length <= 0)
                return;

            PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(blockingSFX));
        }
    }
}
