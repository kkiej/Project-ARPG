using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

namespace LZ
{
    public class PlayerNetworkManager : CharacterNetworkManager
    {
        PlayerManager player;

        public NetworkVariable<FixedString64Bytes> characterName = new NetworkVariable<FixedString64Bytes>("Character", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [Header("Flasks")]
        public NetworkVariable<int> remainingHealthFlasks = new NetworkVariable<int>(3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> remainingFocusPointsFlasks = new NetworkVariable<int>(3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> isChugging = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [Header("Actions")]
        public NetworkVariable<bool> isUsingRightHand = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> isUsingLeftHand = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [Header("Equipment")]
        public NetworkVariable<int> currentWeaponBeingUsed = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> currentRightHandWeaponID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> currentLeftHandWeaponID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> currentSpellID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> currentQuickSlotItemID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [Header("Two Handing")]
        public NetworkVariable<int> currentWeaponBeingTwoHanded = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> isTwoHandingWeapon = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> isTwoHandingRightWeapon = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> isTwoHandingLeftWeapon = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [Header("Spells")]
        public NetworkVariable<bool> isChargingRightSpell = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> isChargingLeftSpell = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [Header("Armor")]
        public NetworkVariable<bool> isMale = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> headEquipmentID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> bodyEquipmentID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> legEquipmentID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> handEquipmentID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [Header("Projectiles")]
        public NetworkVariable<int> mainProjectileID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> secondaryProjectileID = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> hasArrowNotched = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);        //  THIS LETS US KNOW IF WE ALREADY HAVE A PROJECTILE LOADED
        public NetworkVariable<bool> isHoldingArrow = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);        //  THIS LETS US KNOW IF WE ARE HOLDING THAT PROJECTILE SO IT DOES NOT RELEASE
        public NetworkVariable<bool> isAiming = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);             //  THIS LETS US KNOW IF WE ARE "ZOOMED" IN AND USING OUR AIMING CAMERA

        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
        }

        public void SetCharacterActionHand(bool rightHandedAction)
        {
            if (rightHandedAction)
            {
                isUsingLeftHand.Value = false;
                isUsingRightHand.Value = true;
            }
            else
            {
                isUsingRightHand.Value = false;
                isUsingLeftHand.Value = true;
            }
        }

        public void SetNewMaxHealthValue(int oldVitality, int newVitality)
        {
            maxHealth.Value = player.playerStatsManager.CalculateHealthBasedOnVitalityLevel(newVitality);
            PlayerUIManager.instance.playerUIHudManager.SetMaxHealthValue(maxHealth.Value);
            currentHealth.Value = maxHealth.Value;
        }
        
        public void SetNewMaxStaminaValue(int oldEndurance, int newEndurance)
        {
            maxStamina.Value = player.playerStatsManager.CalculateStaminaBasedOnEnduranceLevel(newEndurance);
            PlayerUIManager.instance.playerUIHudManager.SetMaxStaminaValue(maxStamina.Value);
            currentStamina.Value = maxStamina.Value;
        }

        public void SetNewMaxFocusPointsValue(int oldMind, int newMind)
        {
            maxFocusPoints.Value = player.playerStatsManager.CalculateFocusPointsBasedOnMindLevel(newMind);
            PlayerUIManager.instance.playerUIHudManager.SetMaxFocusPointValue(maxFocusPoints.Value);
            currentFocusPoints.Value = maxFocusPoints.Value;
        }

        public void OnCurrentRightHandWeaponIDChange(int oldID, int newID)
        {
            if (!player.IsOwner)
            {
                WeaponItem newWeapon = Instantiate(WorldItemDatabase.Instance.GetWeaponByID(newID));
                player.playerInventoryManager.currentRightHandWeapon = newWeapon;
            }

            player.playerEquipmentManager.LoadRightWeapon();

            if (player.IsOwner)
            {
                PlayerUIManager.instance.playerUIHudManager.SetRightWeaponQuickSlotIcon(newID);

                if (player.playerInventoryManager.currentRightHandWeapon.weaponClass == WeaponClass.Bow)
                {
                    PlayerUIManager.instance.playerUIHudManager.ToggleProjectileQuickSlotsVisibility(true);
                }
                else
                {
                    PlayerUIManager.instance.playerUIHudManager.ToggleProjectileQuickSlotsVisibility(false);
                }
            }
        }
        
        public void OnCurrentLeftHandWeaponIDChange(int oldID, int newID)
        {
            if (!player.IsOwner)
            {
                WeaponItem newWeapon = Instantiate(WorldItemDatabase.Instance.GetWeaponByID(newID));
                player.playerInventoryManager.currentLeftHandWeapon = newWeapon;
            }

            player.playerEquipmentManager.LoadLeftWeapon();
            
            if (player.IsOwner)
            {
                PlayerUIManager.instance.playerUIHudManager.SetLeftWeaponQuickSlotIcon(newID);

                if (player.playerInventoryManager.currentLeftHandWeapon.weaponClass == WeaponClass.Bow)
                {
                    PlayerUIManager.instance.playerUIHudManager.ToggleProjectileQuickSlotsVisibility(true);
                }
                else
                {
                    PlayerUIManager.instance.playerUIHudManager.ToggleProjectileQuickSlotsVisibility(false);
                }
            }
        }

        public void OnCurrentWeaponBeingUsedIDChange(int oldID, int newID)
        {
            WeaponItem newWeapon = Instantiate(WorldItemDatabase.Instance.GetWeaponByID(newID));
            player.playerCombatManager.currentWeaponBeingUsed = newWeapon;

            // 如果本端是拥有者则无需执行此代码，因为已在本地完成过处理
            if (player.IsOwner)
                return;

            if (player.playerCombatManager.currentWeaponBeingUsed != null)
                player.playerAnimatorManager.UpdateAnimatorController(player.playerCombatManager.currentWeaponBeingUsed.weaponAnimator);
        }

        public void OnCurrentSpellIDChange(int oldID, int newID)
        {
            SpellItem newSpell = null;

            if (WorldItemDatabase.Instance.GetSpellByID(newID))
                newSpell = Instantiate(WorldItemDatabase.Instance.GetSpellByID(newID));

            if (newSpell != null)
            {
                player.playerInventoryManager.currentSpell = newSpell;

                if (player.IsOwner)
                    PlayerUIManager.instance.playerUIHudManager.SetSpellItemQuickSlotIcon(newID);
            }
        }

        public void OnCurrentQuickSlotItemIDChange(int oldID, int newID)
        {
            QuickSlotItem newQuickSlotItem = null;

            if (WorldItemDatabase.Instance.GetQuickSlotItemByID(newID))
                newQuickSlotItem = Instantiate(WorldItemDatabase.Instance.GetQuickSlotItemByID(newID));

            if (newQuickSlotItem != null)
            {
                player.playerInventoryManager.currentQuickSlotItem = newQuickSlotItem;
            }
            else
            {
                player.playerInventoryManager.currentQuickSlotItem = null;
            }

            if (player.IsOwner)
                PlayerUIManager.instance.playerUIHudManager.SetQuickSlotItemQuickSlotIcon(newID);
        }

        public void OnMainProjectileIDChange(int oldID, int newID)
        {
            RangedProjectileItem newProjectile = null;

            if (WorldItemDatabase.Instance.GetProjectileByID(newID))
                newProjectile = Instantiate(WorldItemDatabase.Instance.GetProjectileByID(newID));

            if (newProjectile != null)
                player.playerInventoryManager.mainProjectile = newProjectile;

            if (player.IsOwner)
                PlayerUIManager.instance.playerUIHudManager.SetMainProjectileQuickSlotIcon(player.playerInventoryManager.mainProjectile);
        }

        public void OnSecondaryProjectileIDChange(int oldID, int newID)
        {
            RangedProjectileItem newProjectile = null;

            if (WorldItemDatabase.Instance.GetProjectileByID(newID))
                newProjectile = Instantiate(WorldItemDatabase.Instance.GetProjectileByID(newID));

            if (newProjectile != null)
                player.playerInventoryManager.secondaryProjectile = newProjectile;

            if (player.IsOwner)
                PlayerUIManager.instance.playerUIHudManager.SetSecondaryProjectileQuickSlotIcon(player.playerInventoryManager.secondaryProjectile);
        }

        public void OnIsHoldingArrowChanged(bool oldStatus, bool newStatus)
        {
            player.animator.SetBool("isHoldingArrow", isHoldingArrow.Value);
        }

        public void OnIsAimingChanged(bool oldStatus, bool newStatus)
        {
            if (!isAiming.Value)
            {
                PlayerCamera.instance.cameraObject.transform.localEulerAngles = new Vector3(0, 0, 0);
                PlayerCamera.instance.cameraObject.fieldOfView = 60;
                PlayerCamera.instance.cameraObject.nearClipPlane = 0.3f;
                PlayerCamera.instance.cameraPivotTransform.localPosition = new Vector3(0, PlayerCamera.instance.cameraPivotYPositionOffSet, 0);
                PlayerUIManager.instance.playerUIHudManager.crossHair.SetActive(false);
            }
            else
            {
                PlayerCamera.instance.gameObject.transform.eulerAngles = new Vector3(0, 0, 0);
                PlayerCamera.instance.cameraPivotTransform.localEulerAngles = new Vector3(0, 0, 0);
                PlayerCamera.instance.cameraObject.fieldOfView = 40;
                PlayerCamera.instance.cameraObject.nearClipPlane = 1.3f;
                PlayerCamera.instance.cameraPivotTransform.localPosition = Vector3.zero;
                PlayerUIManager.instance.playerUIHudManager.crossHair.SetActive(true);
            }
        }

        public void OnIsChargingRightSpellChanged(bool oldStatus, bool newStatus)
        {
            player.animator.SetBool("isChargingRightSpell", isChargingRightSpell.Value);
        }

        public void OnIsChargingLeftSpellChanged(bool oldStatus, bool newStatus)
        {
            player.animator.SetBool("isChargingLeftSpell", isChargingLeftSpell.Value);
        }

        public override void OnIsBlockingChanged(bool oldStatus, bool newStatus)
        {
            base.OnIsBlockingChanged(oldStatus, newStatus);

            if (IsOwner)
            {
                player.playerStatsManager.blockingPhysicalAbsorption = player.playerCombatManager.currentWeaponBeingUsed.physicalBaseDamageAbsorption;
                player.playerStatsManager.blockingMagicAbsorption = player.playerCombatManager.currentWeaponBeingUsed.magicBaseDamageAbsorption;
                player.playerStatsManager.blockingFireAbsorption = player.playerCombatManager.currentWeaponBeingUsed.fireBaseDamageAbsorption;
                player.playerStatsManager.blockingLightningAbsorption = player.playerCombatManager.currentWeaponBeingUsed.lightningBaseDamageAbsorption;
                player.playerStatsManager.blockingHolyAbsorption = player.playerCombatManager.currentWeaponBeingUsed.holyBaseDamageAbsorption;
                player.playerStatsManager.blockingStability = player.playerCombatManager.currentWeaponBeingUsed.stability;
            }
        }

        public void OnIsTwoHandingWeaponChanged(bool oldStatus, bool newStatus)
        {
            if (!isTwoHandingWeapon.Value)
            {
                if (IsOwner)
                {
                    isTwoHandingLeftWeapon.Value = false;
                    isTwoHandingRightWeapon.Value = false;
                }

                player.playerEquipmentManager.UnTwoHandWeapon();
                player.playerEffectsManager.RemoveStaticEffect(WorldCharacterEffectsManager.instance.twoHandingEffect.staticEffectID);
            }
            else
            {
                StaticCharacterEffect twoHandEffect = Instantiate(WorldCharacterEffectsManager.instance.twoHandingEffect);
                player.playerEffectsManager.AddStaticEffect(twoHandEffect);
            }

            player.animator.SetBool("isTwoHandingWeapon", isTwoHandingWeapon.Value);
        }

        public void OnIsTwoHandingRightWeaponChanged(bool oldStatus, bool newStatus)
        {
            if (!isTwoHandingRightWeapon.Value)
                return;

            if (IsOwner)
            {
                currentWeaponBeingTwoHanded.Value = currentRightHandWeaponID.Value;
                isTwoHandingWeapon.Value = true;
            }

            player.playerInventoryManager.currentTwoHandWeapon = player.playerInventoryManager.currentRightHandWeapon;
            player.playerEquipmentManager.TwoHandRightWeapon();
        }

        public void OnIsTwoHandingLeftWeaponChanged(bool oldStatus, bool newStatus)
        {
            if (!isTwoHandingLeftWeapon.Value)
                return;

            if (IsOwner)
            {
                currentWeaponBeingTwoHanded.Value = currentLeftHandWeaponID.Value;
                isTwoHandingWeapon.Value = true;
            }

            player.playerInventoryManager.currentTwoHandWeapon = player.playerInventoryManager.currentLeftHandWeapon;
            player.playerEquipmentManager.TwoHandLeftWeapon();
        }

        public void OnIsChuggingChanged(bool oldStatus, bool newStatus)
        {
            player.animator.SetBool("isChuggingFlask", isChugging.Value);
        }

        public void OnHeadEquipmentChanged(int oldValue, int newValue)
        {
            // 该逻辑已在拥有端执行，无需重复运行
            if (IsOwner)
                return;

            HeadEquipmentItem equipment = WorldItemDatabase.Instance.GetHeadEquipmentByID(headEquipmentID.Value);

            if (equipment != null)
            {
                player.playerEquipmentManager.LoadHeadEquipment(Instantiate(equipment));
            }
            else
            {
                player.playerEquipmentManager.LoadHeadEquipment(null);
            }
        }

        public void OnBodyEquipmentChanged(int oldValue, int newValue)
        {
            // 该逻辑已在拥有端执行，无需重复运行
            if (IsOwner)
                return;

            BodyEquipmentItem equipment = WorldItemDatabase.Instance.GetBodyEquipmentByID(bodyEquipmentID.Value);

            if (equipment != null)
            {
                player.playerEquipmentManager.LoadBodyEquipment(Instantiate(equipment));
            }
            else
            {
                player.playerEquipmentManager.LoadBodyEquipment(null);
            }
        }

        public void OnLegEquipmentChanged(int oldValue, int newValue)
        {
            // 该逻辑已在拥有端执行，无需重复运行
            if (IsOwner)
                return;

            LegEquipmentItem equipment = WorldItemDatabase.Instance.GetLegEquipmentByID(legEquipmentID.Value);

            if (equipment != null)
            {
                player.playerEquipmentManager.LoadLegEquipment(Instantiate(equipment));
            }
            else
            {
                player.playerEquipmentManager.LoadLegEquipment(null);
            }
        }

        public void OnHandEquipmentChanged(int oldValue, int newValue)
        {
            // 该逻辑已在拥有端执行，无需重复运行
            if (IsOwner)
                return;

            HandEquipmentItem equipment = WorldItemDatabase.Instance.GetHandEquipmentByID(headEquipmentID.Value);

            if (equipment != null)
            {
                player.playerEquipmentManager.LoadHandEquipment(Instantiate(equipment));
            }
            else
            {
                player.playerEquipmentManager.LoadHandEquipment(null);
            }
        }

        public void OnIsMaleChanged(bool oldStatus, bool newStatus)
        {
            player.playerBodyManager.ToggleBodyType(isMale.Value);
        }

        //  ITEM ACTIONS
        [ServerRpc]
        public void NotifyTheServerOfWeaponActionServerRpc(ulong clientID, int actionID, int weaponID)
        {
            if (IsServer)
            {
                NotifyTheServerOfWeaponActionClientRpc(clientID, actionID, weaponID);
            }
        }

        [ClientRpc]
        private void NotifyTheServerOfWeaponActionClientRpc(ulong clientID, int actionID, int weaponID)
        {
            if (clientID != NetworkManager.Singleton.LocalClientId)
            {
                PerformWeaponBasedAction(actionID, weaponID);
            }
        }

        private void PerformWeaponBasedAction(int actionID, int weaponID)
        {
            WeaponItemAction weaponAction = WorldActionManager.instance.GetWeaponItemActionByID(actionID);

            if (weaponAction != null)
            {
                weaponAction.AttemptToPerformAction(player, WorldItemDatabase.Instance.GetWeaponByID(weaponID));
            }
            else
            {
                Debug.LogError("ACTION IS NULL, CANNOT BE PERFORMED");
            }
        }

        [ClientRpc]
        public override void DestroyAllCurrentActionFXClientRpc()
        {
            base.DestroyAllCurrentActionFXClientRpc();

            if (hasArrowNotched.Value)
            {
                // ANIMATE THE BOW
                Animator bowAnimator;

                if (player.playerNetworkManager.isTwoHandingLeftWeapon.Value)
                {
                    bowAnimator = player.playerEquipmentManager.leftHandWeaponModel.GetComponentInChildren<Animator>();
                }
                else
                {
                    bowAnimator = player.playerEquipmentManager.rightHandWeaponModel.GetComponentInChildren<Animator>();
                }

                //  ANIMATE THE BOW
                bowAnimator.SetBool("isDrawn", false);
                bowAnimator.Play("Bow_Fire_01");

                if (player.IsOwner)
                    hasArrowNotched.Value = false;
            }
        }

        //  DRAW PROJECTILE
        [ServerRpc]
        public void NotifyServerOfDrawnProjectileServerRpc(int projectileID)
        {
            if (IsServer)
            {
                NotifyServerOfDrawnProjectileClientRpc(projectileID);
            }
        }

        [ClientRpc]
        private void NotifyServerOfDrawnProjectileClientRpc(int projectileID)
        {
            Animator bowAnimator;

            if (isTwoHandingLeftWeapon.Value)
            {
                bowAnimator = player.playerEquipmentManager.leftHandWeaponModel.GetComponentInChildren<Animator>();
            }
            else
            {
                bowAnimator = player.playerEquipmentManager.rightHandWeaponModel.GetComponentInChildren<Animator>();
            }

            //  ANIMATE THE BOW
            bowAnimator.SetBool("isDrawn", true);
            bowAnimator.Play("Bow_Draw_01");

            //  INSTANTIATE THE ARROW
            GameObject arrow = Instantiate(WorldItemDatabase.Instance.GetProjectileByID(projectileID).drawProjectileModel, player.playerEquipmentManager.leftHandWeaponSlot.transform);
            player.playerEffectsManager.activeDrawnProjectileFX = arrow;

            //  PLAY SFX
            player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(WorldSoundFXManager.instance.notchArrowSFX));
        }

        #region RELEASE PROJECTILE

        [ServerRpc]
        public void NotifyServerOfReleasedProjectileServerRpc(ulong playerClientID, int projectileID, float xPosition, float yPosition, float zPosition, float yCharacterRotation)
        {
            if (IsServer)
            {
                NotifyServerOfReleasedProjectileClientRpc(playerClientID, projectileID, xPosition, yPosition, zPosition, yCharacterRotation);
            }
        }

        [ClientRpc]
        public void NotifyServerOfReleasedProjectileClientRpc(ulong playerClientID, int projectileID, float xPosition, float yPosition, float zPosition, float yCharacterRotation)
        {
            if (playerClientID != NetworkManager.Singleton.LocalClientId)
                PerformReleasedProjectileFromRpc(projectileID, xPosition, yPosition, zPosition, yCharacterRotation);
        }

        private void PerformReleasedProjectileFromRpc(int projectileID, float xPosition, float yPosition, float zPosition, float yCharacterRotation)
        {
            RangedProjectileItem projectileItem = null;

            //  THE PROJECTILE WE ARE FIRING
            if (WorldItemDatabase.Instance.GetProjectileByID(projectileID) != null)
                projectileItem = WorldItemDatabase.Instance.GetProjectileByID(projectileID);

            if (projectileItem == null)
                return;

            Transform projectileInstantiationLocation;
            GameObject projectileGameObject;
            Rigidbody projectileRigidbody;
            RangedProjectileDamageCollider projectileDamageCollider;

            projectileInstantiationLocation = player.playerCombatManager.lockOnTransform;
            projectileGameObject = Instantiate(projectileItem.releaseProjectileModel, projectileInstantiationLocation);
            projectileDamageCollider = projectileGameObject.GetComponent<RangedProjectileDamageCollider>();
            projectileRigidbody = projectileGameObject.GetComponent<Rigidbody>();

            //  (TODO MAKE FORMULA TO SET RANGE PROJECTILE DAMAGE)
            projectileDamageCollider.physicalDamage = 100;
            projectileDamageCollider.characterShootingProjectile = player;

            //  FIRE AN ARROW BASED ON 1 OF 3 VARIATIONS
            // 1. LOCKED ONTO A TARGET

            // 2. AIMING
            if (player.playerNetworkManager.isAiming.Value)
            {
                projectileGameObject.transform.LookAt(new Vector3(xPosition, yPosition, zPosition));
            }
            else
            {
                // 2. LOCKED AND NOT AIMING
                if (player.playerCombatManager.currentTarget != null)
                {
                    Quaternion arrowRotation = Quaternion.LookRotation(player.playerCombatManager.currentTarget.characterCombatManager.lockOnTransform.position
                        - projectileGameObject.transform.position);
                    projectileGameObject.transform.rotation = arrowRotation;
                }
                // 3. UNLOCKED AND NOT AIMING
                else
                {
                    //  TEMPORARY, IN THE FUTURE THE ARROW WILL USE THE CAMERA'S LOOK DIRECTION TO ALIGN ITS UP/DOWN ROTATION VALUE
                    //  HINT IF YOU WANT TO DO THIS ON YOUR OWN LOOK AT THE FORWARD DIRECTION VALUE OF THE CAMERA, AND DIRECT THE ARROW ACCORDINGLY
                    player.transform.rotation = Quaternion.Euler(player.transform.rotation.x, yCharacterRotation, player.transform.rotation.z);
                    Quaternion arrowRotation = Quaternion.LookRotation(player.transform.forward);
                    projectileGameObject.transform.rotation = arrowRotation;
                }
            }

            //  GET ALL CHARACTER COLLIDERS AND IGNORE SELF
            Collider[] characterColliders = player.GetComponentsInChildren<Collider>();
            List<Collider> collidersArrowWillIgnore = new List<Collider>();

            foreach (var item in characterColliders)
                collidersArrowWillIgnore.Add(item);

            foreach (Collider hitBox in collidersArrowWillIgnore)
                Physics.IgnoreCollision(projectileDamageCollider.damageCollider, hitBox, true);

            projectileRigidbody.AddForce(projectileGameObject.transform.forward * projectileItem.forwardVelocity);
            projectileGameObject.transform.parent = null;
        }
		
		#endregion

        #region Hide Weapon

        [ServerRpc]
        public void HideWeaponsServerRpc()
        {
            if (IsServer)
                HideWeaponsClientRpc();
        }

        [ClientRpc]
        private void HideWeaponsClientRpc()
        {
            if (player.playerEquipmentManager.rightHandWeaponModel != null)
                player.playerEquipmentManager.rightHandWeaponModel.SetActive(false);

            if (player.playerEquipmentManager.leftHandWeaponModel != null)
                player.playerEquipmentManager.leftHandWeaponModel.SetActive(false);
        }

        #endregion

        #region Quick Slot Item Action

        [ServerRpc]
        public void NotifyServerOfQuickSlotItemActionServerRpc(ulong clientID, int quickSlotItemID)
        {
            NotifyServerOfQuickSlotItemActionClientRpc(clientID, quickSlotItemID);
        }

        [ClientRpc]
        private void NotifyServerOfQuickSlotItemActionClientRpc(ulong clientID, int quickSlotItemID)
        {
            if (clientID != NetworkManager.Singleton.LocalClientId)
            {
                QuickSlotItem item = WorldItemDatabase.Instance.GetQuickSlotItemByID(quickSlotItemID);
                item.AttemptToUseItem(player);
            }
        }

        #endregion
    }
}