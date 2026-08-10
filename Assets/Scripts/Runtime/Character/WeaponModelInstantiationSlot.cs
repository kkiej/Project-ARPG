using UnityEngine;

namespace LZ
{
    public class WeaponModelInstantiationSlot : MonoBehaviour
    {
        public WeaponModelSlot weaponSlot;
        public GameObject currentWeaponModel;

        [Tooltip("加载后对武器【骨架 Armature】施加的基础朝向修正（抵消 ER 武器 FBX 与骨架 FBX 的轴向差异）：\n" +
                 "默认 X 清零、Z 转 90°（预制体根保持 0,0,0 不动）。会与武器自身的 weaponModelRotationOffset 叠加。\n" +
                 "【仅旧的骨骼挂点回退路径使用】。走 ER dummy 挂点时改用 dummyFlipRotation。")]
        public Vector3 baseRotationCorrection = new Vector3(0f, 0f, 90f);

        [Tooltip("ER dummy 挂点专用：DSAS 的 RotX(180°) 翻转在 Unity 轴系下的等价修正。\n" +
                 "武器根直接对齐 dummy 后叠加此旋转（再叠加武器自身 weaponModelRotationOffset）。\n" +
                 "默认 (180,0,0)；若朝向不对按需校准（可能是 (0,180,0)/(0,0,180) 之一）。")]
        public Vector3 dummyFlipRotation = new Vector3(180f, 0f, 0f);

        public void UnloadWeapon()
        {
            if (currentWeaponModel != null)
            {
                Destroy(currentWeaponModel);
            }
        }

        public void PlaceWeaponModelIntoSlot(GameObject weaponModel)
        {
            PlaceWeaponModelIntoSlot(weaponModel, Vector3.zero, Vector3.zero, Vector3.one);
        }

        /// <summary>
        /// 挂载武器。预制体根保持零位姿（ER 武器根即 0,0,0），仅叠加武器自身的位置偏移；
        /// 真正的朝向修正落在武器【骨架 Armature】上：施加 <see cref="baseRotationCorrection"/>
        /// （默认 X 清零、Z 转 90°）叠加武器自身 localEulerOffset，抵消 FBX 轴向差异。
        /// 无骨架（纯静态网格）时，修正退回加在预制体根上。
        /// </summary>
        public void PlaceWeaponModelIntoSlot(GameObject weaponModel, Vector3 localPositionOffset, Vector3 localEulerOffset, Vector3 localScale)
        {
            currentWeaponModel = weaponModel;
            RemoveDummyDriver(weaponModel);
            //  剑刃：挂到本槽所在的 ER attach 骨（R_Weapon/L_Weapon/L_Shield），走统一挂载核心。
            PlaceModelOnAttachBone(weaponModel, transform, baseRotationCorrection, localPositionOffset, localEulerOffset, localScale);
        }

        /// <summary>
        /// 【统一挂载核心 —— 复刻 DSAS dummy→attach 骨的刚性跟随】
        /// c0000.flver 实测：武器 dummy(如剑刃 ref=20 / 鞘 ref=30) 的 <c>follows_attach=True</c>，
        /// 空间骨(Model_Dmy_AttachWeapon / Model_Dmy_Storage) 相对角色恒定，故 DSAS 公式
        /// <c>CurrentMatrix = dummyRest·Inv(attachRest)·attachCurrent</c> 对"武器作为 attach 骨子物体"
        /// 塌缩为常量局部变换 —— 即"把武器刚性挂到 dummy 的 attach 骨"。剑刃(attach=R_Weapon)与
        /// 剑鞘(attach=Pelvis) 因此走完全相同的这段代码，仅 attach 骨与偏移不同。
        /// 预制体根只承载位置偏移；朝向修正 <paramref name="baseRotation"/>(FLVER→Unity 轴向差的等价常量)
        /// 叠加每件武器的 <paramref name="localEulerOffset"/> 落在【骨架 Armature】上。无骨架时退回加在根上。
        /// </summary>
        public static void PlaceModelOnAttachBone(GameObject model, Transform attachBone, Vector3 baseRotation,
            Vector3 localPositionOffset, Vector3 localEulerOffset, Vector3 localScale)
        {
            model.transform.SetParent(attachBone, false);

            Vector3 scale = localScale == Vector3.zero ? Vector3.one : localScale;
            model.transform.localPosition = localPositionOffset;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = scale;

            Quaternion correction = Quaternion.Euler(baseRotation) * Quaternion.Euler(localEulerOffset);
            Transform armature = FindArmature(model);
            if (armature != null)
                armature.localRotation = correction;
            else
                model.transform.localRotation = correction; //  纯静态网格：无骨架，退回加在根上
        }

        /// <summary>
        /// ER 数据驱动挂载：把武器根直接挂到骨架里的 dummy 挂点(mountPoint)上，
        /// 复刻 DSAS 的 <c>武器矩阵 = RotX(180°) × dummy矩阵</c>：
        /// 武器根 SetParent 到 dummy → 本地位移取武器微调 → 本地旋转 = <see cref="dummyFlipRotation"/> 叠加武器自身 rotationOffset。
        /// dummy 已带 ER 握持朝向，故无需再动武器 Armature。
        /// </summary>
        public void PlaceWeaponModelOnMount(GameObject weaponModel, Transform mountPoint, Vector3 localPositionOffset, Vector3 localEulerOffset, Vector3 localScale)
        {
            currentWeaponModel = weaponModel;
            weaponModel.transform.SetParent(mountPoint, false);

            Vector3 scale = localScale == Vector3.zero ? Vector3.one : localScale;
            weaponModel.transform.localPosition = localPositionOffset;
            weaponModel.transform.localRotation = Quaternion.Euler(dummyFlipRotation) * Quaternion.Euler(localEulerOffset);
            weaponModel.transform.localScale = scale;
        }

        /// <summary>移除武器上残留的 ER 双骨挂载驱动（走非驱动挂点路径时，避免驱动继续每帧覆盖位姿）。</summary>
        private static void RemoveDummyDriver(GameObject weaponModel)
        {
            var driver = weaponModel.GetComponent<ERWeaponDummyMount>();
            if (driver != null)
                Destroy(driver);
        }

        /// <summary>
        /// 取武器的骨架 Armature：优先按名字匹配含 "Armature" 的子物体，
        /// 回退为蒙皮根骨的父级（通常即 Armature），再回退为蒙皮根骨本身。
        /// </summary>
        private static Transform FindArmature(GameObject weaponModel)
        {
            foreach (Transform t in weaponModel.GetComponentsInChildren<Transform>(true))
            {
                if (t == weaponModel.transform) continue;
                if (t.name.IndexOf("Armature", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return t;
            }

            var smr = weaponModel.GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (smr == null) return null;
            Transform bone = (smr.bones != null && smr.bones.Length > 0 && smr.bones[0] != null) ? smr.bones[0] : smr.rootBone;
            if (bone == null) return null;
            return (bone.parent != null && bone.parent != weaponModel.transform) ? bone.parent : bone;
        }

        public void PlaceWeaponModelInUnequippedSlot(GameObject weaponModel, WeaponClass weaponClass, PlayerManager player)
        {
            // TO DO, MOVE WEAPON ON BACK CLOSER OR MORE OUTWARD DEPENDING ON CHEST EQUIPMENT (SO IT DOESNT APPEAR TO FLOAT)

            currentWeaponModel = weaponModel;
            RemoveDummyDriver(weaponModel);
            weaponModel.transform.parent = transform;

            switch (weaponClass)
            {
                case WeaponClass.StraightSword:
                    weaponModel.transform.localPosition = new Vector3(0.064f, 0f, -0.06f);
                    weaponModel.transform.localRotation = Quaternion.Euler(194, 90, -0.22f);
                    break;
                case WeaponClass.Spear:
                    weaponModel.transform.localPosition = new Vector3(0.064f, 0f, -0.06f);
                    weaponModel.transform.localRotation = Quaternion.Euler(194, 90, -0.22f);
                    break;
                case WeaponClass.MediumShield:
                    weaponModel.transform.localPosition = new Vector3(0.074f, -0.002f, 0.069f);
                    weaponModel.transform.localRotation = Quaternion.Euler(-180.235f, 180.202f, -15.65601f);
                    break;
                default:
                    break;
            }
        }
    }
}