using System;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class CharacterEffectsManager : MonoBehaviour
    {
        // 处理即时特效（受击，治疗）
        
        // 处理时间累积特效（毒药，累积）
        
        // 处理静态特效（添加/移除Buff）

        CharacterManager character;

        [Header("Current Active FX")]
        public GameObject activeSpellWarmUpFX;
        public GameObject activeDrawnProjectileFX;

        [Header("VFX")]
        [SerializeField] GameObject bloodSplatterVFX;
        [SerializeField] GameObject criticalBloodSplatterVFX;

        [Header("Static Effects")]
        public List<StaticCharacterEffect> staticEffects = new List<StaticCharacterEffect>();

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
                GameObject bloodSplatter = Instantiate(WorldCharacterEffectsManager.instance.bloodSplatterVFX, contactPoint, Quaternion.identity);
            }
        }

        public void PlayCriticalBloodSplatterVFX(Vector3 contactPoint)
        {
            //  IF WE MANUALLY HAVE PLACED A BLOOD SPLATTER VFX ON THIS MODEL, PLAY ITS VERSION
            if (bloodSplatterVFX != null)
            {
                GameObject bloodSplatter = Instantiate(criticalBloodSplatterVFX, contactPoint, Quaternion.identity);
            }
            //  ELSE, USE THE GENERIC (DEFAULT VERSION) WE HAVE ELSEWHERE
            else
            {
                GameObject bloodSplatter = Instantiate(WorldCharacterEffectsManager.instance.criticalBloodSplatterVFX, contactPoint, Quaternion.identity);
            }
        }

        public void AddStaticEffect(StaticCharacterEffect effect)
        {
            //  IF YOU WANT TO SYNC EFFECTS ACROSS NETWORK, IF YOU ARE THE OWNER LAUNCH A SERVER RPC HERE TO PROCESS THE EFFECT ON ALL OTHER CLIENTS

            // 1. ADD A STATIC EFFECT TO THE CHARACTER
            staticEffects.Add(effect);

            // 2. PROCESS ITS EFFECT
            effect.ProcessStaticEffect(character);

            // 3. CHECK FOR NULL ENTRIES IN YOUR LIST AND REMOVE THEM
            for (int i = staticEffects.Count - 1; i > -1; i--)
            {
                if (staticEffects[i] == null)
                    staticEffects.RemoveAt(i);
            }
        }

        public void RemoveStaticEffect(int effectID)
        {
            //  IF YOU WANT TO SYNC EFFECTS ACROSS NETWORK, IF YOU ARE THE OWNER LAUNCH A SERVER RPC HERE TO PROCESS THE EFFECT ON ALL OTHER CLIENTS

            StaticCharacterEffect effect;

            for (int i = 0; i < staticEffects.Count; i++)
            {
                if (staticEffects[i] != null)
                {
                    if (staticEffects[i].staticEffectID == effectID)
                    {
                        effect = staticEffects[i];
                        // 1. REMOVE STATIC EFFECT FROM CHARACTER
                        effect.RemoveStaticEffect(character);
                        // 2. REMOVE STATIC EFFECT FROM LIST
                        staticEffects.Remove(effect);
                    }
                }
            }

            // 3. CHECK FOR NULL ENTRIES IN YOUR LIST AND REMOVE THEM
            for (int i = staticEffects.Count - 1; i > -1; i--)
            {
                if (staticEffects[i] == null)
                    staticEffects.RemoveAt(i);
            }
        }
    }
}