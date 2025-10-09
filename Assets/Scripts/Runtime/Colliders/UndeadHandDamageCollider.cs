using UnityEngine;
using UnityEngine.Serialization;

namespace LZ
{
    public class UndeadHandDamageCollider : DamageCollider
    {
        [SerializeField] private AICharacterManager undeadCharacter;

        protected override void Awake()
        {
            base.Awake();

            damageCollider = GetComponent<Collider>();
            undeadCharacter = GetComponentInParent<AICharacterManager>();
        }

        protected override void GetBlockingDotValues(CharacterManager damageTarget)
        {
            directionFromAttackToDamageTarget = undeadCharacter.transform.position - damageTarget.transform.position;
            dotValueFromAttackToDamageTarget = Vector3.Dot(directionFromAttackToDamageTarget, damageTarget.transform.forward);
        }

        protected override void DamageTarget(CharacterManager damageTarget)
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
            damageEffect.angleHitFrom = Vector3.SignedAngle(undeadCharacter.transform.forward,
                damageTarget.transform.forward, Vector3.up);

            // 方法1：
            // 如果AI在主机端击中目标，无论在其它客户端是什么表现，都会造成伤害
            //if (undeadCharacter.IsOwner)
            //{
            //    damageTarget.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
            //        damageTarget.NetworkObjectId, undeadCharacter.NetworkObjectId, damageEffect.physicalDamage,
            //        damageEffect.magicDamage, damageEffect.fireDamage, damageEffect.holyDamage,
            //        damageEffect.poiseDamage, damageEffect.angleHitFrom, damageEffect.contactPoint.x,
            //        damageEffect.contactPoint.y, damageEffect.contactPoint.z);
            //}
            
            // 方法2：
            // 如果AI击中了被连接角色，无论在其他客户端是什么表现，都会造成伤害
            if (damageTarget.IsOwner)
            {
                damageTarget.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
                    damageTarget.NetworkObjectId,
                    undeadCharacter.NetworkObjectId,
                    damageEffect.physicalDamage,
                    damageEffect.magicDamage,
                    damageEffect.fireDamage,
                    damageEffect.holyDamage,
                    damageEffect.poiseDamage,
                    damageEffect.angleHitFrom,
                    damageEffect.contactPoint.x,
                    damageEffect.contactPoint.y,
                    damageEffect.contactPoint.z);
            }
        }

        protected override void CheckForParry(CharacterManager damageTarget)
        {
            if (charactersDamaged.Contains(damageTarget))
                return;

            if (!undeadCharacter.characterNetworkManager.isParryable.Value)
                return;

            if (!damageTarget.IsOwner)
                return;

            if (damageTarget.characterNetworkManager.isParrying.Value)
            {
                charactersDamaged.Add(damageTarget);
                damageTarget.characterNetworkManager.NotifyServerOfParryServerRpc(undeadCharacter.NetworkObjectId);
                damageTarget.characterAnimatorManager.PlayTargetActionAnimationInstantly("Parry_Land_01", true);
            }
        }
    }
}