using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace LZ
{
    [CreateAssetMenu(menuName = "A.I/States/Combat Stance")]
    public class CombatStanceState : AIState
    {
        // 1. 根据目标与角色的距离和角度选择攻击状态的攻击方式
        // 2. 在等待攻击时处理任何战斗逻辑（格挡、侧移、躲避等）
        // 3. 如果目标移出战斗范围，切换到追击目标状态
        // 4. 如果目标不再存在，切换到空闲状态

        [Header("Attacks")]
        public List<AICharacterAttackAction> aiCharacterAttacks; // 此角色可能进行的所有攻击的列表
        [SerializeField] protected List<AICharacterAttackAction> potentialAttacks; // 包含在此情境下所有可能的攻击（基于角度、距离等）
        [SerializeField] private AICharacterAttackAction chosenAttack;
        [SerializeField] private AICharacterAttackAction previousAttack;
        protected bool hasAttack;

        [Header("Combo")]
        [SerializeField] protected bool canPerformCombo; // 如果角色可以执行连击攻击，在初次攻击后
        [SerializeField] protected int chanceToPerformCombo = 25; // 角色在下一次攻击时执行连击的几率（百分比）
        protected bool hasRolledForComboChance; // 如果我们已经在这个状态下掷过几率了
        
        [Header("Engagement Distance")]
        [SerializeField] public float maximumEngagementDistance = 5; // 在我们进入追击状态前距离目标最远的距离

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
            
            // 如果没有攻击，就新建一个
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
            aiCharacter.navMeshAgent.CalculatePath(
                aiCharacter.aiCharacterCombatManager.currentTarget.transform.position, path);
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
                
                if (potentialAttack.maximumAttackAngle < aiCharacter.aiCharacterCombatManager.viewableAngle)
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

        protected override void ResetStateFlags(AICharacterManager aiCharacter)
        {
            base.ResetStateFlags(aiCharacter);

            hasAttack = false;
            hasRolledForComboChance = false;
        }
    }
}