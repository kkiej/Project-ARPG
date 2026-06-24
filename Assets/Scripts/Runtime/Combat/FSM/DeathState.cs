namespace LZ
{
    /// <summary>
    /// 死亡状态。Phase 5。由 <see cref="CharacterStateMachine"/> 的全局转移在 <see cref="CharacterManager.isDead"/>
    /// 置位时进入（any → Death，最高优先级，对齐设计文档 §3.3）。
    /// <para/>
    /// 死亡动画已由 <see cref="CharacterManager.ProcessDeathEvent"/>（执行层）播放，DeathState 不重播，
    /// 只停住一切裁决直到复活（<see cref="CharacterManager.ReviveCharacter"/> 清 isDead）→ 回移动状态。
    /// </summary>
    public class DeathState : CharacterState
    {
        public override CharacterState Tick(CharacterStateMachine machine)
        {
            PlayerManager player = machine.player;

            // 复活后回到移动状态（执行层 ReviveCharacter 已 ReturnToController）。
            if (!player.isDead.Value)
                return new LocomotionState();

            // 执行层：死亡 / 倒地期间维持重力 / 贴地（canMove=false，水平速度归零）。
            player.playerLocomotionManager.HandleAllMovement();
            return null;
        }
    }
}
