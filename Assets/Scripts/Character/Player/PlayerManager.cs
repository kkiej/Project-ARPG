using UnityEngine;

namespace LZ
{
    public class PlayerManager : CharacterManager
    {
        private PlayerLocomotionManager playerLocomotionManager;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Do more stuff, only for the player
            playerLocomotionManager = GetComponent<PlayerLocomotionManager>();
        }

        protected override void Update()
        {
            base.Update();
            
            // if we do not own this gameObject, we do not control or edit it
            if (!IsOwner)
            {
                return;
            }
            
            // Handle movement
            playerLocomotionManager.HandleAllMovement();
        }

        protected override void LateUpdate()
        {
            if (!IsOwner)
            {
                return;
            }
            base.LateUpdate();
            
            PlayerCamera.instance.HandleAllCameraActions();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            // if this is the player object owned by this client
            if (IsOwner)
            {
                PlayerCamera.instance.player = this;
            }
        }
    }
}