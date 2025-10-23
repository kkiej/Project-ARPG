using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LZ
{
    public class AIState : ScriptableObject
    {
        public virtual AIState Tick(AICharacterManager aiCharacter)
        {
            return this;
        }

        public virtual AIState SwitchState(AICharacterManager aiCharacter, AIState newState)
        {
            ResetStateFlags(aiCharacter);
            return newState;
        }

        protected virtual void ResetStateFlags(AICharacterManager aiCharacter)
        {
            // 重置任何状态标志，以便当你返回到该状态时，它们会再次变为空白
        }

        public bool IsDestinationReachable(AICharacterManager aiCharacter, Vector3 destination)
        {
            aiCharacter.navMeshAgent.enabled = true;

            NavMeshPath navMeshPath = new NavMeshPath();

            if (aiCharacter.navMeshAgent.CalculatePath(destination, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}