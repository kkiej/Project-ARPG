# Elden Ring 资源解包完整教程

> 本文档记录从《艾尔登法环》游戏本体提取资产并转换为 Unity 可用格式（FBX、PNG）的完整工作流，供 Project-ARPG 项目学习参考使用。
>
> **版权声明**：所有解包内容仅用于个人学习研究，**严禁用于商业项目、二次分发或重新销售**。

---

## 目录

1. [前置准备](#1-前置准备)
2. [阶段一：解密游戏本体（UXM）](#2-阶段一解密游戏本体uxm)
3. [阶段二：理解游戏目录结构](#3-阶段二理解游戏目录结构)
4. [阶段三：解包地图资源（WitchyBND）](#4-阶段三解包地图资源witchybnd)
5. [阶段四：FLVER 转 FBX（Aqua-Toolset）](#5-阶段四flver-转-fbxaqua-toolset)
6. [阶段五：贴图处理（TPF → DDS → PNG）](#6-阶段五贴图处理tpf--dds--png)
7. [阶段六：地图布局数据（MSB / Smithbox）](#7-阶段六地图布局数据msb--smithbox)
8. [阶段七：角色与动画（进阶）](#8-阶段七角色与动画进阶)
9. [Unity 导入注意事项](#9-unity-导入注意事项)
10. [常见问题](#10-常见问题)
11. [实用脚本](#11-实用脚本)

---

## 1. 前置准备

### 1.1 必要工具清单

| 工具 | 用途 | 下载地址 |
|------|------|----------|
| **UXM Selective Unpacker** | 解密游戏本体，解包 `.bdt`/`.bhd` 主存档 | Nexus Mods 搜 "UXM Selective Unpacker" |
| **WitchyBND** | 解包/打包 `.dcx`、`.bnd`、`.tpf` 等所有 FromSoft 格式（推荐，替代 Yabber） | <https://github.com/ividyon/WitchyBND/releases> |
| **Aqua-Toolset** | `.flver` → `.fbx` 模型转换 | <https://github.com/Shadowth117/Aqua-Toolset/releases> |
| **Noesis** + Souls 插件 | 模型预览 / 备用转换器 | <https://richwhitehouse.com/index.php?content=inc_projects.php> |
| **Smithbox** | 地图（MSB）/ 参数表（PARAM）/ 材质可视化编辑 | Nexus Mods 搜 "Smithbox" |
| **texconv** | DDS 批量转 PNG | <https://github.com/microsoft/DirectXTex/releases> |
| **Paint.NET** 或 GIMP + DDS 插件 | 单张 DDS 查看 | 可选 |

### 1.2 环境要求

- **.NET 7 / 8 Desktop Runtime**（WitchyBND、Smithbox 都需要）
- **磁盘空间**：单地图解包后约 5 ~ 15 GB；完整解包整个游戏约 100+ GB
- **Windows 10/11**

### 1.3 法律 & 安全提醒

- ❌ 不要在联机模式下使用解包过的游戏（Easy Anti-Cheat 会检测）
- ✅ 解包前先备份整个 `Game/` 文件夹
- ❌ 不要把解出来的资产上传到公开仓库或商业项目

---

## 2. 阶段一：解密游戏本体（UXM）

法环原版游戏文件是加密打包的，必须先用 UXM 解密。

### 2.1 操作步骤

1. **备份**：把 `ELDEN RING/Game/` 整个文件夹复制一份当原版备份
2. 打开 `UXM Selective Unpacker.exe`
3. 在 **Game executable** 选择 `eldenring.exe`
4. 点击 **Unpack** 等待完成

### 2.2 验证结果

解包完成后，`Game/` 目录会自动出现以下子文件夹：

```
Game/
├── asset/         通用资产（武器、家具、特效）
├── chr/           角色（玩家、NPC、Boss、敌人）
├── cutscene/      过场动画
├── event/         事件脚本
├── facegen/       捏脸数据
├── map/           地图 ⭐ 重点
├── msg/           文本 / 本地化
├── param/         参数表
├── parts/         玩家可换装备零件
├── remo/          过场镜头
├── script/        Lua 脚本
├── sfx/           视觉特效
└── sound/         音效
```

> 💡 解密只做一次。后续所有解包都基于这些目录下的文件。

---

## 3. 阶段二：理解游戏目录结构

### 3.1 地图命名规则

地图路径 `Game/map/m10/m10_00_00_00/` 的含义：

| 段 | 含义 | 范围 |
|----|------|------|
| `m10` | **大地图区域** | 10=史东薇尔/宁姆格福，11=雷亚卢卡利亚，12=盖利德，13=圣树王城，14=亚坛高原，15=史东薇尔城，16=蒙格温王朝，60=无尽世界地图等 |
| `_00` | 区块（block） | 同一区域内的分区 |
| `_00` | 子区块 | |
| `_00` | LOD / 版本 | |

### 3.2 地图文件类型

| 后缀 | 内容 | 处理方式 |
|------|------|----------|
| `.mapbnd.dcx` | 地图块模型+贴图打包 | WitchyBND 解包 → flver + tpf |
| `.btl.dcx` | 灯光数据 | Smithbox 查看 |
| `.btab.dcx` | 灯光绑定表 | Smithbox |
| `.msb.dcx` | **地图布局**（物体位置、敌人配置） | Smithbox |
| `.nvmhktbnd.dcx` | 导航网格 | 一般不用 |
| `.entryfilelist` | 索引清单 | 不用动 |
| `.hkxbhd` + `.hkxbdt` | 碰撞数据（Havok，成对） | WitchyBND |
| `.gibhd` + `.gibdt` | GI 烘焙数据（成对） | 不用动 |

---

## 4. 阶段三：解包地图资源（WitchyBND）

> ⚠️ **如果之前用过 Yabber**：建议先清理 Yabber 解出来的旧文件夹（命名形如 `xxx-mapbnd-dcx`），再用 WitchyBND 重新解一次。WitchyBND 对 ER 新格式兼容性更好，且支持递归一次解到底。

### 4.1 配置 WitchyBND

首次运行 `WitchyBND.exe` 会进入配置菜单，按以下推荐设置：

**第一页：**

| 选项 | 设置 |
|------|------|
| Use specialized BND handlers | ✅ Enabled |
| **Recursive binder processing** | ✅ **Enabled** ⭐ 关键设置 |
| Parallel processing | ✅ Enabled |
| Pause on error | ✅ Enabled |
| Offline mode | ✅ Enabled（避开 GitHub 限流） |
| Unpack TAE as folder | ✅ Enabled |
| Flexible decompression | ✅ Enabled |

**第二页：Configure Windows integration** 子菜单：

1. ✅ **Register WitchyBND context menu**（注册右键菜单）
2. ✅ **Unregister old Witchy/Yabber context menu**（清理 Yabber 旧菜单）
3. ✅ **Add WitchyBND to PATH environment variable**（命令行调用用）

### 4.2 解包操作

1. 进入 `Game/map/m10/m10_00_00_00/`
2. 资源管理器搜索框输入 `*.dcx` 过滤
3. **Ctrl + A** 全选所有 `.dcx`（同时建议也选上 `.hkxbhd` + `.hkxbdt` 对）
4. **右键** → **WitchyBND → Unpack**（Win11 可能在"显示更多选项"里）
5. 等待 5-15 分钟完成

### 4.3 解包结果验证

解包成功后，每个 `.dcx` 会生成一个同名文件夹：

```
m10_00_00_00_000004-mapbnd-dcx/
├── _witchy-bnd4.xml        ← 重打包元数据，不要删
└── GR/data/INTERROOT_win64/map/m10_00_00_00/sib/
    ├── m10_00_00_00_000004.flver    ← 模型
    └── m10_00_00_00_000004.tpf      ← 贴图（也可能在外层 tpfbhd/tpfbdt 里）
```

### 4.4 提取所有 FLVER 到统一目录

参考 [§11 实用脚本 - extract-flver.ps1](#111-extract-flverps1)

---

## 5. 阶段四：FLVER 转 FBX（Aqua-Toolset）

### 5.1 工具入口

新版 Aqua-Toolset 把 Souls 系工具拆成独立程序，文件夹下你会看到多个 exe：

| 文件 | 用途 | 法环用 |
|------|------|--------|
| **`SoulsModelTool.exe`** | Souls/FromSoft 专用 GUI | ⭐ **首选** |
| `AquaModelTool.exe` | 主程序（PSO2 + Souls + 其他） | 备用 |
| `AquaAutoRig.exe` | 自动绑骨 | 角色绑骨用 |
| `CMXPatcher.exe` / `WeaponInstaller.exe` | PSO2 专用 | 跳过 |

### 5.2 GUI 批量转换（推荐 SoulsModelTool）

#### 启动与设置

1. 打开 `SoulsModelTool.exe`
2. **File → `Set Game (For MSB Extraction)`** → 选 **Elden Ring** ⭐ 必做

#### 关键设置（首次使用必调）

**左侧 Fromsoft modifiers：**

| 选项 | 推荐值 | 说明 |
|------|--------|------|
| Convert FLVER with metadata | ✅ | 嵌入元数据 |
| **Apply material names to mesh** | ✅ | 网格名带材质名，Unity 里好辨认 |
| Transform mesh | ✅ | 应用变换 |
| Add FBX Root Node | ❌ | 这是 Blender 用的，Unity 不需要 |
| Add the FLVER's dummy nodes | 地图❌ / 角色✅ | 武器挂点等 |
| Parent dummy nodes to attach nodes | ✅ | 跟上一项联动 |
| **Include Tangent Data** | ✅ ⭐ | **关键！** 法线贴图渲染必需 |

**MSB extraction：**

| 选项 | 推荐 |
|------|------|
| Extract Unreferenced Files | ✅ |
| **Separate To .flver Model Instance Sets** | ✅ ⭐ Bloodborne 及之后游戏（含 ER）必须开 |

**右侧导出设置：**

| 选项 | **必须设置** | 说明 |
|------|------------|------|
| Export Format | Fbx (Default) | 保持 |
| Mirror Type | Mirror Z (Default) | 法环需镜像 Z |
| **FBX Coordinate System** | **OpenGL Y Up (Classic, adds 90 degrees)** ⭐⭐⭐ | **必改！** Unity 是 Y-Up，默认的 BB Tool Z Up 会让模型躺地上 |
| Custom Scale | 1 | 保持 |
| Scale Handling | Use File Scale | 保持 |

> ⚠️ **最容易踩的坑**：FBX Coordinate System 必须改成 **OpenGL Y Up**，否则导入 Unity 后所有建筑会"躺平"在地上（Z 轴方向错）。

#### 转换操作

1. 关闭设置窗口（设置自动保存）
2. **File → Convert From Soft model, TPF, or archive**
3. 在文件对话框中导航到 `all_flver` 目录
4. **Ctrl + A 全选** 所有 flver 文件（**注意：必须多选，单选只会转一个**）
5. 点 **打开** → 等待转换

#### 输出文件

每个 `.flver` 会生成 4 个文件：

| 文件 | 用途 | Unity 需要 |
|------|------|-----------|
| `xxx.fbx` | ⭐ **主模型文件** | ✅ |
| `xxx.flver.matData.json` | 材质元数据 | ❌ 可忽略 |
| `xxx.flver.dummyData.json` | dummy 点元数据 | ❌ 可忽略 |
| `xxx.flver.boneData.json` | 骨骼元数据 | ❌ 可忽略 |

3 个 JSON 是为了反向转换（fbx → flver）用的，做参考用不到。

### 5.3 备用：用 AquaModelTool 主程序

如果 `SoulsModelTool.exe` 启动有问题，可以用 `AquaModelTool.exe`：

菜单路径：**Tools → Souls Games → FromSoft Model Tools → FLVER → Convert to FBX**

操作选项与 SoulsModelTool 相同。

### 5.4 验证 FBX

用 **Blender** 或 **Autodesk FBX Review** 打开输出的 fbx：

- ✅ 模型几何正常
- ✅ 贴图坐标（UV）正确（如果有贴图）
- ✅ 没有"线条乱飞"现象（这是 Noesis 老版本的 bug，Aqua-Toolset 不会有）

---

## 6. 阶段五：贴图处理 ⚠️ 重要发现

### 6.1 ⚠️ 法环地图模型贴图无法 1:1 还原

> **这一节务必先读，否则会浪费大量时间。**

经过实测，**法环的地图模型（mapbnd 里的 flver）使用的是 SAT（Shader Asset Template）程序化材质系统**，跟 Dark Souls 3 / Sekiro 这种"模型直接绑定贴图"完全不同。

**证据 1：SoulsModelTool 输出的 `*.flver.matData.json` 显示所有贴图 Path 都是空的：**

```json
{
  "Name": "m10_00_001",
  "MTD": "...Map_m10_00\\m10_00_001.mtd",
  "Textures": [
    {
      "ParamName": "M_AMSN_V__snp_Texture2D_2_AlbedoMap_0",
      "Path": ""              ← 注意这里
    }
  ]
}
```

**证据 2：`allmaterial.matbinbnd.dcx` 里的 `m10_xx_xxx.matbin` 全是占位模板，只引用 `SYSTEX_Dummy.tif`：**

```xml
<Sampler>
  <Path>N:\GR\data\Other\SysTex\SYSTEX_Dummy.tif</Path>
</Sampler>
```

### 6.2 法环材质系统真相

| 组件 | 作用 |
|------|------|
| **材质名** `m10_00_001` | 仅作 ID，不绑定具体贴图 |
| **MTD / matxml** | 着色器模板（只声明 sampler 类型） |
| **ParamName** 如 `M_AMSN_V` | 编码着色器类型：A=Albedo, M=Metallic, S=Smooth, N=Normal, V=Vertex paint |
| **真实贴图** | runtime 通过 **vertex color + 世界空间投影 + 共享 aet 贴图库** 动态合成 |

**结论：法环地图贴图通过 GPU 着色器动态合成，无法导出为静态贴图。** 这跟 Unreal 的世界材质（World Material）思路类似。

### 6.3 贴图处理实战方案

#### 方案 A：保持白模（强烈推荐）⭐

地图模型保持无贴图状态，作为**几何/尺度/布局参考**。

- ✅ 工作量：0
- ✅ 用途：关卡设计、空间感、建筑结构参考
- ⚠️ 局限：模型在 Unity 里是默认材质（白色或紫色）

#### 方案 B：Smithbox 查看完整渲染

要看带贴图的完整视觉效果，**用 Smithbox 在游戏原生引擎里查看**：

1. 启动 Smithbox → New Project → Elden Ring → 指向 `Game/`
2. Map Editor → 双击 `m10_00_00_00` 等地图
3. 截图保存到 `docs/references/` 作为视觉参考

#### 方案 C：手动给关键建筑贴图（少量精修）

如果一定要有贴图的版本：

1. 从 `Game/asset/aet/aetXXX/*.tpf.dcx` 里手动选合适的贴图
2. 在 Blender 里手动赋给重要建筑（如某 Boss 战场）
3. 工作量大，适合做"封面级"参考

### 6.4 角色 / 武器贴图 ✅ 可正常导出

**角色模型（chr）和武器（parts）的贴图是直接绑定的，可以正常导出。** 流程：

```
.chrbnd.dcx  ──[WitchyBND]──►  flver + tpf
                                    │
                                [WitchyBND]
                                    ▼
                                  .dds  ──[texconv]──► .png
```

只有地图模型用 SAT 系统，做角色/武器参考时无需担心。

### 6.5 通用 TPF → DDS → PNG 流程（用于 chr / parts）

```powershell
# 1. WitchyBND 解包所有 tpf
Get-ChildItem -Recurse -Filter "*.tpf" | ForEach-Object { & WitchyBND.exe $_.FullName }

# 2. texconv 批量转 png
texconv -ft png -y -o "D:\output\png" "D:\input\dds\*.dds"
```

---

## 7. 阶段六：地图布局数据（MSB / Smithbox）

`.msb.dcx` 文件包含**关卡布局信息**：每个模型在地图中的位置、旋转、缩放、对应的 mapbnd 编号、敌人摆放等。这是设计 ARPG 关卡时最有参考价值的数据。

### 7.1 用 Smithbox 打开

1. 启动 **Smithbox**
2. **File → Open Project** → 指向 `Game/` 目录
3. 选择 **Elden Ring** 项目类型
4. 切到 **Map Editor** 标签
5. 双击地图 ID（如 `m10_00_00_00`）打开

### 7.2 可以做什么

- 🔍 浏览所有地图物体的世界位置、旋转、缩放
- 🔍 查看敌人 AI 和巡逻路径
- 🔍 查看碰撞体、触发器、灯光摆放
- 📋 导出布局为 JSON / CSV 供 Unity 关卡设计参考

### 7.3 导出布局示例

Smithbox 中右键地图 → Export → 选择导出格式（JSON 最适合脚本读取）。

---

## 8. 阶段七：角色与动画（进阶）

### 8.1 角色模型

| 路径 | 内容 |
|------|------|
| `Game/chr/c0000.chrbnd.dcx` | 玩家基础模型 |
| `Game/chr/c1000.chrbnd.dcx` ~ `c9999.chrbnd.dcx` | 各种敌人 / NPC / Boss |

解包流程：

```
.chrbnd.dcx  ──[WitchyBND]──►  flver + hkx骨骼 + tpf贴图
                                      │
                              [Aqua-Toolset]
                                      ▼
                                    .fbx
```

### 8.2 动画（最麻烦的环节）

| 路径 | 内容 |
|------|------|
| `Game/chr/c0000.anibnd.dcx` | 玩家动画 |
| `Game/chr/c1000.anibnd.dcx` ~ | 敌人动画 |

解包后得到 `.hkx`（Havok 动画格式），需要专门工具转换：

- **SoulsAssetPipeline**（C# 库）
- **HKLib** + 配套脚本
- **Aqua-Toolset** 的 hkx 实验性支持（不稳定）

> ⚠️ 动画转换是 Souls 系最难的环节，骨骼对应、根运动（root motion）经常出问题。如果只做 ARPG 参考研究，可以暂时跳过动画，只用模型 + 参数表 + 关卡布局。

> 📘 **角色 / 动画 / 贴图深入**：命名编号速查、Soulstruct 自动贴图、HKX→FBX→Unity 区分动画、**TAE 动画事件（攻击判定/无敌帧）自动化**、怪物 Boss 工作流，见专门文档 [`EldenRing-Character-Animation-Reference.md`](./EldenRing-Character-Animation-Reference.md)。

---

## 9. Unity 导入注意事项

### 9.1 坐标系与单位

- ⚠️ **法环原生是 Z-Up 左手系**（不是常说的"和 Unity 一样"）
- ✅ 单位：米
- 🔑 **必须在 SoulsModelTool 导出时勾选 `OpenGL Y Up (Classic, adds 90 degrees)`**，否则模型躺地上
- 已正确设置的 fbx 导入 Unity 后**不需要手动旋转**

### 9.2 FBX 导入设置

1. **Scale Factor**：1
2. **Convert Units**：✅ 勾选
3. **Bake Axis Conversion**：⚠️ **不要勾**（已在源头处理）
4. **Materials**：
   - 地图模型：保持 None（无意义，反正贴图没绑定）
   - 角色/武器模型：选 **Use External Materials**
5. **Textures**：选 **Extract Textures** 把贴图分离出来（仅对 chr/parts 有效）

### 9.3 关于贴图（重要更新）

**地图模型贴图**：见 [§6.1 法环 SAT 程序化材质系统](#61-️-法环地图模型贴图无法-11-还原)。地图 fbx 导入后会显示一堆 "Missing Material"，**这是正常的**，不要花时间去匹配。

**角色 / 武器模型贴图**：可正常匹配。命名约定（适用于 chr / parts）：

| 后缀 | 含义 |
|------|------|
| `*_a.dds` | Albedo / Diffuse |
| `*_n.dds` | Normal |
| `*_m.dds` | Metallic |
| `*_r.dds` | Roughness |
| `*_e.dds` | Emissive |

### 9.4 法线贴图

法环用的是**两通道法线**（RG 通道），Unity 需要在导入时选择 **Normal Map** 类型，并勾选 **Create from Grayscale = false**。

### 9.5 项目放置位置（避免污染主项目）

法环资产**不要放到 `Assets/Art/` 等主目录**，避免：
- 不小心被打包进游戏 build
- 污染 Git（资产太大）
- 商业版权风险

推荐结构：

```
Assets/_ELDENRING_REF/         ← 下划线前缀避免主项目误用
├── Models/                    ← FBX 模型
├── Textures/                  ← DDS 贴图（仅 chr / parts 用）
├── .gitignore                 ← 全部 ignore 防止入仓
└── README.md                  ← 用途说明
```

`.gitignore` 内容：

```
# 仅供本地学习参考，禁止入仓
*
!.gitignore
!README.md
```

---

## 10. 常见问题

### Q1：解包到一半报错 "Decompression failed"

→ 多半是 `.dcx` 文件损坏或不完整。重新从备份恢复该文件。

### Q2：右键看不到 WitchyBND 菜单（Windows 11）

→ Win11 默认折叠了第三方右键菜单：
- 按 **Shift + 右键** 显示完整菜单
- 或点 **显示更多选项**
- 或永久禁用 Win11 新右键：参考 ExplorerPatcher

### Q3：Aqua-Toolset 转 FBX 后模型没有贴图

→ 检查：
1. `.tpf` 是否已解包成 `.dds`
2. `.dds` 是否和 `.flver` 在同一目录
3. Aqua-Toolset 转换前打开 **Settings → 勾选 Embed Textures**

### Q4：法环更新后旧工具报错

→ 法环每次大版本更新可能改 flver 子格式：
- WitchyBND 必须升级到最新版
- Aqua-Toolset 也要升级
- 关注 ividyon、Shadowth117 的 GitHub

### Q5：联机被 EAC 检测怎么办

→ 解包过的游戏目录**禁止联机**。要联机时：
- 把备份的原版 `Game/` 还原回去
- 或装两份游戏（一份解包，一份原版）

---

## 11. 实用脚本

### 11.1 extract-flver.ps1

从指定目录递归提取所有 `.flver` 到统一文件夹（避开输出目录避免循环）。

```powershell
# 用法：修改 $src 和 $dst 路径后运行
$src = "D:\Game\ELDEN RING\Game\map\m10\m10_00_00_00"
$dst = Join-Path $src "all_flver"

if (-not (Test-Path $dst)) { New-Item -ItemType Directory -Path $dst | Out-Null }

$files = Get-ChildItem -Path $src -Recurse -Filter "*.flver" -File |
    Where-Object { $_.DirectoryName -notlike "*\all_flver*" }

Write-Output "找到 $($files.Count) 个 .flver 文件"

$dup = $files | Group-Object -Property Name | Where-Object { $_.Count -gt 1 }
if ($dup) {
    Write-Warning "存在 $($dup.Count) 组重名文件，将自动加序号"
    $idx = @{}
    foreach ($f in $files) {
        if ($dup.Name -contains $f.Name) {
            $idx[$f.Name] = ($idx[$f.Name] ?? 0) + 1
            $newName = "{0}_{1}{2}" -f $f.BaseName, $idx[$f.Name], $f.Extension
            Copy-Item $f.FullName -Destination (Join-Path $dst $newName) -Force
        } else {
            Copy-Item $f.FullName -Destination $dst -Force
        }
    }
} else {
    $files | ForEach-Object { Copy-Item $_.FullName -Destination $dst -Force }
}

Write-Output "完成：$((Get-ChildItem $dst -Filter '*.flver').Count) 个文件已复制到 $dst"
```

### 11.2 extract-tpf-to-dds.ps1

批量解包指定目录的 `.tpf.dcx` 并集中收集 `.dds`。

```powershell
$src = "D:\Game\ELDEN RING\Game\map\m10\m10_0000-tpfbhd"
$dst = "D:\output\dds"

if (-not (Test-Path $dst)) { New-Item -ItemType Directory -Path $dst | Out-Null }

# 调用 WitchyBND（需要已加入 PATH）
Get-ChildItem -Path $src -Recurse -Filter "*.tpf.dcx" -File | ForEach-Object {
    Write-Host "解包: $($_.Name)"
    & WitchyBND.exe $_.FullName
}

# 收集所有 dds
Get-ChildItem -Path $src -Recurse -Filter "*.dds" -File |
    ForEach-Object { Copy-Item $_.FullName -Destination $dst -Force }

Write-Output "完成：$((Get-ChildItem $dst -Filter '*.dds').Count) 个 DDS 已收集"
```

### 11.3 dds-to-png.ps1

批量 DDS 转 PNG（需先安装 texconv 并加入 PATH）。

```powershell
$src = "D:\output\dds"
$dst = "D:\output\png"

if (-not (Test-Path $dst)) { New-Item -ItemType Directory -Path $dst | Out-Null }

& texconv -ft png -y -o $dst "$src\*.dds"

Write-Output "完成：$((Get-ChildItem $dst -Filter '*.png').Count) 个 PNG"
```

### 11.4 cleanup-yabber-output.ps1

清理 Yabber 解包产生的旧文件夹，仅保留原始 `.dcx`。

```powershell
$path = "D:\Game\ELDEN RING\Game\map\m10\m10_00_00_00"

# Yabber 输出文件夹命名特征：以 -dcx / -bnd 结尾
Get-ChildItem -Path $path -Directory |
    Where-Object { $_.Name -match '-(dcx|bnd)$' } |
    Remove-Item -Recurse -Force

# 同时清理之前的提取目录
Remove-Item -Recurse -Force (Join-Path $path "all_flver") -ErrorAction SilentlyContinue

Write-Output "清理完成"
```

---

## 12. 完整工作流总览图

```
ELDEN RING/Game/Data*.bdt  (加密原版)
   │
   │  [UXM Selective Unpacker]
   ▼
Game/map, Game/chr, Game/param, ...   (解密后)
   │
   │  [WitchyBND - 递归模式]
   ▼
.dcx / .bnd / .tpf  →  解包到子文件夹
   │
   ├──► .flver  ──[Aqua-Toolset]──► .fbx ──┐
   │                                        │
   ├──► .tpf    ──[WitchyBND]──► .dds       │
   │                              │         ▼
   │                              └──[texconv]──► .png ──► Unity 导入
   │
   ├──► .msb    ──[Smithbox]──► 可视化关卡布局 / JSON 导出
   │
   ├──► .param  ──[Smithbox + Paramdex]──► 武器/敌人/掉落数据表
   │
   └──► .hkx    ──[SoulsAssetPipeline]──► .fbx 动画（可选，难度高）
```

---

## 13. 进度追踪

> Project-ARPG 团队实际操作记录

### 阶段进度

- [x] 阶段一：UXM 解密游戏本体
- [x] 阶段二：理解目录结构
- [x] 阶段三：解包 m10 区域全部 `.dcx`（WitchyBND）
- [x] 阶段四：FLVER → FBX 批量转换（SoulsModelTool + OpenGL Y Up）
- [x] 阶段五：贴图调研 ⚠️ **确认地图贴图为 SAT 程序化材质，无法还原**
- [ ] 阶段六：MSB 关卡布局分析（待开始）
- [ ] 阶段七：角色 / 动画（按需）
- [x] Unity 导入与验证（m10 全部 321 个 fbx 已导入）

### m10 区域（史东薇尔/宁姆格福）完成情况

| 子区域 | mapbnd 数 | 推测内容 | 状态 |
|--------|----------|---------|------|
| `m10_00_00_00` | 271 | 主区（地表，绝大部分建筑/地形） | ✅ |
| `m10_00_00_99` | 28 | 特殊用途（推测：室内/剧情场景/过场） | ✅ |
| `m10_01_00_00` | 22 | 第 2 区块（推测：地下/塔楼独立部分） | ✅ |
| **总计** | **321** | | **全部导入 Unity** (336 MB) |

> ⚠️ `m10/common/` 下的 `m10_cgrading.tpf.dcx` 是颜色分级 LUT 贴图，非模型，跳过即可。

### 下一步可选方向

| 方向 | 描述 | 预计工作量 |
|------|------|-----------|
| **m11-m19 其他大区域** | 雷亚卢卡利亚、盖利德、圣树王城等 | 每区 1-2 小时 |
| **MSB 布局提取** | 用 Smithbox 获取 321 个模型的世界位置 | 0.5-1 天 |
| **角色 / Boss 模型** | `Game/chr/` 下解包 + 贴图 | 1-2 天 |
| **参数表 (PARAM)** | 武器、敌人、掉落数据 | 0.5 天 |
| **Smithbox 视觉参考截图** | 在原生引擎里截图保存 | 几小时 |

---

## 14. 参考资料

- [Souls Modding Wiki - Tools Database](https://soulsmodding.wikidot.com/tool:main)
- [WitchyBND GitHub](https://github.com/ividyon/WitchyBND)
- [Aqua-Toolset GitHub](https://github.com/Shadowth117/Aqua-Toolset)
- [Smithbox GitHub](https://github.com/vawser/Smithbox)
- [Paramdex (ER 参数定义)](https://github.com/soulsmods/Paramdex)
- [SoulsAssetPipeline](https://github.com/MeowMaritus/SoulsAssetPipeline)

---

**文档版本**：v1.1
**最后更新**：2026-05-20
**适用游戏版本**：Elden Ring 1.10+ / Shadow of the Erdtree

### 更新日志

#### v1.1 (2026-05-20)

- ⭐ §5：补充 SoulsModelTool 关键设置 **FBX Coordinate System = OpenGL Y Up**，避免模型躺地上
- ⭐ §6：**重写阶段五**，加入法环 SAT 程序化材质系统的实测发现（地图贴图无法 1:1 还原）
- §6：新增 6.4 角色/武器贴图可正常导出说明
- §9：修正坐标系描述（法环原生 Z-Up，靠工具转 Y-Up）
- §9：新增 9.5 项目放置位置最佳实践（`Assets/_ELDENRING_REF/`）
- §13：进度追踪从概要 checklist 扩展为详细的 m10 区域统计表
- §13：增加 "下一步可选方向" 表

#### v1.0 (2026-05-20)

- 初始版本，覆盖 UXM → WitchyBND → Aqua-Toolset → Unity 完整流程
