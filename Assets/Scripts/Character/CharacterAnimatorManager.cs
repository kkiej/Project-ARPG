using System;
using Unity.Netcode;
using UnityEngine;

namespace LZ
{
    public class CharacterAnimatorManager : MonoBehaviour
    {
        private CharacterManager character;
        private int horizontal;
        private int vertical;

        protected virtual void Awake()
        {
            character = GetComponent<CharacterManager>();

            horizontal = Animator.StringToHash("Horizontal");
            vertical = Animator.StringToHash("Vertical");
        }

        public void UpdateAnimatorMovementParameters(float horizontalMovement, float verticalMovement, bool isSprinting)
        {
            float horizontalAmount = horizontalMovement;
            float verticalAmount = verticalMovement;
            if (isSprinting)
            {
                verticalAmount = 2;
            }
            
            character.animator.SetFloat(horizontal, horizontalAmount, 0.1f, Time.deltaTime);
            character.animator.SetFloat(vertical, verticalAmount, 0.1f, Time.deltaTime);
        }

        public virtual void PlayTargetActionAnimation(string targetAnimation, bool isPerformingAction,
            bool applyRootMotion = true, bool canRotate = false, bool canMove = false)
        {
            character.applyRootMotion = applyRootMotion;
            character.animator.CrossFade(targetAnimation, 0.2f);
            // 可以用来阻止角色尝试新的动作
            // 例如，如果你受到伤害，并开始执行受击动画
            // 如果你被眩晕，这个标志会变为真
            // 然后我们可以在尝试新动作之前检查这个标志
            character.isPerformingAction = isPerformingAction;
            character.canRotate = canRotate;
            character.canMove = canMove;
            
            // 告诉服务器/主机我们播放了一个动画，并为在场的每个人播放这个动画
            character.characterNetworkManager.NotifyTheServerOfActionAnimationServerRpc(NetworkManager.Singleton.LocalClientId,
                targetAnimation, applyRootMotion);
        }
        
        public virtual void PlayTargetAttackActionAnimation(AttackType attackType, string targetAnimation,
            bool isPerformingAction, bool applyRootMotion = true, bool canRotate = false, bool canMove = false)
        {
            // 跟踪上次执行的攻击（用于连招）
            // 跟踪当前攻击类型（轻攻击、重攻击等）
            // 更新动画集为当前武器动画
            // 判断我们的攻击是否可以被招架
            // 通知网络我们的“正在攻击”标志是激活状态（用于反击伤害等）
            character.characterCombatManager.currentAttackType = attackType;
            character.applyRootMotion = applyRootMotion;
            character.animator.CrossFade(targetAnimation, 0.2f);
            character.isPerformingAction = isPerformingAction;
            character.canRotate = canRotate;
            character.canMove = canMove;
            
            // 告诉服务器/主机我们播放了一个动画，并为在场的每个人播放这个动画
            character.characterNetworkManager.NotifyTheServerOfAttackActionAnimationServerRpc(NetworkManager.Singleton.LocalClientId,
                targetAnimation, applyRootMotion);
        }
    }
}