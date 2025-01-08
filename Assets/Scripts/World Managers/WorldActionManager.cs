using System;
using UnityEngine;

namespace LZ
{
    public class WorldActionManager : MonoBehaviour
    {
        public static WorldActionManager instance;

        [Header("Weapon Item Actions")]
        public WeaponItemAction weaponItemActions;
        
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
    }
}