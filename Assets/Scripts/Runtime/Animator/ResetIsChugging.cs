using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class ResetIsChugging : StateMachineBehaviour
    {
        PlayerManager player;

        // Flask 的 chugging 逻辑已迁移到 PlayerAnimatorManager.FlaskSequenceCoroutine()。
        // 此 SMB 仅作为 ControllerState 内部状态机的遗留 fallback。
        // 当 Animancer Flask 序列处于激活状态时，此 SMB 不会触发。
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (player == null)
                player = animator.GetComponent<PlayerManager>();

            if (player == null)
                return;

            if (player.IsOwner)
                player.playerNetworkManager.isChugging.Value = false;
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }
}
