using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [System.Serializable]
    // 由于我们希望在每个存档文件中引用这些数据，这个脚本不是一个MonoBehaviour，而是可序列化的。
    public class CharacterSaveData
    {
        [Header("Scene Index")]
        public int sceneIndex = 1;
        
        [Header("Character Name")]
        public string characterName = "Character";

        [Header("Dead Spot")]
        public bool hasDeadSpot = false;
        public float deadSpotPositionX;
        public float deadSpotPositionY;
        public float deadSpotPositionZ;
        public int deadSpotRuneCount;

        [Header("Body Type")]
        public bool isMale = true;
        public int hairStyleID;
        public float hairColorRed;
        public float hairColorGreen;
        public float hairColorBlue;

        [Header("Time Played")]
        public float secondsPlayed;

        // QUESTION: WHY NOT USE A VECTOR3?
        // ANSWER: WE CAN ONLY SAVE DATA FROM "BASIC" VARIABLE TYPES (Float, Int, String, Bool, ect)
        [Header("World Coordinates")]
        public float xPosition;
        public float yPosition;
        public float zPosition;

        [Header("Resources")]
        public int currentHealth;
        public float currentStamina;
        public int currentFocusPoints;
        public int runes;

        [Header("Stats")]
        public int vitality;
        public int mind;
        public int endurance;
        public int strength;
        public int dexterity;
        public int intelligence;
        public int faith;

        [Header("Sites Of Grace")]
        public int lastSiteOfGraceRestedAt = 0;
        public SerializableDictionary<int, bool> sitesOfGrace;      //  THE INT IS THE SITE OF GRACE I.D, THE BOOL IS THE "ACTIVATED" STATUS
        
        [Header("Bosses")]
        public SerializableDictionary<int, bool> bossesAwakened;    //  THE INT IS THE BOSS I.D, THE BOOL IS THE AWAKENED STATUS
        public SerializableDictionary<int, bool> bossesDefeated;    //  THE INT IS THE BOSS I.D, THE BOOL IS THE DEFEATED STATUS

        [Header("World Items")]
        public SerializableDictionary<int, bool> worldItemsLooted;  //  THE INT IS THE ITEM I.D, THE BOOL IS THE LOOTED STATUS

        [Header("Equipment")]
        public int headEquipment;
        public int bodyEquipment;
        public int legEquipment;
        public int handEquipment;

        public int rightWeaponIndex;
        public SerializableWeapon rightWeapon01;
        public SerializableWeapon rightWeapon02;
        public SerializableWeapon rightWeapon03;

        public int leftWeaponIndex;
        public SerializableWeapon leftWeapon01;
        public SerializableWeapon leftWeapon02;
        public SerializableWeapon leftWeapon03;

        public int quickSlotIndex;
        public SerializableQuickSlotItem quickSlotItem01;
        public SerializableQuickSlotItem quickSlotItem02;
        public SerializableQuickSlotItem quickSlotItem03;

        public SerializableRangedProjectile mainProjectile;
        public SerializableRangedProjectile secondaryProjectile;

        public int currentHealthFlasksRemaining = 3;
        public int currentFocusPointsFlaskRemaining = 1;

        [Header("Inventory")]
        public List<SerializableWeapon> weaponsInInventory;
        public List<SerializableRangedProjectile> projectilesInInventory;
        public List<SerializableQuickSlotItem> quickSlotItemsInInventory;
        public List<int> headEquipmentInInventory;
        public List<int> bodyEquipmentInInventory;
        public List<int> handEquipmentInInventory;
        public List<int> legEquipmentInInventory;

        //  THIS WILL CHANGE A LITTLE WHEN WE ADD MULTIPLE SPELL SLOTS, IT WILL BE SOMEWHAT SIMILAR TO HOW WEAPONS ARE SAVED
        public int currentSpell;


        public CharacterSaveData()
        {
            sitesOfGrace = new SerializableDictionary<int, bool>();
            bossesAwakened = new SerializableDictionary<int, bool>();
            bossesDefeated = new SerializableDictionary<int, bool>();
            worldItemsLooted = new SerializableDictionary<int, bool>();

            weaponsInInventory = new List<SerializableWeapon>();
            projectilesInInventory = new List<SerializableRangedProjectile>();
            quickSlotItemsInInventory = new List<SerializableQuickSlotItem>();
            headEquipmentInInventory = new List<int>();
            bodyEquipmentInInventory = new List<int>();
            legEquipmentInInventory = new List<int>();
            handEquipmentInInventory = new List<int>();
        }
    }
}