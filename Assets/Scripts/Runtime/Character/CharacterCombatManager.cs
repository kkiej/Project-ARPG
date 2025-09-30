using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace LZ
{
    public class CharacterCombatManager : NetworkBehaviour
    {
        protected CharacterManager character;

        [Header("Last Attack Animation Performed")]
        public string lastAttackAnimationPerformed;

        [Header("Previous Poise Damage Taken")]
        public float previousPoiseDamageTaken;

        [Header("Attack Target")]
        public CharacterManager currentTarget;
        
        [Header("Attack Type")]
        public AttackType currentAttackType;

        [Header("Lock On Transform")]
        public Transform lockOnTransform;
        
        [Header("Attack Flags")]
        public bool canPerformRollingAttack = false;
        public bool canPerformBackstepAttack = false;
        public bool canBlock = true;

        [Header("Critical Attack")]
        private Transform riposteReceiverTransform;
        [SerializeField] float criticalAttackDistanceCheck = 0.7f;
        public int pendingCriticalDamage;

        protected virtual void Awake()
        {
            character = GetComponent<CharacterManager>();
        }

        public virtual void SetTarget(CharacterManager newTarget)
        {
            if (character.IsOwner)
            {
                if (newTarget != null)
                {
                    currentTarget = newTarget;
                    character.characterNetworkManager.currentTargetNetworkObjectID.Value = newTarget.GetComponent<NetworkObject>().NetworkObjectId;
                }
                else
                {
                    currentTarget = null;
                }
            }
        }

        //  USED TO ATTEMPT A BACKSTAB/RIPSOTE
        public virtual void AttemptCriticalAttack()
        {
            //  WE CANNOT PERFORM A CRITICAL STRIKE IF WE ARE PERFORMING ANOTHER ACTION
            if (character.isPerformingAction)
                return;

            //  WE CANNOT PERFORM A CRITICAL STRIKE IF WE ARE OUT OF STAMINA
            if (character.characterNetworkManager.currentStamina.Value <= 0)
                return;

            //  AIM A RAYCAST INFRONT OF US AND CHECK FOR ANY POTENTIAL TARGETS TO CRITICALLY ATTACK
            RaycastHit[] hits = Physics.RaycastAll(character.characterCombatManager.lockOnTransform.position,
                character.transform.TransformDirection(Vector3.forward), criticalAttackDistanceCheck, WorldUtilityManager.Instance.GetCharacterLayers());

            for (int i = 0; i < hits.Length; i++)
            {
                //  CHECK EACH OF THE HITS 1 BY 1, GIVING THEM THEIR OWN VARIABLE
                RaycastHit hit = hits[i];

                CharacterManager targetCharacter = hit.transform.GetComponent<CharacterManager>();

                if (targetCharacter != null)
                {
                    //  IF THE CHARACTER IS THE ONE ATTEMPTING THE CRITICAL STRIKE, GO TO THE NEXT HIT IN THE ARRAY OF TOTAL HITS
                    if (targetCharacter == character)
                        continue;

                    //  IF WE CANNOT DAMAGE THE CHARACTER THAT IS TARGETED CONTINUE TO CHECK THE NEXT HIT IN THE ARRAY OF HITS
                    if (!WorldUtilityManager.Instance.CanIDamageThisTarget(character.characterGroup, targetCharacter.characterGroup))
                        continue;

                    //  THIS GETS US OUR POSITION AND ANGLE IN RESPECT TO OUR CURRENT CRITICAL DAMAGE TARGET
                    Vector3 directionFromCharacterToTarget = character.transform.position - targetCharacter.transform.position;
                    float targetViewableAngle = Vector3.SignedAngle(directionFromCharacterToTarget, targetCharacter.transform.forward, Vector3.up);

                    if (targetCharacter.characterNetworkManager.isRipostable.Value)
                    {
                        if (targetViewableAngle >= -60 && targetViewableAngle <= 60)
                        {
                            AttemptRiposte(hit);
                            return;
                        }
                    }

                    //  TO DO ADD BACKSTAB CHECK
                }
            }
        }

        public virtual void AttemptRiposte(RaycastHit hit)
        {

        }

        public virtual void ApplyCriticalDamage()
        {
            character.characterEffectsManager.PlayCriticalBloodSplatterVFX(character.characterCombatManager.lockOnTransform.position);
            character.characterSoundFXManager.PlayCriticalStrikeSoundFX();

            if (character.IsOwner)
                character.characterNetworkManager.currentHealth.Value -= pendingCriticalDamage;
        }

        
        public IEnumerator ForceMoveEnemyCharacterToRipsotePosition(CharacterManager enemyCharacter, Vector3 ripostePosition)
        {
            float timer = 0;

            if (riposteReceiverTransform == null)
            {
                GameObject riposteTransformObject = new GameObject("Riposte Transform");
                riposteTransformObject.transform.parent = transform;
                riposteTransformObject.transform.position = Vector3.zero;
                riposteReceiverTransform = riposteTransformObject.transform;
            }

            riposteReceiverTransform.localPosition = ripostePosition;
            enemyCharacter.transform.position = riposteReceiverTransform.position;
            transform.rotation = Quaternion.LookRotation(-enemyCharacter.transform.forward);

            while (timer < 0.2f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }     

        public void EnableIsInvulnerable()
        {
            if (character.IsOwner)
                character.characterNetworkManager.isInvulnerable.Value = true;
        }

        public void DisableIsInvulnerable()
        {
            if (character.IsOwner)
                character.characterNetworkManager.isInvulnerable.Value = false;
        }

        public void EnableIsRipostable()
        {
            if (character.IsOwner)
                character.characterNetworkManager.isRipostable.Value = true;
        }

        public void EnableCanDoRollingAttack()
        {
            canPerformRollingAttack = true;
        }

        public void DisableCanDoRollingAttack()
        {
            canPerformRollingAttack = false;
        }

        public void EnableCanDoBackstepAttack()
        {
            canPerformBackstepAttack = true;
        }

        public void DisableCanDoBackstepAttack()
        {
            canPerformBackstepAttack = false;
        }

        public virtual void EnableCanDoCombo()
        {

        }

        public virtual void DisableCanDoCombo()
        {

        }
    }
}