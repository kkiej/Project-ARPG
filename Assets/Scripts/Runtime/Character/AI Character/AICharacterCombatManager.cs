using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace LZ
{
    public class AICharacterCombatManager : CharacterCombatManager
    {
        protected AICharacterManager aiCharacter;

        [Header("Damage")]
        [SerializeField] protected int baseDamage = 25;
        [SerializeField] protected int basePoiseDamage = 25;

        [Header("Action Recovery")]
        public float actionRecoveryTimer = 0;

        [Header("Pivot")]
        public bool enablePivot = true;
        
        [Header("Target Information")]
        public float distanceFromTarget;
        public float viewableAngle;
        public Vector3 targetsDirection;
        
        [Header("Detection")]
        [SerializeField] float detectionRadius = 15;
        public float minimumFOV = -35;
        public float maximumFOV = 35;

        [Header("Attack Rotation Speed")]
        public float attackRotationSpeed = 25;

        [Header("Stance Settings")]
        public float maxStance = 150;
        public float currentStance;
        [SerializeField] float stanceRegeneratedPersecond = 15;
        [SerializeField] bool ignoreStanceBreak = false;

        [Header("Stance Timer")]
        [SerializeField] float stanceRegenerationTimer = 0;
        private float stanceTickTimer = 0; 
        [SerializeField] float defaultTimeUntilStanceRegenerationBegins = 15;

        [Header("Activation Range")]
        public List<PlayerManager> playersWithinActivationRange = new List<PlayerManager>();


        protected override void Awake()
        {
            base.Awake();

            aiCharacter = GetComponent<AICharacterManager>();
            lockOnTransform = GetComponentInChildren<LockOnTransform>().transform;
        }

        private void Update()
        {
            HandleStanceBreak();
        }

        public void AddPlayerToPlayersWithinRange(PlayerManager player)
        {
            if (playersWithinActivationRange.Contains(player))
                return;

            playersWithinActivationRange.Add(player);

            for (int i = 0; i < playersWithinActivationRange.Count; i++)
            {
                if (playersWithinActivationRange[i] == null)
                    playersWithinActivationRange.RemoveAt(i);
            }
        }

        public void RemovePlayerFromPlayersWithinRange(PlayerManager player)
        {
            if (!playersWithinActivationRange.Contains(player))
                return;

            playersWithinActivationRange.Remove(player);

            for (int i = 0; i < playersWithinActivationRange.Count; i++)
            {
                if (playersWithinActivationRange[i] == null)
                    playersWithinActivationRange.RemoveAt(i);
            }
        }

        public void AwardRunesOnDeath(PlayerManager player)
        {
            // 1. CHECK IF PLAYER IS FRIENDLY TO HOST (NOT AN INVADER)
            if (player.characterGroup == CharacterGroup.Team02)
                return;

            // 2. IF YOU WANT TO GIVE LESS OR MORE RUNES TO A CLIENT VS A HOST, DO IT HERE
            //if (NetworkManager.Singleton.IsHost)
            //{

            //}

            // 3. AWARD RUNES (CONSIDER RUNE ALTERING ITEMS HERE OR EFFECTS THAT GIVE MORE OR LESS RUNES
            player.playerStatsManager.AddRunes(aiCharacter.characterStatsManager.runesDroppedOnDeath);
        }

        private void HandleStanceBreak()
        {
            if (!aiCharacter.IsOwner)
                return;

            if (aiCharacter.isDead.Value)
                return;

            if (stanceRegenerationTimer > 0)
            {
                stanceRegenerationTimer -= Time.deltaTime;
            }
            else
            {
                stanceRegenerationTimer = 0;

                if (currentStance < maxStance)
                {
                    //  BEGIN ADDING STANCE EACH TICK
                    stanceTickTimer += Time.deltaTime;

                    if (stanceTickTimer >= 1)
                    {
                        stanceTickTimer = 0;
                        currentStance += stanceRegeneratedPersecond;
                    }
                }
                else
                {
                    currentStance = maxStance;
                }
            }

            if (currentStance <= 0)
            {
                //  (OPTIONAL) IF WE ARE IN A VERY HIGH INTENSITY DAMAGE ANIMATION (LIKE BEING LAUNCHED INTO THE AIR) DO NOT PLAY THE STANCE BREAK ANIMATION
                //  THIS WOULD FEEL LESS IMPACTFUL IN GAMEPLAY
                DamageIntensity previousDamageIntensity = WorldUtilityManager.Instance.GetDamageIntensityBasedOnPoiseDamage(previousPoiseDamageTaken);

                if (previousDamageIntensity == DamageIntensity.Colossal)
                {
                    currentStance = 1;
                    return;
                }

                //  TO DO: IF WE ARE BEING BACKSTABBED/RIPOSTED (CRITICALLY DAMAGED) DO NOT PLAY THE STANCE BREAK ANIMATION, AS THIS WOULD BREAK THE STATE

                currentStance = maxStance;

                if (ignoreStanceBreak)
                    return;

                aiCharacter.characterAnimatorManager.PlayTargetActionAnimationInstantly("Stance_Break_01", true);
            }
        }

        public void DamageStance(int stanceDamage)
        {
            //  WHEN STANCE IS DAMAGED, THE TIMER IS RESET, MEANING CONSTANT ATTACKS GIVE NO CHANCE AT RECOVERING STANCE THAT IS LOST
            stanceRegenerationTimer = defaultTimeUntilStanceRegenerationBegins;

            currentStance -= stanceDamage;
        }

        public virtual void AlertCharacterToSound(Vector3 positionOfSound)
        {
            if (!aiCharacter.IsOwner)
                return;

            if (aiCharacter.isDead.Value)
                return;

            if (aiCharacter.idle == null)
                return;

            if (aiCharacter.investigateSound == null)
                return;

            if (!aiCharacter.idle.willInvestigateSound)
                return;

            //  IF THEY ARE SLEEPING, HERE IS WHERE YOU WAKE THEM UP
            if (aiCharacter.idle.idleStateMode == IdleStateMode.Sleep && !aiCharacter.aiCharacterNetworkManager.isAwake.Value)
            {
                aiCharacter.aiCharacterNetworkManager.isAwake.Value = true;
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(aiCharacter.aiCharacterNetworkManager.wakingAnimation.Value.ToString(), true);
            }

            aiCharacter.investigateSound.positionOfSound = positionOfSound;
            aiCharacter.currentState = aiCharacter.currentState.SwitchState(aiCharacter, aiCharacter.investigateSound);
        }

        public virtual void FindATargetViaLineOfSight(AICharacterManager aiCharacter)
        {
            if (currentTarget != null)
                return;

            Collider[] colliders = Physics.OverlapSphere(aiCharacter.transform.position, detectionRadius, WorldUtilityManager.Instance.GetCharacterLayers());

            for (int i = 0; i < colliders.Length; i++)
            {
                CharacterManager targetCharacter = colliders[i].transform.GetComponent<CharacterManager>();
                
                if (targetCharacter == null)
                    continue;
                
                if (targetCharacter == aiCharacter)
                    continue;
                
                if (targetCharacter.isDead.Value)
                    continue;
                
                // 可以攻击这些角色吗？可以的话，把他们设置成目标
                if (WorldUtilityManager.Instance.CanIDamageThisTarget(aiCharacter.characterGroup, targetCharacter.characterGroup))
                {
                    // 可找到的潜在目标，必须是在我们面前的
                    Vector3 targetsDirection = targetCharacter.transform.position - aiCharacter.transform.position;
                    float angleOfPotentialTarget = Vector3.Angle(targetsDirection, aiCharacter.transform.forward);

                    if (angleOfPotentialTarget > minimumFOV && angleOfPotentialTarget < maximumFOV)
                    {
                        // 最后检查一下环境遮挡
                        if (Physics.Linecast(aiCharacter.characterCombatManager.lockOnTransform.position,
                                targetCharacter.characterCombatManager.lockOnTransform.position,
                                WorldUtilityManager.Instance.GetEnvironLayers()))
                        {
                            Debug.DrawLine(aiCharacter.characterCombatManager.lockOnTransform.position, targetCharacter.characterCombatManager.lockOnTransform.position);
                        }
                        else
                        {
                            targetsDirection = targetCharacter.transform.position - transform.position;
                            viewableAngle = WorldUtilityManager.Instance.GetAngleOfTarget(transform, targetsDirection);
                            aiCharacter.characterCombatManager.SetTarget(targetCharacter);
                            
                            if (enablePivot)
                                PivotTowardsTarget(aiCharacter);
                        }
                    }
                }
            }
        }

        public virtual void PivotTowardsTarget(AICharacterManager aiCharacter)
        {
            // 根据目标的可视角度播放枢轴动画
            if (aiCharacter.isPerformingAction)
                return;

            if (viewableAngle >= 20 && viewableAngle <= 60)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Right_45", true);
            }
            else if (viewableAngle <= -20 && viewableAngle >= -60)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Left_45", true);
            }
            else if (viewableAngle >= 61 && viewableAngle <= 110)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Right_90", true);
            }
            else if (viewableAngle <= -61 && viewableAngle >= -110)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Left_90", true);
            }
            if (viewableAngle >= 110 && viewableAngle <= 145)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Right_135", true);
            }
            else if (viewableAngle <= -110 && viewableAngle >= -145)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Left_135", true);
            }
            if (viewableAngle >= 146 && viewableAngle <= 180)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Right_180", true);
            }
            else if (viewableAngle <= -146 &&viewableAngle >= -180)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Left_180", true);
            }
        }

        public virtual void PivotTowardsPosition(AICharacterManager aiCharacter, Vector3 position)
        {
            //  PLAY A PIVOT ANIMATION DEPENDING ON VIEWABLE ANGLE OF TARGET
            if (aiCharacter.isPerformingAction)
                return;

            Vector3 targetsDirection = position - aiCharacter.transform.position;
            float viewableAngle = WorldUtilityManager.Instance.GetAngleOfTarget(aiCharacter.transform, targetsDirection);

            if (viewableAngle >= 20 && viewableAngle <= 60)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Right_45", true);
            }
            else if (viewableAngle <= -20 && viewableAngle >= -60)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Left_45", true);
            }
            else if (viewableAngle >= 61 && viewableAngle <= 110)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Right_90", true);
            }
            else if (viewableAngle <= -61 && viewableAngle >= -110)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Left_90", true);
            }
            if (viewableAngle >= 110 && viewableAngle <= 145)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Right_135", true);
            }
            else if (viewableAngle <= -110 && viewableAngle >= -145)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Left_135", true);
            }
            if (viewableAngle >= 146 && viewableAngle <= 180)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Right_180", true);
            }
            else if (viewableAngle <= -146 &&viewableAngle >= -180)
            {
                aiCharacter.characterAnimatorManager.PlayTargetActionAnimation("Turn_Left_180", true);
            }
        }

        public void RotateTowardsAgent(AICharacterManager aiCharacter)
        {
            if (aiCharacter.aiCharacterNetworkManager.isMoving.Value)
            {
                aiCharacter.transform.rotation = aiCharacter.navMeshAgent.transform.rotation;
            }
        }
        
        public void RotateTowardsTargetWhilstAttacking(AICharacterManager aiCharacter)
        {
            if (currentTarget == null)
                return;

            if (!aiCharacter.characterLocomotionManager.canRotate)
                return;

            if (!aiCharacter.isPerformingAction)
                return;
            
            Vector3 targetDirection = currentTarget.transform.position - aiCharacter.transform.position;
            targetDirection.y = 0;
            targetDirection.Normalize();

            if (targetDirection == Vector3.zero)
                targetDirection = aiCharacter.transform.forward;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            aiCharacter.transform.rotation = Quaternion.Slerp(aiCharacter.transform.rotation, targetRotation, attackRotationSpeed * Time.deltaTime);

        }

        public void HandleActionRecovery(AICharacterManager aiCharacter)
        {
            if (actionRecoveryTimer > 0)
            {
                if (!aiCharacter.isPerformingAction)
                {
                    actionRecoveryTimer -= Time.deltaTime;
                }
            }
        }
    }
}