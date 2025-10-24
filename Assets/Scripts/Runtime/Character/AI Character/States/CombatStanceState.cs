using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LZ
{
    [CreateAssetMenu(menuName = "A.I/States/Combat Stance")]
    public class CombatStanceState : AIState
    {
        [Header("Attacks")]
        public List<AICharacterAttackAction> aiCharacterAttacks; // 此角色可能进行的所有攻击的列表
        [SerializeField] protected List<AICharacterAttackAction> potentialAttacks; // 包含在此情境下所有可能的攻击（基于角度、距离等）
        [SerializeField] private AICharacterAttackAction chosenAttack;
        [SerializeField] private AICharacterAttackAction previousAttack;
        protected bool hasAttack;

        [Header("Combo")]
        [SerializeField] protected bool canPerformCombo; // 如果角色可以执行连击攻击，在初次攻击后
        [SerializeField] protected int percentageOfTimeWillPerformCombo = 25; // 角色在下一次攻击时执行连击的几率（百分比）
        [SerializeField] public bool onlyPerformComboIfInitialAttackHits = false;
        protected bool hasRolledForComboChance; // 如果我们已经在这个状态下掷过几率了
        
        [Header("Engagement Distance")]
        [SerializeField] public float maximumEngagementDistance = 5; // 在我们进入追击状态前距离目标最远的距离

        [Header("Circling")]
        [SerializeField] bool willCircleTarget = false;
        private bool hasChoosenCirclePath = false;
        private float strafeMoveAmount;

        [Header("Blocking")]
        [SerializeField] bool canBlock = false;
        [SerializeField] int percentageOfTimeWillBlock = 75;
        private bool hasRolledForBlockChance = false;
        private bool willBlockDuringThisCombatRotation = false;
        //  YOU COULD HANDLE THIS MULTIPLE WAYS
        //  1. YOU COULD HAVE A BLOCKING CHARACTER ALWAYS BLOCK DURING THE COMBAT STANCE STATE
        //  2. YOU COULD "ROLL" FOR A BLOCK CHANCE AND HAVE THEM BLOCK A PERCENTAGE OF THE TIME
        //  BONUS: YOU COULD CREATE SPECIFIC COMBAT STANCE STATES, WHERE BLOCKING CONDITIONS ARE DIFFERENT FOR EACH CREATURE (MAYBE SOME ONLY BLOCK ON % OF LIFE OR WHATEVER)

        public override AIState Tick(AICharacterManager aiCharacter)
        {
            if (aiCharacter.isPerformingAction)
                return this;

            if (!aiCharacter.navMeshAgent.enabled)
                aiCharacter.navMeshAgent.enabled = true;
            
            // 如果你希望AI角色在目标超出其视野范围时朝向并转向目标
            if (aiCharacter.aiCharacterCombatManager.enablePivot)
            {
                if (!aiCharacter.aiCharacterNetworkManager.isMoving.Value)
                {
                    if (aiCharacter.aiCharacterCombatManager.viewableAngle < -30 || aiCharacter.aiCharacterCombatManager.viewableAngle > 30)
                        aiCharacter.aiCharacterCombatManager.PivotTowardsTarget(aiCharacter);
                }
            }
            
            // 旋转并朝向我们的目标
            aiCharacter.aiCharacterCombatManager.RotateTowardsAgent(aiCharacter);
            
            // 如果我们的目标不存在了，切换回Idle
            if (aiCharacter.aiCharacterCombatManager.currentTarget == null)
                return SwitchState(aiCharacter, aiCharacter.idle);

            if (willCircleTarget)
                SetCirclePath(aiCharacter);

            if (canBlock && !hasRolledForBlockChance)
            {
                hasRolledForBlockChance = true;
                willBlockDuringThisCombatRotation = RollForOutcomeChance(percentageOfTimeWillBlock);
            }

            //  ROLL FOR COMBO CHANCE
            if (canPerformCombo && !hasRolledForComboChance)
            {
                hasRolledForComboChance = true;
                aiCharacter.attack.willPerformCombo = RollForOutcomeChance(percentageOfTimeWillPerformCombo);
            }

            if (willBlockDuringThisCombatRotation)
                aiCharacter.aiCharacterNetworkManager.isBlocking.Value = true;

            //  IF WE DO NOT HAVE AN ATTACK, GET ONE
            if (!hasAttack)
            {
                GetNewAttack(aiCharacter);
            }
            else
            {
                aiCharacter.attack.currentAttack = chosenAttack;
                // 为连击掷骰子
                return SwitchState(aiCharacter, aiCharacter.attack);
            }

            // 如果超出交战距离，切换回追击目标状态
            if (aiCharacter.aiCharacterCombatManager.distanceFromTarget > maximumEngagementDistance)
                return SwitchState(aiCharacter, aiCharacter.pursueTarget);
            
            NavMeshPath path = new NavMeshPath();
            aiCharacter.navMeshAgent.CalculatePath(aiCharacter.aiCharacterCombatManager.currentTarget.transform.position, path);
            aiCharacter.navMeshAgent.SetPath(path);

            return this;
        }

        protected virtual void GetNewAttack(AICharacterManager aiCharacter)
        {
            potentialAttacks = new List<AICharacterAttackAction>();

            foreach (var potentialAttack in aiCharacterAttacks)
            {
                // 若我们这次攻击离得太近，检查下一个
                if (potentialAttack.minimumAttackDistance > aiCharacter.aiCharacterCombatManager.distanceFromTarget)
                    continue;
                
                // 若我们这次攻击离得太远，检查下一个
                if (potentialAttack.maximumAttackDistance < aiCharacter.aiCharacterCombatManager.distanceFromTarget)
                    continue;
                
                if (potentialAttack.minimumAttackAngle > aiCharacter.aiCharacterCombatManager.viewableAngle)
                    continue;

                //  IF THE TARGET IS OUTSIDE MAXIMUM FIELD OF VIEW FOR THIS ATTACK, CHECK THE NEXT
                if (potentialAttack.maximumAttackDistance < aiCharacter.aiCharacterCombatManager.viewableAngle)
                    continue;
                
                potentialAttacks.Add(potentialAttack);
            }

            if (potentialAttacks.Count <= 0)
                return;

            var totalWeight = 0;

            foreach (var attack in potentialAttacks)
            {
                totalWeight += attack.attackWeight;
            }

            var randomWeightValue = Random.Range(1, totalWeight + 1);
            var processedWeight = 0;

            foreach (var attack in potentialAttacks)
            {
                processedWeight += attack.attackWeight;

                if (randomWeightValue <= processedWeight)
                {
                    chosenAttack = attack;
                    previousAttack = chosenAttack;
                    hasAttack = true;
                    return;
                }
            }
        }

        protected virtual bool RollForOutcomeChance(int outcomeChance)
        {
            bool outcomeWillBePerformed = false;

            int randomPercentage = Random.Range(0, 100);

            if (randomPercentage < outcomeChance)
                outcomeWillBePerformed = true;

            return outcomeWillBePerformed;
        }

        protected virtual void SetCirclePath(AICharacterManager aiCharacter)
        {
            if (Physics.CheckSphere(aiCharacter.aiCharacterCombatManager.lockOnTransform.position, aiCharacter.characterController.radius + 0.25f, WorldUtilityManager.Instance.GetEnvironLayers()))
            {
                //  STOP STRAFING/CIRCLING BECAUSE WE'VE HIT SOMETHING, INSTEAD PATH TOWARDS ENEMY (WE USE ABS INCASE ITS NEGATIVE, TO MAKE IT POSITIVE)
                //  THIS WILL MAKE OUR CHARACTER FOLLOW THE NAVMESH AGENT AND PATH TOWARDS THE TARGET
                Debug.Log("WE ARE COLLIDING WITH SOMETHING, ENDING STRAFE");
                aiCharacter.characterAnimatorManager.SetAnimatorMovementParameters(0, Mathf.Abs(strafeMoveAmount));
                return;
            }

            //  STRAFE
            Debug.Log("STRAFING");
            aiCharacter.characterAnimatorManager.SetAnimatorMovementParameters(strafeMoveAmount, 0);

            if (hasChoosenCirclePath)
                return;

            hasChoosenCirclePath = true;

            //  STRAFE LEFT? OR RIGHT?
            int leftOrRightIndex = Random.Range(0, 100);

            if (leftOrRightIndex >= 50)
            {
                //  LEFT
                strafeMoveAmount = -0.5f;
            }
            else
            {
                //  RIGHT
                strafeMoveAmount = 0.5f;
            }
        }

        protected override void ResetStateFlags(AICharacterManager aiCharacter)
        {
            base.ResetStateFlags(aiCharacter);

            hasAttack = false;
            hasRolledForComboChance = false;
            hasRolledForBlockChance = false;
            hasChoosenCirclePath = false;
            willBlockDuringThisCombatRotation = false;
            strafeMoveAmount = 0;
        }
    }
}