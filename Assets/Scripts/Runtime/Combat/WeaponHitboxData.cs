using System;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 单个命中段（对应 AtkParam_Pc 的一段 hitN）。
    /// ER 命中模型：<c>hitN_DmyPoly1</c> → <c>hitN_DmyPoly2</c> 两个 dummy 点之间连一根胶囊，半径 = <c>hitN_Radius</c>。
    /// 当 <see cref="dmyB"/> = -1 时表示单点球（只在 <see cref="dmyA"/> 处放一个半径 = radius 的球）。
    /// </summary>
    [Serializable]
    public struct WeaponHitSegment
    {
        public int dmyA;      // hitN_DmyPoly1
        public int dmyB;      // hitN_DmyPoly2，-1 = 单点球
        public float radius;  // hitN_Radius（米）

        public bool IsSphere => dmyB < 0;

        /// <summary>用于跨攻击去重/匹配的 key（只看 dummy 对，不看半径）。</summary>
        public long Key => ((long)dmyA << 32) ^ (uint)dmyB;
    }

    /// <summary>某个具体攻击（AtkParam ID）用到的命中段集合。</summary>
    [Serializable]
    public struct WeaponAttackHitboxes
    {
        [Tooltip("AtkParam_Pc 的 ID = behaviorVariationId*1000 + (动画槽号-30000)。")]
        public int atkParamId;
        public WeaponHitSegment[] segments;
    }

    /// <summary>
    /// 单件武器的 ER 命中框数据（由 <c>WeaponHitboxDataBuilder</c> 从
    /// EquipParamWeapon + BehaviorParam_PC + AtkParam_Pc 解析生成）。
    /// <para/>
    /// 数据链：武器 <see cref="behaviorVariationId"/> → BehaviorParam_PC(variationId 相同、refType=0) 的一组 refId
    /// = 一组 AtkParam ID → 每个 AtkParam 的 hit0..hit15（dummy 对 + 半径）。
    /// <para/>
    /// 运行期由 <see cref="MeleeWeaponDamageCollider"/> 按「当前攻击动画槽号」解析出 AtkParam ID，
    /// 取对应命中段并只激活这些段的胶囊（逐攻击精确）；解析不到时回退到 <see cref="unionSegments"/>（整刀刃）。
    /// </summary>
    [CreateAssetMenu(menuName = "Character/Weapon Hitbox Data")]
    public class WeaponHitboxData : ScriptableObject
    {
        [Header("来源标识")]
        [Tooltip("武器模型号，如 WP_A_0200 → 200。")]
        public int weaponModelId;
        [Tooltip("EquipParamWeapon.behaviorVariationId，用于把动画槽号换算为 AtkParam ID。")]
        public int behaviorVariationId;

        [Header("并集命中段（去重后的整刀刃，用于生成胶囊 + 解析不到攻击时的回退）")]
        [Tooltip("跨该武器所有攻击去重后的段（同 dummy 对取最大半径）。碰撞体生成工具按此生成一根/一段胶囊。")]
        public WeaponHitSegment[] unionSegments = Array.Empty<WeaponHitSegment>();

        [Header("逐攻击命中段（AtkParam ID → 段），运行期逐攻击精确切换用")]
        public WeaponAttackHitboxes[] perAttack = Array.Empty<WeaponAttackHitboxes>();

        // atkParamId → segments，首次用到时构建
        private Dictionary<int, WeaponHitSegment[]> _map;

        private void BuildMap()
        {
            _map = new Dictionary<int, WeaponHitSegment[]>(perAttack != null ? perAttack.Length : 0);
            if (perAttack == null) return;
            foreach (var e in perAttack)
                if (!_map.ContainsKey(e.atkParamId))
                    _map[e.atkParamId] = e.segments ?? Array.Empty<WeaponHitSegment>();
        }

        /// <summary>
        /// 把「当前攻击动画槽号（ER animId = 类别*1_000_000 + 槽号，如 23030000）」换算为 AtkParam ID。
        /// 公式：atkId = behaviorVariationId*1000 + (槽号 - 30000)。槽号取 animId % 1_000_000。
        /// 返回值可能落在无效区间；调用方用 <see cref="GetSegments"/> 命中失败即回退。
        /// </summary>
        public int ResolveAtkParamId(int animId)
        {
            if (animId <= 0) return -1;
            int slot = animId % 1_000_000;
            return behaviorVariationId * 1000 + (slot - 30000);
        }

        /// <summary>取某 AtkParam ID 的命中段；无该攻击数据返回 null（调用方回退 <see cref="unionSegments"/>）。</summary>
        public WeaponHitSegment[] GetSegments(int atkParamId)
        {
            if (_map == null) BuildMap();
            return _map.TryGetValue(atkParamId, out var segs) ? segs : null;
        }
    }
}
