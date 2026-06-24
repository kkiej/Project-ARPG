namespace LZ
{
    /// <summary>
    /// 角色状态机的状态基类（ER 风格显式 FSM）。
    /// 每个状态封装「进入 / 每帧 / 退出」三段逻辑：
    /// <list type="bullet">
    /// <item><see cref="OnEnter"/>：进入状态时调用一次（设 flag、播起手 clip 等）。</item>
    /// <item><see cref="Tick"/>：每帧驱动，返回要转移到的下一个状态；返回 null（或当前状态自身）表示停留。</item>
    /// <item><see cref="OnExit"/>：退出状态时调用一次（清理 flag 等）。</item>
    /// </list>
    /// 状态机仅在 Owner 端运行（裁决），底层执行（移动 / 播放 / RPC）沿用现有 Manager（见设计文档 §3.4）。
    /// </summary>
    public abstract class CharacterState
    {
        /// <summary>进入该状态时调用一次。</summary>
        public virtual void OnEnter(CharacterStateMachine machine) { }

        /// <summary>退出该状态时调用一次。</summary>
        public virtual void OnExit(CharacterStateMachine machine) { }

        /// <summary>
        /// 每帧驱动。返回要转移到的下一个状态；返回 null（或当前状态自身）表示停留在当前状态。
        /// </summary>
        public abstract CharacterState Tick(CharacterStateMachine machine);
    }
}
