using Unity.Netcode;
using UnityEngine;

namespace LZ
{
    public class PlayerUIManager : MonoBehaviour
    {
        public static PlayerUIManager instance;
        
        [Header("NETWORK JOIN")] [SerializeField]
        private bool startGameAsClient;

        [HideInInspector] public PlayerUIHudManager playerUIHudManager;
        [HideInInspector] public PlayerUIPopUpManager playerUIPopUpManager;

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
    }
}