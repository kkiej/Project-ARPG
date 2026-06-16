# 模块化角色接入方案（ELDEN RING 资产 → Project-ARPG）

> 目标：把 ELDEN RING 解包得到的**通用骨骼 + 动画 + 部件模型**接入工程，替换现有美术资源；并把当前"所有部件塞进一个 prefab + SetActive"的换装方案，改造成**共享骨骼 + 动态蒙皮重绑定 + 按需加载**的可扩展方案。
>
> 关联文档：[`Architecture.md`](./Architecture.md) §4 角色系统 / §8 装备系统、[`EldenRing-Character-Animation-Reference.md`](./EldenRing-Character-Animation-Reference.md)（命名编号）、[`EldenRing-Unpacking-Guide.md`](./EldenRing-Unpacking-Guide.md)（解包流程）。
>
> ⚠️ ELDEN RING 资产仅供学习研究，禁止商用/分发；正式版需替换为自有美术。本方案的**技术架构**是自有资产，可直接沿用。

---

## 1. 背景与结论

### 1.1 现状（当前换装方案）

- `PlayerBodyManager.cs`：把 `maleHead / maleBody[] / maleArms[] / maleLegs[] / hair / femaleXXX` 等**所有身体部件**作为子物体硬引用在玩家 prefab 上。
- `EquipmentModel.cs`（SO）：`LoadModel()` 内按 `equipmentModelType` 在 `playerEquipmentManager.maleHeadFullHelmets / hats / hoods / maleBodies / ...` 这些**预摆放数组**里用**名字匹配** + `SetActive(true)` 开关显隐。

即：**衣柜是手工预置、全量常驻、靠显隐切换**。

### 1.2 为什么撑不住

ELDEN RING 本机解包：头 442 / 身 670 / 腿 314 / 臂 262 / 武器 1004 件。把它们全做成子物体塞进一个 prefab：

- prefab 体积爆炸，编辑器几乎打不开；
- 上千个 `SkinnedMeshRenderer` 即使 `SetActive(false)` 也占内存/序列化开销；
- 无法数据驱动，新增一件就要手摆。

### 1.3 结论

采用 **ELDEN RING 的模块化共享骨骼方案**，但**上层只编目你真正要用的套装**（数据驱动的精选目录），不把全量资产塞进工程。

> 你的 GDD 主角是固定浪人，不是 ER 的自由换装娃娃。所以：**用 ER 的底层技术（共享骨骼+动态重绑定），不用 ER 的全量衣柜规模。**

---

## 2. 两套设计对比

| 维度 | 现状：单 prefab + SetActive | 目标：模块化共享骨骼 |
|------|----------------------------|----------------------|
| 骨骼 | 每个 prefab 自带 | **一具通用骨骼**，所有部件共享 |
| 部件存放 | prefab 子物体（全量常驻） | 独立 prefab/Addressable，按需加载 |
| 换装实现 | 名字匹配 + `SetActive` | 加载 SMR → **bones 重绑定到主骨骼** |
| 内存 | 全量驻留 | 仅当前穿着 |
| 扩展性 | 手摆，几十件上限 | 数据驱动，理论无上限 |
| 与 ER 资产契合 | ❌ | ✅ 原生 |
| 实现复杂度 | 低 | 中（重绑定 + 异步加载） |
| 网络/存档 | 传 itemID | 传 itemID（**不变**） |

---

## 3. 目标架构

### 3.1 核心原理：共享骨骼 + SkinnedMeshRenderer 重绑定

ELDEN RING 里 `c0000` 只有一具骨骼，所有 `bd/hd/am/lg` 网格都蒙皮到它。Unity 里复刻：

```
玩家 Prefab (FBX RootNode)
└── Master (ER 根骨 / 根运动骨，来自 c0000 skeleton)
     ├── Pelvis
     │    ├── Spine → Spine1 → Spine2 → Neck → Head
     │    ├── L_Clavicle → L_Shoulder → L_UpperArm → L_Elbow → L_Forearm → L_Hand → L_Finger*
     │    ├── R_Clavicle → ...
     │    ├── L_Thigh → L_Knee → L_Calf → L_Foot → L_Toe0
     │    └── R_Thigh → ...
     ├── L_Weapon / R_Weapon (武器挂点)
     └── Ctrl* / AxillaRig* / *Nub (控制/辅助骨，蒙皮一般不引用)

部件 Prefab（如 bd_m_xxxx）
└── SkinnedMeshRenderer (bones 引用的是部件自带的同名骨骼)   ← 加载后需要"重绑定"
```

> ⚠️ **ER 骨骼命名实测**（来自 `bone_fix_log` + `c0000.fbx` + Unity Hierarchy）：
> - `c0000 Armature` 是**骨架容器对象**（所有骨骼的父级 Transform），**不是骨骼**。`SkeletonBoneMap` 就挂在它上面。
> - ER 设计上的根骨是 **`Master`**（根运动骨），骨盆是 **`Pelvis`**，**没有 `Hips`**。脊柱 `Spine/Spine1/Spine2`，锁骨 `L_Clavicle`，大腿 `L_Thigh`。
> - **本 FBX 导入后骨架被部分"压平"**：`Master`/`AxillaRig*`/`Ctrl*`/`LinkNode*`/`MasterRig*`/`Model_Dmy*`/`Pelvis` 等都成了 `c0000 Armature` 的**直接子级（叶子）**，`Master` 因此没有子节点；真正的形变骨链仍挂在 `Pelvis` 下（`Pelvis → L_Thigh/R_Thigh/L_Hip/R_Hip/Pelvis_Mantle → ...`）。
> - 整套约 **488 根**，含大量控制骨/`*Nub`/`[cloth]*` 布料骨——蒙皮通常不引用，重绑定匹配不到会被跳过（正常）。
> - **压平不影响换装**：重绑定按骨骼名匹配，与层级是否嵌套无关；部件蒙皮到与动画同一批骨骼，形变天然一致。压平只影响根运动（Master 不带动 Pelvis），属 KCC root motion 范畴，与本系统无关。

**重绑定**：部件 FBX 自带一套同名骨骼；加载后把它的 `SkinnedMeshRenderer.bones` 按**骨骼名**逐个替换为主骨骼对应的 Transform，`rootBone` 同理。完成后部件就跟着主骨骼动了，自带骨骼丢弃。

> 前提：所有部件与角色 FBX **共用同一套骨骼命名**。ER 资产天然满足（都基于 c0000 skeleton）。用 Soulstruct/Blender 导出 FBX 时务必保持骨骼名一致、不要重命名。`SkeletonBoneMap` 会自动把根骨识别为 `Master`（找不到则退而求其次 `Pelvis`/`Root`）。

### 3.2 模块层级

```
┌───────────────────────────────────────────────┐
│ 数据层                                          │
│   ArmorItem.modularPartCode  : 权威数据(SO 上)   │
│       例 "1350"                                 │
│   EquipmentPartCatalog : code → prefab 资源索引  │
│       例 "BD_M_1350" → prefab（可自动扫描填充）   │
└───────────────────────────────────────────────┘
                  │ 槽位前缀+性别+编号 → code
                  ▼
┌───────────────────────────────────────────────┐
│ 装配层 (运行时)                                  │
│   ModularCharacterAssembler                     │
│     • EquipPart(slot, prefab) 重绑定+提取 SMR    │
│     • RebindToSkeleton(smr)   骨骼重绑定          │
│     • UnequipPart(slot)       卸下并清理          │
│   SkeletonBoneMap  (主骨骼 name→Transform 缓存)  │
└───────────────────────────────────────────────┘
                  │
                  ▼
┌───────────────────────────────────────────────┐
│ 接入层 (改造现有类)                              │
│   PlayerEquipmentManager  : Load*Equipment →     │
│       ApplyModularPart(slot, equipment)         │
│   PlayerBodyManager       : 默认裸模(空安全)      │
│   WeaponModelInstantiationSlot : 武器(刚性挂点)不变│
└───────────────────────────────────────────────┘
```

### 3.3 与现有系统的关系（最小侵入）

- **网络/存档不变**：仍然只同步/存 `itemID`（`SerializableWeapon`、`currentXXXEquipmentID` 等照旧）。装配器只是把"拿到 ID 之后实例化模型"这一步换实现。
- **动画不变**：ER 动画 FBX 共享同一骨骼，导入后照常注册进 `AnimationClipRegistry`，Animancer 多层照常播放。
- **武器不变**：武器是**刚性挂点**（挂到手骨），继续用 `WeaponModelInstantiationSlot`，不需要重绑定。

---

## 4. 资产管线

### 4.1 FBX 导出与组织

| 资产 | 来源 | 处理 | 落地目录 |
|------|------|------|----------|
| 通用骨骼 | `c0000.chrbnd` 的 skeleton | 导成 FBX（仅骨架，可含默认裸模） | `Assets/_ELDENRING_REF/Skeleton/` |
| 身体默认裸模 | `c0000` 默认 bd/hd/am/lg | 蒙皮到通用骨骼 | `.../Parts/Body/` |
| 装备部件 | `parts/{hd,bd,am,lg}_*` | 每件一个 SMR，蒙皮到同骨骼 | `.../Parts/{Head,Body,Arm,Leg}/` |
| 武器 | `parts/wp_*` | 静态网格 + 挂点 | `.../Weapons/` |
| 动画 | `chr/c0000*.anibnd` 的 HKX | HKX→FBX，保留动画 ID 命名 | `.../Animations/c0000/` |
| 贴图 | `texbnd/partsbnd` 的 TPF | DDS→PNG | `.../Textures/` |

> 命名保留 ER 原始编号（如 `bd_m_1101`、`a000_003000`），方便回查 §参考文档。

### 4.2 Addressables 按需加载

- 每件部件标记为 Addressable，`address = itemID` 或 `"part_bd_1101"`。
- 装配器用 `Addressables.LoadAssetAsync<GameObject>(address)` 异步加载，`Release` 释放。
- 好处：编辑器/包体不强引用全量资产，内存只留穿着的几件。

### 4.3 骨骼一致性校验（关键）

写一个编辑器工具，校验每件部件 FBX 的骨骼名集合 ⊆ 主骨骼骨骼名集合；不一致的标红。否则重绑定时会丢骨头导致穿模/拉飞。

---

## 5. 核心代码改造

> 以下为**新增/改造草案**，命名空间 `LZ`，与现有风格一致。先小范围跑通再铺开。

### 5.1 新增：主骨骼映射缓存

```csharp
namespace LZ
{
    // 挂在玩家 prefab 的 Armature 根上，Awake 时缓存全部骨骼 name→Transform
    public class SkeletonBoneMap : MonoBehaviour
    {
        public Transform rootBone;                 // 通常是 Hips
        readonly Dictionary<string, Transform> _map = new();

        void Awake() => Build();

        public void Build()
        {
            _map.Clear();
            foreach (var t in GetComponentsInChildren<Transform>(true))
                _map[t.name] = t;
        }

        public Transform Get(string boneName) =>
            _map.TryGetValue(boneName, out var t) ? t : null;
    }
}
```

### 5.2 新增：模块化装配器（重绑定核心）

```csharp
namespace LZ
{
    public enum BodySlot { Head, Torso, Arms, Legs, Hair }

    public class ModularCharacterAssembler : MonoBehaviour
    {
        [SerializeField] SkeletonBoneMap skeleton;
        readonly Dictionary<BodySlot, GameObject> _equipped = new();
        readonly Dictionary<BodySlot, AsyncOperationHandle<GameObject>> _handles = new();

        // 加载并穿上某 slot 的部件（address 由 itemID 解析）
        public async void EquipPart(BodySlot slot, string address)
        {
            UnequipPart(slot);
            if (string.IsNullOrEmpty(address)) return;   // 该槽为空 → 显示默认裸模(见 5.4)

            var handle = Addressables.InstantiateAsync(address, transform);
            _handles[slot] = handle;
            var go = await handle.Task;

            foreach (var smr in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                RebindToSkeleton(smr);

            _equipped[slot] = go;
        }

        public void UnequipPart(BodySlot slot)
        {
            if (_handles.TryGetValue(slot, out var h))
            {
                Addressables.ReleaseInstance(h);   // 释放实例 + 资产引用
                _handles.Remove(slot);
            }
            _equipped.Remove(slot);
        }

        // 把部件 SMR 的骨骼重映射到主骨骼
        void RebindToSkeleton(SkinnedMeshRenderer smr)
        {
            var src = smr.bones;
            var dst = new Transform[src.Length];
            for (int i = 0; i < src.Length; i++)
                dst[i] = src[i] ? (skeleton.Get(src[i].name) ?? src[i]) : null;

            smr.bones = dst;
            if (smr.rootBone) smr.rootBone = skeleton.Get(smr.rootBone.name) ?? smr.rootBone;
            // 可选：把 smr.transform 提到角色根下，丢弃部件自带骨架子树
        }
    }
}
```

> 关键点：`smr.bones` 数组的**顺序**必须和 `sharedMesh.bindposes` 对应，所以是**按索引替换 Transform**、保持长度不变，只换引用——不要重排。

### 5.3 数据来源：方案 B（数据在 SO + 命名约定索引）—— 已落地

> ⚠️ **不要用 itemID 做配置主键**。`WorldItemDatabase.Awake()` 里 `items[i].itemID = i` 是**按列表顺序运行时分配**的，增删/重排物品会让所有 ID 漂移。因此放弃"itemID → 部件"的目录，改为：

**(1) 权威数据写在装备 SO 上**（`ArmorItem.modularPartCode`）：

```csharp
public class ArmorItem : EquipmentItem
{
    // ...原有字段...
    [Header("Modular Part (ER 共享骨架换装)")]
    public string modularPartCode; // 仅填基础编号，如 "1350"；留空=不走模块化
}
```

**(2) 运行时按约定拼出部件名**：`{槽位前缀}_{性别}_{编号}`，槽位前缀 = `HD/BD/AM/LG`，性别 = `M/F`。
例：身体甲 `modularPartCode=1350`、男性 → `BD_M_1350`（正好等于 ER 部件 prefab 文件名）。

**(3) `EquipmentPartCatalog` 退化为"code → prefab"的资源索引**，且可一键自动填充：

```csharp
[CreateAssetMenu(menuName = "ARPG/Equipment Part Catalog")]
public class EquipmentPartCatalog : ScriptableObject
{
    [System.Serializable]
    public struct Entry { public string code; public GameObject prefab; } // code = prefab 文件名
    // GetPrefabByCode("BD_M_1350")，大小写不敏感
    // 编辑器右键菜单 "Auto Populate From Folder"：扫描 sourceFolder 下所有 prefab，按文件名自动填充
}
```

优点：**无 itemID 耦合**（数据跟 SO 走）、**零手动拖拽**（扫描文件夹自动建索引）、**直接引用 prefab**（打包安全，无需 Addressables/Resources）。
> 备注：本工程 **未安装** `com.unity.addressables`（`Bard.AddressablesManager` 是 DLL 自定义封装），故不走 Addressables 异步路径；如将来要按需加载，仅需把 `GetPrefabByCode` 内部换成异步资源加载即可，上层接口不变。

### 5.4 改造：`PlayerEquipmentManager` —— 已落地（可选开关）

不改动网络/存档：owner 与远端都经过 `LoadHead/Body/Leg/HandEquipment`，在其中调 `ApplyModularPart(slot, equipment)` 即可两端通吃。

```csharp
public bool useModularEquipment;           // 开关，默认关 = 旧行为不变
public ModularCharacterAssembler modularAssembler;
public EquipmentPartCatalog partCatalog;

private bool ApplyModularPart(BodySlot slot, EquipmentItem equipment)
{
    if (!useModularEquipment) return false;                 // 关 → 走旧 SetActive
    string baseCode = (equipment as ArmorItem)?.modularPartCode;
    if (equipment == null || string.IsNullOrEmpty(baseCode))
    { modularAssembler.UnequipPart(slot); return true; }
    string code = $"{SlotPrefix(slot)}_{(isMale?"M":"F")}_{baseCode}"; // BD_M_1350
    var prefab = partCatalog.GetPrefabByCode(code);
    if (prefab) modularAssembler.EquipPart(slot, prefab);
    else        modularAssembler.UnequipPart(slot);
    return true;
}
```

槽位映射：Head→`HD`、Torso→`BD`、Arms→`AM`、Legs→`LG`。`Awake` 中开启模块化时跳过旧的 `InitializeArmorModels()`。

### 5.5 改造：`PlayerBodyManager` —— 已做空安全

- 所有 `SetActive`/`foreach` 已加判空，移除 Synty 身体后不再空引用崩溃。
- "卸甲默认裸模"两种选择：①引用留空（部件含裸身，完全接管）；②把 c0000 自带裸身/头/发 SMR 填进对应字段（穿身体甲时自动隐藏裸身躯干，防穿模，推荐）。
- 保留头发/捏脸逻辑（`SetHairColor` 等已判空）。

### 5.6 不动：`WeaponModelInstantiationSlot`

武器是刚性挂点（手骨/背后），现有实例化逻辑正确，**保持不变**。仅把武器 FBX 换成 ER 的 `wp_*`。

---

## 6. 分阶段实施步骤

> 每阶段都能独立验证，避免一次性大改翻车。

### 阶段 0：单件验证（0.5 天）

1. 导一具通用骨骼 FBX + 1 件躯干部件 FBX（同骨骼名）进工程。
2. 写最小测试场景：手动 `RebindToSkeleton` 把躯干绑到骨骼，播一个待机动画，确认**不穿模、不拉飞、跟着动**。
3. ✅ 验证"重绑定"技术成立，再继续。

### 阶段 1：装配器 + 目录（1-2 天）

1. 实现 `SkeletonBoneMap` / `ModularCharacterAssembler` / `EquipmentPartCatalog`。
2. 接入 Addressables，编目 3-4 件部件（头/身/臂/腿各一）。
3. 运行时调 `EquipPart` 能换上/卸下。

### 阶段 2：对接现有装备流程（1-2 天）

1. 改造 `PlayerEquipmentManager` 的 ID 变化回调 → 调装配器。
2. 改造 `PlayerBodyManager` 默认裸模逻辑。
3. 确认**存档读档 / 网络同步**仍正常（只传 itemID）。

### 阶段 3：替换美术 + 骨骼校验工具（1-2 天）

1. 写编辑器骨骼一致性校验工具（§4.3）。
2. 批量导入要用的 ER 部件，编目。
3. 武器换成 `wp_*`，走原 `WeaponModelInstantiationSlot`。

### 阶段 4：动画接入（独立线，可并行）

1. HKX→FBX（保留动画 ID 命名）→ 注册进 `AnimationClipRegistry`。
2. 用 §参考文档的 DSAnimStudio/TAE 流程区分动画用途、回填动画事件。

---

## 7. 风险与取舍

| 风险 | 说明 | 对策 |
|------|------|------|
| 骨骼名不一致 | 重绑定丢骨 → 穿模/拉飞 | §4.3 校验工具，导出时锁定骨骼名 |
| bindposes 不匹配 | 部件与骨骼绑定姿势不同 | 部件必须以同一骨骼/绑定姿势导出 |
| 异步加载时序 | 切场景/快速换装竞态 | 装配器内做 handle 取消/串行队列 |
| 内存泄漏 | Addressable 未 Release | `UnequipPart` 必须配对释放 |
| 法线/描边 | 卡通描边依赖平均法线顶点色 | 部件烘焙时跑现有 `AverageNormal` 工具 |
| 过度工程 | 主角固定浪人，未必需全模块化 | 上层只编目精选套装，不铺全量 |

### 取舍建议

- 若主角**外观基本固定**（只换武器+少量套装）：模块化只做"躯干/头"两槽即可，其余固定。
- 若确实要做**自由换装**：再把臂/腿/头发全槽接入。
- 无论哪种，**底层共享骨骼 + 动画复用**都先搭好，因为这是接入 ER 动画的前提。

---

## 8. 验收标准

- [ ] 单件部件能重绑定到通用骨骼并随动画正确运动（阶段 0）
- [ ] 通过 itemID 能异步换上/卸下部件，内存随之增减（阶段 1-2）
- [ ] 读档/联机后外观与 itemID 一致（阶段 2）
- [ ] 骨骼一致性校验工具可批量检查（阶段 3）
- [ ] 武器走原挂点逻辑正常（阶段 3）
- [ ] ER 动画注册进 Registry 并可播放（阶段 4）

---

## 9. 驱动附加骨与布料（披风/裙甲）—— 方案记录（待实施）

> 来自实测：ER 护甲部件除基础骨外，自带大量**附加骨**——分两类，驱动方式不同。当前 `ModularCharacterAssembler.GraftExtraBones` 已把它们"嫁接"到 c0000 骨架下，但**只解决了"刚性跟随"，没有解决"次级运动/布料摆动"**。本节记录后续怎么真正驱动它们。

### 9.1 附加骨分类与目标

| 类别 | 命名特征 | 当前状态 | 目标驱动方式 |
|------|----------|----------|--------------|
| **刚性护甲骨** | `_Pauldron` `_BreastPlate` `_ArmArmor` `_Belt` `_Collar` `_Gauntlet` | ✅ 已嫁接到基础骨下，**刚性跟随**（够用） | 维持刚性跟随即可；如需轻微晃动再加弹簧骨 |
| **布料骨链** | `_Mantle`(披风) `_Skirt`(裙甲) `_String`(绳带) `_ScaleMail`(鳞甲) | ⚠️ 已嫁接，但**僵直不摆动** | **弹簧骨/布料模拟**（本节重点） |
| **布料模拟目标** | `*_sim` | 嫁接为静态 | 作为弹簧链的受驱节点 |
| **碰撞代理骨** | `*_Collidable_*`（挂在 `UpArmTwist`/`ThighTwist`/`Pelvis`/`Calf`） | 嫁接为静态 | **作为碰撞体放置锚点**（见 9.3） |
| **挂点 Dummy** | `*_Dmy_*` `格納用` | 嫁接/兜底 | 用作武器/道具挂点，不参与形变 |

> 关键认知：ER 原本用 **Havok Cloth** 驱动布料，解包**拿不到** Havok 数据。所以 Unity 侧必须**用替代方案重新驱动布料骨链**。好处是 ER 已经把布料骨链（`Mantle/Skirt/String`）和碰撞锚点（`*_Collidable_*`）都建好了，我们直接复用这套骨骼拓扑即可，不用自己拉骨。

### 9.2 布料方案选型（三选一）

| 方案 | 原理 | 优点 | 缺点 | 推荐度 |
|------|------|------|------|--------|
| **A. 弹簧骨链**（开源 SpringBone / Dynamic Bone / VRM SpringBone） | 对 `Mantle/Skirt/String` 骨链做程序化弹簧+阻尼+碰撞 | 直接复用 ER 骨链；轻量；可联机用确定性参数 | 无真正布料自碰撞 | ⭐⭐⭐ 首选（披风/裙甲/绳带足够） |
| **B. Magica Cloth 2**（付费资产，BoneCloth 模式） | 同样基于骨链，但带更好的碰撞/约束/性能 | 效果好、性能优、支持骨链与网格两种 | 付费、引入新依赖 | ⭐⭐ 追求质量时上 |
| **C. Unity 内置 Cloth 组件** | 直接对披风**网格**做布料 | 引擎自带 | 需独立网格、与 SMR 蒙皮配合差、性能一般、不好联机 | ⭐ 不推荐 |

**结论**：优先 **方案 A（弹簧骨链）**——把嫁接后的 `Mantle/Skirt/String/ScaleMail` 骨链交给弹簧骨组件，复用 ER 现成拓扑，最省事且适配联机。质量不够再升级 Magica Cloth 2（同样是骨链驱动，迁移成本低）。

### 9.3 落地步骤（待实施）

1. **装配后识别布料骨链**：`ModularCharacterAssembler` 装配完成后，扫描嫁接出的附加骨，按命名（`_Mantle/_Skirt/_String/_ScaleMail/_sim`）收集成若干"骨链根"。
2. **自动挂弹簧骨组件**：对每条骨链根挂上弹簧骨组件，设默认刚度/阻尼/重力参数（披风软、裙甲硬、绳带最软）。
3. **自动生成碰撞体**：读取 `*_Collidable_*` 锚点骨（ER 已标好位置），在对应基础骨（`UpArmTwist/ThighTwist/Pelvis/Calf`）上自动生成胶囊碰撞体，喂给弹簧骨做碰撞，避免披风穿腿/穿身。
4. **参数目录化**：在 `EquipmentPartCatalog.Entry` 增加"布料预设"字段（软/中/硬），不同部件用不同手感。
5. **联机一致性**：弹簧骨用**固定步长 + 确定性参数**，或仅在本地表现层跑（布料不影响判定，可不同步，各端本地模拟即可）。
6. **性能**：远处/被裁剪的角色关闭弹簧骨更新（接入现有 `AIActivationBeacon`/距离裁剪）。

### 9.4 刚性附加骨（无需布料）

`Pauldron/BreastPlate/ArmArmor` 等已通过嫁接做到刚性跟随，**默认无需额外驱动**。仅当美术希望"行走时肩甲轻晃"才对这些也挂低强度弹簧骨。

### 9.5 待办清单

- [ ] 装配器增加"布料骨链识别"（按命名收集）
- [ ] 选定弹簧骨方案（开源 SpringBone vs Magica Cloth 2）并接入
- [ ] `*_Collidable_*` → 自动生成胶囊碰撞体
- [ ] `EquipmentPartCatalog` 增加布料预设字段
- [ ] 联机/性能策略（本地模拟 + 距离裁剪）

---

**文档版本**：v1.1 ｜ **创建**：2026-06-12 ｜ **更新**：2026-06-15（新增 §9 附加骨/布料驱动方案）｜ 维护：架构层改动后同步 `Architecture.md` §4/§8
