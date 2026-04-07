using System.Collections.Generic;
using System.Reflection;
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

        /// <summary>子类返回 false 可跳过 ControllerState 初始化（Player 全走 Animancer clip）。</summary>
        protected virtual bool useControllerState => true;

        // ── Per-character clip name → AnimationClip lookup（供远端 RPC 查找）──
        private Dictionary<string, AnimationClip> _clipLookup = new Dictionary<string, AnimationClip>();

        /// <summary>每帧执行的阶段性检查（如跳跃等落地、蓄力检测松手），播放新动作时自动清除。</summary>
        private System.Action _phaseUpdate;

        // ── Animancer Layer Indices (与原 AnimatorController 层顺序一致) ──
        protected const int BaseLayer = 0;
        protected const int UpperbodyLayer = 1;
        protected const int ActionLayer = 2;
        protected const int PingDamageLayer = 3;
        private const float PingDamageDefaultWeight = 0.84f;

        // ── Locomotion (Base Layer → Animancer) ──
        private bool _locomotionEnabled;
        private bool _inLocomotionMode;
        private AnimancerState _currentLocoState;
        private Vector2MixerState _activeMixer;
        private object _currentLocoKey;
        private WeaponAnimationSet _activeWeaponAnimSet;

        [Header("Animation Data")]
        public CharacterAnimationData animData;

        [Header("Flags")]
        public bool applyRootMotion;

        [Header("Damage Animations")]
        public string lastDamageAnimationPlayed;
        [HideInInspector] public AnimationClip lastDamageClipPlayed;

        //  PING HIT REACTIONS (clip-based, built from animData)
        [HideInInspector] public List<AnimationClip> forward_Ping_Damage_Clips = new List<AnimationClip>();
        [HideInInspector] public List<AnimationClip> backward_Ping_Damage_Clips = new List<AnimationClip>();
        [HideInInspector] public List<AnimationClip> left_Ping_Damage_Clips = new List<AnimationClip>();
        [HideInInspector] public List<AnimationClip> right_Ping_Damage_Clips = new List<AnimationClip>();

        //  MEDIUM HIT REACTIONS (clip-based, built from animData)
        [HideInInspector] public List<AnimationClip> forward_Medium_Damage_Clips = new List<AnimationClip>();
        [HideInInspector] public List<AnimationClip> backward_Medium_Damage_Clips = new List<AnimationClip>();
        [HideInInspector] public List<AnimationClip> left_Medium_Damage_Clips = new List<AnimationClip>();
        [HideInInspector] public List<AnimationClip> right_Medium_Damage_Clips = new List<AnimationClip>();

        //  PING HIT REACTIONS (string-based, legacy)
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

            if (useControllerState)
            {
                InitControllerState(character.animator.runtimeAnimatorController);
            }
            else
            {
                character.animator.runtimeAnimatorController = null;
            }

            BuildClipLookup(animData);
            InitAnimancerLayers();
            InitDamageClipLists();
            InitLocomotion();
        }

        #region Animancer Layers Init

        private void InitAnimancerLayers()
        {
            if (character.animancer == null) return;

            // Layer 1: Upperbody — 只影响上半身
            if (animData != null && animData.upperbodyMask != null)
            {
                var layer = character.animancer.Layers[UpperbodyLayer];
                layer.Mask = animData.upperbodyMask;
                layer.SetWeight(0f);
            }

            // Layer 2: Action — 无 mask，全身覆盖（攻击/翻滚/受击等）
            character.animancer.Layers[ActionLayer].SetWeight(0f);

            // Layer 3: Ping Damage — 只影响头胸部位
            if (animData != null && animData.pingDamageMask != null)
            {
                var layer = character.animancer.Layers[PingDamageLayer];
                layer.Mask = animData.pingDamageMask;
                layer.SetWeight(0f);
            }
        }

        private void InitDamageClipLists()
        {
            if (animData == null) return;

            if (animData.hitForwardPing01 != null) forward_Ping_Damage_Clips.Add(animData.hitForwardPing01);
            if (animData.hitForwardPing02 != null) forward_Ping_Damage_Clips.Add(animData.hitForwardPing02);

            if (animData.hitBackwardPing01 != null) backward_Ping_Damage_Clips.Add(animData.hitBackwardPing01);
            if (animData.hitBackwardPing02 != null) backward_Ping_Damage_Clips.Add(animData.hitBackwardPing02);

            if (animData.hitLeftPing01 != null) left_Ping_Damage_Clips.Add(animData.hitLeftPing01);
            if (animData.hitLeftPing02 != null) left_Ping_Damage_Clips.Add(animData.hitLeftPing02);

            if (animData.hitRightPing01 != null) right_Ping_Damage_Clips.Add(animData.hitRightPing01);
            if (animData.hitRightPing02 != null) right_Ping_Damage_Clips.Add(animData.hitRightPing02);

            if (animData.hitForwardMedium01 != null) forward_Medium_Damage_Clips.Add(animData.hitForwardMedium01);
            if (animData.hitForwardMedium02 != null) forward_Medium_Damage_Clips.Add(animData.hitForwardMedium02);

            if (animData.hitBackwardMedium01 != null) backward_Medium_Damage_Clips.Add(animData.hitBackwardMedium01);
            if (animData.hitBackwardMedium02 != null) backward_Medium_Damage_Clips.Add(animData.hitBackwardMedium02);

            if (animData.hitLeftMedium01 != null) left_Medium_Damage_Clips.Add(animData.hitLeftMedium01);
            if (animData.hitLeftMedium02 != null) left_Medium_Damage_Clips.Add(animData.hitLeftMedium02);

            if (animData.hitRightMedium01 != null) right_Medium_Damage_Clips.Add(animData.hitRightMedium01);
            if (animData.hitRightMedium02 != null) right_Medium_Damage_Clips.Add(animData.hitRightMedium02);
        }

        #endregion

        #region Upperbody Layer Playback

        /// <summary>在 Upperbody 层播放 clip，播完自动淡出层权重。</summary>
        public AnimancerState PlayUpperbodyClip(AnimationClip clip, float fadeDuration = 0.2f)
        {
            var layer = character.animancer.Layers[UpperbodyLayer];
            layer.SetWeight(1f);
            var state = layer.Play(clip, fadeDuration);
            state.Events(this).OnEnd = () => ReturnFromUpperbody(fadeDuration);
            return state;
        }

        /// <summary>远端客户端在 Upperbody 层播放。</summary>
        public void PlayUpperbodyClipOnRemote(AnimationClip clip, float fadeDuration = 0.2f)
        {
            PlayUpperbodyClip(clip, fadeDuration);
        }

        private void ReturnFromUpperbody(float fadeDuration = 0.2f)
        {
            character.animancer.Layers[UpperbodyLayer].StartFade(0f, fadeDuration);
            OnUpperbodyReturn();
        }

        /// <summary>Upperbody 动画结束时的回调。子类重写以执行特定逻辑（如 ResetUpperbodyAction）。</summary>
        protected virtual void OnUpperbodyReturn() { }

        /// <summary>
        /// 在 Upperbody 层播放动作动画，并通过 RPC 同步到其他客户端。
        /// 等价于 PlayTargetActionAnimation 但在 Upperbody 层执行。
        /// </summary>
        public virtual void PlayTargetUpperbodyAnimation(
            AnimationClip clip,
            bool canRun = true,
            bool canRoll = true)
        {
            PlayUpperbodyClip(clip, 0.2f);
            character.characterLocomotionManager.canRun = canRun;
            character.characterLocomotionManager.canRoll = canRoll;

            character.characterNetworkManager.NotifyTheServerOfUpperbodyAnimationServerRpc(
                NetworkManager.Singleton.LocalClientId, clip.name);
        }

        #endregion

        #region Ping Damage Layer Playback

        /// <summary>在 Ping Damage 层播放 clip，播完自动淡出层权重。</summary>
        public AnimancerState PlayPingDamageClip(AnimationClip clip, float fadeDuration = 0.2f)
        {
            var layer = character.animancer.Layers[PingDamageLayer];
            layer.SetWeight(PingDamageDefaultWeight);
            var state = layer.Play(clip, fadeDuration);
            state.Events(this).OnEnd = () => ReturnFromPingDamage(fadeDuration);
            return state;
        }

        /// <summary>远端客户端在 Ping Damage 层播放。</summary>
        public void PlayPingDamageClipOnRemote(AnimationClip clip, float fadeDuration = 0.2f)
        {
            PlayPingDamageClip(clip, fadeDuration);
        }

        private void ReturnFromPingDamage(float fadeDuration = 0.2f)
        {
            character.animancer.Layers[PingDamageLayer].StartFade(0f, fadeDuration);
        }

        /// <summary>
        /// 在 Ping Damage 层播放伤害动画，并通过 RPC 同步。
        /// 不设置 isPerformingAction，不阻断移动。
        /// </summary>
        public virtual void PlayTargetPingDamageAnimation(AnimationClip clip)
        {
            lastDamageClipPlayed = clip;
            lastDamageAnimationPlayed = clip != null ? clip.name : "";
            PlayPingDamageClip(clip, 0.2f);

            character.characterNetworkManager.NotifyTheServerOfPingDamageAnimationServerRpc(
                NetworkManager.Singleton.LocalClientId, clip.name);
        }

        /// <summary>从 clip 列表中随机选一个（避免重复上次播放的 clip）。</summary>
        public AnimationClip GetRandomClipFromList(List<AnimationClip> clipList)
        {
            if (clipList == null || clipList.Count == 0) return null;

            List<AnimationClip> finalList = new List<AnimationClip>(clipList);
            finalList.Remove(lastDamageClipPlayed);
            finalList.RemoveAll(c => c == null);

            if (finalList.Count == 0) return null;
            return finalList[Random.Range(0, finalList.Count)];
        }

        #endregion

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
            _phaseUpdate?.Invoke();

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
        /// 优先使用当前武器的覆盖动画，若为 null 则 fallback 到 animData 默认值。
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
                ApplyLocomotionClipOverrides(_currentLocoState as ManualMixerState, targetMixer);
            }
            else if (targetIdle != null)
            {
                var resolvedIdle = ResolveClip(targetIdle);
                _currentLocoState = character.animancer.Layers[0].Play(resolvedIdle, fadeDuration);
                _activeMixer = null;
            }
        }

        /// <summary>
        /// 查找武器覆盖中是否有匹配的替换 clip。
        /// 用于 idle 等独立 clip 的覆盖。
        /// </summary>
        private AnimationClip ResolveClip(AnimationClip baseClip)
        {
            if (baseClip == null) return null;
            var overrides = _activeWeaponAnimSet?.locomotionClipOverrides;
            if (overrides == null) return baseClip;

            for (int i = 0; i < overrides.Length; i++)
                if (overrides[i].original == baseClip && overrides[i].replacement != null)
                    return overrides[i].replacement;

            return baseClip;
        }

        /// <summary>
        /// 将武器的 clip 覆盖应用到已播放的 MixerState 的子状态上。
        /// 通过 MixerState.Set(index, clip) 原地替换子 clip，
        /// 阈值 / 混合树结构保持不变 —— 与 AnimatorOverrideController 行为一致。
        /// </summary>
        private void ApplyLocomotionClipOverrides(ManualMixerState mixer, MixerTransition2D baseTransition)
        {
            if (mixer == null) return;

            var overrides = _activeWeaponAnimSet?.locomotionClipOverrides;
            var baseAnims = baseTransition.Animations;

            for (int i = 0; i < mixer.ChildCount && i < baseAnims.Length; i++)
            {
                var baseClip = baseAnims[i] as AnimationClip;
                if (baseClip == null) continue;

                AnimationClip desired = baseClip;
                if (overrides != null)
                {
                    for (int j = 0; j < overrides.Length; j++)
                    {
                        if (overrides[j].original == baseClip && overrides[j].replacement != null)
                        {
                            desired = overrides[j].replacement;
                            break;
                        }
                    }
                }

                var child = mixer.GetChild(i);
                if (child is ClipState cs && cs.Clip == desired) continue;

                mixer.Set(i, desired, destroyPrevious: true);
            }
        }

        /// <summary>子类重写以提供 isTwoHandingWeapon 状态。基类默认返回 false。</summary>
        protected virtual bool GetIsTwoHanding() => false;

        /// <summary>读取当前 Animancer Mixer 的 (Horizontal, Vertical) 参数值。用于 AI Owner 端网络同步。</summary>
        public Vector2 GetCurrentMixerParameter()
        {
            if (_activeMixer != null)
                return _activeMixer.Parameter;
            return Vector2.zero;
        }

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

        #region Clip Lookup (name → clip)

        private void BuildClipLookup(object source)
        {
            if (source == null) return;
            foreach (var field in source.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.FieldType != typeof(AnimationClip)) continue;
                var clip = field.GetValue(source) as AnimationClip;
                if (clip != null)
                    _clipLookup[clip.name] = clip;
            }
        }

        /// <summary>注册武器动画集的所有 clip 到本地字典（武器切换时调用）。</summary>
        public void RegisterWeaponClips(object weaponAnimSet)
        {
            BuildClipLookup(weaponAnimSet);
        }

        /// <summary>通过 clip 名称查找本地字典。</summary>
        public AnimationClip LookupClipByName(string clipName)
        {
            if (_clipLookup.TryGetValue(clipName, out var clip))
                return clip;
            if (AnimationClipRegistry.Instance != null)
                return AnimationClipRegistry.Instance.GetClipByName(clipName);
            return null;
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
            
            if (useControllerState)
            {
                character.SetAnimFloat(horizontal, snappedHorizontal, 0.1f, Time.deltaTime);
                character.SetAnimFloat(vertical, snappedVertical, 0.1f, Time.deltaTime);
            }

            ApplyMixerParameter(snappedHorizontal, snappedVertical, 0.1f, Time.deltaTime);
        }

        public void SetAnimatorMovementParameters(float horizontalMovement, float verticalMovement)
        {
            if (useControllerState)
            {
                character.SetAnimFloat(vertical, verticalMovement, 0.1f, Time.deltaTime);
                character.SetAnimFloat(horizontal, horizontalMovement, 0.1f, Time.deltaTime);
            }

            ApplyMixerParameter(horizontalMovement, verticalMovement, 0.1f, Time.deltaTime);
        }

        #region Animancer clip 播完 → 回到 ControllerState

        /// <summary>
        /// 在 Action 层播放 clip 并注册结束回调：fade 回 Locomotion + 重置 action flag。
        /// 替代 AnimatorController 中 Action Override 层 Empty 状态 + ResetActionFlag 的功能。
        /// </summary>
        protected AnimancerState PlayClipWithAutoReturn(AnimationClip clip, float fadeDuration = 0.2f)
        {
            CancelActiveChain();
            _inLocomotionMode = false;
            var actionLayer = character.animancer.Layers[ActionLayer];
            actionLayer.SetWeight(1f);
            return PlayClipWithAutoReturnInternal(clip, fadeDuration);
        }

        private AnimancerState PlayClipWithAutoReturnInternal(AnimationClip clip, float fadeDuration = 0.2f)
        {
            var state = character.animancer.Layers[ActionLayer].Play(clip, fadeDuration);
            state.Events(this).OnEnd = () => ReturnToController(fadeDuration);
            return state;
        }

        public void CancelActiveChain()
        {
            _phaseUpdate = null;
        }

        /// <summary>
        /// fade 回 Locomotion 并重置所有 action flag。
        /// 等价于 ResetActionFlag.OnStateEnter 的逻辑。
        /// </summary>
        public void ReturnToController(float fadeDuration = 0.2f)
        {
            // 淡出 Action 层（clip-based 动作）
            character.animancer.Layers[ActionLayer].StartFade(0f, fadeDuration);

            if (_locomotionEnabled)
            {
                _inLocomotionMode = true;
                PlayLocomotionState(fadeDuration, force: true);
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

        private AttackType _heavyChargedAttackType;
        private AnimationClip _heavyReleaseClip;
        private AnimationClip _heavyFullReleaseClip;

        /// <summary>
        /// 播放重攻击蓄力链：Attack → Hold → Release/FullRelease。
        /// 使用 Events 驱动 Attack→Hold 转换，Update 检测松手。
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
            _inLocomotionMode = false;
            character.animancer.Layers[ActionLayer].SetWeight(1f);

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

            _heavyChargedAttackType = chargedAttackType;
            _heavyReleaseClip = releaseClip;
            _heavyFullReleaseClip = fullReleaseClip;

            var layer = character.animancer.Layers[ActionLayer];
            var state = layer.Play(attackClip, 0.2f);
            state.Events(this).OnEnd = () => OnHeavyAttackEnterHold(holdClip);
        }

        private void OnHeavyAttackEnterHold(AnimationClip holdClip)
        {
            var layer = character.animancer.Layers[ActionLayer];
            layer.Play(holdClip, 0f);
            SendPhaseRpc(holdClip.name);

            _phaseUpdate = CheckHeavyAttackRelease;
        }

        private void CheckHeavyAttackRelease()
        {
            var layer = character.animancer.Layers[ActionLayer];
            var holdState = layer.CurrentState;

            bool releasedEarly = !character.characterNetworkManager.isChargingAttack.Value;
            bool holdFinished = holdState != null && holdState.NormalizedTime >= 1f;

            if (releasedEarly || holdFinished)
            {
                _phaseUpdate = null;

                AnimationClip finalClip;
                if (releasedEarly)
                {
                    finalClip = _heavyReleaseClip;
                }
                else
                {
                    finalClip = _heavyFullReleaseClip;
                    character.characterCombatManager.currentAttackType = _heavyChargedAttackType;
                }

                SendPhaseRpc(finalClip.name);
                PlayClipWithAutoReturnInternal(finalClip, 0.2f);
            }
        }

        #endregion

        #region Multi-Phase Chain — Normal Jump Sequence

        private AnimationClip _jumpStartClip;
        private AnimationClip _jumpLiftClip;
        private AnimationClip _jumpIdleClip;
        private AnimationClip _jumpEndClip;

        private void ResolveJumpClips()
        {
            bool is2H = GetIsTwoHanding();

            _jumpStartClip = (is2H && animData.jumpStart2H != null) ? animData.jumpStart2H : animData.jumpStart;
            _jumpLiftClip  = (is2H && animData.jumpLift2H  != null) ? animData.jumpLift2H  : animData.jumpLift;
            _jumpIdleClip  = (is2H && animData.jumpIdle2H  != null) ? animData.jumpIdle2H  : animData.jumpIdle;
            _jumpEndClip   = (is2H && animData.jumpEnd2H   != null) ? animData.jumpEnd2H   : animData.jumpEnd;
        }

        /// <summary>
        /// 播放完整跳跃序列：Start → Lift → AirIdle（等落地）→ End。
        /// 根据双持状态自动选择 1H / 2H 动画。
        /// </summary>
        public void PlayJumpSequence()
        {
            if (animData == null || animData.jumpStart == null) return;

            ResolveJumpClips();

            if (_jumpStartClip == null) return;

            CancelActiveChain();
            _inLocomotionMode = false;
            character.animancer.Layers[ActionLayer].SetWeight(1f);

            var layer = character.animancer.Layers[ActionLayer];
            var state = layer.Play(_jumpStartClip, 0.2f);
            SendPhaseRpc(_jumpStartClip.name);
            state.Events(this).OnEnd = OnJumpStartEnd;
        }

        private void OnJumpStartEnd()
        {
            var layer = character.animancer.Layers[ActionLayer];

            if (_jumpLiftClip != null)
            {
                var state = layer.Play(_jumpLiftClip, 0.1f);
                SendPhaseRpc(_jumpLiftClip.name);
                state.Events(this).OnEnd = OnJumpLiftEnd;
            }
            else
            {
                OnJumpLiftEnd();
            }
        }

        private void OnJumpLiftEnd()
        {
            if (!character.characterLocomotionManager.isGrounded && _jumpIdleClip != null)
            {
                var layer = character.animancer.Layers[ActionLayer];
                layer.Play(_jumpIdleClip, 0f);
                SendPhaseRpc(_jumpIdleClip.name);
                _phaseUpdate = CheckJumpLanding;
            }
            else
            {
                PlayJumpLanding();
            }
        }

        private void CheckJumpLanding()
        {
            if (character.characterLocomotionManager.isGrounded)
                PlayJumpLanding();
        }

        private void PlayJumpLanding()
        {
            _phaseUpdate = null;

            if (character.IsOwner)
                character.characterNetworkManager.isJumping.Value = false;

            if (_jumpEndClip != null)
            {
                SendPhaseRpc(_jumpEndClip.name);
                PlayClipWithAutoReturnInternal(_jumpEndClip, 0.2f);
            }
            else
            {
                ReturnToController(0.2f);
            }
        }

        #endregion

        #region Multi-Phase Chain — Jump Attack Sequence

        private AnimationClip _jumpAttackAirIdleClip;
        private AnimationClip _jumpAttackEndClip;

        /// <summary>
        /// 播放跳跃攻击序列：Attack → [AirIdle（等落地）] → Landing。
        /// 使用 Events 驱动 Attack→AirIdle 转换，Update 检测落地。
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
            _inLocomotionMode = false;
            character.animancer.Layers[ActionLayer].SetWeight(1f);

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

            _jumpAttackAirIdleClip = airIdleClip;
            _jumpAttackEndClip = endClip;

            var layer = character.animancer.Layers[ActionLayer];
            var state = layer.Play(attackClip, 0.2f);
            state.Events(this).OnEnd = OnJumpAttackClipEnd;
        }

        private void OnJumpAttackClipEnd()
        {
            if (!character.characterLocomotionManager.isGrounded && _jumpAttackAirIdleClip != null)
            {
                var layer = character.animancer.Layers[ActionLayer];
                layer.Play(_jumpAttackAirIdleClip, 0f);
                SendPhaseRpc(_jumpAttackAirIdleClip.name);
                _phaseUpdate = CheckJumpAttackLanding;
            }
            else
            {
                PlayJumpAttackLanding();
            }
        }

        private void CheckJumpAttackLanding()
        {
            if (character.characterLocomotionManager.isGrounded)
                PlayJumpAttackLanding();
        }

        private void PlayJumpAttackLanding()
        {
            _phaseUpdate = null;

            if (character.IsOwner)
                character.characterNetworkManager.isJumping.Value = false;

            SendPhaseRpc(_jumpAttackEndClip.name);
            PlayClipWithAutoReturnInternal(_jumpAttackEndClip, 0.2f);
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
            if (useControllerState)
                InitControllerState(weaponController);
        }

        /// <summary>
        /// 设置当前武器的动画集，用于覆盖 locomotion 默认动画。
        /// 传 null 恢复为 animData 默认值。调用后自动刷新当前移动状态。
        /// </summary>
        public void SetActiveWeaponAnimationSet(WeaponAnimationSet set)
        {
            _activeWeaponAnimSet = set;
            _currentLocoKey = null;

            if (set != null)
                RegisterWeaponClips(set);

            if (_locomotionEnabled && _inLocomotionMode)
                PlayLocomotionState(0.25f, force: true);
        }
    }
}
