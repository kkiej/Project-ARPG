using UnityEngine;
using UnityEngine.UI;

namespace LZ
{
    public class PlayerUIHudManager : MonoBehaviour
    {
        [SerializeField] CanvasGroup[] canvasGroup;

        [Header("Stat Bars")]
        [SerializeField] private UI_StatBar healthBar;
        [SerializeField] private UI_StatBar staminaBar;

        [Header("Quick Slots")]
        [SerializeField] private Image rightWeaponQuickSlotIcon;
        [SerializeField] private Image leftWeaponQuickSlotIcon;

        [Header("Boss Health Bar")]
        public Transform bossHealthBarParent;
        public GameObject bossHealthBarObject;

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
                Debug.Log("Item has no icon");
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
                Debug.Log("Item has no icon");
                leftWeaponQuickSlotIcon.enabled = false;
                leftWeaponQuickSlotIcon.sprite = null;
                return;
            }
            
            // 这是你检查是否满足物品要求的地方，如果你想创建一个警告，提示无法在用户界面中使用它。

            leftWeaponQuickSlotIcon.sprite = weapon.itemIcon;
            leftWeaponQuickSlotIcon.enabled = true;
        }
    }
}