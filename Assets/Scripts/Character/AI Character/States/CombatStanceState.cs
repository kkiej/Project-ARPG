using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class CombatStanceState : AIState
    {
        // 1. 根据目标与角色的距离和角度选择攻击状态的攻击方式
        // 2. 在等待攻击时处理任何战斗逻辑（格挡、侧移、躲避等）
        // 3. 如果目标移出战斗范围，切换到追击目标状态
        // 4. 如果目标不再存在，切换到空闲状态

        [Header("Attacks")]
        public List<AICharacterAttackAction> aiCharacterAttacks; // 此角色可能进行的所有攻击的列表
        protected List<AICharacterAttackAction> potentialAttacks; // 包含在此情境下所有可能的攻击（基于角度、距离等）
        private AICharacterAttackAction chooseAttack;
        private AICharacterAttackAction previousAttack;
        protected bool hasAttack;

        [Header("Combo")]
        [SerializeField] protected bool canPerFormCombo; // 如果角色可以执行连击攻击，在初次攻击后
        [SerializeField] protected int chanceToPerformCombo = 25; // 角色在下一次攻击时执行连击的几率（百分比）
        protected bool hasRolledForComboChance; // 如果我们已经在这个状态下掷过几率了

        [SerializeField] protected float maximumEngagementDistance = 5; // 在我们进入追击状态前距离目标最远的距离

        public override AIState Tick(AICharacterManager aiCharacter)
        {
            if (aiCharacter.isPerformingAction)
                return this;

            if (!aiCharacter.navMeshAgent.enabled)
                aiCharacter.navMeshAgent.enabled = true;
        }

        protected virtual void GetNewAttack(AICharacterManager aiCharacter)
        {
            potentialAttacks = new List<AICharacterAttackAction>();

            foreach (var potentialAttack in potentialAttacks)
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

            var taotalWeight = 0;

            foreach (var attack in potentialAttacks)
            {
                taotalWeight += attack.attackWeight;
            }

            var randomWeightValue = Random.Range(1, taotalWeight + 1);
            var processedWeight = 0;

            foreach (var attack in potentialAttacks)
            {
                processedWeight += attack.attackWeight;

                if (randomWeightValue <= processedWeight)
                {
                    chooseAttack = attack;
                    previousAttack = chooseAttack;
                    hasAttack = true;
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