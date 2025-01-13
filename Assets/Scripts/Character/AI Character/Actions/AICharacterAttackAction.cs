using UnityEngine;

namespace LZ
{
    public class AICharacterAttackAction : ScriptableObject
    {
        [Header("Attack")]
        [SerializeField] private string attackAnimation;
        
        [Header("Combo Action")]
        public AICharacterAttackAction comboAction; // 该攻击动作的连击动作

        [Header("Action Values")]
        [SerializeField] private AttackType attackType;
        public int attackWeight = 50;
        // 可以重复的攻击
        public float actionRecoveryTime = 1.5f;
        public float minimumAttackAngle = -35;
        public float maximumAttackAngle = 35;
        public float minimumAttackDistance = 0;
        public float maximumAttackDistance = 2;
        
        public void AttemptToPerformAction(AICharacterManager aiCharacter)
        {
            aiCharacter.characterAnimatorManager.PlayTargetAttackActionAnimation(attackType, attackAnimation, true);
        }
    }
}