using System;
using Unity.Netcode;
using UnityEngine;

namespace LZ
{
    public class CharacterAnimatorManager : MonoBehaviour
    {
        private CharacterManager character;
        private float horizontal;
        private float vertical;

        protected virtual void Awake()
        {
            character = GetComponent<CharacterManager>();
        }

        public void UpdateAnimatorMovementParameters(float horizontalValue, float verticalValue)
        {
            character.animator.SetFloat("Horizontal", horizontalValue, 0.1f, Time.deltaTime);
            character.animator.SetFloat("Vertical", verticalValue, 0.1f, Time.deltaTime);
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
    }
}