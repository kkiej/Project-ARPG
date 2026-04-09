using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

namespace LZ
{
    /// <summary>
    /// KCC 桥接层 — 实现 ICharacterController 接口，
    /// 接收来自 CharacterLocomotionManager / PlayerLocomotionManager 的移动意图，
    /// 转换为 KinematicCharacterMotor 要求的速度和旋转。
    ///
    /// 阶段 0：空壳，所有回调直接透传/空实现。
    /// 后续阶段会逐步迁入重力、地面检测、坡道、跳跃等逻辑。
    /// </summary>
    public class KCCCharacterController : MonoBehaviour, ICharacterController
    {
        [Header("References")]
        public KinematicCharacterMotor motor;

        [Header("Gravity")]
        public Vector3 gravity = new Vector3(0, -5.55f, 0);

        [Header("Collision")]
        public List<Collider> ignoredColliders = new List<Collider>();
        [Tooltip("撞到其他 KCC 角色时施加的推力，0 = 不推")]
        public float pushForceOnCharacters = 2f;

        // ── 外部写入的移动意图 ──
        [HideInInspector] public Vector3 moveVelocity;
        [HideInInspector] public Quaternion targetRotation;
        [HideInInspector] public bool useDirectRotation;

        // ── Root Motion 累积量（由 OnAnimatorMove 写入）──
        [HideInInspector] public Vector3 rootMotionDelta;
        [HideInInspector] public Quaternion rootMotionRotationDelta = Quaternion.identity;
        [HideInInspector] public bool useRootMotion;

        // ── 跳跃请求（由 PlayerLocomotionManager 写入）──
        [HideInInspector] public bool jumpRequested;
        [HideInInspector] public float jumpUpSpeed;
        [HideInInspector] public Vector3 jumpScalableDirection;

        // ── 附加速度（外部击退等）──
        private Vector3 _internalVelocityAdd;

        private void Start()
        {
            if (motor == null)
                motor = GetComponent<KinematicCharacterMotor>();

            motor.CharacterController = this;
        }

        #region Public API

        /// <summary>添加一次性速度（击退、弹射等）。</summary>
        public void AddVelocity(Vector3 velocity)
        {
            _internalVelocityAdd += velocity;
        }

        /// <summary>是否稳定站在地面上。</summary>
        public bool IsGrounded => motor != null && motor.GroundingStatus.IsStableOnGround;

        /// <summary>KCC Capsule 的半径。</summary>
        public float CapsuleRadius => motor != null ? motor.Capsule.radius : 0.5f;

        /// <summary>
        /// 设置本帧的目标旋转（平滑旋转场景，每帧调用一次）。
        /// KCC 会在 UpdateRotation 中应用，AfterCharacterUpdate 自动重置标志。
        /// </summary>
        public void SetTargetRotation(Quaternion rotation)
        {
            targetRotation = rotation;
            useDirectRotation = true;
        }

        /// <summary>
        /// 立即设置旋转（一次性场景：闪避朝向、传送等），绕过 UpdateRotation。
        /// </summary>
        public void SetRotationImmediate(Quaternion rotation)
        {
            if (motor != null)
                motor.SetRotation(rotation);
        }

        #endregion

        #region ICharacterController

        public void BeforeCharacterUpdate(float deltaTime)
        {
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (useRootMotion)
            {
                currentRotation = rootMotionRotationDelta * currentRotation;
            }
            else if (useDirectRotation)
            {
                currentRotation = targetRotation;
            }
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (useRootMotion)
            {
                if (deltaTime > 0)
                {
                    currentVelocity = rootMotionDelta / deltaTime;
                    if (motor.GroundingStatus.IsStableOnGround)
                        currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
                }
                else
                {
                    currentVelocity = Vector3.zero;
                }
            }
            else
            {
                if (motor.GroundingStatus.IsStableOnGround)
                {
                    currentVelocity = moveVelocity;
                    if (currentVelocity.sqrMagnitude > 0f)
                        currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;
                }
                else
                {
                    // 空中：保留纵向速度（重力/跳跃积累），水平分量直接替换为输入速度
                    float verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                    currentVelocity = moveVelocity + motor.CharacterUp * verticalSpeed;
                    currentVelocity += gravity * deltaTime;
                }
            }

            // 跳跃
            if (jumpRequested)
            {
                motor.ForceUnground();
                currentVelocity += motor.CharacterUp * jumpUpSpeed;
                if (jumpScalableDirection.sqrMagnitude > 0f)
                    currentVelocity += jumpScalableDirection;
                jumpRequested = false;
            }

            // 附加速度
            if (_internalVelocityAdd.sqrMagnitude > 0f)
            {
                currentVelocity += _internalVelocityAdd;
                _internalVelocityAdd = Vector3.zero;
            }
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            rootMotionDelta = Vector3.zero;
            rootMotionRotationDelta = Quaternion.identity;
            useRootMotion = false;
            useDirectRotation = false;
            moveVelocity = Vector3.zero;
        }

        public void PostGroundingUpdate(float deltaTime)
        {
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            if (ignoredColliders.Contains(coll))
                return false;
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            // 角色互推：当撞到另一个 KCC 角色时，给对方施加一个轻微推力
            if (pushForceOnCharacters > 0f)
            {
                var otherKCC = hitCollider.GetComponentInParent<KCCCharacterController>();
                if (otherKCC != null && otherKCC != this)
                {
                    Vector3 pushDir = (hitCollider.transform.position - motor.TransientPosition).normalized;
                    pushDir.y = 0;
                    otherKCC.AddVelocity(pushDir * pushForceOnCharacters);
                }
            }
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }

        #endregion
    }
}
