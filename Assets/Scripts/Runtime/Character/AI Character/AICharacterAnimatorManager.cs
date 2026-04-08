using UnityEngine;

namespace LZ
{
    public class AICharacterAnimatorManager : CharacterAnimatorManager
    {
        private AICharacterManager aiCharacter;

        protected override void Awake()
        {
            base.Awake();

            aiCharacter = GetComponent<AICharacterManager>();
        }

        protected override bool ShouldApplyRootMotion()
        {
            return aiCharacter.characterLocomotionManager.isGrounded;
        }
    }
}
