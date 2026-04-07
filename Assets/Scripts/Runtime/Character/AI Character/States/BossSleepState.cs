using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "A.I/States/Boss Sleep")]
    public class BossSleepState : AIState
    {
        [SerializeField] AnimationClip sleepClip;
        [SerializeField] AnimationClip wakeClip;
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

            if (!sleepAnimationSet && !aiCharacter.aiCharacterNetworkManager.isAwake.Value)
            {
                sleepAnimationSet = true;
                if (sleepClip != null)
                {
                    aiCharacter.aiCharacterNetworkManager.sleepingAnimation.Value = sleepClip.name;
                    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(sleepClip, true);
                }
                if (wakeClip != null)
                    aiCharacter.aiCharacterNetworkManager.wakingAnimation.Value = wakeClip.name;
            }

            if (aiCharacter.characterCombatManager.currentTarget != null && !aiCharacter.aiCharacterNetworkManager.isAwake.Value)
            {
                aiCharacter.aiCharacterNetworkManager.isAwake.Value = true;

                if (!aiCharacter.isPerformingAction && !aiCharacter.isDead.Value && wakeClip != null)
                    aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(wakeClip, true);

                return SwitchState(aiCharacter, aiCharacter.pursueTarget);
            }

            return this;
        }
    }
}