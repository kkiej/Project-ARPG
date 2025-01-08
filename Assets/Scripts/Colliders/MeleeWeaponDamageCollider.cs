using UnityEngine;

namespace LZ
{
    public class MeleeWeaponDamageCollider : DamageCollider
    {
        [Header("Attacking Character")]
        public CharacterManager characterCausingDamage; // （在计算伤害时，这用于检查攻击者的伤害修正、效果等）
    }
}