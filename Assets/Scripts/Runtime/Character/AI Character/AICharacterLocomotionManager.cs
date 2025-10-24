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
                aiCharacter.characterNetworkManager.verticalMovement.Value = aiCharacter.animator.GetFloat("Vertical");
                aiCharacter.characterNetworkManager.horizontalMovement.Value = aiCharacter.animator.GetFloat("Horizontal");
            }
            else
            {
                aiCharacter.animator.SetFloat("Vertical", aiCharacter.aiCharacterNetworkManager.verticalMovement.Value, 0.1f, Time.deltaTime);
                aiCharacter.animator.SetFloat("Horizontal", aiCharacter.aiCharacterNetworkManager.horizontalMovement.Value, 0.1f, Time.deltaTime);
            }
        }
    }
}