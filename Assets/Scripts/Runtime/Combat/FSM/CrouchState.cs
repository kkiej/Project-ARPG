namespace LZ
{
    /// <summary>
    /// 下蹲状态（切换式）。地面 <see cref="LocomotionState"/> 收到 <see cref="InputCommand.Crouch"/> 进入；
    /// 进入播 a000_390000，随后按移动输入在下蹲 idle(300000) / 四向下蹲移动(320000+dir) 间切换
    /// （执行层 <see cref="CharacterAnimatorManager.UpdateCrouchLocomotion"/> 驱动，非动画事件）。
    /// <para/>
    /// 退出路径：
    /// <list type="bullet">
    /// <item>再次按下蹲 → 播站起 a000_390001 → 回 <see cref="LocomotionState"/>。</item>
    /// <item>跳 / 闪避 → 取消下蹲（不播站起）转 <see cref="JumpState"/> / <see cref="DodgeState"/>。</item>
    /// <item>轻 / 重攻击 → 下蹲攻击（<see cref="PlayerAttackState.TryCreateCrouchAttack"/>，a023_030310）。</item>
    /// <item>离地（被推下台等）→ 直接回 <see cref="LocomotionState"/> 兜底。</item>
    /// </list>
    /// 下蹲不设 isPerformingAction，故可移动并被上述输入取消。
    /// </summary>
    public class CrouchState : CharacterState
    {
        public override void OnEnter(CharacterStateMachine machine)
        {
            machine.player.playerAnimatorManager.EnterCrouch();
        }

        public override CharacterState Tick(CharacterStateMachine machine)
        {
            PlayerManager player = machine.player;
            CharacterAnimatorManager anim = player.playerAnimatorManager;

            // 执行层：KCC 速度 + 旋转（下蹲允许移动）。
            player.playerLocomotionManager.HandleAllMovement();

            // 离地兜底：被推下平台 / 失足 → 退出下蹲回移动状态。
            if (!player.characterLocomotionManager.isGrounded)
            {
                anim.ExitCrouch(false);
                return new LocomotionState();
            }

            InputCommand cmd = machine.PeekBuffered();

            // 再次按下蹲 → 站起。
            if (cmd == InputCommand.Crouch)
            {
                machine.ClearBuffer();
                anim.ExitCrouch(true);
                return new LocomotionState();
            }

            // 跳 / 闪避 → 取消下蹲（不播站起），交由对应状态接管动画。
            if (cmd == InputCommand.Jump)
            {
                machine.ClearBuffer();
                anim.ExitCrouch(false);
                return new JumpState();
            }
            if (cmd == InputCommand.Dodge)
            {
                machine.ClearBuffer();
                anim.ExitCrouch(false);
                return new DodgeState();
            }

            // 下蹲攻击：轻 / 重攻击 → a023_030310（无下蹲攻击的武器返回 null，则忽略不打断下蹲）。
            if (cmd == InputCommand.LightAttack || cmd == InputCommand.HeavyAttack)
            {
                PlayerAttackState attack = PlayerAttackState.TryCreateCrouchAttack(player, cmd);
                if (attack != null)
                {
                    machine.ClearBuffer();
                    anim.ExitCrouch(false);
                    return attack;
                }
                machine.ClearBuffer();  // 无下蹲攻击 → 丢弃该输入，留在下蹲
            }

            // 驱动下蹲 locomotion（站立 idle / 移动四向）。
            bool isMoving = player.playerNetworkManager.isMoving.Value;
            bool lockedOn = player.playerNetworkManager.isLockedOn.Value;
            float v = PlayerInputManager.instance != null ? PlayerInputManager.instance.verticalInput : 0f;
            float h = PlayerInputManager.instance != null ? PlayerInputManager.instance.horizontalInput : 0f;
            anim.UpdateCrouchLocomotion(isMoving, v, h, lockedOn);

            return null;
        }
    }
}
