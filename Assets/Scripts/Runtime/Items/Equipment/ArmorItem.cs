using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class ArmorItem : EquipmentItem
    {
        [Header("Equipment Absorption Bonus")]
        public float physicalDamageAbsorption;
        public float magicDamageAbsorption;
        public float fireDamageAbsorption;
        public float holyDamageAbsorption;
        public float lightningDamageAbsorption;

        [Header("Equipment Resistance Bonus")]
        public float immunity;      // 腐败与中毒抗性
        public float robustness;    // 出血与冰冻抗性
        public float focus;         // 狂乱与睡眠抗性
        public float vitality;      // 死亡诅咒抗性

        [Header("Poise")]
        public float poise;

        public EquipmentModel[] equipmentModels;

        [Header("Modular Part (ER 共享骨架换装)")]
        [Tooltip("ER 部件基础编号，仅填数字部分，如 1350。\n" +
                 "运行时会按 槽位前缀(HD/BD/AM/LG) + 性别(M/F) + 此编号 拼出部件名（如 BD_M_1350）去 EquipmentPartCatalog 查 prefab。\n" +
                 "留空 = 该装备不走模块化（沿用旧的预置模型方案）。")]
        public string modularPartCode;
    }
}