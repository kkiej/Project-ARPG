using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class AIDurkCharacterManager : AIBossCharacterManager
    {
        // 为什么给Durk设计专属的角色管理器？
        // 我们的角色管理器相当于一个中心枢纽，用以引用角色所有组件
        // 以"玩家"管理器为例，它将包含玩家角色特有的所有组件
        // "亡灵"管理器则包含亡灵单位特有的所有组件
        // 由于Durk拥有专属音效（棍棒击打、跺地声）这些仅该角色独有，我们专门创建了Durk音效管理器
        // 为了引用这个新音效管理器，同时保持设计模式的一致性，需要通过Durk角色管理器来进行引用


        [HideInInspector] public AIDurkSoundFXManager durkSoundFXManager;
        [HideInInspector] public AIDurkCombatManager durkCombatManager;

        protected override void Awake()
        {
            base.Awake();

            durkSoundFXManager = GetComponent<AIDurkSoundFXManager>();
            durkCombatManager = GetComponent<AIDurkCombatManager>();
        }
    }
}