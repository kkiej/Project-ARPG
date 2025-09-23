using UnityEngine;

namespace LZ
{
    public class CharacterStatsManager : MonoBehaviour
    {
        private CharacterManager character;
        
        [Header("Stamina Regeneration")]
        [SerializeField] private float staminaRegenerationAmount = 2;
        private float staminaRegenerationTimer = 0;
        private float staminaTickTimer = 0;
        [SerializeField] private float staminaRegenerationDelay = 2;

        protected virtual void Awake()
        {
            character = GetComponent<CharacterManager>();
        }

        protected virtual void Start()
        {
            
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
    }
}