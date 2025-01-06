using UnityEngine;

namespace LZ
{
    public class PlayerEffectsManager : CharacterEffectsManager
    {
        [Header("Debug Delete Later")]
        [SerializeField] private InstantCharacterEffect effectToTest;
        [SerializeField] private bool processEffect;

        private void Update()
        {
            if (processEffect)
            {
                processEffect = false;
                InstantCharacterEffect effect = Instantiate(effectToTest);
                ProcessInstantEffect(effect);
            }
        }
    }
}