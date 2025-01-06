using System;
using UnityEngine;

namespace LZ
{
    public class CharacterEffectsManager : MonoBehaviour
    {
        // 处理即时特效（受击，治疗）
        
        // 处理时间累积特效（毒药，累积）
        
        // 处理静态特效（添加/移除Buff）

        private CharacterManager character;

        protected virtual void Awake()
        {
            character = GetComponent<CharacterManager>();
        }

        public virtual void ProcessInstantEffect(InstantCharacterEffect effect)
        {
            effect.ProcessEffect(character);
        }
    }
}