# Elden Ring 角色 / 动画 / 贴图 命名编号对照 & 自动化工作流

> 配套文档：解包总流程见 [`EldenRing-Unpacking-Guide.md`](./EldenRing-Unpacking-Guide.md)。
> 本文专注于：**命名编号规则速查**、**用 Soulstruct 在 Blender 中自动贴图**、**HKX 动画 → FBX → Unity 的批量化与区分**、**TAE 动画事件（攻击判定/无敌帧/特效/音效）如何复用 ER 原始配置自动化到 Unity**、以及**怪物 / Boss 工作流**。
>
> **版权声明**：所有解包内容仅供个人学习研究，严禁商用 / 分发。

---

## 目录

1. [文件类型与命名总规则](#1-文件类型与命名总规则)
2. [角色编号体系（cXXXX）](#2-角色编号体系cxxxx)
3. [玩家装备 / 模型 / 贴图（parts）](#3-玩家装备--模型--贴图parts)
4. [动画命名体系（aXXX_YYYYYY）](#4-动画命名体系axxx_yyyyyy)
5. [如何区分"这条动画是哪个、干什么用"](#5-如何区分这条动画是哪个干什么用)
6. [TAE 动画事件 = ER 的"动画事件系统"](#6-tae-动画事件--er-的动画事件系统)
7. [Soulstruct + Blender 自动贴图](#7-soulstruct--blender-自动贴图)
8. [HKX 动画 → FBX → Unity 批量流程](#8-hkx-动画--fbx--unity-批量流程)
9. [怪物 / Boss 工作流](#9-怪物--boss-工作流)
10. [工具清单与参考](#10-工具清单与参考)

---

## 1. 文件类型与命名总规则

所有角色资源都在 `Game/chr/`，以角色 ID `cXXXX` 为前缀。每个角色对应一组 BND 容器：

| 文件后缀 | 内容 | 工具 | Unity 是否需要 |
|----------|------|------|----------------|
| `cXXXX.chrbnd.dcx` | **模型(FLVER) + 骨架(HKX skeleton) + 部分贴图**（注：c0000 的 FLVER 仅含骨架，无 mesh/贴图，实际模型来自 parts） | Soulstruct / WitchyBND | ✅ 模型+骨架 |
| `cXXXX.anibnd.dcx` | **所有动画(HKX) + 骨架 + TAE(动画事件)** | Soulstruct / DSAnimStudio | ✅ 动画+事件 |
| `cXXXX.behbnd.dcx` | **行为图(Havok Behavior) + HKS 脚本** = 状态机：把"状态"映射到"动画ID" | DSAnimStudio | 参考用（理解动画用途） |
| `cXXXX_h.texbnd.dcx` | **高分辨率贴图**（_h = high） | Soulstruct / WitchyBND | ✅ 怪物贴图 |
| `cXXXX_l.texbnd.dcx` | 低分辨率贴图（_l = low，LOD/远景用） | 同上 | 一般跳过 |
| `cXXXX_aNx.anibnd.dcx` | 动画**分包**（按类别拆分，见 §4.3） | 同 anibnd | 玩家专用 |

> 💡 **关键**：`anibnd` 里同时装着 **动画 (.hkx)** 和 **动画事件 (.tae)**，二者通过同一个动画 ID 关联。这就是为什么"动画"和"事件"能自动对上 —— 它们本就是一套数据。

命名小结：
- `c` + 4 位数字 = 角色 ID
- `_h` / `_l` = 高 / 低模贴图
- `_div00` / `_div01` = 大体型 Boss 的分体（模型被拆成多个部件，如龙）
- `_dlc01` / `_dlc02` = 黄金树幽影 DLC 追加内容

---

## 2. 角色编号体系（cXXXX）

本机解包出 **约 280 个角色 ID**（`c0000` ~ `c9001`）。编号按"类型分段"，规律如下（基于本目录实测 + FromSoft 通用约定）：

| ID 区间 | 类别 | 说明 / 代表 |
|---------|------|-------------|
| `c0000` | **玩家** | 唯一可换装的模块化角色，无自带贴图（贴图来自 parts 装备 + facegen 捏脸） |
| `c0100`–`c0130` | 玩家相关变体 | 教程/特殊状态用的玩家骨架变体 |
| `c1000` | 通用 / 系统 | 占位或通用对象 |
| `c2xxx` | **人型杂兵 / NPC** | 亚人、士兵、盗贼、商人等（c2010 亚人、c2270 系列等） |
| `c3xxx` | **野兽 / 动物 / 中型敌人** | 狼、犬、熊、飞龙幼体等 |
| `c4xxx` | **主力敌人 + 大量 Boss** | 骑士、战士、大型怪、众多 Boss 集中在此段 |
| `c5xxx` | 特殊 / 大型怪 | 巨人、特殊机制敌人 |
| `c6xxx` | 特殊 / 召唤物 / 法术实体 | 灵体、召唤、机关 |
| `c7000` / `c7100` | 特殊系统对象 | |
| `c8xxx` | **超大型 Boss / 特殊** | 巨龙、天空类、剧情 Boss |
| `c9001` | 系统 / 测试 | |

> ⚠️ 上表是"分段规律"，**不是逐一确认的对照**。FromSoft 不在文件名里写敌人名字，要拿到"c4xxx = 某某 Boss"的精确对照，必须交叉查下面的数据源。

### 2.1 如何把 cXXXX 精确对应到"某个 Boss / 怪物的名字"

文件名不含名字，权威对照有三条路：

1. **NpcParam + NpcName 文本（最权威）**
   - `regulation.bin` 里的 `NpcParam` 行 ID（如 `40000000`）的高位编码了 chr ID（`4000` → `c4000`）。
   - `NpcParam.nameId` → 指向 `msg` 里的 `NpcName` 文本（`Game/msg/zhocn/` 中文 / `engus` 英文）。
   - 用 **Smithbox** 打开 `regulation.bin`，在 NpcParam 里搜 chr 编号即可看到名字。

2. **社区现成对照表（最省事）**
   - 搜 "Elden Ring chr id list" / "Elden Ring enemy model id spreadsheet"，社区有完整的 `cXXXX → 敌人名` 表（含 DLC）。直接查表最快。

3. **DSAnimStudio 直接加载预览**
   - 用 DSAnimStudio 打开 `cXXXX.anibnd.dcx`，能直接看到模型长什么样，肉眼即可确认是谁。

> 建议：做文档时以"社区对照表"为主，关键 Boss 再用 Smithbox 的 NpcParam 二次核对。

---

## 3. 玩家装备 / 模型 / 贴图（parts）

玩家 `c0000` 是**模块化**的：身体本体 + 一堆可替换装备零件，零件都在 `Game/parts/`（本机约 2993 个文件，其中非 LOD 约 1498 个）。命名 `前缀_性别_编号.partsbnd.dcx`：

| 前缀 | 部位 | 数量(本机) | 说明 |
|------|------|-----------|------|
| `wp` | **武器 Weapon** | 1004 | `wp_a_XXXX`（a=通用），武器/盾/法器 |
| `bd` | **躯干 Body** | 670 | 胸甲 / 衣服 |
| `hd` | **头 Head** | 442 | 头盔 / 头部 |
| `lg` | **腿 Legs** | 314 | 护腿 / 裤子 |
| `am` | **手臂 Arms** | 262 | 护手 / 袖 |
| `fg` | **脸部装备 FaceGear** | 195 | 面具等 |
| `hr` | **头发 Hair** | 74 | 发型 |
| `fc` | **脸 Face** | 31 | 捏脸基础脸模 |

命名细节：
- 性别码：`_m_` 男 / `_f_` 女 / `_a_`（武器等）通用。
- `_l` 后缀 = **低模 LOD**（如 `am_m_0000_l`），近景用无 `_l` 的高模即可，`_l` 可整体跳过。
- 编号 `0000` 通常是裸体 / 默认件。
- 装备的"游戏内名字"同样查 **`EquipParamProtector`（防具）/ `EquipParamWeapon`（武器）→ nameId → msg**（用 Smithbox）。

> 玩家贴图：`c0000.chrbnd` 本身几乎没有贴图，**玩家的贴图分散在各 parts 零件的 partsbnd 内**（每件装备自带 tpf），捏脸贴图在 `Game/facegen/`。

---

## 4. 动画命名体系（aXXX_YYYYYY）

动画文件命名 `aXXX_YYYYYY.hkx`，例：`a000_003000.hkx`。

```
a  000   _   003000
│   │         │
│   │         └── 动画编号 YYYYYY（6位）= 这一类里的具体动作
│   └──────────── 类别 XXX（3位）= "动画分组 / Moveset 大类"
└──────────────── 固定前缀 a (animation)
```

- **XXX（类别）**：把动画按"用途/武器类型"分组。`a000` 是**通用基础组**（数量最大，本机 c0000 的 a000 有 1455 条）——走跑、翻滚、跳跃、受击、死亡、姿态、通用交互都在这里。其余 `a002 / a010 / a020 / ...` 多为**按武器类型/特殊行为划分的招式组**。
- **YYYYYY（编号）**：同一类别内区分具体动作（待机 / 走 / 跑 / R1 / R2 / 翻滚攻击 / 跳劈 ...）。

> ⚠️ 6 位编号里"哪段=哪个动作"在不同类别下不完全一致，**不要硬背**。判定"具体是什么动作"请走 §5 的方法（DSAnimStudio + TAE + 行为图），那才是权威来源。

### 4.1 anibnd 分包（玩家专用）

玩家动画太多，被拆成多个 `anibnd`（解包时都要，合起来才完整）：

| 文件 | 内容 |
|------|------|
| `c0000.anibnd.dcx` | 主包（仅骨架 skeleton.hkx + compendium，**不含动画**） |
| `c0000_a0x` ~ `c0000_a9x` | 按类别段拆分的动画包（a0x = a00~a09 类，依此类推） |
| `c0000_a00_hi / _md / _lo` | a00 类按**优先级**拆分（hi=高优先/近景核心动作 / md=中优先 / lo=低优先/远景简化） |
| `c0000_dlc01 / _dlc02` | DLC 追加动画 |

> 用 Soulstruct / WitchyBND 解包时，把 `c0000*.anibnd.dcx` **全选一起解**，否则动画不全。普通怪物通常只有一个 `cXXXX.anibnd.dcx`，简单很多。

---

## 5. 如何区分"这条动画是哪个、干什么用"

这是你最关心的问题。光看 `a000_003000.hkx` 文件名是看不出"这是跳劈还是翻滚"的。可靠办法按推荐度排序：

### 方法 A：DSAnimStudio 实时预览（最直观，强烈推荐）⭐

DSAnimStudio（Meowmaritus 出品）专为 FromSoft 动画设计：

1. 打开 `cXXXX.anibnd.dcx`（玩家则需配合 chrbnd + behbnd）。
2. 左侧动画列表逐条点 → 右侧**实时播放骨骼动作**，肉眼立刻知道是什么。
3. 它会**同时显示该动画的 TAE 事件**（攻击判定盒、无敌帧、特效、音效、可取消窗口），等于把"动画 + 事件"一起看。
4. 边看边在你的对照表里记 `动画ID → 含义`。

> 这是把"几千条动画"语义化最快的路径。先用它给关键动画（待机/走/跑/翻滚/各 R1R2/受击/死亡/处决）建一份小抄，再批量导出。

### 方法 B：用行为图 / 攻击参数反查（权威映射）

ER 里"角色状态 → 播哪条动画"由 **behbnd（Havok Behavior + HKS）** 决定；"动画造成多少伤害 / 什么属性"由 `regulation.bin` 的 **`BehaviorParam_PC` → `AtkParam_PC`** 决定，它们都引用**动画 ID**。

- 用 **Smithbox** 打开 regulation.bin，看 `Behavior` / `AttackParam`，能把"攻击编号 → 动画 ID → 伤害/判定"串起来。
- 适合做"招式表/战斗数值"对照，而不只是"动作名字"。

### 方法 C：命名 + 数量规律辅助判断

- `a000` 类里**编号最小的一批**几乎都是 locomotion（待机/转身/走/跑）。
- 同一动作常有"开始 / 循环 / 结束"三连号（如 `..._000` / `_001` / `_002`）。
- 配合方法 A 抽查几条就能摸清该角色的编号习惯。

> 结论：**DSAnimStudio 看动作 + TAE 看事件 + Smithbox 看参数**，三者就能完整回答"这条动画是谁、干嘛的、打多少伤害、什么时候能取消"。

---

## 6. TAE 动画事件 = ER 的"动画事件系统"

### 6.1 TAE 是什么

**TAE（TimeAct）** 是 FromSoft 的"动画事件轨道"，存在 `anibnd` 里，每条动画对应一个时间轴，上面挂着按帧触发的事件。常见事件类型：

| 事件 | 作用 | 对应 Unity 里要做的事 |
|------|------|----------------------|
| **攻击判定（Hitbox / Behavior）** | 第几帧到第几帧、用哪个判定盒造成伤害 | 开/关攻击 Collider 或触发命中检测 |
| **无敌帧 / 受身（Invulnerability / iframe）** | 翻滚等动作的无敌区间 | 切换无敌状态标志 |
| **可取消窗口（Cancel / Combo window）** | 何时能接下一招 / 转向 / 移动 | 输入缓冲 & 连招判定 |
| **SFX（视觉特效）** | 第几帧在哪个挂点放粒子 | 触发 VFX |
| **SoundEffect（音效）** | 脚步声 / 挥砍声 / 语音 | 触发 AudioSource |
| **Root Motion / 位移** | 强制位移、贴地 | 应用根运动 |
| **武器轨迹 / 残影** | 刀光 | Trail 特效 |

> 一句话：**TAE 就是你想要的"动画事件"。** 它和动画是同一个 ID，所以"哪条动画带哪些事件"是天然对应的，不用你手配——关键是把它**解析出来并转成 Unity 能用的格式**。

### 6.2 如何"复用 ER 原始事件"而不是逐条手配（自动化思路）

核心：**解析 TAE → 程序化生成 Unity 数据**。两条可行路线：

**路线 1：Soulstruct 读 TAE（推荐，因为你已在用 Soulstruct）**

Soulstruct（Python 库）能读写 ER 的 TAE：
1. 用 Soulstruct 打开 `anibnd`，遍历每条动画的 TAE，拿到 `事件类型 / 起始帧 / 结束帧 / 参数`。
2. 把这些导成中间格式（建议 **JSON**，每条动画一个条目）：
   ```json
   {
     "animId": "a000_003000",
     "frameRate": 30,
     "events": [
       { "type": "Hitbox",   "start": 8,  "end": 14, "param": { "atkId": 3000 } },
       { "type": "Invuln",   "start": 2,  "end": 22 },
       { "type": "SFX",      "start": 8,  "dummyPoly": 145, "sfxId": 523003 },
       { "type": "Sound",    "start": 8,  "soundId": 120 },
       { "type": "Cancel",   "start": 20, "end": 30 }
     ]
   }
   ```
3. 在 Unity 写一个 **编辑器脚本（C#）**，读这份 JSON，对每个导入的 AnimationClip：
   - 把帧号换算成时间（`time = frame / frameRate`），
   - 用 `AnimationUtility.SetAnimationEvents()` 批量写入 `AnimationEvent`（`functionName` 用你统一的回调，如 `OnHitboxStart` / `OnIFrameStart` / `OnSpawnVFX`，`intParameter`/`stringParameter` 传 atkId/sfxId）。
   - 或者生成 **ScriptableObject**（每条动画一个事件资产），运行时由你的战斗状态机读取（比 AnimationEvent 更灵活，推荐做 ARPG 用这种）。

**路线 2：DSAnimStudio 导出 + 手写转换器**

DSAnimStudio 能可视化所有 TAE 事件，可逐条核对/导出，再用脚本转 Unity。适合先小规模验证映射是否正确。

### 6.3 落地建议（给 Project-ARPG）

- **不要在 Unity 里逐条手配事件**——那是几千条，必然崩。做"TAE → JSON → Unity 资产"的一次性管线。
- 先只解析你战斗真正要用的事件类型：**攻击判定帧、无敌帧、可取消窗口**（这三类决定手感）。特效/音效作为第二批。
- 帧率：ER 动画帧率因动画而异（常见 24fps 或 30fps），导 FBX 时确认采样率一致，否则事件帧会错位。
- ER 的 `sfxId` / `soundId` / `atkId` 是 ER 自己的资源 ID，**在 Unity 里无法直接用**——需要建一张"ER ID → 你项目资源"的映射表，转换时查表替换。这一步无法全自动，但"事件的时机/帧"可以全自动继承。

---

## 7. Soulstruct + Blender 自动贴图

你的目标：Soulstruct 导入 Blender 时**自动把贴图贴上**，不手动连节点。

### 7.1 自动贴图的前提

Soulstruct 的 FLVER 导入器会读模型材质里的贴图引用（来自 MTD/MATBIN + TPF），自动建 Blender 材质节点。要让它**找得到贴图**，必须满足：

1. **Soulstruct 配置了游戏根目录**：在 Blender 3D Viewport 侧栏 **Soulstruct tab → Settings** 中设置：
   - `Game` = **ELDEN_RING**（属性名 `game_enum`）
   - `Game Root` = `D:\Game\ELDEN RING\Game`（属性名 `eldenring_game_root_str`）
   
   > ⚠️ 脚本批处理时须在代码中显式设置这两个属性，否则可能默认为 Dark Souls: Remastered 导致报错。
2. **贴图来源齐全**：
   - 怪物：贴图在 `cXXXX_h.texbnd.dcx` —— 导入 `cXXXX.chrbnd` 时要让 Soulstruct 能访问到对应 texbnd（同一 chr 文件夹下即可）。
   - 玩家装备：贴图在各 `partsbnd` 内，自带，一般能直接读到。
3. **导入时勾选贴图相关选项**：Soulstruct 的 FLVER Import 面板里打开 **Import Textures / Load Textures**（把 TPF 里的贴图解出来建成 Image 节点）以及材质构建选项。

### 7.2 推荐流程

1. Blender 安装 Soulstruct 插件 → 3D Viewport 侧栏 Soulstruct tab → Settings 里设置 **Game = ELDEN_RING**、**Game Root = `...\ELDEN RING\Game`**。
2. 用 Soulstruct 面板 **Import Character (chrbnd)**（或 Import FLVER），选 `cXXXX.chrbnd.dcx`。
3. 确认勾上"导入贴图 / 构建材质"。导入后材质球里 Albedo/Normal 等应已连好。
4. 若贴图仍缺：手动先用 WitchyBND 把 `cXXXX_h.texbnd.dcx` 解成 TPF→DDS，放到 Soulstruct 期望的贴图目录，再重导。

### 7.3 法线贴图注意

ER 法线是**双通道（RG）**、且常带打包通道。Blender 里：法线贴图节点的 Color Space 要设 **Non-Color**，并经 Normal Map 节点；Soulstruct 通常已处理好，若发现凹凸反了，检查绿通道是否需翻转。导入 Unity 时同样要把法线贴图标记为 **Normal Map** 类型（见解包总教程 §9.4）。

---

## 8. HKX 动画 → FBX → Unity 批量流程

你已经把动画导成了 FBX。这里给"批量 + 可区分 + 不丢事件"的组织建议。

### 8.1 导出 FBX 时

- **每条动画导成一个 FBX**，按 anibnd 源分文件夹，命名**保留原始动画 ID**。实际输出结构：
  ```
  Animations/
    c0000_a00_hi/a000_010000.fbx
    c0000_a0x/a002_000000.fbx
    c0000_dlc01/a200_000000.fbx
    ...
  ```
  **千万不要重命名成无意义的序号**——动画 ID 是你回查 TAE/参数的唯一钥匙。
- 确认骨架与模型骨架一致（同一 skeleton.hkx），否则 Unity 里重定向会错。
- 采样率取决于原始 HKX（常见 **24fps** 或 30fps），导出时确认 Blender 的 Sampling Rate 设为 1.0 以保持原始帧率。
- **必须包含父级 Empty 节点**：FBX 导出时须选中骨架的父级 Empty，否则 Unity 里会缺少 `c0000 Armature` 这层节点，导致 AnimationClip 的曲线路径绑定失败。
- **clip 命名技巧**：FBX 导出设置 `use_all_actions=False` + `use_nla_strips=False` 时，Blender 以 Scene 名作为 clip 名。批量脚本中在每次导出前设置 `bpy.context.scene.name = anim_name` 即可得到正确的 clip 名。

### 8.2 Unity 导入与 Avatar 配置

- 在 `Assets/_ELDENRING_REF/Animations/cXXXX/` 下按角色分目录。
- **骨架 FBX**（c0000 骨架模型）：Rig → Animation Type = **Generic**，Avatar Definition = **Create From This Model**，生成 c0000Avatar。
- **动画 FBX**：Rig → Animation Type = **Generic**，Avatar Definition = **Copy From Other Avatar**，Source = c0000Avatar。
- **Part FBX**（装备模型）：同上，Copy From c0000Avatar，确保 skinning 能正确匹配。
- 用一份 **`动画ID → 语义名` 对照表**（§5 用 DSAnimStudio 建的）做批量重命名/打标签，例如给 AnimationClip 加标签 `Idle / Run / Roll / Attack_R1_01 / Hit_Front / Death`。
- 写编辑器脚本：导入后自动按对照表设置 Clip 名、循环标志（locomotion 类设 Loop）、根运动开关。

### 8.3 事件回填

按 §6.2，把 TAE 解析出的 JSON 用编辑器脚本写回这些 Clip 的 AnimationEvent / 生成事件 ScriptableObject。这样"原版的攻击判定帧、无敌帧"就自动继承了。

### 8.4 区分动画用途的最终产物

建议在工程里维护一张表（CSV/ScriptableObject）：

| animId | 角色 | 语义名 | 类型 | 关键事件帧 | 备注 |
|--------|------|--------|------|-----------|------|
| a000_000000 | c0000 | Idle | Locomotion | - | 待机循环 |
| a000_003000 | c0000 | Attack_R1_01 | Attack | hit 8-14, cancel 20-30 | 轻击一段 |
| ... | | | | | |

这张表就是你"区分每条动画干嘛用"的总索引，由 DSAnimStudio + TAE 解析共同填充。

### 8.5 骨骼修正（Universal Skeleton → 正确 Rest Pose）

c0000 的**通用骨架**（从 anibnd 中的 skeleton.hkx 来）的 rest pose 和各 part 模型自带的骨骼位置不一致。如果不修正，Unity 里 skinning 会错位。关键步骤：

1. 导入 c0000 通用骨架（从 chrbnd 或 anibnd）。
2. 导入各 part 的 FLVER（每个 partsbnd 自带一份部分骨架）。
3. 用 "Apply Pose as Rest Pose" 或脚本将 part 骨架的 bone tail/roll 信息传递到通用骨架。
4. 修正后的骨架用于导出动画和模型 FBX，确保 Unity 端一致。

> ⚠️ 这是整个工作流中最容易出问题的环节。如果 Unity 中发现模型 T-Pose 变形或动画播放时肢体扭曲，首先检查骨骼 rest pose 是否一致。

### 8.6 批量处理注意事项

#### `blender --background` 模式

Blender 支持无界面后台运行（`blender --background --python script.py`），适合批量导出。但 Soulstruct 插件需要 patch 两处才能正常工作：

1. **`draw_regions.py`**（`soulstruct/blender/msb/draw_regions.py`）：
   模块级 `gpu.shader.from_builtin()` 在无 GPU 上下文时崩溃。需加 `if bpy.app.background:` 守卫跳过 shader/batch 初始化。

2. **`import_operators.py`**（`soulstruct/blender/flver/models/operators/import_operators.py`）：
   导入完成后的 `bpy.ops.view3d.view_selected()` 在无 3D Viewport 时崩溃。需加 `if not bpy.app.background:` 守卫。

#### 并行处理

Blender 单进程无法并行导入/导出。但可以**同时启动多个 `blender --background` 进程**，每个处理不同的 anibnd 包或 parts 子集，实现并行加速。

#### 常见报错

| 报错 | 原因 | 解决 |
|------|------|------|
| `has no attribute 'soulstruct_settings'` | Soulstruct 插件注册失败（通常因 GPU crash） | patch draw_regions.py |
| `view3d.view_selected` context error | 无 3D Viewport | patch import_operators.py |
| MTDBND 找不到 / Dark Souls Remastered | game_enum 未设为 ELDEN_RING | 脚本开头调用 setup_soulstruct_settings() |

---

## 9. 怪物 / Boss 工作流

怪物/Boss 比玩家**简单**（非模块化、动画通常单包）：

1. **定位**：用 §2.1 的对照表/Smithbox 找到目标 Boss 的 `cXXXX`。
2. **模型 + 贴图**：Soulstruct 导入 `cXXXX.chrbnd.dcx`（配 `cXXXX_h.texbnd.dcx` 自动贴图，见 §7）。
   - 大型 Boss 若有 `_div00/_div01` 分体，需都导入再拼。
3. **动画**：解包 `cXXXX.anibnd.dcx`（通常就一个包）→ HKX → FBX，命名保留 ID。
4. **事件**：解析该 anibnd 里的 TAE → JSON → Unity（§6）。Boss 的攻击判定/阶段切换都在 TAE + BehaviorParam 里。
5. **数值/AI 参考**：Smithbox 看该 Boss 的 NpcParam（血量/防御/部位）、AtkParam（各招伤害）、行为图（连招逻辑）。

> Boss 的"招式含义"同样用 **DSAnimStudio 逐条预览**最快——一个 Boss 动画量远小于玩家，半小时就能把整套招式建好对照表。

---

## 10. 工具清单与参考

| 工具 | 用途 | 地址 |
|------|------|------|
| **Soulstruct (+ Blender 插件)** | FLVER/动画/TAE 读取，自动建材质，Python 脚本化 | github: Grimrukh/soulstruct |
| **DSAnimStudio** | 动画 + TAE 事件 实时可视化（区分动画神器） | github: Meowmaritus/DSAnimStudio |
| **WitchyBND** | 解包 anibnd/chrbnd/texbnd → hkx/flver/tpf | github: ividyon/WitchyBND |
| **Smithbox** | regulation.bin 参数（NpcParam/AtkParam/Behavior）、msg 文本对照 | github: vawser/Smithbox |
| **Paramdex** | ER 参数字段定义 | github: soulsmods/Paramdex |
| **texconv** | DDS → PNG | github: microsoft/DirectXTex |
| 社区 chr/anim ID 表 | `cXXXX → 敌人名`、动画 ID 语义 | 搜 "Elden Ring chr id list" |

---

### 一页流程图

```
chr/cXXXX.*  (解包源)
   │
   ├─ chrbnd ─[Soulstruct→Blender, 自动贴图]→ 模型(带材质) ─→ FBX ─→ Unity
   │            (配 texbnd_h 才有贴图)
   │
   ├─ anibnd ─┬─ skeleton.hkx ──────────────────────────────────┐ (动画解析依赖骨架)
   │          ├─ HKX 动画 ─[需要skeleton]→ FBX(保留animID) ────┤
   │          │                                                 ├─→ Unity AnimationClip
   │          └─ TAE 事件 ─[Soulstruct/Python]→ JSON ───────────┘   + 自动写 AnimationEvent/SO
   │                          (攻击帧/无敌帧/取消窗口/SFX/音效)
   │
   ├─ behbnd ─[DSAnimStudio]→ 状态→动画ID 映射 (理解用途)
   │
   └─ regulation.bin ─[Smithbox]→ NpcParam/AtkParam/Behavior (名字/数值/判定)
                                   └→ msg 文本 = 中文名
```

---

**文档版本**：v1.1 ｜ **更新**：2026-06-10 ｜ **适用**：Elden Ring 1.10+ / SOTE DLC
**数据来源**：本机解包目录 `D:\Game\ELDEN RING\Game`（chr 约 280 个角色 ID，parts 约 2993 件 / 非 LOD 约 1498 件，c0000 动画 9336 条）
