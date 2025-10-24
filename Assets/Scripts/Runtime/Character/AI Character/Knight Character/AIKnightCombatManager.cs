using UnityEngine;

namespace LZ
{
    public class AIKnightCombatManager : AICharacterCombatManager
    {
        [Header("Damage Colliders")]
        [SerializeField] ManualDamageCollider swordDamageCollider;

        [Header("Damage Modifiers")]
        [SerializeField] float attack01DamageModifier = 1.0f;
        [SerializeField] float attack02DamageModifier = 1.4f;

        public void SetAttack01Damage()
        {
            swordDamageCollider.physicalDamage = baseDamage * attack01DamageModifier;
            swordDamageCollider.poiseDamage = basePoiseDamage * attack01DamageModifier;
        }

        public void SetAttack02Damage()
        {
            swordDamageCollider.physicalDamage = baseDamage * attack02DamageModifier;
            swordDamageCollider.poiseDamage = basePoiseDamage * attack02DamageModifier;
        }

        public void OpenSwordDamageCollider()
        {
            aiCharacter.characterSoundFXManager.PlayAttackGruntSoundFX();
            swordDamageCollider.EnableDamageCollider();
        }

        public void CloseSwordDamageCollider()
        {
            swordDamageCollider.DisableDamageCollider();
        }

        public override void CloseAllDamageColliders()
        {
            base.CloseAllDamageColliders();

            swordDamageCollider.DisableDamageCollider();
        }
    }
}
