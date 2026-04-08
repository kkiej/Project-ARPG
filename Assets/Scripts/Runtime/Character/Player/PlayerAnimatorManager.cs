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

        /// <summary>播放空瓶动画（药水用完时）。</summary>
        public void PlayFlaskEmptyAnimation()
        {
            if (animData == null || animData.flaskEmpty == null) return;
            PlayTargetUpperbodyAnimation(animData.flaskEmpty, canRun: false, canRoll: false);
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

            // 播放 Drink clip（clip 上的 Animation Event 触发 SuccessfullyUseQuickSlotItem）
            if (animData.flaskDrink != null)
            {
                var layer = player.animancer.Layers[UpperbodyLayer];
                NotifyUpperbody(animData.flaskDrink);
                var state = layer.Play(animData.flaskDrink, 0.1f);
                state.Events(this).OnEnd = OnFlaskDrinkEnd;
            }
        }

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

            if (animData.flaskEmpty != null)
            {
                NotifyUpperbody(animData.flaskEmpty);
                var state = layer.Play(animData.flaskEmpty, 0.2f);
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
