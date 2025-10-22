using Unity.Netcode;
using UnityEngine;

namespace LZ
{
    public class PlayerUIManager : MonoBehaviour
    {
        public static PlayerUIManager instance;
        [HideInInspector] public PlayerManager localPlayer;

        [Header("NETWORK JOIN")]
        [SerializeField] bool startGameAsClient;

        [HideInInspector] public PlayerUIHudManager playerUIHudManager;
        [HideInInspector] public PlayerUIPopUpManager playerUIPopUpManager;
        [HideInInspector] public PlayerUICharacterMenuManager playerUICharacterMenuManager;
        [HideInInspector] public PlayerUIEquipmentManager playerUIEquipmentManager;
        [HideInInspector] public PlayerUISiteOfGraceManager playerUISiteOfGraceManager;
        [HideInInspector] public PlayerUITeleportLocationManager playerUITeleportLocationManager;
        [HideInInspector] public PlayerUILoadingScreenManager playerUILoadingScreenManager;

        [Header("UI Flags")]
        public bool menuWindowIsOpen = false;       // 物品栏界面、装备菜单、铁匠菜单等
        public bool popUpWindowIsOpen = false;      // 物品拾取、对话弹出等

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

            playerUIHudManager = GetComponentInChildren<PlayerUIHudManager>();
            playerUIPopUpManager = GetComponentInChildren<PlayerUIPopUpManager>();
            playerUICharacterMenuManager = GetComponentInChildren<PlayerUICharacterMenuManager>();
            playerUIEquipmentManager = GetComponentInChildren<PlayerUIEquipmentManager>();
            playerUISiteOfGraceManager = GetComponentInChildren<PlayerUISiteOfGraceManager>();
            playerUITeleportLocationManager = GetComponentInChildren<PlayerUITeleportLocationManager>();
            playerUILoadingScreenManager = GetComponentInChildren<PlayerUILoadingScreenManager>();
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (startGameAsClient)
            {
                startGameAsClient = false;
                // we must first shut down, because we have started as a host during the title screen
                NetworkManager.Singleton.Shutdown();
                // we then restart, as a client
                NetworkManager.Singleton.StartClient();
            }
        }

        public void CloseAllMenuWindows()
        {
            playerUICharacterMenuManager.CloseCharacterMenu();
            playerUIEquipmentManager.CloseEquipmentManagerMenu();
            playerUISiteOfGraceManager.CloseSiteOfGraceManagerMenu();
            playerUITeleportLocationManager.CloseTeleportLocationManagerMenu();
        }
    }
}