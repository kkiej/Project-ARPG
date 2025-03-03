using UnityEngine;

namespace LZ
{
    public class WeaponItem : Item
    {
        // 动画控制器覆盖（根据当前使用的武器更改攻击动画）

        [Header("Weapon Model")]
        public GameObject weaponModel;

        [Header("Weapon Requirements")]
        public int strengthREQ = 0;
        public int dexREQ = 0;
        public int intREQ = 0;
        public int faithREQ = 0;

        [Header("Weapon Base Damage")]
        public int physicalDamage = 0;
        public int magicDamage = 0;
        public int fireDamage = 0;
        public int holyDamage = 0;
        public int lightningDamage = 0;
        
        // 武器防御吸收（阻挡能力）

        [Header("Weapon Poise")]
        public float poiseDamage = 10;
        // 攻击时的进攻姿态加成

        [Header("Attack Modifiers")]
        public float light_Attack_01_Modifier = 1.0f;
        public float light_Attack_02_Modifier = 1.2f;
        public float heavy_Attack_01_Modifier = 1.4f;
        public float heavy_Attack_02_Modifier = 1.6f;
        public float charge_Attack_01_Modifier = 2.0f;
        public float charge_Attack_02_Modifier = 2.2f;
        // 暴击伤害修正等

        [Header("Stamina Cost Modifiers")]
        public int baseStaminaCost = 20;
        public float lightAttackStaminaCostMultiplier = 0.9f;
        // 奔跑攻击耐力消耗修正
        // 重攻击耐力消耗修正等

        [Header("Actions")]
        public WeaponItemAction oh_RB_Action;
        public WeaponItemAction oh_RT_Action;

        // 战争灰烬
        // 格挡声音

        [Header("Whooshes")]
        public AudioClip[] whooshes;
    }
}