namespace LZ
{
    /// <summary>
    /// 受击硬直状态。Phase 5。由 <see cref="CharacterStateMachine"/> 的全局转移在
    /// <see cref="CharacterCombatManager.isStaggered"/> 置位时进入（可打断任意状态，对齐设计文档 §3.3）。
    /// <para/>
    /// 硬直动画已由 <see cref="TakeDamageEffect"/>（执行层）播放并设 isPerformingAction，HitState 不重播，
    /// 只表达"硬直中"以闸住输入 / 打断当前动作。硬直结束（isPerformingAction 复位）→ 回移动状态。
    /// <para/>
    /// 注意：只有姿态被打破（poise broken）的 medium 硬直才进此状态；不打断的 ping flinch 不触发。
    /// </summary>
    public class HitState : CharacterState
    {
        public override void OnEnter(CharacterStateMachine machine)
        {
            // 信号已消费。
            machine.player.characterCombatManager.isStaggered = false;
        }

        public override CharacterState Tick(CharacterStateMachine machine)
        {
            PlayerManager player = machine.player;

            // 持续消费硬直信号：硬直期间再次受击由执行层重播（isPerformingAction 维持），
            // 不靠该标志续命，避免标志残留导致退出后又被全局转移拉回 HitState。
            player.characterCombatManager.isStaggered = false;

            // 执行层：硬直期间 canMove=false，HandleAllMovement 归零水平速度并维持重力 / 贴地。
            player.playerLocomotionManager.HandleAllMovement();

            // 硬直结束（执行层 ReturnToController 复位 isPerformingAction）→ 回移动状态。
            if (!player.isPerformingAction)
                return new LocomotionState();

            return null;
        }
    }
}
