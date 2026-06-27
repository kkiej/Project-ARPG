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

        /// <summary>远端客户端在 Upperbody 层播放。子类可重写以补挂 ER clip 缺失的时点事件（如喝药 ConsumeCurrentGoods）。</summary>
        public virtual void PlayUpperbodyClipOnRemote(AnimationClip clip, float fadeDuration = 0.2f)
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
        /// 用 <see cref="CommonAnimationConvention"/> + <see cref="CharacterAnimationLibrary"/> 运行时构建 locomotion 混合树，
        /// 不依赖手搓 MixerTransition2D 资产。**对齐 ER 的两套模式**（运行期实测）：
        /// <list type="bullet">
        /// <item><b>非锁定</b>：只前向（idle/走/快走/奔跑），其它方向靠角色转向，不需要侧/后移动画。Cartesian 沿 +Y 轴。</item>
        /// <item><b>锁定</b>：四向走/慢跑 + 前向奔跑(冲刺)。Directional，阈值 idle(0,0)、walk±0.5、jog±1、sprint(0,2)。</item>
        /// </list>
        /// 缓存按 (stance, 负重组, 锁定) 复用；核心 clip（idle/walkF/jogF）缺失时返回 null → 调用方告警不回退旧动画（§8.7）。
        /// </summary>
        private MixerTransition2D BuildCommonLocomotionMixer()
        {
            var conv = animData != null ? animData.commonConvention : null;
            if (conv == null) return null;

            int group = conv.defaultLoadGroup;
            // 运行时选姿态前缀：握持(单/双手) + 武器大类 → aXXX 类别号。
            int stance = CommonAnimationConvention.ResolveStanceCategory(GetIsTwoHanding(), GetWeaponStanceClass());
            bool lockedOn = GetIsLockedOn();

            // 缓存按 (stance, 负重组, 锁定) 复用：换持武 / 负重 / 切换锁定时自动重建。
            int cacheKey = (stance * 100 + group) * 2 + (lockedOn ? 1 : 0);
            if (_builtLocoMixer != null && _builtLocoKey == cacheKey)
                return _builtLocoMixer;

            // idle：按约定 idleId 解析（按姿态前缀，单锚点，固定 load0/dir0）。idleId 合法可为 0（a000_000000），
            // 用 IdleUnset(-1) 判未配。已迁移角色不再回退 animData.idle1H（§8.7 P1）。
            AnimationClip idle = conv.idleId != CommonAnimationConvention.IdleUnset
                ? LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.idleId, 0, CommonAnimationConvention.DirForward))
                : null;

            AnimationClip walkF = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.walkBase, group, CommonAnimationConvention.DirForward));
            AnimationClip jogF = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.jogBase, group, CommonAnimationConvention.DirForward));
            AnimationClip runF = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.runBase, group, CommonAnimationConvention.DirForward));

            // 核心前向缺失 → 数据不足，放弃数据驱动。
            if (idle == null || walkF == null || jogF == null)
                return null;
            if (runF == null) runF = jogF;

            MixerTransition2D mixer;

            if (!lockedOn)
            {
                // 非锁定：纯前向。方向由 HandleStandardRotation 把角色转到输入方向，动画始终前向。
                // 用 Cartesian（沿 +Y 轴的共线阈值，Directional 会退化）。
                mixer = new MixerTransition2D { Type = MixerTransition2D.MixerType.Cartesian };
                mixer.Animations = new UnityEngine.Object[] { idle, walkF, jogF, runF };
                mixer.Thresholds = new Vector2[]
                {
                    new Vector2(0f, 0f), new Vector2(0f, 0.5f), new Vector2(0f, 1f), new Vector2(0f, 2f),
                };
            }
            else
            {
                // 锁定：四向走/慢跑 + 前向奔跑。
                AnimationClip walkB = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.walkBase, group, CommonAnimationConvention.DirBackward));
                AnimationClip walkL = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.walkBase, group, CommonAnimationConvention.DirLeft));
                AnimationClip walkR = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.walkBase, group, CommonAnimationConvention.DirRight));
                AnimationClip jogB = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.jogBase, group, CommonAnimationConvention.DirBackward));
                AnimationClip jogL = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.jogBase, group, CommonAnimationConvention.DirLeft));
                AnimationClip jogR = LookupClipByAnimId(CommonAnimationConvention.ComposeId(stance, conv.jogBase, group, CommonAnimationConvention.DirRight));

                // 个别方向缺失用同类前向兜底，避免 null 子状态。
                if (walkB == null) walkB = walkF;
                if (walkL == null) walkL = walkF;
                if (walkR == null) walkR = walkF;
                if (jogB == null) jogB = jogF;
                if (jogL == null) jogL = jogF;
                if (jogR == null) jogR = jogF;

                mixer = new MixerTransition2D { Type = MixerTransition2D.MixerType.Directional };
                mixer.Animations = new UnityEngine.Object[] { idle, walkF, walkB, walkL, walkR, jogF, jogB, jogL, jogR, runF };
                mixer.Thresholds = new Vector2[]
                {
                    new Vector2(0f, 0f),
                    new Vector2(0f, 0.5f), new Vector2(0f, -0.5f), new Vector2(-0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(0f, 1f),   new Vector2(0f, -1f),   new Vector2(-1f, 0f),   new Vector2(1f, 0f),
                    new Vector2(0f, 2f),
                };
            }

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

        /// <summary>子类重写以提供锁定状态（决定 locomotion 用四向还是纯前向）。基类默认 false（非锁定）。</summary>
        protected virtual bool GetIsLockedOn() => false;

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
        /// 解析一个固定 a000 前缀、无负重/方向的通用动作 clip（换武 / 喝药 / 无道具 等，§8.7 P3）。
        /// base 由 <see cref="CommonAnimationConvention"/> 取（可在 Inspector 校正）；约定未配 / base 为 Unset / 库未命中 → null。
        /// 调用方据此回退 <see cref="CharacterAnimationData"/> 强类型字段，迁移期零回归。
        /// </summary>
        public AnimationClip ResolveCommonActionClip(System.Func<CommonAnimationConvention, int> baseSelector)
        {
            var conv = animData != null ? animData.commonConvention : null;
            if (conv == null) return null;

            int baseId = baseSelector(conv);
            if (baseId == CommonAnimationConvention.IdleUnset) return null;

            return LookupClipByAnimId(CommonAnimationConvention.ComposeId(
                CommonAnimationConvention.RollStance, baseId, 0, CommonAnimationConvention.DirForward));
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

            // 动作结束/被打断时确保命中框已关，避免 TAE 窗事件被取消导致命中框泄漏。
            CloseDamageColliders();

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
            bool canRoll = false,
            float damageWindowStart = 0f,
            float damageWindowEnd = 0f)
        {
            character.characterCombatManager.currentAttackType = attackType;
            character.characterCombatManager.lastAttackClipPerformed = clip;
            character.characterCombatManager.lastAttackAnimationPerformed = clip.name;
            this.applyRootMotion = applyRootMotion;
            var state = PlayClipWithAutoReturn(clip, 0.2f);
            character.isPerformingAction = isPerformingAction;
            character.characterLocomotionManager.canRotate = canRotate;
            character.characterLocomotionManager.canMove = canMove;
            character.characterNetworkManager.isAttacking.Value = true;
            character.characterLocomotionManager.canRoll = canRoll;

            // ER clip 无 OpenDamageCollider 动画事件 → 按 TAE AttackBehavior 窗驱动命中框（owner 权威）。
            if (character.IsOwner)
                ScheduleDamageWindow(state, damageWindowStart, damageWindowEnd);

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

        /// <summary>当前 Action 层活跃状态（供子类在播放动作 clip 后补挂 ER 缺失的时点事件，如法术释放）。无则 null。</summary>
        public Animancer.AnimancerState CurrentActionState
            => character != null && character.animancer != null
                ? character.animancer.Layers[ActionLayer].CurrentState
                : null;

        #endregion

        #region Damage Window（TAE AttackBehavior 驱动命中框）

        /// <summary>开启近战命中框（替代 ER clip 缺失的 OpenDamageCollider 动画事件）。基类空实现，玩家子类接装备管理器。</summary>
        protected virtual void OpenDamageColliders() { }

        /// <summary>关闭近战命中框。基类空实现，玩家子类接装备管理器。</summary>
        protected virtual void CloseDamageColliders() { }

        /// <summary>
        /// 给某个攻击 clip 的播放状态挂上「按 TAE AttackBehavior 窗开/合命中框」的时点事件。
        /// 窗为秒（clip 绝对时间），内部换算成归一化时点。endSec&lt;=0 视为无 TAE 数据 → 不挂（旧 clip 仍走自带动画事件）。
        /// 仅在 owner 调度即可：伤害判定是 owner 权威。
        /// </summary>
        protected void ScheduleDamageWindow(AnimancerState state, float startSec, float endSec)
        {
            if (state == null || endSec <= 0f) return;
            float len = (float)state.Length;
            if (len <= 0f) return;

            // 进入新攻击段前，先确保上一段的命中框已关（连招/相位切换时防泄漏）。
            CloseDamageColliders();

            var ev = state.Events(this);
            ev.Add(Mathf.Clamp01(startSec / len), OpenDamageColliders);
            ev.Add(Mathf.Clamp01(endSec / len), CloseDamageColliders);
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

        #region Multi-Phase Chain — Chargeable Heavy Attack（ER：长按 0500 蓄满 / 短按切 0505 直接出手）

        private AnimationClip _chargeQuickClip;        // 0505：短按/未蓄满的直接出手
        private AttackType _chargeUnchargedType;       // 短按时的攻击类型
        private AttackType _chargeChargedType;         // 蓄满时的攻击类型
        private float _chargeMinHoldTime;              // 起手后此时间内不判松手（给 Hold 交互留识别窗）
        private float _chargeCommitTime;               // 0500 播到此仍按住=蓄满，此前松手=短按
        private bool _chargeCommitted;                 // 已提交（蓄满或已切短按），停止判定

        /// <summary>
        /// ER 蓄力重击：起手即播蓄满 clip <paramref name="chargedClip"/>(0500，前期有较长蓄力过程)。
        /// 起手 <paramref name="minHoldTime"/> 秒内不判松手；之后若松手（isChargingAttack=false）则切短按直接出手
        /// <paramref name="quickClip"/>(0505)；若播到 <paramref name="commitTime"/> 仍按住则判蓄满，0500 继续播完。
        /// </summary>
        public virtual void PlayChargeAttackAnimation(
            WeaponItem weapon,
            AttackType unchargedType,
            AttackType chargedType,
            AnimationClip chargedClip,
            AnimationClip quickClip,
            float minHoldTime,
            float commitTime,
            bool isPerformingAction,
            bool applyRootMotion = true,
            bool canRotate = false,
            bool canMove = false,
            bool canRoll = false)
        {
            CancelActiveChain();
            _inLocomotionMode = false;

            character.characterCombatManager.currentAttackType = unchargedType;
            character.characterCombatManager.lastAttackClipPerformed = chargedClip;
            character.characterCombatManager.lastAttackAnimationPerformed = chargedClip.name;
            this.applyRootMotion = applyRootMotion;
            character.isPerformingAction = isPerformingAction;
            character.characterLocomotionManager.canRotate = canRotate;
            character.characterLocomotionManager.canMove = canMove;
            character.characterLocomotionManager.canRoll = canRoll;

            if (character.IsOwner)
                character.characterNetworkManager.isAttacking.Value = true;

            SendAttackActionRpc(chargedClip, applyRootMotion);

            _chargeQuickClip = quickClip;
            _chargeUnchargedType = unchargedType;
            _chargeChargedType = chargedType;
            _chargeMinHoldTime = minHoldTime;
            _chargeCommitTime = commitTime;
            _chargeCommitted = false;

            var state = ActivateLayerAndPlay(ActionLayer, chargedClip, 0.2f, 1f);
            state.Events(this).OnEnd = () => ReturnToController(0.2f);

            _phaseUpdate = CheckChargeAttack;
        }

        private void CheckChargeAttack()
        {
            if (_chargeCommitted) return;

            var chargeState = character.animancer.Layers[ActionLayer].CurrentState;
            float t = chargeState != null ? (float)chargeState.Time : 0f;
            // 起手保护窗：给手柄 Hold 交互识别时间，避免一起手就被判成短按。
            if (t < _chargeMinHoldTime) return;

            bool stillHolding = character.characterNetworkManager.isChargingAttack.Value;

            if (t >= _chargeCommitTime)
            {
                // 提交蓄满：0500 继续播完，切到蓄满攻击类型。
                _chargeCommitted = true;
                _phaseUpdate = null;
                if (stillHolding)
                    character.characterCombatManager.currentAttackType = _chargeChargedType;
                return;
            }

            if (!stillHolding)
            {
                // 蓄满提交前松手 → 短按：切到直接出手 0505（无则继续 0500 蓄满）。
                _chargeCommitted = true;
                _phaseUpdate = null;
                if (_chargeQuickClip != null)
                {
                    character.characterCombatManager.currentAttackType = _chargeUnchargedType;
                    character.characterCombatManager.lastAttackClipPerformed = _chargeQuickClip;
                    character.characterCombatManager.lastAttackAnimationPerformed = _chargeQuickClip.name;
                    SendPhaseRpc(_chargeQuickClip.name);
                    PlayClipWithAutoReturnInternal(_chargeQuickClip, 0.1f);
                }
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

            // 起跳即开始记录最高点：跳跃上升高度固定，空中出招时机不固定，故下落高度须以起跳为起点。
            BeginJumpApex();

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
            TrackJumpApex();
            if (character.characterLocomotionManager.isGrounded)
                PlayJumpLanding();
        }

        private void PlayJumpLanding()
        {
            _phaseUpdate = null;
            _jumpApexTracking = false;

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

        #region Multi-Phase Chain — Jump Attack Sequence（ER 落地驱动融合）

        private AnimationClip _jumpAttackAirHoldClip;        // 060：空中维持/下落（可空，回退通用 jumpIdle）
        private AnimationClip _jumpAttackLandingClip;        // 070：触地攻（带命中框）
        private AnimationClip _jumpAttackRecoveryClip;       // 071：落地恢复（短距离 + 前序未到尾声；其余落地恢复缺省时的回退）
        private AnimationClip _jumpAttackRecoveryLongClip;   // 072：长距离 + 前序未到尾声
        private AnimationClip _jumpAttackRecoveryFastClip;   // 081：短距离 + 前序接近尾声
        private AnimationClip _jumpAttackRecoveryFastLongClip;// 082：长距离 + 前序接近尾声
        private AnimationClip _jumpAttackFallbackEndClip;    // 未配 070 时回退的通用落地（jumpEnd）
        private AttackType _jumpAttackLandingType;
        private float _jumpAttackLandingLookahead;
        private float _jumpAttackLandingDmgStart, _jumpAttackLandingDmgEnd; // 070 触地攻命中窗（秒）
        private float _jumpAttackLongFallThreshold;          // 下落高度阈值（米）：峰值Y−落地Y ≥ 此值视为落得高
        private float _jumpAttackFastProgressThreshold;      // 前序快慢阈值（归一化 0~1）
        private float _jumpApexY;                            // 起跳后累计的最高点 Y（用于算下落高度）；从起跳开始记，延续到跳攻落地
        private bool _jumpApexTracking;                      // 是否正在记录跳跃最高点（起跳置位，落地清零）
        private bool _jumpAttackAirClipCompleted;            // 空中攻 030 是否在触地前已播完（进入维持）→ 视为接近尾声
        private bool _jumpAttackLandedLong;                  // 本次触地：是否落得高/远（触地瞬间锁存）
        private bool _jumpAttackLandedFast;                  // 本次触地：前序是否接近尾声（触地瞬间锁存）

        /// <summary>
        /// 播放跳跃攻击序列（ER 融合）：空中攻 03x030 →[空中维持 060/jumpIdle]→ 触地攻 03x070 →[落地恢复 03x071]。
        /// 三段切换由<b>落地检测</b>驱动（<see cref="KCCCharacterController.IsNearGround"/> 预判 + isGrounded），非动画事件：
        /// 空中攻播放中/播完一旦快触地即切 070；播完仍在高处则播 060 维持并持续探测。070 带自身 attackType 命中框，
        /// 070 播完接 071 收招。未配 070 时回退旧的通用 jumpEnd。
        /// </summary>
        public virtual void PlayJumpAttackSequenceAnimation(
            WeaponItem weapon,
            AttackType airAttackType,
            AnimationClip airAttackClip,
            AnimationClip airHoldClip,
            AnimationClip landingAttackClip,
            AttackType landingAttackType,
            AnimationClip landingRecoveryClip,
            AnimationClip fallbackEndClip,
            bool isPerformingAction,
            bool applyRootMotion = true,
            float landingLookahead = 0f,
            bool canRotate = false,
            bool canMove = false,
            bool canRoll = false,
            float airDamageWindowStart = 0f,
            float airDamageWindowEnd = 0f,
            float landingDamageWindowStart = 0f,
            float landingDamageWindowEnd = 0f,
            AnimationClip landingRecoveryLongClip = null,
            AnimationClip landingRecoveryFastClip = null,
            AnimationClip landingRecoveryFastLongClip = null,
            float landingLongFallThreshold = 0f,
            float landingFastProgressThreshold = 0f)
        {
            CancelActiveChain();
            _inLocomotionMode = false;

            character.characterCombatManager.currentAttackType = airAttackType;
            character.characterCombatManager.lastAttackClipPerformed = airAttackClip;
            character.characterCombatManager.lastAttackAnimationPerformed = airAttackClip.name;
            this.applyRootMotion = applyRootMotion;
            character.isPerformingAction = isPerformingAction;
            character.characterLocomotionManager.canRotate = canRotate;
            character.characterLocomotionManager.canMove = canMove;
            character.characterLocomotionManager.canRoll = canRoll;

            if (character.IsOwner)
                character.characterNetworkManager.isAttacking.Value = true;

            SendAttackActionRpc(airAttackClip, applyRootMotion);

            _jumpAttackAirHoldClip = airHoldClip;
            _jumpAttackLandingClip = landingAttackClip;
            _jumpAttackRecoveryClip = landingRecoveryClip;
            _jumpAttackRecoveryLongClip = landingRecoveryLongClip;
            _jumpAttackRecoveryFastClip = landingRecoveryFastClip;
            _jumpAttackRecoveryFastLongClip = landingRecoveryFastLongClip;
            _jumpAttackFallbackEndClip = fallbackEndClip;
            _jumpAttackLandingType = landingAttackType;
            _jumpAttackLandingLookahead = landingLookahead;
            _jumpAttackLandingDmgStart = landingDamageWindowStart;
            _jumpAttackLandingDmgEnd = landingDamageWindowEnd;
            _jumpAttackLongFallThreshold = landingLongFallThreshold;
            _jumpAttackFastProgressThreshold = landingFastProgressThreshold;
            _jumpAttackAirClipCompleted = false;
            _jumpAttackLandedLong = false;
            _jumpAttackLandedFast = false;
            // 峰值从起跳(PlayJumpSequence)就开始记并延续至此，这里不重置；
            // 仅当跳攻不是经由起跳进入（如从下落直接接空中攻）时兜底以当前高度起记。
            if (!_jumpApexTracking) BeginJumpApex();

            var state = ActivateLayerAndPlay(ActionLayer, airAttackClip, 0.2f, 1f);
            state.Events(this).OnEnd = OnJumpAttackAirClipEnd;

            // 空中攻 030 命中框（owner 权威）。
            if (character.IsOwner)
                ScheduleDamageWindow(state, airDamageWindowStart, airDamageWindowEnd);

            // 空中攻播放期间也持续探测：矮跳/快落时提前落地即可立刻切触地攻，不必等空中攻播完。
            _phaseUpdate = CheckJumpAttackLanding;
        }

        private bool NearGroundForLanding()
            => character.kcc != null
                ? character.kcc.IsNearGround(_jumpAttackLandingLookahead)
                : character.characterLocomotionManager.isGrounded;

        /// <summary>当前世界 Y（优先用 KCC 的 TransientPosition，回退 transform）。</summary>
        private float CurrentWorldY()
            => character.kcc != null ? character.kcc.CurrentY : character.transform.position.y;

        /// <summary>起跳时调用：以当前高度为起点开始记录最高点。</summary>
        private void BeginJumpApex()
        {
            _jumpApexY = CurrentWorldY();
            _jumpApexTracking = true;
        }

        /// <summary>空中阶段每帧调用：刷新最高点，供落地时计算下落高度。</summary>
        private void TrackJumpApex()
        {
            float y = CurrentWorldY();
            if (y > _jumpApexY) _jumpApexY = y;
        }

        private void OnJumpAttackAirClipEnd()
        {
            // 空中攻 030 已完整播完：无论随后是立刻落地还是进维持，前序都算「接近尾声」→ 倾向快速恢复(8x)。
            _jumpAttackAirClipCompleted = true;

            if (NearGroundForLanding())
            {
                PlayJumpAttackLanding();
                return;
            }

            // 仍在高处：播空中维持（060 或回退通用 jumpIdle），继续探测落地。
            if (_jumpAttackAirHoldClip != null)
            {
                character.animancer.Layers[ActionLayer].Play(_jumpAttackAirHoldClip, 0.1f);
                SendPhaseRpc(_jumpAttackAirHoldClip.name);
            }
            _phaseUpdate = CheckJumpAttackLanding;
        }

        private void CheckJumpAttackLanding()
        {
            TrackJumpApex();
            if (NearGroundForLanding())
                PlayJumpAttackLanding();
        }

        private void PlayJumpAttackLanding()
        {
            _phaseUpdate = null;

            // 触地瞬间锁存落地恢复的两个判据（恢复段在 070 播完后才选 clip，此刻先存下当时的下落高度/前序进度）：
            //   高低 = 本次下落高度(起跳后最高点Y − 落地Y)是否达阈值；快慢 = 空中攻是否已播完 或 其归一化进度是否达阈值。
            _jumpApexTracking = false;
            float fallHeight = _jumpApexY - CurrentWorldY();
            _jumpAttackLandedLong = _jumpAttackLongFallThreshold > 0f
                && fallHeight >= _jumpAttackLongFallThreshold;

            float airProgress = 1f;
            if (!_jumpAttackAirClipCompleted)
            {
                float len = CurrentActionLength;
                airProgress = len > 0f ? CurrentActionTime / len : 0f;
            }
            _jumpAttackLandedFast = _jumpAttackFastProgressThreshold > 0f
                && airProgress >= _jumpAttackFastProgressThreshold;

            if (character.IsOwner)
                character.characterNetworkManager.isJumping.Value = false;

            // 未配触地攻 → 回退旧通用落地（jumpEnd / 直接收）。
            if (_jumpAttackLandingClip == null)
            {
                if (_jumpAttackFallbackEndClip != null)
                {
                    SendPhaseRpc(_jumpAttackFallbackEndClip.name);
                    PlayClipWithAutoReturnInternal(_jumpAttackFallbackEndClip, 0.2f);
                }
                else
                {
                    ReturnToController(0.2f);
                }
                return;
            }

            // 触地攻 070：换成自身 attackType 命中框（沿用现有伤害/网络逻辑读取 currentAttackType + isAttacking）。
            character.characterCombatManager.currentAttackType = _jumpAttackLandingType;
            character.characterCombatManager.lastAttackClipPerformed = _jumpAttackLandingClip;
            character.characterCombatManager.lastAttackAnimationPerformed = _jumpAttackLandingClip.name;
            if (character.IsOwner)
                character.characterNetworkManager.isAttacking.Value = true;

            SendPhaseRpc(_jumpAttackLandingClip.name);
            var state = character.animancer.Layers[ActionLayer].Play(_jumpAttackLandingClip, 0.1f);
            state.Events(this).OnEnd = OnJumpAttackLandingEnd;

            // 触地攻 070 命中框（owner 权威）。
            if (character.IsOwner)
                ScheduleDamageWindow(state, _jumpAttackLandingDmgStart, _jumpAttackLandingDmgEnd);
        }

        private void OnJumpAttackLandingEnd()
        {
            // 落地恢复（无命中）：按触地瞬间锁存的「距离长短 × 前序快慢」四选一，缺省逐级回退到 071。
            AnimationClip recovery = SelectJumpLandingRecovery(_jumpAttackLandedLong, _jumpAttackLandedFast);
            if (recovery != null)
            {
                SendPhaseRpc(recovery.name);
                PlayClipWithAutoReturnInternal(recovery, 0.15f);
            }
            else
            {
                ReturnToController(0.2f);
            }
        }

        /// <summary>
        /// 落地恢复 clip 选择：落得低/高(071/072) × 前序慢/快(071·072/081·082)。
        /// 「高低」按下落高度，「快慢」按前序空中攻进度。某变体未配时回退：快速→对应非快速，高→低，最终落到 071（<see cref="_jumpAttackRecoveryClip"/>）。
        /// </summary>
        private AnimationClip SelectJumpLandingRecovery(bool isLong, bool isFast)
        {
            AnimationClip Pick(AnimationClip primary, AnimationClip fallback)
                => primary != null ? primary : fallback;

            if (isFast && isLong) return Pick(_jumpAttackRecoveryFastLongClip,           // 082
                                         Pick(_jumpAttackRecoveryLongClip,               // ← 072
                                         Pick(_jumpAttackRecoveryFastClip,               // ← 081
                                              _jumpAttackRecoveryClip)));                // ← 071
            if (isFast)            return Pick(_jumpAttackRecoveryFastClip,               // 081
                                              _jumpAttackRecoveryClip);                  // ← 071
            if (isLong)            return Pick(_jumpAttackRecoveryLongClip,               // 072
                                              _jumpAttackRecoveryClip);                  // ← 071
            return _jumpAttackRecoveryClip;                                              // 071
        }

        #endregion

        #region Crouch（下蹲：进入 → idle/四向移动 → 站起，固定 a000 前缀，按 animId 数据驱动解析）

        private bool _crouching;
        private bool _crouchEntering;          // 进入过渡 clip 播放中，期间不被 idle/移动覆盖
        private int _crouchAnimKey = int.MinValue;  // 当前下蹲 loco clip 标识，避免每帧重播

        /// <summary>当前是否处于下蹲（含进入过渡）。</summary>
        public bool IsCrouching => _crouching;

        // 下蹲全部用 a000 前缀（RollStance=0），不随持武姿态变。
        private AnimationClip LookupCrouchClip(System.Func<CommonAnimationConvention, int> baseSel, int direction)
        {
            var conv = animData != null ? animData.commonConvention : null;
            if (conv == null) return null;
            int b = baseSel(conv);
            if (b == CommonAnimationConvention.IdleUnset) return null;
            return LookupClipByAnimId(CommonAnimationConvention.ComposeId(
                CommonAnimationConvention.RollStance, b, 0, direction));
        }

        /// <summary>进入下蹲：播 a000_390000 过渡，随后转下蹲 idle/移动（由 <see cref="UpdateCrouchLocomotion"/> 驱动）。
        /// 不设 isPerformingAction，使下蹲中仍可移动并取消到跳/闪/攻。</summary>
        public void EnterCrouch()
        {
            CancelActiveChain();
            _inLocomotionMode = false;        // 关掉自动 locomotion 评估，下蹲动画改由 CrouchState 驱动
            _crouching = true;
            _crouchEntering = true;
            _crouchAnimKey = int.MinValue;
            character.isPerformingAction = false;
            character.characterLocomotionManager.canMove = true;
            character.characterLocomotionManager.canRotate = true;
            character.characterLocomotionManager.canRun = false;   // 下蹲中不可冲刺

            AnimationClip enter = LookupCrouchClip(c => c.crouchEnterId, CommonAnimationConvention.DirForward);
            if (enter != null)
            {
                SendPhaseRpc(enter.name);
                var state = ActivateLayerAndPlay(ActionLayer, enter, 0.15f, 1f);
                state.Events(this).OnEnd = () => _crouchEntering = false;
            }
            else
            {
                _crouchEntering = false;  // 无进入 clip → 直接进 idle
            }
        }

        /// <summary>下蹲期间每帧驱动：站立播下蹲 idle(300000)，移动播四向下蹲移动(320000+dir)。
        /// 非锁定只用前向（朝向由旋转处理），锁定用四向。</summary>
        public void UpdateCrouchLocomotion(bool isMoving, float vertical, float horizontal, bool lockedOn)
        {
            if (!_crouching || _crouchEntering) return;

            AnimationClip clip;
            int key;
            if (isMoving)
            {
                int dir = lockedOn ? CommonAnimationConvention.ResolveDirection(vertical, horizontal)
                                   : CommonAnimationConvention.DirForward;
                clip = LookupCrouchClip(c => c.crouchMoveBase, dir);
                key = dir;
            }
            else
            {
                clip = LookupCrouchClip(c => c.crouchIdleBase, CommonAnimationConvention.DirForward);
                key = 100;  // idle 专用 key
            }

            if (clip == null || key == _crouchAnimKey) return;
            _crouchAnimKey = key;
            character.animancer.Layers[ActionLayer].Play(clip, 0.2f);
            SendPhaseRpc(clip.name);
        }

        /// <summary>退出下蹲。playStandup=true：播 a000_390001 站起再回 locomotion；
        /// false：取消到其它动作（跳/闪/攻），仅清下蹲标志，由后续动作接管动画。</summary>
        public void ExitCrouch(bool playStandup)
        {
            if (!_crouching) return;
            _crouching = false;
            _crouchEntering = false;
            _crouchAnimKey = int.MinValue;

            if (!playStandup)
                return;  // 取消路径：调用方紧接着会播自己的动画

            AnimationClip standup = LookupCrouchClip(c => c.crouchStandupId, CommonAnimationConvention.DirForward);
            if (standup != null)
            {
                character.isPerformingAction = true;   // 站起过渡锁住，播完 ReturnToController 复位
                SendPhaseRpc(standup.name);
                PlayClipWithAutoReturnInternal(standup, 0.15f);
            }
            else
            {
                ReturnToController(0.15f);
            }
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
