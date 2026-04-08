using UnityEngine;

namespace LZ
{
    public class CharacterLocomotionManager : MonoBehaviour
    {
        CharacterManager character;

        [Header("Flags")]
        public bool isRolling = false;
        public bool canRotate = true;
        public bool canMove = true;
        public bool canRun = true;
        public bool canRoll = true;
        public bool isGrounded = true;

        [Header("In Air")]
        [SerializeField] protected float inAirTimer = 0;

        private bool _wasGroundedLastFrame = true;

        protected virtual void Awake()
        {
            character = GetComponent<CharacterManager>();
        }

        protected virtual void Update()
        {
            if (character.kcc != null)
            {
                bool grounded = character.kcc.IsGrounded;

                if (grounded && !_wasGroundedLastFrame)
                    OnIsGrounded();
                else if (!grounded && _wasGroundedLastFrame)
                    OnIsNotGrounded();

                isGrounded = grounded;
                _wasGroundedLastFrame = grounded;

                if (isGrounded)
                    inAirTimer = 0;
                else
                    inAirTimer += Time.deltaTime;
            }

            HandleDesyncPrevention();
        }

        private void HandleDesyncPrevention()
        {
            if (character.IsOwner)
                return;

            Vector3 targetPos = character.characterNetworkManager.networkPosition.Value;
            float distance = Vector3.Distance(transform.position, targetPos);

            if (distance > 2.5f)
            {
                if (character.motor != null)
                    character.motor.SetPosition(targetPos);
                else
                    character.transform.position = targetPos;
                character.characterNetworkManager.networkPositionVelocity = Vector3.zero;
            }
            else if (distance > 0.01f)
            {
                Vector3 smoothed = Vector3.SmoothDamp(
                    transform.position, targetPos,
                    ref character.characterNetworkManager.networkPositionVelocity,
                    character.characterNetworkManager.networkPositionSmoothTime);

                if (character.motor != null)
                    character.motor.SetPosition(smoothed);
                else
                    character.transform.position = smoothed;
            }
        }

        public void EnableCanRotate()
        {
            canRotate = true;
        }
        
        public void DisableCanRotate()
        {
            canRotate = false;
        }

        protected virtual void OnIsGrounded()
        {
        }

        protected virtual void OnIsNotGrounded()
        {
        }
    }
}
