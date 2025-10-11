using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class FireBallManager : SpellManager
    {
        // - 脚本功能说明 - 本脚本作为火球术激活后的核心控制中枢，主要实现以下功能：
        // 1. 使法术弹道能够轻微"追踪"或"弯曲"以跟随移动中的锁定目标
        // 2. 通过本脚本的专用函数统一处理伤害数值分配
        // 3. 控制视觉特效与音效的启用/禁用（包括碰撞爆炸、轨迹尾焰等效果）

        // 扩展建议：若存在多个共享相同逻辑的法术（如目标锁定、碰撞体检测、冲击粒子等），可将通用功能抽象为基类

        [Header("Colliders")]
        public FireBallDamageCollider damageCollider;

        [Header("Instantiated FX")]
        private GameObject instantiatedDestructionFX;

        private bool hasCollided = false;
        public bool isFullyCharged = false;
        private Rigidbody fireBallRigidbody;
        private Coroutine destructionFXCoroutine;

        protected override void Awake()
        {
            base.Awake();

            fireBallRigidbody = GetComponent<Rigidbody>();
        }

        protected override void Update()
        {
            base.Update();

            if (spellTarget != null)
                transform.LookAt(spellTarget.characterCombatManager.lockOnTransform.position);
        }

        private void OnCollisionEnter(Collision collision)
        {
            //  IF WE COLLIDE WITH A CHARACTER, IGNORE THIS WE WILL LET THE DAMAGE COLLIDER HANDLE CHARACTER COLLISIONS, THIS IS JUST FOR IMPACT VFX
            if (collision.gameObject.layer == 6)
                return;

            if (!hasCollided)
            {
                hasCollided = true;
                InstantiateSpellDestructionFX();
            }
        }

        public void InitializeFireBall(CharacterManager spellCaster)
        {
            damageCollider.spellCaster = spellCaster;

            //  TO DO SET UP DAMAGE FORMULA TO CALCULATE DAMAGE BASED ON CHARACTERS STATS, SPELL POWER AND SPELL CASTING WEAPON'S SPELL BUFF
            damageCollider.fireDamage = 150;

            if (isFullyCharged)
                damageCollider.fireDamage *= 1.4f;
        }

        public void InstantiateSpellDestructionFX()
        {
            if (isFullyCharged)
            {
                instantiatedDestructionFX = Instantiate(impactParticleFullCharge, transform.position, Quaternion.identity);
            }
            else
            {
                instantiatedDestructionFX = Instantiate(impactParticle, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }

        public void WaitThenInstantiateSpellDestructionFX(float timeToWait)
        {
            if (destructionFXCoroutine != null)
                StopCoroutine(destructionFXCoroutine);

            destructionFXCoroutine = StartCoroutine(WaitThenInstantiateFX(timeToWait));
            StartCoroutine(WaitThenInstantiateFX(timeToWait));
        }

        private IEnumerator WaitThenInstantiateFX(float timeToWait)
        {
            yield return new WaitForSeconds(timeToWait);

            InstantiateSpellDestructionFX();
        }
    }
}
