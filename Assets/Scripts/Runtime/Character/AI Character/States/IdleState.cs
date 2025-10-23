using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LZ
{
    [CreateAssetMenu(menuName = "A.I/States/Idle")]
    public class IdleState : AIState
    {
        [Header("Idle Options")]
        public IdleStateMode idleStateMode;

        [Header("Patrol Options")]
        public AIPatrolPath aiPatrolPath;
        [SerializeField] bool hasFoundClosestPointNearCharacterSpawn = false;   // IF THE CHARACTER SPAWNS CLOSER TO THE SECOND POINT, START AT THE SECOND POINT
        [SerializeField] bool patrolComplete = false;   // HAVE WE FINISHED THE ENTIRE PATROL YET
        [SerializeField] bool repeatPatrol = false;     // UPON FINISHING, DO WE REPEAT THE PATH AGAIN
        [SerializeField] int patrolDestinationIdex;     // WHICH POINT OF THE PATROL ARE WE CURRENTLY WORKING TOWARDS
        [SerializeField] bool hasPatrolDestination = false;     // DO WE HAVE A POINT WE ARE CURRENTLY WORKING TOWARDS
        [SerializeField] Vector3 currentPatrolDestination;      // THE SPECIFIC DESTINATION COORDS WE ARE HEADING TOWARDS
        [SerializeField] float distanceFromCurrentDestination;  //  THE DISTANCE FROM THE A.I CHARACTER TO THE DESTINATION
        [SerializeField] float timeBetweenPatrols = 15;              //  MINIMUM TIME BEFORE STARTING A NEW PATROL
        [SerializeField] float restTimer = 0;                       //  ACTIVE TIMER COUNTING THE TIME RESTED

        [Header("Sleep Options")]
        public bool willInvestigateSound = true;
        private bool sleepAnimationSet = false;
        [SerializeField] string sleepAnimation = "Sleep_01";
        [SerializeField] string wakingAnimation = "Wake_01";

        public override AIState Tick(AICharacterManager aiCharacter)
        {
            if (aiCharacter.aiCharacterNetworkManager.isAwake.Value)
                aiCharacter.aiCharacterCombatManager.FindATargetViaLineOfSight(aiCharacter);

            switch (idleStateMode)
            {
                case IdleStateMode.Idle: return Idle(aiCharacter);
                case IdleStateMode.Patrol: return Patrol(aiCharacter);
                case IdleStateMode.Sleep: return SleepUntilDisturbed(aiCharacter);
                default:
                    break;
            }

            return this;
        }

        protected virtual AIState Idle(AICharacterManager aiCharacter)
        {
            if (aiCharacter.characterCombatManager.currentTarget != null)
            {
                return SwitchState(aiCharacter, aiCharacter.pursueTarget);
            }
            else
            {
                //  RETURN THIS STATE, TO CONTINUALLY SEARCH FOR A TARGET (KEEP THE STATE HERE, UNTIL A TARGET IS FOUND)
                return this;
            }
        }

        protected virtual AIState Patrol(AICharacterManager aiCharacter)
        {
            if (!aiCharacter.aiCharacterLocomotionManager.isGrounded)
                return this;

            if (aiCharacter.isPerformingAction)
            {
                aiCharacter.navMeshAgent.enabled = false;
                aiCharacter.characterNetworkManager.isMoving.Value = false;
                return this;
            }

            if (!aiCharacter.navMeshAgent.enabled)
                aiCharacter.navMeshAgent.enabled = true;

            if (aiCharacter.aiCharacterCombatManager.currentTarget != null)
                return SwitchState(aiCharacter, aiCharacter.pursueTarget);

            //  IF OUR PATROL IS COMPLETE AND WE WILL REPEAT IT CHECK FOR REST TIME
            if (patrolComplete && repeatPatrol)
            {
                //  IF THE TIME HAS NOT EXCEEDED ITS SET LIMIT, STOP AND WAIT
                if (timeBetweenPatrols > restTimer)
                {
                    aiCharacter.navMeshAgent.enabled = false;
                    aiCharacter.characterNetworkManager.isMoving.Value = false;
                    restTimer += Time.deltaTime;
                }
                else
                {
                    patrolDestinationIdex = -1;
                    hasPatrolDestination = false;
                    currentPatrolDestination = aiCharacter.transform.position;
                    patrolComplete = false;
                    restTimer = 0;
                }
            }
            else if (patrolComplete && !repeatPatrol)
            {
                aiCharacter.navMeshAgent.enabled = false;
                aiCharacter.characterNetworkManager.isMoving.Value = false;
            }

            //  IF WE HAVE A DESTINATION, MOVE TOWARDS IT
            if (hasPatrolDestination)
            {
                distanceFromCurrentDestination = Vector3.Distance(aiCharacter.transform.position, currentPatrolDestination);

                if (distanceFromCurrentDestination > 2)
                {
                    aiCharacter.navMeshAgent.enabled = true;
                    aiCharacter.aiCharacterLocomotionManager.RotateTowardsAgent(aiCharacter);
                }
                else
                {
                    currentPatrolDestination = aiCharacter.transform.position;
                    hasPatrolDestination = false;
                }
            }
            //  OTHERWISE, GET A NEW DESTINATION
            else
            {
                patrolDestinationIdex += 1;

                if (patrolDestinationIdex > aiPatrolPath.patrolPoints.Count - 1)
                {
                    patrolComplete = true;
                    return this;
                }

                if (!hasFoundClosestPointNearCharacterSpawn)
                {
                    hasFoundClosestPointNearCharacterSpawn = true;
                    float closestDistance = Mathf.Infinity;

                    for (int i = 0; i < aiPatrolPath.patrolPoints.Count; i++)
                    {
                        float distanceFromThisPoint = Vector3.Distance(aiCharacter.transform.position, aiPatrolPath.patrolPoints[i]);

                        if (distanceFromThisPoint < closestDistance)
                        {
                            closestDistance = distanceFromThisPoint;
                            patrolDestinationIdex = i;
                            currentPatrolDestination = aiPatrolPath.patrolPoints[i];
                        }
                    }
                }
                else
                {
                    currentPatrolDestination = aiPatrolPath.patrolPoints[patrolDestinationIdex];
                }

                hasPatrolDestination = true;
            }

            NavMeshPath path = new NavMeshPath();
            aiCharacter.navMeshAgent.CalculatePath(currentPatrolDestination, path);
            aiCharacter.navMeshAgent.SetPath(path);

            return this;
        }

        protected virtual AIState SleepUntilDisturbed(AICharacterManager aiCharacter)
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

        protected override void ResetStateFlags(AICharacterManager aiCharacter)
        {
            base.ResetStateFlags(aiCharacter);

            sleepAnimationSet = false;
        }
    }
}