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
                aiCharacter.transform.rotation = aiCharacter.navMeshAgent.transform.rotation;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (aiCharacter.IsOwner)
            {
                aiCharacter.characterNetworkManager.verticalMovement.Value = aiCharacter.GetAnimFloat("Vertical");
                aiCharacter.characterNetworkManager.horizontalMovement.Value = aiCharacter.GetAnimFloat("Horizontal");
            }
            else
            {
                aiCharacter.SetAnimFloat("Vertical", aiCharacter.aiCharacterNetworkManager.verticalMovement.Value, 0.1f, Time.deltaTime);
                aiCharacter.SetAnimFloat("Horizontal", aiCharacter.aiCharacterNetworkManager.horizontalMovement.Value, 0.1f, Time.deltaTime);
            }
        }
    }
}