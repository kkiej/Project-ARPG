using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LZ
{
    public class PlayerInputManager : MonoBehaviour
    {
        public static PlayerInputManager instance;
        // Think about goals in steps
        // 1. Find a way to read the values of a joy stick
        // 2. move character based on those values
        
        private PlayerControls playerControls;

        [Header("PLAYER MOVEMENT INPUT")]
        [SerializeField]
        private Vector2 movementInput;
        public float horizontalInput;
        public float verticalInput;
        public float moveAmount;
        
        [Header("CAMERA MOVEMENT INPUT")]
        [SerializeField]
        private Vector2 cameraInput;
        public float cameraHorizontalInput;
        public float cameraVerticalInput;

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
            
            // when the scene changes, run this logic
            SceneManager.activeSceneChanged += OnSceneChange;
            
            instance.enabled = false;
        }

        private void OnSceneChange(Scene oldScene, Scene newScene)
        {
            // if we are loading into our world scene, enable our players controls
            if (newScene.buildIndex == WorldSaveGameManager.instance.GetWorldSceneIndex())
            {
                instance.enabled = true;
            }
            // otherwise we must be at the main menu, disable our players controls
            // this is so our player cant move around if we enter things like a character creation menu ect.
            else
            {
                instance.enabled = false;
            }
        }

        private void OnEnable()
        {
            if (playerControls == null)
            {
                playerControls = new PlayerControls();

                playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
                playerControls.PlayerCamera.Movement.performed += i => cameraInput = i.ReadValue<Vector2>();
            }
            
            playerControls.Enable();
        }

        private void OnDestroy()
        {
            // if we destroy this object, unsubscribe from this event
            SceneManager.activeSceneChanged -= OnSceneChange;
        }

        // if we minimize or lower the window, stop adjusting inputs
        private void OnApplicationFocus(bool hasFocus)
        {
            if (enabled)
            {
                if (hasFocus)
                {
                    playerControls.Enable();
                }
                else
                {
                    playerControls.Disable();
                }
            }
        }

        private void Update()
        {
            HandlePlayerMovementInput();
            HandleCameraMovementInput();
        }

        private void HandlePlayerMovementInput()
        {
            verticalInput = movementInput.y;
            horizontalInput = movementInput.x;
            
            // returns the absolute number, (Meaning number without the negative sign, so its always positive)
            moveAmount = Mathf.Clamp01(Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput));
            
            // we clamp the values, so they are 0, 0.5 or 1
            if (moveAmount <= 0.5 && moveAmount > 0)
            {
                moveAmount = 0.5f;
            }
            else if (moveAmount > 0.5f && moveAmount <= 1)
            {
                moveAmount = 1f;
            }
        }

        private void HandleCameraMovementInput()
        {
            cameraVerticalInput = cameraInput.y;
            cameraHorizontalInput = cameraInput.x;
        }
    }
}