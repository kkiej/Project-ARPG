using System;
using Unity.Netcode;
using UnityEngine;

namespace LZ
{
    public class CharacterManager : NetworkBehaviour
    {
        public CharacterController characterController;

        private CharacterNetworkManager characterNetworkManager;

        private Transform currentTransform;
        
        protected virtual void Awake()
        {
            DontDestroyOnLoad(this);

            characterController = GetComponent<CharacterController>();
            characterNetworkManager = GetComponent<CharacterNetworkManager>();
        }

        protected virtual void Update()
        {
            currentTransform = transform;
            // if this character is being controlled from our side, then assign its network position to the position of our transform
            if (IsOwner)
            {
                characterNetworkManager.networkPosition.Value = currentTransform.position;
                characterNetworkManager.networkRotation.Value = currentTransform.rotation;
            }
            // if this character is being controlled from else where, then assign its position here locally by the position of its network transform
            else
            {
                transform.position = Vector3.SmoothDamp(currentTransform.position,
                    characterNetworkManager.networkPosition.Value,
                    ref characterNetworkManager.networkPositionVelocity,
                    characterNetworkManager.networkPositionSmoothTime);

                transform.rotation = Quaternion.Slerp(currentTransform.rotation,
                    characterNetworkManager.networkRotation.Value,
                    characterNetworkManager.networkRotationSmoothTime);
            }
        }

        protected virtual void LateUpdate()
        {
            
        }
    }
}