using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class AICharacterLocomotionManager : CharacterLocomotionManager
    {
        AICharacterManager aiCharacter;

        protected override void Awake()
        {
            base.Awake();

            aiCharacter = GetComponent<AICharacterManager>();
        }

        public void RotateTowardsAgent(AICharacterManager aiCharacter)
        {
            if (aiCharacter.aiCharacterNetworkManager.isMoving.Value)
            {
                Quaternion agentRotation = aiCharacter.navMeshAgent.transform.rotation;

                if (aiCharacter.kcc != null)
                    aiCharacter.kcc.SetTargetRotation(agentRotation);
                else
                    aiCharacter.transform.rotation = agentRotation;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (aiCharacter.IsOwner)
            {
                var param = aiCharacter.characterAnimatorManager.GetCurrentMixerParameter();
                aiCharacter.characterNetworkManager.horizontalMovement.Value = param.x;
                aiCharacter.characterNetworkManager.verticalMovement.Value = param.y;
            }
            else
            {
                aiCharacter.characterAnimatorManager.SetAnimatorMovementParameters(
                    aiCharacter.aiCharacterNetworkManager.horizontalMovement.Value,
                    aiCharacter.aiCharacterNetworkManager.verticalMovement.Value);
            }
        }
    }
}