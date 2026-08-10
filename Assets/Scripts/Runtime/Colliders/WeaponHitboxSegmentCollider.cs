using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 一段武器命中胶囊（对应 ER 一段 hitN：dummyA↔dummyB）。
    /// 挂在 <see cref="MeleeWeaponDamageCollider"/> 的子物体上，自身带一个定向 <see cref="CapsuleCollider"/>：
    /// 生成时已把子物体本地 Y 轴对齐到 dummyA→dummyB，故胶囊 direction=Y、center=0，
    /// 运行期只需按当前攻击的 hitN_Radius 改半径（高度随半径重算，端帽仍落在两 dummy 上）。
    /// <para/>
    /// 触发命中转交父级 <see cref="MeleeWeaponDamageCollider"/> 统一结算（共用「本次挥砍已伤害列表」，多段不会重复打）。
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    public class WeaponHitboxSegmentCollider : MonoBehaviour
    {
        [Header("ER 段标识（生成工具回填）")]
        public int dmyA = -1;
        public int dmyB = -1;                 // -1 = 单点球
        [Tooltip("两 dummy 间的固定距离（米）。单点球为 0。半径变化时高度 = baseDistance + 2*radius。")]
        public float baseDistance;
        [Tooltip("并集里该段的最大半径（米）。解析不到具体攻击时按此回退启用。")]
        public float defaultRadius = 0.06f;

        [Tooltip("所属近战伤害结算器。生成工具回填；为空时运行期向上查找。")]
        public MeleeWeaponDamageCollider owner;

        private CapsuleCollider _capsule;

        private void Awake()
        {
            _capsule = GetComponent<CapsuleCollider>();
            _capsule.isTrigger = true;
            _capsule.enabled = false;
            _capsule.direction = 1; // Y
            if (owner == null)
                owner = GetComponentInParent<MeleeWeaponDamageCollider>();
        }

        /// <summary>按给定半径启用该段胶囊。</summary>
        public void Activate(float radius)
        {
            if (_capsule == null) _capsule = GetComponent<CapsuleCollider>();
            _capsule.radius = radius;
            _capsule.height = baseDistance + 2f * radius;
            _capsule.enabled = true;
        }

        public void Deactivate()
        {
            if (_capsule == null) _capsule = GetComponent<CapsuleCollider>();
            _capsule.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (owner != null)
                owner.HandleContact(other, transform.position);
        }
    }
}
