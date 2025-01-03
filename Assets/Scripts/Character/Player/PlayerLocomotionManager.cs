using UnityEngine;

namespace LZ
{
    public class PlayerLocomotionManager : CharacterLocomotionManager
    {
        private PlayerManager player;
        
        public float verticalMovement;
        public float horizontalMovement;
        public float moveAmount;

        private Vector3 moveDirection;
        private Vector3 targetRotationDirection;
        
        [SerializeField]
        private float walkingSpeed = 2f;
        [SerializeField]
        private float runningSpeed = 5f;
        [SerializeField]
        private float rotationSpeed = 15f;
        
        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
        }

        public void HandleAllMovement()
        {
            HandleGroundedMovement();
            HandleRotation();
            // Aerial movement
        }

        private void GetVerticalAndHorizontalInputs()
        {
            verticalMovement = PlayerInputManager.instance.verticalInput;
            horizontalMovement = PlayerInputManager.instance.horizontalInput;
            
            // clamp the movements
        }

        private void HandleGroundedMovement()
        {
            GetVerticalAndHorizontalInputs();
            
            // our move direction is based on our cameras facing perspective & our movement inputs
            var cameraTransform = PlayerCamera.instance.transform;
            moveDirection = cameraTransform.forward * verticalMovement;
            moveDirection += cameraTransform.right * horizontalMovement;
            moveDirection.Normalize();
            moveDirection.y = 0;

            if (PlayerInputManager.instance.moveAmount > 0.5f)
            {
                player.characterController.Move(moveDirection * (runningSpeed * Time.deltaTime));
            }
            else if (PlayerInputManager.instance.moveAmount <= 0.5f)
            {
                player.characterController.Move(moveDirection * (walkingSpeed * Time.deltaTime));
            }
        }

        private void HandleRotation()
        {
            targetRotationDirection = Vector3.zero;
            var cameraTransform = PlayerCamera.instance.cameraObject.transform;
            targetRotationDirection = cameraTransform.forward * verticalMovement;
            targetRotationDirection += cameraTransform.right * horizontalMovement;
            targetRotationDirection.Normalize();
            targetRotationDirection.y = 0;

            if (targetRotationDirection == Vector3.zero)
            {
                targetRotationDirection = transform.forward;
            }

            Quaternion newRotation = Quaternion.LookRotation(targetRotationDirection);
            Quaternion targetRotation =
                Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
            transform.rotation = targetRotation;
        }
    }
}