using System;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 某角色实际会用到的 ER 通用动画（<c>a000_</c>）子集（设计文档 §8.3 档位 A）。
    /// <para/>
    /// <c>a000_</c> 资产总量上万，但任一角色运行时的工作集很小（当前 locomotion 风格 + 一个负重组的
    /// 走/跑/翻滚等，约一两百个）。本资产由 <c>CommonAnimationAutoFiller</c> 按 <see cref="CommonAnimationConvention"/>
    /// 扫文件夹自动回填，**只硬引用需要的子集**，随角色加载；运行时注册进 <see cref="CharacterAnimationLibrary"/> 按 animId 解析。
    /// <para/>
    /// 替代 <see cref="CharacterAnimationData"/> 中 locomotion / dodge 那部分强类型字段（强类型字段无法扩展到上万）。
    /// 菜单：Assets → Create → Character → Common Animation Set。
    /// </summary>
    [CreateAssetMenu(menuName = "Character/Common Animation Set")]
    public class CommonAnimationSet : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            [Tooltip("仅供阅读的标签，如 Walk_g0_fwd。")]
            public string label;
            [Tooltip("ER animId（a000_ 六位数）。")]
            public int animId;
            public AnimationClip clip;
        }

        [Tooltip("自动回填的 (animId → clip) 子集。由 CommonAnimationAutoFiller 生成。")]
        public List<Entry> entries = new List<Entry>();
    }
}
