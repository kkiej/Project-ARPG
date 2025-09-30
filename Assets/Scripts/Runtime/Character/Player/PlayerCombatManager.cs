using Unity.Netcode;
using UnityEngine;

namespace LZ
{
    public class PlayerCombatManager : CharacterCombatManager
    {
        private PlayerManager player;
        
        public WeaponItem currentWeaponBeingUsed;

        [Header("Flags")]
        public bool canComboWithMainHandWeapon;
        //public bool canComboWithOffHandWeapon;

        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
        }

        public void PerformWeaponBasedAction(WeaponItemAction weaponAction, WeaponItem weaponPerformingAction)
        {
            if (player.IsOwner)
            {
                // 执行动作
                weaponAction.AttemptToPerformAction(player, weaponPerformingAction);
            
                // 通知服务器我们执行了动作，因此我们也从服务器端执行它
                player.playerNetworkManager.NotifyTheServerOfWeaponActionServerRpc(
                    NetworkManager.Singleton.LocalClientId, weaponAction.actionID, weaponPerformingAction.itemID);
            }
        }

        // 处决攻击
        public override void AttemptRiposte(RaycastHit hit)
        {
            CharacterManager targetCharacter = hit.transform.gameObject.GetComponent<CharacterManager>();

            // 若因故目标角色为空，则直接返回
            if (targetCharacter == null)
                return;

            // 若自初始检测后目标角色因故无法被处决，则直接返回
            if (!targetCharacter.characterNetworkManager.isRipostable.Value)
                return;

            // 若已有其他单位正在对该角色执行致命一击（或本端已在执行），则直接返回
            if (targetCharacter.characterNetworkManager.isBeingCriticallyDamaged.Value)
                return;

            // 仅可使用近战武器发动弹反处决
            MeleeWeaponItem riposteWeapon;
            MeleeWeaponDamageCollider riposteCollider;

            //  TODO: 检查当前是双手持握左手武器还是右手武器（这将决定弹反处决时使用的武器）

            riposteWeapon = player.playerInventoryManager.currentRightHandWeapon as MeleeWeaponItem;
            riposteCollider = player.playerEquipmentManager.rightWeaponManager.meleeDamageCollider;

            // 弹反处决动画将根据武器的动画控制器而变化，因此动画可在对应控制器中选择，但动画名称将始终保持一致
            character.characterAnimatorManager.PlayTargetActionAnimationInstantly("Riposte_01", true);

            //  WHILST PERFORMING A CRITICAL STRIKE, YOU CANNOT BE DAMAGED
            if (character.IsOwner)
                character.characterNetworkManager.isInvulnerable.Value = true;

            // 1. CREATE A NEW DAMAGE EFFECT FOR THIS TYPE OF DAMAGE
            TakeCriticalDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeCriticalDamageEffect);

            // 2. APPLY ALL OF THE DAMAGE STATS FROM THE COLLIDER TO THE DAMAGE EFFECT
            damageEffect.physicalDamage = riposteCollider.physicalDamage;
            damageEffect.holyDamage = riposteCollider.holyDamage;
            damageEffect.fireDamage = riposteCollider.fireDamage;
            damageEffect.lightningDamage = riposteCollider.lightningDamage;
            damageEffect.magicDamage = riposteCollider.magicDamage;
            damageEffect.poiseDamage = riposteCollider.poiseDamage;

            // 3. MULTIPLY DAMAGE BY WEAPONS RIPOSTE MODIFIER
            damageEffect.physicalDamage *= riposteWeapon.riposte_Attack_01_Modifier;
            damageEffect.holyDamage *= riposteWeapon.riposte_Attack_01_Modifier;
            damageEffect.fireDamage *= riposteWeapon.riposte_Attack_01_Modifier;
            damageEffect.lightningDamage *= riposteWeapon.riposte_Attack_01_Modifier;
            damageEffect.magicDamage *= riposteWeapon.riposte_Attack_01_Modifier;
            damageEffect.poiseDamage *= riposteWeapon.riposte_Attack_01_Modifier;

            // 4. USING A SERVER RPC SEND THE RIPOSTE TO THE TARGET, WHERE THEY WILL PLAY THE PROPER ANIMATIONS ON THEIR END, AND TAKE THE DAMAGE
            targetCharacter.characterNetworkManager.NotifyTheServerOfRiposteServerRpc(
                targetCharacter.NetworkObjectId,
                character.NetworkObjectId,
                "Riposted_01",
                riposteWeapon.itemID,
                damageEffect.physicalDamage,
                damageEffect.magicDamage,
                damageEffect.fireDamage,
                damageEffect.holyDamage,
                damageEffect.poiseDamage);
        }

        public virtual void DrainStaminaBasedOnAttack()
        {
            if (!player.IsOwner)
                return;

            if (currentWeaponBeingUsed == null)
                return;

            float staminaDeducted = 0;

            switch (currentAttackType)
            {
                case AttackType.LightAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.lightAttackStaminaCostMultiplier;
                    break;
                case AttackType.LightAttack02:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.lightAttackStaminaCostMultiplier;
                    break;
                case AttackType.HeavyAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.heavyAttackStaminaCostMultiplier;
                    break;
                case AttackType.HeavyAttack02:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.heavyAttackStaminaCostMultiplier;
                    break;
                case AttackType.ChargedAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.chargedAttackStaminaCostMultiplier;
                    break;
                case AttackType.ChargedAttack02:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.chargedAttackStaminaCostMultiplier;
                    break;
                case AttackType.RunningAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.runningAttackStaminaCostMultiplier;
                    break;
                case AttackType.RollingAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.rollingAttackStaminaCostMultiplier;
                    break;
                case AttackType.BackstepAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.backstepAttackStaminaCostMultiplier;
                    break;
                default:
                    break;
            }

            player.playerNetworkManager.currentStamina.Value -= Mathf.RoundToInt(staminaDeducted);
        }

        public override void SetTarget(CharacterManager newTarget)
        {
            base.SetTarget(newTarget);

            if (player.IsOwner)
            {
                PlayerCamera.instance.SetLockCameraHeight();
            }
        }
        
        // 动画事件调用
        public override void EnableCanDoCombo()
        {
            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                player.playerCombatManager.canComboWithMainHandWeapon = true;
            }
            else
            {
                // Enable off hand combo
            }
        }

        public override void DisableCanDoCombo()
        {
            player.playerCombatManager.canComboWithMainHandWeapon = false;
            //player.playerCombatManager.canComboWithOffHandWeapon = false;
        }
    }
}