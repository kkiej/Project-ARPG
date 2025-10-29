using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

namespace LZ
{
    public class AICharacterNetworkManager : CharacterNetworkManager
    {
        AICharacterManager aiCharacter;

        [Header("Sleep")]
        public NetworkVariable<bool> isAwake = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<FixedString64Bytes> sleepingAnimation = new NetworkVariable<FixedString64Bytes>("Sleep_01", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<FixedString64Bytes> wakingAnimation = new NetworkVariable<FixedString64Bytes>("Wake_01", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        protected override void Awake()
        {
            base.Awake();

            aiCharacter = GetComponent<AICharacterManager>();
        }

        public override void OnIsDeadChanged(bool oldStatus, bool newStatus)
        {
            base.OnIsDeadChanged(oldStatus, newStatus);

            if (aiCharacter.isDead.Value)
            {
                aiCharacter.aiCharacterInventoryManager.DropItem();
                aiCharacter.aiCharacterCombatManager.AwardRunesOnDeath(PlayerUIManager.instance.localPlayer);
            }
        }

        public override void OnLockOnTargetIDChange(ulong oldID, ulong newID)
        {
            base.OnLockOnTargetIDChange(oldID, newID);

            //  IF YOUR CHARACTER HAS A TARGET, DISABLE THE INTERACTABLE COLLIDER
            if (aiCharacter.aiCharacterCombatManager.currentTarget != null && aiCharacter.aiCharacterSoundFXManager.interactableDialogueObject != null)
                aiCharacter.aiCharacterSoundFXManager.interactableDialogueObject.SetActive(false);

            //  OPTIONALLY RE-ENABLE IT WHEN THE TARGET IS GONE
            if (aiCharacter.aiCharacterCombatManager.currentTarget == null && aiCharacter.aiCharacterSoundFXManager.interactableDialogueObject != null)
                aiCharacter.aiCharacterSoundFXManager.interactableDialogueObject.SetActive(true);
        }
    }
}