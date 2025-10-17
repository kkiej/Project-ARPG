using UnityEngine;
using UnityEngine.SceneManagement;

namespace LZ
{
    public class PlayerInputManager : MonoBehaviour
    {
        // 输入控制
        private PlayerControls playerControls;
        
        // Singleton
        public static PlayerInputManager instance;

        // local player
        public PlayerManager player;
        
        [Header("Camera Movement Input")]
        [SerializeField] private Vector2 cameraInput;
        public float cameraHorizontalInput;
        public float cameraVerticalInput;

        [Header("Lock On Input")]
        [SerializeField] private bool lockOn_Input;
        [SerializeField] private bool lockOn_Left_Input;
        [SerializeField] private bool lockOn_Right_Input;
        private Coroutine lockOnCoroutine;

        [Header("Player Movement Input")]
        [SerializeField] private Vector2 movementInput;
        public float horizontalInput;
        public float verticalInput;
        public float moveAmount;

        [Header("Player Action Input")]
        [SerializeField] private bool dodgeInput;
        [SerializeField] private bool sprintInput;
        [SerializeField] private bool jumpInput;
        [SerializeField] private bool switch_Right_Weapon_Input;
        [SerializeField] private bool switch_Left_Weapon_Input;
        [SerializeField] bool interaction_Input = false;
        
        [Header("Bumper Inputs")]
        [SerializeField] bool RB_Input = false;
        [SerializeField] bool hold_RB_Input = false;
        [SerializeField] bool LB_Input = false;
        [SerializeField] bool hold_LB_Input = false;

        [Header("Trigger Inputs")]
        [SerializeField] private bool RT_Input;
        [SerializeField] private bool Hold_RT_Input;
        [SerializeField] bool LT_Input = false;
        
		[Header("Two Hand Inputs")]
        [SerializeField] bool two_Hand_Input = false;
        [SerializeField] bool two_Hand_Right_Weapon_Input = false;
        [SerializeField] bool two_Hand_Left_Weapon_Input = false;
		
        [Header("Qued Inputs")]
        [SerializeField] private bool input_Que_Is_Active = false;
        [SerializeField] float default_Que_Input_Time = 0.35f;
        [SerializeField] float que_Input_Timer = 0;
        [SerializeField] bool que_RB_Input = false;
        [SerializeField] bool que_RT_Input = false;

        [Header("UI INPUTS")]
        [SerializeField] bool openCharacterMenuInput = false;
        [SerializeField] bool closeMenuInput = false;

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
            if (playerControls != null)
            {
                playerControls.Disable();
            }
        }

        private void OnSceneChange(Scene oldScene, Scene newScene)
        {
            // 如果我们正在加载到我们的世界场景中，启用我们的玩家控制。
            if (newScene.buildIndex == WorldSaveGameManager.instance.GetWorldSceneIndex())
            {
                instance.enabled = true;
                if (playerControls != null)
                {
                    playerControls.Enable();
                }
            }
            // 否则我们一定在主菜单，禁用我们的玩家控制
            // 这样我们的玩家就不能在进入比如角色创建菜单等时四处移动。
            else
            {
                instance.enabled = false;
                if (playerControls != null)
                {
                    playerControls.Disable();
                }
            }
        }

        private void OnEnable()
        {
            if (playerControls == null)
            {
                playerControls = new PlayerControls();

                playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
                playerControls.PlayerCamera.Movement.performed += i => cameraInput = i.ReadValue<Vector2>();
                
                // 动作
                playerControls.PlayerActions.Dodge.performed += i => dodgeInput = true;
                playerControls.PlayerActions.Jump.performed += i => jumpInput = true;
                playerControls.PlayerActions.SwitchRightWeapon.performed += i => switch_Right_Weapon_Input = true;
                playerControls.PlayerActions.SwitchLeftWeapon.performed += i => switch_Left_Weapon_Input = true;
                playerControls.PlayerActions.Interact.performed += i => interaction_Input = true;
                
                // Bumpers
                playerControls.PlayerActions.RB.performed += i => RB_Input = true;
                playerControls.PlayerActions.HoldRB.performed += i => hold_RB_Input = true;
                playerControls.PlayerActions.HoldRB.canceled += i => hold_RB_Input = false;

                playerControls.PlayerActions.LB.performed += i => LB_Input = true;
                playerControls.PlayerActions.LB.canceled += i => player.playerNetworkManager.isBlocking.Value = false;
                playerControls.PlayerActions.HoldLB.performed += i => hold_LB_Input = true;
                playerControls.PlayerActions.HoldLB.canceled += i => hold_LB_Input = false;

                //  TRIGGERS
                playerControls.PlayerActions.RT.performed += i => RT_Input = true;
                playerControls.PlayerActions.HoldRT.performed += i => Hold_RT_Input = true;
                playerControls.PlayerActions.HoldRT.canceled += i => Hold_RT_Input = false;
                playerControls.PlayerActions.LT.performed += i => LT_Input = true;

                //  TWO HAND
                playerControls.PlayerActions.TwoHandWeapon.performed += i => two_Hand_Input = true;
                playerControls.PlayerActions.TwoHandWeapon.canceled += i => two_Hand_Input = false;
                playerControls.PlayerActions.TwoHandRightWeapon.performed += i => two_Hand_Right_Weapon_Input = true;
                playerControls.PlayerActions.TwoHandRightWeapon.canceled += i => two_Hand_Right_Weapon_Input = false;
                playerControls.PlayerActions.TwoHandLeftWeapon.performed += i => two_Hand_Left_Weapon_Input = true;
                playerControls.PlayerActions.TwoHandLeftWeapon.canceled += i => two_Hand_Left_Weapon_Input = false;

                //  LOCK ON
                playerControls.PlayerActions.LockOn.performed += i => lockOn_Input = true;
                playerControls.PlayerActions.SeekLeftLockOnTarget.performed += i => lockOn_Left_Input = true;
                playerControls.PlayerActions.SeekRightLockOnTarget.performed += i => lockOn_Right_Input = true;
                
                // 长按输入，将bool设置成true
                playerControls.PlayerActions.Sprint.performed += i => sprintInput = true;
                // 释放输入，将bool设置成false
                playerControls.PlayerActions.Sprint.canceled += i => sprintInput = false;
                
                //  QUED INPUTS
                playerControls.PlayerActions.QueRB.performed += i => QueInput(ref que_RB_Input);
                playerControls.PlayerActions.QueRT.performed += i => QueInput(ref que_RT_Input);

                //  UI INPUTS
                playerControls.PlayerActions.Dodge.performed += i => closeMenuInput = true;
                playerControls.PlayerActions.OpenCharacterMenu.performed += i => openCharacterMenuInput = true;
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
            HandleTwoHandInput();
            HandleLockOnInput();
            HandleLockOnSwitchTargetInput();
            HandlePlayerMovementInput();
            HandleCameraMovementInput();
            HandleDodgeInput();
            HandleSprintInput();
            HandleJumpInput();
            HandleRBInput();
            HandleHoldRBInput();
            HandleLBInput();
            HandleHoldLBInput();
            HandleRTInput();
            HandleChargeRTInput();
            HandleLTInput();
            HandleSwitchRightWeaponInput();
            HandleSwitchLeftWeaponInput();
            HandleQuedInputs();
            HandleInteractionInput();
            HandleCloseUIInput();
            HandleOpenCharacterMenuInput();
        }

        //  TWO HAND
        private void HandleTwoHandInput()
        {
            if (!two_Hand_Input)
                return;

            if (two_Hand_Right_Weapon_Input)
            {
                //  IF WE ARE USING THE TWO HAND INPUT AND PRESSING THE RIGHT TWO HAND BUTTON WE WANT TO STOP THE REGULAR RB INPUT (OR ELSE WE WOULD ATTACK)
                RB_Input = false;
                two_Hand_Right_Weapon_Input = false;
                player.playerNetworkManager.isBlocking.Value = false;

                if (player.playerNetworkManager.isTwoHandingWeapon.Value)
                {
                    //  IF WE ARE TWO HANDING A WEAPON ALREADY, CHANGE THE IS TWOHANDING BOOL TO FALSE WHICH TRIGGERS AN "ONVALUECHANGED" FUNCTION,
                    //  WHICH UN-TWOHANDS CURRENT WEAPON
                    player.playerNetworkManager.isTwoHandingWeapon.Value = false;
                    return;
                }
                else
                {
                    //  IF WE ARE NOT ALREADY TWO HANGING, CHANGE THE RIGHT TWO HAND BOOL TO TRUE, WHICH TRIGGERS AN ONVALUECHANGED FUNCTION
                    //  THIS FUNCTION TWO HANDS THE RIGHT WEAPON
                    player.playerNetworkManager.isTwoHandingRightWeapon.Value = true;
                    return;
                }
            }
            else if (two_Hand_Left_Weapon_Input)
            {
                //  IF WE ARE USING THE TWO HAND INPUT AND PRESSING THE RIGHT TWO HAND BUTTON WE WANT TO STOP THE REGULAR RB INPUT (OR ELSE WE WOULD ATTACK)
                LB_Input = false;
                two_Hand_Left_Weapon_Input = false;
                player.playerNetworkManager.isBlocking.Value = false;

                if (player.playerNetworkManager.isTwoHandingWeapon.Value)
                {
                    //  IF WE ARE TWO HANDING A WEAPON ALREADY, CHANGE THE IS TWOHANDING BOOL TO FALSE WHICH TRIGGERS AN "ONVALUECHANGED" FUNCTION,
                    //  WHICH UN-TWOHANDS CURRENT WEAPON
                    player.playerNetworkManager.isTwoHandingWeapon.Value = false;
                    return;
                }
                else
                {
                    //  IF WE ARE NOT ALREADY TWO HANGING, CHANGE THE RIGHT TWO HAND BOOL TO TRUE, WHICH TRIGGERS AN ONVALUECHANGED FUNCTION
                    //  THIS FUNCTION TWO HANDS THE RIGHT WEAPON
                    player.playerNetworkManager.isTwoHandingLeftWeapon.Value = true;
                    return;
                }
            }
        }

        //  LOCK ON
        private void HandleLockOnInput()
        {
            //  CHECK FOR DEAD TARGET
            if (player.playerNetworkManager.isLockedOn.Value)
            {
                if (player.playerCombatManager.currentTarget == null)
                    return;

                if (player.playerCombatManager.currentTarget.isDead.Value)
                {
                    player.playerNetworkManager.isLockedOn.Value = false;
                }

                //  ATTEMPT TO FIND NEW TARGET

                //  THIS ASSURES US THAT THE COROUTINE NEVER RUNS MUILTPLE TIMES OVERLAPPING ITSELF
                if (lockOnCoroutine != null)
                    StopCoroutine(lockOnCoroutine);

                lockOnCoroutine = StartCoroutine(PlayerCamera.instance.WaitThenFindNewTarget());
            }
            
            if (lockOn_Input && player.playerNetworkManager.isLockedOn.Value)
            {
                lockOn_Input = false;
                PlayerCamera.instance.ClearLockOnTargets();
                player.playerNetworkManager.isLockedOn.Value = false;
                //  DISABLE LOCK ON
                return;
            }
            
            if (lockOn_Input && !player.playerNetworkManager.isLockedOn.Value)
            {
                lockOn_Input = false;
                
                // 如果我们在用远程武器瞄准就返回（瞄准时不允许锁定）
                
                PlayerCamera.instance.HandleLocatingLockOnTargets();

                if (PlayerCamera.instance.nearestLockOnTarget != null)
                {
                    player.playerCombatManager.SetTarget(PlayerCamera.instance.nearestLockOnTarget);
                    player.playerNetworkManager.isLockedOn.Value = true;
                }
            }
        }

        private void HandleLockOnSwitchTargetInput()
        {
            if (lockOn_Left_Input)
            {
                lockOn_Left_Input = false;

                if (player.playerNetworkManager.isLockedOn.Value)
                {
                    PlayerCamera.instance.HandleLocatingLockOnTargets();

                    if (PlayerCamera.instance.leftLockOnTarget != null)
                    {
                        player.playerCombatManager.SetTarget(PlayerCamera.instance.leftLockOnTarget);
                    }
                }
            }
            
            if (lockOn_Right_Input)
            {
                lockOn_Right_Input = false;

                if (player.playerNetworkManager.isLockedOn.Value)
                {
                    PlayerCamera.instance.HandleLocatingLockOnTargets();

                    if (PlayerCamera.instance.rightLockOnTarget != null)
                    {
                        player.playerCombatManager.SetTarget(PlayerCamera.instance.rightLockOnTarget);
                    }
                }
            }
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
                return;

            if (moveAmount != 0)
            {
                player.playerNetworkManager.isMoving.Value = true;
            }
            else
            {
                player.playerNetworkManager.isMoving.Value = false;
            }

            if (!player.playerLocomotionManager.canRun)
            {
                if (moveAmount > 0.5f)
                    moveAmount = 0.5f;

                if (verticalInput > 0.5f)
                    verticalInput = 0.5f;

                if (horizontalInput > 0.5f)
                    horizontalInput = 0.5f;
                Debug.Log("canRun: false" + "   moveAmount: " + moveAmount);
            }
            else
            {
                Debug.Log("canRun: true" + "   moveAmount: " + moveAmount);
            }

            if (!player.playerNetworkManager.isLockedOn.Value || player.playerNetworkManager.isSprinting.Value)
            {
                player.playerAnimatorManager.UpdateAnimatorMovementParameters(0, moveAmount, player.playerNetworkManager.isSprinting.Value);
            }
            // 如果我们被锁定，也要传递水平方向的movement
            else
            {
                player.playerAnimatorManager.UpdateAnimatorMovementParameters(horizontalInput, verticalInput, player.playerNetworkManager.isSprinting.Value);
            }
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

        private void HandleSprintInput()
        {
            if (sprintInput)
            {
                player.playerLocomotionManager.HandleSprinting();
            }
            else
            {
                player.playerNetworkManager.isSprinting.Value = false;
            }
        }

        private void HandleJumpInput()
        {
            if (jumpInput)
            {
                jumpInput = false;
                
                // 如果我们有打开的UI窗口，那么不做任何事直接返回
                if (PlayerUIManager.instance.menuWindowIsOpen)
                    return;
					
                // 尝试执行跳跃动作
                player.playerLocomotionManager.AttemptToPerformJump();
            }
        }

        private void HandleRBInput()
        {
            if (two_Hand_Input)
                return;

            if (RB_Input)
            {
                RB_Input = false;
                
                // TODO: 如果我们有UI窗口开着，那么什么也不做，直接返回
                
                player.playerNetworkManager.SetCharacterActionHand(true);
                
                // TODO: 如果我们双持武器，使用双持动作

                player.playerCombatManager.PerformWeaponBasedAction(player.playerInventoryManager.currentRightHandWeapon.oh_RB_Action, player.playerInventoryManager.currentRightHandWeapon);
            }
        }

        private void HandleHoldRBInput()
        {
            if (hold_RB_Input)
            {
                player.playerNetworkManager.isChargingRightSpell.Value = true;
                player.playerNetworkManager.isHoldingArrow.Value = true;
            }
            else
            {
                player.playerNetworkManager.isChargingRightSpell.Value = false;
                player.playerNetworkManager.isHoldingArrow.Value = false;
            }
        }

        private void HandleLBInput()
        {
            if (two_Hand_Input)
                return;

            if (LB_Input)
            {
                LB_Input = false;

                //  TODO: IF WE HAVE A UI WINDOW OPEN, RETURN AND DO NOTHING

                player.playerNetworkManager.SetCharacterActionHand(false);

                //  TODO: IF WE ARE TWO HANDING THE WEAPON, USE THE TWO HANDED ACTION

                player.playerCombatManager.PerformWeaponBasedAction(player.playerInventoryManager.currentLeftHandWeapon.oh_LB_Action, player.playerInventoryManager.currentLeftHandWeapon);
            }
        }

        private void HandleHoldLBInput()
        {
            if (hold_LB_Input)
            {
                player.playerNetworkManager.isChargingLeftSpell.Value = true;
            }
            else
            {
                player.playerNetworkManager.isChargingLeftSpell.Value = false;
            }
        }

        private void HandleRTInput()
        {
            if (RT_Input)
            {
                RT_Input = false;
                
                // TODO: 如果我们有UI窗口开着，那么什么也不做，直接返回
                
                player.playerNetworkManager.SetCharacterActionHand(true);
                
                // TODO: 如果我们双持武器，使用双持动作

                player.playerCombatManager.PerformWeaponBasedAction(player.playerInventoryManager.currentRightHandWeapon.oh_RT_Action, player.playerInventoryManager.currentRightHandWeapon);
            }
        }

        private void HandleChargeRTInput()
        {
            // 我们只想检查是不是需要蓄力攻击
            if (player.isPerformingAction)
            {
                if (player.playerNetworkManager.isUsingRightHand.Value)
                {
                    player.playerNetworkManager.isChargingAttack.Value = Hold_RT_Input;
                }
            }
        }

        private void HandleLTInput()
        {
            if (LT_Input)
            {
                LT_Input = false;

                WeaponItem weaponPerformingAshOfWar = player.playerCombatManager.SelectWeaponToPerformAshOfWar();

                weaponPerformingAshOfWar.ashOfWarAction.AttemptToPerformAction(player);
            }
        }

        private void HandleSwitchRightWeaponInput()
        {
            if (switch_Right_Weapon_Input)
            {
                switch_Right_Weapon_Input = false;

                if (PlayerUIManager.instance.menuWindowIsOpen)
                    return;

                player.playerEquipmentManager.SwitchRightWeapon();
            }
        }
        
        private void HandleSwitchLeftWeaponInput()
        {
            if (switch_Left_Weapon_Input)
            {
                switch_Left_Weapon_Input = false;

                if (PlayerUIManager.instance.menuWindowIsOpen)
                    return;

                player.playerEquipmentManager.SwitchLeftWeapon();
            }
        }
        
        private void HandleInteractionInput()
        {
            if (interaction_Input)
            {
                interaction_Input = false;

                player.playerInteractionManager.Interact();
            }
        }
        
        private void QueInput(ref bool quedInput)   //传递引用意味着我们传递的是特定的布尔变量本身，而非该布尔变量的值（真或假）
        {
            // 重置所有已排队的输入，以确保每次只能有一个（输入）进入队列
            que_RB_Input = false;
            que_RT_Input = false;
            //que_LB_Input = false;
            //que_LT_Input = false;

            // 检查UI窗口是否处于打开状态，若已打开则直接返回

            if (player.isPerformingAction || player.playerNetworkManager.isJumping.Value)
            {
                quedInput = true;
                que_Input_Timer = default_Que_Input_Time;
                input_Que_Is_Active = true;
            }
        }

        private void ProcessQuedInput()
        {
            if (player.isDead.Value)
                return;

            if (que_RB_Input)
                RB_Input = true;

            if (que_RT_Input)
                RT_Input = true;
        }

        private void HandleQuedInputs()
        {
            if (input_Que_Is_Active)
            {
                // 当计时器大于0，一直尝试输入
                if (que_Input_Timer > 0)
                {
                    que_Input_Timer -= Time.deltaTime;
                    ProcessQuedInput();
                }
                else
                {
                    // 重置所有输入队列
                    que_RB_Input = false;
                    que_RT_Input = false;

                    input_Que_Is_Active = false;
                    que_Input_Timer = 0;
                }
            }
        }

        private void HandleOpenCharacterMenuInput()
        {
            if (openCharacterMenuInput)
            {
                openCharacterMenuInput = false;

                PlayerUIManager.instance.playerUIPopUpManager.CloseAllPopUpWindows();
                PlayerUIManager.instance.CloseAllMenuWindows();
                PlayerUIManager.instance.playerUICharacterMenuManager.OpenCharacterMenu();
            }
        }

        private void HandleCloseUIInput()
        {
            if (closeMenuInput)
            {
                closeMenuInput = false;

                if (PlayerUIManager.instance.menuWindowIsOpen)
                {
                    PlayerUIManager.instance.CloseAllMenuWindows();
                }
            }
        }
    }
}