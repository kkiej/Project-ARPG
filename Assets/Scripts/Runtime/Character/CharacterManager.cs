using System.Collections;
using System.Collections.Generic;
using Animancer;
using KinematicCharacterController;
using UnityEngine;
using Unity.Netcode;

namespace LZ
{
    public class CharacterManager : NetworkBehaviour
    {
        [Header("Flags")]
        public bool isPerformingAction = false;

        [Header("Status")]
        public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [HideInInspector] public KinematicCharacterMotor motor;
        [HideInInspector] public KCCCharacterController kcc;
        [HideInInspector] public Animator animator;
        [HideInInspector] public AnimancerComponent animancer;

        [HideInInspector] public CharacterNetworkManager characterNetworkManager;
        [HideInInspector] public CharacterEffectsManager characterEffectsManager;
        [HideInInspector] public CharacterAnimatorManager characterAnimatorManager;
        [HideInInspector] public CharacterCombatManager characterCombatManager;
        [HideInInspector] public CharacterStatsManager characterStatsManager;
        [HideInInspector] public CharacterSoundFXManager characterSoundFXManager;
        [HideInInspector] public CharacterLocomotionManager characterLocomotionManager;
        [HideInInspector] public CharacterUIManager characterUIManager;

        [Header("Character Group")]
        public CharacterGroup characterGroup;

        protected virtual void Awake()
        {
            motor = GetComponent<KinematicCharacterMotor>();
            kcc = GetComponent<KCCCharacterController>();
            animator = GetComponent<Animator>();
            animancer = GetComponent<AnimancerComponent>();

            characterNetworkManager = GetComponent<CharacterNetworkManager>();
            characterEffectsManager = GetComponent<CharacterEffectsManager>();
            characterAnimatorManager = GetComponent<CharacterAnimatorManager>();
            characterCombatManager = GetComponent<CharacterCombatManager>();
            characterStatsManager = GetComponent<CharacterStatsManager>();
            characterSoundFXManager = GetComponent<CharacterSoundFXManager>();
            characterLocomotionManager = GetComponent<CharacterLocomotionManager>();
            characterUIManager = GetComponent<CharacterUIManager>();
        }

        protected virtual void Start()
        {
            IgnoreMyOwnColliders();
        }

        protected virtual void Update()
        {
            
            // 如果这个角色是由我们控制的，那么将其网络位置赋值为我们变换的位置
            if (IsOwner)
            {
                characterNetworkManager.networkPosition.Value = transform.position;
                characterNetworkManager.networkRotation.Value = transform.rotation;
            }
            // 如果这个角色是由其他地方控制的，那么将其位置通过网络变换的位置在本地赋值
            else
            {
                Vector3 smoothedPos = Vector3.SmoothDamp(
                    transform.position,
                    characterNetworkManager.networkPosition.Value,
                    ref characterNetworkManager.networkPositionVelocity,
                    characterNetworkManager.networkPositionSmoothTime);

                Quaternion smoothedRot = Quaternion.Slerp(
                    transform.rotation,
                    characterNetworkManager.networkRotation.Value,
                    characterNetworkManager.networkRotationSmoothTime);

                if (motor != null)
                {
                    motor.SetPositionAndRotation(smoothedPos, smoothedRot);
                }
                else
                {
                    transform.position = smoothedPos;
                    transform.rotation = smoothedRot;
                }
            }
        }

        protected virtual void FixedUpdate()
        {
            
        }

        protected virtual void LateUpdate()
        {
            
        }

        protected virtual void OnEnable()
        {
            
        }
        
        protected virtual void OnDisable()
        {
            
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            characterNetworkManager.OnIsActiveChanged(false, characterNetworkManager.isActive.Value);

            isDead.OnValueChanged += characterNetworkManager.OnIsDeadChanged;
            characterNetworkManager.isMoving.OnValueChanged += characterNetworkManager.OnIsMovingChanged;
            characterNetworkManager.isActive.OnValueChanged += characterNetworkManager.OnIsActiveChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            isDead.OnValueChanged -= characterNetworkManager.OnIsDeadChanged;
            characterNetworkManager.isMoving.OnValueChanged -= characterNetworkManager.OnIsMovingChanged;
            characterNetworkManager.isActive.OnValueChanged -= characterNetworkManager.OnIsActiveChanged;
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

                if (!manuallySelectDeathAnimation && !characterNetworkManager.isBeingCriticallyDamaged.Value)
                {
                    var ad = characterAnimatorManager.animData;
                    if (ad != null && ad.dead != null)
                        characterAnimatorManager.PlayTargetActionAnimation(ad.dead, true);
                    else
                        Debug.LogWarning($"{name}: dead clip 未配置", this);
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
            Collider primaryCollider = GetComponent<Collider>();
            Collider[] damageableCharacterColliders = GetComponentsInChildren<Collider>();
            List<Collider> ignoreColliders = new List<Collider>();
            
            foreach (var collider in damageableCharacterColliders)
            {
                ignoreColliders.Add(collider);
            }
            
            if (primaryCollider != null && !ignoreColliders.Contains(primaryCollider))
                ignoreColliders.Add(primaryCollider);
            
            // Unity 物理层面互相忽略（标准碰撞检测 / trigger 等）
            foreach (var collider in ignoreColliders)
            {
                foreach (var otherCollider in ignoreColliders)
                {
                    if (collider != null && otherCollider != null)
                        Physics.IgnoreCollision(collider, otherCollider, true);
                }
            }

            // KCC 移动扫描层面忽略自身碰撞体（KCC 用 IsColliderValidForCollisions 过滤）
            if (kcc != null)
            {
                foreach (var collider in ignoreColliders)
                {
                    if (collider != null && !kcc.ignoredColliders.Contains(collider))
                        kcc.ignoredColliders.Add(collider);
                }
            }
        }
    }
}
