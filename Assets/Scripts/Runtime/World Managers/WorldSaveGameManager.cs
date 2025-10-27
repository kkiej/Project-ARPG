using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LZ
{
    public class WorldSaveGameManager : MonoBehaviour
    {
        public static WorldSaveGameManager instance;

        public PlayerManager player;

        [Header("SAVE/LOAD")]
        [SerializeField] bool saveGame;
        [SerializeField] bool loadGame;

        [Header("World Scene Index")]
        [SerializeField] int worldSceneIndex = 1;

        [Header("Save Data Writer")]
        private SaveFileDataWriter saveFileDataWriter;

        [Header("Current Character Data")]
        public CharacterSlot currentCharacterSlotBeingUsed;
        public CharacterSaveData currentCharacterData;
        private string saveFileName;

        [Header("Character Slots")]
        public CharacterSaveData characterSlot01;
        public CharacterSaveData characterSlot02;
        public CharacterSaveData characterSlot03;
        public CharacterSaveData characterSlot04;
        public CharacterSaveData characterSlot05;
        public CharacterSaveData characterSlot06;
        public CharacterSaveData characterSlot07;
        public CharacterSaveData characterSlot08;
        public CharacterSaveData characterSlot09;
        public CharacterSaveData characterSlot10;
        
        private void Awake()
        {
            // There can only be one instance of this script at one time, if another exists, destroy it
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            LoadAllCharacterProfiles();
        }

        private void Update()
        {
            if (saveGame)
            {
                saveGame = false;
                SaveGame();
            }

            if (loadGame)
            {
                loadGame = false;
                LoadGame();
            }
        }

        public bool HasFreeCharacterSlot()
        {
            saveFileDataWriter = new SaveFileDataWriter();
            saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;

            // 检查是否能够创建新存档文件（需先检测其他已存在的文件）
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_01);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
                return true;

            // 检查是否能够创建新存档文件（需先检测其他已存在的文件）
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_02);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
                return true;

            // 检查是否能够创建新存档文件（需先检测其他已存在的文件）
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_03);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
                return true;

            // 检查是否能够创建新存档文件（需先检测其他已存在的文件）
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_04);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
                return true;

            // 检查是否能够创建新存档文件（需先检测其他已存在的文件）
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_05);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
                return true;

            // 检查是否能够创建新存档文件（需先检测其他已存在的文件）
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_06);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
                return true;

            // 检查是否能够创建新存档文件（需先检测其他已存在的文件）
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_07);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
                return true;

            // 检查是否能够创建新存档文件（需先检测其他已存在的文件）
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_08);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
                return true;

            // 检查是否能够创建新存档文件（需先检测其他已存在的文件）
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_09);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
                return true;

            // 检查是否能够创建新存档文件（需先检测其他已存在的文件）
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_10);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
                return true;

            return false;
        }

        public string DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot characterSlot)
        {
            string fileName = "";
            
            switch (characterSlot)
            {
                case CharacterSlot.CharacterSlot_01:
                    fileName = "characterSlot_01";
                    break;
                case CharacterSlot.CharacterSlot_02:
                    fileName = "characterSlot_02";
                    break;
                case CharacterSlot.CharacterSlot_03:
                    fileName = "characterSlot_03";
                    break;
                case CharacterSlot.CharacterSlot_04:
                    fileName = "characterSlot_04";
                    break;
                case CharacterSlot.CharacterSlot_05:
                    fileName = "characterSlot_05";
                    break;
                case CharacterSlot.CharacterSlot_06:
                    fileName = "characterSlot_06";
                    break;
                case CharacterSlot.CharacterSlot_07:
                    fileName = "characterSlot_07";
                    break;
                case CharacterSlot.CharacterSlot_08:
                    fileName = "characterSlot_08";
                    break;
                case CharacterSlot.CharacterSlot_09:
                    fileName = "characterSlot_09";
                    break;
                case CharacterSlot.CharacterSlot_10:
                    fileName = "characterSlot_10";
                    break;
                default:
                    break;
            }

            return fileName;
        }

        public void AttemptToCreateNewGame()
        {
            saveFileDataWriter = new SaveFileDataWriter();
            saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
            
            // 检查我们是否可以创建一个新的存档文件（首先检查其他已存在的文件）
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_01);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
            {
                // 如果这个档案槽位还没有被占用，使用这个槽位创建一个新的档案
                currentCharacterSlotBeingUsed = CharacterSlot.CharacterSlot_01;
                currentCharacterData = new CharacterSaveData();
                NewGame();
                return;
            }
            
            // 检查我们是否可以创建一个新的存档文件（首先检查其他已存在的文件）
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_02);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
            {
                // 如果这个档案槽位还没有被占用，使用这个槽位创建一个新的档案
                currentCharacterSlotBeingUsed = CharacterSlot.CharacterSlot_02;
                currentCharacterData = new CharacterSaveData();
                NewGame();
                return;
            }
            
            // 检查我们是否可以创建一个新的存档文件（首先检查其他已存在的文件）
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_03);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
            {
                // 如果这个档案槽位还没有被占用，使用这个槽位创建一个新的档案
                currentCharacterSlotBeingUsed = CharacterSlot.CharacterSlot_03;
                currentCharacterData = new CharacterSaveData();
                NewGame();
                return;
            }
            
            // 检查我们是否可以创建一个新的存档文件（首先检查其他已存在的文件）
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_04);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
            {
                // 如果这个档案槽位还没有被占用，使用这个槽位创建一个新的档案
                currentCharacterSlotBeingUsed = CharacterSlot.CharacterSlot_04;
                currentCharacterData = new CharacterSaveData();
                NewGame();
                return;
            }
            
            // 检查我们是否可以创建一个新的存档文件（首先检查其他已存在的文件）
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_05);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
            {
                // 如果这个档案槽位还没有被占用，使用这个槽位创建一个新的档案
                currentCharacterSlotBeingUsed = CharacterSlot.CharacterSlot_05;
                currentCharacterData = new CharacterSaveData();
                NewGame();
                return;
            }
            
            // 检查我们是否可以创建一个新的存档文件（首先检查其他已存在的文件）
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_06);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
            {
                // 如果这个档案槽位还没有被占用，使用这个槽位创建一个新的档案
                currentCharacterSlotBeingUsed = CharacterSlot.CharacterSlot_06;
                currentCharacterData = new CharacterSaveData();
                NewGame();
                return;
            }
            
            // 检查我们是否可以创建一个新的存档文件（首先检查其他已存在的文件）
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_07);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
            {
                // 如果这个档案槽位还没有被占用，使用这个槽位创建一个新的档案
                currentCharacterSlotBeingUsed = CharacterSlot.CharacterSlot_07;
                currentCharacterData = new CharacterSaveData();
                NewGame();
                return;
            }
            
            // 检查我们是否可以创建一个新的存档文件（首先检查其他已存在的文件）
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_08);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
            {
                // 如果这个档案槽位还没有被占用，使用这个槽位创建一个新的档案
                currentCharacterSlotBeingUsed = CharacterSlot.CharacterSlot_08;
                currentCharacterData = new CharacterSaveData();
                NewGame();
                return;
            }
            
            // 检查我们是否可以创建一个新的存档文件（首先检查其他已存在的文件）
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_09);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
            {
                // 如果这个档案槽位还没有被占用，使用这个槽位创建一个新的档案
                currentCharacterSlotBeingUsed = CharacterSlot.CharacterSlot_09;
                currentCharacterData = new CharacterSaveData();
                NewGame();
                return;
            }
            
            // 检查我们是否可以创建一个新的存档文件（首先检查其他已存在的文件）
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_10);

            if (!saveFileDataWriter.CheckToSeeIfFileExists())
            {
                // 如果这个档案槽位还没有被占用，使用这个槽位创建一个新的档案
                currentCharacterSlotBeingUsed = CharacterSlot.CharacterSlot_10;
                currentCharacterData = new CharacterSaveData();
                NewGame();
                return;
            }
            
            // 如果没有空插槽，提醒玩家
            TitleScreenManager.instance.DisplayNoFreeCharacterSlotsPopUp();
        }

        private void NewGame()
        {
            //  SAVES THE NEWLY CREATED CHARACTERS STATS, AND ITEMS (WHEN CREATION SCREEN IS ADDED)
            player.playerNetworkManager.vigor.Value = 15;
            player.playerNetworkManager.endurance.Value = 10;
            player.playerNetworkManager.mind.Value = 10;

            SaveGame();
            WorldSceneManager.instance.LoadWorldScene(worldSceneIndex);
        }
        
        public void LoadGame()
        {
            // 加载一个先前的文件，文件名取决于我们正在使用的槽位
            saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);

            saveFileDataWriter = new SaveFileDataWriter();
            // 通常可以在多种机器类型上工作（Application.persistentDataPath）
            saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
            saveFileDataWriter.saveFileName = saveFileName;
            currentCharacterData = saveFileDataWriter.LoadSaveFile();

            WorldSceneManager.instance.LoadWorldScene(worldSceneIndex);
        }

        public void SaveGame()
        {
            // 根据我们正在使用的槽位，将当前文件保存为一个文件名
            saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);
            
            saveFileDataWriter = new SaveFileDataWriter();
            // 通常可以在多种机器类型上工作（Application.persistentDataPath）
            saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
            saveFileDataWriter.saveFileName = saveFileName;
            
            // 将玩家信息从游戏传递到他们的存档文件
            player.SaveGameDataToCurrentCharacterData(ref currentCharacterData);
            
            // 将这些信息写入一个JSON文件，并保存到这台机器上
            saveFileDataWriter.CreateNewCharacterSaveFile(currentCharacterData);
        }

        public void DeleteGame(CharacterSlot characterSlot)
        {
            // 根据名字选择文件
            saveFileDataWriter = new SaveFileDataWriter();
            saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(characterSlot);
            
            saveFileDataWriter.DeleteSaveFile();
        }
        
        // 在开始游戏时加载设备上的所有角色档案
        private void LoadAllCharacterProfiles()
        {
            saveFileDataWriter = new SaveFileDataWriter();
            saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;

            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_01);
            characterSlot01 = saveFileDataWriter.LoadSaveFile();
            
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_02);
            characterSlot02 = saveFileDataWriter.LoadSaveFile();
            
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_03);
            characterSlot03 = saveFileDataWriter.LoadSaveFile();
            
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_04);
            characterSlot04 = saveFileDataWriter.LoadSaveFile();
            
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_05);
            characterSlot05 = saveFileDataWriter.LoadSaveFile();
            
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_06);
            characterSlot06 = saveFileDataWriter.LoadSaveFile();
            
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_07);
            characterSlot07 = saveFileDataWriter.LoadSaveFile();
            
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_08);
            characterSlot08 = saveFileDataWriter.LoadSaveFile();
            
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_09);
            characterSlot09 = saveFileDataWriter.LoadSaveFile();
            
            saveFileDataWriter.saveFileName =
                DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot.CharacterSlot_10);
            characterSlot10 = saveFileDataWriter.LoadSaveFile();
        }

        public int GetWorldSceneIndex()
        {
            return worldSceneIndex;
        }

        public SerializableWeapon GetSerializableWeaponFromWeaponItem(WeaponItem weapon)
        {
            SerializableWeapon serializedWeapon = new SerializableWeapon();

            //  GET WEAPON I.D
            serializedWeapon.itemID = weapon.itemID;
            
            //  GET ASH OF WAR I.D IF ONE IS PRESENT (THERE SHOULD ALWAYS BE ONE BY DEFAULT)
            if (weapon.ashOfWarAction != null)
            {
                serializedWeapon.ashOfWarID = weapon.ashOfWarAction.itemID;
            }
            else
            {
                //  WE USE AN INVALID ID IF THERE IS NO ASH OF WAR, SO THE VALUE WILL BE NULL IF IT TRIES TO SEARCH FOR ONE USING THE I.D
                serializedWeapon.ashOfWarID = -1;
            }

            return serializedWeapon;
        }

        public SerializableRangedProjectile GetSerializableRangedProjectileFromRangedProjectileItem(RangedProjectileItem projectile)
        {
            SerializableRangedProjectile serializedProjectile = new SerializableRangedProjectile();

            if (projectile != null)
            {
                //  GET WEAPON I.D
                serializedProjectile.itemID = projectile.itemID;
                serializedProjectile.itemAmount = projectile.currentAmmoAmount;
            }
            else
            {
                serializedProjectile.itemID = -1;
            }

            return serializedProjectile;
        }

        public SerializableFlask GetSerializableFlaskFromFlaskItem(FlaskItem flask)
        {
            SerializableFlask serializedFlask = new SerializableFlask();

            if (flask != null)
            {
                serializedFlask.itemID = flask.itemID;
            }
            else
            {
                serializedFlask.itemID = -1;
            }

            return serializedFlask;
        }

        public SerializableQuickSlotItem GetSerializableQuickSlotItemFromQuickSlotItem(QuickSlotItem quickSlotItem)
        {
            SerializableQuickSlotItem serializedQuickSlotItem = new SerializableQuickSlotItem();

            if (quickSlotItem != null)
            {
                //  GET WEAPON I.D
                serializedQuickSlotItem.itemID = quickSlotItem.itemID;
                serializedQuickSlotItem.itemAmount = quickSlotItem.itemAmount;
            }
            else
            {
                serializedQuickSlotItem.itemID = -1;
            }

            return serializedQuickSlotItem;
        }

    }
}