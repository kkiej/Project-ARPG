namespace LZ
{
    /// <summary>
    /// 玩家"输入意图"。输入层把按键翻译成 InputCommand 投递给状态机，
    /// 与具体按键绑定 / 具体动画解耦（ER 风格：输入指令 → 逻辑动作）。
    /// </summary>
    public enum InputCommand
    {
        None,
        LightAttack,   // RB
        HeavyAttack,   // RT
        OffHand,       // LB（副手/格挡/双持）
        AshOfWar,      // LT（战技）
        Dodge,         // 翻滚/后撤步
        Jump,
        Sprint,
        Interact
    }

    /// <summary>
    /// 当前持武状态，决定使用 MovesetData 里的哪一张招式图。
    /// </summary>
    public enum HandState
    {
        OneHandRight,  // 单手右手主武器
        TwoHand,       // 双手持武器
        DualWield      // 双武器架势(power stance)
    }
}
