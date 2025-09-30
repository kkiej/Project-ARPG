﻿using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Character Effects/Instant Effects/Take Damage")]
    public class TakeDamageEffect : InstantCharacterEffect
    {
        [Header("Character Causing Damage")]
        public CharacterManager characterCausingDamage; // 如果伤害是其它角色造成的，它会被储存在这里

        [Header("Damage")]
        public float physicalDamage = 0; // （未来会被分为“标准”，“打击”，“挥砍”和“穿刺”）
        public float magicDamage = 0;
        public float fireDamage = 0;
        public float lightningDamage = 0;
        public float holyDamage = 0;
        
        [Header("Final Damage")]
        private int finalDamageDealt = 0; // 在所有计算完成后，角色所受的伤害

        [Header("Poise")]
        public float poiseDamage = 0;
        public bool poiseIsBroken; // 如果角色的姿态被打破，角色会处于眩晕状态并播放受击动画

        // （待办）建立增强效果
        // 累积效果数值

        [Header("Animation")]
        public bool playDamageAnimation = true;
        public bool manuallySelectDamageAnimation;
        public string damageAnimation;

        [Header("Sound FX")]
        public bool willPlayDamageSFX = true;
        public AudioClip elementalDamageSoundFX; // 如果有元素伤害（魔法/火焰/雷电/神圣），在常规SFX上使用

        [Header("Direction Damage Taken From")]
        public float angleHitFrom; // 用来角色播放哪个受击动画（向后，向左，向右等）
        public Vector3 contactPoint; // 用来决定血液特效实例化的位置

        public override void ProcessEffect(CharacterManager character)
        {
            // 检查“无敌状态”
            if (character.characterNetworkManager.isInvulnerable.Value)
                return;
            
            base.ProcessEffect(character);

            // 如果角色死亡，不处理额外的受击特效
            if (character.isDead.Value)
                return;

            CalculateDamage(character);
            
            // 检查伤害来自哪个方向
            // 播放伤害动画
            PlayDirectionalBasedDamageAnimation(character);
            
            // 检查是否有叠加效果（中毒、流血等）
            
            // 播放伤害音效
            PlayDamageSFX(character);
            
            // 播放伤害视觉效果（血液）
            PlayDamageVFX(character);

            //  RUN THIS AFTER ALL OTHER FUNCTIONS THAT WOULD ATTEMPT TO PLAY AN ANIMATION UPON BEING DAMAGED & AFTER POISE/STANCE DAMAGE IS CALCULATED
            CalculateStanceDamage(character);
            //  IF CHARACTER IS A.I, CHECK FOR NEW TARGET IF CHARACTER CAUSING DAMAGE IS PRESENT
        }

        private void CalculateDamage(CharacterManager character)
        {
            if (!character.IsOwner)
                return;
            
            if (characterCausingDamage != null)
            {
                // 检查伤害修正器并修改基础伤害（物理/元素伤害增益）

            }
            
            // 检查角色的固定防御并从伤害中减去
            
            // 检查角色的护甲吸收，并从伤害中减去百分比
            
            // 将所有伤害类型相加，并应用最终伤害
            finalDamageDealt =
                Mathf.RoundToInt(physicalDamage + magicDamage + fireDamage + lightningDamage + holyDamage);

            if (finalDamageDealt <= 0)
                finalDamageDealt = 1;

            character.characterNetworkManager.currentHealth.Value -= finalDamageDealt;
            
            // 计算平衡伤害决定角色是否会被眩晕
            // 从角色总韧性值中扣除所受的韧性伤害
            character.characterStatsManager.totalPoiseDamage -= poiseDamage;

            //  WE STORE THE PREVIOUS POISE DAMAGE TAKEN FOR OTHER INTERACTIONS
            character.characterCombatManager.previousPoiseDamageTaken = poiseDamage;

            float remainingPoise = character.characterStatsManager.basePoiseDefense +
                character.characterStatsManager.offensivePoiseBonus +
                character.characterStatsManager.totalPoiseDamage;

            if (remainingPoise <= 0)
                poiseIsBroken = true;

            //  SINCE THE CHARACTER HAS BEEN HIT, WE RESET THE POISE TIMER
            character.characterStatsManager.poiseResetTimer = character.characterStatsManager.defaultPoiseResetTime;
        }

        private void CalculateStanceDamage(CharacterManager character)
        {
            AICharacterManager aiCharacter = character as AICharacterManager;

            //  YOU CAN OPTIONALLY GIVE WEAPONS THEIR OWN STANCE DAMAGE VALUES, OR USE POISE DAMAGE
            int stanceDamage = Mathf.RoundToInt(poiseDamage);

            if (aiCharacter != null)
            {
                aiCharacter.aiCharacterCombatManager.DamageStance(stanceDamage);
            }
        }

        private void PlayDamageVFX(CharacterManager character)
        {
            // 如果是火焰等其它伤害，就播放其它特效
            character.characterEffectsManager.PlayBloodSplatterVFX(contactPoint);
        }

        private void PlayDamageSFX(CharacterManager character)
        {
            AudioClip physicalDamageSFX =
                WorldSoundFXManager.instance.ChooseRandomSFXFromArray(WorldSoundFXManager.instance.physicalDamageSFX);
            
            character.characterSoundFXManager.PlaySoundFX(physicalDamageSFX);
            character.characterSoundFXManager.PlayDamageGruntSoundFX();
            
            // 如果火焰伤害大于0，播放燃烧音效
            // 如果闪电伤害大于0，播放电击音效
        }

        private void PlayDirectionalBasedDamageAnimation(CharacterManager character)
        {
            if (!character.IsOwner)
                return;

            if (character.isDead.Value)
                return;

            if (poiseIsBroken)
            {
                if (angleHitFrom >= 145 && angleHitFrom <= 180)
                {
                    damageAnimation = character.characterAnimatorManager.GetRandomAnimationFromList(character.characterAnimatorManager.forward_Medium_Damage);
                }
                else if (angleHitFrom <= -145 && angleHitFrom >= -180)
                {
                    damageAnimation = character.characterAnimatorManager.GetRandomAnimationFromList(character.characterAnimatorManager.forward_Medium_Damage);
                }
                else if (angleHitFrom >= -45 && angleHitFrom <= 45)
                {
                    damageAnimation = character.characterAnimatorManager.GetRandomAnimationFromList(character.characterAnimatorManager.backward_Medium_Damage);
                }
                else if (angleHitFrom >= -144 && angleHitFrom <= -45)
                {
                    damageAnimation = character.characterAnimatorManager.GetRandomAnimationFromList(character.characterAnimatorManager.left_Medium_Damage);
                }
                else if (angleHitFrom >= 45 && angleHitFrom <= 144)
                {
                    damageAnimation = character.characterAnimatorManager.GetRandomAnimationFromList(character.characterAnimatorManager.right_Medium_Damage);
                }
            }
            else
            {
                if (angleHitFrom >= 145 && angleHitFrom <= 180)
                {
                    damageAnimation = character.characterAnimatorManager.GetRandomAnimationFromList(character.characterAnimatorManager.forward_Ping_Damage);
                }
                else if (angleHitFrom <= -145 && angleHitFrom >= -180)
                {
                    damageAnimation = character.characterAnimatorManager.GetRandomAnimationFromList(character.characterAnimatorManager.forward_Ping_Damage);
                }
                else if (angleHitFrom >= -45 && angleHitFrom <= 45)
                {
                    damageAnimation = character.characterAnimatorManager.GetRandomAnimationFromList(character.characterAnimatorManager.backward_Ping_Damage);
                }
                else if (angleHitFrom >= -144 && angleHitFrom <= -45)
                {
                    damageAnimation = character.characterAnimatorManager.GetRandomAnimationFromList(character.characterAnimatorManager.left_Ping_Damage);
                }
                else if (angleHitFrom >= 45 && angleHitFrom <= 144)
                {
                    damageAnimation = character.characterAnimatorManager.GetRandomAnimationFromList(character.characterAnimatorManager.right_Ping_Damage);
                }
            }

            character.characterAnimatorManager.lastDamageAnimationPlayed = damageAnimation;

            if (poiseIsBroken)
            {
                // 若处于破防状态，则限制角色的移动和行动能力
                character.characterAnimatorManager.PlayTargetActionAnimation(damageAnimation, true);
            }
            else
            {
                // 若未处于破防状态，则仅播放上半身动画而不限制行动
                character.characterAnimatorManager.PlayTargetActionAnimation(damageAnimation, false, false, true, true);
            }
        }
    }
}