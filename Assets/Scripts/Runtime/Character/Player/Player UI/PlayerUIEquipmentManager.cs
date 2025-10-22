using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

namespace LZ
{
    public class PlayerUIEquipmentManager : PlayerUIMenu
    {
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

        [Header("Armor Slots")]
        [SerializeField] Image headEquipmentSlot;
        private Button headEquipmentSlotButton;
        [SerializeField] Image bodyEquipmentSlot;
        private Button bodyEquipmentSlotButton;
        [SerializeField] Image legEquipmentSlot;
        private Button legEquipmentSlotButton;
        [SerializeField] Image handEquipmentSlot;
        private Button handEquipmentSlotButton;

        [Header("Projectile Slots")]
        [SerializeField] Image mainProjectileEquipmentSlot;
        [SerializeField] TextMeshProUGUI mainProjectileCount;
        private Button mainProjectileEquipmentSlotButton;
        [SerializeField] Image secondaryProjectileEquipmentSlot;
        [SerializeField] TextMeshProUGUI secondaryProjectileCount;
        private Button secondaryProjectileEquipmentSlotButton;

        [Header("Quick Slots")]
        [SerializeField] Image quickSlot01EquipmentSlot;
        [SerializeField] TextMeshProUGUI quickSlot01Count;
        private Button quickSlot01Button;
        [SerializeField] Image quickSlot02EquipmentSlot;
        [SerializeField] TextMeshProUGUI quickSlot02Count;
        private Button quickSlot02Button;
        [SerializeField] Image quickSlot03EquipmentSlot;
        [SerializeField] TextMeshProUGUI quickSlot03Count;
        private Button quickSlot03Button;

        //  THIS INVENTORY POPULATES WITH RELATED ITEMS WHEN CHANGING EQUIPMENT
        [Header("Equipment Inventory")]
        public EquipmentType currentSelectedEquipmentSlot;
        [SerializeField] GameObject equipmentInventoryWindow;
        [SerializeField] GameObject equipmentInventorySlotPrefab;
        [SerializeField] Transform equipmentInventoryContentWindow;
        [SerializeField] Item currentSelectedItem;

        private void Awake()
        {
            rightHandSlot01Button = rightHandSlot01.GetComponentInParent<Button>(true);
            rightHandSlot02Button = rightHandSlot02.GetComponentInParent<Button>(true);
            rightHandSlot03Button = rightHandSlot03.GetComponentInParent<Button>(true);

            leftHandSlot01Button = leftHandSlot01.GetComponentInParent<Button>(true);
            leftHandSlot02Button = leftHandSlot02.GetComponentInParent<Button>(true);
            leftHandSlot03Button = leftHandSlot03.GetComponentInParent<Button>(true);

            headEquipmentSlotButton = headEquipmentSlot.GetComponentInParent<Button>(true);
            bodyEquipmentSlotButton = bodyEquipmentSlot.GetComponentInParent<Button>(true);
            handEquipmentSlotButton = handEquipmentSlot.GetComponentInParent<Button>(true);
            legEquipmentSlotButton = legEquipmentSlot.GetComponentInParent<Button>(true);

            mainProjectileEquipmentSlotButton = mainProjectileEquipmentSlot.GetComponentInParent<Button>(true);
            secondaryProjectileEquipmentSlotButton = secondaryProjectileEquipmentSlot.GetComponentInParent<Button>(true);

            quickSlot01Button = quickSlot01EquipmentSlot.GetComponentInParent<Button>(true);
            quickSlot02Button = quickSlot02EquipmentSlot.GetComponentInParent<Button>(true);
            quickSlot03Button = quickSlot03EquipmentSlot.GetComponentInParent<Button>(true);
        }

        public override void OpenMenu()
        {
            base.OpenMenu();

            ToggleEquipmentButtons(true);
            equipmentInventoryWindow.SetActive(false);
            ClearEquipmentInventory();
            RefreshEquipmentSlotIcons();
        }

        public void RefreshMenu()
        {
            ClearEquipmentInventory();
            RefreshEquipmentSlotIcons();
        }

        private void ToggleEquipmentButtons(bool isEnabled)
        {
            rightHandSlot01Button.enabled = isEnabled;
            rightHandSlot02Button.enabled = isEnabled;
            rightHandSlot03Button.enabled = isEnabled;

            leftHandSlot01Button.enabled = isEnabled;
            leftHandSlot02Button.enabled = isEnabled;
            leftHandSlot03Button.enabled = isEnabled;

            headEquipmentSlotButton.enabled = isEnabled;
            bodyEquipmentSlotButton.enabled = isEnabled;
            legEquipmentSlotButton.enabled = isEnabled;
            handEquipmentSlotButton.enabled = isEnabled;

            mainProjectileEquipmentSlotButton.enabled = isEnabled;
            secondaryProjectileEquipmentSlotButton.enabled = isEnabled;

            quickSlot01Button.enabled = isEnabled;
            quickSlot02Button.enabled = isEnabled;
            quickSlot03Button.enabled = isEnabled;
        }

        //  THIS FUNCTION SIMPLY RETURNS YOU TO THE LAST SELECTED BUTTON WHEN YOU ARE FINISHED EQUIPPING A NEW ITEM
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
                case EquipmentType.Head:
                    lastSelectedButton = headEquipmentSlotButton;
                    break;
                case EquipmentType.Body:
                    lastSelectedButton = bodyEquipmentSlotButton;
                    break;
                case EquipmentType.Legs:
                    lastSelectedButton = legEquipmentSlotButton;
                    break;
                case EquipmentType.Hands:
                    lastSelectedButton = handEquipmentSlotButton;
                    break;
                case EquipmentType.MainProjectile:
                    lastSelectedButton = mainProjectileEquipmentSlotButton;
                    break;
                case EquipmentType.SecondaryProjectile:
                    lastSelectedButton = secondaryProjectileEquipmentSlotButton;
                    break;
                case EquipmentType.QuickSlot01:
                    lastSelectedButton = quickSlot01Button;
                    break;
                case EquipmentType.QuickSlot02:
                    lastSelectedButton = quickSlot02Button;
                    break;
                case EquipmentType.QuickSlot03:
                    lastSelectedButton = quickSlot03Button;
                    break;
                default:
                    break;
            }

            if (lastSelectedButton != null)
            {
                lastSelectedButton.Select();
                lastSelectedButton.OnSelect(null);
            }

            equipmentInventoryWindow.SetActive(false);
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

            //  HEAD EQUIPMENT
            HeadEquipmentItem headEquipment = player.playerInventoryManager.headEquipment;

            if (headEquipment != null)
            {
                headEquipmentSlot.enabled = true;
                headEquipmentSlot.sprite = headEquipment.itemIcon;
            }
            else
            {
                headEquipmentSlot.enabled = false;
            }

            //  BODY EQUIPMENT
            BodyEquipmentItem bodyEquipment = player.playerInventoryManager.bodyEquipment;

            if (bodyEquipment != null)
            {
                bodyEquipmentSlot.enabled = true;
                bodyEquipmentSlot.sprite = bodyEquipment.itemIcon;
            }
            else
            {
                bodyEquipmentSlot.enabled = false;
            }

            //  LEG EQUIPMENT
            LegEquipmentItem legEquipment = player.playerInventoryManager.legEquipment;

            if (legEquipment != null)
            {
                legEquipmentSlot.enabled = true;
                legEquipmentSlot.sprite = legEquipment.itemIcon;
            }
            else
            {
                legEquipmentSlot.enabled = false;
            }

            //  HAND EQUIPMENT
            HandEquipmentItem handEquipment = player.playerInventoryManager.handEquipment;

            if (handEquipment != null)
            {
                handEquipmentSlot.enabled = true;
                handEquipmentSlot.sprite = handEquipment.itemIcon;
            }
            else
            {
                handEquipmentSlot.enabled = false;
            }

            //  PROJECTILE EQUIPMENT
            RangedProjectileItem mainProjectileEquipment = player.playerInventoryManager.mainProjectile;

            if (mainProjectileEquipment != null)
            {
                mainProjectileEquipmentSlot.enabled = true;
                mainProjectileEquipmentSlot.sprite = mainProjectileEquipment.itemIcon;
                mainProjectileCount.enabled = true;
                mainProjectileCount.text = mainProjectileEquipment.currentAmmoAmount.ToString();
            }
            else
            {
                mainProjectileEquipmentSlot.enabled = false;
                mainProjectileCount.enabled = false;
            }

            RangedProjectileItem secondaryProjectileEquipment = player.playerInventoryManager.secondaryProjectile;

            if (secondaryProjectileEquipment != null)
            {
                secondaryProjectileEquipmentSlot.enabled = true;
                secondaryProjectileEquipmentSlot.sprite = secondaryProjectileEquipment.itemIcon;
                secondaryProjectileCount.enabled = true;
                secondaryProjectileCount.text = secondaryProjectileEquipment.currentAmmoAmount.ToString();
            }
            else
            {
                secondaryProjectileEquipmentSlot.enabled = false;
                secondaryProjectileCount.enabled = false;
            }

            //  QUICK SLOTS

            QuickSlotItem quickSlotEquipment01 = player.playerInventoryManager.quickSlotItemsInQuickSlots[0];

            if (quickSlotEquipment01 != null)
            {
                quickSlot01EquipmentSlot.enabled = true;
                quickSlot01EquipmentSlot.sprite = quickSlotEquipment01.itemIcon;

                if (quickSlotEquipment01.isConsumable)
                {
                    quickSlot01Count.enabled = true;
                    quickSlot01Count.text = quickSlotEquipment01.GetCurrentAmount(player).ToString();
                }
                else
                {
                    quickSlot01Count.enabled = false;
                }
            }
            else
            {
                quickSlot01EquipmentSlot.enabled = false;
                quickSlot01Count.enabled = false;
            }

            QuickSlotItem quickSlotEquipment02 = player.playerInventoryManager.quickSlotItemsInQuickSlots[1];

            if (quickSlotEquipment02 != null)
            {
                quickSlot02EquipmentSlot.enabled = true;
                quickSlot02EquipmentSlot.sprite = quickSlotEquipment02.itemIcon;

                if (quickSlotEquipment02.isConsumable)
                {
                    quickSlot02Count.enabled = true;
                    quickSlot02Count.text = quickSlotEquipment02.GetCurrentAmount(player).ToString();
                }
                else
                {
                    quickSlot02Count.enabled = false;
                }
            }
            else
            {
                quickSlot02EquipmentSlot.enabled = false;
                quickSlot02Count.enabled = false;
            }

            QuickSlotItem quickSlotEquipment03 = player.playerInventoryManager.quickSlotItemsInQuickSlots[2];

            if (quickSlotEquipment03 != null)
            {
                quickSlot03EquipmentSlot.enabled = true;
                quickSlot03EquipmentSlot.sprite = quickSlotEquipment03.itemIcon;

                if (quickSlotEquipment03.isConsumable)
                {
                    quickSlot03Count.enabled = true;
                    quickSlot03Count.text = quickSlotEquipment03.GetCurrentAmount(player).ToString();
                }
                else
                {
                    quickSlot03Count.enabled = false;
                }
            }
            else
            {
                quickSlot03EquipmentSlot.enabled = false;
                quickSlot03Count.enabled = false;
            }
        }

        private void ClearEquipmentInventory()
        {
            foreach (Transform item in equipmentInventoryContentWindow)
            {
                Destroy(item.gameObject);
            }
        }

        public void LoadEquipmentInventory()
        {
            ToggleEquipmentButtons(false);
            equipmentInventoryWindow.SetActive(true);

            switch (currentSelectedEquipmentSlot)
            {
                case EquipmentType.RightWeapon01:
                    LoadWeaponInventory();
                    break;
                case EquipmentType.RightWeapon02:
                    LoadWeaponInventory();
                    break;
                case EquipmentType.RightWeapon03:
                    LoadWeaponInventory();
                    break;
                case EquipmentType.LeftWeapon01:
                    LoadWeaponInventory();
                    break;
                case EquipmentType.LeftWeapon02:
                    LoadWeaponInventory();
                    break;
                case EquipmentType.LeftWeapon03:
                    LoadWeaponInventory();
                    break;
                case EquipmentType.Head:
                    LoadHeadEquipmentInventory();
                    break;
                case EquipmentType.Body:
                    LoadBodyEquipmentInventory();
                    break;
                case EquipmentType.Legs:
                    LoadLegEquipmentInventory();
                    break;
                case EquipmentType.Hands:
                    LoadHandEquipmentInventory();
                    break;
                case EquipmentType.MainProjectile:
                    LoadProjectileInventory();
                    break;
                case EquipmentType.SecondaryProjectile:
                    LoadProjectileInventory();
                    break;
                case EquipmentType.QuickSlot01:
                    LoadQuickSlotInventory();
                    break;
                case EquipmentType.QuickSlot02:
                    LoadQuickSlotInventory();
                    break;
                case EquipmentType.QuickSlot03:
                    LoadQuickSlotInventory();
                    break;
                default:
                    break;
            }
        }

        private void LoadWeaponInventory()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            List<WeaponItem> weaponsInInventory = new List<WeaponItem>();

            //  SEARCH OUR ENTIRE INVENTORY, AND OUT OF ALL OF THE ITEMS IN OUR INVENTORY IF THE ITEM IS A WEAPON ADD IT TO OUR WEAPONS LIST
            for (int i = 0; i < player.playerInventoryManager.itemsInInventory.Count; i++)
            {
                WeaponItem weapon = player.playerInventoryManager.itemsInInventory[i] as WeaponItem;

                if (weapon != null)
                    weaponsInInventory.Add(weapon);
            }

            if (weaponsInInventory.Count <= 0)
            {
                //  TO DO SEND A PLAYER A MESSAGE THAT HE HAS NONE OF ITEM TYPE IN INVENTORY
                equipmentInventoryWindow.SetActive(false);
                ToggleEquipmentButtons(true);
                RefreshMenu();
                return;
            }

            bool hasSelectedFirstInventorySlot = false;

            for (int i = 0; i < weaponsInInventory.Count; i++)
            {
                GameObject inventorySlotGameObject = Instantiate(equipmentInventorySlotPrefab, equipmentInventoryContentWindow);
                UI_EquipmentInventorySlot equipmentInventorySlot = inventorySlotGameObject.GetComponent<UI_EquipmentInventorySlot>();
                equipmentInventorySlot.AddItem(weaponsInInventory[i]);

                //  THIS WILL SELECT THE FIRST BUTTON IN THE LIST
                if (!hasSelectedFirstInventorySlot)
                {
                    hasSelectedFirstInventorySlot = true;
                    Button inventorySlotButton = inventorySlotGameObject.GetComponent<Button>();
                    inventorySlotButton.Select();
                    inventorySlotButton.OnSelect(null);
                }
            }
        }

        private void LoadHeadEquipmentInventory()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            List<HeadEquipmentItem> headEquipmentInInventory = new List<HeadEquipmentItem>();

            //  SEARCH OUR ENTIRE INVENTORY, AND OUT OF ALL OF THE ITEMS IN OUR INVENTORY IF THE ITEM IS A WEAPON ADD IT TO OUR WEAPONS LIST
            for (int i = 0; i < player.playerInventoryManager.itemsInInventory.Count; i++)
            {
                HeadEquipmentItem equipment = player.playerInventoryManager.itemsInInventory[i] as HeadEquipmentItem;

                if (equipment != null)
                    headEquipmentInInventory.Add(equipment);
            }

            if (headEquipmentInInventory.Count <= 0)
            {
                //  TO DO SEND A PLAYER A MESSAGE THAT HE HAS NONE OF ITEM TYPE IN INVENTORY
                equipmentInventoryWindow.SetActive(false);
                ToggleEquipmentButtons(true);
                RefreshMenu();
                return;
            }

            bool hasSelectedFirstInventorySlot = false;

            for (int i = 0; i < headEquipmentInInventory.Count; i++)
            {
                GameObject inventorySlotGameObject = Instantiate(equipmentInventorySlotPrefab, equipmentInventoryContentWindow);
                UI_EquipmentInventorySlot equipmentInventorySlot = inventorySlotGameObject.GetComponent<UI_EquipmentInventorySlot>();
                equipmentInventorySlot.AddItem(headEquipmentInInventory[i]);

                //  THIS WILL SELECT THE FIRST BUTTON IN THE LIST
                if (!hasSelectedFirstInventorySlot)
                {
                    hasSelectedFirstInventorySlot = true;
                    Button inventorySlotButton = inventorySlotGameObject.GetComponent<Button>();
                    inventorySlotButton.Select();
                    inventorySlotButton.OnSelect(null);
                }
            }
        }

        private void LoadBodyEquipmentInventory()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            List<BodyEquipmentItem> bodyEquipmentInInventory = new List<BodyEquipmentItem>();

            //  SEARCH OUR ENTIRE INVENTORY, AND OUT OF ALL OF THE ITEMS IN OUR INVENTORY IF THE ITEM IS A WEAPON ADD IT TO OUR WEAPONS LIST
            for (int i = 0; i < player.playerInventoryManager.itemsInInventory.Count; i++)
            {
                BodyEquipmentItem equipment = player.playerInventoryManager.itemsInInventory[i] as BodyEquipmentItem;

                if (equipment != null)
                    bodyEquipmentInInventory.Add(equipment);
            }

            if (bodyEquipmentInInventory.Count <= 0)
            {
                //  TO DO SEND A PLAYER A MESSAGE THAT HE HAS NONE OF ITEM TYPE IN INVENTORY
                equipmentInventoryWindow.SetActive(false);
                ToggleEquipmentButtons(true);
                RefreshMenu();
                return;
            }

            bool hasSelectedFirstInventorySlot = false;

            for (int i = 0; i < bodyEquipmentInInventory.Count; i++)
            {
                GameObject inventorySlotGameObject = Instantiate(equipmentInventorySlotPrefab, equipmentInventoryContentWindow);
                UI_EquipmentInventorySlot equipmentInventorySlot = inventorySlotGameObject.GetComponent<UI_EquipmentInventorySlot>();
                equipmentInventorySlot.AddItem(bodyEquipmentInInventory[i]);

                //  THIS WILL SELECT THE FIRST BUTTON IN THE LIST
                if (!hasSelectedFirstInventorySlot)
                {
                    hasSelectedFirstInventorySlot = true;
                    Button inventorySlotButton = inventorySlotGameObject.GetComponent<Button>();
                    inventorySlotButton.Select();
                    inventorySlotButton.OnSelect(null);
                }
            }
        }

        private void LoadLegEquipmentInventory()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            List<LegEquipmentItem> legEquipmentInInventory = new List<LegEquipmentItem>();

            //  SEARCH OUR ENTIRE INVENTORY, AND OUT OF ALL OF THE ITEMS IN OUR INVENTORY IF THE ITEM IS A WEAPON ADD IT TO OUR WEAPONS LIST
            for (int i = 0; i < player.playerInventoryManager.itemsInInventory.Count; i++)
            {
                LegEquipmentItem equipment = player.playerInventoryManager.itemsInInventory[i] as LegEquipmentItem;

                if (equipment != null)
                    legEquipmentInInventory.Add(equipment);
            }

            if (legEquipmentInInventory.Count <= 0)
            {
                //  TO DO SEND A PLAYER A MESSAGE THAT HE HAS NONE OF ITEM TYPE IN INVENTORY
                equipmentInventoryWindow.SetActive(false);
                ToggleEquipmentButtons(true);
                RefreshMenu();
                return;
            }

            bool hasSelectedFirstInventorySlot = false;

            for (int i = 0; i < legEquipmentInInventory.Count; i++)
            {
                GameObject inventorySlotGameObject = Instantiate(equipmentInventorySlotPrefab, equipmentInventoryContentWindow);
                UI_EquipmentInventorySlot equipmentInventorySlot = inventorySlotGameObject.GetComponent<UI_EquipmentInventorySlot>();
                equipmentInventorySlot.AddItem(legEquipmentInInventory[i]);

                //  THIS WILL SELECT THE FIRST BUTTON IN THE LIST
                if (!hasSelectedFirstInventorySlot)
                {
                    hasSelectedFirstInventorySlot = true;
                    Button inventorySlotButton = inventorySlotGameObject.GetComponent<Button>();
                    inventorySlotButton.Select();
                    inventorySlotButton.OnSelect(null);
                }
            }
        }

        private void LoadHandEquipmentInventory()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            List<HandEquipmentItem> handEquipmentInInventory = new List<HandEquipmentItem>();

            //  SEARCH OUR ENTIRE INVENTORY, AND OUT OF ALL OF THE ITEMS IN OUR INVENTORY IF THE ITEM IS A WEAPON ADD IT TO OUR WEAPONS LIST
            for (int i = 0; i < player.playerInventoryManager.itemsInInventory.Count; i++)
            {
                HandEquipmentItem equipment = player.playerInventoryManager.itemsInInventory[i] as HandEquipmentItem;

                if (equipment != null)
                    handEquipmentInInventory.Add(equipment);
            }

            if (handEquipmentInInventory.Count <= 0)
            {
                //  TO DO SEND A PLAYER A MESSAGE THAT HE HAS NONE OF ITEM TYPE IN INVENTORY
                equipmentInventoryWindow.SetActive(false);
                ToggleEquipmentButtons(true);
                RefreshMenu();
                return;
            }

            bool hasSelectedFirstInventorySlot = false;

            for (int i = 0; i < handEquipmentInInventory.Count; i++)
            {
                GameObject inventorySlotGameObject = Instantiate(equipmentInventorySlotPrefab, equipmentInventoryContentWindow);
                UI_EquipmentInventorySlot equipmentInventorySlot = inventorySlotGameObject.GetComponent<UI_EquipmentInventorySlot>();
                equipmentInventorySlot.AddItem(handEquipmentInInventory[i]);

                //  THIS WILL SELECT THE FIRST BUTTON IN THE LIST
                if (!hasSelectedFirstInventorySlot)
                {
                    hasSelectedFirstInventorySlot = true;
                    Button inventorySlotButton = inventorySlotGameObject.GetComponent<Button>();
                    inventorySlotButton.Select();
                    inventorySlotButton.OnSelect(null);
                }
            }
        }

        private void LoadProjectileInventory()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            List<RangedProjectileItem> projectilesInInventory = new List<RangedProjectileItem>();

            //  SEARCH OUR ENTIRE INVENTORY, AND OUT OF ALL OF THE ITEMS IN OUR INVENTORY IF THE ITEM IS A WEAPON ADD IT TO OUR WEAPONS LIST
            for (int i = 0; i < player.playerInventoryManager.itemsInInventory.Count; i++)
            {
                RangedProjectileItem projectile = player.playerInventoryManager.itemsInInventory[i] as RangedProjectileItem;

                if (projectile != null)
                    projectilesInInventory.Add(projectile);
            }

            if (projectilesInInventory.Count <= 0)
            {
                //  TO DO SEND A PLAYER A MESSAGE THAT HE HAS NONE OF ITEM TYPE IN INVENTORY
                equipmentInventoryWindow.SetActive(false);
                ToggleEquipmentButtons(true);
                RefreshMenu();
                return;
            }

            bool hasSelectedFirstInventorySlot = false;

            for (int i = 0; i < projectilesInInventory.Count; i++)
            {
                GameObject inventorySlotGameObject = Instantiate(equipmentInventorySlotPrefab, equipmentInventoryContentWindow);
                UI_EquipmentInventorySlot equipmentInventorySlot = inventorySlotGameObject.GetComponent<UI_EquipmentInventorySlot>();
                equipmentInventorySlot.AddItem(projectilesInInventory[i]);

                //  THIS WILL SELECT THE FIRST BUTTON IN THE LIST
                if (!hasSelectedFirstInventorySlot)
                {
                    hasSelectedFirstInventorySlot = true;
                    Button inventorySlotButton = inventorySlotGameObject.GetComponent<Button>();
                    inventorySlotButton.Select();
                    inventorySlotButton.OnSelect(null);
                }
            }
        }

        private void LoadQuickSlotInventory()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            List<QuickSlotItem> quickSlotItemsInInventory = new List<QuickSlotItem>();

            //  SEARCH OUR ENTIRE INVENTORY, AND OUT OF ALL OF THE ITEMS IN OUR INVENTORY IF THE ITEM IS A WEAPON ADD IT TO OUR WEAPONS LIST
            for (int i = 0; i < player.playerInventoryManager.itemsInInventory.Count; i++)
            {
                QuickSlotItem quickSlotItem = player.playerInventoryManager.itemsInInventory[i] as QuickSlotItem;

                if (quickSlotItem != null)
                    quickSlotItemsInInventory.Add(quickSlotItem);
            }

            if (quickSlotItemsInInventory.Count <= 0)
            {
                //  TO DO SEND A PLAYER A MESSAGE THAT HE HAS NONE OF ITEM TYPE IN INVENTORY
                equipmentInventoryWindow.SetActive(false);
                ToggleEquipmentButtons(true);
                RefreshMenu();
                return;
            }

            bool hasSelectedFirstInventorySlot = false;

            for (int i = 0; i < quickSlotItemsInInventory.Count; i++)
            {
                GameObject inventorySlotGameObject = Instantiate(equipmentInventorySlotPrefab, equipmentInventoryContentWindow);
                UI_EquipmentInventorySlot equipmentInventorySlot = inventorySlotGameObject.GetComponent<UI_EquipmentInventorySlot>();
                equipmentInventorySlot.AddItem(quickSlotItemsInInventory[i]);

                //  THIS WILL SELECT THE FIRST BUTTON IN THE LIST
                if (!hasSelectedFirstInventorySlot)
                {
                    hasSelectedFirstInventorySlot = true;
                    Button inventorySlotButton = inventorySlotGameObject.GetComponent<Button>();
                    inventorySlotButton.Select();
                    inventorySlotButton.OnSelect(null);
                }
            }
        }

        public void SelectEquipmentSlot(int equipmentSlot)
        {
            currentSelectedEquipmentSlot = (EquipmentType)equipmentSlot;
        }

        public void UnEquipSelectedItem()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
            Item unequippedItem;

            switch (currentSelectedEquipmentSlot)
            {
                case EquipmentType.RightWeapon01:

                    unequippedItem = player.playerInventoryManager.weaponsInRightHandSlots[0];

                    if (unequippedItem != null)
                    {
                        player.playerInventoryManager.weaponsInRightHandSlots[0] = Instantiate(WorldItemDatabase.Instance.unarmedWeapon);

                        if (unequippedItem.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                            player.playerInventoryManager.AddItemToInventory(unequippedItem);
                    }

                    if (player.playerInventoryManager.rightHandWeaponIndex == 0)
                        player.playerNetworkManager.currentRightHandWeaponID.Value = WorldItemDatabase.Instance.unarmedWeapon.itemID;

                    break;
                case EquipmentType.RightWeapon02:

                    unequippedItem = player.playerInventoryManager.weaponsInRightHandSlots[1];

                    if (unequippedItem != null)
                    {
                        player.playerInventoryManager.weaponsInRightHandSlots[1] = Instantiate(WorldItemDatabase.Instance.unarmedWeapon);

                        if (unequippedItem.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                            player.playerInventoryManager.AddItemToInventory(unequippedItem);
                    }

                    if (player.playerInventoryManager.rightHandWeaponIndex == 1)
                        player.playerNetworkManager.currentRightHandWeaponID.Value = WorldItemDatabase.Instance.unarmedWeapon.itemID;

                    break;
                case EquipmentType.RightWeapon03:

                    unequippedItem = player.playerInventoryManager.weaponsInRightHandSlots[2];

                    if (unequippedItem != null)
                    {
                        player.playerInventoryManager.weaponsInRightHandSlots[2] = Instantiate(WorldItemDatabase.Instance.unarmedWeapon);

                        if (unequippedItem.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                            player.playerInventoryManager.AddItemToInventory(unequippedItem);
                    }

                    if (player.playerInventoryManager.rightHandWeaponIndex == 2)
                        player.playerNetworkManager.currentRightHandWeaponID.Value = WorldItemDatabase.Instance.unarmedWeapon.itemID;

                    break;
                case EquipmentType.LeftWeapon01:

                    unequippedItem = player.playerInventoryManager.weaponsInLeftHandSlots[0];

                    if (unequippedItem != null)
                    {
                        player.playerInventoryManager.weaponsInLeftHandSlots[0] = Instantiate(WorldItemDatabase.Instance.unarmedWeapon);

                        if (unequippedItem.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                            player.playerInventoryManager.AddItemToInventory(unequippedItem);
                    }

                    if (player.playerInventoryManager.leftHandWeaponIndex == 0)
                        player.playerNetworkManager.currentLeftHandWeaponID.Value = WorldItemDatabase.Instance.unarmedWeapon.itemID;

                    break;
                case EquipmentType.LeftWeapon02:

                    unequippedItem = player.playerInventoryManager.weaponsInLeftHandSlots[1];

                    if (unequippedItem != null)
                    {
                        player.playerInventoryManager.weaponsInLeftHandSlots[1] = Instantiate(WorldItemDatabase.Instance.unarmedWeapon);

                        if (unequippedItem.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                            player.playerInventoryManager.AddItemToInventory(unequippedItem);
                    }

                    if (player.playerInventoryManager.leftHandWeaponIndex == 1)
                        player.playerNetworkManager.currentLeftHandWeaponID.Value = WorldItemDatabase.Instance.unarmedWeapon.itemID;

                    break;
                case EquipmentType.LeftWeapon03:

                    unequippedItem = player.playerInventoryManager.weaponsInLeftHandSlots[2];

                    if (unequippedItem != null)
                    {
                        player.playerInventoryManager.weaponsInLeftHandSlots[2] = Instantiate(WorldItemDatabase.Instance.unarmedWeapon);

                        if (unequippedItem.itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                            player.playerInventoryManager.AddItemToInventory(unequippedItem);
                    }

                    if (player.playerInventoryManager.leftHandWeaponIndex == 2)
                        player.playerNetworkManager.currentLeftHandWeaponID.Value = WorldItemDatabase.Instance.unarmedWeapon.itemID;

                    break;
                case EquipmentType.Head:

                    unequippedItem = player.playerInventoryManager.headEquipment;

                    if (unequippedItem != null)
                        player.playerInventoryManager.AddItemToInventory(unequippedItem);

                    player.playerInventoryManager.headEquipment = null;
                    player.playerEquipmentManager.LoadHeadEquipment(player.playerInventoryManager.headEquipment);

                    break;
                case EquipmentType.Body:

                    unequippedItem = player.playerInventoryManager.bodyEquipment;

                    if (unequippedItem != null)
                        player.playerInventoryManager.AddItemToInventory(unequippedItem);

                    player.playerInventoryManager.bodyEquipment = null;
                    player.playerEquipmentManager.LoadBodyEquipment(player.playerInventoryManager.bodyEquipment);

                    break;
                case EquipmentType.Legs:

                    unequippedItem = player.playerInventoryManager.legEquipment;

                    if (unequippedItem != null)
                        player.playerInventoryManager.AddItemToInventory(unequippedItem);

                    player.playerInventoryManager.legEquipment = null;
                    player.playerEquipmentManager.LoadLegEquipment(player.playerInventoryManager.legEquipment);

                    break;
                case EquipmentType.Hands:

                    unequippedItem = player.playerInventoryManager.handEquipment;

                    if (unequippedItem != null)
                        player.playerInventoryManager.AddItemToInventory(unequippedItem);

                    player.playerInventoryManager.handEquipment = null;
                    player.playerEquipmentManager.LoadHandEquipment(player.playerInventoryManager.handEquipment);

                    break;
                case EquipmentType.MainProjectile:

                    unequippedItem = player.playerInventoryManager.mainProjectile;

                    if (unequippedItem != null)
                        player.playerInventoryManager.AddItemToInventory(unequippedItem);

                    player.playerInventoryManager.mainProjectile = null;
                    player.playerEquipmentManager.LoadMainProjectileEquipment(player.playerInventoryManager.mainProjectile);

                    break;
                case EquipmentType.SecondaryProjectile:

                    unequippedItem = player.playerInventoryManager.secondaryProjectile;

                    if (unequippedItem != null)
                        player.playerInventoryManager.AddItemToInventory(unequippedItem);

                    player.playerInventoryManager.secondaryProjectile = null;
                    player.playerEquipmentManager.LoadSecondaryProjectileEquipment(player.playerInventoryManager.secondaryProjectile);

                    break;
                case EquipmentType.QuickSlot01:

                    unequippedItem = player.playerInventoryManager.quickSlotItemsInQuickSlots[0];

                    if (unequippedItem != null)
                        player.playerInventoryManager.AddItemToInventory(unequippedItem);

                    player.playerInventoryManager.quickSlotItemsInQuickSlots[0] = null;

                    if (player.playerInventoryManager.quickSlotItemIndex == 0)
                        player.playerNetworkManager.currentQuickSlotItemID.Value = -1;

                    break;
                case EquipmentType.QuickSlot02:

                    unequippedItem = player.playerInventoryManager.quickSlotItemsInQuickSlots[1];

                    if (unequippedItem != null)
                        player.playerInventoryManager.AddItemToInventory(unequippedItem);

                    player.playerInventoryManager.quickSlotItemsInQuickSlots[1] = null;

                    if (player.playerInventoryManager.quickSlotItemIndex == 1)
                        player.playerNetworkManager.currentQuickSlotItemID.Value = -1;

                    break;
                case EquipmentType.QuickSlot03:

                    unequippedItem = player.playerInventoryManager.quickSlotItemsInQuickSlots[2];

                    if (unequippedItem != null)
                        player.playerInventoryManager.AddItemToInventory(unequippedItem);

                    player.playerInventoryManager.quickSlotItemsInQuickSlots[2] = null;

                    if (player.playerInventoryManager.quickSlotItemIndex == 2)
                        player.playerNetworkManager.currentQuickSlotItemID.Value = -1;

                    break;
                default:
                    break;
            }

            //  REFRESHES MENU (ICONS ECT)
            RefreshMenu();
        }
    }
}