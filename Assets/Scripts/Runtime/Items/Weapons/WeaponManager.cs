using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class WeaponManager : MonoBehaviour
    {
        public MeleeWeaponDamageCollider meleeDamageCollider;

        private void Awake()
        {
            meleeDamageCollider = GetComponentInChildren<MeleeWeaponDamageCollider>();
        }

        public void SetWeaponDamage(CharacterManager characterWieldingWeapon, WeaponItem weapon)
        {
            if (meleeDamageCollider == null)
                return;

            int upgradeLevel = (int)weapon.upgradeLevel;
            int upgradeDamage = 0;

            //  11 DAMAGE ADDED TO WEAPON PER UPGRADE LEVEL
            for (int i = 0; i < upgradeLevel; i++)
            {
                if (i >= 1)
                    upgradeDamage += 11;
            }

            //  TO DO, EFFECT SCALING MULTIPLIERS


            meleeDamageCollider.characterCausingDamage = characterWieldingWeapon;

            //  IF THE BASE DAMAGE IS ABOVE 0, APPLY UPGRADED DAMAGE TO THE DAMAGE TYPE
            int physicalDamage = weapon.physicalDamage;
            if (physicalDamage > 0)
                physicalDamage += upgradeDamage;
            meleeDamageCollider.physicalDamage = physicalDamage;

            int magicDamage = weapon.magicDamage;
            if (magicDamage > 0)
                magicDamage += upgradeDamage;
            meleeDamageCollider.magicDamage = magicDamage;

            int fireDamage = weapon.fireDamage;
            if (fireDamage > 0)
                fireDamage += upgradeDamage;
            meleeDamageCollider.fireDamage = fireDamage;

            int lightningDamage = weapon.lightningDamage;
            if (lightningDamage > 0)
                lightningDamage += upgradeDamage;
            meleeDamageCollider.lightningDamage = lightningDamage;

            int holyDamage = weapon.holyDamage;
            if (holyDamage > 0)
                holyDamage += upgradeDamage;
            meleeDamageCollider.holyDamage = holyDamage;

            meleeDamageCollider.poiseDamage = weapon.poiseDamage;

            meleeDamageCollider.light_Attack_01_Modifier = weapon.light_Attack_01_Modifier;
            meleeDamageCollider.light_Attack_02_Modifier = weapon.light_Attack_02_Modifier;
            meleeDamageCollider.light_Jump_Attack_01_Modifier = weapon.light_Jumping_Attack_01_Modifier;
            meleeDamageCollider.heavy_Attack_01_Modifier = weapon.heavy_Attack_01_Modifier;
            meleeDamageCollider.heavy_Attack_02_Modifier = weapon.heavy_Attack_02_Modifier;
            meleeDamageCollider.heavy_Jump_Attack_01_Modifier = weapon.heavy_Jumping_Attack_01_Modifier;
            meleeDamageCollider.charge_Attack_01_Modifier = weapon.charge_Attack_01_Modifier;
            meleeDamageCollider.charge_Attack_02_Modifier = weapon.charge_Attack_02_Modifier;
            
            meleeDamageCollider.running_Attack_01_Modifier = weapon.running_Attack_01_Modifier;
            meleeDamageCollider.rolling_Attack_01_Modifier = weapon.rolling_Attack_01_Modifier;
            meleeDamageCollider.backstep_Attack_01_Modifier = weapon.backstep_Attack_01_Modifier;

            meleeDamageCollider.dw_Attack_01_Modifier = weapon.dw_Attack_01_Modifier;
            meleeDamageCollider.dw_Attack_02_Modifier = weapon.dw_Attack_02_Modifier;
            meleeDamageCollider.dw_Jump_Attack_01_Modifier = weapon.dw_Jump_Attack_01_Modifier;
            meleeDamageCollider.dw_Run_Attack_01_Modifier = weapon.dw_Run_Attack_01_Modifier;
            meleeDamageCollider.dw_Roll_Attack_01_Modifier = weapon.dw_Roll_Attack_01_Modifier;
            meleeDamageCollider.dw_Backstep_Attack_01_Modifier = weapon.dw_Backstep_Attack_01_Modifier;
        }
    }
}