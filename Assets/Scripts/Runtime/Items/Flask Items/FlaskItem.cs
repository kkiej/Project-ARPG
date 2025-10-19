using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Items/Consumeables/Flask")]
    public class FlaskItem : QuickSlotItem
    {
        [Header("Flask Type")]
        public bool healthFlask = true;

        [Header("Restoration Value")]
        [SerializeField] int flaskRestoration = 50;

        [Header("Empty Item")]
        public GameObject emptyFlaskItem;
        public string emptyFlaskAnimation;

        public override bool CanIUseThisItem(PlayerManager player)
        {
            if (!player.playerCombatManager.isUsingItem && player.isPerformingAction)
                return false;

            if (player.playerNetworkManager.isAttacking.Value)
                return false;

            return true;
        }

        public override void AttemptToUseItem(PlayerManager player)
        {
            if (!CanIUseThisItem(player))
                return;

            //  HEALTH FLASK CHECK
            if (healthFlask && player.playerNetworkManager.remainingHealthFlasks.Value <= 0)
            {
                if (player.playerCombatManager.isUsingItem)
                    return;

                player.playerCombatManager.isUsingItem = true;

                if (player.IsOwner)
                {
                    player.playerAnimatorManager.PlayTargetActionAnimation(emptyFlaskAnimation, false, false, true, true, false);
                    player.playerNetworkManager.HideWeaponsServerRpc();
                }

                Destroy(player.playerEffectsManager.activeQuickSlotItemFX);
                GameObject emptyFlask = Instantiate(emptyFlaskItem, player.playerEquipmentManager.rightHandWeaponSlot.transform);
                player.playerEffectsManager.activeQuickSlotItemFX = emptyFlask;
                return;
            }

            //  FOCUS POINTS FLASK CHECK
            if (!healthFlask && player.playerNetworkManager.remainingFocusPointsFlasks.Value <= 0)
            {
                if (player.playerCombatManager.isUsingItem)
                    return;

                player.playerCombatManager.isUsingItem = true;

                if (player.IsOwner)
                {
                    player.playerAnimatorManager.PlayTargetActionAnimation(emptyFlaskAnimation, false, false, true, true, false);
                    player.playerNetworkManager.HideWeaponsServerRpc();
                }

                Destroy(player.playerEffectsManager.activeQuickSlotItemFX);
                GameObject emptyFlask = Instantiate(emptyFlaskItem, player.playerEquipmentManager.rightHandWeaponSlot.transform);
                player.playerEffectsManager.activeQuickSlotItemFX = emptyFlask;
                return;
            }

            //  CHECK FOR CHUGGING
            if (player.playerCombatManager.isUsingItem)
            {
                if (player.IsOwner)
                    player.playerNetworkManager.isChugging.Value = true;

                return;
            }

            player.playerCombatManager.isUsingItem = true;

            player.playerEffectsManager.activeQuickSlotItemFX = Instantiate(itemModel, player.playerEquipmentManager.rightHandWeaponSlot.transform);

            if (player.IsOwner)
            {
                player.playerAnimatorManager.PlayTargetActionAnimation(useItemAnimation, false, false, true, true, false);
                player.playerNetworkManager.HideWeaponsServerRpc();
            }
        }

        public override void SuccessfullyUseItem(PlayerManager player)
        {
            base.SuccessfullyUseItem(player);

            if (player.IsOwner)
            {
                if (healthFlask)
                {
                    player.playerNetworkManager.currentHealth.Value += flaskRestoration;
                    player.playerNetworkManager.remainingHealthFlasks.Value -= 1;
                }
                else
                {
                    player.playerNetworkManager.currentFocusPoints.Value += flaskRestoration;
                    player.playerNetworkManager.remainingFocusPointsFlasks.Value -= 1;
                }
            }

            if (healthFlask && player.playerNetworkManager.remainingHealthFlasks.Value <= 0)
            {
                Destroy(player.playerEffectsManager.activeQuickSlotItemFX);
                GameObject emptyFlask = Instantiate(emptyFlaskItem, player.playerEquipmentManager.rightHandWeaponSlot.transform);
                player.playerEffectsManager.activeQuickSlotItemFX = emptyFlask;
            }

            if (!healthFlask && player.playerNetworkManager.remainingFocusPointsFlasks.Value <= 0)
            {
                Destroy(player.playerEffectsManager.activeQuickSlotItemFX);
                GameObject emptyFlask = Instantiate(emptyFlaskItem, player.playerEquipmentManager.rightHandWeaponSlot.transform);
                player.playerEffectsManager.activeQuickSlotItemFX = emptyFlask;
            }

            PlayHealingFX(player);
        }

        private void PlayHealingFX(PlayerManager player)
        {
            Instantiate(WorldCharacterEffectsManager.instance.healingFlaskVFX, player.transform);
            player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.healingFlaskSFX);
        }

        public override int GetCurrentAmount(PlayerManager player)
        {
            int currentAmount = 0;

            if (healthFlask)
                currentAmount = player.playerNetworkManager.remainingHealthFlasks.Value;

            if (!healthFlask)
                currentAmount = player.playerNetworkManager.remainingFocusPointsFlasks.Value;

            return currentAmount;
        }
    }
}
