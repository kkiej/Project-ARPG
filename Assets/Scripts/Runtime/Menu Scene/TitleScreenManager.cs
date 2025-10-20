using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LZ
{
    public class TitleScreenManager : MonoBehaviour
    {
        public static TitleScreenManager instance;

        //  MAIN MENU
        [Header("Main Menu Menus")]
        [SerializeField] GameObject titleScreenMainMenu;
        [SerializeField] GameObject titleScreenLoadMenu;
        [SerializeField] GameObject titleScreenCharacterCreationMenu;

        [Header("Main Menu Buttons")]
        [SerializeField] Button loadMenuReturnButton;
        [SerializeField] Button mainMenuLoadGameButton;
        [SerializeField] Button mainMenuNewGameButton;
        [SerializeField] Button deleteCharacterPopUpConfirmButton;

        [Header("Main Menu Pop Ups")]
        [SerializeField] GameObject noCharacterSlotsPopUp;
        [SerializeField] Button noCharacterSlotsOkayButton;
        [SerializeField] GameObject deleteCharacterSlotPopUp;

        //  CHARACTER CREATION MENU
        [Header("Character Creation Main Panel Buttons")]
        [SerializeField] Button characterNameButton;
        [SerializeField] Button characterClassButton;
        [SerializeField] Button startGameButton;

        [Header("Character Creation Class Panel Buttons")]
        [SerializeField] Button[] characterClassButtons;

        [Header("Character Creation Secondary Panel Menus")]
        [SerializeField] GameObject characterClassMenu;

        [Header("Character Slots")]
        public CharacterSlot currentSelectedSlot = CharacterSlot.NO_SLOT;

        [Header("Classes")]
        public CharacterClass[] startingClasses;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void StartNetworkAsHost()
        {
            NetworkManager.Singleton.StartHost();
        }

        public void AttemptToCreateNewCharacter()
        {
            if (WorldSaveGameManager.instance.HasFreeCharacterSlot())
            {
                OpenCharacterCreationMenu();
            }
            else
            {
                //  若所有槽位均已占用，则向玩家发送提示信息
                DisplayNoFreeCharacterSlotsPopUp();
            }
        }

        public void StartNewGame()
        {
            WorldSaveGameManager.instance.AttemptToCreateNewGame();
        }

        public void OpenLoadGameMenu()
        {
            // 关闭主菜单
            titleScreenMainMenu.SetActive(false);
            
            // 开启加载菜单
            titleScreenLoadMenu.SetActive(true);
            
            // 选择返回按钮
            loadMenuReturnButton.Select();
        }

        public void CloseLoadGameMenu()
        {
            // 关闭加载菜单
            titleScreenLoadMenu.SetActive(false);
            
            // 开启主菜单
            titleScreenMainMenu.SetActive(true);
            
            // 选择加载按钮
            mainMenuLoadGameButton.Select();
        }

        public void OpenCharacterCreationMenu()
        {
            titleScreenCharacterCreationMenu.SetActive(true);
        }

        public void CloseCharacterCreationMenu()
        {
            titleScreenCharacterCreationMenu.SetActive(false);
        }

        public void OpenChooseCharacterClassSubMenu()
        {
            //  1. DISABLE ALL MAIN MENU BUTTONS (SO YOU CANT ACCIDENTALLY HIT ONE)
            ToggleCharacterCreationScreenMainMenuButtons(false);

            //  2. ENABLE SUB MENU OBJECT (CLASS LIST WITH BUTTONS)
            characterClassMenu.SetActive(true);

            //  3. AUTO SELECT FIRST BUTTON
            if (characterClassButtons.Length > 0)
            {
                characterClassButtons[0].Select();
                characterClassButtons[0].OnSelect(null);
            }
        }

        public void CloseChooseCharacterClassSubMenu()
        {
            //  1. RE-ENABLE ALL MAIN MENU BUTTONS
            ToggleCharacterCreationScreenMainMenuButtons(true);

            //  2. DISABLE SUB MENU OBJECT
            characterClassMenu.SetActive(false);

            //  3. AUTO SELECT "CHOOSE CLASS BUTTON" (SINCE IT WAS THE LAST BUTTON YOU HIT DURING THE MAIN MENU
            characterClassButton.Select();
            characterClassButton.OnSelect(null);
        }

        private void ToggleCharacterCreationScreenMainMenuButtons(bool status)
        {
            characterNameButton.enabled = status;
            characterClassButton.enabled = status;
            startGameButton.enabled = status;
        }

        public void DisplayNoFreeCharacterSlotsPopUp()
        {
            noCharacterSlotsPopUp.SetActive(true);
            noCharacterSlotsOkayButton.Select();
        }

        public void CloseNoFreeCharacterSlotsPopUp()
        {
            noCharacterSlotsPopUp.SetActive(false);
            mainMenuNewGameButton.Select();
        }

        public void SelectCharacterSlot(CharacterSlot characterSlot)
        {
            currentSelectedSlot = characterSlot;
        }

        public void SelectNoSlot()
        {
            currentSelectedSlot = CharacterSlot.NO_SLOT;
        }

        public void AttemptToDeleteCharacterSlot()
        {
            if (currentSelectedSlot != CharacterSlot.NO_SLOT)
            {
                deleteCharacterSlotPopUp.SetActive(true);
                deleteCharacterPopUpConfirmButton.Select();
            }
        }

        public void DeleteCharacterSlot()
        {
            deleteCharacterSlotPopUp.SetActive(false);
            WorldSaveGameManager.instance.DeleteGame(currentSelectedSlot);
            
            // 我们先禁用然后启用加载菜单，以刷新槽位（已删除的槽位现在将变为非激活状态）
            titleScreenLoadMenu.SetActive(false);
            titleScreenLoadMenu.SetActive(true);
            
            loadMenuReturnButton.Select();
        }

        public void CloseDeleteCharacterPopUp()
        {
            deleteCharacterSlotPopUp.SetActive(false);
            loadMenuReturnButton.Select();
        }

        //  CHARACTER CLASS

        public void SelectClass(int classID)
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            if (startingClasses.Length <= 0)
                return;

            startingClasses[classID].SetClass(player);
            CloseChooseCharacterClassSubMenu();
        }

        public void PreviewClass(int classID)
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            if (startingClasses.Length <= 0)
                return;

            startingClasses[classID].SetClass(player);
        }

        public void SetCharacterClass(PlayerManager player, int vitality, int endurance, int mind, int strength, int dexterity, int intelligence, int faith,
            WeaponItem[] mainHandWeapons, WeaponItem[] offHandWeapons, 
            HeadEquipmentItem headEquipment, BodyEquipmentItem bodyEquipment, LegEquipmentItem legEquipment, HandEquipmentItem handEquipment,
            QuickSlotItem[] quickSlotItems)
        {
            // 1. Set the stats
            player.playerNetworkManager.vitality.Value = vitality;
            player.playerNetworkManager.endurance.Value = endurance;
            player.playerNetworkManager.mind.Value = mind;
            player.playerNetworkManager.strength.Value = strength;
            player.playerNetworkManager.dexterity.Value = dexterity;
            player.playerNetworkManager.intelligence.Value = intelligence;
            player.playerNetworkManager.faith.Value = faith;

            // 2. Set the weapons
            player.playerInventoryManager.weaponsInRightHandSlots[0] = Instantiate(mainHandWeapons[0]);
            player.playerInventoryManager.weaponsInRightHandSlots[1] = Instantiate(mainHandWeapons[1]);
            player.playerInventoryManager.weaponsInRightHandSlots[2] = Instantiate(mainHandWeapons[2]);
            player.playerInventoryManager.currentRightHandWeapon = player.playerInventoryManager.weaponsInRightHandSlots[0];
            player.playerNetworkManager.currentRightHandWeaponID.Value = player.playerInventoryManager.weaponsInRightHandSlots[0].itemID;

            player.playerInventoryManager.weaponsInLeftHandSlots[0] = Instantiate(offHandWeapons[0]);
            player.playerInventoryManager.weaponsInLeftHandSlots[1] = Instantiate(offHandWeapons[1]);
            player.playerInventoryManager.weaponsInLeftHandSlots[2] = Instantiate(offHandWeapons[2]);
            player.playerInventoryManager.currentLeftHandWeapon = player.playerInventoryManager.weaponsInLeftHandSlots[0];
            player.playerNetworkManager.currentLeftHandWeaponID.Value = player.playerInventoryManager.weaponsInLeftHandSlots[0].itemID;

            // 3. Set the armor

            //  HEAD EQUIPMENT
            if (headEquipment != null)
            {
                HeadEquipmentItem equipment = Instantiate(headEquipment);
                player.playerInventoryManager.headEquipment = equipment;
            }
            else
            {
                player.playerInventoryManager.headEquipment = null;
            }

            //  BODY EQUIPMENT
            if (bodyEquipment != null)
            {
                BodyEquipmentItem equipment = Instantiate(bodyEquipment);
                player.playerInventoryManager.bodyEquipment = equipment;
            }
            else
            {
                player.playerInventoryManager.bodyEquipment = null;
            }

            // LEG EQUIPMENT
            if (legEquipment != null)
            {
                LegEquipmentItem equipment = Instantiate(legEquipment);
                player.playerInventoryManager.legEquipment = equipment;
            }
            else
            {
                player.playerInventoryManager.legEquipment = null;
            }

            //  HAND EQUIPMENT
            if (handEquipment != null)
            {
                HandEquipmentItem equipment = Instantiate(handEquipment);
                player.playerInventoryManager.handEquipment = equipment;
            }
            else
            {
                player.playerInventoryManager.handEquipment = null;
            }

            player.playerEquipmentManager.EquipArmor();

            // 4. Set the quickslot items
            player.playerInventoryManager.quickSlotItemIndex = 0;

            if (quickSlotItems[0] != null)
                player.playerInventoryManager.quickSlotItemsInQuickSlots[0] = Instantiate(quickSlotItems[0]);
            if (quickSlotItems[1] != null)
                player.playerInventoryManager.quickSlotItemsInQuickSlots[1] = Instantiate(quickSlotItems[1]);
            if (quickSlotItems[2] != null)
                player.playerInventoryManager.quickSlotItemsInQuickSlots[2] = Instantiate(quickSlotItems[2]);

            player.playerEquipmentManager.LoadQuickSlotEquipment(player.playerInventoryManager.quickSlotItemsInQuickSlots[player.playerInventoryManager.quickSlotItemIndex]);
        }
    }
}