using System.Collections;
using System.Collections.Generic;
using Animancer;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

namespace LZ
{
    public class CharacterAnimatorManager : MonoBehaviour
    {
        private CharacterManager character;
        private int horizontal;
        private int vertical;

        private ControllerTransition _controllerTransition = new ControllerTransition();
        private Coroutine _activeChainCoroutine;

        // ── Locomotion (Base Layer → Animancer) ──
        private bool _locomotionEnabled;
        private bool _inLocomotionMode;
        private AnimancerState _currentLocoState;
        private Vector2MixerState _activeMixer;
        private object _currentLocoKey;

        [Header("Animation Data")]
        public CharacterAnimationData animData;

        [Header("Flags")]
        public bool applyRootMotion;

        [Header("Damage Animations")]
        public string lastDamageAnimationPlayed;

        //  PING HIT REACTIONS
        [SerializeField] string hit_Forward_Ping_01 = "Hit_Forward_Ping_01";
        [SerializeField] string hit_Forward_Ping_02 = "Hit_Forward_Ping_02";
        [SerializeField] string hit_Backward_Ping_01 = "Hit_Backward_Ping_01";
        [SerializeField] string hit_Backward_Ping_02 = "Hit_Backward_Ping_02";
        [SerializeField] string hit_Left_Ping_01 = "Hit_Left_Ping_01";
        [SerializeField] string hit_Left_Ping_02 = "Hit_Left_Ping_02";
        [SerializeField] string hit_Right_Ping_01 = "Hit_Right_Ping_01";
        [SerializeField] string hit_Right_Ping_02 = "Hit_Right_Ping_02";

        public List<string> forward_Ping_Damage = new List<string>();
        public List<string> backward_Ping_Damage = new List<string>();
        public List<string> left_Ping_Damage = new List<string>();
        public List<string> right_Ping_Damage = new List<string>();

        //  MEDIUM HIT REACTIONS
        [SerializeField] string hit_Forward_Medium_01 = "Hit_Forward_Medium_01";
        [SerializeField] string hit_Forward_Medium_02 = "Hit_Forward_Medium_02";
        [SerializeField] string hit_Backward_Medium_01 = "Hit_Backward_Medium_01";
        [SerializeField] string hit_Backward_Medium_02 = "Hit_Backward_Medium_02";
        [SerializeField] string hit_Left_Medium_01 = "Hit_Left_Medium_01";
        [SerializeField] string hit_Left_Medium_02 = "Hit_Left_Medium_02";
        [SerializeField] string hit_Right_Medium_01 = "Hit_Right_Medium_01";
        [SerializeField] string hit_Right_Medium_02 = "Hit_Right_Medium_02";

        public List<string> forward_Medium_Damage = new List<string>();
        public List<string> backward_Medium_Damage = new List<string>();
        public List<string> left_Medium_Damage = new List<string>();
        public List<string> right_Medium_Damage = new List<string>();

        protected virtual void Awake()
        {
            character = GetComponent<CharacterManager>();

            horizontal = Animator.StringToHash("Horizontal");
            vertical = Animator.StringToHash("Vertical");
        }

        protected virtual void Start()
        {
            forward_Ping_Damage.Add(hit_Forward_Ping_01);
            forward_Ping_Damage.Add(hit_Forward_Ping_02);

            backward_Ping_Damage.Add(hit_Backward_Ping_01);
            backward_Ping_Damage.Add(hit_Backward_Ping_02);

            left_Ping_Damage.Add(hit_Left_Ping_01);
            left_Ping_Damage.Add(hit_Left_Ping_02);

            right_Ping_Damage.Add(hit_Right_Ping_01);
            right_Ping_Damage.Add(hit_Right_Ping_02);

            forward_Medium_Damage.Add(hit_Forward_Medium_01);
            forward_Medium_Damage.Add(hit_Forward_Medium_02);
            
            backward_Medium_Damage.Add(hit_Backward_Medium_01);
            backward_Medium_Damage.Add(hit_Backward_Medium_02);
            
            left_Medium_Damage.Add(hit_Left_Medium_01);
            left_Medium_Damage.Add(hit_Left_Medium_02);
            
            right_Medium_Damage.Add(hit_Right_Medium_01);
            right_Medium_Damage.Add(hit_Right_Medium_02);

            InitControllerState(character.animator.runtimeAnimatorController);
            InitLocomotion();
        }

        #region Locomotion State Machine

        private void InitLocomotion()
        {
            if (animData == null) return;
            if (animData.idle1H == null && animData.locomotion1H == null) return;

            _locomotionEnabled = true;
            _inLocomotionMode = true;

            PlayLocomotionState(0f, force: true);
        }

        protected virtual void Update()
        {
            if (_locomotionEnabled && _inLocomotionMode && !character.isPerformingAction)
                EvaluateLocomotionState();
        }

        /// <summary>每帧根据 isMoving / isBlocking / isTwoHanding 决定当前移动状态。</summary>
        private void EvaluateLocomotionState()
        {
            PlayLocomotionState(0.25f, force: false);
        }

        /// <summary>
        /// 评估条件并播放正确的移动状态。
        /// force = true 时忽略 key 缓存（用于 ReturnToController 等必须立即切换的场景）。
        /// </summary>
        private void PlayLocomotionState(float fadeDuration, bool force)
        {
            if (animData == null) return;

            bool isMoving = character.characterNetworkManager.isMoving.Value;
            bool isBlocking = character.characterNetworkManager.isBlocking.Value;
            bool isTwoHanding = GetIsTwoHanding();

            MixerTransition2D targetMixer;
            AnimationClip targetIdle;

            if (isTwoHanding)
            {
                if (isBlocking) { targetMixer = animData.blockingLocomotion2H; targetIdle = animData.blockingIdle2H; }
                else            { targetMixer = animData.locomotion2H;         targetIdle = animData.idle2H; }
            }
            else
            {
                if (isBlocking) { targetMixer = animData.blockingLocomotion1H; targetIdle = animData.blockingIdle1H; }
                else            { targetMixer = animData.locomotion1H;         targetIdle = animData.idle1H; }
            }

            object newKey = (isMoving && targetMixer != null) ? (object)targetMixer : targetIdle;
            if (newKey == null) return;
            if (!force && newKey == _currentLocoKey && _currentLocoState != null) return;

            _currentLocoKey = newKey;

            if (isMoving && targetMixer != null)
            {
                _currentLocoState = character.animancer.Layers[0].Play(targetMixer, fadeDuration);
                _activeMixer = _currentLocoState as Vector2MixerState;
            }
            else if (targetIdle != null)
            {
                _currentLocoState = character.animancer.Layers[0].Play(targetIdle, fadeDuration);
                _activeMixer = null;
            }
        }

        /// <summary>子类重写以提供 isTwoHandingWeapon 状态。基类默认返回 false。</summary>
        protected virtual bool GetIsTwoHanding() => false;

        /// <summary>外部（如 Mixer Parameter）直接设置混合树参数。</summary>
        private void ApplyMixerParameter(float h, float v, float dampTime, float deltaTime)
        {
            if (_activeMixer == null) return;

            if (dampTime > 0f)
            {
                float factor = 1f - Mathf.Exp(-deltaTime / dampTime);
                h = Mathf.Lerp(_activeMixer.ParameterX, h, factor);
                v = Mathf.Lerp(_activeMixer.ParameterY, v, factor);
            }

            _activeMixer.Parameter = new Vector2(h, v);
        }

        #endregion

        /// <summary>
        /// 将 RuntimeAnimatorController 托管给 Animancer，创建 ControllerState。
        /// Animator 上的 Controller 会被清空，由 Animancer 的 Playable 图接管。
        /// </summary>
        private void InitControllerState(RuntimeAnimatorController controller)
        {
            if (controller == null || character.animancer == null) return;

            _controllerTransition.State?.Destroy();
            _controllerTransition.Controller = controller;
            character.animator.runtimeAnimatorController = null;
            character.animancer.Play(_controllerTransition);
            character.controllerState = _controllerTransition.State;
        }

        public string GetRandomAnimationFromList(List<string> animationList)
        {
            List<string> finalList = new List<string>();

            foreach (var item in animationList)
            {
                finalList.Add(item);
            }
            
            // 检查我们是否已经播放过这个伤害动画，避免重复
            finalList.Remove(lastDamageAnimationPlayed);

            // 删除列表中的空值
            for (int i = finalList.Count - 1; i > -1; i--)
            {
                if (finalList[i] == null)
                {
                    finalList.RemoveAt(i);
                }
            }

            int randomValue = Random.Range(0, finalList.Count);

            return finalList[randomValue];
        }

        public void UpdateAnimatorMovementParameters(float horizontalMovement, float verticalMovement, bool isSprinting)
        {
            float snappedHorizontal;
            float snappedVertical;
			// 该if条件链会将水平移动值取整为 -1、-0.5、0、0.5 或 1 这几个离散值

            if (horizontalMovement > 0 && horizontalMovement <= 0.5f)
            {
                snappedHorizontal = 0.5f;
            }
            else if (horizontalMovement > 0.5f && horizontalMovement <= 1)
            {
                snappedHorizontal = 1;
            }
            else if (horizontalMovement < 0 && horizontalMovement >= -0.5f)
            {
                snappedHorizontal = -0.5f;
            }
            else if (horizontalMovement < -0.5f && horizontalMovement >= -1)
            {
                snappedHorizontal = -1;
            }
            else
            {
                snappedHorizontal = 0;
            }

            if (verticalMovement > 0 && verticalMovement <= 0.5f)
            {
                snappedVertical = 0.5f;
            }
            else if (verticalMovement > 0.5f && verticalMovement <=1)
            {
                snappedVertical = 1;
            }
            else if (verticalMovement < 0 && verticalMovement >= -0.5f)
            {
                snappedVertical = -0.5f;
            }
            else if (verticalMovement < -0.5f && verticalMovement >= -1)
            {
                snappedVertical = -1;
            }
            else
            {
                snappedVertical = 0;
            }
            
            if (isSprinting)
            {
                snappedVertical = 2;
            }
            
            character.SetAnimFloat(horizontal, snappedHorizontal, 0.1f, Time.deltaTime);
            character.SetAnimFloat(vertical, snappedVertical, 0.1f, Time.deltaTime);

            ApplyMixerParameter(snappedHorizontal, snappedVertical, 0.1f, Time.deltaTime);
        }

        //  THIS FUNCTION WILL JUST PASS THE RAW NUMBER
        public void SetAnimatorMovementParameters(float horizontalMovement, float verticalMovement)
        {
            character.SetAnimFloat(vertical, verticalMovement, 0.1f, Time.deltaTime);
            character.SetAnimFloat(horizontal, horizontalMovement, 0.1f, Time.deltaTime);

            ApplyMixerParameter(horizontalMovement, verticalMovement, 0.1f, Time.deltaTime);
        }

        #region Animancer clip 播完 → 回到 ControllerState

        /// <summary>
        /// 用 Animancer 播放 clip 并注册结束回调：fade 回 ControllerState + 重置 action flag。
        /// 替代 AnimatorController 中 Empty 状态 + ResetActionFlag 的功能。
        /// </summary>
        protected AnimancerState PlayClipWithAutoReturn(AnimationClip clip, float fadeDuration = 0.2f)
        {
            CancelActiveChain();
            _inLocomotionMode = false;
            return PlayClipWithAutoReturnInternal(clip, fadeDuration);
        }

        private AnimancerState PlayClipWithAutoReturnInternal(AnimationClip clip, float fadeDuration = 0.2f)
        {
            var state = character.animancer.Play(clip, fadeDuration);
            state.Events(this).OnEnd = () => ReturnToController(fadeDuration);
            return state;
        }

        public void CancelActiveChain()
        {
            if (_activeChainCoroutine != null)
            {
                StopCoroutine(_activeChainCoroutine);
                _activeChainCoroutine = null;
            }
        }

        /// <summary>
        /// fade 回 ControllerState 并重置所有 action flag。
        /// 等价于 ResetActionFlag.OnStateEnter 的逻辑。
        /// </summary>
        public void ReturnToController(float fadeDuration = 0.2f)
        {
            if (_locomotionEnabled)
            {
                _inLocomotionMode = true;
                PlayLocomotionState(fadeDuration, force: true);
            }
            else
            {
                if (character.controllerState != null)
                    character.animancer.Play(character.controllerState, fadeDuration);
            }

            character.isPerformingAction = false;
            applyRootMotion = false;
            character.characterLocomotionManager.canRotate = true;
            character.characterLocomotionManager.canMove = true;
            character.characterLocomotionManager.canRun = true;
            character.characterLocomotionManager.canRoll = true;
            character.characterLocomotionManager.isRolling = false;
            character.characterCombatManager.DisableCanDoCombo();
            character.characterCombatManager.DisableCanDoRollingAttack();
            character.characterCombatManager.DisableCanDoBackstepAttack();

            if (character.characterEffectsManager.activeSpellWarmUpFX != null)
                Object.Destroy(character.characterEffectsManager.activeSpellWarmUpFX);
            if (character.characterEffectsManager.activeQuickSlotItemFX != null)
                Object.Destroy(character.characterEffectsManager.activeQuickSlotItemFX);

            if (character.IsOwner)
            {
                character.characterNetworkManager.isJumping.Value = false;
                character.characterNetworkManager.isInvulnerable.Value = false;
                character.characterNetworkManager.isAttacking.Value = false;
                character.characterNetworkManager.isRipostable.Value = false;
                character.characterNetworkManager.isBeingCriticallyDamaged.Value = false;
                character.characterNetworkManager.isParrying.Value = false;
                character.characterNetworkManager.isRolling.Value = false;
            }
        }

        #endregion

        /// <summary>远端客户端通过 RPC 收到 clip 后调用，同样注册自动回调。</summary>
        public void PlayClipOnRemote(AnimationClip clip, float fadeDuration)
        {
            PlayClipWithAutoReturn(clip, fadeDuration);
        }

        #region Controller 播放辅助 (供 RPC handler 等外部调用)

        /// <summary>在 ControllerState 内 CrossFade 到指定状态</summary>
        public void CrossFadeOnController(string stateName, float fadeDuration = 0.2f)
        {
            CancelActiveChain();
            _inLocomotionMode = false;
            if (character.controllerState == null) return;
            character.animancer.Play(character.controllerState, fadeDuration);
            character.controllerState.Playable.CrossFade(stateName, fadeDuration);
        }

        /// <summary>在 ControllerState 内立即 Play 指定状态</summary>
        public void PlayOnController(string stateName)
        {
            CancelActiveChain();
            _inLocomotionMode = false;
            if (character.controllerState == null) return;
            character.animancer.Play(character.controllerState);
            character.controllerState.Playable.Play(stateName);
        }

        #endregion

        #region Play Action Animation (string — 通过 AnimatorController 状态机)

        public virtual void PlayTargetActionAnimation(
            string targetAnimation, 
            bool isPerformingAction, 
            bool applyRootMotion = true, 
            bool canRotate = false, 
            bool canMove = false,
            bool canRun = true,
            bool canRoll = false)
        {
            //Debug.Log("Playing Animation: " + targetAnimation);
            this.applyRootMotion = applyRootMotion;
            CrossFadeOnController(targetAnimation);
            // 可以用来阻止角色尝试新的动作
            // 例如，如果你受到伤害，并开始执行受击动画
            // 如果你被眩晕，这个标志会变为真
            // 然后我们可以在尝试新动作之前检查这个标志
            character.isPerformingAction = isPerformingAction;
            character.characterLocomotionManager.canRotate = canRotate;
            character.characterLocomotionManager.canMove = canMove;
            character.characterLocomotionManager.canRun = canRun;
            character.characterLocomotionManager.canRoll = canRoll;

            //  TELL THE SERVER/HOST WE PLAYED AN ANIMATION, AND TO PLAY THAT ANIMATION FOR EVERYBODY ELSE PRESENT
            character.characterNetworkManager.NotifyTheServerOfActionAnimationServerRpc(NetworkManager.Singleton.LocalClientId, targetAnimation, applyRootMotion);
        }
        
        public virtual void PlayTargetActionAnimationInstantly(
            string targetAnimation,
            bool isPerformingAction,
            bool applyRootMotion = true,
            bool canRotate = false,
            bool canMove = false,
            bool canRun = true,
            bool canRoll = false)
        {
            this.applyRootMotion = applyRootMotion;
            PlayOnController(targetAnimation);
            //  CAN BE USED TO STOP CHARACTER FROM ATTEMPTING NEW ACTIONS
            //  FOR EXAMPLE, IF YOU GET DAMAGED, AND BEGIN PERFORMING A DAMAGE ANIMATION
            //  THIS FLAG WILL TURN TRUE IF YOU ARE STUNNED
            //  WE CAN THEN CHECK FOR THIS BEFORE ATTEMPTING NEW ACTIONS
            character.isPerformingAction = isPerformingAction;
            character.characterLocomotionManager.canRotate = canRotate;
            character.characterLocomotionManager.canMove = canMove;
            character.characterLocomotionManager.canRun = canRun;
            character.characterLocomotionManager.canRoll = canRoll;

            //  TELL THE SERVER/HOST WE PLAYED AN ANIMATION, AND TO PLAY THAT ANIMATION FOR EVERYBODY ELSE PRESENT
            character.characterNetworkManager.NotifyTheServerOfInstantActionAnimationServerRpc(NetworkManager.Singleton.LocalClientId, targetAnimation, applyRootMotion);
        }

        public virtual void PlayTargetAttackActionAnimation(
            WeaponItem weapon,
            AttackType attackType,
            string targetAnimation,
            bool isPerformingAction,
            bool applyRootMotion = true,
            bool canRotate = false,
            bool canMove = false,
            bool canRoll = false)
        {
            // 跟踪上次执行的攻击（用于连招）
            // 跟踪当前攻击类型（轻攻击、重攻击等）
            // 更新动画集为当前武器动画
            // 判断我们的攻击是否可以被招架
            // 通知网络我们的“正在攻击”标志是激活状态（用于反击伤害等）
            character.characterCombatManager.currentAttackType = attackType;
            character.characterCombatManager.lastAttackAnimationPerformed = targetAnimation;
            UpdateAnimatorController(weapon.weaponAnimator);
            this.applyRootMotion = applyRootMotion;
            CrossFadeOnController(targetAnimation);
            character.isPerformingAction = isPerformingAction;
            character.characterLocomotionManager.canRotate = canRotate;
            character.characterLocomotionManager.canMove = canMove;
            character.characterNetworkManager.isAttacking.Value = true;
            character.characterLocomotionManager.canRoll = canRoll;

            //  TELL THE SERVER/HOST WE PLAYED AN ANIMATION, AND TO PLAY THAT ANIMATION FOR EVERYBODY ELSE PRESENT
            character.characterNetworkManager.NotifyTheServerOfAttackActionAnimationServerRpc(NetworkManager.Singleton.LocalClientId, targetAnimation, applyRootMotion);
        }

        #endregion

        #region Play Action Animation (AnimationClip — 通过 Animancer 直接播放)

        public virtual void PlayTargetActionAnimation(
            AnimationClip clip,
            bool isPerformingAction,
            bool applyRootMotion = true,
            bool canRotate = false,
            bool canMove = false,
            bool canRun = true,
            bool canRoll = false)
        {
            this.applyRootMotion = applyRootMotion;
            PlayClipWithAutoReturn(clip, 0.2f);
            character.isPerformingAction = isPerformingAction;
            character.characterLocomotionManager.canRotate = canRotate;
            character.characterLocomotionManager.canMove = canMove;
            character.characterLocomotionManager.canRun = canRun;
            character.characterLocomotionManager.canRoll = canRoll;

            character.characterNetworkManager.NotifyTheServerOfActionAnimationServerRpc(
                NetworkManager.Singleton.LocalClientId, clip.name, applyRootMotion);
        }

        public virtual void PlayTargetActionAnimationInstantly(
            AnimationClip clip,
            bool isPerformingAction,
            bool applyRootMotion = true,
            bool canRotate = false,
            bool canMove = false,
            bool canRun = true,
            bool canRoll = false)
        {
            this.applyRootMotion = applyRootMotion;
            PlayClipWithAutoReturn(clip, 0f);
            character.isPerformingAction = isPerformingAction;
            character.characterLocomotionManager.canRotate = canRotate;
            character.characterLocomotionManager.canMove = canMove;
            character.characterLocomotionManager.canRun = canRun;
            character.characterLocomotionManager.canRoll = canRoll;

            character.characterNetworkManager.NotifyTheServerOfInstantActionAnimationServerRpc(
                NetworkManager.Singleton.LocalClientId, clip.name, applyRootMotion);
        }

        public virtual void PlayTargetAttackActionAnimation(
            WeaponItem weapon,
            AttackType attackType,
            AnimationClip clip,
            bool isPerformingAction,
            bool applyRootMotion = true,
            bool canRotate = false,
            bool canMove = false,
            bool canRoll = false)
        {
            character.characterCombatManager.currentAttackType = attackType;
            character.characterCombatManager.lastAttackClipPerformed = clip;
            character.characterCombatManager.lastAttackAnimationPerformed = clip.name;
            this.applyRootMotion = applyRootMotion;
            PlayClipWithAutoReturn(clip, 0.2f);
            character.isPerformingAction = isPerformingAction;
            character.characterLocomotionManager.canRotate = canRotate;
            character.characterLocomotionManager.canMove = canMove;
            character.characterNetworkManager.isAttacking.Value = true;
            character.characterLocomotionManager.canRoll = canRoll;

            character.characterNetworkManager.NotifyTheServerOfAttackActionAnimationServerRpc(
                NetworkManager.Singleton.LocalClientId, clip.name, applyRootMotion);
        }

        #endregion

        #region Multi-Phase Chain — Heavy Attack Charge

        /// <summary>
        /// 播放重攻击蓄力链：Attack → Hold → Release/FullRelease。
        /// 替代 AnimatorController 中 Heavy_Attack → Hold → Release 的自动转换。
        /// </summary>
        public virtual void PlayHeavyAttackChainAnimation(
            WeaponItem weapon,
            AttackType initialAttackType,
            AttackType chargedAttackType,
            AnimationClip attackClip,
            AnimationClip holdClip,
            AnimationClip releaseClip,
            AnimationClip fullReleaseClip,
            bool isPerformingAction,
            bool applyRootMotion = true,
            bool canRotate = false,
            bool canMove = false,
            bool canRoll = false)
        {
            CancelActiveChain();

            character.characterCombatManager.currentAttackType = initialAttackType;
            character.characterCombatManager.lastAttackClipPerformed = attackClip;
            character.characterCombatManager.lastAttackAnimationPerformed = attackClip.name;
            this.applyRootMotion = applyRootMotion;
            character.isPerformingAction = isPerformingAction;
            character.characterLocomotionManager.canRotate = canRotate;
            character.characterLocomotionManager.canMove = canMove;
            character.characterLocomotionManager.canRoll = canRoll;

            if (character.IsOwner)
                character.characterNetworkManager.isAttacking.Value = true;

            character.characterNetworkManager.NotifyTheServerOfAttackActionAnimationServerRpc(
                NetworkManager.Singleton.LocalClientId, attackClip.name, applyRootMotion);

            _activeChainCoroutine = StartCoroutine(HeavyAttackChainCoroutine(
                chargedAttackType, attackClip, holdClip, releaseClip, fullReleaseClip));
        }

        private IEnumerator HeavyAttackChainCoroutine(
            AttackType chargedAttackType,
            AnimationClip attackClip,
            AnimationClip holdClip,
            AnimationClip releaseClip,
            AnimationClip fullReleaseClip)
        {
            // Phase 1: 播放攻击动作
            var state = character.animancer.Play(attackClip, 0.2f);
            yield return state;

            // Phase 2: 播放蓄力保持（非循环 clip，播完 = 满蓄力）
            state = character.animancer.Play(holdClip, 0f);
            SendPhaseRpc(holdClip.name);

            bool releasedEarly = false;
            while (state.NormalizedTime < 1f)
            {
                if (!character.characterNetworkManager.isChargingAttack.Value)
                {
                    releasedEarly = true;
                    break;
                }
                yield return null;
            }

            // Phase 3: 根据蓄力结果决定释放动画
            AnimationClip finalClip;
            if (releasedEarly)
            {
                finalClip = releaseClip;
            }
            else
            {
                finalClip = fullReleaseClip;
                character.characterCombatManager.currentAttackType = chargedAttackType;
            }

            _activeChainCoroutine = null;
            SendPhaseRpc(finalClip.name);
            PlayClipWithAutoReturnInternal(finalClip, 0.2f);
        }

        #endregion

        #region Multi-Phase Chain — Jump Attack Sequence

        /// <summary>
        /// 播放跳跃攻击序列：Attack → [AirIdle] → Landing。
        /// 替代 AnimatorController 中跳跃攻击的 isGrounded 条件转换。
        /// airIdleClip 为 null 时（轻跳攻），攻击 clip 停在最后一帧等待落地。
        /// </summary>
        public virtual void PlayJumpAttackSequenceAnimation(
            WeaponItem weapon,
            AttackType attackType,
            AnimationClip attackClip,
            AnimationClip airIdleClip,
            AnimationClip endClip,
            bool isPerformingAction,
            bool applyRootMotion = true,
            bool canRotate = false,
            bool canMove = false,
            bool canRoll = false)
        {
            CancelActiveChain();

            character.characterCombatManager.currentAttackType = attackType;
            character.characterCombatManager.lastAttackClipPerformed = attackClip;
            character.characterCombatManager.lastAttackAnimationPerformed = attackClip.name;
            this.applyRootMotion = applyRootMotion;
            character.isPerformingAction = isPerformingAction;
            character.characterLocomotionManager.canRotate = canRotate;
            character.characterLocomotionManager.canMove = canMove;
            character.characterLocomotionManager.canRoll = canRoll;

            if (character.IsOwner)
                character.characterNetworkManager.isAttacking.Value = true;

            character.characterNetworkManager.NotifyTheServerOfAttackActionAnimationServerRpc(
                NetworkManager.Singleton.LocalClientId, attackClip.name, applyRootMotion);

            _activeChainCoroutine = StartCoroutine(JumpAttackSequenceCoroutine(
                attackClip, airIdleClip, endClip));
        }

        private IEnumerator JumpAttackSequenceCoroutine(
            AnimationClip attackClip,
            AnimationClip airIdleClip,
            AnimationClip endClip)
        {
            // Phase 1: 播放攻击动作
            var state = character.animancer.Play(attackClip, 0.2f);
            yield return state;

            // Phase 2: 攻击播完后还在空中，进入空中待机
            if (!character.characterLocomotionManager.isGrounded)
            {
                state = character.animancer.Play(airIdleClip, 0f);
                SendPhaseRpc(airIdleClip.name);

                while (!character.characterLocomotionManager.isGrounded)
                    yield return null;
            }

            // Phase 3: 着陆
            _activeChainCoroutine = null;
            SendPhaseRpc(endClip.name);
            var endState = character.animancer.Play(endClip, 0.2f);

            if (character.IsOwner)
                character.characterNetworkManager.isJumping.Value = false;

            endState.Events(this).OnEnd = () => ReturnToController(0.2f);
        }

        #endregion

        #region RPC Helper

        private void SendPhaseRpc(string clipName)
        {
            if (character.IsOwner)
            {
                character.characterNetworkManager.NotifyTheServerOfActionAnimationServerRpc(
                    NetworkManager.Singleton.LocalClientId, clipName, applyRootMotion);
            }
        }

        #endregion

        /// <summary>
        /// 切换武器的 AnimatorOverrideController。销毁旧 ControllerState，用新 controller 重建。
        /// </summary>
        public void UpdateAnimatorController(AnimatorOverrideController weaponController)
        {
            InitControllerState(weaponController);
        }
    }
}
