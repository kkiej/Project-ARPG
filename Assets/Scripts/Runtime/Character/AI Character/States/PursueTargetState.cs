using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LZ
{
    [CreateAssetMenu(menuName = "A.I/States/Pursue Target")]
    public class PursueTargetState : AIState
    {
        public override AIState Tick(AICharacterManager aiCharacter)
        {
            // 检查我们是否正在执行一个动作（如果是，则在动作完成前不做任何事情）
            if (aiCharacter.isPerformingAction)
            {
                aiCharacter.characterAnimatorManager.SetAnimatorMovementParameters(0, 0);
                return this;
            }

            aiCharacter.characterAnimatorManager.SetAnimatorMovementParameters(0, 1);

            //  CHECK IF OUR TARGET IS NULL, IF WE DO NOT HAVE A TARGET, RETURN TO IDLE STATE
            if (aiCharacter.aiCharacterCombatManager.currentTarget == null)
                return SwitchState(aiCharacter, aiCharacter.idle);

            // 确保我们的导航网格代理是激活的，如果不是则启用它
            if (!aiCharacter.navMeshAgent.enabled)
                aiCharacter.navMeshAgent.enabled = true;

            // 如果我们的目标在角色可视角度外，转向面对他们
            if (aiCharacter.aiCharacterCombatManager.enablePivot)
            {
                if (aiCharacter.aiCharacterCombatManager.viewableAngle < aiCharacter.aiCharacterCombatManager.minimumFOV
                    || aiCharacter.aiCharacterCombatManager.viewableAngle > aiCharacter.aiCharacterCombatManager.maximumFOV)
                    aiCharacter.aiCharacterCombatManager.PivotTowardsTarget(aiCharacter);
            }
            
            aiCharacter.aiCharacterLocomotionManager.RotateTowardsAgent(aiCharacter);

            // 如果我们在目标的战斗范围内，切换状态到战斗姿态状态
            // 方法1
            //if (aiCharacter.aiCharacterCombatManager.distanceFromTarget <= aiCharacter.combatStance.maximumEngagementDistance)
            //    return SwitchState(aiCharacter, aiCharacter.combatStance);
            
            // 方法2
            if (aiCharacter.aiCharacterCombatManager.distanceFromTarget <= aiCharacter.navMeshAgent.stoppingDistance)
                return SwitchState(aiCharacter, aiCharacter.combatStance);

            // 如果目标无法到达，并且他们离得很远，返回家

            // 追击目标
            // 方法1
            //aiCharacter.navMeshAgent.SetDestination(aiCharacter.aiCharacterCombatManager.currentTarget.transform
            //    .position);
            
            // 方法2
            NavMeshPath path = new NavMeshPath();
            aiCharacter.navMeshAgent.CalculatePath(aiCharacter.aiCharacterCombatManager.currentTarget.transform.position, path);
            aiCharacter.navMeshAgent.SetPath(path);

            return this;
        }
    }
}