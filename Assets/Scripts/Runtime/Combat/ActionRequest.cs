namespace LZ
{
    /// <summary>
    /// 输入意图请求（设计文档 §3.1）。输入层把按键翻译成 ActionRequest 投递给
    /// <see cref="CharacterStateMachine"/>，由统一缓冲窗口管理；状态机据此裁决转移。
    /// <para/>
    /// 目前只承载单个 <see cref="InputCommand"/> 与投递时刻；后续可在此扩展方向 / 长按 / 来源等字段，
    /// 而无需改动投递方（PlayerInputManager）与消费方（各 CharacterState）。
    /// </summary>
    public readonly struct ActionRequest
    {
        public readonly InputCommand Command;

        /// <summary>投递时刻（<c>Time.time</c>），用于缓冲窗口过期判断 / 调试。</summary>
        public readonly float QueuedTime;

        public ActionRequest(InputCommand command, float queuedTime)
        {
            Command = command;
            QueuedTime = queuedTime;
        }

        public bool IsNone => Command == InputCommand.None;

        public static readonly ActionRequest None = new ActionRequest(InputCommand.None, 0f);
    }
}
