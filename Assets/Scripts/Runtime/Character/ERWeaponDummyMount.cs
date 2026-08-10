using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 复刻 DSAS 的 ER 双骨 dummy 武器挂载，每帧 LateUpdate 依当前动画姿态重算武器世界变换。
    ///
    /// 源码对应（DSAnimStudioNETCore）：
    ///   NewChrAsm.cs:721-726        weaponWorld = RotX(180°) * dummy.CurrentMatrix
    ///   NewDummyPolyInfo.cs:34,82-87 CurrentMatrix = Reference * Attach；
    ///                                Reference = dummyLocal * spaceBone.FKMatrix（空间骨=ParentBoneIndex）
    ///   NewDummyPolyManager.cs:1258  Attach = 跟随骨(AttachBoneIndex) 的动画增量
    ///   NewAnimSkeleton_FLVER.cs:139 增量 = Invert(bone.ReferenceFKMatrix) * bone.FKMatrix
    ///
    /// 换算到 Unity（用骨骼 localToWorldMatrix 作 FK，行/列向量约定翻转后）：
    ///   W(t) = A(t) · A0⁻¹ · S(t) · Ld · Flip · Offset
    ///     A(t)=跟随骨当前世界矩阵  A0=跟随骨绑定姿势世界矩阵
    ///     S(t)=空间骨当前世界矩阵  Ld=dummy 相对空间骨的局部矩阵(= S0⁻¹·Dw0)
    ///     Flip=RotX(180°) 的 Unity 等价（可调）  Offset=武器自身位姿微调
    ///
    /// 常量 A0、Ld 必须用【绑定姿势】捕获（PlayerEquipmentManager.Awake，动画未生效前），
    /// 由 <see cref="Configure"/> 传入。
    /// </summary>
    public class ERWeaponDummyMount : MonoBehaviour
    {
        [Header("骨骼引用（运行时读当前动画姿态）")]
        [Tooltip("空间骨：ER 的 ParentBoneIndex，通常是 Model_Dmy_AttachWeapon。必须被动画驱动，否则武器停在参考点。")]
        public Transform spaceBone;
        [Tooltip("跟随骨：ER 的 AttachBoneIndex，通常是 R_Weapon / L_Weapon。")]
        public Transform attachBone;

        [Header("绑定姿势常量（Awake 捕获后传入）")]
        [Tooltip("Ld = 空间骨绑定世界⁻¹ · dummy 绑定世界。dummy 相对空间骨的固定局部变换。")]
        public Matrix4x4 dummyLocalInSpace = Matrix4x4.identity;
        [Tooltip("A0⁻¹ = 跟随骨绑定姿势世界矩阵的逆。")]
        public Matrix4x4 attachRestInverse = Matrix4x4.identity;

        [Header("朝向修正与武器微调")]
        [Tooltip("DSAS 的 RotX(180°) 在 Unity 轴系的等价修正。默认 (180,0,0)；朝向不对按需校准。")]
        public Vector3 flipEuler = new Vector3(180f, 0f, 0f);
        public Vector3 positionOffset = Vector3.zero;
        public Vector3 rotationOffset = Vector3.zero;
        public Vector3 scale = Vector3.one;

        [Header("自检")]
        [Tooltip("开启后首帧检测空间骨是否随动画移动，未移动会告警（多半是动画没驱动 Model_Dmy_AttachWeapon）。")]
        public bool selfCheckSpaceBoneAnimated = true;

        private bool spaceBoneRestCaptured;
        private Matrix4x4 spaceBoneRest;
        private bool warnedSpaceStatic;

        /// <summary>由挂载方一次性配置并立即摆位。</summary>
        public void Configure(
            Transform space, Transform attach,
            Matrix4x4 ld, Matrix4x4 a0Inverse,
            Vector3 flip, Vector3 posOffset, Vector3 rotOffset, Vector3 scl)
        {
            spaceBone = space;
            attachBone = attach;
            dummyLocalInSpace = ld;
            attachRestInverse = a0Inverse;
            flipEuler = flip;
            positionOffset = posOffset;
            rotationOffset = rotOffset;
            scale = (scl == Vector3.zero) ? Vector3.one : scl;

            spaceBoneRestCaptured = false;
            warnedSpaceStatic = false;

            ApplyNow();
        }

        private void LateUpdate()
        {
            ApplyNow();
        }

        private void ApplyNow()
        {
            if (spaceBone == null || attachBone == null)
                return;

            Matrix4x4 S = spaceBone.localToWorldMatrix;
            Matrix4x4 A = attachBone.localToWorldMatrix;

            Matrix4x4 flip = Matrix4x4.Rotate(Quaternion.Euler(flipEuler));
            Matrix4x4 offset = Matrix4x4.TRS(positionOffset, Quaternion.Euler(rotationOffset), Vector3.one);

            //  W = A(t) · A0⁻¹ · S(t) · Ld · Flip · Offset
            Matrix4x4 W = A * attachRestInverse * S * dummyLocalInSpace * flip * offset;

            Vector3 pos = W.GetColumn(3);
            //  用列向量取朝向，避免含骨骼缩放时 Matrix4x4.rotation 抽取不稳
            Vector3 forward = W.GetColumn(2);
            Vector3 up = W.GetColumn(1);
            Quaternion rot = (forward.sqrMagnitude > 1e-8f && up.sqrMagnitude > 1e-8f)
                ? Quaternion.LookRotation(forward, up)
                : transform.rotation;

            transform.SetPositionAndRotation(pos, rot);

            //  世界缩放对齐到 scale：补偿父级 lossyScale，保证成品尺寸与 authoring 一致
            Vector3 pls = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
            transform.localScale = new Vector3(
                SafeDiv(scale.x, pls.x),
                SafeDiv(scale.y, pls.y),
                SafeDiv(scale.z, pls.z));

            SelfCheck(S);
        }

        private void SelfCheck(Matrix4x4 S)
        {
            if (!selfCheckSpaceBoneAnimated || warnedSpaceStatic)
                return;

            if (!spaceBoneRestCaptured)
            {
                spaceBoneRest = S;
                spaceBoneRestCaptured = true;
                return;
            }

            //  第二帧起，若空间骨世界矩阵始终没变，多半动画没驱动 Model_Dmy_AttachWeapon → 武器会停在参考点
            float delta = (S.GetColumn(3) - spaceBoneRest.GetColumn(3)).magnitude;
            if (delta < 1e-5f)
            {
                //  给动画几帧启动时间后仍未动才告警：这里用位置几乎不变作为近似
                Debug.LogWarning(
                    $"[ERWeaponDummyMount] 空间骨 '{spaceBone.name}' 在动画中未见移动。" +
                    "若武器停在离手很远的参考点，说明导入的动画未驱动该骨（Humanoid 重定向会丢辅助骨）。" +
                    "需用 Generic Rig 保留 c0000 全部骨骼并保留其关键帧。", this);
                warnedSpaceStatic = true;
            }
            else
            {
                //  一旦观察到移动即认定正常，停止自检
                warnedSpaceStatic = true;
            }
        }

        private static float SafeDiv(float a, float b)
        {
            return Mathf.Abs(b) < 1e-6f ? a : a / b;
        }
    }
}
