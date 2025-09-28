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

        //  ARMOR MODELS
    }
}