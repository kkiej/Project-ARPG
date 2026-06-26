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

        // ── 负重档（乘 10 进 ID 的十位）。运行期实测确认 4 档，含超重。项目暂只用 LoadLight。 ──
        public const int LoadLight = 0;
        public const int LoadMedium = 1;
        public const int LoadHeavy = 2;
        public const int LoadOverweight = 3;

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
        [Tooltip("快走刹停基址，如 a0XX_022100 → 22100。+组*10 +方向(0-3)。")]
        public int jogStopBase = 22100;
        [Tooltip("奔跑刹停基址，如 a0XX_022200 → 22200。+组*10。")]
        public int runStopBase = 22200;
        [Tooltip("蹲走基址，如 a000_021000 → 21000。+方向(0-3)。")]
        public int crouchWalkBase = 21000;

        [Header("Dodge 动作基址")]
        [Tooltip("后撤步基址，如 a0XX_027000 → 27000。随姿态前缀，+组*10，无方向。")]
        public int backstepBase = 27000;
        [Tooltip("翻滚基址，如 a000_027100 → 27100。固定 a000 前缀，+组*10 +方向(0-3)。")]
        public int rollBase = 27100;
        [Tooltip("前手翻基址，如 a000_027140 → 27140。+方向(0-3)。")]
        public int handspringBase = 27140;

        // ── Phase 3 待接入：以下 base 已由运行期实测+文件比对确认（见 ER-Animation-Category-Reference.md §6.4-6.6），
        //    但消费方（跳跃/喝药/换武）暂仍走 CharacterAnimationData 强类型字段，待 P3 统一改走 ComposeId+库解析。 ──
        [Header("Phase 3 待接入：跳跃 / 落地（a000_202xxx，base + 速度档*10；前缀 202 非移动的 02）")]
        [Tooltip("起跳基址：站 a000_202000 → 202000。速度档(站0/走1/快走2/奔跑3)进十位。")]
        public int jumpBase = 202000;
        [Tooltip("落地基址：a000_202100 → 202100。速度档进十位（站/走110? 实测站走共用100，快走110，奔跑120）。")]
        public int jumpLandBase = 202100;

        [Header("Phase 3：换武 / 喝药 / 无道具（a000_0xxxxx）")]
        [Tooltip("换右手武器基址 a000_029001 → 29001（盾→武器为 29000）。")]
        public int weaponSwapRightBase = 29001;
        [Tooltip("换左手基址 a000_029031 → 29031（盾为 29030）。")]
        public int weaponSwapLeftBase = 29031;
        // 喝药三段：050110 举瓶起手 / 050111 饮(含 ConsumeCurrentGoods 消耗→回血时点) / 050112 收。
        // 注：早期被 TAE 提取器错误的「事件名」误判为战斗动作（type0 错标 InvokeAttackBehavior、
        // type114 错标 Hitbox_DummyPoly），经 DSAS 截图核实更正：这些就是喝药动画。
        [Tooltip("喝药（有药）基址 a000_050110 → 50110。一组三段：起手/饮/收 = 50110/50111/50112。")]
        public int flaskDrinkBase = 50110;
        [Tooltip("空手/无道具动画 a000_050050 → 50050：未装备药或药已喝光时，喝药改播此动画。")]
        public int noItemUseBase = 50050;
        [Range(0f, 1f)]
        [Tooltip("饮(a000_050111)内 ConsumeCurrentGoods 的归一化时点：播到此处触发回血/消耗，替代 ER clip 缺失的" +
                 " Unity AnimationEvent。仅对 a000_ 通用 clip 生效。DSAS 实测：ConsumeCurrentGoods 第 8 帧、clip 共 30 帧" +
                 "(1s@30fps) → 8/30 ≈ 0.2667。")]
        public float flaskConsumeNormalizedTime = 8f / 30f;

        [Header("闪避无敌帧（秒，按 clip 绝对播放时间；来源 c0000 TAE 转储的 type0 flag8 \"Flag As Dodging\"）")]
        [Tooltip("翻滚无敌帧起点（秒）。a000_027100 实测 flag8 自第 0 帧起。")]
        public float rollIFrameStartSeconds = 0f;
        [Tooltip("翻滚无敌帧终点（秒）。a000_027100 实测 flag8 帧 0–16 → 16/30 ≈ 0.533s（@30fps）。")]
        public float rollIFrameEndSeconds = 16f / 30f;
        [Tooltip("后撤步无敌帧起点（秒）。a000_027000 实测 flag8 自第 0 帧起。")]
        public float backstepIFrameStartSeconds = 0f;
        [Tooltip("后撤步无敌帧终点（秒）。a000_027000 实测 flag8 帧 0–7 → 7/30 ≈ 0.233s（之后 7–11 帧仅 PvE 无敌，从简不接）。")]
        public float backstepIFrameEndSeconds = 7f / 30f;

        // ── Phase 2/3 待接入：ER animId 待确认，先留 IdleUnset(-1) 占位。 ──
        [Header("待确认：受击 / 死亡（ER animId 未知，-1=未填）")]
        [Tooltip("中度受击硬直基址（a000_，方向 0-3）。待确认 ER id。")]
        public int hitMediumBase = IdleUnset;
        [Tooltip("轻微受击 flinch 基址（a000_，方向 0-3）。待确认 ER id。")]
        public int hitPingBase = IdleUnset;
        [Tooltip("死亡基址（a000_）。待确认 ER id。")]
        public int deathBase = IdleUnset;

        // ── Phase 3 待接入：姿势 emote（a000_08xxxx，每个动作独立 id，不走 base+load+dir，单列以备清单化）。 ──
        [Header("Phase 3 待接入：姿势 emote 起始基址（a000_080000 → 80000，仅作清单锚点）")]
        [Tooltip("emote 段起始（如 BOW a000_080000 → 80000）。emote 为离散 id，运行时按具体动作号查，不做 load/dir 组合。")]
        public int gestureBase = 80000;

        [Header("运行时默认值（接入负重/武器系统前的兜底）")]
        [Tooltip("当前项目尚无装备负重系统：运行时统一用此负重档（0轻/1中/2重/3超重）。接入后改由角色状态提供。")]
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
