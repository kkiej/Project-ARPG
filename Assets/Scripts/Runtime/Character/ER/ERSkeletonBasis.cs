using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 求解 FLVER→Unity 的全局基变换 B（含旋转/镜像/平移）。
    /// DSAS 里 dummy 数据是 FLVER 坐标，且 FLVER 参考姿势与 Unity 骨架完全不同（脊柱沿 +X、腿沿 +Y…），
    /// 手猜坐标必错，故用【多根同名骨的 FLVER-bind 位置 vs Unity-bind 位置】做仿射最小二乘，解出 B。
    /// 解出的 B 满足 <c>unityPos ≈ B.MultiplyPoint3x4(flverPos)</c>，方向用 <c>B.MultiplyVector</c>。
    /// </summary>
    public static class ERSkeletonBasis
    {
        /// <summary>
        /// 仿射最小二乘：求 3x4 矩阵(A|t)使 dst ≈ A·src + t。返回 Matrix4x4(行 0..2 为解，行 3 = 0,0,0,1)。
        /// 需至少 4 组不共面的对应点；本项目用 ~20 根遍布全身的骨，条件良好且天然含镜像。
        /// </summary>
        public static bool TrySolveAffine(IList<Vector3> src, IList<Vector3> dst, out Matrix4x4 result)
        {
            result = Matrix4x4.identity;
            if (src == null || dst == null || src.Count != dst.Count || src.Count < 4)
                return false;

            //  正规方程 M(4x4) = Σ u·uᵀ，u = [sx,sy,sz,1]；三个输出轴共用 M。
            double[,] M = new double[4, 4];
            double[] bx = new double[4], by = new double[4], bz = new double[4];

            for (int i = 0; i < src.Count; i++)
            {
                double[] u = { src[i].x, src[i].y, src[i].z, 1.0 };
                for (int r = 0; r < 4; r++)
                {
                    for (int c = 0; c < 4; c++)
                        M[r, c] += u[r] * u[c];
                    bx[r] += u[r] * dst[i].x;
                    by[r] += u[r] * dst[i].y;
                    bz[r] += u[r] * dst[i].z;
                }
            }

            if (!Solve4x4(M, bx, out double[] wx)) return false;
            if (!Solve4x4(M, by, out double[] wy)) return false;
            if (!Solve4x4(M, bz, out double[] wz)) return false;

            result.SetRow(0, new Vector4((float)wx[0], (float)wx[1], (float)wx[2], (float)wx[3]));
            result.SetRow(1, new Vector4((float)wy[0], (float)wy[1], (float)wy[2], (float)wy[3]));
            result.SetRow(2, new Vector4((float)wz[0], (float)wz[1], (float)wz[2], (float)wz[3]));
            result.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
            return true;
        }

        /// <summary>把一个 FLVER 空间 dummy 帧转到 Unity 角色空间的 TRS 矩阵。</summary>
        /// <remarks>
        /// 方向(fwd/up)用 <see cref="Quaternion.LookRotation"/> 重建为合法的 Unity(左手)旋转，
        /// 天然规避 B 含镜像时直接取矩阵旋转会失效的问题——只保留朝向，武器网格自身手性由 FBX 导入处理。
        /// </remarks>
        public static Matrix4x4 ConvertDummyFrame(Matrix4x4 B, ERDummyFrame d)
        {
            Vector3 posU = B.MultiplyPoint3x4(d.pos);
            Vector3 upU = B.MultiplyVector(d.up);
            Vector3 fwdU = B.MultiplyVector(d.fwd);

            if (upU.sqrMagnitude < 1e-10f) upU = Vector3.up;
            if (fwdU.sqrMagnitude < 1e-10f) fwdU = Vector3.forward;

            Quaternion rotU = Quaternion.LookRotation(fwdU.normalized, upU.normalized);
            return Matrix4x4.TRS(posU, rotU, Vector3.one);
        }

        //  4x4 线性方程组高斯消元（带部分主元）。
        private static bool Solve4x4(double[,] Ain, double[] bin, out double[] x)
        {
            x = new double[4];
            double[,] a = (double[,])Ain.Clone();
            double[] b = (double[])bin.Clone();

            for (int col = 0; col < 4; col++)
            {
                int piv = col;
                double best = System.Math.Abs(a[col, col]);
                for (int r = col + 1; r < 4; r++)
                {
                    double v = System.Math.Abs(a[r, col]);
                    if (v > best) { best = v; piv = r; }
                }
                if (best < 1e-12) return false;

                if (piv != col)
                {
                    for (int c = 0; c < 4; c++) { (a[col, c], a[piv, c]) = (a[piv, c], a[col, c]); }
                    (b[col], b[piv]) = (b[piv], b[col]);
                }

                double diag = a[col, col];
                for (int r = 0; r < 4; r++)
                {
                    if (r == col) continue;
                    double f = a[r, col] / diag;
                    if (f == 0) continue;
                    for (int c = col; c < 4; c++) a[r, c] -= f * a[col, c];
                    b[r] -= f * b[col];
                }
            }

            for (int i = 0; i < 4; i++) x[i] = b[i] / a[i, i];
            return true;
        }
    }
}
