using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    //  USED FOR BACKSTAB AND RIPOSTE AT SPECIFIC ANIMATION FRAMES
    [CreateAssetMenu(menuName = "Character Effects/Instant Effects/Take Critical Damage Effect")]
    public class TakeCriticalDamageEffect : TakeDamageEffect
    {
        public override void ProcessEffect(CharacterManager character)
        {
            if (character.characterNetworkManager.isInvulnerable.Value)
                return;

            //  IF THE CHARACTER IS DEAD, NO ADDITIONAL DAMAGE EFFECTS SHOULD BE PROCESSED
            if (character.isDead.Value)
                return;

            CalculateDamage(character);

            character.characterCombatManager.pendingCriticalDamage = finalDamageDealt;
        }

        protected override void CalculateDamage(CharacterManager character)
        {
            if (!character.IsOwner)
                return;

            if (characterCausingDamage != null)
            {
                //  CHECK FOR DAMAGE MODIFIERS AND MODIFY BASE DAMAGE (PHYSICAL/ELEMENTAL DAMAGE BUFF)
            }

            //  CHECK CHARACTER FOR FLAT DEFENSES AND SUBTRACT THEM FROM THE DAMAGE

            //  CHECK CHARACTER FOR ARMOR ABSORPTIONS, AND SUBTRACT THE PERCENTAGE FROM THE DAMAGE

            //  ADD ALL DAMAGE TYPES TOGETHER, AND APPLY FINAL DAMAGE
            finalDamageDealt = Mathf.RoundToInt(physicalDamage + magicDamage + fireDamage + lightningDamage + holyDamage);

            if (finalDamageDealt <= 0)
                finalDamageDealt = 1;

            //  CALCULATE POISE DAMAGE TO DETERMINE IF THE CHARACTER WILL BE STUNNED

            //  WE SUBJECT POISE DAMAGE FROM THE CHARACTERS TOTAL
            character.characterStatsManager.totalPoiseDamage -= poiseDamage;

            //  WE STORE THE PREVIOUS POISE DAMAGE TAKEN FOR OTHER INTERACTIONS
            character.characterCombatManager.previousPoiseDamageTaken = poiseDamage;

            float remainingPoise = character.characterStatsManager.basePoiseDefense +
                character.characterStatsManager.offensivePoiseBonus +
                character.characterStatsManager.totalPoiseDamage;

            if (remainingPoise <= 0)
                poiseIsBroken = true;

            //  SINCE THE CHARACTER HAS BEEN HIT, WE RESET THE POISE TIMER
            character.characterStatsManager.poiseResetTimer = character.characterStatsManager.defaultPoiseResetTime;
        }
    }
}