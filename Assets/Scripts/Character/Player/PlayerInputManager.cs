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

        public PlayerManager player;
        // Think about goals in steps
        // 1. Find a way to read the values of a joy stick
        // 2. move character based on those values
        
        private PlayerControls playerControls;
        
        [Header("CAMERA MOVEMENT INPUT")]
        [SerializeField] private Vector2 cameraInput;
        public float cameraHorizontalInput;
        public float cameraVerticalInput;

        [Header("PLAYER MOVEMENT INPUT")]
        [SerializeField] private Vector2 movementInput;
        public float horizontalInput;
        public float verticalInput;
        public float moveAmount;

        [Header("PLAYER ACTION INPUT")]
        [SerializeField] private bool dodgeInput;

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
            // 如果我们正在加载到我们的世界场景中，启用我们的玩家控制。
            if (newScene.buildIndex == WorldSaveGameManager.instance.GetWorldSceneIndex())
            {
                instance.enabled = true;
            }
            // 否则我们一定在主菜单，禁用我们的玩家控制
            // 这样我们的玩家就不能在进入比如角色创建菜单等时四处移动。
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
                playerControls.PlayerActions.Dodge.performed += i => dodgeInput = true;
            }
            
            playerControls.Enable();
        }

        private void OnDestroy()
        {
            // 如果我们销毁这个对象，就取消订阅这个事件。
            SceneManager.activeSceneChanged -= OnSceneChange;
        }

        // 如果我们最小化或降低窗口，停止调整输入。
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
            HandleAllInputs();
        }

        private void HandleAllInputs()
        {
            HandlePlayerMovementInput();
            HandleCameraMovementInput();
            HandleDodgeInput();
        }

        // movement
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
            
            // 为什么我们在水平方向传递0？因为我们只想要非侧移运动
            // 当我们侧移或锁定目标时，我们会使用水平方向

            if (player == null)
            {
                return;
            }
            // 如果我们没有锁定目标，只使用moveAmount
            player.playerAnimatorManager.UpdateAnimatorMovementParameters(0, moveAmount);
            
            // 如果我们被锁定，也要传递水平方向的movement
        }

        private void HandleCameraMovementInput()
        {
            cameraVerticalInput = cameraInput.y;
            cameraHorizontalInput = cameraInput.x;
        }

        
        // actions
        private void HandleDodgeInput()
        {
            if (dodgeInput)
            {
                dodgeInput = false;
                
                // 未来注意：如果菜单或用户界面窗口打开，则返回（不做任何操作）
                
                player.playerLocomotionManager.AttemptToPerformDodge();
            }
        }
    }
}