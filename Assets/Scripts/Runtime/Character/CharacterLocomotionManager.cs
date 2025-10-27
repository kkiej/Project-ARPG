using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class CharacterLocomotionManager : MonoBehaviour
    {
        CharacterManager character;

        [Header("Ground Check & Jumping")]
        [SerializeField] protected float gravityForce = -5.55f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckSphereRadius = 1;
        [SerializeField] protected Vector3 yVelocity; // 我们角色被向上或向下拉（跳跃或下落）的力
        [SerializeField] protected float groundedYVelocity = -20; // 我们的角色在着地时粘附于地面的力
        [SerializeField] protected float fallStartYVelocity = -5; // 当角色失去地面接触时开始下落的力（随着下落时间的增加而增大）
        protected bool fallingVelocityHasBeenSet;
        [SerializeField] protected float inAirTimer = 0;

        [Header("Flags")]
        public bool isRolling = false;
        public bool canRotate = true;
        public bool canMove = true;
        public bool canRun = true;
        public bool canRoll = true;
        public bool isGrounded = true;

        [Header("Slope Sliding")]
        [SerializeField] float slopeSlideStartPositionYOffset = 1;
        [SerializeField] float slopeSlideSphereCastMaxDistance = 2;
        private Vector3 slopeSlideVelocity;
        [SerializeField] float slopeSlideSpeed = -11;
        [SerializeField] float slopeSlideSpeedMultiplier = 3;
        [SerializeField] float slipperySurfaceMaxAngle = 15;
        private bool isSliding = false;
        private bool slideUntilGrounded = false;


        protected virtual void Awake()
        {
            character = GetComponent<CharacterManager>();
        }

        protected virtual void Update()
        {
            HandleGroundCheck();
            SetGroundedVelocity();
            HandleSlopeSlideCheck();

            if (character.characterLocomotionManager.isGrounded)
            {
                // 如果我们没有尝试跳跃或向上移动
                if (yVelocity.y <= 0)
                {
                    inAirTimer = 0;
                    fallingVelocityHasBeenSet = false;
                    yVelocity.y = groundedYVelocity;
                }
            }
            else
            {
                // 如果我们没在跳跃，并且我们的下落速度没有被设置
                if (!character.characterNetworkManager.isJumping.Value && !fallingVelocityHasBeenSet)
                {
                    fallingVelocityHasBeenSet = true;
                    yVelocity.y = fallStartYVelocity;
                }

                inAirTimer += Time.deltaTime;
                character.animator.SetFloat("InAirTimer", inAirTimer);
                yVelocity.y += gravityForce * Time.deltaTime;
            }
            
            // 应该始终对Y轴速度施加一定的力
            character.characterController.Move(yVelocity * Time.deltaTime);
        }

        protected void HandleGroundCheck()
        {
            isGrounded = Physics.CheckSphere(character.transform.position, groundCheckSphereRadius, groundLayer);
        }

        //  DRAWS OUR GROUND CHECK SPHERE IN SCENE VIEW
        protected void OnDrawGizmosSelected()
        {
            //Gizmos.DrawSphere(character.transform.position, groundCheckSphereRadius);
        }

        public void EnableCanRotate()
        {
            canRotate = true;
        }
        
        public void DisableCanRotate()
        {
            canRotate = false;
        }

        //  SLOPES & SLIDING
        private void HandleSlopeSlideCheck()
        {
            if (slopeSlideVelocity == Vector3.zero)
                isSliding = false;

            if (!isGrounded && slideUntilGrounded)
            {
                SetSlopeSlideVelocity(WorldUtilityManager.Instance.GetEnviroLayers());
                return;
            }

            if (!isGrounded)
                return;

            SetSlopeSlideVelocity(WorldUtilityManager.Instance.GetSlipperyEnviroLayers());
        }

        //  THIS FUNCTION DETERMINES WHAT OUR SLOPE SLIDE VELOCITY WILL BE WHEN NOT GROUNDED
        private void SetSlopeSlideVelocity(LayerMask layers)
        {
            Vector3 startPosition = new Vector3(transform.position.x, transform.position.y + slopeSlideStartPositionYOffset, transform.position.z);

            //  USE A SPHERE CAST TO DETERMINE THE ANGLE OF WHATS BELOW US, AND IF THE ANGLE IS TOO GREAT, WE ADJUST OUR SLOPE SLIDE VELOCITY
            if (Physics.SphereCast
                (startPosition, groundCheckSphereRadius, Vector3.down, out RaycastHit hitinfo, slopeSlideSphereCastMaxDistance, layers))
            {
                float angle = Vector3.Angle(hitinfo.normal, Vector3.up);
                slopeSlideVelocity = Vector3.ProjectOnPlane(new Vector3(0, slopeSlideSpeed, 0), hitinfo.normal);

                if (angle >= slipperySurfaceMaxAngle)
                {
                    slopeSlideVelocity = Vector3.ProjectOnPlane(new Vector3(0, slopeSlideSpeed, 0), hitinfo.normal);
                    return;
                }
            }
            //  OTHERWISE, SET SLOPE SLIDE VELOCITY TO ZERO
            else
            {
                slopeSlideVelocity = Vector3.zero;
            }

            if (isSliding)
            {
                slopeSlideVelocity -= slopeSlideVelocity * Time.deltaTime * slopeSlideSpeedMultiplier;

                if (slopeSlideVelocity.magnitude > 1)
                    return;
            }

            slopeSlideVelocity = Vector3.zero;
        }

        private void SetGroundedVelocity()
        {
            //  TO DO IF IGNORING GRAVITY, DO NOT PROCESS THIS FUNCTION

            if (slopeSlideVelocity != Vector3.zero)
            {
                //  IF YOU ARE IN THE PROCESSING OF JUMPING, AND YOUR JUMP IS STILL GAINING HEIGHT, DO NOT SLIDE OFF SURFACES
                if (character.characterNetworkManager.isJumping.Value && yVelocity.y > 0)
                {
                    isSliding = false;
                }
                else
                {
                    isSliding = true;
                }
            }

            if (isSliding)
            {
                yVelocity.y += WorldUtilityManager.Instance.slopeSlideForce * Time.deltaTime;
                Vector3 slideVelocity = slopeSlideVelocity;

                if (character.characterController.enabled)
                    character.characterController.Move(slideVelocity * Time.deltaTime);
            }

            if (isGrounded)
            {
                if (yVelocity.y <= 0 && !isSliding)
                    yVelocity.y = groundedYVelocity;
            }
            else
            {
                //  HANDLE SLIDING OFF A CHARACTER
            }

            if (!character.characterController.enabled)
                return;

            //  THIS IS A DESYNC PREVENTION MEASURE
            if (!character.IsOwner)
            {
                float distance = Vector3.Distance(transform.position, character.characterNetworkManager.networkPosition.Value);

                if (distance > 2.5f)
                {
                    yVelocity = Vector3.zero;
                    character.transform.position = character.characterNetworkManager.networkPosition.Value;
                }
            }
        }
    }
}