# Elden Ring 风格大场景渲染 —— 技术美术作品集执行方案

> 本文档是**个人技术美术作品集**的规划，与 GachaGame 主工程无关，只是暂放在这里。
> 目标产出物不是游戏，而是一个「光照与大气技术 Demo」+ 一份技术 Breakdown。
>
> 基础参考实现：`D:\UnityProject\IllusionRP`（`com.kurisu.illusion-render-pipelines` v1.2.5，依赖 URP 17.3.0）

---

## 0. 先说结论：优先级从哪来

复刻 ER 最常见的失败模式，是把预算砸在模型精度和 PBR 材质上。按对「像不像」的实际贡献排序，ER 的视觉特征是：

| 排序 | 特征 | 对应技术 | 权重 |
|---|---|---|---|
| 1 | **分层雾 + 航空透视**，远山层层递退，尺度感全靠这个 | 物理大气散射 + 体积雾 | 🔴 决定性 |
| 2 | **天光主导**，太阳很软，大量阴天/黄昏/雨后场景 | 间接光（GI）质量 = 画面质量 | 🔴 决定性 |
| 3 | **体积光轴**，云隙光、遗迹窗光、树冠透光 | 体积雾内 raymarch 阴影 | 🔴 高 |
| 4 | **低对比去饱和中间调 + 高光溢出** | filmic tonemap + 物理 bloom | 🟠 中高 |
| 5 | **巨物地标染色环境**（黄金树把整片区域染金） | 大面积自发光体进 GI | 🟠 中 |
| 6 | **潮湿感** | wetness 系统 | 🟡 中 |
| 7 | 模型精度 / 材质细节 | —— | ⚪ 低 |

**第 2 条是本方案的技术主线**：ER 式光照是天光主导的，所以 ambient/GI 的质量直接等于画面质量。这就是 PRTGI（或同类 probe GI）在这个项目里不是配菜、而是主菜的原因。

> 对比参考：在固定机位的近景 diorama 里，PRTGI 的 SH L2 只能表达低频，是**质量降级**，烘焙 lightmap 更优。开放世界三个条件全部翻转（连续 24h 光照态 / km² 体积 / 中远景为主），才轮到 PRTGI。这个判断依据见第 6 节。

---

## 1. 前置决策

### 1.1 管线选择

| 方案 | 优点 | 缺点 | 作品集含金量 |
|---|---|---|---|
| **HDRP** | 大气/体积雾/体积云/SSR/SSGI/APV 全部现成 | 你只是在**用**功能，不是在**做**功能 | ⭐ 低 |
| **URP + IllusionRP**（推荐） | 屏幕空间那一层已备齐，可专注补大场景缺口 | 需划清「哪些是我写的」 | ⭐⭐⭐ 高 |
| **纯 URP 自己写全部** | 每一行都是自己的 | 慢 3-5 倍，容易做不完 | ⭐⭐⭐⭐ 最高 |
| 自研 SRP | —— | 作品集周期内做不完，不建议 | —— |

**推荐第 2 条**，但必须处理好归属问题。

### 1.2 ⚠️ IllusionRP 的归属边界（重要）

`package.json` 的 author 是 **AkiKurisu**。如果这套不是你写的，直接拿来当底座会在评审时面临「那你写了什么」的质疑。两个处理方式：

- **方式 A（推荐）**：明确划界。技术文档首页写清「渲染管线基于开源项目 IllusionRP，本项目新增模块：大气散射 / 时间轴系统 / GPU 驱动植被 / 地形材质 / …」，并在每个模块标注是新增还是改造。这是业界正常做法，坦诚反而加分。
- **方式 B**：只把它当参考实现**读**，自己在纯 URP 上重写需要的部分。含金量最高，但周期翻倍。

**绝对不要**：不声明来源地整体展示。这是作品集里最严重的失分项。

### 1.3 规模决策：不要做 ER 的地图尺寸

真实 ER 的交界地约 79 km²。本项目建议 **512m × 512m 单个 vista**。

理由：一个高质量 vista 比一张稀疏的 10km 地图有说服力得多。评审看的是渲染技术深度，不是关卡产能。规模限制带来的技术约束（见 6.2 的 probe 预算）在这个尺寸下基本不存在。

---

## 2. 现状盘点：IllusionRP 有什么、缺什么

扫过 `Runtime/RenderPipeline/` 的实际存量：

### ✅ 已有（可直接用/微调）

| 模块 | 文件 |
|---|---|
| 体积雾 | `PostProcessing/VolumetricFog/VolumetricFogPass.cs` |
| 自动曝光 | `PostProcessing/Exposure/ExposurePass.cs` + `ExposureDebugPass.cs` |
| 高级 Tonemapping | `PostProcessing/Tonemapping/AdvancedTonemappingPass.cs` |
| 卷积 Bloom | `PostProcessing/ConvolutionBloom/ConvolutionBloomPass.cs` |
| GTAO | `ScreenSpaceLighting/GroundTruthAmbientOcclusion/` |
| SSGI | `ScreenSpaceLighting/ScreenSpaceGlobalIllumination/` |
| SSR（含水面） | `ScreenSpaceLighting/ScreenSpaceReflection/` + `WaterSSRDataPass.cs` |
| Contact Shadows | `Shadows/ContactShadows/` + `DiffuseShadowDenoisePass.cs` |
| Per-Object Shadow | `Shadows/PerObjectShadow/` |
| 屏幕空间阴影 + 时域降噪 | `Shadows/ScreenSpaceShadows/` |
| 次表面散射 | `SubsurfaceScattering/SubsurfaceScatteringPass.cs` |
| 加权混合 OIT | `Transparency/WeightedBlendedOITPass.cs` |
| VRS | `VRS/StencilVRSGenerationPass.cs` |
| Color/Depth Pyramid | `ColorPyramidPass.cs` / `DepthPyramidPass.cs` |
| PreIntegratedFGD（split-sum） | `PreIntegratedFGD/PreIntegratedFGDPass.cs` |
| **PRTGI** | `PrecomputeRadianceTransfer/PRTRelightPass.cs` + `Shaders/PrecomputeRadianceTransfer/*.compute` |
| 光追（存在，未细查） | `RayTracing/` |

**结论：屏幕空间那一层基本完备。** 这省掉了大量工作。

### ❌ 缺口（正好是 ER 味道最关键的几项）

| 缺什么 | 现状 | ER 影响 | 本方案步骤 |
|---|---|---|---|
| **物理大气散射** | 只有 `Shaders/Skybox/ProceduralSkybox.shader` / `GradientSkybox` / `CubemapSkybox`，无 LUT 化大气 | 🔴 特征 #1 的一半 | Step 1 |
| **时间轴驱动系统** | 无 | 🔴 作品集决定性一击 | Step 2 |
| **雾与大气的耦合** | 体积雾独立参数，未由大气驱动 | 🔴 特征 #1、#3 | Step 3 |
| **体积云** | 无 | 🟠 天空占画面 1/3 | Step 7 |
| **地形/植被 GPU 驱动渲染** | 无 | 🔴 大场景性能命门 | Step 5 |
| **地形材质系统** | 无 | 🟠 | Step 6 |
| **水体波形模拟** | 只有 `Shaders/Water/Water.shader`，无模拟 | 🟡 | Step 8 |
| **潮湿/天气系统** | 无 | 🟡 | Step 9 |
| **PRTGI cascade/streaming** | 单张固定 `RWTexture3D`，无 cascade | 🟡（本项目尺寸下非必须） | Step 4 可选项 |

---

## 3. 执行顺序总览

```
Step 1  物理大气散射 LUT          ★★★ 必做   1-2 周
Step 2  时间轴驱动系统            ★★★ 必做   3-5 天
Step 3  体积雾接大气 + 光轴        ★★★ 必做   1 周
Step 4  PRTGI 接时间轴            ★★★ 必做   1 周
──────── 到这里画面已经「像」了，以下每步都是在成立的画面上加分 ────────
Step 5  GPU 驱动植被              ★★★ 强推   2 周
Step 6  地形材质系统              ★★  强推   1 周
Step 7  体积云                    ★★  加分   1.5-2 周
Step 8  水体                      ★   加分   1 周
Step 9  潮湿/天气                 ★   加分   3-5 天
Step 10 工具化 + Debug + 性能表    ★★★ 必做   全程穿插，最后收口 1 周
```

**为什么大气排第一**：它做完的那一刻画面就已经"像"了。后面每一项都是在一个已经成立的画面上加分，而不是赌最后能不能凑出效果。这是排序的唯一原则——**先让画面成立，再堆细节**。

---

## Step 1 — 物理大气散射

### 目标
一套每帧可重算的大气模型，太阳一转，天空颜色、远景雾色、环境光自动全部一致。

### 为什么第一个做
- 同时解决 ER 特征 #1（航空透视）和 #2（天光主导）
- 便宜到可以每帧重算 → **昼夜循环是免费的**，Step 2 才有意义
- 有成熟论文和参考实现，风险低

### 推荐方案：Hillaire 2020 四 LUT 法

```
Transmittance LUT       256×64      静态，只依赖大气参数
Multi-Scattering LUT    32×32       静态
Sky-View LUT            200×100     每帧重算（很便宜）
Aerial-Perspective LUT  32×32×32    froxel volume，每帧重算，覆盖近处 ~32km
```

- 前两张只在大气参数变化时重算，做成 Editor 烘焙或启动时算一次
- 后两张每帧重算，太阳方向变化自动生效
- Aerial-Perspective LUT 是航空透视的载体，**直接就是 ER 远景层次感的来源**

### 已知坑
1. **Sky-View LUT 的天顶角参数化必须用论文里的非线性映射**（地平线附近加密）。用均匀映射会在地平线出现明显条带，这是最容易踩的坑。
2. 多次散射 LUT 的迭代收敛：论文用的是各向同性假设下的解析近似，不要试图直接做完整多次散射蒙特卡洛。
3. 单位统一：从这一步开始就用物理单位（太阳照度 ~120000 lux）。混用任意单位后面必然返工。

### 备选方案对比

| 方案 | 论文/来源 | 优点 | 缺点 | 适用 |
|---|---|---|---|---|
| **Hillaire 2020**（推荐） | *A Scalable and Production Ready Sky and Atmosphere Rendering Technique*, EGSR 2020 | 便宜、可每帧重算、有航空透视 LUT | 实现量中等 | ✅ 本项目 |
| Bruneton & Neyret 2008 | *Precomputed Atmospheric Scattering*, EGSR 2008 | 精度最高，是理论基础 | 4D LUT 大、重算贵、不适合动态昼夜 | 打理论底子必读 |
| Bruneton 2017 修订版 | github `ebruneton/precomputed_atmospheric_scattering` | 代码完整可读 | 同上 | 参考实现 |
| Nishita 1993 | *Display of the Earth Taking into Account Atmospheric Scattering* | 简单，单次散射解析 | 无多次散射，黄昏发灰发假 | 快速原型 |
| Preetham 1999 | *A Practical Analytic Model for Daylight* | 极便宜 | 解析拟合，只有天空、无航空透视 | Unity 内置 procedural skybox 就是这类 |
| Unity HDRP `PhysicallyBasedSky` | HDRP 源码 | 可直接读到 Hillaire 风格的工程实现 | 抄不动到 URP，但可对照调试 | **强烈建议对照阅读** |

### 集成要点
- 新增 `AtmospherePass`，插进 `IllusionRendererFeature` 的 pass 列表，位置在所有光照 pass 之前（因为 Step 3/4 都要读它的 LUT）
- LUT 以全局 texture 形式暴露（`Shader.SetGlobalTexture`），供雾、天空盒、PRTGI 共用
- 天空盒改为直接采样 Sky-View LUT，替换现有 `ProceduralSkybox.shader`

### 验收标准
- [ ] 太阳从 0° 转到 90° 全程无跳变、无条带
- [ ] 黄昏时地平线出现正确的橙红渐变和蓝紫对侧天空
- [ ] 远山出现可见的航空透视洗白，且随距离连续变化
- [ ] LUT 重算总耗时 < 0.3ms（1080p，中端 GPU）

---

## Step 2 — 时间轴驱动系统

### 目标
一个 0-24h 滑竿，单一数据源驱动全部光照相关参数。

### 为什么这一步最重要（作品集视角）
> **截图证明你会调一帧，滑竿证明你建了一套系统。招 TA 的人只看后者。**

这是全方案里性价比最高的一步：工作量不大，但它是把所有零散模块串成"一套系统"的那根线。没有它，你的 demo 是几个独立特效；有了它，你的 demo 是一个光照系统。

### 推荐方案：ScriptableObject Profile + 曲线

```
TimeOfDayProfile (ScriptableObject)
├─ AnimationCurve  太阳强度 / 色温
├─ Gradient        天光颜色、雾颜色染色
├─ AnimationCurve  雾密度、雾高度衰减
├─ AnimationCurve  曝光补偿目标
├─ 引用            后处理 LUT（按时段切换/混合）
└─ 大气参数        （多数由 Step 1 自动推导，只留少量艺术化覆盖）
```

一个 `TimeOfDayController` 每帧：
1. 由 `time` 求太阳/月亮方向
2. 更新大气参数 → 触发 Step 1 的 LUT 重算
3. 更新雾参数（Step 3）
4. 触发 PRTGI relight（Step 4）
5. 更新曝光目标、后处理

### 关键设计原则
**下游模块只读时间轴，绝不反向写。** 任何"某个 pass 自己偷偷改了雾颜色"的设计都会让系统失去可预测性，也失去了展示价值。

### 备选方案对比

| 方案 | 优点 | 缺点 |
|---|---|---|
| **ScriptableObject + 曲线**（推荐） | 美术可编辑、可存多套 preset、易 diff | 需要写 Editor |
| 真实天文公式驱动太阳（纬度/经度/日期） | 物理正确，有说服力 | 艺术控制弱，需额外加 override 层 |
| Timeline / 动画剪辑驱动 | Unity 原生、可做过场 | 不适合连续可交互的滑竿 |
| 纯代码 lerp 两个 preset | 最快出原型 | 只有两个状态，展示不出"连续系统" |

**建议组合**：真实天文公式算太阳方向（加分项，写在文档里很好看）+ ScriptableObject 曲线做艺术化覆盖。

### 验收标准
- [ ] 拖动滑竿，天空/雾/GI/曝光**同步连续**变化，无一项滞后或跳变
- [ ] 全部参数可在一个 Inspector 面板内编辑
- [ ] 可保存/切换至少 2 套 Profile（如「晴朗黄昏」/「阴雨白天」）

---

## Step 3 — 体积雾接大气 + 光轴

### 目标
让现有 `VolumetricFogPass` 的散射系数由大气 LUT 驱动，加局部雾体和体积光轴。

### 三件事
1. **雾接大气**：froxel 的 in-scattering 用 Step 1 的大气数据，而不是独立的雾颜色参数。这样雾色天然和天空一致——ER 远景的关键。
2. **局部雾体（Local Fog Volume）**：山谷积雾、遗迹地面浮雾。做成 Volume 组件，box/sphere 形状 + 高度衰减，注入 froxel。
3. **体积光轴**：froxel 内 raymarch 主光阴影贴图。这是 ER 特征 #3，视觉冲击极大。

### 已知坑
- **必须做时域抗噪**。froxel raymarch 的噪点在低密度雾里非常明显。用 blue noise 或 Halton 序列做每帧 jitter + temporal reprojection（历史帧混合系数 ~0.9）。
- froxel 深度分布用**指数分布**而非线性，近处精度才够。典型 `160×90×64`。
- 相位函数用 Henyey-Greenstein（`g ≈ 0.6~0.8` 做前向散射），不要用各向同性，否则光轴出不来。

### 备选方案对比

| 方案 | 论文/来源 | 优点 | 缺点 |
|---|---|---|---|
| **Froxel 体积雾**（推荐，已有基础） | Wronski, *Volumetric Fog*, SIGGRAPH 2014 Advances in RTR | 统一处理雾+光轴+局部雾体，成本可控 | 需时域抗噪 |
| Frostbite 统一体积渲染 | Hillaire, *Physically Based and Unified Volumetric Rendering in Frostbite*, SIGGRAPH 2015 | 最完整的工程方案，含体积阴影 | 实现量大 |
| 屏幕空间 God Rays（radial blur） | —— | 极便宜 | 假、只在光源在屏幕内时成立 | 
| 后处理 raymarch 光轴（不用 froxel） | 常见 URP 教程做法 | 简单 | 无法和局部雾体统一，遮挡关系差 |
| 纯 exponential height fog | —— | 几乎免费 | 无光轴、无局部变化 | 兜底降级用 |

**参考阅读**：Hillaire 2016 SIGGRAPH course notes *Volumetric Rendering in Frostbite*（比 2015 论文更实操）。

### 验收标准
- [ ] 雾颜色随时间轴自动跟随天空，无需手调
- [ ] 山谷可放置独立积雾且边界柔和
- [ ] 逆光时出现清晰光轴，且被几何正确遮挡
- [ ] 相机移动时无可见噪点爬行

---

## Step 4 — PRTGI 接时间轴

### 目标
让间接光随时间轴连续变化。这是 ER 特征 #2（天光主导）的技术承载。

### 好消息：现有实现天生支持
`Shaders/PrecomputeRadianceTransfer/BrickRelight.compute` 已经是**每帧读实时光源**：
- L237 `Light mainLight = GetMainLight();`
- L267 `Light light = GetAdditionalLight(lightIndex, surfel.position);`

也就是说 bake 阶段存的是**几何信息**（surfel 的 position/normal/albedo/天空可见性），与光照无关；runtime 用当前光源重新点亮。所以接时间轴基本是"接上就能用"：太阳一转，GI 自动跟。

### 要做的事
1. 把大气的天光（Sky-View LUT 积分出的 ambient）作为 sky contribution 喂进 relight，而不是用固定 ambient
2. 时间轴变化时提高 relight 帧预算（现有实现是多帧摊薄的，太阳快速转动时要能跟上）
3. **配 SSGI 补近景高频**——IllusionRP 文档自己推荐的组合就是「户外 PRTGI + 室内/近景 SSGI」

### 规模预算（本项目 512m 尺寸）

`ProbeRelight.compute` 最终写进单张 `RWTexture3D<float3>` coefficient voxel —— **固定网格，无 streaming/cascade**。量级估算：

| 覆盖范围 | probe 间距 | probe 数 | SH9 半精度显存（量级） | 可行性 |
|---|---|---|---|---|
| 512×512×64 m | 4 m | 128×128×16 ≈ 26 万 | ~十几 MB | ✅ 舒适 |
| 512×512×64 m | 2 m | 256×256×32 ≈ 210 万 | ~100 MB+ | ⚠️ 需 brick 稀疏化 |
| 真实 ER ~79 km² | 任意 | —— | —— | ❌ 单张 3D 纹理没戏 |

**结论：本项目用 4m 间距，不需要动 cascade。** 想把 cascade 当技术亮点再说（见下）。

### 可选加分项：Cascade / Clipmap Probe Volume
以相机为中心的多级网格（近密远疏，跟相机滚动，只 relight 附近 brick）。这是 Lumen 的 world-space radiance cache 和 Unity APV brick streaming 的做法。**工作量不小，但这是把"我用了 PRTGI"变成"我扩展了 PRTGI"的关键一步**，如果时间允许强烈建议做。

### 备选方案对比

| 方案 | 来源 | 优点 | 缺点 | 适用 |
|---|---|---|---|---|
| **PRTGI**（推荐，已有） | Sloan 2002 *Precomputed Radiance Transfer*；IllusionRP 实现 | 已有代码、天然支持动态光、成本稳定 | 只有低频、需 bake 几何、单网格 | ✅ 本项目 |
| **DDGI** | Majercik et al. 2019, *Dynamic Diffuse Global Illumination with Ray-Traced Irradiance Fields* | 全动态（几何也能动）、更现代、无 bake | 需 RT 硬件或软件 raymarch | IllusionRP 有 `RayTracing/`，值得评估 |
| Unity APV + Sky Occlusion | Unity 6 / URP 17 | 官方、有 brick streaming | 场景混合是离散状态，连续昼夜要靠 sky occlusion | 省事路线 |
| SSGI 单独用 | 已有 | 高频细节好、零 bake | 屏幕外信息缺失，大场景漏光严重 | **只能当补充** |
| 多套 Lightmap 混合 | —— | 质量最高 | 24h 连续变化烘不起 | ❌ 不适用大场景 |
| Lumen 式 Surface Cache + Radiance Cache | UE5 | 质量与动态性都最好 | 复刻工作量远超作品集周期 | ❌ |

> **注意 DDGI 这条线**：如果你的目标岗位偏前沿，且 `RayTracing/` 目录里已有可用基建，DDGI 比 PRTGI 更能体现技术前瞻性。代价是硬件门槛。建议先按 PRTGI 做通，有余力再加 DDGI 作为对比方案——**"我实现了两种 GI 并做了对比分析"本身就是极强的作品集素材**。

### 验收标准
- [ ] 拖时间轴，墙面/地面的间接光颜色连续跟随天空变化
- [ ] 黄昏时背光面呈现正确的蓝紫天光，而非死黑
- [ ] 快速拖动滑竿时 GI 无明显滞后（或滞后可接受并已在文档中说明）
- [ ] 大面积自发光地标（黄金树）能把周围环境染色（验证特征 #5）

---

## Step 5 — GPU 驱动植被

### 目标
compute culling + indirect draw，撑住 km 级视距的草木密度。

### 为什么强推
**这是简历上最硬的一条。** "GPU-driven rendering" 在招聘权重很高，且它是可量化的（给出剔除前后的 draw call / 帧时间对比表）。

### 推荐架构

```
密度图/散布规则
      ↓ (Editor 或运行时 compute 生成)
实例 Buffer（position / rotation / scale / LOD 参数）
      ↓ compute shader
Frustum Culling → HiZ Occlusion Culling → LOD 分桶
      ↓ 每个 LOD 一个 args buffer
Graphics.RenderMeshIndirect / RenderPrimitivesIndirect
```

要点：
- HiZ 用现有的 `DepthPyramidPass.cs`——**已经有了，直接复用**
- LOD 链：高模 → 低模 → billboard impostor（远处）
- 风场：一张全局风力纹理（噪声滚动）+ 顶点着色器摆动，**同一个风场同时驱动植被和 Step 9 的布料**，这种系统级一致性很能体现设计能力
- Clipmap 式实例生成：只在相机附近生成实例，远处降密度

### 备选方案对比

| 方案 | 来源 | 优点 | 缺点 |
|---|---|---|---|
| **自写 compute culling + indirect**（推荐） | Haar & Aaltonen, *GPU-Driven Rendering Pipelines*, SIGGRAPH 2015（AC Unity）；Wihlidal, *Optimizing the Graphics Pipeline with Compute*, GDC 2016 | 含金量最高、完全可控 | 实现量最大 |
| Unity GPU Resident Drawer | Unity 6 / URP 17 内置 | 几乎零成本接入 | 是"用功能"不是"做功能"，作品集价值低 |
| Unity Terrain Detail / Tree 系统 | 内置 | 最省事 | 性能差、可控性差、看不出技术 |
| `DrawMeshInstancedIndirect`（旧 API） | —— | 兼容老版本 | 已被 `RenderMeshIndirect` 取代 |
| Impostor / Octahedral Billboard | Ghost of Tsushima（Bell, GDC 2021 *Samurai Landscapes*） | 远景成本极低 | 需离线烘 impostor atlas |

**参考阅读**：*Samurai Landscapes*（Ghost of Tsushima 植被与风场，和 ER 氛围最接近的公开分享）、Horizon Zero Dawn 的 placement system。

### 验收标准
- [ ] 视野内 10 万+ 实例稳定 60fps（列出具体机型）
- [ ] 剔除前后 draw call / 帧时间对比表
- [ ] 风场变化时植被摆动自然、无穿插撕裂
- [ ] LOD 切换无可见 pop（用 dither cross-fade）

---

## Step 6 — 地形材质系统

### 目标
一套统一的地形/岩石材质，杜绝平铺感。

### 四个必备技术
1. **Height-blend 混合层**（不是 linear lerp——用 linear lerp 是新手标志）。按高度图取最大值加权，得到岩石从沙土里"露出来"的自然过渡。
2. **陡坡 Triplanar 映射**。断崖上的 UV 拉伸只能靠三平面投影解决。
3. **Detail normal 距离淡出**。近处细节法线，远处淡掉，避免高频噪点和 shimmer。
4. **Macro variation noise**。一层大尺度低频噪声乘在 albedo 上，破掉平铺规律。**这一项成本最低、效果最明显**，很多项目漏掉。

### 备选方案对比

| 方案 | 来源 | 优点 | 缺点 |
|---|---|---|---|
| **Height-blend Splatting**（推荐） | Mishkinis, *Advanced Terrain Texture Splatting* | 便宜、效果好 | 层数受 splat 通道限制 |
| Virtual Texturing | id Tech / Far Cry 系列 | 层数无限、内存可控 | 实现复杂度高一个数量级 |
| Texture Array + 索引图 | —— | 层数多 | 采样数仍随层数增长 |
| Unity Terrain 内置 splatmap | —— | 现成 | 只有 linear lerp，平铺感重 |
| 程序化材质（noise 全生成） | —— | 零贴图 | 难做到写实 |

### 验收标准
- [ ] 500m 视距内无可见平铺规律
- [ ] 断崖陡面无 UV 拉伸
- [ ] 岩石与地面过渡自然（非线性混合边界）

---

## Step 7 — 体积云（加分）

### 目标
天空占画面约 1/3，好云对氛围贡献极大。

### 推荐方案：Horizon Zero Dawn 法

- Worley-Perlin 3D 噪声做基础形状，curl noise 做边缘丝絮
- 两级 raymarch：cheap（只查密度找到云边界）→ expensive（做光照积分）
- 光照：Beer's law + powder effect（云内背光边缘变亮的关键）
- **时域升采样**：每帧只算 1/16 像素（4×4 Bayer 序列），reproject 历史帧。这是它能跑实时的核心。
- 接入 Step 1 的大气：云要被航空透视影响，否则远处云会"贴"在天上

### 备选方案对比

| 方案 | 来源 | 优点 | 缺点 |
|---|---|---|---|
| **HZD Raymarch**（推荐） | Schneider & Vos, *The Real-time Volumetric Cloudscapes of Horizon Zero Dawn*, SIGGRAPH 2015 | 质量高、有完整公开分享 | 贵，需时域升采样 |
| Nubis 3 | Schneider, SIGGRAPH 2023 | 当前最先进 | 复杂度高 |
| Häggström 硕士论文实现 | Rurik Häggström 2018 | **最好的落地实现指南**，比原论文更清晰 | —— |
| 2D 云层贴图 + 视差 | —— | 极便宜 | 无体积感、无法穿云 |
| 云 Cubemap（离线烘） | —— | 便宜 | 静态，昼夜光照不对 |

**建议**：如果周期紧，Step 7 可以降级为"高质量 2D 云层 + 正确的大气着色"，先保 Step 1-6 完成度。

---

## Step 8 — 水体（加分）

### 推荐方案
- 波形：Gerstner 波叠加（4-6 个）够用；追求真实用 FFT
- 反射：SSR（**已有 `WaterSSRDataPass`**）+ planar reflection 兜底
- 岸线：对比 scene depth 做软融合 + 泡沫带
- 折射：抓屏 + 深度衰减吸收色
- Caustics：投影贴图动画（便宜）或屏幕空间

### 备选方案对比

| 方案 | 来源 | 优点 | 缺点 |
|---|---|---|---|
| **Gerstner 波**（推荐） | 经典方法 | 便宜、可控、易做岸线响应 | 大洋细节不足 |
| FFT 海面 | Tessendorf, *Simulating Ocean Water*, 2001 | 真实感最强 | 贵、不易做局部响应 |
| Unity HDRP Water System | HDRP 源码 | 可参考完整工程实现 | 移植到 URP 工作量大 |
| Sea of Thieves 方案 | GDC 2018 分享 | 风格化 + 交互性强 | 风格不符 ER |

---

## Step 9 — 潮湿 / 天气（加分）

### 内容
- **Wetness 系统**：porosity 压暗 albedo、smoothness 提升、法线弱化
- **积水**：SDF 或高度图判定低洼处积水，积水面 smoothness 拉满
- **涟漪**：雨滴法线动画，只在积水区
- **风场统一**：Step 5 的风场同时驱动植被和布料 —— **系统级一致性是加分点**
- 接时间轴/天气 Profile，一个 `wetness` 全局参数驱动全部材质

### 要点
**wetness 必须是一个全局参数驱动所有材质**，不是每个材质手调。这又是"系统 vs 手工"的区别。

---

## Step 10 — 工具化 + Debug + 性能表（必做）

> **这一节是技术美术作品集和图形程序作品集的分界线。** 很多人做完效果就交，白白丢掉最容易拿到的分。

### 10.1 工具化
- 所有参数 Volume 化（沿用 URP Volume 框架）
- 时间轴曲线编辑器（自定义 EditorWindow）
- Preset 系统：至少 2-3 套完整氛围可一键切换
- **证明你交付的是美术能用的东西，不是只有你自己能跑的 demo**

### 10.2 Debug View（务必录进视频）
- GI only / Fog only / Albedo only / Normal / Roughness
- PRTGI probe 网格可视化 + surfel 可视化
- 植被 culling 结果可视化（剔除掉的染红）
- Froxel 切片可视化
- Overdraw / Shader complexity
- 曝光直方图（`ExposureDebugPass` 已有）

这是「我知道自己在做什么」的最强信号。

### 10.3 性能表
逐 pass 耗时表（RenderDoc 或 GPU Profiler 截图）+ 取舍说明：

| Pass | 耗时 (ms) | 分辨率 | 备注 |
|---|---|---|---|
| Atmosphere LUT | | | |
| Volumetric Fog | | 半分辨率 | 为什么降半 |
| PRTGI Relight | | | 多帧摊薄 |
| Vegetation Culling | | | 剔除率 % |
| … | | | |

**懂成本 > 会实现。** 这张表比任何截图都能说明专业度。

### 10.4 技术 Breakdown 文档
每个模块写：原理 / 参考论文 / 你做的取舍 / 踩的坑 / 性能数据。

**很多时候这份文档比 demo 本身更能决定结果。**

---

## 4. 交付物清单

- [ ] **60-90s 视频**：飞行相机路径 + 时间轴滑竿演示 + debug view 切换
- [ ] **技术 Breakdown 文档**（见 10.4）
- [ ] **Debug view 截图集**
- [ ] **性能表**
- [ ] **模块归属声明**（见 1.2）
- [ ] 可选：可交互 build 或 Editor 工程

---

## 5. 场景内容规划

| 项 | 规格 |
|---|---|
| 范围 | 512m × 512m |
| 地形 | 一片草地 + 几处岩石断崖 + 一条河谷 |
| 地标 | 一个远景巨物自发光地标（验证特征 #5） |
| 水体 | 一小片湖/河 |
| 植被 | 草 + 灌木 + 树，3 级 LOD |
| 相机 | 一条 60-90s 飞行路径，覆盖近景/中景/远景 vista |

**不要做**：角色、动画、UI、玩法、多个区域。全部是无关成本。

---

## 6. 附录：关键判断的依据

### 6.1 为什么 PRTGI 适合这里、不适合近景 diorama

| | 近景 diorama | ER 式开放世界 |
|---|---|---|
| 光照状态数 | 2 个离散态 | 连续 24h |
| 场景体积 | 十几平米 | km² |
| 能否烘 lightmap | 能（两套 ~19MB×2） | 不能（24 时刻全图烘不起） |
| 观察距离 | 贴脸，需接触阴影细节 | 中远景为主，低频足够 |
| **结论** | **lightmap 更优**，PRTGI 是质量降级 | **PRTGI 是唯一合理选择** |

关键在最后一行：PRTGI 的 SH L2 只表达低频。这在近景是致命缺陷，在开放世界恰好就是需要的——远山的间接光本来就没有高频信息。

### 6.2 PRTGI 的规模上限来源

`ProbeRelight.compute` 最终写入单张 `RWTexture3D<float3>`，是固定网格，无 streaming。所以：
- 本项目 512m / 4m 间距 → ~26 万 probe → 舒适
- 真实 ER 规模 → 必须加 cascade/clipmap（Lumen radiance cache / APV brick streaming 的做法）

### 6.3 平台注意
`ProbeRelight.compute` / `BrickRelight.compute` 首行都是：
```
#pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
```
**不含 gles3** —— 整套 PRTGI 依赖 compute shader / StructuredBuffer / RWTexture3D，WebGL2 与微信小游戏平台不可用。本作品集为 PC 平台，不受影响，但这条限制要记住。

### 6.4 版本注意
IllusionRP 依赖 **URP 17.3.0**（Unity 6 系），PRT 模块约 8785 行代码大量使用 RenderGraph 期 API。作品集工程直接建在 Unity 6 + URP 17 上即可，不要试图往老版本 URP 移植。

---

## 7. 参考文献汇总

**大气**
- Bruneton & Neyret, *Precomputed Atmospheric Scattering*, EGSR 2008
- Hillaire, *A Scalable and Production Ready Sky and Atmosphere Rendering Technique*, EGSR 2020 ← **主要参考**
- Nishita et al., *Display of the Earth Taking into Account Atmospheric Scattering*, SIGGRAPH 1993
- Preetham et al., *A Practical Analytic Model for Daylight*, SIGGRAPH 1999
- Unity HDRP `PhysicallyBasedSky` 源码

**体积渲染**
- Wronski, *Volumetric Fog*, SIGGRAPH 2014 Advances in Real-Time Rendering
- Hillaire, *Physically Based and Unified Volumetric Rendering in Frostbite*, SIGGRAPH 2015
- Hillaire, *Volumetric Rendering in Frostbite*, SIGGRAPH 2016 course notes ← 更实操

**体积云**
- Schneider & Vos, *The Real-time Volumetric Cloudscapes of Horizon Zero Dawn*, SIGGRAPH 2015
- Schneider, *Nubis* 系列（至 SIGGRAPH 2023 Nubis3）
- Häggström, *Real-time rendering of volumetric clouds*, MSc thesis 2018 ← 最好的实现指南

**GI**
- Sloan et al., *Precomputed Radiance Transfer for Real-Time Rendering*, SIGGRAPH 2002
- Majercik et al., *Dynamic Diffuse Global Illumination with Ray-Traced Irradiance Fields*, JCGT 2019（DDGI）
- Unity APV 文档 / `ProbeReferenceVolume` 源码

**GPU 驱动渲染**
- Haar & Aaltonen, *GPU-Driven Rendering Pipelines*, SIGGRAPH 2015
- Wihlidal, *Optimizing the Graphics Pipeline with Compute*, GDC 2016
- Bell, *Samurai Landscapes*（Ghost of Tsushima 植被/风场）, GDC 2021

**地形/水/PBR**
- Mishkinis, *Advanced Terrain Texture Splatting*
- Tessendorf, *Simulating Ocean Water*, 2001
- Karis, *Real Shading in Unreal Engine 4*, SIGGRAPH 2013
- Lagarde & de Rousiers, *Moving Frostbite to PBR*, SIGGRAPH 2014
