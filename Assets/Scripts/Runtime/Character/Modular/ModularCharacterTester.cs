using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 阶段 0 验证助手。把它挂在装有 <see cref="ModularCharacterAssembler"/> 的角色上，
    /// 在 Inspector 指定一个测试部件 prefab，然后用组件右键菜单（⋮ -> 这些方法）一键验证：
    ///   1) Validate Test Part Bones —— 检查骨骼名是否与主骨骼一致（重绑定前置条件）
    ///   2) Equip Test Part          —— 实例化并重绑定到主骨骼（运行时观察是否跟着动画动、是否穿模）
    ///   3) Unequip Test Part        —— 卸下
    /// 验证通过后即可删除本组件，进入阶段 2 正式对接装备流程。
    /// </summary>
    [RequireComponent(typeof(ModularCharacterAssembler))]
    public class ModularCharacterTester : MonoBehaviour
    {
        [Tooltip("用于阶段 0 验证的单个部件 prefab（如一件躯干）。")]
        [SerializeField] private GameObject testPartPrefab;

        [SerializeField] private BodySlot testSlot = BodySlot.Torso;

        private ModularCharacterAssembler assembler;

        private ModularCharacterAssembler Assembler
        {
            get
            {
                if (assembler == null) assembler = GetComponent<ModularCharacterAssembler>();
                return assembler;
            }
        }

        [ContextMenu("0. Diagnose (打印两边骨骼名对照)")]
        public void DiagnoseTestPart()
        {
            if (testPartPrefab == null)
            {
                Debug.LogWarning("[ModularCharacterTester] 未指定 testPartPrefab。", this);
                return;
            }
            Assembler.Diagnose(testPartPrefab);
        }

        [ContextMenu("1. Validate Test Part Bones")]
        public void ValidateTestPartBones()
        {
            if (testPartPrefab == null)
            {
                Debug.LogWarning("[ModularCharacterTester] 未指定 testPartPrefab。", this);
                return;
            }

            List<string> missing = Assembler.ValidateBones(testPartPrefab);
            if (missing.Count == 0)
            {
                Debug.Log($"[ModularCharacterTester] ✅ 骨骼校验通过：'{testPartPrefab.name}' 的所有骨骼都能在主骨架中找到。", this);
                return;
            }

            // 按类别归类：布料/Dummy 可忽略；护甲刚性骨会被重挂接；面部骨需关注
            var clothSim = new List<string>();
            var dummy = new List<string>();
            var face = new List<string>();
            var rigid = new List<string>();

            foreach (string bone in missing)
            {
                switch (ModularCharacterAssembler.ClassifyMissingBone(bone))
                {
                    case ModularCharacterAssembler.MissingBoneCategory.ClothSim: clothSim.Add(bone); break;
                    case ModularCharacterAssembler.MissingBoneCategory.Dummy: dummy.Add(bone); break;
                    case ModularCharacterAssembler.MissingBoneCategory.Face: face.Add(bone); break;
                    default: rigid.Add(bone); break;
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine($"[ModularCharacterTester] '{testPartPrefab.name}' 有 {missing.Count} 根骨骼不在主骨架中。主体形变骨（Spine/Pelvis/Clavicle/Thigh 等）若不在此列即表示已匹配 ✅。分类如下：");
            sb.AppendLine($"  • 布料/物理模拟骨 ×{clothSim.Count}（可忽略，Unity 无 Havok）");
            sb.AppendLine($"  • 挂点/Dummy ×{dummy.Count}（可忽略，不参与形变）");
            sb.AppendLine($"  • 护甲刚性骨等 ×{rigid.Count}（Equip 时会自动重挂接到主骨架以跟随运动）");
            if (rigid.Count > 0) sb.AppendLine($"      {string.Join(", ", rigid)}");
            sb.AppendLine($"  • 面部骨 ×{face.Count}（⚠️ 通用骨架未含面部骨，身体件可忽略；头部件需补面部骨）");
            if (face.Count > 0) sb.AppendLine($"      {string.Join(", ", face)}");

            // 只有面部骨缺失才升级为警告，其余为信息
            if (face.Count > 0)
                Debug.LogWarning(sb.ToString(), this);
            else
                Debug.Log(sb.ToString(), this);
        }

        [ContextMenu("2. Equip Test Part")]
        public void EquipTestPart()
        {
            if (testPartPrefab == null)
            {
                Debug.LogWarning("[ModularCharacterTester] 未指定 testPartPrefab。", this);
                return;
            }

            GameObject instance = Assembler.EquipPart(testSlot, testPartPrefab);
            if (instance != null)
                Debug.Log($"[ModularCharacterTester] 已装配 '{testPartPrefab.name}' 到槽位 {testSlot}。运行时请观察是否跟随动画、是否穿模。", instance);
        }

        [ContextMenu("3. Unequip Test Part")]
        public void UnequipTestPart()
        {
            Assembler.UnequipPart(testSlot);
            Debug.Log($"[ModularCharacterTester] 已卸下槽位 {testSlot}。", this);
        }
    }
}
