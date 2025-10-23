using UnityEngine;
using UnityEngine.AI;

namespace LZ
{
    [CreateAssetMenu(menuName = "A.I/States/Investigate Sound")]
    public class InvestigateSoundState : AIState
    {
        [Header("Flags")]
        [SerializeField] bool destinationSet = false;
        [SerializeField] bool destinationReached = false;

        [Header("Position")]
        public Vector3 positionOfSound = Vector3.zero;

        //  THE DEFAULT TIME THE A.I WILL STAND AROUND UPON REACHING THE SOURCE OF THE SOUND
        [Header("Investigation Timer")]
        [SerializeField] float investigationTime = 3;
        [SerializeField] float investigationTimer = 0;

        public override AIState Tick(AICharacterManager aiCharacter)
        {
            if (aiCharacter.isPerformingAction)
                return this;

            aiCharacter.aiCharacterCombatManager.FindATargetViaLineOfSight(aiCharacter);

            if (aiCharacter.aiCharacterCombatManager.currentTarget != null)
                return SwitchState(aiCharacter, aiCharacter.pursueTarget);

            if (!destinationSet)
            {
                destinationSet = true;
                aiCharacter.aiCharacterCombatManager.PivotTowardsPosition(aiCharacter, positionOfSound);
                aiCharacter.navMeshAgent.enabled = true;

                if (!IsDestinationReachable(aiCharacter, positionOfSound))
                {
                    NavMeshHit hit;

                    if (NavMesh.SamplePosition(positionOfSound, out hit, 2, NavMesh.AllAreas))
                    {
                        NavMeshPath partialPath = new NavMeshPath();
                        aiCharacter.navMeshAgent.CalculatePath(hit.position, partialPath);
                        aiCharacter.navMeshAgent.SetPath(partialPath);
                    }
                }
                else
                {
                    NavMeshPath path = new NavMeshPath();
                    aiCharacter.navMeshAgent.CalculatePath(positionOfSound, path);
                    aiCharacter.navMeshAgent.SetPath(path);
                }
            }

            aiCharacter.aiCharacterCombatManager.RotateTowardsAgent(aiCharacter);

            float distanceFromDestination = Vector3.Distance(aiCharacter.transform.position, positionOfSound);

            //  YOU CAN USE THIS FLAG TO DO OTHER THINGS ON ARRIVAL 
            if (distanceFromDestination <= aiCharacter.navMeshAgent.stoppingDistance)
                destinationReached = true;

            if (destinationReached)
            {
                if (investigationTimer < investigationTime)
                {
                    investigationTimer += Time.deltaTime;
                }
                else
                {
                    return SwitchState(aiCharacter, aiCharacter.idle);
                }
            }

            return this;
        }

        protected override void ResetStateFlags(AICharacterManager aiCharacter)
        {
            base.ResetStateFlags(aiCharacter);

            aiCharacter.navMeshAgent.enabled = false;
            destinationReached = false;
            destinationSet = false;
            investigationTimer = 0;
            positionOfSound = Vector3.zero;
        }
    }
}
