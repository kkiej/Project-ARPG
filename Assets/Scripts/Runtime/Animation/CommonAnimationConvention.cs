using UnityEngine;

namespace LZ
{
    /// <summary>
    /// ER 通用动作（idle / 行走 / 翻滚 / 后撤步 等）的 ID 约定表（设计文档 §8，权威编码见
    /// <c>Assets/_ELDENRING_REF/ER-Animation-Category-Reference.md</c> §6）。
    /// <para/>
    /// ER 通用动作 ID 由三段编码：
    /// <code>animId = stance * 1_000_000 + (actionBase + load*10 + direction)</code>
    /// <list type="bullet">
    /// <item><b>stance（姿态前缀 aXXX）</b>：由"握持(单/双手) + 武器大类"决定，不是 a000 内的子号。
    ///       如单手轻型=a000、双手轻型=a010、单手重型=a002、双手长柄=a013…（见 <see cref="CommonStance"/>）。</item>
    /// <item><b>load（负重）</b>：十位×10，仅 3 档：轻=0 / 中=10 / 重=20（见 <see cref="LoadLight"/> 等）。</item>
    /// <item><b>direction（方向）</b>：个位 +0..3：前 0 / 后 1 / 左 2 / 右 3。</item>
    /// </list>
    /// 这与 <see cref="MovesetSlotConvention.ComposeAnimId"/> 同构（前缀=category），多前缀天然由
    /// <see cref="CharacterAnimationLibrary"/> 按 animId 区分。
    /// <para/>
    /// 注意权威表的例外：<b>翻滚固定 a000 前缀</b>（不随姿态变），<b>后撤步 / locomotion 随姿态前缀变</b>。
    /// <para/>
    /// 基址做成可编辑字段：若个别 ID 猜测有误，可在 Inspector 直接校正，无需改代码。
    /// 菜单：Assets → Create → ARPG → Common Animation Convention。
    /// </summary>
    [CreateAssetMenu(menuName = "ARPG/Common Animation Convention")]
    public class CommonAnimationConvention : ScriptableObject
    {
        // ── 方向索引 ──
        public const int DirForward = 0;
        public const int DirBackward = 1;
        public const int DirLeft = 2;
        public const int DirRight = 3;

        // ── 负重档（仅 3 档，乘 10 进 ID 的十位）──
        public const int LoadLight = 0;
        public const int LoadMedium = 1;
        public const int LoadHeavy = 2;

        // ── 翻滚约定固定使用的姿态前缀（a000，权威表 §6.3）──
        public const int RollStance = 0;

        // idle 未配置的哨兵：用 -1，因为 idle(a000_000000) 的后缀基址合法为 0。
        public const int IdleUnset = -1;

        [Header("Locomotion 动作基址（六位后缀，运行时叠加 stance 前缀 +组*10 +方向）")]
        [Tooltip("站立 idle 后缀基址：a000_000000 → 0。-1 表示未填，运行时回退 CharacterAnimationData.idle。")]
        public int idleId = 0;
        [Tooltip("行走基址，如 a0XX_020000 → 20000。+组*10 +方向(0-3)。")]
        public int walkBase = 20000;
        [Tooltip("慢跑基址，如 a0XX_020100 → 20100。+组*10 +方向(0-3)。")]
        public int jogBase = 20100;
        [Tooltip("奔跑/冲刺基址，如 a0XX_020200 → 20200。+组*10（仅前向）。")]
        public int runBase = 20200;
        [Tooltip("蹲走基址，如 a000_021000 → 21000。+方向(0-3)。")]
        public int crouchWalkBase = 21000;

        [Header("Dodge 动作基址")]
        [Tooltip("后撤步基址，如 a0XX_027000 → 27000。随姿态前缀，+组*10，无方向。")]
        public int backstepBase = 27000;
        [Tooltip("翻滚基址，如 a000_027100 → 27100。固定 a000 前缀，+组*10 +方向(0-3)。")]
        public int rollBase = 27100;
        [Tooltip("前手翻基址，如 a000_027140 → 27140。+方向(0-3)。")]
        public int handspringBase = 27140;

        [Header("运行时默认值（接入负重/武器系统前的兜底）")]
        [Tooltip("当前项目尚无装备负重系统：运行时统一用此负重档（0轻/1中/2重）。接入后改由角色状态提供。")]
        public int defaultLoadGroup = LoadLight;
        [Tooltip("解析姿态前缀时，握持已知但武器大类未知时使用的默认大类。")]
        public CommonStanceClass defaultStanceClass = CommonStanceClass.Light;

        /// <summary>
        /// 组合出带姿态前缀的完整 animId：<c>stance*1_000_000 + actionBase + load*10 + direction</c>。
        /// 与 <see cref="MovesetSlotConvention.ComposeAnimId"/> 同构，可统一进 <see cref="CharacterAnimationLibrary"/> 按 id 解析。
        /// </summary>
        public static int ComposeId(int stanceCategory, int actionBase, int loadGroup, int direction)
        {
            return stanceCategory * 1_000_000 + ComposeSuffix(actionBase, loadGroup, direction);
        }

        /// <summary>只组合六位后缀（不含姿态前缀）：<c>actionBase + load*10 + direction</c>。</summary>
        public static int ComposeSuffix(int actionBase, int loadGroup, int direction)
        {
            return actionBase + loadGroup * 10 + direction;
        }

        /// <summary>
        /// 由"握持(是否双手) + 武器大类"解析姿态前缀类别号（即 <see cref="CommonStance"/> 的整型值）。
        /// 弓/弩/盾在单手时无独立姿态，回退轻型(a000)。
        /// </summary>
        public static int ResolveStanceCategory(bool twoHanding, CommonStanceClass weaponClass)
        {
            switch (weaponClass)
            {
                case CommonStanceClass.Heavy: return twoHanding ? (int)CommonStance.TwoHandHeavy : (int)CommonStance.OneHandHeavy;
                case CommonStanceClass.Long:  return twoHanding ? (int)CommonStance.TwoHandLong  : (int)CommonStance.OneHandLong;
                case CommonStanceClass.Bow:      return twoHanding ? (int)CommonStance.TwoHandBow      : (int)CommonStance.OneHandLight;
                case CommonStanceClass.Crossbow: return twoHanding ? (int)CommonStance.TwoHandCrossbow : (int)CommonStance.OneHandLight;
                case CommonStanceClass.Shield:   return twoHanding ? (int)CommonStance.TwoHandShield   : (int)CommonStance.OneHandLight;
                case CommonStanceClass.Light:
                default: return twoHanding ? (int)CommonStance.TwoHandLight : (int)CommonStance.OneHandLight;
            }
        }

        /// <summary>把项目的 <see cref="WeaponClass"/> 归并到通用姿态大类。</summary>
        public static CommonStanceClass FromWeaponClass(WeaponClass weaponClass)
        {
            switch (weaponClass)
            {
                case WeaponClass.Spear: return CommonStanceClass.Long;
                case WeaponClass.Bow: return CommonStanceClass.Bow;
                case WeaponClass.MediumShield:
                case WeaponClass.LightShield: return CommonStanceClass.Shield;
                case WeaponClass.StraightSword:
                case WeaponClass.Fist:
                default: return CommonStanceClass.Light;
            }
        }

        /// <summary>把输入的 (vertical, horizontal) 量化为四向索引（前/后/左/右），取绝对值较大的轴。</summary>
        public static int ResolveDirection(float vertical, float horizontal)
        {
            if (Mathf.Abs(vertical) >= Mathf.Abs(horizontal))
                return vertical >= 0f ? DirForward : DirBackward;
            return horizontal >= 0f ? DirRight : DirLeft;
        }
    }

    /// <summary>
    /// ER 通用动作的姿态前缀（aXXX）。整型值即 ER 类别号，直接进 <see cref="CommonAnimationConvention.ComposeId"/>。
    /// 见 <c>ER-Animation-Category-Reference.md</c> §6.0。
    /// </summary>
    public enum CommonStance
    {
        OneHandLight = 0,     // a000 单手/双武器·轻型（直剑/太刀/法杖…）
        OneHandHeavy = 2,     // a002 双武器·右手重型（大剑/巨剑…）
        OneHandLong = 3,      // a003 双武器·右手长柄（矛/戟/镰…）
        TwoHandLight = 10,    // a010 双手·轻型
        TwoHandHeavy = 12,    // a012 双手·重型
        TwoHandLong = 13,     // a013 双手·长柄
        TwoHandBow = 14,      // a014 双手·弓
        TwoHandShield = 15,   // a015 双手·盾
        TwoHandCrossbow = 16, // a016 双手·弩
    }

    /// <summary>解析姿态前缀用的武器大类（比 ER 全武器类别号粗，足够定位姿态）。</summary>
    public enum CommonStanceClass
    {
        Light,
        Heavy,
        Long,
        Bow,
        Crossbow,
        Shield,
    }
}
