using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class AIUndeadCombatManager : AICharacterCombatManager
    {
        [Header("Damage Colliders")]
        [SerializeField] ManualDamageCollider rightHandDamageCollider;
        [SerializeField] ManualDamageCollider leftHandDamageCollider;

        [Header("Damage Modifiers")]
        [SerializeField] float attack01DamageModifier = 1.0f;
        [SerializeField] float attack02DamageModifier = 1.4f;

        public void SetAttack01Damage()
        {
            rightHandDamageCollider.physicalDamage = baseDamage * attack01DamageModifier;
            leftHandDamageCollider.physicalDamage = baseDamage * attack01DamageModifier;

            rightHandDamageCollider.poiseDamage = basePoiseDamage * attack01DamageModifier;
            leftHandDamageCollider.poiseDamage = basePoiseDamage * attack01DamageModifier;
        }
        
        public void SetAttack02Damage()
        {
            rightHandDamageCollider.physicalDamage = baseDamage * attack02DamageModifier;
            leftHandDamageCollider.physicalDamage = baseDamage * attack02DamageModifier;

            rightHandDamageCollider.poiseDamage = basePoiseDamage * attack02DamageModifier;
            leftHandDamageCollider.poiseDamage = basePoiseDamage * attack02DamageModifier;
        }

        public void OpenRightHandDamageCollider()
        {
            aiCharacter.characterSoundFXManager.PlayAttackGruntSoundFX();
            rightHandDamageCollider.EnableDamageCollider();
        }

        public void CloseRightHandDamageCollider()
        {
            rightHandDamageCollider.DisableDamageCollider();
        }
        
        public void OpenLeftHandDamageCollider()
        {
            aiCharacter.characterSoundFXManager.PlayAttackGruntSoundFX();
            leftHandDamageCollider.EnableDamageCollider();
        }

        public void CloseLeftHandDamageCollider()
        {
            leftHandDamageCollider.DisableDamageCollider();
        }

        public override void CloseAllDamageColliders()
        {
            base.CloseAllDamageColliders();

            rightHandDamageCollider.DisableDamageCollider();
            leftHandDamageCollider.DisableDamageCollider();
        }
    }
}