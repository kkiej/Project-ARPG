using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "A.I/States/Boss Sleep")]
    public class BossSleepState : AIState
    {
        [SerializeField] string sleepAnimation = "Sleep_01";
        [SerializeField] string wakingAnimation = "Wake_01";
        private bool sleepAnimationSet = false;

        public bool hasBeenAwakened = false;

        public override AIState Tick(AICharacterManager aiCharacter)
        {
            //  IF THE BOSS HAS ALREADY BEEN INITIALLY "WOKEN UP" WE DONT WANT TO REPLY THE FIRST TIME WAKING ANIMATION
            aiCharacter.navMeshAgent.enabled = false;

            if (!hasBeenAwakened)
            {
                return HasNotBeenAwakened(aiCharacter);
            }
            else
            {
                return HasBeenAwakened(aiCharacter);
            }
        }

        private AIState HasBeenAwakened(AICharacterManager aiCharacter)
        {
            if (aiCharacter.characterCombatManager.currentTarget != null && !aiCharacter.aiCharacterNetworkManager.isAwake.Value)
            {
                aiCharacter.aiCharacterNetworkManager.isAwake.Value = true;
                return SwitchState(aiCharacter, aiCharacter.pursueTarget);
            }

            return this;
        }

        private AIState HasNotBeenAwakened(AICharacterManager aiCharacter)
        {
            aiCharacter.navMeshAgent.enabled = false;

            //  IF WE HAVENT SET OUR SLEEP ANIMATION, AND THE CHARACTER IS SLEEPING SET THE ANIMATION NOW
            if (!sleepAnimationSet && !aiCharacter.aiCharacterNetworkManager.isAwake.Value)
            {
                sleepAnimationSet = true;
                aiCharacter.aiCharacterNetworkManager.sleepingAnimation.Value = sleepAnimation;
                aiCharacter.aiCharacterNetworkManager.wakingAnimation.Value = wakingAnimation;
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(aiCharacter.aiCharacterNetworkManager.sleepingAnimation.Value.ToString(), true);
            }

            if (aiCharacter.characterCombatManager.currentTarget != null && !aiCharacter.aiCharacterNetworkManager.isAwake.Value)
            {
                aiCharacter.aiCharacterNetworkManager.isAwake.Value = true;

                if (!aiCharacter.isPerformingAction && !aiCharacter.isDead.Value)
                    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(aiCharacter.aiCharacterNetworkManager.wakingAnimation.Value.ToString(), true);

                return SwitchState(aiCharacter, aiCharacter.pursueTarget);
            }

            return this;
        }
    }
}