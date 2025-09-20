using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class AIDurkSoundFXManager : CharacterSoundFXManager
    {
        [Header("Club Whooshes")]
        public AudioClip[] clubWhooshes;
        
        [Header("Club Impacts")]
        public AudioClip[] clubImpacts;
        
        [Header("Stomp Impacts")]
        public AudioClip[] stompImpacts;
        
        public virtual void PlayClubImpactSoundFX()
        {
            if (clubImpacts.Length > 0)
                PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(clubImpacts));
        }
        
        public virtual void PlayStompImpactSoundFX()
        {
            if (stompImpacts.Length > 0)
                PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(stompImpacts));
        }
    }
}