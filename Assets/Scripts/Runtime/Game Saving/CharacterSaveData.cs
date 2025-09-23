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

        [Header("Time Played")]
        public float secondsPlayed;

        [Header("World Coordinates")]
        public float xPosition;
        public float yPosition;
        public float zPosition;

        [Header("Resources")]
        public int currentHealth;
        public float currentStamina;
        
        [Header("Stats")]
        public int vitality;
        public int endurance;
        
        [Header("Sites Of Grace")]
        public SerializableDictionary<int, bool> sitesOfGrace;      //  THE INT IS THE SITE OF GRACE I.D, THE BOOL IS THE "ACTIVATED" STATUS
        
        [Header("Bosses")]
        public SerializableDictionary<int, bool> bossesAwakened;    //  THE INT IS THE BOSS I.D, THE BOOL IS THE AWAKENED STATUS
        public SerializableDictionary<int, bool> bossesDefeated;    //  THE INT IS THE BOSS I.D, THE BOOL IS THE DEFEATED STATUS

        public CharacterSaveData()
        {
            sitesOfGrace = new SerializableDictionary<int, bool>();
            bossesAwakened = new SerializableDictionary<int, bool>();
            bossesDefeated = new SerializableDictionary<int, bool>();
        }
    }
}