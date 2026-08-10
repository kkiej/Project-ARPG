using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 忠实复刻 DSAS 的武器/剑鞘挂载（读表驱动，剑刃与鞘走同一套公式）。
    ///
    /// DSAS 权威公式（NewDummyPolyInfo / NewChrAsm.DoWPN）：
    ///   ReferenceMatrix = dummy 朝向·位移 · 空间骨(ParentBone)绑定世界      // 空间骨恒在原点 → = dummy 自身帧
    ///   AttachMatrix    = Inv(跟随骨绑定) · 跟随骨当前                        // 跟随骨的动画增量
    ///   CurrentMatrix   = ReferenceMatrix · AttachMatrix
    ///   武器世界        = RotX(180°) · CurrentMatrix (+ 翻转) · 角色世界
    ///
    /// 因空间骨恒定，把武器作为“跟随骨的子物体”后，其相对跟随骨的局部变换是【常量】：
    ///   localOffset = Inv(跟随骨绑定) · (dummy 绑定 · flip)
    /// 于是每帧的跟随由 Unity 父子关系自动完成，无需逐帧驱动。剑刃(跟随 R_Weapon)与鞘(跟随 Pelvis)
    /// 只是 dummy refID / 跟随骨不同，代码完全一致。
    ///
    /// dummy 数据是 FLVER 坐标，用 <see cref="ERSkeletonBasis"/> 从骨架自标定出的基变换 B 转到 Unity 角色空间，
    /// 从根本上避免手猜坐标系。
    /// </summary>
    public class ERWeaponMounter
    {
        private Transform charRoot;
        private ERMountData data;
        private Matrix4x4 basis = Matrix4x4.identity;
        private bool solved;

        //  角色空间(相对 charRoot)下的骨绑定矩阵 + 骨 Transform（Calibrate 时于绑定姿势抓取）。
        private readonly Dictionary<string, Matrix4x4> boneBind = new Dictionary<string, Matrix4x4>();
        private readonly Dictionary<string, Transform> boneTf = new Dictionary<string, Transform>();

        public bool IsReady => solved;

        /// <summary>
        /// 于【绑定姿势】(Awake、动画生效前)标定：抓取标定骨 + 各 dummy 跟随骨的绑定矩阵，最小二乘求解 B。
        /// </summary>
        public void Calibrate(Transform root, ERMountData mountData)
        {
            charRoot = root;
            data = mountData;
            solved = false;
            boneBind.Clear();
            boneTf.Clear();
            if (data == null || charRoot == null) return;

            var names = new HashSet<string>();
            foreach (var cb in data.calibrationBones)
                if (!string.IsNullOrEmpty(cb.name)) names.Add(cb.name);
            foreach (var d in data.dummies)
                if (!string.IsNullOrEmpty(d.attachBone)) names.Add(d.attachBone);

            Matrix4x4 rootInv = charRoot.worldToLocalMatrix;
            foreach (var nm in names)
            {
                var t = FindDescendant(charRoot, nm);
                if (t == null) continue;
                boneTf[nm] = t;
                boneBind[nm] = rootInv * t.localToWorldMatrix;
            }

            var src = new List<Vector3>();
            var dst = new List<Vector3>();
            foreach (var cb in data.calibrationBones)
            {
                if (boneBind.TryGetValue(cb.name, out var m))
                {
                    src.Add(cb.flverPos);
                    dst.Add(m.GetColumn(3));
                }
            }

            solved = ERSkeletonBasis.TrySolveAffine(src, dst, out basis);
            if (!solved)
            {
                Debug.LogWarning($"[ERWeaponMounter] 基变换标定失败（有效标定骨 {src.Count} 组，需≥4 且不共面）。将回退旧挂载。");
                return;
            }

            //  一次性诊断：打印 B 对已知骨的映射残差 + 关键 dummy 的映射位置(角色空间)。
            LogCalibrationDiagnostics();
        }

        private void LogCalibrationDiagnostics()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("[ERWeaponMounter] 标定诊断（角色空间，X:左右 Y:上下 Z:前后）：");
            foreach (var nm in new[] { "Pelvis", "R_Hand", "L_Hand", "R_Weapon", "L_Weapon" })
            {
                var cb = data.calibrationBones.Find(x => x.name == nm);
                if (cb != null && boneBind.TryGetValue(nm, out var m))
                {
                    Vector3 mapped = basis.MultiplyPoint3x4(cb.flverPos);
                    Vector3 actual = m.GetColumn(3);
                    sb.AppendLine($"  {nm}: B(flver)={fmt(mapped)}  actualUnity={fmt(actual)}  残差={(mapped - actual).magnitude:F3}m");
                }
            }
            foreach (var d in data.dummies)
            {
                Vector3 mapped = basis.MultiplyPoint3x4(d.pos);
                sb.AppendLine($"  dummy{d.referenceId} → 角色空间 {fmt(mapped)} (attach={d.attachBone})");
            }
            Debug.Log(sb.ToString());
        }

        private static string fmt(Vector3 v) => $"({v.x:F3},{v.y:F3},{v.z:F3})";

        /// <summary>
        /// 按 dummy refID 把模型挂到其跟随骨上（复刻 DSAS）。成功返回 true 并输出所用跟随骨。
        /// </summary>
        /// <param name="flipEuler">DSAS 的 ER 固定翻转(RotX180)在 Unity 下的等价常量（dummy 局部叠加）。</param>
        /// <param name="finePos">位置微调（角色空间：X左右 / Y上下 / Z前后）。</param>
        /// <param name="fineRot">朝向微调（角色空间：叠加在角色朝向之上；fineRot=0 时鞘按 FBX 原生朝向对齐角色轴）。</param>
        public bool TryMount(GameObject model, int dummyRef, Vector3 flipEuler,
            Vector3 finePos, Vector3 fineRot, Vector3 scale, out Transform attachBone)
        {
            attachBone = null;
            if (!solved || data == null || model == null || dummyRef < 0) return false;

            var d = data.GetDummy(dummyRef);
            if (d == null) return false;
            if (!boneTf.TryGetValue(d.attachBone, out var attachTf)) return false;
            attachBone = attachTf;

            //  用【稳定的角色根】把模型放到角色空间 dummy 位置对应的世界点，再 SetParent 到跟随骨。
            //  位置取 B 的角色空间结果(左右已正确)；朝向脱离噪声大的 B 旋转，改用"角色朝向×微调"，可预测、便于逐步调正。
            Matrix4x4 dummyMat = ERSkeletonBasis.ConvertDummyFrame(basis, d);        // 角色空间
            Vector3 posChar = dummyMat.GetColumn(3);                                 // 角色空间位置(左右正确)
            posChar += (Vector3)(finePos);                                           // 角色空间微调(X左右/Y上下/Z前后)
            Vector3 wpos = charRoot.localToWorldMatrix.MultiplyPoint3x4(posChar);    // → 世界

            Quaternion finalRot = charRoot.rotation * Quaternion.Euler(fineRot);     // 角色朝向 × 微调

            model.transform.SetParent(attachTf, false);                 // 先挂到跟随骨(随其运动)
            model.transform.localScale = (scale == Vector3.zero) ? Vector3.one : scale;
            model.transform.rotation = finalRot;                        // 再按世界目标摆位姿(Unity 自动转局部)
            model.transform.position = wpos;
            return true;
        }

        private static Transform FindDescendant(Transform root, string exact)
        {
            foreach (Transform c in root)
            {
                if (c.name == exact) return c;
                var f = FindDescendant(c, exact);
                if (f != null) return f;
            }
            return null;
        }
    }
}
