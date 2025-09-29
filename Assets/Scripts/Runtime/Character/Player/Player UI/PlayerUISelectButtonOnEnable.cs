using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LZ
{
    public class PlayerUISelectButtonOnEnable : MonoBehaviour
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            button.Select();
            // on older versions of unity .Select would sometimes not fire "OnSelected" events on the button so using this afterwards fixed that
            button.OnSelect(null);
        }
    }
}