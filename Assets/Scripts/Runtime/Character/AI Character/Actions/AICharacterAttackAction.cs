using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "A.I/Actions/Attack")]
    public class AICharacterAttackAction : ScriptableObject
    {
        [Header("Attack")]
        [SerializeField] private string attackAnimation;
        [SerializeField] bool isParryable = true;

        [Header("Combo Action")]
        public AICharacterAttackAction comboAction; // 该攻击动作的连击动作

        [Header("Action Values")]
        [SerializeField] private AttackType attackType;
        public int attackWeight = 50;
        // 可以重复的攻击
        public float actionRecoveryTime = 1.5f; // 角色在执行此次攻击后，能够再次发动攻击前的间隔时间
        public float minimumAttackAngle = -35;
        public float maximumAttackAngle = 35;
        public float minimumAttackDistance = 0;
        public float maximumAttackDistance = 2;
        
        public void AttemptToPerformAction(AICharacterManager aiCharacter)
        {
            // 若你的AI行为模式类似于玩家角色（例如入侵者AI？），如果是，则使用此逻辑
            //aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(attackType, attackAnimation, true);

            // 若你的AI使用的是纯动画驱动的简单攻击（非装备/物品驱动），则使用此逻辑
            aiCharacter.characterAnimatorManager.PlayTargetActionAnimation(attackAnimation, true);
            aiCharacter.aiCharacterNetworkManager.isParryable.Value = isParryable;
        }
    }
}