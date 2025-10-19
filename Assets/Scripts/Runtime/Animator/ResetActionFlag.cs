using UnityEngine;

namespace LZ
{
    public class ResetActionFlag : StateMachineBehaviour
    {
        private CharacterManager character;
        
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (character == null)
            {
                character = animator.GetComponent<CharacterManager>();
            }

            // 当一个动作结束时调用，并且状态重置为"Empty"
            character.isPerformingAction = false;
            character.characterAnimatorManager.applyRootMotion = false;
            character.characterLocomotionManager.canRotate = true;
            character.characterLocomotionManager.canMove = true;
            character.characterLocomotionManager.canRun = true;
            character.characterLocomotionManager.isRolling = false;
            character.characterCombatManager.DisableCanDoCombo();
            character.characterCombatManager.DisableCanDoRollingAttack();
            character.characterCombatManager.DisableCanDoBackstepAttack();
            
            if (character.characterEffectsManager.activeSpellWarmUpFX != null)
                Destroy(character.characterEffectsManager.activeSpellWarmUpFX);

            if (character.characterEffectsManager.activeQuickSlotItemFX != null)
                Destroy(character.characterEffectsManager.activeQuickSlotItemFX);

            if (character.IsOwner)
            {
                character.characterNetworkManager.isJumping.Value = false;
                character.characterNetworkManager.isInvulnerable.Value = false;
                character.characterNetworkManager.isAttacking.Value = false;
                character.characterNetworkManager.isRipostable.Value = false;
                character.characterNetworkManager.isBeingCriticallyDamaged.Value = false;
                character.characterNetworkManager.isParrying.Value = false;
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