namespace LZ
{
    /// <summary>
    /// 基础移动状态：地面移动 / 旋转 / 滞空下落。状态机的默认（兜底）状态。
    /// <para/>
    /// 阶段 3：作为 FSM 骨架，逐帧调用现有执行层 <see cref="PlayerLocomotionManager.HandleAllMovement"/>，
    /// 行为与「接管前」逐帧一致（设计文档 §3.4：执行层不变，FSM 只负责裁决 / 转移）。
    /// <para/>
    /// 后续阶段在 <see cref="Tick"/> 内加入向 Attack / Dodge / Jump 等状态的转移判定
    /// （检测到对应输入请求时 return 新 State）。
    /// </summary>
    public class LocomotionState : CharacterState
    {
        public override CharacterState Tick(CharacterStateMachine machine)
        {
            PlayerManager player = machine.player;

            // 执行层：KCC 速度计算 + 旋转（沿用现有实现，不在 state 内重写物理）。
            player.playerLocomotionManager.HandleAllMovement();

            // ── 阶段 4 / 5：动作转移（非动作中；带 MovesetData 的武器才接管攻击）──
            if (!player.isPerformingAction)
            {
                InputCommand cmd = machine.PeekBuffered();

                // 闪避（地面）→ DodgeState（翻滚 / 后撤步及其取消攻击在该状态内处理）。
                if (cmd == InputCommand.Dodge && player.characterLocomotionManager.isGrounded)
                {
                    machine.ClearBuffer();
                    return new DodgeState();
                }

                // 跳跃（地面）→ JumpState（前置判定在 AttemptToPerformJump 内，失败会即时退回）。
                if (cmd == InputCommand.Jump && player.characterLocomotionManager.isGrounded)
                {
                    machine.ClearBuffer();
                    return new JumpState();
                }

                // 攻击：空中 → 跳跃攻击；地面（非起跳瞬间）→ 冲刺 / 普通起手。对齐旧 LightAttackWeaponItemAction 分流。
                if (cmd == InputCommand.LightAttack || cmd == InputCommand.HeavyAttack)
                {
                    PlayerAttackState attack = null;

                    if (!player.characterLocomotionManager.isGrounded)
                        attack = PlayerAttackState.TryCreateJumpAttack(player, cmd);
                    else if (!player.playerNetworkManager.isJumping.Value)
                        attack = PlayerAttackState.TryCreateOpener(player, cmd);

                    if (attack != null)
                    {
                        machine.ClearBuffer();
                        return attack;
                    }
                }
            }
            return null;
        }
    }
}
