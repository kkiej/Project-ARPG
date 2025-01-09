using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LZ
{
    public class CharacterManager : NetworkBehaviour
    {
        [Header("Status")]
        public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        
        [HideInInspector] public CharacterController characterController;
        [HideInInspector] public Animator animator;

        [HideInInspector] public CharacterNetworkManager characterNetworkManager;
        [HideInInspector] public CharacterEffectsManager characterEffectsManager;
        [HideInInspector] public CharacterAnimatorManager characterAnimatorManager;
        [HideInInspector] public CharacterCombatManager characterCombatManager;

        [Header("Flags")]
        public bool isPerformingAction;
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
            characterEffectsManager = GetComponent<CharacterEffectsManager>();
            characterAnimatorManager = GetComponent<CharacterAnimatorManager>();
            characterCombatManager = GetComponent<CharacterCombatManager>();
        }

        protected virtual void Start()
        {
            IgnoreMyOwnColliders();
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

        public virtual IEnumerator ProcessDeathEvent(bool manuallySelectDeathAnimation = false)
        {
            if (IsOwner)
            {
                characterNetworkManager.currentHealth.Value = 0;
                isDead.Value = true;
                
                // 重置任何需要重置的标志
                // 目前没有
                
                // 如果我们不在地面上，播放空中死亡动画

                if (!manuallySelectDeathAnimation)
                {
                    characterAnimatorManager.PlayTargetActionAnimation("Dead_01", true);
                }
            }
            
            // 播放死亡音效

            yield return new WaitForSeconds(5);
            
            // 用符文奖励玩家
            
            // 禁用角色模型
        }

        public virtual void ReviveCharacter()
        {
            
        }

        protected virtual void IgnoreMyOwnColliders()
        {
            Collider characterControllerCollider = GetComponent<Collider>();
            Collider[] damageableCharacterColliders = GetComponentsInChildren<Collider>();
            List<Collider> ignoreColliders = new List<Collider>();
            
            // 将我们所有的可造成伤害的角色碰撞体添加到将用于忽略碰撞的列表中
            foreach (var collider in damageableCharacterColliders)
            {
                ignoreColliders.Add(collider);
            }
            
            // 将我们的角色控制器碰撞体添加到将用于忽略碰撞的列表中
            ignoreColliders.Add(characterControllerCollider);
            
            // 遍历列表中所有的碰撞体，互相忽略碰撞
            foreach (var collider in ignoreColliders)
            {
                foreach (var otherCollider in ignoreColliders)
                {
                    Physics.IgnoreCollision(collider, otherCollider, true);
                }
            }
        }
    }
}