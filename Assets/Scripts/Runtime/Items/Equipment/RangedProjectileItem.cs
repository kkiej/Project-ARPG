using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Items/Ranged Projectile")]
    public class RangedProjectileItem : Item
    {
        public ProjectileClass projectileClass;

        [Header("Velocity")]
        public float forwardVelocity = 2200;
        public float upwardVelocity = 0;
        public float ammoMass = 0.01f;

        [Header("Capacity")]
        public int maxAmmoAmount = 30;
        public int currentAmmoAmount = 30;

        [Header("Damage")]
        public int physicalDamage = 0;
        public int magicDamage = 0;
        public int fireDamage = 0;
        public int holyDamage = 0;
        public int lightningDamage = 0;

        [Header("Model")]
        public GameObject drawProjectileModel;
        public GameObject releaseProjectileModel;
    }
}
