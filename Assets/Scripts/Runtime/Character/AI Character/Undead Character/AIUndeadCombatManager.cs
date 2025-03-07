using UnityEngine;

namespace LZ
{
    public class AIUndeadCombatManager : AICharacterCombatManager
    {
        [Header("Damage Colliders")]
        [SerializeField] private UndeadHandDamageCollider rightHandDamageCollider;
        [SerializeField] private UndeadHandDamageCollider leftHandDamageCollider;

        [Header("Damage")]
        [SerializeField] private int baseDamage = 25;
        [SerializeField] private float attack01DamageModifier = 1.0f;
        [SerializeField] private float attack02DamageModifier = 1.4f;

        public void SetAttack01Damage()
        {
            rightHandDamageCollider.physicalDamage = baseDamage * attack01DamageModifier;
            leftHandDamageCollider.physicalDamage = baseDamage * attack01DamageModifier;
        }
        
        public void SetAttack02Damage()
        {
            rightHandDamageCollider.physicalDamage = baseDamage * attack02DamageModifier;
            leftHandDamageCollider.physicalDamage = baseDamage * attack02DamageModifier;
        }

        public void OpenRightHandDamageCollider()
        {
            aiCharacter.characterSoundFXManager.PlayAttackGrunt();
            rightHandDamageCollider.EnableDamageCollider();
        }

        public void DisableRightHandDamageCollider()
        {
            rightHandDamageCollider.DisableDamageCollider();
        }
        
        public void OpenLeftHandDamageCollider()
        {
            aiCharacter.characterSoundFXManager.PlayAttackGrunt();
            leftHandDamageCollider.EnableDamageCollider();
        }

        public void DisableLeftHandDamageCollider()
        {
            leftHandDamageCollider.DisableDamageCollider();
        }
    }
}