namespace LZ
{
    /// <summary>
    /// 闪避状态（翻滚 / 后撤步）。Phase 5。
    /// <para/>
    /// OnEnter 复用执行层 <see cref="PlayerLocomotionManager.AttemptToPerformDodge"/>（按移动量自动选翻滚 / 后撤步），
    /// 不在 state 内重写位移 / 朝向 / 网络。Tick 逐帧驱动现有移动执行层。
    /// <para/>
    /// 闪避取消攻击（翻滚 / 后撤步攻击）：闪避动画期间缓冲到轻 / 重输入即 cancel 进
    /// <see cref="PlayerAttackState"/> 的翻滚 / 后撤步攻击节点。取消窗即「闪避状态本身」，
    /// 不依赖任何手设动画事件（符合全用 ER TAE / 状态量原则）；后续可用翻滚 clip 的 TAE 收窄窗口。
    /// </summary>
    public class DodgeState : CharacterState
    {
        private bool _isBackstep;

        public override void OnEnter(CharacterStateMachine machine)
        {
            PlayerManager player = machine.player;

            // 与 AttemptToPerformDodge 内部判定一致：有移动量 → 翻滚，否则后撤步。
            // 在进入同帧捕获，供后续选择翻滚攻击 / 后撤步攻击（执行层不直接暴露做了哪种）。
            _isBackstep = !(PlayerInputManager.instance.moveAmount > 0);

            // 执行层：朝向 + 播翻滚/后撤步 clip + 扣耐力 + 网络标志（失败时 isPerformingAction 保持 false）。
            player.playerLocomotionManager.AttemptToPerformDodge();
        }

        public override CharacterState Tick(CharacterStateMachine machine)
        {
            PlayerManager player = machine.player;

            // 执行层：KCC 速度 + 旋转（沿用现有实现，不在 state 内重写物理）。
            player.playerLocomotionManager.HandleAllMovement();

            // 闪避取消：闪避动画期间按攻击 → 翻滚 / 后撤步攻击（crossfade 覆盖闪避 clip）。
            if (player.isPerformingAction)
            {
                InputCommand cmd = machine.PeekBuffered();
                if (cmd == InputCommand.LightAttack || cmd == InputCommand.HeavyAttack)
                {
                    PlayerAttackState attack = PlayerAttackState.TryCreateDodgeAttack(player, _isBackstep);
                    if (attack != null)
                    {
                        machine.ClearBuffer();
                        return attack;
                    }
                }
            }

            // 闪避结束（执行层 ReturnToController 复位 isPerformingAction）或未成功 → 回移动状态。
            if (!player.isPerformingAction)
                return new LocomotionState();

            return null;
        }
    }
}
