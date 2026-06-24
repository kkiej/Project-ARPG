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

        // ── Per-character 动画解析服务（按角色作用域，供 owner 选片与远端 RPC 查找；不依赖全局大表）──
        private readonly CharacterAnimationLibrary _animLibrary = new CharacterAnimationLibrary();

        /// <summary>每角色动画解析服务（设计文档 §8.4）。FSM 按 animId / 远端按 name 都经此解析。</summary>
        public CharacterAnimationLibrary AnimationLibrary => _animLibrary;

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

        protected virtual void Awake()
        {
            character = GetComponent<CharacterManager>();

            horizontal = Animator.StringToHash("Horizontal");
            vertical = Animator.StringToHash("Vertical");
        }

        protected virtual void Start()
        {
            character.animator.runtimeAnimatorController = null;

            //  模块化角色的网格在运行时重绑骨架，SkinnedMeshRenderer 的 bounds 不可靠，
            //  靠可见性剔除会误判为"不可见"而冻结骨骼。固定为 AlwaysAnimate 保证骨骼始终被动画驱动。
            character.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            BuildClipLookup(animData);
            if (animData != null)
                _animLibrary.RegisterCommonSet(animData.commonSet);
            InitAnimancerLayers();
            InitDamageClipLists();
            InitLocomotion();
        }

        #region Root Motion

        /// <summary>
        /// 子类重写以控制何时应用 Root Motion。
        /// Player：仅在 applyRootMotion 标志为 true 时（翻滚/攻击等动作期间）。
        /// AI：落地时始终应用（动画驱动移动）。
        /// </summary>
        protected virtual bool ShouldApplyRootMotion() => applyRootMotion;

        protected virtual void OnAnimatorMove()
        {
            if (!ShouldApplyRootMotion()) return;
            if (character.kcc == null) return;

            character.kcc.rootMotionDelta += character.animator.deltaPosition;
            character.kcc.rootMotionRotationDelta = character.animator.deltaRotation * character.kcc.rootMotionRotationDelta;
            character.kcc.useRootMotion = true;
        }

        #endregion

        #region Animancer Layers

        /// <summary>
        /// 在指定层播放 clip，安全处理层激活。
        /// 当层 weight ≈ 0（未激活）时：clip 立即播放（内部 fade=0），改为淡入层权重，
        /// 避免"空层 T-pose 闪帧"（Animancer 空层输出默认 pose）。
        /// 当层已激活时：保持层权重，正常 crossfade。
        /// </summary>
        private AnimancerState ActivateLayerAndPlay(int layerIndex, AnimationClip clip, float fadeDuration, float targetWeight)
        {
            var layer = character.animancer.Layers[layerIndex];

            if (fadeDuration > 0f && layer.Weight < 0.01f)
            {
                var state = layer.Play(clip, 0f);
                layer.StartFade(targetWeight, fadeDuration);
                return state;
            }
            else
            {
                layer.SetWeight(targetWeight);
                return layer.Play(clip, fadeDuration);
            }
        }

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
            var actionLayer = character.animancer.Layers[ActionLayer];
            actionLayer.SetWeight(0f);
            actionLayer.SetDebugName("Action Override");

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
            var state = ActivateLayerAndPlay(UpperbodyLayer, clip, fadeDuration, 1f);
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
            var state = ActivateLayerAndPlay(PingDamageLayer, clip, fadeDuration, PingDamageDefaultWeight);
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
            // 已迁移角色（挂了 commonConvention）走纯 ER 通用 locomotion，不再依赖遗留 idle1H/locomotion1H 字段是否赋值。
            bool dataDriven = animData.commonConvention != null;
            if (!dataDriven && animData.idle1H == null && animData.locomotion1H == null) return;

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

            // 已迁移角色（挂了 commonConvention）的非格挡 locomotion 走纯 ER 通用系统，不回退旧 clip。
            // 格挡 locomotion 的 ER id 暂未知，保持旧路径（见设计文档 §8.7 P2/P3）。
            bool dataDriven = !isBlocking && animData.commonConvention != null;
            if (dataDriven)
            {
                PlayCommonLocomotion(isMoving, fadeDuration, force);
                return;
            }

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
                _currentLocoState = character.animancer.Layers[0].Play(ResolveClip(targetIdle), fadeDuration);
                _activeMixer = null;
            }
        }

        // 缺通用 locomotion 数据时只告警一次，避免每帧刷屏（也是"不回退旧动画"的可见信号）。
        private bool _warnedMissingCommonLoco;

        /// <summary>
        /// 已迁移角色的纯 ER 通用 locomotion：移动播运行时混合树，站立播通用 idle（<c>a000_000000</c>）。
        /// 数据缺失（回填器未跑 / 约定未配）时 warn-once 并跳过，**不回退旧动画**（设计文档 §8.7 P1）。
        /// </summary>
        private void PlayCommonLocomotion(bool isMoving, float fadeDuration, bool force)
        {
            if (isMoving)
            {
                var mixer = BuildCommonLocomotionMixer();
                if (mixer == null) { WarnMissingCommonLoco(); return; }

                if (!force && (object)mixer == _currentLocoKey && _currentLocoState != null) return;

                _currentLocoKey = mixer;
                // 通用混合树的 clip 已是最终 ER 动画，不再叠加武器覆盖（覆盖表引用 animData clip，不匹配）。
                _currentLocoState = character.animancer.Layers[0].Play(mixer, fadeDuration);
                _activeMixer = _currentLocoState as Vector2MixerState;
                return;
            }

            var idle = ResolveCommonIdle();
            if (idle == null) { WarnMissingCommonLoco(); return; }

            if (!force && (object)idle == _currentLocoKey && _currentLocoState != null) return;

            _currentLocoKey = idle;
            _currentLocoState = character.animancer.Layers[0].Play(idle, fadeDuration);
            _activeMixer = null;
        }

        private void WarnMissingCommonLoco()
        {
            if (_warnedMissingCommonLoco) return;
            _warnedMissingCommonLoco = true;
            Debug.LogWarning(
                $"[{name}] 通用 ER locomotion 数据缺失（idle/走/跑 解析不到）。" +
                "请重跑 Common Animation Filler 生成 CommonAnimationSet，并确认 CharacterAnimationData 已挂 commonConvention/commonSet。" +
                "（已按 §8.7 不再回退旧动画。）", this);
        }

        /// <summary>
        /// 解析通用 ER 站立 idle（<c>a000_000000</c>，按当前姿态前缀）。
        /// 约定未配（idleId == IdleUnset）或库未命中（如未重跑回填器）时返回 null，调用方回退 animData idle。
        /// </summary>
        private AnimationClip ResolveCommonIdle()
        {
            var conv = animData != null ? animData.commonConvention : null;
            if (conv == null || conv.idleId == CommonAnimationConvention.IdleUnset) return null;

            int stance = CommonAnimationConvention.ResolveStanceCategory(GetIsTwoHanding(), GetWeaponStanceClass());
            return LookupClipByAnimId(
                CommonAnimationConvention.ComposeId(stance, conv.idleId, 0, CommonAnimationConvention.DirForward));
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

        // ── 运行时构建的通用 locomotion 混合树（Option B，设计文档 §8.3/§8.4）──
        private MixerTransition2D _builtLocoMixer;
        private int _builtLocoKey = int.MinValue;

        /// <summary>
        /// 用 <see cref="CommonAnimationConvention"/> + <see cref="CharacterAnimationLibrary"/> 运行时构建一棵
        /// 方向性 locomotion 混合树（idle / 四向走 / 四向慢跑 / 前向奔跑），不依赖手搓 MixerTransition2D 资产。
        /// 阈值布局固定为 ER 约定：idle(0,0)、walk ±0.5、jog ±1、sprint(0,2)。
        /// 缓存按负重组复用；核心 clip（idle/walkF/jogF）缺失时返回 null → 调用方回退 animData 手搓混合树。
        /// </summary>
        private MixerTransition2D BuildCommonLocomotionMixer()
        {
            var conv = animData != null ? animData.commonConvention : null;
            if (conv == null) return null;

            int group = conv.defaultLoadGroup;
            // 运行时选姿态前缀：握持(单/双手) + 武器大类 → aXXX 类别号。
            int stance = CommonAnimationConvention.ResolveStanceCategory(GetIsTwoHanding(), GetWeaponStanceClass());

            // 缓存按 (stance, 负重组) 复用：换持武状态 / 负重时自动重建。
            int cacheKey = stance * 100 + group;
            if (_builtLocoMixer != null && _builtLocoKey == cacheKey)
                return _builtLocoMixer;

            // idle：按约定 idleId 解析（按姿态前缀，单锚点，固定 load0/dir0）。idleId 合法可为 0（a000_000000），
            // 用 IdleUnset(-1) 判未配。已迁移角色不再回退 animData.idle1H（§8.7 P1）。
            AnimationClip idle = conv.idleId != CommonAnimationConvention.IdleUnset
                ? LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.idleId, 0, CommonAnimationConvention.DirForward))
                : null;

            AnimationClip walkF = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.walkBase, group, CommonAnimationConvention.DirForward));
            AnimationClip walkB = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.walkBase, group, CommonAnimationConvention.DirBackward));
            AnimationClip walkL = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.walkBase, group, CommonAnimationConvention.DirLeft));
            AnimationClip walkR = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.walkBase, group, CommonAnimationConvention.DirRight));
            AnimationClip jogF = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.jogBase, group, CommonAnimationConvention.DirForward));
            AnimationClip jogB = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.jogBase, group, CommonAnimationConvention.DirBackward));
            AnimationClip jogL = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.jogBase, group, CommonAnimationConvention.DirLeft));
            AnimationClip jogR = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.jogBase, group, CommonAnimationConvention.DirRight));
            AnimationClip runF = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.runBase, group, CommonAnimationConvention.DirForward));

            // 核心方向缺失 → 数据不足，放弃数据驱动，回退手搓混合树。
            if (idle == null || walkF == null || jogF == null)
                return null;

            // 个别方向缺失用同类前向兜底，避免 null 子状态。
            if (walkB == null) walkB = walkF;
            if (walkL == null) walkL = walkF;
            if (walkR == null) walkR = walkF;
            if (jogB == null) jogB = jogF;
            if (jogL == null) jogL = jogF;
            if (jogR == null) jogR = jogF;
            if (runF == null) runF = jogF;

            var clips = new UnityEngine.Object[] { idle, walkF, walkB, walkL, walkR, jogF, jogB, jogL, jogR, runF };
            var thresholds = new Vector2[]
            {
                new Vector2(0f, 0f),
                new Vector2(0f, 0.5f), new Vector2(0f, -0.5f), new Vector2(-0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 1f),   new Vector2(0f, -1f),   new Vector2(-1f, 0f),   new Vector2(1f, 0f),
                new Vector2(0f, 2f),
            };

            var mixer = new MixerTransition2D { Type = MixerTransition2D.MixerType.Directional };
            mixer.Animations = clips;
            mixer.Thresholds = thresholds;

            _builtLocoMixer = mixer;
            _builtLocoKey = cacheKey;
            return mixer;
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

        /// <summary>子类重写以提供当前武器大类（用于运行时解析通用动画的姿态前缀）。基类默认轻型。</summary>
        protected virtual CommonStanceClass GetWeaponStanceClass() => CommonStanceClass.Light;

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

        #region Clip Lookup (name / animId → clip)

        private void BuildClipLookup(object source)
        {
            _animLibrary.RegisterClipFields(source);
        }

        /// <summary>注册武器动画集的所有 clip 到本地解析服务（武器切换时调用）。</summary>
        public void RegisterWeaponClips(object weaponAnimSet)
        {
            _animLibrary.RegisterClipFields(weaponAnimSet);
        }

        /// <summary>注册当前武器 moveset 的所有攻击 clip（装备时调用，owner + 远端都跑）。</summary>
        public void RegisterMoveset(MovesetData moveset)
        {
            _animLibrary.RegisterMoveset(moveset);
        }

        /// <summary>
        /// 通过 clip 名称解析：先查本角色作用域服务；未命中再回退全局 <see cref="AnimationClipRegistry"/>（过渡期兜底，档位 A4 移除）。
        /// </summary>
        public AnimationClip LookupClipByName(string clipName)
        {
            if (_animLibrary.TryGetByName(clipName, out var clip))
                return clip;
            if (AnimationClipRegistry.Instance != null)
                return AnimationClipRegistry.Instance.GetClipByName(clipName);
            return null;
        }

        /// <summary>通过 ER animId 解析（FSM 选片 / RPC 按 id 同步）。未命中返回 null。</summary>
        public AnimationClip LookupClipByAnimId(int animId)
        {
            return _animLibrary.TryGetByAnimId(animId, out var clip) ? clip : null;
        }

        /// <summary>
        /// 发送攻击动画的网络广播：clip 有 ER animId（FSM/ER 路径）走紧凑的 id RPC，
        /// 否则（通用 / 旧 WeaponItemAction 路径的 clip 无 id）回退按名字 RPC。
        /// </summary>
        private void SendAttackActionRpc(AnimationClip clip, bool applyRootMotion)
        {
            ulong localId = NetworkManager.Singleton.LocalClientId;
            if (_animLibrary.TryGetAnimId(clip, out int animId))
                character.characterNetworkManager.NotifyTheServerOfAttackActionAnimationByIdServerRpc(localId, animId, applyRootMotion);
            else
                character.characterNetworkManager.NotifyTheServerOfAttackActionAnimationServerRpc(localId, clip.name, applyRootMotion);
        }

        #endregion


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
            
            ApplyMixerParameter(snappedHorizontal, snappedVertical, 0.1f, Time.deltaTime);
        }

        public void SetAnimatorMovementParameters(float horizontalMovement, float verticalMovement)
        {
            ApplyMixerParameter(horizontalMovement, verticalMovement, 0.1f, Time.deltaTime);
        }

        #region Animancer clip 播完 → 回到 Locomotion

        /// <summary>
        /// 在 Action 层播放 clip 并注册结束回调：fade 回 Locomotion + 重置 action flag。
        /// 替代 AnimatorController 中 Action Override 层 Empty 状态 + ResetActionFlag 的功能。
        /// </summary>
        protected AnimancerState PlayClipWithAutoReturn(AnimationClip clip, float fadeDuration = 0.2f)
        {
            CancelActiveChain();
            _inLocomotionMode = false;
            var state = ActivateLayerAndPlay(ActionLayer, clip, fadeDuration, 1f);
            state.Events(this).OnEnd = () => ReturnToController(fadeDuration);
            return state;
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
            if (character.isDead.Value)
                return;

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

            SendAttackActionRpc(clip, applyRootMotion);
        }

        #endregion

        #region Action Layer Playback Query（供 FSM 判断开窗时机）

        /// <summary>当前 Action 层正在播放的 clip 的已播放时间（秒）；无活跃状态返回 0。</summary>
        public float CurrentActionTime
        {
            get
            {
                if (character.animancer == null) return 0f;
                var state = character.animancer.Layers[ActionLayer].CurrentState;
                return state != null ? (float)state.Time : 0f;
            }
        }

        /// <summary>当前 Action 层正在播放的 clip 的总时长（秒）；无活跃状态返回 0。</summary>
        public float CurrentActionLength
        {
            get
            {
                if (character.animancer == null) return 0f;
                var state = character.animancer.Layers[ActionLayer].CurrentState;
                return state != null ? (float)state.Length : 0f;
            }
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

            SendAttackActionRpc(attackClip, applyRootMotion);

            _heavyChargedAttackType = chargedAttackType;
            _heavyReleaseClip = releaseClip;
            _heavyFullReleaseClip = fullReleaseClip;

            var state = ActivateLayerAndPlay(ActionLayer, attackClip, 0.2f, 1f);
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

            var state = ActivateLayerAndPlay(ActionLayer, _jumpStartClip, 0.2f, 1f);
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

                character.isPerformingAction = true;
                character.characterLocomotionManager.canMove = false;
                character.characterLocomotionManager.canRotate = false;
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

            SendAttackActionRpc(attackClip, applyRootMotion);

            _jumpAttackAirIdleClip = airIdleClip;
            _jumpAttackEndClip = endClip;

            var state = ActivateLayerAndPlay(ActionLayer, attackClip, 0.2f, 1f);
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
