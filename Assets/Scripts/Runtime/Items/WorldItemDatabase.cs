using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace LZ
{
    public class WorldItemDatabase : MonoBehaviour
    {
        public static WorldItemDatabase Instance;

        public WeaponItem unarmedWeapon;

        public GameObject pickUpItemPrefab;

        [Header("Weapons")]
        [SerializeField] List<WeaponItem> weapons = new List<WeaponItem>();

        [Header("Head Equipment")]
        [SerializeField] List<HeadEquipmentItem> headEquipment = new List<HeadEquipmentItem>();

        [Header("Body Equipment")]
        [SerializeField] List<BodyEquipmentItem> bodyEquipment = new List<BodyEquipmentItem>();

        [Header("Leg Equipment")]
        [SerializeField] List<LegEquipmentItem> legEquipment = new List<LegEquipmentItem>();

        [Header("Hand Equipment")]
        [SerializeField] List<HandEquipmentItem> handEquipment = new List<HandEquipmentItem>();

        [Header("Ashes Of War")]
        [SerializeField] List<AshOfWar> ashesOfWar = new List<AshOfWar>();

        [Header("Spells")]
        [SerializeField] List<SpellItem> spells = new List<SpellItem>();

        [Header("Projectiles")]
        [SerializeField] List<RangedProjectileItem> projectiles = new List<RangedProjectileItem>();

        [Header("Quick Slot")]
        [SerializeField] List<QuickSlotItem> quickSlotItems = new List<QuickSlotItem>();

        [Header("Upgrade Materials")]
        [SerializeField] List<UpgradeMaterial> upgradeMaterials = new List<UpgradeMaterial>();

        //  A LIST OF EVERY ITEM WE HAVE IN THE GAME
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
            
            foreach (var item in headEquipment)
            {
                items.Add(item);
            }

            foreach (var item in bodyEquipment)
            {
                items.Add(item);
            }

            foreach (var item in legEquipment)
            {
                items.Add(item);
            }

            foreach (var item in handEquipment)
            {
                items.Add(item);
            }

            foreach (var item in ashesOfWar)
            {
                items.Add(item);
            }

            foreach (var item in spells)
            {
                items.Add(item);
            }

            foreach (var item in projectiles)
            {
                items.Add(item);
            }

            foreach (var item in quickSlotItems)
            {
                items.Add(item);
            }

            foreach (var item in upgradeMaterials)
            {
                items.Add(item);
            }

            //  ASSIGN ALL OF OUR ITEMS A UNIQUE ITEM ID
            for (int i = 0; i < items.Count; i++)
            {
                items[i].itemID = i;
            }
        }

        //  ITEM DATABASE

        public Item GetItemByID(int ID)
        {
            return items.FirstOrDefault(item => item.itemID == ID);
        }

        public WeaponItem GetWeaponByID(int ID)
        {
            return weapons.FirstOrDefault(weapon => weapon.itemID == ID);
        }
        
        public HeadEquipmentItem GetHeadEquipmentByID(int ID)
        {
            return headEquipment.FirstOrDefault(equipment => equipment.itemID == ID);
        }

        public BodyEquipmentItem GetBodyEquipmentByID(int ID)
        {
            return bodyEquipment.FirstOrDefault(equipment => equipment.itemID == ID);
        }

        public LegEquipmentItem GetLegEquipmentByID(int ID)
        {
            return legEquipment.FirstOrDefault(equipment => equipment.itemID == ID);
        }

        public HandEquipmentItem GetHandEquipmentByID(int ID)
        {
            return handEquipment.FirstOrDefault(equipment => equipment.itemID == ID);
        }

        public AshOfWar GetAshOfWarByID(int ID)
        {
            return ashesOfWar.FirstOrDefault(item => item.itemID == ID);
        }

        public SpellItem GetSpellByID(int ID)
        {
            return spells.FirstOrDefault(item => item.itemID == ID);
        }

        public RangedProjectileItem GetProjectileByID(int ID)
        {
            return projectiles.FirstOrDefault(item => item.itemID == ID);
        }

        public QuickSlotItem GetQuickSlotItemByID(int ID)
        {
            return quickSlotItems.FirstOrDefault(item => item.itemID == ID);
        }

        public UpgradeMaterial GetUpgradeMaterialByID(int ID)
        {
            return upgradeMaterials.FirstOrDefault(item => item.itemID == ID);
        }

        //  ITEM SERIALIZATION
        
        public WeaponItem GetWeaponFromSerializedData(SerializableWeapon serializableWeapon)
        {
            WeaponItem weapon = null;

            if (GetWeaponByID(serializableWeapon.itemID))
                weapon = Instantiate(GetWeaponByID(serializableWeapon.itemID));

            if (weapon == null)
                return Instantiate(unarmedWeapon);

            if (GetAshOfWarByID(serializableWeapon.ashOfWarID))
            {
                AshOfWar ashOfWar = Instantiate(GetAshOfWarByID(serializableWeapon.ashOfWarID));
                weapon.ashOfWarAction = ashOfWar;
            }

            weapon.upgradeLevel = (UpgradeLevel)serializableWeapon.upgradeLevel;

            return weapon;
        }

        public RangedProjectileItem GetRangedProjectileFromSerializedData(SerializableRangedProjectile serializableProjectile)
        {
            RangedProjectileItem rangedProjectile = null;

            if (GetProjectileByID(serializableProjectile.itemID))
            {
                rangedProjectile = Instantiate(GetProjectileByID(serializableProjectile.itemID));
                rangedProjectile.currentAmmoAmount = serializableProjectile.itemAmount;
            }

            return rangedProjectile;
        }

        public FlaskItem GetFlaskFromSerializedData(SerializableFlask serializableFlask)
        {
            FlaskItem flask = null;

            if (GetQuickSlotItemByID(serializableFlask.itemID))
                flask = Instantiate(GetQuickSlotItemByID(serializableFlask.itemID)) as FlaskItem;

            return flask;
        }

        public QuickSlotItem GetQuickSlotItemFromSerializedData(SerializableQuickSlotItem serializableQuickSlotItem)
        {
            QuickSlotItem quickSlotItem = null;

            if (GetQuickSlotItemByID(serializableQuickSlotItem.itemID))
            {
                quickSlotItem = Instantiate(GetQuickSlotItemByID(serializableQuickSlotItem.itemID));
                quickSlotItem.itemAmount = serializableQuickSlotItem.itemAmount;
            }

            return quickSlotItem;
        }
    }
}