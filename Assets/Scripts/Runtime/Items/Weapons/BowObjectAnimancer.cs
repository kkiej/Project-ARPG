using Animancer;
using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 替代 "Bow Object" AnimatorController。
    /// 挂在弓武器模型上，需同时挂载 AnimancerComponent。
    /// 在 Inspector 中拖入 Bow_Draw_01、Bow_Aim_01、Bow_Fire_01 三个 clip。
    /// </summary>
    [RequireComponent(typeof(AnimancerComponent))]
    public class BowObjectAnimancer : MonoBehaviour
    {
        [SerializeField] private AnimationClip bowDraw;
        [SerializeField] private AnimationClip bowAim;
        [SerializeField] private AnimationClip bowFire;

        [Tooltip("原 Animator 中 Draw→Aim 的过渡时间点（NormalizedTime）")]
        [SerializeField] private float drawToAimTransitionPoint = 0.78f;

        private AnimancerComponent _animancer;

        private void Awake()
        {
            _animancer = GetComponent<AnimancerComponent>();

            var animator = GetComponent<Animator>();
            if (animator != null)
                animator.runtimeAnimatorController = null;
        }

        /// <summary>拉弓 → 自动过渡到瞄准姿态。</summary>
        public void PlayDraw()
        {
            if (bowDraw == null) return;

            var state = _animancer.Play(bowDraw, 0.25f);

            if (bowAim != null)
            {
                state.Events(this).Add(drawToAimTransitionPoint, () =>
                {
                    _animancer.Play(bowAim, 0.25f);
                });
            }
        }

        /// <summary>射击 → 播完后回到待机。</summary>
        public void PlayFire()
        {
            if (bowFire == null) return;

            var state = _animancer.Play(bowFire, 0f);
            state.Events(this).OnEnd = () => _animancer.Stop();
        }

        /// <summary>直接回到待机（无动画）。</summary>
        public void ResetBow()
        {
            _animancer.Stop();
        }
    }
}
