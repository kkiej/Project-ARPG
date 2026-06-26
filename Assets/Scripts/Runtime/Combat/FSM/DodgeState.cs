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
    /// <para/>
    /// 无敌帧：按 ER TAE（c0000 转储的 type0 flag8 "Flag As Dodging"）的绝对秒数窗驱动 isInvulnerable。
    /// 翻滚 a000_027100 = 0~0.533s；后撤步 a000_027000 = 0~0.233s（其后 7–11 帧的 PvE-only 无敌从简不接）。
    /// </summary>
    public class DodgeState : CharacterState
    {
        private bool _isBackstep;
        private bool _iFrameActive;

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

            // 闪避无敌帧：按 ER TAE（type0 flag8 "Flag As Dodging"）窗口设置 isInvulnerable。
            // 翻滚 a000_027100 与后撤步 a000_027000 各有一段全无敌窗（秒），区间由 CommonAnimationConvention 配置。
            UpdateIFrames(player);

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

        /// <summary>退出闪避（含被受击 / 死亡全局打断、取消进攻击、自然结束）时务必清掉无敌帧。</summary>
        public override void OnExit(CharacterStateMachine machine)
        {
            SetInvulnerable(machine.player, false);
            _iFrameActive = false;
        }

        /// <summary>
        /// 按闪避 clip 的 TAE 无敌窗（秒）逐帧开关 isInvulnerable。翻滚 / 后撤步各取一段全无敌窗。
        /// 仅在数据驱动（有 commonConvention）时生效，避免给非 ER clip 误加无敌帧（旧 clip 走其自身动画事件路径）。
        /// </summary>
        private void UpdateIFrames(PlayerManager player)
        {
            CharacterAnimatorManager anim = player.playerAnimatorManager;
            CommonAnimationConvention conv = anim != null && anim.animData != null ? anim.animData.commonConvention : null;
            if (conv == null)
                return;

            float start = _isBackstep ? conv.backstepIFrameStartSeconds : conv.rollIFrameStartSeconds;
            float end   = _isBackstep ? conv.backstepIFrameEndSeconds   : conv.rollIFrameEndSeconds;

            float t = anim.CurrentActionTime;
            bool inWindow = anim.CurrentActionLength > 0f && t >= start && t <= end;

            if (inWindow != _iFrameActive)
            {
                SetInvulnerable(player, inWindow);
                _iFrameActive = inWindow;
            }
        }

        private static void SetInvulnerable(PlayerManager player, bool value)
        {
            // FSM 仅 Owner 运行；isInvulnerable 为 Owner 写、Everyone 读，伤害判定各端据此跳过。
            if (player.IsOwner)
                player.characterNetworkManager.isInvulnerable.Value = value;
        }
    }
}
