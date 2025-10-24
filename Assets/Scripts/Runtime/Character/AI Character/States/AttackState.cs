using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "A.I/States/Attack")]
    public class AttackState : AIState
    {
        [Header("Current Attack")]
        [HideInInspector] public AICharacterAttackAction currentAttack;
        [HideInInspector] public bool willPerformCombo = false;

        [Header("State Flags")]
        protected bool hasPerformedAttack = false;
        protected bool hasPerformedCombo = false;

        [Header("Pivot After Attack")]
        [SerializeField] protected bool pivotAfterAttack = false;

        public override AIState Tick(AICharacterManager aiCharacter)
        {
            if (aiCharacter.aiCharacterCombatManager.currentTarget == null)
                return SwitchState(aiCharacter, aiCharacter.idle);

            if (aiCharacter.aiCharacterCombatManager.currentTarget.isDead.Value)
                return SwitchState(aiCharacter, aiCharacter.idle);
            
            // 攻击时旋转朝向目标
            aiCharacter.aiCharacterCombatManager.RotateTowardsTargetWhilstAttacking(aiCharacter);
            
            // 设置移动值为0
            aiCharacter.characterAnimatorManager.UpdateAnimatorMovementParameters(0, 0, false);

            //  PERFORM A COMBO
            PerformCombo(aiCharacter);

            if (aiCharacter.isPerformingAction)
                return this;

            if (!hasPerformedAttack)
            {
                // 如果我们仍处于从一个动作中恢复的状态，在执行另一个之前要等待
                if (aiCharacter.aiCharacterCombatManager.actionRecoveryTimer > 0)
                    return this;

                PerformAttack(aiCharacter);

                // 回到顶部，这样如果我们有连招，就可以在合适的时机进行处理
                return this;
            }
            
            if(pivotAfterAttack)
                aiCharacter.aiCharacterCombatManager.PivotTowardsTarget(aiCharacter);

            return SwitchState(aiCharacter, aiCharacter.combatStance);
        }

        protected void PerformAttack(AICharacterManager aiCharacter)
        {
            hasPerformedAttack = true;
            currentAttack.AttemptToPerformAction(aiCharacter);
            aiCharacter.aiCharacterCombatManager.actionRecoveryTimer = currentAttack.actionRecoveryTime;
        }

        protected override void ResetStateFlags(AICharacterManager aiCharacter)
        {
            base.ResetStateFlags(aiCharacter);

            hasPerformedAttack = false;
            hasPerformedCombo = false;
            willPerformCombo = false;
        }

        protected virtual void PerformCombo(AICharacterManager aiCharacter)
        {
            bool canPerformTheCombo = false;

            if (!willPerformCombo)
                return;

            if (hasPerformedCombo)
                return;

            if (currentAttack.comboAction == null)
                return;

            //  IF WE DONT NEED TO HIT OUR CURRENT TARGET, WE WILL PERFORM THE COMBO ANYWAY
            if (aiCharacter.aiCharacterCombatManager.canPerformCombo
                && !aiCharacter.combatStance.onlyPerformComboIfInitialAttackHits)
                canPerformTheCombo = true;

            //  IF WE DO NEED TO HIT THE TARGET, AND WE HAVE HIT THE TARGET, PERFORM THE COMBO
            if (aiCharacter.aiCharacterCombatManager.canPerformCombo
                && aiCharacter.combatStance.onlyPerformComboIfInitialAttackHits
                && aiCharacter.aiCharacterCombatManager.hasHitTargetDuringCombo)
                canPerformTheCombo = true;

            if (canPerformTheCombo)
            {
                hasPerformedCombo = true;
                currentAttack.comboAction.AttemptToPerformAction(aiCharacter);
            }

        }
    }
}