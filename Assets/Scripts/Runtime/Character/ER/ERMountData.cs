using System;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 单个 ER dummy（挂点）的绑定姿势数据，直接从 c0000.flver 解包而来。
    /// 因所有武器/收纳 dummy 的空间骨(Model_Dmy_*)恒在原点，dummy 的“绑定世界矩阵”= 其自身局部帧，
    /// 即 (pos, right/up/fwd)。这三组轴向来自 FLVER 的 forward/upward 正交化。
    /// 坐标全部为【FLVER 坐标系】，运行时用自标定基变换 B 转到 Unity 角色空间。
    /// </summary>
    [Serializable]
    public class ERDummyFrame
    {
        public int referenceId;
        public string spaceBone;
        public string attachBone;
        public bool followsAttach;

        //  FLVER 空间下的 dummy 绑定帧
        public Vector3 pos;
        public Vector3 right;
        public Vector3 up;
        public Vector3 fwd;
    }

    /// <summary>标定骨：某骨在 FLVER 参考姿势下的世界位置，用来最小二乘求解 FLVER→Unity 基变换 B。</summary>
    [Serializable]
    public class ERCalibBone
    {
        public string name;
        public Vector3 flverPos;
    }

    /// <summary>
    /// ER 挂载数据（角色级，全武器共用一份）。由 <c>Tools/ER/导入挂载数据(JSON)</c> 从
    /// <c>Assets/_ELDENRING_REF/er_mount_data.json</c>（dump_c0000_dummies.py 生成）导入。
    /// 忠实复刻 DSAS：dummy 表在角色身上(c0000)，武器仅按 WepAbsorpPosParam 引用 dummy refID。
    /// </summary>
    [CreateAssetMenu(menuName = "LZ/ER/ER Mount Data", fileName = "ERMountData")]
    public class ERMountData : ScriptableObject
    {
        public List<ERDummyFrame> dummies = new List<ERDummyFrame>();
        public List<ERCalibBone> calibrationBones = new List<ERCalibBone>();

        public ERDummyFrame GetDummy(int refId)
        {
            for (int i = 0; i < dummies.Count; i++)
                if (dummies[i].referenceId == refId) return dummies[i];
            return null;
        }
    }
}
