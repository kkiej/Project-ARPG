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
        private bool hasCollided = false;
        public Rigidbody rigidBody;

        protected override void Awake()
        {
            base.Awake();

            rigidBody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (rigidBody.velocity != Vector3.zero)
            {
                rigidBody.rotation = Quaternion.LookRotation(rigidBody.velocity);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!hasCollided)
            {
                //  NOT USED YET
                //hasCollided = true;

                CharacterManager potentialTarget = collision.transform.gameObject.GetComponent<CharacterManager>();

                //  (TODO) CHECK FOR SHIELD OBJECT AND PERFORM BLOCK

                //  (TODO) INSTANTIATE IMPACT PARTICLE AND PERFORM ARROW PENETRATION

                if (characterShootingProjectile == null)
                    return;

                if (potentialTarget == null)
                    return;

                if (WorldUtilityManager.Instance.CanIDamageThisTarget(characterShootingProjectile.characterGroup, potentialTarget.characterGroup))
                {
                    DamageTarget(potentialTarget);
                }

                //  TEMPORARY UNTIL WE CREATE THE ARROW PENETRATION LOGIC
                Destroy(gameObject);
            }
        }
    }
}
