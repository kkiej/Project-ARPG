using Unity.Netcode;
using UnityEngine;

namespace LZ
{
    public class CharacterManager : NetworkBehaviour
    {
        [HideInInspector] public CharacterController characterController;
        [HideInInspector] public Animator animator;

        [HideInInspector] public CharacterNetworkManager characterNetworkManager;

        [Header("Flags")]
        public bool isPerformingAction;
        public bool isJumping;
        public bool isGrounded = true;
        public bool applyRootMotion;
        public bool canRotate = true;
        public bool canMove = true;
        
        protected virtual void Awake()
        {
            DontDestroyOnLoad(this);

            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            characterNetworkManager = GetComponent<CharacterNetworkManager>();
        }

        protected virtual void Update()
        {
            animator.SetBool("isGrounded", isGrounded);
            
            // 如果这个角色是由我们控制的，那么将其网络位置赋值为我们变换的位置
            if (IsOwner)
            {
                characterNetworkManager.networkPosition.Value = transform.position;
                characterNetworkManager.networkRotation.Value = transform.rotation;
            }
            // 如果这个角色是由其他地方控制的，那么将其位置通过网络变换的位置在本地赋值
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position,
                    characterNetworkManager.networkPosition.Value,
                    ref characterNetworkManager.networkPositionVelocity,
                    characterNetworkManager.networkPositionSmoothTime);

                transform.rotation = Quaternion.Slerp(transform.rotation,
                    characterNetworkManager.networkRotation.Value,
                    characterNetworkManager.networkRotationSmoothTime);
            }
        }

        protected virtual void LateUpdate()
        {
            
        }
    }
}