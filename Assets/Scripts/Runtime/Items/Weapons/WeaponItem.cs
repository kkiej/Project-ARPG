using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class WeaponItem : EquipmentItem
    {
        [Header("Animations")]
        public WeaponAnimationSet weaponAnimationSet;

        [Header("Moveset (FSM 数据驱动连招，与 weaponAnimationSet 并存)")]
        [Tooltip("开启 useStateMachine 后由该 MovesetData 驱动攻击；为空则该武器走旧的 WeaponItemAction 路径。")]
        public MovesetData moveset;

        [Header("Model Instantiation")]
        public WeaponModelType weaponModelType;

        [Header("Weapon Model")]
        public GameObject weaponModel;

        [Header("Weapon Model Attach Offset (ER c0000 挂点微调)")]
        [Tooltip("挂到 R_Weapon/L_Weapon/L_Shield 挂点骨后的本地偏移。\n" +
                 "默认全 0 = 直接对齐 ER 武器 dummy（多数情况无需改）。个别武器姿态不对时在此微调。")]
        public Vector3 weaponModelPositionOffset = Vector3.zero;
        public Vector3 weaponModelRotationOffset = Vector3.zero;
        public Vector3 weaponModelScale = Vector3.one;

        [Header("Weapon Class")]
        public WeaponClass weaponClass;

        [Header("Upgrade Level")]
        public UpgradeLevel upgradeLevel;

        [Header("ER 溯源")]
        [Tooltip("此武器在 EquipParamWeapon 里的行 ID。仅作数据溯源/查表用，与运行时 itemID 无关。0 表示非 ER 来源。")]
        public int erRowId;

        [Header("ER 挂载 (WepAbsorpPosParam → dummy refID)")]
        [Tooltip("此武器在 EquipParamWeapon 的 wepAbsorpPosId（偏移 0x170）。溯源用，可在 DSAS 装备窗口直接看到。-1=未设置。")]
        public int wepAbsorpPosId = -1;
        [Tooltip("拳套/徒手类：勾选后不用 dummy，按 ER 骨骼直接绑定（isSkeletonBind=1）。")]
        public bool isSkeletonBind = false;
        [Tooltip("单手右持挂点 dummy refID（WepAbsorpPosParam.right_0）。挂到骨架里 'c0000 Dummy<n> [refID]' 节点。\n" +
                 "-1 = 回退到 R_Weapon 挂点骨（旧行为）。")]
        public int dummyRightHand = -1;
        [Tooltip("单手左持挂点（left_0）。-1 = 回退 L_Weapon。")]
        public int dummyLeftHand = -1;
        [Tooltip("双手持握挂点（both_0）。-1 = 回退单手挂点。")]
        public int dummyBothHand = -1;
        [Tooltip("右手武器收纳/挂身挂点（rightHang_0）。-1 = 无收纳挂点。")]
        public int dummyRightHang = -1;
        [Tooltip("左手武器收纳/挂身挂点（leftHang_0）。-1 = 无收纳挂点。")]
        public int dummyLeftHang = -1;

        [Header("ER 剑鞘 (可选，部分武器才有；= 武器 model index 1)")]
        [Tooltip("鞘子网格在武器模型里的 renderer/节点名(FBX 节点名，如 'WP_A_0200_1')。\n" +
                 "留空 = 此武器无鞘。有值时：手持模型隐藏该 renderer，另在腰间显示一份鞘。\n" +
                 "对应 ER 武器的 model index 1(WepAbsorpPosParam.WepInvisibleTypes.Sheath)。")]
        public string sheathRendererName = "";
        [Tooltip("鞘的挂点骨 = 其 dummy 的 attach 骨。长剑 model1=dummy 2030→角色 dummy30，c0000.flver 实测 attach_bone='Pelvis'(世界 Z=-0.12 左侧)。\n" +
                 "剑鞘与剑刃走同一套挂载：剑刃挂 R_Weapon、鞘挂 Pelvis。")]
        public string sheathBoneName = "Pelvis";
        [Tooltip("鞘位置微调【角色空间】：X=左右(左为负) / Y=上下 / Z=前后。\n" +
                 "基准由 ER dummy(默认 30/31)+ 自标定基算出(左右已正确)，此处把它拉到髋部并前后微调。")]
        public Vector3 sheathPositionOffset = new Vector3(0f, -0.29f, 0f);
        [Tooltip("鞘朝向微调【角色空间】：fineRot=0 时鞘按 FBX 原生朝向对齐角色轴。\n" +
                 "直剑鞘应沿左大腿下垂、略朝后。先试 (0,90,0)/(0,0,90) 等 90°增量把它从'横伸'转成'下垂'，再细调。")]
        public Vector3 sheathRotationOffset = Vector3.zero;
        [Tooltip("鞘(model1)单手右持时的收纳 dummy refID(WepAbsorpPosParam.right_1，长剑=2030→%1000=30)。默认 30。")]
        public int erSheathDummyRightHand = 30;
        [Tooltip("鞘(model1)单手左持时的收纳 dummy refID(left_1，长剑=2031→%1000=31)。默认 31。")]
        public int erSheathDummyLeftHand = 31;

        [Header("Weapon Requirements")]
        public int strengthREQ = 0;
        public int dexREQ = 0;
        public int intREQ = 0;
        public int faithREQ = 0;

        [Header("Weapon Base Damage")]
        public int physicalDamage = 0;
        public int magicDamage = 0;
        public int fireDamage = 0;
        public int holyDamage = 0;
        public int lightningDamage = 0;
        
        // 武器防御吸收（阻挡能力）

        [Header("Weapon Poise")]
        public float poiseDamage = 10;
        // 攻击时的进攻姿态加成

        [Header("Attack Modifiers")]
        public float light_Attack_01_Modifier = 1.0f;
        public float light_Attack_02_Modifier = 1.2f;
        public float light_Jumping_Attack_01_Modifier = 1.0f;
        public float heavy_Attack_01_Modifier = 1.4f;
        public float heavy_Attack_02_Modifier = 1.6f;
        public float heavy_Jumping_Attack_01_Modifier = 1.8f;
        public float charge_Attack_01_Modifier = 2.0f;
        public float charge_Attack_02_Modifier = 2.2f;
        public float running_Attack_01_Modifier = 1.1f;
        public float rolling_Attack_01_Modifier = 1.1f;
        public float backstep_Attack_01_Modifier = 1.1f;
        public float dw_Attack_01_Modifier = 0.77f;
        public float dw_Attack_02_Modifier = 0.87f;
        public float dw_Jump_Attack_01_Modifier = 1.27f;
        public float dw_Run_Attack_01_Modifier = 0.75f;
        public float dw_Roll_Attack_01_Modifier = 0.72f;
        public float dw_Backstep_Attack_01_Modifier = 0.77f;

        [Header("Stamina Cost Modifiers")]
        public int baseStaminaCost = 20;
        public float lightAttackStaminaCostMultiplier = 1.0f;
        public float heavyAttackStaminaCostMultiplier = 1.3f;
        public float chargedAttackStaminaCostMultiplier = 1.5f;
        public float runningAttackStaminaCostMultiplier = 1.1f;
        public float rollingAttackStaminaCostMultiplier = 1.1f;
        public float backstepAttackStaminaCostMultiplier = 1.1f;

        [Header("Weapon Blocking Absorption")]
        public float physicalBaseDamageAbsorption = 50;
        public float magicBaseDamageAbsorption = 50;
        public float fireBaseDamageAbsorption = 50;
        public float holyBaseDamageAbsorption = 50;
        public float lightningBaseDamageAbsorption = 50;
        public float stability = 50;    // REDUCES STAMINA LOST FROM BLOCK

        [Header("Actions")]
        public WeaponItemAction oh_RB_Action;   // ONE HAND RIGHT BUMPER ACTION
        public WeaponItemAction oh_RT_Action;   // ONE HAND RIGHT TRIGGER ACTION
        public WeaponItemAction oh_LB_Action;   // ONE HAND LEFT BUMPER ACTION
        public AshOfWar ashOfWarAction;

        [Header("SFX")]
        public AudioClip[] whooshes;
        public AudioClip[] blocking;
    }
}