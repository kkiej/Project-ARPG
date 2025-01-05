using System;
using UnityEngine;

namespace LZ
{
    public class TitleScreenLoadMenuInputManager : MonoBehaviour
    {
        private PlayerControls playerControls;
        
        [Header("Title Screen Inputs")]
        [SerializeField] private bool deleteCharacterSlot = false;

        private void Update()
        {
            if (deleteCharacterSlot)
            {
                deleteCharacterSlot = false;
                TitleScreenManager.instance.AttemptToDeleteCharacterSlot();
            }
        }

        private void OnEnable()
        {
            if (playerControls == null)
            {
                playerControls = new PlayerControls();
                playerControls.UI.X.performed += i => deleteCharacterSlot = true;
            }
            
            playerControls.Enable();
        }

        private void OnDisable()
        {
            playerControls.Disable();
        }
    }
}