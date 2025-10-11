using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class SpellManager : MonoBehaviour
    {
        [Header("Spell Target")]
        [SerializeField] protected CharacterManager spellTarget;

        [Header("VFX")]
        [SerializeField] protected GameObject impactParticle;             //  THE PARTICLES THAT SPAWN WHEN THIS PROJECTILE EXPLODES
        [SerializeField] protected GameObject impactParticleFullCharge;   //  THE PARTICLES THAT SPAWN WHEN THIS PROJECTILE EXPLODES, AFTER IT WAS FULLY CHARGED

        protected virtual void Awake()
        {

        }

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {

        }
    }
}
