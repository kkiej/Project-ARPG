using Unity.Mathematics;
using UnityEngine;

namespace LZ
{
    public class PlayerCamera : MonoBehaviour
    {
        public static PlayerCamera instance;
        public PlayerManager player;
        public Camera cameraObject;
        [SerializeField]
        private Transform cameraPivotTransform;

        // Change these to tweak camera performance
        [Header("Camera Settings")]
        private float cameraSmoothSpeed = 1f; // the bigger this number, the longer for the camera to reach its position during movement
        [SerializeField]
        private float leftAndRightRotationSpeed = 220;
        [SerializeField]
        private float upAndDownRotationSpeed = 220;
        [SerializeField]
        private float minimumPivot = -30;
        [SerializeField]
        private float maximumPivot = 60;
        [SerializeField]
        private float cameraCollisionOffset = 0.2f;
        
        [Header("Camera Values")]
        private Vector3 cameraVelocity;
        [SerializeField]
        private float leftAndRightLookAngle;
        [SerializeField]
        private float upAndDownLookAngle;

        private float defaultCameraPosition;
        private float targetCameraPosition;
        
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
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void HandleAllCameraActions()
        {
            if (player != null)
            {
                HandleFollowTarget();
                HandleRotations();
                HandleCollisions();
            }
        }

        private void HandleFollowTarget()
        {
            Vector3 targetCameraPosition = Vector3.SmoothDamp(transform.position, player.transform.position,
                ref cameraVelocity, cameraSmoothSpeed * Time.deltaTime);
            transform.position = targetCameraPosition;
        }

        private void HandleRotations()
        {
            // if locked on , force rotation towards target
            // else rotate regularly
            
            // Normal rotations
            leftAndRightLookAngle += (PlayerInputManager.instance.cameraHorizontalInput * leftAndRightRotationSpeed) *
                                     Time.deltaTime;
            upAndDownLookAngle -= (PlayerInputManager.instance.cameraVerticalInput * upAndDownRotationSpeed) *
                                  Time.deltaTime;
            upAndDownLookAngle = Mathf.Clamp(upAndDownLookAngle, minimumPivot, maximumPivot);
            
            Vector3 cameraRotation = Vector3.zero;
            Quaternion targetRotation;
            
            // Rotate this gameObject left and right
            cameraRotation.y = leftAndRightLookAngle;
            targetRotation = Quaternion.Euler(cameraRotation);
            transform.rotation = targetRotation;

            // Rotate pivot gameObject up and down
            cameraRotation = Vector3.zero;
            cameraRotation.x = upAndDownLookAngle;
            targetRotation = Quaternion.Euler(cameraRotation);
            cameraPivotTransform.localRotation = targetRotation;
        }

        private void HandleCollisions()
        {
            
        }
    }
}