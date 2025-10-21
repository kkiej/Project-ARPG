using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class RangedProjectileDamageCollider : DamageCollider
    {
        [Header("Marksmen")]
        public CharacterManager characterShootingProjectile;

        [Header("Collision")]
        private bool hasPenetratedSurface = false;
        public Rigidbody rigidBody;
        private CapsuleCollider capsuleCollider;

        protected override void Awake()
        {
            base.Awake();

            rigidBody = GetComponent<Rigidbody>();
            capsuleCollider = GetComponent<CapsuleCollider>();
        }

        private void FixedUpdate()
        {
            if (rigidBody.linearVelocity != Vector3.zero)
            {
                rigidBody.rotation = Quaternion.LookRotation(rigidBody.linearVelocity);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            CreatePenetrationIntoObject(collision);

            CharacterManager potentialTarget = collision.transform.gameObject.GetComponent<CharacterManager>();

            //  (TODO) CHECK FOR SHIELD OBJECT AND PERFORM BLOCK

            //  (TODO) INSTANTIATE IMPACT PARTICLE AND PERFORM ARROW PENETRATION

            if (characterShootingProjectile == null)
                return;

            Collider contactCollider = collision.gameObject.GetComponent<Collider>();

            if (contactCollider != null)
                contactPoint = contactCollider.ClosestPointOnBounds(transform.position);

            if (potentialTarget == null)
                return;

            if (WorldUtilityManager.Instance.CanIDamageThisTarget(characterShootingProjectile.characterGroup, potentialTarget.characterGroup))
            {
                CheckForBlock(potentialTarget);
                DamageTarget(potentialTarget);
            }
        }

        protected override void CheckForBlock(CharacterManager damageTarget)
        {
            if (charactersDamaged.Contains(damageTarget))
                return;

            float angle = Vector3.Angle(damageTarget.transform.forward, transform.forward);

            if (damageTarget.characterNetworkManager.isBlocking.Value && angle > 145)
            {
                charactersDamaged.Add(damageTarget);
                TakeBlockedDamageEffect blockedDamageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeBlockedDamageEffect);

                if (characterShootingProjectile != null)
                    blockedDamageEffect.characterCausingDamage = characterShootingProjectile;

                blockedDamageEffect.physicalDamage = physicalDamage;
                blockedDamageEffect.magicDamage = magicDamage;
                blockedDamageEffect.fireDamage = fireDamage;
                blockedDamageEffect.holyDamage = holyDamage;
                blockedDamageEffect.poiseDamage = poiseDamage;
                blockedDamageEffect.staminaDamage = poiseDamage;   // IF YOU WANT TO GIVE STAMINA DAMAGE ITS OWN VARIABLE, INSTEAD OF USING POISE GO FOR IT
                blockedDamageEffect.contactPoint = contactPoint;

                // 3. APPLY BLOCKED CHARACTER DAMAGE TO TARGET
                damageTarget.characterEffectsManager.ProcessInstantEffect(blockedDamageEffect);
            }

            //  TO DO MAKE ARROW "BOUNCE OFF" SHIELD INSTEAD OF PENETRATING PLAYER/CHARACTER
        }

        private void CreatePenetrationIntoObject(Collision hit)
        {
            if (!hasPenetratedSurface)
            {
                hasPenetratedSurface = true;

                //  GET THE CONTACT POINT
                gameObject.transform.position = hit.GetContact(0).point;

                //  STOPS OUR ARROW FROM "SCALING" IN SIZE WITH SCALED UP OR DOWN OBJECTS
                var emptyObject = new GameObject();
                emptyObject.transform.parent = hit.collider.transform;
                gameObject.transform.SetParent(emptyObject.transform, true);

                //  HOW FAR THE ARROW PENETRATES INTO THE SURFACE
                transform.position += transform.forward * (Random.Range(0.1f, 0.3f));

                //  DISABLE COLLIDERS AND RIGIDBODY
                rigidBody.isKinematic = true;
                capsuleCollider.enabled = false;

                //  DESTROY DAMAGE COLLIDER, AND DESTROY ARROW AFTER A TIME
                Destroy(GetComponent<RangedProjectileDamageCollider>());
                Destroy(gameObject, 20);
            }
        }
    }
}
