using System;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class DamageCollider : MonoBehaviour
    {
        [Header("Collider")]
        [SerializeField] protected Collider damageCollider;
        
        [Header("Damage")]
        public float physicalDamage = 0; // （未来会被分为“标准”，“打击”，“挥砍”和“穿刺”）
        public float magicDamage = 0;
        public float fireDamage = 0;
        public float lightningDamage = 0;
        public float holyDamage = 0;

        [Header("Poise")]
        public float poiseDamage = 0;

        [Header("Contact Point")]
        protected Vector3 contactPoint;

        [Header("Characters Damaged")]
        protected List<CharacterManager> charactersDamaged = new List<CharacterManager>();
        
        [Header("Block")]
        protected Vector3 directionFromAttackToDamageTarget;
        protected float dotValueFromAttackToDamageTarget;

        protected virtual void Awake()
        {
            
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();

            if (damageTarget != null)
            {
                contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
                
                // 检查是否可以基于友军伤害对目标造成伤害
                
                // 检查目标是否在格挡
                CheckForBlock(damageTarget);

                //  CHECK IF TARGET IS PARRYING
                CheckForParry(damageTarget);

                if (!damageTarget.characterNetworkManager.isInvulnerable.Value)
                    DamageTarget(damageTarget);
            }
        }
        
        protected virtual void CheckForBlock(CharacterManager damageTarget)
        {
            //  若该角色已受损，请勿继续
            if (charactersDamaged.Contains(damageTarget))
                return;

            GetBlockingDotValues(damageTarget);

            // 1. 检查受损角色是否处于格挡状态
            if (damageTarget.characterNetworkManager.isBlocking.Value && dotValueFromAttackToDamageTarget > 0.3f)
            {
                // 2. 若角色正在格挡，则检查其是否面向正确方向以成功格挡

                charactersDamaged.Add(damageTarget);

                TakeBlockedDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeBlockedDamageEffect);

                damageEffect.physicalDamage = physicalDamage;
                damageEffect.magicDamage = magicDamage;
                damageEffect.fireDamage = fireDamage;
                damageEffect.holyDamage = holyDamage;
                damageEffect.poiseDamage = poiseDamage;
                damageEffect.staminaDamage = poiseDamage;   // IF YOU WANT TO GIVE STAMINA DAMAGE ITS OWN VARIABLE, INSTEAD OF USING POISE GO FOR IT
                damageEffect.contactPoint = contactPoint;

                // 3. 对目标施加被格挡后的角色的伤害
                damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);
            }
        }

        protected virtual void CheckForParry(CharacterManager damageTarget)
        {

        }

        protected virtual void GetBlockingDotValues(CharacterManager damageTarget)
        {
            directionFromAttackToDamageTarget = transform.position - damageTarget.transform.position;
            dotValueFromAttackToDamageTarget = Vector3.Dot(directionFromAttackToDamageTarget, damageTarget.transform.forward);
        }

        protected virtual void DamageTarget(CharacterManager damageTarget)
        {
            // 我们不想在一次攻击中对同一个目标造成多次伤害
            // 所以我们将它们添加到一个列表中，在造成伤害之前进行检查
            if (charactersDamaged.Contains(damageTarget))
                return;
            
            charactersDamaged.Add(damageTarget);

            TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeDamageEffect);
            damageEffect.physicalDamage = physicalDamage;
            damageEffect.magicDamage = magicDamage;
            damageEffect.fireDamage = fireDamage;
            //damageEffect.lightningDamage = lightningDamage;
            damageEffect.holyDamage = holyDamage;
            damageEffect.poiseDamage = poiseDamage;
            damageEffect.contactPoint = contactPoint;
            
            damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);
        }

        public virtual void EnableDamageCollider()
        {
            damageCollider.enabled = true;
        }

        public virtual void DisableDamageCollider()
        {
            damageCollider.enabled = false;
            charactersDamaged.Clear(); // 当我们重置攻击碰撞的时候重置可以被攻击的角色列表，这样他们就可以再次被攻击
        }
    }
}