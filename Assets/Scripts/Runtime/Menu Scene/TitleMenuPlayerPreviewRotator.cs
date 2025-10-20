using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class TitleMenuPlayerPreviewRotator : MonoBehaviour
    {
        PlayerControls playerControls;

        [Header("Camera Input")]
        [SerializeField] private Vector2 cameraInput;
        [SerializeField] private float horizontalInput;

        [Header("Rotation")]
        [SerializeField] private float lookAngle;
        [SerializeField] private float rotationSpeed = 5;

        private void OnEnable()
        {
            if (playerControls == null)
            {
                playerControls = new PlayerControls();

                playerControls.PlayerCamera.Movement.performed += i => cameraInput = i.ReadValue<Vector2>();
            }

            playerControls.Enable();
        }

        private void OnDisable()
        {
            playerControls.Disable();
        }

        private void Update()
        {
            horizontalInput = cameraInput.x;

            lookAngle += (horizontalInput * rotationSpeed) * Time.deltaTime;
            Vector3 cameraRotation = Vector3.zero;
            cameraRotation.y = lookAngle;
            Quaternion targetRotation = Quaternion.Euler(cameraRotation);
            transform.rotation = targetRotation;
        }
    }
}
