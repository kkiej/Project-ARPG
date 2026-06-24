namespace LZ
{
    /// <summary>
    /// 跳跃状态。Phase 5。
    /// <para/>
    /// OnEnter 复用执行层 <see cref="PlayerLocomotionManager.AttemptToPerformJump"/>（含耐力 / 地面 / 重复跳等前置判定，
    /// 失败则 isJumping 不置位，下一帧即退回移动）。跳跃序列 Start→Lift→AirIdle→Landing 由执行层
    /// <see cref="CharacterAnimatorManager.PlayJumpSequence"/> 按 isGrounded 驱动，非动画事件。
    /// <para/>
    /// 注意：跳跃本体不设 isPerformingAction（以便空中接跳攻），故用 <see cref="CharacterNetworkManager.isJumping"/>
    /// 判定在跳 / 落地。空中（离地后）缓冲到攻击 → cancel 进 <see cref="PlayerAttackState"/> 跳跃攻击。
    /// 掉落（非跳跃）的空中攻击仍由 <see cref="LocomotionState"/> 的空中分支处理。
    /// </summary>
    public class JumpState : CharacterState
    {
        public override void OnEnter(CharacterStateMachine machine)
        {
            PlayerManager player = machine.player;

            // 执行层：前置判定 + 播跳跃序列 + 设 isJumping + 跳跃方向 + 扣耐力。
            player.playerLocomotionManager.AttemptToPerformJump();
        }

        public override CharacterState Tick(CharacterStateMachine machine)
        {
            PlayerManager player = machine.player;

            // 执行层：KCC 速度（含空中位移）+ 旋转（沿用现有实现）。
            player.playerLocomotionManager.HandleAllMovement();

            // 空中跳攻：起跳离地后，缓冲到攻击 → 跳跃攻击（crossfade 覆盖跳跃 clip）。
            if (!player.characterLocomotionManager.isGrounded && !player.isPerformingAction)
            {
                InputCommand cmd = machine.PeekBuffered();
                if (cmd == InputCommand.LightAttack || cmd == InputCommand.HeavyAttack)
                {
                    PlayerAttackState attack = PlayerAttackState.TryCreateJumpAttack(player, cmd);
                    if (attack != null)
                    {
                        machine.ClearBuffer();
                        return attack;
                    }
                }
            }

            // 落地（执行层 PlayJumpLanding 复位 isJumping）或起跳失败 → 回移动状态。
            if (!player.playerNetworkManager.isJumping.Value)
                return new LocomotionState();

            return null;
        }
    }
}
