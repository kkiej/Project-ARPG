using Animancer;
using UnityEngine;

namespace LZ
{
    public class PlayerAnimatorManager : CharacterAnimatorManager
    {
        private PlayerManager player;

        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
        }

        protected override bool GetIsTwoHanding()
        {
            return player != null && player.playerNetworkManager.isTwoHandingWeapon.Value;
        }

        protected override CommonStanceClass GetWeaponStanceClass()
        {
            var weapon = player != null ? player.playerInventoryManager.currentRightHandWeapon : null;
            if (weapon == null) return CommonStanceClass.Light;
            return CommonAnimationConvention.FromWeaponClass(weapon.weaponClass);
        }

        protected override bool GetIsLockedOn()
        {
            return player != null && player.playerNetworkManager.isLockedOn.Value;
        }

        /// <summary>
        /// Upperbody 动画结束回调，复制原 ResetUpperbodyAction StateMachineBehaviour 的逻辑。
        /// </summary>
        protected override void OnUpperbodyReturn()
        {
            if (player == null) return;

            if (player.playerEffectsManager.activeQuickSlotItemFX != null)
                Destroy(player.playerEffectsManager.activeQuickSlotItemFX);

            player.playerLocomotionManager.canRun = true;
            player.playerEquipmentManager.UnHideWeapons();

            if (player.playerEffectsManager.activeQuickSlotItemFX != null)
                Destroy(player.playerEffectsManager.activeQuickSlotItemFX);

            if (player.playerCombatManager.isUsingItem)
            {
                player.playerCombatManager.isUsingItem = false;

                if (!player.isPerformingAction)
                    player.playerLocomotionManager.canRoll = true;
            }
        }

        #region Flask Sequence (Upperbody Layer — Events 驱动)

        /// <summary>
        /// 播放完整的喝药水序列：DrinkStart → Drink（循环 chugging）→ End。
        /// 使用 Animancer Events 链式驱动阶段转换。
        /// </summary>
        public void PlayFlaskSequence()
        {
            if (animData == null || animData.flaskDrinkStart == null) return;

            var layer = player.animancer.Layers[UpperbodyLayer];
            layer.SetWeight(1f);

            NotifyUpperbody(animData.flaskDrinkStart);
            var state = layer.Play(animData.flaskDrinkStart, 0.2f);
            state.Events(this).OnEnd = OnFlaskDrinkStartEnd;
        }

        /// <summary>播放空瓶 / 无道具动画（药水用完或未装备时）。优先 ER a000_050050（§8.7 P3），回退旧 flaskEmpty。</summary>
        public void PlayFlaskEmptyAnimation()
        {
            AnimationClip clip = ResolveCommonActionClip(c => c.noItemUseBase) ?? (animData != null ? animData.flaskEmpty : null);
            if (clip == null) return;
            PlayTargetUpperbodyAnimation(clip, canRun: false, canRoll: false);
        }

        private void OnFlaskDrinkStartEnd()
        {
            // —— ResetIsChugging SMB 等价逻辑 ——
            bool wasChugging = player.playerNetworkManager.isChugging.Value;

            if (wasChugging && player.IsOwner)
            {
                FlaskItem flask = player.playerInventoryManager.currentQuickSlotItem as FlaskItem;
                if (flask != null && IsFlaskEmpty(flask))
                {
                    HandleEmptyFlaskDuringChug(flask);
                    return;
                }

                player.playerNetworkManager.isChugging.Value = false;
            }

            // 播放 Drink clip（a000_050111）。
            if (animData.flaskDrink != null)
            {
                var layer = player.animancer.Layers[UpperbodyLayer];
                NotifyUpperbody(animData.flaskDrink);
                var state = layer.Play(animData.flaskDrink, 0.1f);
                state.Events(this).OnEnd = OnFlaskDrinkEnd;
                TryScheduleFlaskConsume(state, animData.flaskDrink);
            }
        }

        /// <summary>
        /// 远端复制体回放：ER 喝药饮段(a000_050111)无 Unity AnimationEvent，需手动在 ConsumeCurrentGoods 时点补挂，
        /// 与本机一致（PlayHealingFX 等远端可见效果；数值改动由 FlaskItem 内部 IsOwner 门控，远端不会双触发）。
        /// </summary>
        public override void PlayUpperbodyClipOnRemote(AnimationClip clip, float fadeDuration = 0.2f)
        {
            var state = PlayUpperbodyClip(clip, fadeDuration);
            TryScheduleFlaskConsume(state, clip);
        }

        /// <summary>
        /// 若 clip 为 ER 喝药饮段(a000_050111 == animData.flaskDrink)，按约定的 ConsumeCurrentGoods 归一化时点
        /// 挂回血/消耗回调，替代 ER clip 缺失的 Unity AnimationEvent。仅 a000_ 通用 clip 生效，旧 clip 仍走自带事件，不双触发。
        /// </summary>
        private void TryScheduleFlaskConsume(AnimancerState state, AnimationClip clip)
        {
            if (state == null || clip == null) return;
            var conv = animData != null ? animData.commonConvention : null;
            if (conv == null || animData.flaskDrink == null) return;
            if (!IsErCommonClip(clip) || clip.name != animData.flaskDrink.name) return;

            state.Events(this).Add(Mathf.Clamp01(conv.flaskConsumeNormalizedTime), OnFlaskConsumeGoods);
        }

        /// <summary>TAE ConsumeCurrentGoods 时点回调：等价旧 flaskDrink clip 上的 SuccessfullyUseQuickSlotItem 动画事件。</summary>
        private void OnFlaskConsumeGoods()
        {
            player.playerCombatManager.SuccessfullyUseQuickSlotItem();
        }

        private static bool IsErCommonClip(AnimationClip clip)
            => clip != null && clip.name.StartsWith("a000_", System.StringComparison.OrdinalIgnoreCase);

        private void OnFlaskDrinkEnd()
        {
            if (player.playerNetworkManager.isChugging.Value)
            {
                // 继续 chugging → 回到 DrinkStart
                if (animData.flaskDrinkStart != null)
                {
                    var layer = player.animancer.Layers[UpperbodyLayer];
                    NotifyUpperbody(animData.flaskDrinkStart);
                    var state = layer.Play(animData.flaskDrinkStart, 0.1f);
                    state.Events(this).OnEnd = OnFlaskDrinkStartEnd;
                }
            }
            else
            {
                PlayFlaskEnd();
            }
        }

        private void PlayFlaskEnd()
        {
            var layer = player.animancer.Layers[UpperbodyLayer];

            if (animData.flaskEnd != null)
            {
                NotifyUpperbody(animData.flaskEnd);
                var state = layer.Play(animData.flaskEnd, 0.2f);
                state.Events(this).OnEnd = () =>
                {
                    layer.StartFade(0f, 0.2f);
                    OnUpperbodyReturn();
                };
            }
            else
            {
                layer.StartFade(0f, 0.2f);
                OnUpperbodyReturn();
            }
        }

        private bool IsFlaskEmpty(FlaskItem flask)
        {
            if (flask.healthFlask)
                return player.playerNetworkManager.remainingHealthFlasks.Value <= 0;
            else
                return player.playerNetworkManager.remainingFocusPointsFlasks.Value <= 0;
        }

        private void HandleEmptyFlaskDuringChug(FlaskItem flask)
        {
            var layer = player.animancer.Layers[UpperbodyLayer];

            if (player.IsOwner)
            {
                player.playerNetworkManager.isChugging.Value = false;
                player.playerNetworkManager.HideWeaponsServerRpc();
            }

            if (player.playerEffectsManager.activeQuickSlotItemFX != null)
                Destroy(player.playerEffectsManager.activeQuickSlotItemFX);

            if (flask.emptyFlaskItem != null)
            {
                GameObject emptyFlask = Instantiate(flask.emptyFlaskItem,
                    player.playerEquipmentManager.rightHandWeaponSlot.transform);
                player.playerEffectsManager.activeQuickSlotItemFX = emptyFlask;
            }

            AnimationClip emptyClip = ResolveCommonActionClip(c => c.noItemUseBase) ?? animData.flaskEmpty;
            if (emptyClip != null)
            {
                NotifyUpperbody(emptyClip);
                var state = layer.Play(emptyClip, 0.2f);
                state.Events(this).OnEnd = () =>
                {
                    layer.StartFade(0f, 0.2f);
                    OnUpperbodyReturn();
                };
            }
            else
            {
                layer.StartFade(0f, 0.2f);
                OnUpperbodyReturn();
            }
        }

        private void NotifyUpperbody(AnimationClip clip)
        {
            if (player.IsOwner)
            {
                player.characterNetworkManager.NotifyTheServerOfUpperbodyAnimationServerRpc(
                    Unity.Netcode.NetworkManager.Singleton.LocalClientId, clip.name);
            }
        }

        #endregion
    }
}
