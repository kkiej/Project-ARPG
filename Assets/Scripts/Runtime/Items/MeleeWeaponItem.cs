using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Items/Weapons/Melee Weapon")]
    public class MeleeWeaponItem : WeaponItem
    {
        [Header("Attack Modifiers")]
        public float riposte_Attack_01_Modifier = 3.3f;
        public float backstab_Attack_01_Modifier = 3.3f;
        // 武器“偏斜”（如果武器在被防御时会弹开另一武器）
        // 可以被加成
    }
}