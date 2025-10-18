using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace LZ
{
    public class PlayerCamera : MonoBehaviour
    {
        public static PlayerCamera instance;
        public PlayerManager player;
        public Camera cameraObject;
        public Transform cameraPivotTransform;
        public float cameraPivotYPositionOffSet = 1.5f;

        // Change these to tweak camera performance
        [Header("Camera Settings")]
        [SerializeField] private float cameraSmoothSpeed = 1f; // the bigger this number, the longer for the camera to reach its position during movement
        [SerializeField] private float leftAndRightRotationSpeed = 220;
        [SerializeField] private float upAndDownRotationSpeed = 220;
        [SerializeField] private float minimumPivot = -30;
        [SerializeField] private float maximumPivot = 60;
        [SerializeField] private float cameraCollisionRadius = 0.2f;
        [SerializeField] private LayerMask collideWithLayers;

        [Header("Camera Values")]
        private Vector3 cameraVelocity;
        private Vector3 cameraObjectPosition; // used for camera collisions (moves the camera object to this position upon colliding)
        [SerializeField] private float leftAndRightLookAngle;
        [SerializeField] private float upAndDownLookAngle;
        private float cameraZPosition;
        private float targetCameraZPosition;

        [Header("Lock On")]
        [SerializeField] private float lockOnRadius = 20;
        [SerializeField] private float minimumViewableAngle = -50;
        [SerializeField] private float maximumViewableAngle = 50;
        [SerializeField] private float lockOnTargetFollowSpeed = 0.2f;
        [SerializeField] private float setCameraHeightSpeed = 1;
        [SerializeField] private float unlockedCameraHeight = 1.65f;
        [SerializeField] private float lockedCameraHeight = 2.0f;
        private Coroutine cameraLockOnHeightCoroutine;
        private List<CharacterManager> availableTargets = new List<CharacterManager>();
        public CharacterManager nearestLockOnTarget;
        public CharacterManager leftLockOnTarget;
        public CharacterManager rightLockOnTarget;

        [Header("Ranged Aim")]
        private Transform followTransformWhenAiming;
        public Vector3 aimDirection;

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
            cameraZPosition = cameraObject.transform.localPosition.z;
        }

        public void HandleAllCameraActions()
        {
            if (player != null)
            {
                HandleFollowTarget();
                HandleRotations();
                HandleCollisions();
            }
        }

        private void HandleFollowTarget()
        {
            if (player.playerNetworkManager.isAiming.Value)
            {
                Vector3 targetCameraPosition = Vector3.SmoothDamp(transform.position, player.playerCombatManager.lockOnTransform.position, ref cameraVelocity, cameraSmoothSpeed * Time.deltaTime);
                transform.position = targetCameraPosition;
            }
            else
            {
                Vector3 targetCameraPosition = Vector3.SmoothDamp(transform.position, player.transform.position, ref cameraVelocity, cameraSmoothSpeed * Time.deltaTime);
                transform.position = targetCameraPosition;
            }
        }

        private void HandleRotations()
        {
            if (player.playerNetworkManager.isAiming.Value)
            {
                HandleAimRotations();
            }
            else
            {
                HandleStandardRotations();
            }
        }

        private void HandleAimRotations()
        {
            if (!player.playerLocomotionManager.isGrounded)
                player.playerNetworkManager.isAiming.Value = false;

            if (player.isPerformingAction)
                return;

            aimDirection = cameraObject.transform.forward.normalized;

            //  LEFT AND RIGHT LOOK
            Vector3 cameraRotationY = Vector3.zero;
            //  UP AND DOWN LOOK
            Vector3 cameraRotationX = Vector3.zero;

            leftAndRightLookAngle += (PlayerInputManager.instance.cameraHorizontalInput * leftAndRightRotationSpeed) * Time.deltaTime;
            upAndDownLookAngle -= (PlayerInputManager.instance.cameraVerticalInput * upAndDownRotationSpeed) * Time.deltaTime;
            upAndDownLookAngle = Mathf.Clamp(upAndDownLookAngle, minimumPivot, maximumPivot);

            cameraRotationY.y = leftAndRightLookAngle;
            cameraRotationX.x = upAndDownLookAngle;

            cameraObject.transform.localEulerAngles = new Vector3(upAndDownLookAngle, leftAndRightLookAngle, 0);
        }

        private void HandleStandardRotations()
        {
            //  IF LOCKED ON, FORCE ROTATION TOWARDS TARGET
            if (player.playerNetworkManager.isLockedOn.Value)
            {
                //  THIS ROTATES THIS GAMEOBJECT
                Vector3 rotationDirection = player.playerCombatManager.currentTarget.characterCombatManager.lockOnTransform.position - transform.position;
                rotationDirection.Normalize();
                rotationDirection.y = 0;
                Quaternion targetRotation = Quaternion.LookRotation(rotationDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lockOnTargetFollowSpeed);

                //  THIS ROTATES THE PIVOT OBJECT
                rotationDirection = player.playerCombatManager.currentTarget.characterCombatManager.lockOnTransform.position - cameraPivotTransform.position;
                rotationDirection.Normalize();

                targetRotation = Quaternion.LookRotation(rotationDirection);
                cameraPivotTransform.transform.rotation = Quaternion.Slerp(cameraPivotTransform.rotation, targetRotation, lockOnTargetFollowSpeed);

                //  SAVE OUR ROTATIONS TO OUR LOOK ANGLES, SO WHEN WE UNLOCK IT DOESNT SNAP TOO FAR AWAY
                leftAndRightLookAngle = transform.eulerAngles.y;
                upAndDownLookAngle = transform.eulerAngles.x;
            }
            //  ELSE ROTATE REGULARLY
            else
            {
                //  ROTATE LEFT AND RIGHT BASED ON HORIZONTAL MOVEMENT ON THE RIGHT JOYSTICK
                leftAndRightLookAngle += (PlayerInputManager.instance.cameraHorizontalInput * leftAndRightRotationSpeed) * Time.deltaTime;
                //  ROTATE UP AND DOWN BASED ON VERTICAL MOVEMENT ON THE RIGHT JOYSTICK
                upAndDownLookAngle -= (PlayerInputManager.instance.cameraVerticalInput * upAndDownRotationSpeed) * Time.deltaTime;
                //  CLAMP THE UP AND DOWN LOOK ANGLE BETWEEN A MIN AND MAX VALUE
                upAndDownLookAngle = Mathf.Clamp(upAndDownLookAngle, minimumPivot, maximumPivot);

                Vector3 cameraRotation = Vector3.zero;
                Quaternion targetRotation;

                // Rotate this gameObject left and right
                cameraRotation.y = leftAndRightLookAngle;
                targetRotation = Quaternion.Euler(cameraRotation);
                transform.rotation = targetRotation;

                // Rotate pivot gameObject up and down
                cameraRotation = Vector3.zero;
                cameraRotation.x = upAndDownLookAngle;
                targetRotation = Quaternion.Euler(cameraRotation);
                cameraPivotTransform.localRotation = targetRotation;
            }
        }

        private void HandleCollisions()
        {
            targetCameraZPosition = cameraZPosition;
            RaycastHit hit;
            // direction for collision check
            Vector3 direction = cameraObject.transform.position - cameraPivotTransform.position;
            direction.Normalize();

            //  WE CHECK IF THERE IS AN OBJECT IN FRONT OF OUR DESIRED DIRECTION ^ (SEE ABOVE)
            if (Physics.SphereCast(cameraPivotTransform.position, cameraCollisionRadius, direction, out hit, Mathf.Abs(targetCameraZPosition), collideWithLayers))
            {
                // if there is, we get our distance from it
                float distanceFromHitObject = Vector3.Distance(cameraPivotTransform.position, hit.point);
                // we then equate our target z position to the following
                targetCameraZPosition = -(distanceFromHitObject - cameraCollisionRadius);
            }

            // if our target position is less than our collision radius, we subtract our collision radius (making it snap back)
            if (Mathf.Abs(targetCameraZPosition) < cameraCollisionRadius)
            {
                targetCameraZPosition = -cameraCollisionRadius;
            }

            //  WE THEN APPLY OUR FINAL POSITION USING A LERP OVER A TIME OF 0.2F

            if (player.playerNetworkManager.isAiming.Value)
            {
                cameraObjectPosition.z = 0;
                cameraObject.transform.localPosition = cameraObjectPosition;
                return;
            }

            cameraObjectPosition.z = Mathf.Lerp(cameraObject.transform.localPosition.z, targetCameraZPosition, 0.2f);
            cameraObject.transform.localPosition = cameraObjectPosition;
        }

        public void HandleLocatingLockOnTargets()
        {
            float shortestDistance = Mathf.Infinity; // 用于确定离我们最近的对象
            float shortestDistanceOfRightTarget = Mathf.Infinity; // 用于确定当前对象右侧单一轴向距离最近的目标（当前对象右侧最近的对象）
            float shortestDistanceOfLeftTarget = -Mathf.Infinity; // 用于确定当前对象左侧单一轴向距离最近的目标（当前对象右侧最近的对象）
            
            // TODO:使用LayerMask
            Collider[] colliders = Physics.OverlapSphere(player.transform.position, lockOnRadius,
                WorldUtilityManager.Instance.GetCharacterLayers());

            for (int i = 0; i < colliders.Length; i++)
            {
                CharacterManager lockOnTarget = colliders[i].GetComponent<CharacterManager>();

                if (lockOnTarget != null)
                {
                    // 检查他们是否在我们的可视范围内
                    Vector3 lockOnTargetsDirection = lockOnTarget.transform.position - player.transform.position;
                    float distanceFromTarget =
                        Vector3.Distance(player.transform.position, lockOnTarget.transform.position);
                    float viewableAngle = Vector3.Angle(lockOnTargetsDirection, cameraObject.transform.forward);
                    
                    // 如果对象死亡，检查下一个潜在对象
                    if (lockOnTarget.isDead.Value)
                        continue;
                    
                    // 如果对象是自己，检查下一个
                    if (lockOnTarget.transform.root == player.transform.root)
                        continue;
                    
                    // 最后如果对象在视野之外或者被环境遮挡，检查下一个
                    if (viewableAngle > minimumViewableAngle && viewableAngle < maximumViewableAngle)
                    {
                        RaycastHit hit;
                        
                        // TODO: 添加只有环境的LayerMask
                        if (Physics.Linecast(player.playerCombatManager.lockOnTransform.position,
                                lockOnTarget.characterCombatManager.lockOnTransform.position, out hit,
                                WorldUtilityManager.Instance.GetEnvironLayers()))
                        {
                            // 我们击中某物，但是看不见
                            continue;
                        }
                        else
                        {
                            //Debug.Log("We made it.");
                            // 否则，将他们添加到潜在列表
                            availableTargets.Add(lockOnTarget);
                        }
                    }
                }
            }
            
            // 排序我们的潜在对象，看看我们先锁哪一个
            for (int i = 0; i < availableTargets.Count; i++)
            {
                if (availableTargets[i] != null)
                {
                    float distanceFromTarget =
                        Vector3.Distance(player.transform.position, availableTargets[i].transform.position);

                    if (distanceFromTarget < shortestDistance)
                    {
                        shortestDistance = distanceFromTarget;
                        nearestLockOnTarget = availableTargets[i];
                    }
                    
                    // 如果我们在搜索目标时已经锁定了目标，那么搜索左右的目标
                    if (player.playerNetworkManager.isLockedOn.Value)
                    {
                        Vector3 relativeEnemyPosition =
                            player.transform.InverseTransformPoint(availableTargets[i].transform.position);

                        var distanceFromLeftTarget = relativeEnemyPosition.x;
                        var distanceFromRightTarget = relativeEnemyPosition.x;

                        if (availableTargets[i] == player.playerCombatManager.currentTarget)
                            continue;
                        
                        // 检查左边的目标
                        if (relativeEnemyPosition.x <= 0.00f && distanceFromLeftTarget > shortestDistanceOfLeftTarget)
                        {
                            shortestDistanceOfLeftTarget = distanceFromLeftTarget;
                            leftLockOnTarget = availableTargets[i];
                        }
                        // 检查右边的目标
                        else if (relativeEnemyPosition.x >= 0.00f && distanceFromRightTarget < shortestDistanceOfRightTarget)
                        {
                            shortestDistanceOfRightTarget = distanceFromRightTarget;
                            rightLockOnTarget = availableTargets[i];
                        }
                    }
                }
                else
                {
                    ClearLockOnTargets();
                    player.playerNetworkManager.isLockedOn.Value = false;
                }
            }
        }

        public void SetLockCameraHeight()
        {
            if (cameraLockOnHeightCoroutine != null)
                StopCoroutine(cameraLockOnHeightCoroutine);

            cameraLockOnHeightCoroutine = StartCoroutine(SetCameraHeight());
        }

        public void ClearLockOnTargets()
        {
            nearestLockOnTarget = null;
            leftLockOnTarget = null;
            rightLockOnTarget = null;
            availableTargets.Clear();
        }

        public IEnumerator WaitThenFindNewTarget()
        {
            while (player.isPerformingAction)
            {
                yield return null;
            }
            
            ClearLockOnTargets();
            HandleLocatingLockOnTargets();

            if (nearestLockOnTarget != null)
            {
                player.playerCombatManager.SetTarget(nearestLockOnTarget);
                player.playerNetworkManager.isLockedOn.Value = true;
            }

            yield return null;
        }

        private IEnumerator SetCameraHeight()
        {
            float duration = 1;
            float timer = 0;

            Vector3 velocity = Vector3.zero;
            Vector3 newLockedCameraHeight =
                new Vector3(cameraPivotTransform.transform.localPosition.x, lockedCameraHeight);
            Vector3 newUnlockedCameraHeight =
                new Vector3(cameraPivotTransform.transform.localPosition.x, unlockedCameraHeight);

            while (timer < duration)
            {
                timer += Time.deltaTime;

                if (player != null)
                {
                    if (player.playerCombatManager.currentTarget != null)
                    {
                        cameraPivotTransform.transform.localPosition = Vector3.SmoothDamp(
                            cameraPivotTransform.transform.localPosition, newLockedCameraHeight, ref velocity,
                            setCameraHeightSpeed);

                        cameraPivotTransform.transform.localRotation = Quaternion.Slerp(
                            cameraPivotTransform.transform.localRotation, Quaternion.Euler(0, 0, 0),
                            lockOnTargetFollowSpeed);
                    }
                    else
                    {
                        cameraPivotTransform.transform.localPosition = Vector3.SmoothDamp(
                            cameraPivotTransform.transform.localPosition, newUnlockedCameraHeight, ref velocity,
                            setCameraHeightSpeed);
                    }
                }

                yield return null;
            }

            if (player != null)
            {
                if (player.playerCombatManager.currentTarget != null)
                {
                    cameraPivotTransform.transform.localPosition = newLockedCameraHeight;
                    cameraPivotTransform.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
                else
                {
                    cameraPivotTransform.transform.localPosition = newUnlockedCameraHeight;
                }
            }

            yield return null;
        }
    }
}