using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class ResetUpperbodyAction : StateMachineBehaviour
    {
        PlayerManager player;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (player == null)
                player = animator.GetComponent<PlayerManager>();

            if (player == null)
                return;

            if (player.playerEffectsManager.activeQuickSlotItemFX != null)
                Destroy(player.playerEffectsManager.activeQuickSlotItemFX);

            player.playerLocomotionManager.canRun = true;
            player.playerEquipmentManager.UnHideWeapons();

            if (player.playerEffectsManager.activeQuickSlotItemFX != null)
                Destroy(player.playerEffectsManager.activeQuickSlotItemFX);

            //  WE CHECK IF THE PLAYER IS USING AN ITEM
            if (player.playerCombatManager.isUsingItem)
            {
                player.playerCombatManager.isUsingItem = false;

                //  ONLY IF THE PLAYER IS NOT INTERACTING, DO WE ALLOW ROLLING
                //  WHY?: IF THE PLAYER IS DAMAGED AND THE DRINKING ANIMATION RETURNS TO THE "RESETUPPERBODYACTION" BEFORE THE DAMAGE ANIMATION RETURNS TO THE "RESETACTION"
                //  THE PLAYER WILL BE FREE TO ROLL EVEN THOUGH THEY ARE STILL IN A DAMAGE ANIMATION

                if (!player.isPerformingAction)
                    player.playerLocomotionManager.canRoll = true;
            }
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
