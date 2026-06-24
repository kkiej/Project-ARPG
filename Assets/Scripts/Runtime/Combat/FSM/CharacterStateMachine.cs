using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 显式角色状态机（Owner 权威）。持有当前状态与玩家引用，每帧 <see cref="Tick"/> 驱动并处理状态转移。
    /// <para/>
    /// 不参与联机同步：远端客户端仍按广播的 clip 名播放（现有 RPC 路径），状态机只在 Owner 端裁决（见设计文档 §1）。
    /// 状态集：<see cref="LocomotionState"/> / <see cref="PlayerAttackState"/>（数据驱动连招 + 蓄力 + 冲刺/跳攻）/
    /// <see cref="DodgeState"/> / <see cref="JumpState"/> / <see cref="HitState"/> / <see cref="DeathState"/>。
    /// 输入经 <see cref="ActionRequest"/> 缓冲投递（阶段 6）。死亡 / 受击为全局转移。
    /// </summary>
    public class CharacterStateMachine
    {
        public readonly PlayerManager player;
        public CharacterState CurrentState { get; private set; }

        // ── 输入缓冲（阶段 6：统一为 ActionRequest，单槽"最近一次输入有效"语义）──
        private const float DefaultBufferTime = 0.35f;
        private ActionRequest _buffered = ActionRequest.None;
        private float _bufferExpireTime;

        public CharacterStateMachine(PlayerManager player, CharacterState initialState)
        {
            this.player = player;
            CurrentState = initialState;
            CurrentState?.OnEnter(this);
        }

        /// <summary>每帧调用（仅 Owner）。先处理全局转移，再驱动当前状态并按其返回值处理转移。</summary>
        public void Tick()
        {
            // ── 全局转移（外部事件触发，非输入；优先级：死亡 > 受击硬直）──
            // 对齐设计文档 §3.3 的 any → Death、受击打断任意状态。死亡 / 硬直动画由执行层播放，FSM 仅切状态。
            if (player.isDead.Value)
            {
                if (!(CurrentState is DeathState))
                    ChangeState(new DeathState());
            }
            else if (player.characterCombatManager.isStaggered && !(CurrentState is HitState))
            {
                ChangeState(new HitState());
            }

            if (CurrentState == null)
                return;

            CharacterState next = CurrentState.Tick(this);
            if (next != null && next != CurrentState)
                ChangeState(next);
        }

        /// <summary>投递一个输入意图到缓冲（覆盖式，窗口内有效）。</summary>
        public void EnqueueInput(InputCommand command, float bufferTime = DefaultBufferTime)
        {
            _buffered = new ActionRequest(command, Time.time);
            _bufferExpireTime = Time.time + bufferTime;
        }

        /// <summary>查看当前缓冲意图（不清除）；窗口过期返回 <see cref="ActionRequest.None"/>。</summary>
        public ActionRequest PeekRequest()
        {
            return Time.time <= _bufferExpireTime ? _buffered : ActionRequest.None;
        }

        /// <summary>查看当前缓冲输入指令（不清除）；窗口过期返回 None。各 State 的便捷入口。</summary>
        public InputCommand PeekBuffered()
        {
            return Time.time <= _bufferExpireTime ? _buffered.Command : InputCommand.None;
        }

        /// <summary>清空输入缓冲（已消费时调用）。</summary>
        public void ClearBuffer()
        {
            _buffered = ActionRequest.None;
            _bufferExpireTime = 0f;
        }

        /// <summary>切换状态：调用旧状态 OnExit、新状态 OnEnter。</summary>
        public void ChangeState(CharacterState next)
        {
            if (next == null || next == CurrentState)
                return;

            CurrentState?.OnExit(this);
            CurrentState = next;
            CurrentState.OnEnter(this);
        }
    }
}
