using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

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
        [SerializeField] Button characterHairButton;
        [SerializeField] Button characterHairColorButton;
        [SerializeField] Button characterSexButton;
        [SerializeField] TextMeshProUGUI characterSexText;
        [SerializeField] Button startGameButton;

        [Header("Character Creation Class Panel Buttons")]
        [SerializeField] Button[] characterClassButtons;
        [SerializeField] Button[] characterHairButtons;
        [SerializeField] Button[] characterHairColorButtons;

        [Header("Character Creation Secondary Panel Menus")]
        [SerializeField] GameObject characterClassMenu;
        [SerializeField] GameObject characterHairMenu;
        [SerializeField] GameObject characterHairColorMenu;
        [SerializeField] GameObject characterNameMenu;
        [SerializeField] TMP_InputField characterNameInputField;

        [Header("Color Sliders")]
        [SerializeField] Slider redSlider;
        [SerializeField] Slider greenSlider;
        [SerializeField] Slider blueSlider;

        [Header("Hidden Gear")]
        private HeadEquipmentItem hiddenHelmet;

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

        public void ToggleBodyType()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            player.playerNetworkManager.isMale.Value = !player.playerNetworkManager.isMale.Value;

            if (player.playerNetworkManager.isMale.Value)
            {
                characterSexText.text = "MALE";
            }
            else
            {
                characterSexText.text = "FEMALE";
            }
        }

        public void OpenTitleScreenMainMenu()
        {
            titleScreenMainMenu.SetActive(true);
        }

        public void CloseTitleScreenMainMenu()
        {
            titleScreenMainMenu.SetActive(false);
        }

        public void OpenCharacterCreationMenu()
        {
            CloseTitleScreenMainMenu();

            titleScreenCharacterCreationMenu.SetActive(true);

            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  SETS DEFAULT BODY TYPE
            player.playerBodyManager.ToggleBodyType(true);
        }

        public void CloseCharacterCreationMenu()
        {
            titleScreenCharacterCreationMenu.SetActive(false);

            OpenTitleScreenMainMenu();
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

        public void OpenChooseHairStyleSubMenu()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  1. DISABLE ALL MAIN MENU BUTTONS (SO YOU CANT ACCIDENTALLY HIT ONE)
            ToggleCharacterCreationScreenMainMenuButtons(false);

            //  2. ENABLE SUB MENU OBJECT (CLASS LIST WITH BUTTONS)
            characterHairMenu.SetActive(true);

            //  3. AUTO SELECT FIRST BUTTON
            if (characterHairButtons.Length > 0)
            {
                characterHairButtons[0].Select();
                characterHairButtons[0].OnSelect(null);
            }

            //  STORE THE HELMET THE PLAYER HAD ON
            if (player.playerInventoryManager.headEquipment != null)
                hiddenHelmet = Instantiate(player.playerInventoryManager.headEquipment);

            //  UNEQUIP THE HELMET AND RELOAD THE GEAR
            player.playerInventoryManager.headEquipment = null;
            player.playerEquipmentManager.EquipArmor();
        }

        public void CloseChooseHairStyleSubMenu()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  1. RE-ENABLE ALL MAIN MENU BUTTONS
            ToggleCharacterCreationScreenMainMenuButtons(true);

            //  2. DISABLE SUB MENU OBJECT
            characterHairMenu.SetActive(false);

            //  3. AUTO SELECT "CHOOSE CLASS BUTTON" (SINCE IT WAS THE LAST BUTTON YOU HIT DURING THE MAIN MENU
            characterHairButton.Select();
            characterHairButton.OnSelect(null);

            if (hiddenHelmet != null)
                player.playerInventoryManager.headEquipment = hiddenHelmet;

            player.playerEquipmentManager.EquipArmor();
        }

        public void OpenChooseHairColorSubMenu()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  1. DISABLE ALL MAIN MENU BUTTONS (SO YOU CANT ACCIDENTALLY HIT ONE)
            ToggleCharacterCreationScreenMainMenuButtons(false);

            //  2. ENABLE SUB MENU OBJECT (CLASS LIST WITH BUTTONS)
            characterHairColorMenu.SetActive(true);

            //  3. AUTO SELECT FIRST BUTTON
            if (characterHairColorButtons.Length > 0)
            {
                characterHairColorButtons[0].Select();
                characterHairColorButtons[0].OnSelect(null);
            }

            //  STORE THE HELMET THE PLAYER HAD ON
            if (player.playerInventoryManager.headEquipment != null)
                hiddenHelmet = Instantiate(player.playerInventoryManager.headEquipment);

            //  UNEQUIP THE HELMET AND RELOAD THE GEAR
            player.playerInventoryManager.headEquipment = null;
            player.playerEquipmentManager.EquipArmor();
        }

        public void CloseChooseHairColorSubMenu()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  1. RE-ENABLE ALL MAIN MENU BUTTONS
            ToggleCharacterCreationScreenMainMenuButtons(true);

            //  2. DISABLE SUB MENU OBJECT
            characterHairColorMenu.SetActive(false);

            //  3. AUTO SELECT "CHOOSE CLASS BUTTON" (SINCE IT WAS THE LAST BUTTON YOU HIT DURING THE MAIN MENU
            characterHairColorButton.Select();
            characterHairColorButton.OnSelect(null);

            if (hiddenHelmet != null)
                player.playerInventoryManager.headEquipment = hiddenHelmet;

            player.playerEquipmentManager.EquipArmor();
        }

        public void OpenChooseNameSubMenu()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  1. DISABLE ALL MAIN MENU BUTTONS (SO YOU CANT ACCIDENTALLY HIT ONE)
            ToggleCharacterCreationScreenMainMenuButtons(false);

            //  2. DISABLE NAME BUTTON GAMEOBJECT, AND REPLACE IT WITH NAME FIELD GAME OBJECT
            characterNameButton.gameObject.SetActive(false);
            characterNameMenu.SetActive(true);

            //  3. SELECT NAME FIELD OBJECT
            characterNameInputField.Select();
        }

        public void CloseChooseNameSubMenu()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  1. RE-ENABLE ALL MAIN MENU BUTTONS
            ToggleCharacterCreationScreenMainMenuButtons(true);

            //  2. ENABLE NAME BUTTON GAMEOBJECT, AND DISABLE NAME FIELD GAME OBJECT
            characterNameMenu.SetActive(false);
            characterNameButton.gameObject.SetActive(true);

            //  3. SELECT NAME BUTTON
            characterNameButton.Select();

            player.playerNetworkManager.characterName.Value = characterNameInputField.text;
        }

        private void ToggleCharacterCreationScreenMainMenuButtons(bool status)
        {
            characterNameButton.enabled = status;
            characterClassButton.enabled = status;
            characterHairButton.enabled = status;
            characterHairColorButton.enabled = status;
            characterSexButton.enabled = status;
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

        //  CHARACTER SLOTS

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
            // 0. Clear the hidden helmet (just incase someone figures how out how to store a helmet and then re-equip it on another class)
            hiddenHelmet = null;

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

        //  CHARACTER HAIR
        public void SelectHair(int hairID)
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            player.playerNetworkManager.hairStyleID.Value = hairID;

            CloseChooseHairStyleSubMenu();
        }

        public void PreviewHair(int hairID)
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            player.playerNetworkManager.hairStyleID.Value = hairID;
        }

        public void SelectHairColor()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            player.playerNetworkManager.hairColorRed.Value = redSlider.value;
            player.playerNetworkManager.hairColorGreen.Value = greenSlider.value;
            player.playerNetworkManager.hairColorBlue.Value = blueSlider.value;

            CloseChooseHairColorSubMenu();
        }

        public void PreviewHairColor()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            player.playerNetworkManager.hairColorRed.Value = redSlider.value;
            player.playerNetworkManager.hairColorGreen.Value = greenSlider.value;
            player.playerNetworkManager.hairColorBlue.Value = blueSlider.value;
        }

        public void SetRedColorSlider(float redValue)
        {
            redSlider.value = redValue;
        }

        public void SetGreenColorSlider(float greenValue)
        {
            greenSlider.value = greenValue;
        }

        public void SetBlueColorSlider(float blueValue)
        {
            blueSlider.value = blueValue;
        }
    }
}