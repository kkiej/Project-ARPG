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
    }
}