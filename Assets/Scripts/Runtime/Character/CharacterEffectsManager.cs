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

        [Header("VFX")]
        [SerializeField] private GameObject bloodSplatterVFX;

        protected virtual void Awake()
        {
            character = GetComponent<CharacterManager>();
        }

        public virtual void ProcessInstantEffect(InstantCharacterEffect effect)
        {
            effect.ProcessEffect(character);
        }

        public void PlayBloodSplatterVFX(Vector3 contactPoint)
        {
            // 如果我们手动在这个模型上放了血液飞溅特效，那么就播放这个版本
            if (bloodSplatterVFX != null)
            {
                GameObject bloodSplatter = Instantiate(bloodSplatterVFX, contactPoint, Quaternion.identity);
            }
            // 否则，使用通用的版本
            else
            {
                GameObject bloodSplatter = Instantiate(WorldCharacterEffectsManager.instance.bloodSplatterVFX,
                    contactPoint, Quaternion.identity);
            }
        }
    }
}