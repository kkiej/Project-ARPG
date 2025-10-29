using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace LZ
{
    public class PlayerUIWeaponUpgradeManager : PlayerUIMenu
    {
        [Header("Menus")]
        [SerializeField] GameObject confirmUpgradePopUp;

        [Header("Text Fields")]
        [SerializeField] TextMeshProUGUI currentMaterialsText;
        [SerializeField] TextMeshProUGUI currentCostText;

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
        private UpgradeMaterial upgradeCost;

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
            
            // SELECTS THE FIRST BUTTON AND FACTORS UPGRADE COST
            SelectEquipmentSlot(0);
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
                PlayerUIManager.instance.PlayUnableToContinueSFX();
                return;
            }

            //  2. IF FAIL MATERIAL CHECK PLAY A SFX OR SOME FORM OF FEEDBACK
            if (!PlayerHasUpgradeCost())
            {
                PlayerUIManager.instance.PlayUnableToContinueSFX();
                return;
            }

            //  3. CHECK IF WEAPON IS AT MAX LEVEL
            if (currentSelectedWeapon.upgradeLevel == UpgradeLevel.Ten)
            {
                PlayerUIManager.instance.PlayUnableToContinueSFX();
                //  IF WEAPON IS AT MAX LEVEL SIMPLY SAY "ARMAMENT FULLY UPGRADED" AND PLAY A SFX OR SOME FORM OF FEEDBACK
                //  ALSO LOWER ALPHA OF WEAPON ICON
                return;
            }

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
            confirmUpgradePopUp.SetActive(false);
            PlayerUIManager.instance.localPlayer.playerInventoryManager.RemoveItemFromInventory(upgradeCost);
            SelectLastSelectedEquipmentSlot();
        }

        public void SelectEquipmentSlot(int equipmentSlot)
        {
            currentSelectedEquipmentSlot = (EquipmentType)equipmentSlot;
            Image currentSelectedEquipmentIcon = null;

            if (currentSelectedEquipmentSlot == EquipmentType.RightWeapon01)
            {
                currentSelectedWeapon = PlayerUIManager.instance.localPlayer.playerInventoryManager.weaponsInRightHandSlots[0];
                currentSelectedEquipmentIcon = rightHandSlot01;
            }

            if (currentSelectedEquipmentSlot == EquipmentType.RightWeapon02)
            {
                currentSelectedWeapon = PlayerUIManager.instance.localPlayer.playerInventoryManager.weaponsInRightHandSlots[1];
                currentSelectedEquipmentIcon = rightHandSlot02;
            }

            if (currentSelectedEquipmentSlot == EquipmentType.RightWeapon03)
            {
                currentSelectedWeapon = PlayerUIManager.instance.localPlayer.playerInventoryManager.weaponsInRightHandSlots[2];
                currentSelectedEquipmentIcon = rightHandSlot03;
            }

            if (currentSelectedEquipmentSlot == EquipmentType.LeftWeapon01)
            {
                currentSelectedWeapon = PlayerUIManager.instance.localPlayer.playerInventoryManager.weaponsInLeftHandSlots[0];
                currentSelectedEquipmentIcon = leftHandSlot01;
            }

            if (currentSelectedEquipmentSlot == EquipmentType.LeftWeapon02)
            {
                currentSelectedWeapon = PlayerUIManager.instance.localPlayer.playerInventoryManager.weaponsInLeftHandSlots[1];
                currentSelectedEquipmentIcon = leftHandSlot02;
            }

            if (currentSelectedEquipmentSlot == EquipmentType.LeftWeapon03)
            {
                currentSelectedWeapon = PlayerUIManager.instance.localPlayer.playerInventoryManager.weaponsInLeftHandSlots[2];
                currentSelectedEquipmentIcon = leftHandSlot03;
            }
            
            //  RESET ALL ICONS COLORS
            Color iconColor = currentSelectedEquipmentIcon.color;
            iconColor.a = 1;
            rightHandSlot01.color = iconColor;
            rightHandSlot02.color = iconColor;
            rightHandSlot03.color = iconColor;
            leftHandSlot01.color = iconColor;
            leftHandSlot02.color = iconColor;
            leftHandSlot03.color = iconColor;

            bool hasCost = PlayerHasUpgradeCost();
            
            //  SET CURRENT SELECTED ICON COLOR DEPENDING ON IF HAS COST
            if (hasCost)
            {
                iconColor.a = 1;
                currentMaterialsText.color = Color.white;
                currentCostText.color = Color.white;
            }
            else
            {
                iconColor.a = 0.2f;
                currentMaterialsText.color = Color.red;
                currentCostText.color = Color.red;
            }

            currentSelectedEquipmentIcon.color = iconColor;

            if (currentSelectedWeapon.upgradeLevel == UpgradeLevel.Ten)
            {
                currentMaterialsText.text = "Current Materials: N/A";
                currentCostText.color = Color.darkGreen;
                currentCostText.text = "WEAPON FULLY UPGRADED";
            }
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

        private bool PlayerHasUpgradeCost()
        {
            upgradeCost = DetermineUpgradeCostOfWeapon(currentSelectedWeapon);
            
            // DO NOT ALLOW THE UPGRADE COST OF "UNARMED" TO BE CALCULATED
            if (currentSelectedWeapon.itemID == WorldItemDatabase.Instance.unarmedWeapon.itemID)
                upgradeCost = null;
            
            // IF FOR SOME REASON WE CANNOT DETERMINE AN UPGRADE COST, ABORT
            if (upgradeCost == null)
            {
                currentMaterialsText.text = "Current Materials: N/A";
                currentCostText.text = "Materials Required: N/A";
                return false;
            }
            
            bool playerHasMaterial = false;
            bool playerHasMaterialAmount = false;
            
            // SEARCH THE PLAYERS INVENTORY FOR UPGRADE STONES
            for (int i = 0; i < PlayerUIManager.instance.localPlayer.playerInventoryManager.itemsInInventory.Count; i++)
            {
                if (PlayerUIManager.instance.localPlayer.playerInventoryManager.itemsInInventory[i] is UpgradeMaterial)
                {
                    UpgradeMaterial playerMaterial = PlayerUIManager.instance.localPlayer.playerInventoryManager.itemsInInventory[i] as UpgradeMaterial;
                    
                    //  IF WE FIND A STONE MATCHING THE TYPE, AND WITH THE AMOUNT NEEDED PASS THE CHECK
                    if (playerMaterial.upgradeStone != upgradeCost.upgradeStone)
                        continue;
                    
                    playerHasMaterial = true;
                    currentMaterialsText.text = "Current Materials: " + playerMaterial.itemName + "  X" + playerMaterial.currentItemAmount;

                    if (playerMaterial.currentItemAmount >= upgradeCost.currentItemAmount)
                    {
                        playerHasMaterialAmount = true;
                        break;
                    }
                }
            }
            
            if (!playerHasMaterial)
                currentMaterialsText.text = "Current Materials: " + upgradeCost.itemName + "  X0";

            if (playerHasMaterial && playerHasMaterialAmount)
                return true;

            return false;
        }

        private UpgradeMaterial DetermineUpgradeCostOfWeapon(WeaponItem weapon)
        {
            currentCostText.text = "Materials Required: N/A";
            
            UpgradeMaterial upgradeCost = null;
            
            // OPTION 1, DIRECTLY REFERENCE AN ITEM FROM THE WORLD ITEM DB, AND USE ITS NAME
            // OPTION 2, MANUALLY INSERT ITEM NAME DEPENDING ON UPGRADE LEVEL

            switch (weapon.upgradeLevel)
            {
                case UpgradeLevel.Zero:
                    upgradeCost = Instantiate(WorldItemDatabase.Instance.smallUpgradeStone);
                    upgradeCost.currentItemAmount = 1;
                    break;
                case UpgradeLevel.One:
                    upgradeCost = Instantiate(WorldItemDatabase.Instance.smallUpgradeStone);
                    upgradeCost.currentItemAmount = 2;
                    break;
                case UpgradeLevel.Two:
                    upgradeCost = Instantiate(WorldItemDatabase.Instance.smallUpgradeStone);
                    upgradeCost.currentItemAmount = 4;
                    break;
                case UpgradeLevel.Three:
                    upgradeCost = Instantiate(WorldItemDatabase.Instance.mediumUpgradeStone);
                    upgradeCost.currentItemAmount = 1;
                    break;
                case UpgradeLevel.Four:
                    upgradeCost = Instantiate(WorldItemDatabase.Instance.mediumUpgradeStone);
                    upgradeCost.currentItemAmount = 2;
                    break;
                case UpgradeLevel.Five:
                    upgradeCost = Instantiate(WorldItemDatabase.Instance.mediumUpgradeStone);
                    upgradeCost.currentItemAmount = 4;
                    break;
                case UpgradeLevel.Six:
                    upgradeCost = Instantiate(WorldItemDatabase.Instance.largeUpgradeStone);
                    upgradeCost.currentItemAmount = 1;
                    break;
                case UpgradeLevel.Seven:
                    upgradeCost = Instantiate(WorldItemDatabase.Instance.largeUpgradeStone);
                    upgradeCost.currentItemAmount = 2;
                    break;
                case UpgradeLevel.Eight:
                    upgradeCost = Instantiate(WorldItemDatabase.Instance.largeUpgradeStone);
                    upgradeCost.currentItemAmount = 4;
                    break;
                case UpgradeLevel.Nine:
                    upgradeCost = Instantiate(WorldItemDatabase.Instance.largeUpgradeStone);
                    upgradeCost.currentItemAmount = 6;
                    break;
                case UpgradeLevel.Ten:
                    upgradeCost = Instantiate(WorldItemDatabase.Instance.largeUpgradeStone);
                    upgradeCost.currentItemAmount = 8;
                    break;
                default:
                    break;
            }

            switch (upgradeCost.upgradeStone)
            {
                case UpgradeStone.small:
                    upgradeCost.itemName = "Small Upgrade Stone";
                    break;
                case UpgradeStone.medium:
                    upgradeCost.itemName = "Medium Upgrade Stone";
                    break;
                case UpgradeStone.large:
                    upgradeCost.itemName = "Large Upgrade Stone";
                    break;
                default:
                    break;
            }

            currentCostText.text = "Materials Required: " + upgradeCost.itemName + "  X" + upgradeCost.currentItemAmount;

            return upgradeCost;
        }
    }
}
