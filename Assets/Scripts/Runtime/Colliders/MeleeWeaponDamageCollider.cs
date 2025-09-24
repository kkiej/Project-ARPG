using UnityEngine;

namespace LZ
{
    public class MeleeWeaponDamageCollider : DamageCollider
    {
        [Header("Attacking Character")]
        public CharacterManager characterCausingDamage; // （在计算伤害时，这用于检查攻击者的伤害修正、效果等）

        [Header("Weapon Attack Modifiers")]
        public float light_Attack_01_Modifier;
        public float light_Attack_02_Modifier;
        public float heavy_Attack_01_Modifier;
        public float heavy_Attack_02_Modifier;
        public float charge_Attack_01_Modifier;
        public float charge_Attack_02_Modifier;
        public float running_Attack_01_Modifier;
        public float rolling_Attack_01_Modifier;
        public float backstep_Attack_01_Modifier;

        protected override void Awake()
        {
            base.Awake();

            if (damageCollider == null)
            {
                damageCollider = GetComponent<Collider>();
            }

            damageCollider.enabled = false; // 近战武器的碰撞体应该在开始时是关闭的，只有当动作允许时才打开
        }

        protected override void OnTriggerEnter(Collider other)
        {
            CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();

            if (damageTarget != null)
            {
                // 我们不想伤害自己
                if (damageTarget == characterCausingDamage)
                    return;
                
                contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
                
                // 检查是否可以基于友军伤害对目标造成伤害
                
                // 检查目标是否在格挡
                
                // 检查目标是否无敌（已经在TakeDamageEffect中检查了）
                //if (damageTarget.characterNetworkManager.isInvulnerable.Value)
                //    return;
                
                DamageTarget(damageTarget);
            }
        }

        protected override void GetBlockingDotValues(CharacterManager damageTarget)
        {
            directionFromAttackToDamageTarget = characterCausingDamage.transform.position - damageTarget.transform.position;
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
            damageEffect.contactPoint = contactPoint;
            damageEffect.angleHitFrom = Vector3.SignedAngle(characterCausingDamage.transform.forward,
                damageTarget.transform.forward, Vector3.up);

            switch (characterCausingDamage.characterCombatManager.currentAttackType)
            {
                case AttackType.LightAttack01:
                    ApplyAttackDamageModifiers(light_Attack_01_Modifier, damageEffect);
                    break;
                case AttackType.LightAttack02:
                    ApplyAttackDamageModifiers(light_Attack_02_Modifier, damageEffect);
                    break;
                case AttackType.HeavyAttack01:
                    ApplyAttackDamageModifiers(heavy_Attack_01_Modifier, damageEffect);
                    break;
                case AttackType.HeavyAttack02:
                    ApplyAttackDamageModifiers(heavy_Attack_02_Modifier, damageEffect);
                    break;
                case AttackType.ChargedAttack01:
                    ApplyAttackDamageModifiers(charge_Attack_01_Modifier, damageEffect);
                    break;
                case AttackType.ChargedAttack02:
                    ApplyAttackDamageModifiers(charge_Attack_02_Modifier, damageEffect);
                    break;
                case AttackType.RunningAttack01:
                    ApplyAttackDamageModifiers(running_Attack_01_Modifier, damageEffect);
                    break;
                case AttackType.RollingAttack01:
                    ApplyAttackDamageModifiers(rolling_Attack_01_Modifier, damageEffect);
                    break;
                case AttackType.BackstepAttack01:
                    ApplyAttackDamageModifiers(backstep_Attack_01_Modifier, damageEffect);
                    break;
                default:
                    break;
            }
            
            if (characterCausingDamage.IsOwner)
            {
                damageTarget.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
                    damageTarget.NetworkObjectId, characterCausingDamage.NetworkObjectId, damageEffect.physicalDamage,
                    damageEffect.magicDamage, damageEffect.fireDamage, damageEffect.holyDamage,
                    damageEffect.poiseDamage, damageEffect.angleHitFrom, damageEffect.contactPoint.x,
                    damageEffect.contactPoint.y, damageEffect.contactPoint.z);
            }
        }

        private void ApplyAttackDamageModifiers(float modifier, TakeDamageEffect damage)
        {
            damage.physicalDamage *= modifier;
            damage.magicDamage *= modifier;
            damage.fireDamage *= modifier;
            damage.lightningDamage *= modifier;
            damage.holyDamage *= modifier;
            damage.poiseDamage *= modifier;
            
            // 如果攻击是完全蓄力的重击，在正常修正计算后乘以完全蓄力修正值
        }
    }
}