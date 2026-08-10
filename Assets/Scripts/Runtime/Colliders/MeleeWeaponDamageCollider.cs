using UnityEngine;

namespace LZ
{
    public class MeleeWeaponDamageCollider : DamageCollider
    {
        [Header("Attacking Character")]
        public CharacterManager characterCausingDamage; // （在计算伤害时，这用于检查攻击者的伤害修正、效果等）

        [Header("ER 多段命中框（AtkParam 精确方案，可选）")]
        [Tooltip("该武器的命中框数据（由 WeaponHitboxDataBuilder 生成）。配了才走「逐攻击多段胶囊」；" +
                 "留空则回退旧的单碰撞体（damageCollider）开关。")]
        public WeaponHitboxData hitboxData;
        [Tooltip("并集命中段对应的子胶囊（由 WeaponColliderAutoFitter 生成回填）。")]
        public WeaponHitboxSegmentCollider[] segmentColliders;

        private bool UsesSegments => segmentColliders != null && segmentColliders.Length > 0;

        [Header("Weapon Attack Modifiers")]
        public float light_Attack_01_Modifier;
        public float light_Attack_02_Modifier;
        public float light_Jump_Attack_01_Modifier;
        public float heavy_Attack_01_Modifier;
        public float heavy_Attack_02_Modifier;
        public float heavy_Jump_Attack_01_Modifier;
        public float charge_Attack_01_Modifier;
        public float charge_Attack_02_Modifier;
        public float running_Attack_01_Modifier;
        public float rolling_Attack_01_Modifier;
        public float backstep_Attack_01_Modifier;
        public float dw_Attack_01_Modifier;
        public float dw_Attack_02_Modifier;
        public float dw_Jump_Attack_01_Modifier;
        public float dw_Run_Attack_01_Modifier;
        public float dw_Roll_Attack_01_Modifier;
        public float dw_Backstep_Attack_01_Modifier;

        protected override void Awake()
        {
            base.Awake();

            if (damageCollider == null)
            {
                damageCollider = GetComponent<Collider>();
            }

            // 多段模式（命中框在子物体上）本物体可能没有 Collider —— 此时 damageCollider 为 null，跳过。
            // 各子段的关闭由 WeaponHitboxSegmentCollider 自身 Awake 处理。
            if (damageCollider != null)
                damageCollider.enabled = false; // 近战武器的碰撞体应该在开始时是关闭的，只有当动作允许时才打开
        }

        protected override void OnTriggerEnter(Collider other)
        {
            // 单碰撞体（旧）模式：命中框就在本物体上，直接结算。
            // 多段模式下命中来自子物体的 WeaponHitboxSegmentCollider，经 HandleContact 转交进来。
            HandleContact(other, transform.position);
        }

        /// <summary>
        /// 命中结算入口。单碰撞体模式由本类 OnTriggerEnter 调用；
        /// 多段模式由各 <see cref="WeaponHitboxSegmentCollider"/> 子物体的触发转交（<paramref name="fromPos"/> 为该段位置，用于取接触点）。
        /// 共用同一份 <c>charactersDamaged</c>，因此一次挥砍多段命中同一目标只结算一次。
        /// </summary>
        public void HandleContact(Collider other, Vector3 fromPos)
        {
            CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();
            if (damageTarget == null)
                return;

            contactPoint = other.ClosestPointOnBounds(fromPos);

            //  WE DO NOT WANT TO DAMAGE OURSELVES
            if (damageTarget == characterCausingDamage)
                return;

            //  CHECK IF WE CAN DAMAGE THIS TARGET BASED ON FRIENDLY FIRE
            if (!WorldUtilityManager.Instance.CanIDamageThisTarget(characterCausingDamage.characterGroup, damageTarget.characterGroup))
                return;

            //  CHECK IF TARGET IS PARRYING
            CheckForParry(damageTarget);

            //  CHECK IF TARGET IS BLOCKING
            CheckForBlock(damageTarget);

            if (!damageTarget.characterNetworkManager.isInvulnerable.Value)
                DamageTarget(damageTarget);
        }

        //  ER 多段命中框：按当前攻击（动画槽号 → AtkParam ID）取命中段，只激活这些段的胶囊并按其半径开启。
        //  解析不到具体攻击（或该武器无 hitboxData）时，回退到并集：按各段 defaultRadius 全部启用。
        public override void EnableDamageCollider()
        {
            if (!UsesSegments)
            {
                base.EnableDamageCollider();
                charactersDamaged.Clear(); // 每次开框重置本次挥砍已伤害列表
                return;
            }

            charactersDamaged.Clear();

            WeaponHitSegment[] segs = null;
            if (hitboxData != null && characterCausingDamage != null)
            {
                int atkId = hitboxData.ResolveAtkParamId(characterCausingDamage.characterCombatManager.currentAttackMotionId);
                segs = hitboxData.GetSegments(atkId);
            }

            foreach (var sc in segmentColliders)
            {
                if (sc == null)
                    continue;

                if (segs == null)
                {
                    // 回退：整刀刃全部启用（按并集里该段的最大半径）
                    sc.Activate(sc.defaultRadius);
                    continue;
                }

                if (TryGetSegmentRadius(segs, sc.dmyA, sc.dmyB, out float radius))
                    sc.Activate(radius);
                else
                    sc.Deactivate();
            }
        }

        public override void DisableDamageCollider()
        {
            if (UsesSegments)
            {
                foreach (var sc in segmentColliders)
                    if (sc != null)
                        sc.Deactivate();
                charactersDamaged.Clear();
                return;
            }

            base.DisableDamageCollider();
        }

        private static bool TryGetSegmentRadius(WeaponHitSegment[] segs, int dmyA, int dmyB, out float radius)
        {
            for (int i = 0; i < segs.Length; i++)
            {
                if (segs[i].dmyA == dmyA && segs[i].dmyB == dmyB)
                {
                    radius = segs[i].radius;
                    return true;
                }
            }
            radius = 0f;
            return false;
        }

        protected override void CheckForParry(CharacterManager damageTarget)
        {
            if (charactersDamaged.Contains(damageTarget))
                return;

            if (!characterCausingDamage.characterNetworkManager.isParryable.Value)
                return;

            if (!damageTarget.IsOwner)
                return;

            if (damageTarget.characterNetworkManager.isParrying.Value)
            {
                charactersDamaged.Add(damageTarget);
                damageTarget.characterNetworkManager.NotifyServerOfParryServerRpc(characterCausingDamage.NetworkObjectId);
                var ad = damageTarget.characterAnimatorManager.animData;
                if (ad != null && ad.parryLand != null)
                    damageTarget.characterAnimatorManager.PlayTargetActionAnimationInstantly(ad.parryLand, true);
                else
                    Debug.LogWarning($"{damageTarget.name}: parryLand clip 未配置", damageTarget);
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
            damageEffect.poiseDamage = poiseDamage;
            damageEffect.contactPoint = contactPoint;
            damageEffect.angleHitFrom = Vector3.SignedAngle(characterCausingDamage.transform.forward, damageTarget.transform.forward, Vector3.up);

            switch (characterCausingDamage.characterCombatManager.currentAttackType)
            {
                case AttackType.LightAttack01:
                    ApplyAttackDamageModifiers(light_Attack_01_Modifier, damageEffect);
                    break;
                case AttackType.LightAttack02:
                    ApplyAttackDamageModifiers(light_Attack_02_Modifier, damageEffect);
                    break;
                case AttackType.LightJumpingAttack01:
                    ApplyAttackDamageModifiers(light_Jump_Attack_01_Modifier, damageEffect);
                    break;
                case AttackType.HeavyAttack01:
                    ApplyAttackDamageModifiers(heavy_Attack_01_Modifier, damageEffect);
                    break;
                case AttackType.HeavyAttack02:
                    ApplyAttackDamageModifiers(heavy_Attack_02_Modifier, damageEffect);
                    break;
                case AttackType.HeavyJumpingAttack01:
                    ApplyAttackDamageModifiers(heavy_Jump_Attack_01_Modifier, damageEffect);
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
                case AttackType.DualAttack01:
                    ApplyAttackDamageModifiers(dw_Attack_01_Modifier, damageEffect);
                    break;
                case AttackType.DualAttack02:
                    ApplyAttackDamageModifiers(dw_Attack_02_Modifier, damageEffect);
                    break;
                case AttackType.DualJumpAttack:
                    ApplyAttackDamageModifiers(dw_Jump_Attack_01_Modifier, damageEffect);
                    break;
                case AttackType.DualRunAttack:
                    ApplyAttackDamageModifiers(dw_Run_Attack_01_Modifier, damageEffect);
                    break;
                case AttackType.DualRollAttack:
                    ApplyAttackDamageModifiers(dw_Roll_Attack_01_Modifier, damageEffect);
                    break;
                case AttackType.DualBackstepAttack:
                    ApplyAttackDamageModifiers(dw_Backstep_Attack_01_Modifier, damageEffect);
                    break;
                default:
                    break;
            }
            
            if (characterCausingDamage.IsOwner)
            {
                damageTarget.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
                    damageTarget.NetworkObjectId,
                    characterCausingDamage.NetworkObjectId,
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

        private void ApplyAttackDamageModifiers(float modifier, TakeDamageEffect damage)
        {
            damage.physicalDamage *= modifier;
            damage.magicDamage *= modifier;
            damage.fireDamage *= modifier;
            damage.holyDamage *= modifier;
            damage.poiseDamage *= modifier;
            
            // 如果攻击是完全蓄力的重击，在正常修正计算后乘以完全蓄力修正值
        }
    }
}