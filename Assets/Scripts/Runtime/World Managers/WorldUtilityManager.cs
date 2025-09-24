using System;
using UnityEngine;

namespace LZ
{
    public class WorldUtilityManager : MonoBehaviour
    {
        public static WorldUtilityManager Instance;

        [Header("Layers")]
        [SerializeField] private LayerMask characterLayers;
        [SerializeField] private LayerMask environLayers;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public LayerMask GetCharacterLayers()
        {
            return characterLayers;
        }

        public LayerMask GetEnvironLayers()
        {
            return environLayers;
        }

        public bool CanIDamageThisTarget(CharacterGroup attackingCharacter, CharacterGroup targetCharacter)
        {
            if (attackingCharacter == CharacterGroup.Team01)
            {
                switch (targetCharacter)
                {
                    case CharacterGroup.Team01:
                        return false;
                    case CharacterGroup.Team02:
                        return true;
                    default:
                        break;
                }
            }
            else if (attackingCharacter == CharacterGroup.Team02)
            {
                switch (targetCharacter)
                {
                    case CharacterGroup.Team01:
                        return true;
                    case CharacterGroup.Team02:
                        return false;
                    default:
                        break;
                }
            }

            return false;
        }

        public float GetAngleOfTarget(Transform characterTransform, Vector3 targetsDirection)
        {
            targetsDirection.y = 0;
            float viewableAngle = Vector3.Angle(characterTransform.forward, targetsDirection);
            Vector3 cross = Vector3.Cross(characterTransform.forward, targetsDirection);

            if (cross.y < 0) 
                viewableAngle = -viewableAngle;

            return viewableAngle;
        }

        public DamageIntensity GetDamageIntensityBasedOnPoiseDamage(float poiseDamage)
        {
            // 投掷匕首、小型物品
            DamageIntensity damageIntensity = DamageIntensity.Ping;

            // 匕首 / 轻攻击
            if (poiseDamage >= 10)
                damageIntensity = DamageIntensity.Light;

            // 标准武器 / 中型攻击
            if (poiseDamage >= 30)
                damageIntensity = DamageIntensity.Medium;

            // 巨型武器 / 重攻击
            if (poiseDamage >= 70)
                damageIntensity = DamageIntensity.Heavy;

            // 终极武器 / 毁天灭地攻击
            if (poiseDamage >= 120)
                damageIntensity = DamageIntensity.Colossal;

            return damageIntensity;
        }
    }
}