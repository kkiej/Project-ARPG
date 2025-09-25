using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Character Effects/Instant Effects/Take Blocked Damage")]
    public class TakeBlockedDamageEffect : InstantCharacterEffect
    {
        [Header("Character Causing Damage")]
        public CharacterManager characterCausingDamage; // 如果伤害是其它角色造成的，它会被储存在这里

        [Header("Damage")]
        public float physicalDamage = 0;            // （未来会被分为“标准”，“打击”，“挥砍”和“穿刺”）
        public float magicDamage = 0;
        public float fireDamage = 0;
        public float lightningDamage = 0;
        public float holyDamage = 0;

        [Header("Final Damage")]
        private int finalDamageDealt = 0;         // 在所有计算完成后，角色所受的伤害

        [Header("Poise")]
        public float poiseDamage = 0;
        public bool poiseIsBroken = false;          // 如果角色的姿态被打破，角色会处于眩晕状态并播放受击动画

        [Header("Stamina")]
        public float staminaDamage = 0;
        public float finalStaminaDamage = 0;

        //  (TO DO) BUILD UPS
        //  build up effect amounts

        [Header("Animation")]
        public bool playDamageAnimation = true;
        public bool manuallySelectDamageAnimation = false;
        public string damageAnimation;

        [Header("Sound FX")]
        public bool willPlayDamageSFX = true;
        public AudioClip elementalDamageSoundFX;    // 如果有元素伤害（魔法/火焰/雷电/神圣），在常规SFX上使用


        [Header("Direction Damage Taken From")]
        public float angleHitFrom;                  // 用来角色播放哪个受击动画（向后，向左，向右等）
        public Vector3 contactPoint;                // 用来决定血液特效实例化的位置

        public override void ProcessEffect(CharacterManager character)
        {
            if (character.characterNetworkManager.isInvulnerable.Value)
                return;

            base.ProcessEffect(character);

            Debug.Log("HIT WAS BLOCKED!");

            // 如果角色死亡，不处理额外的受击特效
            if (character.isDead.Value)
                return;

            CalculateDamage(character);
            CalculateStaminaDamage(character);
            PlayDirectionalBasedBlockingAnimation(character);
            // 检查是否有叠加效果（中毒、流血等）
            PlayDamageSFX(character);
            PlayDamageVFX(character);

            // 如果角色是AI，检查如果造成伤害的角色存在则寻找新目标
			
			CheckForGuardBreak(character);
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

            Debug.Log("ORIGINAL PHYSICAL DAMAGE: " + physicalDamage);

            physicalDamage -= (physicalDamage * (character.characterStatsManager.blockingPhysicalAbsorption / 100));
            magicDamage -= (magicDamage * (character.characterStatsManager.blockingMagicAbsorption / 100));
            fireDamage -= (fireDamage * (character.characterStatsManager.blockingFireAbsorption / 100));
            lightningDamage -= (lightningDamage * (character.characterStatsManager.blockingLightningAbsorption / 100));
            holyDamage -= (holyDamage * (character.characterStatsManager.blockingHolyAbsorption / 100));

            finalDamageDealt = Mathf.RoundToInt(physicalDamage + magicDamage + fireDamage + lightningDamage + holyDamage);

            if (finalDamageDealt <= 0)
            {
                finalDamageDealt = 1;
            }

            Debug.Log("FINAL PHYSICAL DAMAGE: " + physicalDamage);

            character.characterNetworkManager.currentHealth.Value -= finalDamageDealt;

            // 计算平衡伤害决定角色是否会被眩晕
        }

        private void CalculateStaminaDamage(CharacterManager character)
        {
            if (!character.IsOwner)
                return;

            finalStaminaDamage = staminaDamage;

            float staminaDamageAbsorption = finalStaminaDamage * (character.characterStatsManager.blockingStability / 100);
            float staminaDamageAfterAbsorption = finalStaminaDamage - staminaDamageAbsorption;

            character.characterNetworkManager.currentStamina.Value -= staminaDamageAfterAbsorption;
        }

        private void CheckForGuardBreak(CharacterManager character)
        {
            //if (character.characterNetworkManager.currentStamina.Value <= 0)
            //  PLAY SFX

            if (!character.IsOwner)
                return;

            if (character.characterNetworkManager.currentStamina.Value <= 0)
            {
                character.characterAnimatorManager.PlayTargetActionAnimation("Guard_Break_01", true);
                character.characterNetworkManager.isBlocking.Value = false;
            }
        }

        private void PlayDamageVFX(CharacterManager character)
        {
            //  IF WE HAVE FIRE DAMAGE, PLAY FIRE PARTICLES
            //  LIGHTNING DAMAGE, LIGHTNING PARTICLES ECT

            // 1. GET VFX BASED ON BLOCKING WEAPON
        }

        private void PlayDamageSFX(CharacterManager character)
        {
            //  IF FIRE DAMAGE IS GREATER THAN 0, PLAY BURN SFX
            //  IF LIGHTNING DAMAGE IS GREATER THAN 0, PLAY ZAP SFX

            character.characterSoundFXManager.PlayBlockSoundFX();
        }

        private void PlayDirectionalBasedBlockingAnimation(CharacterManager character)
        {
            if (!character.IsOwner)
                return;

            if (character.isDead.Value)
                return;

            DamageIntensity damageIntensity = WorldUtilityManager.Instance.GetDamageIntensityBasedOnPoiseDamage(poiseDamage);
            // 2. 播放与攻击"强度"相匹配的格挡动画

            // 待办：检查双手持握状态，若为双手持握则改用双手版本的格挡动画
            switch (damageIntensity)
            {
                case DamageIntensity.Ping:
                    damageAnimation = "Block_Ping_01";
                    break;
                case DamageIntensity.Light:
                    damageAnimation = "Block_Light_01";
                    break;
                case DamageIntensity.Medium:
                    damageAnimation = "Block_Medium_01";
                    break;
                case DamageIntensity.Heavy:
                    damageAnimation = "Block_Heavy_01";
                    break;
                case DamageIntensity.Colossal:
                    damageAnimation = "Block_Colossal_01";
                    break;
                default:
                    break;
            }

            character.characterAnimatorManager.lastDamageAnimationPlayed = damageAnimation;
            character.characterAnimatorManager.PlayTargetActionAnimation(damageAnimation, true);
        }
    }
}