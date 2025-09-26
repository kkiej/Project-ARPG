using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LZ
{
    public class WorldItemDatabase : MonoBehaviour
    {
        public static WorldItemDatabase Instance;

        public WeaponItem unarmedWeapon;

        [SerializeField] private List<WeaponItem> weapons = new List<WeaponItem>();
        
        // 游戏中我们有的每一个物品的列表
        [Header("Items")]
        private List<Item> items = new List<Item>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
            // 将我们的所有武器添加到物品列表
            foreach (var weapon in weapons)
            {
                items.Add(weapon);
            }
            
            // 为所有的物品分配一个唯一的ID
            for (int i = 0; i < items.Count; i++)
            {
                items[i].itemID = i;
            }
        }

        public WeaponItem GetWeaponByID(int ID)
        {
            return weapons.FirstOrDefault(weapon => weapon.itemID == ID);
        }
    }
}