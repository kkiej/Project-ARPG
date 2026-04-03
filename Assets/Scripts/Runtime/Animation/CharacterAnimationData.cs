using Animancer;
using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 角色通用动画数据（非武器相关）。
    /// 对应 Humanoid AnimatorController 中 Action Override 层的非攻击状态，
    /// 以及 Upperbody / Ping Damage 层的通用动画。
    /// 每种角色类型（Player、各 AI）各一份资产。
    /// </summary>
    [CreateAssetMenu(menuName = "Character/Character Animation Data")]
    public class CharacterAnimationData : ScriptableObject
    {
        [Header("Jump Sequence")]
        public AnimationClip jumpStart;
        public AnimationClip jumpLift;
        public AnimationClip jumpIdle;
        public AnimationClip jumpEnd;

        [Header("Roll & Dodge")]
        public AnimationClip rollForward;
        public AnimationClip backstep;

        [Header("Medium Damage Reactions")]
        public AnimationClip hitForwardMedium01;
        public AnimationClip hitForwardMedium02;
        public AnimationClip hitBackwardMedium01;
        public AnimationClip hitBackwardMedium02;
        public AnimationClip hitLeftMedium01;
        public AnimationClip hitLeftMedium02;
        public AnimationClip hitRightMedium01;
        public AnimationClip hitRightMedium02;

        [Header("Ping Damage Reactions")]
        public AnimationClip hitForwardPing01;
        public AnimationClip hitForwardPing02;
        public AnimationClip hitBackwardPing01;
        public AnimationClip hitBackwardPing02;
        public AnimationClip hitLeftPing01;
        public AnimationClip hitLeftPing02;
        public AnimationClip hitRightPing01;
        public AnimationClip hitRightPing02;

        [Header("Block Reactions")]
        public AnimationClip blockPing;
        public AnimationClip blockLight;
        public AnimationClip blockMedium;
        public AnimationClip blockHeavy;
        public AnimationClip blockColossal;

        [Header("Guard & Stance Break")]
        public AnimationClip guardBreak;
        public AnimationClip stanceBreak;

        [Header("Parries")]
        public AnimationClip fastParry;
        public AnimationClip mediumParry;
        public AnimationClip slowParry;
        public AnimationClip parryLand;
        public AnimationClip parried;

        [Header("Critical — Backstab")]
        public AnimationClip backstab;
        public AnimationClip backstabbed;
        public AnimationClip backstabDeath;
        public AnimationClip backstabGetUp;

        [Header("Critical — Riposte")]
        public AnimationClip riposte;
        public AnimationClip riposted;
        public AnimationClip riposteDeath;
        public AnimationClip riposteGetUp;

        [Header("Death")]
        public AnimationClip dead;

        [Header("Misc Actions")]
        public AnimationClip passThroughFog;
        public AnimationClip activateSiteOfGrace;
        public AnimationClip pickUpItem;

        [Header("Upperbody — Weapon Swap")]
        public AnimationClip swapRightWeapon;
        public AnimationClip swapLeftWeapon;

        [Header("Upperbody — Flask")]
        public AnimationClip flaskDrinkStart;
        public AnimationClip flaskDrink;
        public AnimationClip flaskEnd;
        public AnimationClip flaskEmpty;

        [Header("Bow")]
        public AnimationClip bowDraw;
        public AnimationClip outOfAmmo;

        [Header("Spell — Main Hand")]
        public AnimationClip spellMainStart;
        public AnimationClip spellMainCharge;
        public AnimationClip spellMainRelease;
        public AnimationClip spellMainFullRelease;

        [Header("Spell — Off Hand")]
        public AnimationClip spellOffStart;
        public AnimationClip spellOffCharge;
        public AnimationClip spellOffRelease;
        public AnimationClip spellOffFullRelease;

        // ─────────────────────────────────────────────
        //  Locomotion (Base Layer)
        //  将 AnimatorController 的 Base Layer 8 个状态迁移到 Animancer。
        //  MixerTransition2D 的 Type 请设为 Directional（等价于 FreeformDirectional），
        //  每个 Mixer 添加 18 个 child 并按 (Horizontal, Vertical) 配置 Threshold：
        //      (0,0) idle, (±0.5,0/±0.5) walk, (±1,0/±1) run, (0,2) sprint
        // ─────────────────────────────────────────────

        [Header("Locomotion — 1H Idle")]
        public AnimationClip idle1H;

        [Header("Locomotion — 1H Blend Tree")]
        public MixerTransition2D locomotion1H;

        [Header("Locomotion — 2H Idle")]
        public AnimationClip idle2H;

        [Header("Locomotion — 2H Blend Tree")]
        public MixerTransition2D locomotion2H;

        [Header("Locomotion — Blocking 1H Idle")]
        public AnimationClip blockingIdle1H;

        [Header("Locomotion — Blocking 1H Blend Tree")]
        public MixerTransition2D blockingLocomotion1H;

        [Header("Locomotion — Blocking 2H Idle")]
        public AnimationClip blockingIdle2H;

        [Header("Locomotion — Blocking 2H Blend Tree")]
        public MixerTransition2D blockingLocomotion2H;
    }
}
