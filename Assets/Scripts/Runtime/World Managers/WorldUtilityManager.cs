using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class WorldUtilityManager : MonoBehaviour
    {
        public static WorldUtilityManager Instance;

        [Header("Layers")]
        [SerializeField] LayerMask characterLayers;
        [SerializeField] LayerMask enviroLayers;
        [SerializeField] LayerMask slipperyEnviroLayers;

        [Header("Forces")]
        public float slopeSlideForce = -15;

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

            DontDestroyOnLoad(gameObject);
        }

        public LayerMask GetCharacterLayers()
        {
            return characterLayers;
        }

        public LayerMask GetEnviroLayers()
        {
            return enviroLayers;
        }

        public LayerMask GetSlipperyEnviroLayers()
        {
            return slipperyEnviroLayers;
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

        public Vector3 GetRipostingPositionBasedOnWeaponClass(WeaponClass weaponClass)
        {
            Vector3 position = new Vector3(0.11f, 0, 0.7f);

            switch (weaponClass)
            {
                case WeaponClass.StraightSword: // CHANGE POSITION HERE IF YOU DESIRE
                    break;
                case WeaponClass.Spear:  // CHANGE POSITION HERE IF YOU DESIRE
                    break;
                case WeaponClass.MediumShield:  // CHANGE POSITION HERE IF YOU DESIRE
                    break;
                case WeaponClass.Fist:  // CHANGE POSITION HERE IF YOU DESIRE
                    break;
                default:
                    break;
            }

            return position;
        }

        public Vector3 GetBackstabPositionBasedOnWeaponClass(WeaponClass weaponClass)
        {
            Vector3 position = new Vector3(0.12f, 0, 0.74f);

            switch (weaponClass)
            {
                case WeaponClass.StraightSword: // CHANGE POSITION HERE IF YOU DESIRE
                    break;
                case WeaponClass.Spear:  // CHANGE POSITION HERE IF YOU DESIRE
                    break;
                case WeaponClass.MediumShield:  // CHANGE POSITION HERE IF YOU DESIRE
                    break;
                case WeaponClass.Fist:  // CHANGE POSITION HERE IF YOU DESIRE
                    break;
                default:
                    break;
            }

            return position;
        }
    }
}