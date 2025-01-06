using System;
using UnityEngine;

namespace LZ
{
    public class CharacterLocomotionManager : MonoBehaviour
    {
        private CharacterManager character;

        [Header("Ground Check & Jumping")]
        [SerializeField] protected float gravityForce = -5.55f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckSphereRadius = 1;
        [SerializeField] protected Vector3 yVelocity; // 我们角色被向上或向下拉（跳跃或下落）的力
        [SerializeField] protected float groundedYVelocity = -20; // 我们的角色在着地时粘附于地面的力
        [SerializeField] protected float fallStartYVelocity = -5; // 当角色失去地面接触时开始下落的力（随着下落时间的增加而增大）
        protected bool fallingVelocityHasBeenSet;
        protected float inAirTimer = 0;
        
        protected virtual void Awake()
        {
            character = GetComponent<CharacterManager>();
        }

        protected virtual void Update()
        {
            HandleGroundCheck();

            if (character.isGrounded)
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
                if (!character.isJumping && !fallingVelocityHasBeenSet)
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
            character.isGrounded =
                Physics.CheckSphere(character.transform.position, groundCheckSphereRadius, groundLayer);
        }

        protected void OnDrawGizmosSelected()
        {
            Gizmos.DrawSphere(character.transform.position, groundCheckSphereRadius);
        }
    }
}