using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 已废弃 — 原用于在格挡状态切换 AnimatorOverrideController，
    /// 现由 Animancer 的 WeaponAnimationSet 覆盖机制取代。
    /// 保留文件以避免 AnimatorController 上的引用丢失。
    /// </summary>
    public class ToggleBlockingController : StateMachineBehaviour
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { }
    }
}