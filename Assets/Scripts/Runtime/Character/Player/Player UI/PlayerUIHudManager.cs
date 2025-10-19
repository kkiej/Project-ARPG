using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

namespace LZ
{
    public class PlayerUIHudManager : MonoBehaviour
    {
        [SerializeField] CanvasGroup[] canvasGroup;

        [Header("Stat Bars")]
        [SerializeField] UI_StatBar healthBar;
        [SerializeField] UI_StatBar staminaBar;
        [SerializeField] UI_StatBar focusPointBar;

        [Header("Quick Slots")]
        [SerializeField] Image rightWeaponQuickSlotIcon;
        [SerializeField] Image leftWeaponQuickSlotIcon;
        [SerializeField] Image spellItemQuickSlotIcon;
        [SerializeField] Image quickSlotItemQuickSlotIcon;
        [SerializeField] TextMeshProUGUI quickSlotItemCount;
        [SerializeField] GameObject projectileQuickSlotsGameObject;
        [SerializeField] Image mainProjectileQuickSlotIcon;
        [SerializeField] TextMeshProUGUI mainProjectileCount;
        [SerializeField] Image secondaryProjectileQuickSlotIcon;
        [SerializeField] TextMeshProUGUI secondaryProjectileCount;

        [Header("Boss Health Bar")]
        public Transform bossHealthBarParent;
        public GameObject bossHealthBarObject;

        [Header("Crosshair")]
        public GameObject crossHair;

        public void ToggleHUD(bool status)
        {
            //  TO DO FADE IN AND OUT OVER TIME

            if (status)
            {
                foreach (var canvas in canvasGroup)
                {
                    canvas.alpha = 1;
                }
            }
            else
            {
                foreach (var canvas in canvasGroup)
                {
                    canvas.alpha = 0;
                }
            }
        }

        public void RefreshHUD()
        {
            healthBar.gameObject.SetActive(false);
            healthBar.gameObject.SetActive(true);
            staminaBar.gameObject.SetActive(false);
            staminaBar.gameObject.SetActive(true);
            focusPointBar.gameObject.SetActive(false);
            focusPointBar.gameObject.SetActive(true);
        }

        public void SetNewHealthValue(int oldValue, int newValue)
        {
            healthBar.SetStat(newValue);
        }

        public void SetMaxHealthValue(int maxHealth)
        {
            healthBar.SetMaxStat(maxHealth);
        }
        
        public void SetNewStaminaValue(float oldValue, float newValue)
        {
            staminaBar.SetStat(Mathf.RoundToInt(newValue));
        }

        public void SetMaxStaminaValue(int maxStamina)
        {
            staminaBar.SetMaxStat(maxStamina);
        }


        public void SetNewFocusPointValue(int oldValue, int newValue)
        {
            focusPointBar.SetStat(Mathf.RoundToInt(newValue));
        }

        public void SetMaxFocusPointValue(int maxFocusPoints)
        {
            focusPointBar.SetMaxStat(maxFocusPoints);
        }

        public void SetRightWeaponQuickSlotIcon(int weaponID)
        {
            //1. Method one, DIRECTLY reference the right weapon in the hand of the player
            //Pros: It's super straight forward
            //Cons: If you forget to call this function AFTER you've loaded your weapons first, it will give you an error
            //Example: You load a previously saved game, you go to reference the weapons upon loading UI but they arent instantiated yet
            //Final Notes: This method is perfectly fine if you remember your order of operations

            //2. Method two, REQUIRE an item ID of the weapon, fetch the weapon from our database and use it to get the weapon items icon
            //Pros: Since you always save the current weapons ID, we dont need to wait to get it from the player we could get it before hand if required
            //Cons: It's not as direct
            //Final Notes: This method is great if you don't want to remember another oder of operations

            //  IF THE DATABASE DOES NOT CONTAIN A WEAPON MATCHING THE GIVEN I.D, RETURN

            WeaponItem weapon = WorldItemDatabase.Instance.GetWeaponByID(weaponID);
            
            if (weapon == null)
            {
                Debug.Log("ITEM IS NULL");
                rightWeaponQuickSlotIcon.enabled = false;
                rightWeaponQuickSlotIcon.sprite = null;
                return;
            }

            if (weapon.itemIcon == null)
            {
                Debug.Log("ITEM HAS NO ICON");
                rightWeaponQuickSlotIcon.enabled = false;
                rightWeaponQuickSlotIcon.sprite = null;
                return;
            }
            
            // 这是你检查是否满足物品要求的地方，如果你想创建一个警告，提示无法在用户界面中使用它。

            rightWeaponQuickSlotIcon.sprite = weapon.itemIcon;
            rightWeaponQuickSlotIcon.enabled = true;
        }
        
        public void SetLeftWeaponQuickSlotIcon(int weaponID)
        {
            //1. Method one, DIRECTLY reference the right weapon in the hand of the player
            //Pros: It's super straight forward
            //Cons: If you forget to call this function AFTER you've loaded your weapons first, it will give you an error
            //Example: You load a previously saved game, you go to reference the weapons upon loading UI but they arent instantiated yet
            //Final Notes: This method is perfectly fine if you remember your order of operations

            //2. Method two, REQUIRE an item ID of the weapon, fetch the weapon from our database and use it to get the weapon items icon
            //Pros: Since you always save the current weapons ID, we dont need to wait to get it from the player we could get it before hand if required
            //Cons: It's not as direct
            //Final Notes: This method is great if you don't want to remember another oder of operations

            //  IF THE DATABASE DOES NOT CONTAIN A WEAPON MATCHING THE GIVEN I.D, RETURN

            WeaponItem weapon = WorldItemDatabase.Instance.GetWeaponByID(weaponID);
            
            if (weapon == null)
            {
                Debug.Log("ITEM IS NULL");
                leftWeaponQuickSlotIcon.enabled = false;
                leftWeaponQuickSlotIcon.sprite = null;
                return;
            }

            if (weapon.itemIcon == null)
            {
                Debug.Log("ITEM HAS NO ICON");
                leftWeaponQuickSlotIcon.enabled = false;
                leftWeaponQuickSlotIcon.sprite = null;
                return;
            }
            
            // 这是你检查是否满足物品要求的地方，如果你想创建一个警告，提示无法在用户界面中使用它。

            leftWeaponQuickSlotIcon.sprite = weapon.itemIcon;
            leftWeaponQuickSlotIcon.enabled = true;
        }

        public void SetSpellItemQuickSlotIcon(int spellID)
        {
            SpellItem spell = WorldItemDatabase.Instance.GetSpellByID(spellID);

            if (spell == null)
            {
                Debug.Log("ITEM IS NULL");
                spellItemQuickSlotIcon.enabled = false;
                spellItemQuickSlotIcon.sprite = null;
                return;
            }

            if (spell.itemIcon == null)
            {
                Debug.Log("ITEM HAS NO ICON");
                spellItemQuickSlotIcon.enabled = false;
                spellItemQuickSlotIcon.sprite = null;
                return;
            }

            //  THIS IS WHERE YOU WOULD CHECK TO SEE IF YOU MEET THE ITEMS REQUIREMENTS IF YOU WANT TO CREATE THE WARNING FOR NOT BEING ABLE TO WIELD IT IN THE UI

            spellItemQuickSlotIcon.sprite = spell.itemIcon;
            spellItemQuickSlotIcon.enabled = true;
        }

        public void SetQuickSlotItemQuickSlotIcon(int itemID)
        {
            QuickSlotItem quickSlotItem = WorldItemDatabase.Instance.GetQuickSlotItemByID(itemID);

            if (quickSlotItem == null)
            {
                Debug.Log("ITEM IS NULL");
                quickSlotItemQuickSlotIcon.enabled = false;
                quickSlotItemQuickSlotIcon.sprite = null;
                quickSlotItemCount.enabled = false;
                return;
            }

            if (quickSlotItem.itemIcon == null)
            {
                Debug.Log("ITEM HAS NO ICON");
                quickSlotItemQuickSlotIcon.enabled = false;
                quickSlotItemQuickSlotIcon.sprite = null;
                quickSlotItemCount.enabled = false;
                return;
            }

            //  TO DO, UPDATE QUANTITY LEFT, SHOW IN UI
            //  FADE OUT ICON IF NONE REMAINING

            quickSlotItemQuickSlotIcon.sprite = quickSlotItem.itemIcon;
            quickSlotItemQuickSlotIcon.enabled = true;

            if (quickSlotItem.isConsumable)
            {
                PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();
                quickSlotItemCount.text = quickSlotItem.GetCurrentAmount(player).ToString();
                quickSlotItemCount.enabled = true;
            }
            else
            {
                quickSlotItemCount.enabled = false;
            }
        }

        public void ToggleProjectileQuickSlotsVisibility(bool status)
        {
            projectileQuickSlotsGameObject.SetActive(status);
        }

        public void SetMainProjectileQuickSlotIcon(RangedProjectileItem projectileItem)
        {
            if (projectileItem == null)
            {
                Debug.Log("ITEM IS NULL");
                mainProjectileQuickSlotIcon.enabled = false;
                mainProjectileQuickSlotIcon.sprite = null;
                mainProjectileCount.enabled = false;
                return;
            }

            if (projectileItem.itemIcon == null)
            {
                Debug.Log("ITEM HAS NO ICON");
                mainProjectileQuickSlotIcon.enabled = false;
                mainProjectileQuickSlotIcon.sprite = null;
                mainProjectileCount.enabled = false;
                return;
            }

            //  TO DO, UPDATE QUANTITY LEFT, SHOW IN UI
            //  FADE OUT ICON IF NONE REMAINING

            mainProjectileQuickSlotIcon.sprite = projectileItem.itemIcon;
            mainProjectileCount.text = projectileItem.currentAmmoAmount.ToString();
            mainProjectileQuickSlotIcon.enabled = true;
            mainProjectileCount.enabled = true;
        }

        public void SetSecondaryProjectileQuickSlotIcon(RangedProjectileItem projectileItem)
        {
            if (projectileItem == null)
            {
                Debug.Log("ITEM IS NULL");
                secondaryProjectileQuickSlotIcon.enabled = false;
                secondaryProjectileQuickSlotIcon.sprite = null;
                secondaryProjectileCount.enabled = false;
                return;
            }

            if (projectileItem.itemIcon == null)
            {
                Debug.Log("ITEM HAS NO ICON");
                secondaryProjectileQuickSlotIcon.enabled = false;
                secondaryProjectileQuickSlotIcon.sprite = null;
                secondaryProjectileCount.enabled = false;
                return;
            }

            //  TO DO, UPDATE QUANTITY LEFT, SHOW IN UI
            //  FADE OUT ICON IF NONE REMAINING

            secondaryProjectileQuickSlotIcon.sprite = projectileItem.itemIcon;
            secondaryProjectileCount.text = projectileItem.currentAmmoAmount.ToString();
            secondaryProjectileQuickSlotIcon.enabled = true;
            secondaryProjectileCount.enabled = true;
        }
    }
}