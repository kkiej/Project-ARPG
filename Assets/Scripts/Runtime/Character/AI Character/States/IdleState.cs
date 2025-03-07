using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "A.I/States/Idle")]
    public class IdleState : AIState
    {
        public override AIState Tick(AICharacterManager aiCharacter)
        {
            if (aiCharacter.characterCombatManager.currentTarget != null)
            {
                return SwitchState(aiCharacter, aiCharacter.pursueTarget);
            }
            else
            {
                // 返回此状态，以继续搜索目标
                aiCharacter.aiCharacterCombatManager.FindATargetViaLineOfSight(aiCharacter);
                return this;
            }
        }
    }
}