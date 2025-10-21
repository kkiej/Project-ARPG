using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Items/Spells/Fire Ball")]
    public class FireBallSpell : SpellItem
    {
        [Header("Projectile Velocity")]
        [SerializeField] float upwardVelocity = 3;
        [SerializeField] float forwardVelocity = 15;
        public override void AttemptToCastSpell(PlayerManager player)
        {
            base.AttemptToCastSpell(player);

            if (!CanICastThisSpell(player))
                return;

            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                player.playerAnimatorManager.PlayTargetActionAnimation(mainHandSpellAnimation, true);
            }
            else
            {
                player.playerAnimatorManager.PlayTargetActionAnimation(offHandSpellAnimation, true);
            }
        }

        public override void InstantiateWarmUpSpellFX(PlayerManager player)
        {
            base.InstantiateWarmUpSpellFX(player);

            // 1. 确定玩家使用的是哪只手施法
            SpellInstantiationLocation spellInstantiationLocation;
            GameObject instantiatedWarmUpSpellFX = Instantiate(spellCastWarmUpFX);

            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                // 2. 在正确位置生成预热特效（咒术直接使用手部，而法杖则使用杖身上的特定点）
                spellInstantiationLocation = player.playerEquipmentManager.rightWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
            }
            else
            {
                // 2. 在正确位置生成预热特效（咒术直接使用手部，而法杖则使用杖身上的特定点）
                spellInstantiationLocation = player.playerEquipmentManager.leftWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
            }

            instantiatedWarmUpSpellFX.transform.parent = spellInstantiationLocation.transform;
            instantiatedWarmUpSpellFX.transform.localPosition = Vector3.zero;
            instantiatedWarmUpSpellFX.transform.localRotation = Quaternion.identity;

            // 3. 将预热特效保存为变量，以便在玩家被打断动画时能够销毁该特效
            player.playerEffectsManager.activeSpellWarmUpFX = instantiatedWarmUpSpellFX;
        }

        public override void SuccessfullyCastSpell(PlayerManager player)
        {
            base.SuccessfullyCastSpell(player);

            // 1. 销毁法术残留的所有预热特效
            if (player.IsOwner)
                player.playerCombatManager.DestroyAllCurrentActionFX();

            // 生成投射物
            SpellInstantiationLocation spellInstantiationLocation;
            GameObject instantiatedReleasedSpellFX = Instantiate(spellCastReleaseFX);

            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                // 2. 在正确位置生成预热特效（咒术直接使用手部，而法杖则使用杖身上的特定点）
                spellInstantiationLocation = player.playerEquipmentManager.rightWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
            }
            else
            {
                // 2. 在正确位置生成预热特效（咒术直接使用手部，而法杖则使用杖身上的特定点）
                spellInstantiationLocation = player.playerEquipmentManager.leftWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
            }


            instantiatedReleasedSpellFX.transform.parent = spellInstantiationLocation.transform;
            instantiatedReleasedSpellFX.transform.localPosition = Vector3.zero;
            instantiatedReleasedSpellFX.transform.localRotation = Quaternion.identity;
            instantiatedReleasedSpellFX.transform.parent = null;

            // APPLY DAMAGE TO THE PROJECTILES DAMAGE COLLIDER
            FireBallManager fireBallManager = instantiatedReleasedSpellFX.GetComponent<FireBallManager>();
            fireBallManager.InitializeFireBall(player);


            // GET ANY COLLIDERS FROM THE CASTER
            //Collider[] characterColliders = player.GetComponentsInChildren<Collider>();
            //Collider characterCollisionCollider = player.GetComponent<Collider>();

            //  USE THE LIST OF COLLIDERS FROM THE CASTER AND NOW APPLY THE IGNORE PHYSICS WITH THE COLLIDERS FROM THE PROJECTILE
            //  NOTE THIS IS NOT NEEDED FOR "FIREBALL" SPECIFICALLY BECAUSE THE COLLISION IS A "ON TRIGGER ENTER" THAT USES A DAMAGE COLLIDER, AND WE ALREADY CHECK FOR THAT
            //  I SIMPLY WANTED TO SHOWCASE ADDITIONAL WAYS OF DOING THIS, AND THE FUNCTION "Physics.IgnoreCollision" <- VERY HANDY
            /*Physics.IgnoreCollision(characterCollisionCollider, fireBallManager.damageCollider.damageCollider, true);

            foreach (var collider in characterColliders)
            {
                Physics.IgnoreCollision(collider, fireBallManager.damageCollider.damageCollider, true);
            }*/

            // SET THE PROJECTILES VELOCITY AND DIRECTION
            // TODO: MAKE PROJECTILES UP AND DOWN DIRECTION GET CHOOSEN BASED ON WHERE PLAYER IS LOOKING

            if (player.playerNetworkManager.isLockedOn.Value)
            {
                instantiatedReleasedSpellFX.transform.LookAt(player.playerCombatManager.currentTarget.transform.position);
            }
            else
            {
                Vector3 forwardDirection = player.transform.forward;
                instantiatedReleasedSpellFX.transform.forward = forwardDirection;
            }

            Rigidbody spellRigidbody = instantiatedReleasedSpellFX.GetComponent<Rigidbody>();
            Vector3 upwardVelocityVector = instantiatedReleasedSpellFX.transform.up * upwardVelocity;
            Vector3 forwardVelocityVector = instantiatedReleasedSpellFX.transform.forward * forwardVelocity;
            Vector3 totalVelocity = upwardVelocityVector + forwardVelocityVector;
            spellRigidbody.linearVelocity = totalVelocity;
        }

        public override void SuccessfullyChargeSpell(PlayerManager player)
        {
            base.SuccessfullyChargeSpell(player);

            // DESTROY ANY WARM UP FX REMAINING FROM THE SPELL
            if (player.IsOwner)
                player.playerCombatManager.DestroyAllCurrentActionFX();

            // INSTANTIATE THE PROJECTILE
            SpellInstantiationLocation spellInstantiationLocation;
            GameObject instantiatedChargeSpellFX = Instantiate(spellChargeFX);

            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                // INSTANTIATE WARM UP FX ON THE CORRECT PLACE. (INCANTATIONS JUST USE HAND, WHILST STAVES USE A POINT ON THE STAVE ITSELF)
                spellInstantiationLocation = player.playerEquipmentManager.rightWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
            }
            else
            {
                // INSTANTIATE WARM UP FX ON THE CORRECT PLACE. (INCANTATIONS JUST USE HAND, WHILST STAVES USE A POINT ON THE STAVE ITSELF)
                spellInstantiationLocation = player.playerEquipmentManager.leftWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
            }

            // "SAVE" THE CHARGE FX AS A VARIABLE SO IT CAN BE DESTROYED IF THE PLAYER IS KNOCKED OUT OF THE ANIMATION
            player.playerEffectsManager.activeSpellWarmUpFX = instantiatedChargeSpellFX;

            instantiatedChargeSpellFX.transform.parent = spellInstantiationLocation.transform;
            instantiatedChargeSpellFX.transform.localPosition = Vector3.zero;
            instantiatedChargeSpellFX.transform.localRotation = Quaternion.identity;
        }

        public override void SuccessfullyCastSpellFullCharge(PlayerManager player)
        {
            base.SuccessfullyCastSpellFullCharge(player);

            // DESTROY ANY WARM UP FX REMAINING FROM THE SPELL
            if (player.IsOwner)
                player.playerCombatManager.DestroyAllCurrentActionFX();

            // INSTANTIATE THE PROJECTILE
            SpellInstantiationLocation spellInstantiationLocation;
            GameObject instantiatedReleasedSpellFX = Instantiate(spellCastReleaseFXFullCharge);

            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                // INSTANTIATE WARM UP FX ON THE CORRECT PLACE. (INCANTATIONS JUST USE HAND, WHILST STAVES USE A POINT ON THE STAVE ITSELF)
                spellInstantiationLocation = player.playerEquipmentManager.rightWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
            }
            else
            {
                // INSTANTIATE WARM UP FX ON THE CORRECT PLACE. (INCANTATIONS JUST USE HAND, WHILST STAVES USE A POINT ON THE STAVE ITSELF)
                spellInstantiationLocation = player.playerEquipmentManager.leftWeaponManager.GetComponentInChildren<SpellInstantiationLocation>();
            }


            instantiatedReleasedSpellFX.transform.parent = spellInstantiationLocation.transform;
            instantiatedReleasedSpellFX.transform.localPosition = Vector3.zero;
            instantiatedReleasedSpellFX.transform.localRotation = Quaternion.identity;
            instantiatedReleasedSpellFX.transform.parent = null;

            // 为投射物的伤害碰撞体设置伤害值
            FireBallManager fireBallManager = instantiatedReleasedSpellFX.GetComponent<FireBallManager>();
            fireBallManager.isFullyCharged = true;
            fireBallManager.InitializeFireBall(player);


            // GET ANY COLLIDERS FROM THE CASTER
            //Collider[] characterColliders = player.GetComponentsInChildren<Collider>();
            //Collider characterCollisionCollider = player.GetComponent<Collider>();

            //  USE THE LIST OF COLLIDERS FROM THE CASTER AND NOW APPLY THE IGNORE PHYSICS WITH THE COLLIDERS FROM THE PROJECTILE
            //  NOTE THIS IS NOT NEEDED FOR "FIREBALL" SPECIFICALLY BECAUSE THE COLLISION IS A "ON TRIGGER ENTER" THAT USES A DAMAGE COLLIDER, AND WE ALREADY CHECK FOR THAT
            //  I SIMPLY WANTED TO SHOWCASE ADDITIONAL WAYS OF DOING THIS, AND THE FUNCTION "Physics.IgnoreCollision" <- VERY HANDY
            /*Physics.IgnoreCollision(characterCollisionCollider, fireBallManager.damageCollider.damageCollider, true);

            foreach (var collider in characterColliders)
            {
                Physics.IgnoreCollision(collider, fireBallManager.damageCollider.damageCollider, true);
            }*/

            // SET THE PROJECTILES VELOCITY AND DIRECTION
            // TODO: MAKE PROJECTILES UP AND DOWN DIRECTION GET CHOOSEN BASED ON WHERE PLAYER IS LOOKING

            if (player.playerNetworkManager.isLockedOn.Value)
            {
                instantiatedReleasedSpellFX.transform.LookAt(player.playerCombatManager.currentTarget.transform.position);
            }
            else
            {
                Vector3 forwardDirection = player.transform.forward;
                instantiatedReleasedSpellFX.transform.forward = forwardDirection;
            }

            Rigidbody spellRigidbody = instantiatedReleasedSpellFX.GetComponent<Rigidbody>();
            Vector3 upwardVelocityVector = instantiatedReleasedSpellFX.transform.up * upwardVelocity;
            Vector3 forwardVelocityVector = instantiatedReleasedSpellFX.transform.forward * forwardVelocity;
            Vector3 totalVelocity = upwardVelocityVector + forwardVelocityVector;
            spellRigidbody.linearVelocity = totalVelocity;
        }
    }
}
