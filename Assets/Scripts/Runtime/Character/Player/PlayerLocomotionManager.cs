using UnityEngine;
using UnityEngine.Serialization;

namespace LZ
{
    public class PlayerLocomotionManager : CharacterLocomotionManager
    {
        private PlayerManager player;
        
        [HideInInspector] public float verticalMovement;
        [HideInInspector] public float horizontalMovement;
        [HideInInspector] public float moveAmount;

        [Header("Movement Settings")]
        private Vector3 moveDirection;
        private Vector3 targetRotationDirection;
        [SerializeField] private float walkingSpeed = 2f;
        [SerializeField] private float runningSpeed = 5f;
        [SerializeField] private float sprintingSpeed = 6.5f;
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private int sprintingStaminaCost = 2;

        [Header("Jump")]
        [SerializeField] private float jumpStaminaCost = 25;
        [SerializeField] private float jumpHeight = 4f;
        [SerializeField] private float jumpForwardSpeed = 5f;
        [SerializeField] private float freeFallSpeed = 2f;
        private Vector3 jumpDirection;
        
        [Header("Dodge")]
        private Vector3 rollDirection;
        [SerializeField] private float dodgeStaminaCost = 25;
        
        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
        }

        protected override void Update()
        {
            base.Update();

            if (player.IsOwner)
            {
                player.characterNetworkManager.verticalMovement.Value = verticalMovement;
                player.characterNetworkManager.horizontalMovement.Value = horizontalMovement;
                player.characterNetworkManager.moveAmount.Value = moveAmount;
            }
            else
            {
                verticalMovement = player.characterNetworkManager.verticalMovement.Value;
                horizontalMovement = player.characterNetworkManager.horizontalMovement.Value;
                moveAmount = player.characterNetworkManager.moveAmount.Value;
                
                // 如果没锁定，传递movement
                if (!player.playerNetworkManager.isLockedOn.Value || player.playerNetworkManager.isSprinting.Value)
                {
                    player.playerAnimatorManager.UpdateAnimatorMovementParameters(0, moveAmount, player.playerNetworkManager.isSprinting.Value);
                }
                // 如果锁定了，传递水平方向和垂直方向的movement
                else
                {
                    player.playerAnimatorManager.UpdateAnimatorMovementParameters(horizontalMovement, verticalMovement, player.playerNetworkManager.isSprinting.Value);
                }
            }
        }

        public void HandleAllMovement()
        {
            HandleGroundedMovement();
            HandleRotation();
            HandleJumpingMovement();
            HandleFreeFallMovement();
        }

        private void GetMovementValues()
        {
            verticalMovement = PlayerInputManager.instance.verticalInput;
            horizontalMovement = PlayerInputManager.instance.horizontalInput;
            moveAmount = PlayerInputManager.instance.moveAmount;
            // clamp the movements
        }

        private void HandleGroundedMovement()
        {
            if (player.characterLocomotionManager.canMove || player.playerLocomotionManager.canRotate)
            {
                GetMovementValues();
            }

            if (!player.characterLocomotionManager.canMove)
                return;

            //  OUR MOVE DIRECTION IS BASED ON OUR CAMERAS FACING PERSPECTIVE & OUR MOVEMENT INPUTS

            if (player.playerNetworkManager.isAiming.Value)
            {
                moveDirection = transform.forward * verticalMovement;
                moveDirection = moveDirection + transform.right * horizontalMovement;
                moveDirection.Normalize();
                moveDirection.y = 0;
            }
            else
            {
                moveDirection = PlayerCamera.instance.transform.forward * verticalMovement;
                moveDirection = moveDirection + PlayerCamera.instance.transform.right * horizontalMovement;
                moveDirection.Normalize();
                moveDirection.y = 0;
            }

            if (player.playerNetworkManager.isSprinting.Value)
            {
                player.characterController.Move(moveDirection * (sprintingSpeed * Time.deltaTime));
            }
            else
            {
                if (PlayerInputManager.instance.moveAmount > 0.5f)
                {
                    player.characterController.Move(moveDirection * (runningSpeed * Time.deltaTime));
                }
                else if (PlayerInputManager.instance.moveAmount <= 0.5f)
                {
                    player.characterController.Move(moveDirection * (walkingSpeed * Time.deltaTime));
                }
            }
        }

        private void HandleJumpingMovement()
        {
            if (player.playerNetworkManager.isJumping.Value)
            {
                player.characterController.Move(jumpDirection * (jumpForwardSpeed * Time.deltaTime));
            }
        }

        private void HandleFreeFallMovement()
        {
            if (!player.characterLocomotionManager.isGrounded)
            {
                Vector3 freeFallDirection;

                freeFallDirection = PlayerCamera.instance.transform.forward * PlayerInputManager.instance.verticalInput;
                freeFallDirection += PlayerCamera.instance.transform.right * PlayerInputManager.instance.horizontalInput;
                freeFallDirection.y = 0;

                player.characterController.Move(freeFallDirection * (freeFallSpeed * Time.deltaTime));
            }
        }

        private void HandleRotation()
        {
            if (player.isDead.Value)
                return;

            if (!player.characterLocomotionManager.canRotate)
                return;

            if (player.playerNetworkManager.isAiming.Value)
            {
                HandleAimRotations();
            }
            else
            {
                HandleStandardRotation();
            }
        }

        private void HandleAimRotations()
        {
            Vector3 targetDirection;
            targetDirection = PlayerCamera.instance.cameraObject.transform.forward;
            targetDirection.y = 0;
            targetDirection.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Quaternion finalRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            transform.rotation = finalRotation;
        }

        private void HandleStandardRotation()
        {
            if (player.playerNetworkManager.isLockedOn.Value)
            {
                if (player.playerNetworkManager.isSprinting.Value || player.playerLocomotionManager.isRolling)
                {
                    Vector3 targetDirection = Vector3.zero;
                    targetDirection = PlayerCamera.instance.cameraObject.transform.forward * verticalMovement;
                    targetDirection += PlayerCamera.instance.cameraObject.transform.right * horizontalMovement;
                    targetDirection.Normalize();
                    targetDirection.y = 0;

                    if (targetDirection == Vector3.zero)
                        targetDirection = transform.forward;

                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    Quaternion finalRotation = Quaternion.Slerp(transform.rotation, targetRotation,
                        rotationSpeed * Time.deltaTime);
                    transform.rotation = finalRotation;
                }
                else
                {
                    if (player.playerCombatManager.currentTarget == null)
                        return;

                    Vector3 targetDirection;
                    targetDirection = player.playerCombatManager.currentTarget.transform.position - transform.position;
                    targetDirection.y = 0;
                    targetDirection.Normalize();

                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    Quaternion finalRotation = Quaternion.Slerp(transform.rotation, targetRotation,
                        rotationSpeed * Time.deltaTime);
                    transform.rotation = finalRotation;
                }
            }
            else
            {
                targetRotationDirection = Vector3.zero;
                targetRotationDirection = PlayerCamera.instance.cameraObject.transform.forward * verticalMovement;
                targetRotationDirection = targetRotationDirection + PlayerCamera.instance.cameraObject.transform.right * horizontalMovement;
                targetRotationDirection.Normalize();
                targetRotationDirection.y = 0;

                if (targetRotationDirection == Vector3.zero)
                {
                    targetRotationDirection = transform.forward;
                }

                Quaternion newRotation = Quaternion.LookRotation(targetRotationDirection);
                Quaternion targetRotation = Quaternion.Slerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
                transform.rotation = targetRotation;
            }
        }

        public void HandleSprinting()
        {
            if (player.isPerformingAction)
            {
                player.playerNetworkManager.isSprinting.Value = false;
            }
            
            // 如果我们没有体力了，将冲刺设置为假（关闭）
            if (player.playerNetworkManager.currentStamina.Value <= 0)
            {
                player.playerNetworkManager.isSprinting.Value = false;
                return;
            }

            if (moveAmount >= 0.5f)
            {
                player.playerNetworkManager.isSprinting.Value = true;
            }
            // 如果我们是静止的或者移动速度很慢，将冲刺设置为假（关闭）
            else
            {
                player.playerNetworkManager.isSprinting.Value = false;
            }

            if (player.playerNetworkManager.isSprinting.Value)
            {
                player.playerNetworkManager.currentStamina.Value -= sprintingStaminaCost * Time.deltaTime;
            }
        }

        public void AttemptToPerformDodge()
        {
            if (player.isPerformingAction)
                return;

            if (player.playerNetworkManager.currentStamina.Value <= 0)
                return;
            
            // 如果我们在运动中尝试躲避，我们将播放翻滚动画
            if (PlayerInputManager.instance.moveAmount > 0)
            {
                rollDirection = PlayerCamera.instance.cameraObject.transform.forward * PlayerInputManager.instance.verticalInput;
                rollDirection += PlayerCamera.instance.cameraObject.transform.right * PlayerInputManager.instance.horizontalInput;
                rollDirection.y = 0;
                rollDirection.Normalize();
            
                Quaternion playerRotation = Quaternion.LookRotation(rollDirection);
                player.transform.rotation = playerRotation;

                player.playerAnimatorManager.PlayTargetActionAnimation("Roll_Forward_01", true, true);
                player.playerLocomotionManager.isRolling = true;
            }
            // 如果我们处于静止状态，我们执行一个后撤步
            else
            {
                player.playerAnimatorManager.PlayTargetActionAnimation("Back_Step_01", true, true);
            }

            player.playerNetworkManager.currentStamina.Value -= dodgeStaminaCost;
            player.playerNetworkManager.DestroyAllCurrentActionFXServerRpc();
        }
        
        public void AttemptToPerformJump()
        {
            // 如果我们正在播放通用动作，我们不想要跳跃（添加战斗时会修改）
            if (player.isPerformingAction)
                return;

            // 如果没有体力，不允许跳跃
            if (player.playerNetworkManager.currentStamina.Value <= 0)
                return;

            // 如果我们在跳跃中，我们不想要重复跳跃直到当前跳跃动作完成
            if (player.playerNetworkManager.isJumping.Value)
                return;

            // 如果我们不在地面上，不允许跳跃
            if (!player.characterLocomotionManager.isGrounded)
                return;
            
            // To Do : 如果双持武器，播放双持条约动画，否则播放单手动画
            player.playerAnimatorManager.PlayTargetActionAnimation("Main_Jump_01", false);

            player.playerNetworkManager.isJumping.Value = true;

            player.playerNetworkManager.currentStamina.Value -= jumpStaminaCost;

            jumpDirection = PlayerCamera.instance.cameraObject.transform.forward *
                            PlayerInputManager.instance.verticalInput;
            jumpDirection += PlayerCamera.instance.cameraObject.transform.right *
                             PlayerInputManager.instance.horizontalInput;
            jumpDirection.y = 0;

            if (jumpDirection != Vector3.zero)
            {
                // 如果我们正在冲刺，跳跃方向是全距离
                if (player.playerNetworkManager.isSprinting.Value)
                {
                    jumpDirection *= 1;
                }
                // 如果我们正在奔跑，跳跃变成一半
                else if (PlayerInputManager.instance.moveAmount > 0.5)
                {
                    jumpDirection *= 0.5f;
                }
                // 如果是在慢走，跳跃变成四分之一
                else if (PlayerInputManager.instance.moveAmount <= 0.5)
                {
                    jumpDirection *= 0.25f;
                }
            }
        }

        public void ApplyJumpingVelocity()
        {
            yVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravityForce);
        }
    }
}