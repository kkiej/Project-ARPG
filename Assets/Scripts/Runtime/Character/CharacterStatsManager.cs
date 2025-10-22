using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class CharacterStatsManager : MonoBehaviour
    {
        CharacterManager character;

        [Header("Runes")]
        public int runesDroppedOnDeath = 50;

        [Header("Stamina Regeneration")]
        [SerializeField] private float staminaRegenerationAmount = 2;
        private float staminaRegenerationTimer = 0;
        private float staminaTickTimer = 0;
        [SerializeField] private float staminaRegenerationDelay = 2;

        [Header("Blocking Absorptions")]
        public float blockingPhysicalAbsorption;
        public float blockingFireAbsorption;
        public float blockingMagicAbsorption;
        public float blockingLightningAbsorption;
        public float blockingHolyAbsorption;
        public float blockingStability;

        [Header("Armor Absorption")]
        public float armorPhysicalDamageAbsorption;
        public float armorMagicDamageAbsorption;
        public float armorFireDamageAbsorption;
        public float armorHolyDamageAbsorption;
        public float armorLightningDamageAbsorption;

        [Header("Armor Resistances")]
        public float armorImmunity;      // 腐败与中毒抗性
        public float armorRobustness;    // 出血与冰冻抗性
        public float armorFocus;         // 狂乱与睡眠抗性
        public float armorVitality;      // 死亡诅咒抗性

        [Header("Poise")]
        public float totalPoiseDamage;              // 累计承受的韧性伤害值
        public float offensivePoiseBonus;           // 使用武器获得的韧性加成（重型武器提供显著更高的加成）
        public float basePoiseDefense;              // 通过护甲/护符等装备获得的韧性加成
        public float defaultPoiseResetTime = 8;     // 韧性伤害重置所需时间（若在此时间内未受击则重置）
        public float poiseResetTimer = 0;           // 当前韧性重置的计时器

        protected virtual void Awake()
        {
            character = GetComponent<CharacterManager>();
        }

        protected virtual void Start()
        {
            
        }

        protected virtual void Update()
        {
            HandlePoiseResetTimer();
        }

        public int CalculateHealthBasedOnVitalityLevel(int vitality)
        {
            float health = 0;

            // 创建一个方程来定义你希望如何计算你的体力（耐力）
            
            health = vitality * 15;

            return Mathf.RoundToInt(health);
        }
        
        public int CalculateStaminaBasedOnEnduranceLevel(int endurance)
        {
            float stamina = 0;

            // 创建一个方程来定义你希望如何计算你的体力（耐力）
            
            stamina = endurance * 10;

            return Mathf.RoundToInt(stamina);
        }

        public int CalculateFocusPointsBasedOnMindLevel(int mind)
        {
            int focusPoints = 0;

            //  CREATE AN EQUATION FOR HOW YOU WANT YOUR STAMINA TO BE CALCULATED

            focusPoints = mind * 10;

            return Mathf.RoundToInt(focusPoints);
        }

        public virtual void RegenerateStamina()
        {
            // 只有自己可以编辑他们的网络变量
            if (!character.IsOwner)
                return;
            
            // 如果我们在使用体力，我们不想生成体力
            if (character.characterNetworkManager.isSprinting.Value)
                return;

            if (character.isPerformingAction)
                return;

            staminaRegenerationTimer += Time.deltaTime;

            if (staminaRegenerationTimer >= staminaRegenerationDelay)
            {
                if (character.characterNetworkManager.currentStamina.Value < character.characterNetworkManager.maxStamina.Value)
                {
                    staminaTickTimer += Time.deltaTime;

                    if (staminaTickTimer >= 0.1)
                    {
                        staminaTickTimer = 0;
                        character.characterNetworkManager.currentStamina.Value += staminaRegenerationAmount;
                    }
                }
            }
        }

        public virtual void ResetStaminaRegenTimer(float previousStaminaAmount, float currentStaminaAmount)
        {
            // 我们只有在使用了体力（耐力）的动作时才想要重置体力恢复
            // 如果我们已经在恢复体力，我们不想重置体力恢复
            if (currentStaminaAmount < previousStaminaAmount)
            {
                staminaRegenerationTimer = 0;
            }
        }

        protected virtual void HandlePoiseResetTimer()
        {
            if (poiseResetTimer > 0)
            {
                poiseResetTimer -= Time.deltaTime;
            }
            else
            {
                totalPoiseDamage = 0;
            }
        }
    }
}