using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace LZ
{
    public class PlayerUIWeaponUpgradeManager : PlayerUIMenu
    {
        [Header("Menus")]
        [SerializeField] GameObject confirmUpgradePopUp;

        [Header("Confirm Upgrade Buttons")]
        [SerializeField] Button confirmUpgradeButton;

        [Header("Weapon Slots")]
        [SerializeField] Image rightHandSlot01;
        private Button rightHandSlot01Button;
        [SerializeField] Image rightHandSlot02;
        private Button rightHandSlot02Button;
        [SerializeField] Image rightHandSlot03;
        private Button rightHandSlot03Button;
        [SerializeField] Image leftHandSlot01;
        private Button leftHandSlot01Button;
        [SerializeField] Image leftHandSlot02;
        private Button leftHandSlot02Button;
        [SerializeField] Image leftHandSlot03;
        private Button leftHandSlot03Button;

        [Header("Selected Slot")]
        public EquipmentType currentSelectedEquipmentSlot;
        [SerializeField] private WeaponItem currentSelectedWeapon;

        [Header("TEMPORARY DELETE LATER")]
        [SerializeField] bool openMenu = false;

        private void Awake()
        {
            rightHandSlot01Button = rightHandSlot01.GetComponentInParent<Button>(true);
            rightHandSlot02Button = rightHandSlot02.GetComponentInParent<Button>(true);
            rightHandSlot03Button = rightHandSlot03.GetComponentInParent<Button>(true);

            leftHandSlot01Button = leftHandSlot01.GetComponentInParent<Button>(true);
            leftHandSlot02Button = leftHandSlot02.GetComponentInParent<Button>(true);
            leftHandSlot03Button = leftHandSlot03.GetComponentInParent<Button>(true);
        }

        private void Update()
        {
            if (openMenu)
            {
                openMenu = false;
                OpenMenu();
            }
        }

        public override void OpenMenu()
        {
            base.OpenMenu();

            confirmUpgradePopUp.SetActive(false);
            ToggleEquipmentButtons(true);
            RefreshEquipmentSlotIcons();
        }

        //  USE TO DISABLE/ENABLE WEAPON BUTTONS WHEN CONFIRMING UPGRADE (SO YOU CANT SELECT ANYTHING OUTSIDE OF CONFIRM/CANCEL)
        private void ToggleEquipmentButtons(bool isEnabled)
        {
            rightHandSlot01Button.enabled = isEnabled;
            rightHandSlot02Button.enabled = isEnabled;
            rightHandSlot03Button.enabled = isEnabled;

            leftHandSlot01Button.enabled = isEnabled;
            leftHandSlot02Button.enabled = isEnabled;
            leftHandSlot03Button.enabled = isEnabled;
        }

        private void RefreshEquipmentSlotIcons()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  RIGHT WEAPON 01
            WeaponItem rightHandWeapon01 = player.playerInventoryManager.weaponsInRightHandSlots[0];

            if (rightHandWeapon01.itemIcon != null)
            {
                rightHandSlot01.enabled = true;
                rightHandSlot01.sprite = rightHandWeapon01.itemIcon;
            }
            else
            {
                rightHandSlot01.enabled = false;
            }

            //  RIGHT WEAPON 02
            WeaponItem rightHandWeapon02 = player.playerInventoryManager.weaponsInRightHandSlots[1];

            if (rightHandWeapon02.itemIcon != null)
            {
                rightHandSlot02.enabled = true;
                rightHandSlot02.sprite = rightHandWeapon02.itemIcon;
            }
            else
            {
                rightHandSlot02.enabled = false;
            }

            //  RIGHT WEAPON 03
            WeaponItem rightHandWeapon03 = player.playerInventoryManager.weaponsInRightHandSlots[2];

            if (rightHandWeapon03.itemIcon != null)
            {
                rightHandSlot03.enabled = true;
                rightHandSlot03.sprite = rightHandWeapon03.itemIcon;
            }
            else
            {
                rightHandSlot03.enabled = false;
            }

            //  LEFT WEAPON 01
            WeaponItem leftHandWeapon01 = player.playerInventoryManager.weaponsInLeftHandSlots[0];

            if (leftHandWeapon01.itemIcon != null)
            {
                leftHandSlot01.enabled = true;
                leftHandSlot01.sprite = leftHandWeapon01.itemIcon;
            }
            else
            {
                leftHandSlot01.enabled = false;
            }

            //  LEFT WEAPON 02
            WeaponItem leftHandWeapon02 = player.playerInventoryManager.weaponsInLeftHandSlots[1];

            if (leftHandWeapon02.itemIcon != null)
            {
                leftHandSlot02.enabled = true;
                leftHandSlot02.sprite = leftHandWeapon02.itemIcon;
            }
            else
            {
                leftHandSlot02.enabled = false;
            }

            //  LEFT RIGHT WEAPON 03
            WeaponItem leftHandWeapon03 = player.playerInventoryManager.weaponsInLeftHandSlots[2];

            if (leftHandWeapon03.itemIcon != null)
            {
                leftHandSlot03.enabled = true;
                leftHandSlot03.sprite = leftHandWeapon03.itemIcon;
            }
            else
            {
                leftHandSlot03.enabled = false;
            }
        }

        public void AttemptToUpgradeWeapon()
        {
            //  0. CHECK FOR UNARMED/NO WEAPON
            if (currentSelectedWeapon.itemID == WorldItemDatabase.Instance.unarmedWeapon.itemID)
            {
                //  PLAY SOME SFX OR FEEDBACK THAT THERES NO WEAPON SELECTED FOR UPGRADE
                return;
            }

            //  1. CHECK FOR MATERIALS
            //  IF MATERIALS AMOUNT IS LESS THAN REQUIRED, CHANGE TEXT COLOR TO RED AND FADE OUT WEAPON ICONS ALPHA

            //  2. IF FAIL MATERIAL CHECK PLAY A SFX OR SOME FORM OF FEEDBACK


            //  3. CHECK IF WEAPON IS AT MAX LEVEL
            //  IF WEAPON IS AT MAX LEVEL SIMPLY SAY "ARMAMENT FULLY UPGRADED" AND PLAY A SFX OR SOME FORM OF FEEDBACK
            //  ALSO LOWER ALPHA OF WEAPON ICON

            //  4. IF PASS MATERIAL CHECK OPEN CONFIRMATION WINDOW AND GO TO "CONFIRM BUTTON"
            ToggleEquipmentButtons(false);
            confirmUpgradePopUp.SetActive(true);
            confirmUpgradeButton.Select();
        }

        public void UpgradeWeapon()
        {
            //  TAKE SELECTED WEAPON AND +1 TO UPGRADE LEVEL
            int currentUpgradeLevel = (int)currentSelectedWeapon.upgradeLevel;
            currentUpgradeLevel += 1;
            currentSelectedWeapon.upgradeLevel = (UpgradeLevel)currentUpgradeLevel;
            ToggleEquipmentButtons(true);
            SelectLastSelectedEquipmentSlot();
        }

        public void SelectEquipmentSlot(int equipmentSlot)
        {
            currentSelectedEquipmentSlot = (EquipmentType)equipmentSlot;

            if (currentSelectedEquipmentSlot == EquipmentType.RightWeapon01)
                currentSelectedWeapon = PlayerUIManager.instance.localPlayer.playerInventoryManager.weaponsInRightHandSlots[0];

            if (currentSelectedEquipmentSlot == EquipmentType.RightWeapon02)
                currentSelectedWeapon = PlayerUIManager.instance.localPlayer.playerInventoryManager.weaponsInRightHandSlots[1];

            if (currentSelectedEquipmentSlot == EquipmentType.RightWeapon03)
                currentSelectedWeapon = PlayerUIManager.instance.localPlayer.playerInventoryManager.weaponsInRightHandSlots[2];

            if (currentSelectedEquipmentSlot == EquipmentType.LeftWeapon01)
                currentSelectedWeapon = PlayerUIManager.instance.localPlayer.playerInventoryManager.weaponsInLeftHandSlots[0];

            if (currentSelectedEquipmentSlot == EquipmentType.LeftWeapon02)
                currentSelectedWeapon = PlayerUIManager.instance.localPlayer.playerInventoryManager.weaponsInLeftHandSlots[1];

            if (currentSelectedEquipmentSlot == EquipmentType.LeftWeapon03)
                currentSelectedWeapon = PlayerUIManager.instance.localPlayer.playerInventoryManager.weaponsInLeftHandSlots[2];
        }

        public void SelectLastSelectedEquipmentSlot()
        {
            Button lastSelectedButton = null;

            ToggleEquipmentButtons(true);

            switch (currentSelectedEquipmentSlot)
            {
                case EquipmentType.RightWeapon01:
                    lastSelectedButton = rightHandSlot01Button;
                    break;
                case EquipmentType.RightWeapon02:
                    lastSelectedButton = rightHandSlot02Button;
                    break;
                case EquipmentType.RightWeapon03:
                    lastSelectedButton = rightHandSlot03Button;
                    break;
                case EquipmentType.LeftWeapon01:
                    lastSelectedButton = leftHandSlot01Button;
                    break;
                case EquipmentType.LeftWeapon02:
                    lastSelectedButton = leftHandSlot02Button;
                    break;
                case EquipmentType.LeftWeapon03:
                    lastSelectedButton = leftHandSlot03Button;
                    break;
            }

            if (lastSelectedButton != null)
            {
                lastSelectedButton.Select();
                lastSelectedButton.OnSelect(null);
            }
        }
    }
}
