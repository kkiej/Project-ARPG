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

        [Header("Body Type")]
        public bool isMale = true;

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

        [Header("Stats")]
        public int vitality;
        public int endurance;
        public int mind;

        [Header("Sites Of Grace")]
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
        public int rightWeapon01;
        public int rightWeapon02;
        public int rightWeapon03;

        public int leftWeaponIndex;
        public int leftWeapon01;
        public int leftWeapon02;
        public int leftWeapon03;

        //  THIS WILL CHANGE A LITTLE WHEN WE ADD MULTIPLE SPELL SLOTS, IT WILL BE SOMEWHAT SIMILAR TO HOW WEAPONS ARE SAVED
        public int currentSpell;


        public CharacterSaveData()
        {
            sitesOfGrace = new SerializableDictionary<int, bool>();
            bossesAwakened = new SerializableDictionary<int, bool>();
            bossesDefeated = new SerializableDictionary<int, bool>();
            worldItemsLooted = new SerializableDictionary<int, bool>();
        }
    }
}