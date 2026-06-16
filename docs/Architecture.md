# Project-ARPG 架构蓝图

> 一份用于对外讲解项目设计的总览文档。
> 项目目标:以《艾尔登法环》为参考,使用 Unity 6 复刻一套支持联机的类魂 ARPG 框架。
>
> Unity 版本:`6000.2.8f1` ・ 渲染管线:`URP 17.2` ・ 命名空间:`LZ`

---

## 1. 技术栈一览

| 类别 | 选型 | 备注 |
|------|------|------|
| 渲染管线 | URP 17.2 + 自定义 RenderFeature | 体积雾、PerObjectShadow、Cluster Light |
| 着色 | Amplify Shader Editor + 自研 RoleShader 工具链 | 角色卡通渲染、面部贴图编辑器 |
| 角色物理 | Kinematic Character Controller (KCC) | 自写 `KCCCharacterController` 作为桥接层 |
| 动画 | Animancer Pro + 自研 Clip 注册表 | 多 Layer(Base/Upperbody/Action/PingDamage) |
| AI 寻路 | Unity NavMesh + 自研状态机 SO | 状态以 ScriptableObject 表达 |
| 网络 | Unity Netcode for GameObjects 2.6 + FacePunch + Steamworks | Owner-authoritative |
| 输入 | Unity Input System(`PlayerControls.inputactions`) | 自动生成 + Manager 中转 |
| 存档 | JSON + `Application.persistentDataPath` | 自研 `SaveFileDataWriter` |
| 数据驱动 | ScriptableObject | Item / Action / State / Animation / Effect |
| 第三方 | Houdini Engine、ProBuilder、Recorder、ParrelSync(多开调试)、TextMeshPro | — |

---

## 2. 顶层架构

```
┌──────────────────────────────────────────────────────────────────────┐
│                       Bootstrap (DontDestroyOnLoad)                  │
│                                                                      │
│   WorldSaveGameManager   WorldSceneManager   WorldSubsceneManager    │
│   WorldGameSessionMgr    WorldAIManager      WorldObjectManager      │
│   WorldUtilityManager    WorldCharacterFX    WorldSoundFXManager     │
│   WorldItemDatabase      WorldActionManager                          │
│                                                                      │
│   PlayerUIManager        PlayerInputManager      PlayerCamera        │
└──────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌──────────────────────────────────────────────────────────────────────┐
│                            Gameplay Layer                            │
│                                                                      │
│   CharacterManager (基类) ──┬── PlayerManager                        │
│                             └── AICharacterManager ── AIBossChar...  │
│                                                                      │
│   Subsystems (挂在同一 GameObject 上,由 CharacterManager 聚合):     │
│     • Locomotion ─ KCCCharacterController(ICharacterController)      │
│     • Combat / Stats / Animator / Effects / SoundFX / Network / UI   │
│                                                                      │
│   Interactables ・ DamageColliders ・ Weapon Actions ・ Spells       │
│   Breakable Objects ・ Event Triggers                                │
└──────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌──────────────────────────────────────────────────────────────────────┐
│                  Data Layer (ScriptableObjects + Save)               │
│                                                                      │
│   Items: WeaponItem / EquipmentItem / SpellItem / FlaskItem ...      │
│   Animation: AnimationClipRegistry / CharacterAnimationData /        │
│              WeaponAnimationSet                                      │
│   AI: AIState / AICharacterAttackAction                              │
│   Combat: WeaponItemAction / AshOfWar / *Effect (Instant / Static)   │
│   Save: CharacterSaveData (+ SerializableWeapon/Flask/Projectile)    │
└──────────────────────────────────────────────────────────────────────┘
```

### 2.1 设计原则

1. **组合优于继承(Composition over Inheritance)**:每个角色 = `CharacterManager` + 一组职责单一的 `*Manager` 组件。
2. **数据驱动(Data-Driven)**:武器、动画、AI 状态、招式、伤害效果全部以 ScriptableObject 表达,程序只搬运数据。
3. **单例 + 服务定位**:跨场景常驻的"世界级"系统统一用 `public static instance + DontDestroyOnLoad`,作为服务入口。
4. **Owner-Authoritative 网络模型**:`NetworkVariable<T>` 默认 `WritePermission.Owner`,客户端修改本地状态 → 自动广播 → 远端 `OnValueChanged` 响应。
5. **关注点分离**:输入 / 移动 / 动画 / 网络 / 存档 互不直接调用,通过 Manager 字段或 NetworkVariable 解耦。

---

## 3. 目录结构

```
Assets/
├── Data/                      # ScriptableObject 数据资产
│   ├── AI Attack Actions/     # AI 招式
│   ├── AI States/             # AI 状态机节点
│   ├── AnimationData/         # CharacterAnimationData (按角色一份)
│   ├── Animator Controllers/  # 旧 AnimatorController 资产
│   ├── Dialogues/             # 对话 SO
│   ├── Effects/               # 伤害/Buff 效果 SO
│   ├── Items/                 # 武器、装备、法术等
│   ├── Weapon Actions/        # 按键动作 SO
│   ├── WeaponAnimationSet/    # 武器动画集 SO
│   └── Animation Clip Registry.asset  # 全局动画 ID 注册表
│
├── Scripts/
│   ├── Runtime/
│   │   ├── Character/
│   │   │   ├── CharacterManager.cs           # 基类
│   │   │   ├── Character*Manager.cs          # 共通子系统
│   │   │   ├── KCCCharacterController.cs     # KCC 桥接
│   │   │   ├── Player/                       # 玩家专属
│   │   │   │   ├── PlayerManager.cs
│   │   │   │   ├── Player UI/                # UI 子模块
│   │   │   │   └── ...
│   │   │   └── AI Character/                 # 敌人专属
│   │   │       ├── AICharacterManager.cs
│   │   │       ├── AIBossCharacterManager.cs
│   │   │       ├── States/                   # 状态机节点
│   │   │       ├── Actions/                  # AI 招式基类
│   │   │       └── <角色名>/                  # Durk / Knight / Undead
│   │   ├── Animation/                        # Registry + AnimationData
│   │   ├── Animator/                         # StateMachineBehaviour
│   │   ├── Breakable Objects/
│   │   ├── Colliders/                        # DamageCollider 家族
│   │   ├── Effects/                          # InstantEffect / StaticEffect
│   │   ├── Event Trigger/
│   │   ├── Game Saving/                      # SaveData + Serializable*
│   │   ├── Interaction/                      # Interactable 家族
│   │   ├── Items/                            # ScriptableObject 物品
│   │   ├── Menu Scene/                       # 标题界面
│   │   ├── Render/                           # WaterSystem 等
│   │   ├── UI/                               # 通用 UI 控件
│   │   ├── Utility/                          # 枚举、Spawner、工具
│   │   ├── Weapon Actions/                   # 按键动作实现
│   │   └── World Managers/                   # 世界级单例
│   ├── Editor/                               # 编辑器工具
│   ├── ClusterLight/                         # 集群光照
│   ├── RenderFeature/                        # URP RenderFeature
│   └── Other/                                # Shader 杂项脚本
│
├── Plugins/                   # 第三方:KCC / Steamworks / FacePunch ...
├── Resources/ ・ Settings/ ・ Scenes/ ・ Prefabs/ ・ Art/ ・ Addons/
└── PlayerControls.inputactions(+ .cs)        # Input System 自动生成
```

---

## 4. 角色系统(Character)

### 4.1 类继承关系

```
NetworkBehaviour
└── CharacterManager                 ← 基类:聚合通用 *Manager + 网络位置同步
    ├── PlayerManager                ← 玩家:加挂 Player 专属 Manager + 存档接口
    └── AICharacterManager           ← AI:加挂 NavMeshAgent + 状态机
        └── AIBossCharacterManager   ← Boss:加挂 bossID / bossFightIsActive
```

### 4.2 子系统组件清单(挂在同一 GameObject 上)

| 通用 (Character*) | Player 专属 | AI 专属 |
|---|---|---|
| `CharacterNetworkManager`(网络变量 + RPC) | `PlayerNetworkManager` | `AICharacterNetworkManager` |
| `CharacterLocomotionManager` | `PlayerLocomotionManager` | `AICharacterLocomotionManager` |
| `CharacterCombatManager` | `PlayerCombatManager` | `AICharacterCombatManager` |
| `CharacterAnimatorManager` | `PlayerAnimatorManager` | `AICharacterAnimatorManager` |
| `CharacterStatsManager` | `PlayerStatsManager` | — |
| `CharacterEffectsManager` | `PlayerEffectsManager` | — |
| `CharacterSoundFXManager` | `PlayerSoundFXManager` | `AICharacterSoundFXManager` |
| `CharacterUIManager`(浮动血条) | — | — |
| `CharacterInventoryManager`(空基类) | `PlayerInventoryManager` | `AICharacterInventoryManager` |
| `CharacterEquipmentManager`(空基类) | `PlayerEquipmentManager` | — |
| — | `PlayerInteractionManager` | — |
| — | `PlayerBodyManager`(性别/发型/捏脸) | — |
| 物理 | `KinematicCharacterMotor` + `KCCCharacterController` | + `NavMeshAgent` |

`CharacterManager.Awake()` 会 `GetComponent` 把所有子 Manager 缓存为公共字段,任何系统都能通过 `character.characterXxxManager.foo` 链式访问。

### 4.3 网络同步策略

`CharacterNetworkManager` 集中持有所有跨网络字段(全部 Owner-write,Everyone-read):

- **位移**:`networkPosition / networkRotation`(Owner 每帧写入,远端 SmoothDamp/Slerp 插值)。
- **状态机**:`isMoving / isBlocking / isParrying / isAttacking / isLockedOn / isInvulnerable / isJumping / isChargingAttack / isRipostable / isBeingCriticallyDamaged / isRolling` 等十余个布尔标志。
- **属性**:`vigor / endurance / mind / strength / dexterity / intelligence / faith` + 当前/上限的 `health / stamina / focusPoints`。
- **动画**:通过 `ServerRpc → ClientRpc` 链路广播 clip ID,详见 §6。
- **伤害**:由攻击方 `NotifyTheServerOfCharacterDamageServerRpc` 上抛 → Server `ClientRpc` 广播 → 各端实例化 `TakeDamageEffect` SO 本地处理。

### 4.4 物理移动(KCC 桥接)

```
PlayerInputManager.movementInput
        │
        ▼
PlayerLocomotionManager.HandleAllMovement()
        │  • CalculateGroundedMovement / CalculateAirMovement
        │  • 写入 KCC 桥接字段:
        │     moveVelocity, targetRotation, jumpRequested, rootMotionDelta
        ▼
KCCCharacterController : ICharacterController
        │  • UpdateVelocity(): 接地/空中分支,重力,跳跃,Root Motion
        │  • UpdateRotation(): 平滑朝向 or Root Motion
        │  • IsColliderValidForCollisions(): 过滤自身 collider
        │  • OnMovementHit(): 角色互推
        ▼
KinematicCharacterMotor (KCC 内核)
        ▼
Transform 实际位置
```

要点:
- **Root Motion 通过 KCC 走**:`CharacterAnimatorManager.OnAnimatorMove()` 把 `animator.deltaPosition/deltaRotation` 写入 `kcc.rootMotionDelta`,KCC 在 `UpdateVelocity` 中转为速度,保证 collider 推进、坡道贴合、防穿墙。
- **远端角色**:`CharacterManager.Update()` 用 `motor.SetPositionAndRotation` 把网络位置同步给 KCC,避免远端走自己的物理。

---

## 5. AI 系统

### 5.1 状态机:ScriptableObject 实例化模式

```
AIState (abstract ScriptableObject)
 ├── IdleState         (Idle / Patrol / Sleep 三种 IdleStateMode)
 ├── PursueTargetState (导航至目标)
 ├── CombatStanceState (选择招式 / 走位 / 格挡 / 闪避)
 ├── AttackState       (执行招式 + Combo)
 ├── InvestigateSoundState
 └── BossSleepState
```

`AICharacterManager.OnNetworkSpawn()` 把每个 SO **`Instantiate` 出独立运行时副本**,避免多怪共享同一状态实例。

每帧 `Update → ProcessStateMachine()`:

```csharp
AIState next = currentState?.Tick(this);
if (next != null) currentState = next;
```

`AIState.SwitchState()` 在切换时 `ResetStateFlags()`,保证回到此状态时不带脏数据。

### 5.2 招式 / 攻击决策

- `AICharacterAttackAction` 是单个招式的 SO(动画 clip + recovery 时间 + connect range + comboAction 等)。
- `CombatStanceState` 持有 `aiCharacterAttacks` 列表 + `potentialAttacks` 临时筛选池;每个 Combat Rotation 还会**掷骰**决定是否格挡 / 闪避 / 绕步。
- `AttackState.PerformAttack()` 调用 `currentAttack.AttemptToPerformAction(aiCharacter)`,内部播放动画并启动 `actionRecoveryTimer`。

### 5.3 目标搜索

`AICharacterCombatManager.FindATargetViaLineOfSight()`:
1. `Physics.OverlapSphere`,Mask 由 `WorldUtilityManager.GetCharacterLayers()` 提供。
2. 过滤死亡/自己,通过 `CanIDamageThisTarget(CharacterGroup, CharacterGroup)` 判断阵营。
3. FOV 角度检查 + `Physics.Linecast` 环境遮挡。
4. 命中 → `SetTarget()` + `PivotTowardsTarget()`。

### 5.4 远距激活与性能优化

- `AIActivationBeacon` 是一个挂在场景中的"闹钟":当玩家远离 → AI 在原位生成 beacon 并禁用自己;玩家靠近 beacon → 重新激活 AI。
- `WorldAIManager.spawnedInCharacters` 维护活跃 AI 列表,提供批量 Reset / Despawn / DisableBossFights。

---

## 6. 动画系统

### 6.1 多层架构(Animancer Layers)

| Layer | 编号 | 用途 | 备注 |
|-------|----|------|------|
| Base | 0 | Locomotion(空闲/移动 BlendTree) | `MixerTransition2D`(Directional) |
| Upperbody | 1 | 上半身动作(换武器/喝药/拉弓) | `AvatarMask = upperbodyMask` |
| Action | 2 | 全身动作(攻击/翻滚/受击) | 无 Mask,优先级高 |
| PingDamage | 3 | 轻量受击反馈(头胸抖动) | `AvatarMask = pingDamageMask` |

`CharacterAnimatorManager.ActivateLayerAndPlay()` 处理 Layer 激活与淡入,避免空层 T-pose 闪帧。

### 6.2 数据资产

```
CharacterAnimationData (SO,每种角色一份)
 ├── 通用 Clip(跳跃链 / 翻滚 / 受击 / 死亡 / 闪避 / 转身 / 招架 / 暴击受击 / 喝药 ...)
 ├── Locomotion Mixer(1H / 2H / Blocking1H / Blocking2H)
 └── AvatarMask 引用

WeaponAnimationSet (SO,每把武器一份)
 ├── 单手 / 双手 / 双持 三组攻击 Clip
 ├── 蓄力链 Clip(Hold / Release / FullRelease)
 └── locomotionClipOverrides[](装备此武器时覆盖角色 Locomotion BlendTree 内的 clip)
```

### 6.3 网络同步的 Clip ID 映射

```
AnimationClipRegistry (SO,全局唯一)
 ├── List<AnimationClip> registeredClips     ← 数组下标即网络传输 ID
 ├── _idToClip / _clipToId / _nameToId       ← 运行时三向字典
```

发送端:`PlayActionAnimation → NotifyTheServerOfActionAnimationServerRpc(animationID)`(字符串/ID)
接收端:`ClientRpc → LookupClipByName → animancer.Play(clip)`

> 关键约束:**所有客户端必须引用同一份 Registry 资产**,顺序固定。这套机制取代了传统 `Animator.Play("State Name")` 的全字符串方案,但仍保留 string lookup 作为兼容层。

---

## 7. 战斗系统

### 7.1 数据流(玩家轻攻击为例)

```
PlayerInputManager
  └─ playerControls.PlayerActions.RB.performed → RB_Input = true
       │
       ▼
PlayerCombatManager (Update / HandleAllInputs)
  └─ PerformWeaponBasedAction(weapon.oh_RB_Action, weapon)
       │
       ▼
LightAttackWeaponItemAction.AttemptToPerformAction()  ← ScriptableObject 招式
  • 检查体力 / 状态 / 是否在地 / 是否冲刺/翻滚/后撤
  • AttemptCriticalAttack() ← 检测背刺/处决
  • PerformLightAttack → PlayTargetAttackActionAnimation
       │
       ▼
PlayerAnimatorManager
  • 播放本地动画(Animancer)
  • 写入 NetworkVariable(isAttacking 等)
  • NotifyTheServerOfAttackActionAnimationServerRpc(clipName) ← 网络广播
       │
       ▼
攻击动画播到关键帧 (Animation Event)
  └─ EnableDamageCollider() ← MeleeWeaponDamageCollider 启用 trigger
       │
       ▼
DamageCollider.OnTriggerEnter()
  • 找到目标 CharacterManager
  • CheckForBlock / CheckForParry
  • DamageTarget()  ← 实例化 TakeDamageEffect SO,填入伤害数据
       │
       ▼
CharacterEffectsManager.ProcessInstantEffect(effect)
  └─ effect.ProcessEffect(character)
       │
       ▼
TakeDamageEffect.ProcessEffect()
  • CalculateDamage()        ← 扣 currentHealth (NetworkVariable,自动广播)
  • PlayDirectionalBasedDamageAnimation()  ← 根据 angleHitFrom 选 ping/medium 受击
  • PlayDamageVFX / PlayDamageSFX
  • CalculateStanceDamage()  ← AI 韧性扣减
       │
       ▼
currentHealth.OnValueChanged → CharacterNetworkManager.OnHpChanged()
  • <=0 ? 调用 character.ProcessDeathEvent() 协程
```

### 7.2 关键概念

- **WeaponItemAction** 是按键 → 动作 的中间桥(SO),允许设计师把"右肩按键 = 轻攻击 / 重攻击 / 法术 / 弓"等组合自由配置在每把武器上。
- **AshOfWar(战技)** 复用 `WeaponItemAction` 体系:武器持有 `ashOfWarAction` 字段,触发后接管动作。
- **DamageCollider** 家族:
  - `MeleeWeaponDamageCollider`(近战)
  - `RangedProjectileDamageCollider`(箭/弩)
  - `SpellProjectileDamageCollider` / `FireBallDamageCollider`(法术)
  - `ManualDamageCollider`(关卡陷阱)
  - `DurkClubDamageCollider` / `DurkStompCollider`(特定 Boss 招式)
- **Effect** 体系:
  - `InstantCharacterEffect`(SO):一次性,如 `TakeDamageEffect / TakeBlockedDamageEffect / TakeCriticalDamageEffect / TakeStaminaDamageEffect / TwoHandingEffect`。
  - `StaticCharacterEffect`(SO):持续 Buff/Debuff,可 `AddStaticEffect / RemoveStaticEffect(id)`。

### 7.3 暴击(背刺 / 处决)

`CharacterCombatManager.AttemptCriticalAttack()`:
1. 向角色正前方 `RaycastAll`(距离 `criticalAttackDistanceCheck`)。
2. 对每个命中体计算 `targetViewableAngle`,在 ±60° 内且目标 `isRipostable.Value=true` → `AttemptRiposte`。
3. 在 ±145°~180° → `AttemptBackstab`。
4. 双方位置由 `ForceMoveEnemyCharacterToRipostePosition / BackstabPosition` 协程对位(0.2s),位置偏移由 `WorldUtilityManager.GetRiposting/BackstabPositionBasedOnWeaponClass(weaponClass)` 给出。

---

## 8. 物品 / 装备系统

```
ScriptableObject Item (基类:itemID, itemName, itemIcon, maxItemAmount, itemDescription)
 ├── EquipmentItem
 │    ├── WeaponItem            ← 伤害 / 蓄力修饰 / 体力消耗 / 格挡吸收 / 三种动作槽 + AshOfWar
 │    │    ├── MeleeWeaponItem
 │    │    ├── RangedWeaponItem
 │    │    └── CasterWeaponItem
 │    ├── HeadEquipmentItem (FullHelmet / Hat / Hood / FaceCover)
 │    ├── BodyEquipmentItem
 │    ├── HandEquipmentItem
 │    ├── LegEquipmentItem
 │    └── RangedProjectileItem
 ├── SpellItem (Sorcery / Incantation)
 ├── QuickSlotItem
 │    └── FlaskItem (HP / FP)
 ├── AshOfWar
 │    └── ParryAshOfWar
 └── UpgradeMaterial
```

`WorldItemDatabase`(单例,挂在世界场景常驻 GO 上)在 `Awake()` 把所有 SO 物品按顺序压入 `items` 列表并 **重写 `itemID = i`**。所有查找接口形如 `GetWeaponByID(int)`,这是网络同步、存档反序列化的统一入口。

### 8.1 装备模型实例化

- `WeaponModelInstantiationSlot`(MonoBehaviour)挂在角色骨骼挂点(右手/左手/盾位/背后)上,标记 `WeaponModelSlot` 枚举。
- `PlayerEquipmentManager` 监听 `currentRightHandWeaponID.OnValueChanged`,通过 `WorldItemDatabase.GetWeaponByID` 找到 `WeaponItem`,实例化 `weaponModel` 到对应槽位,并把 `DamageCollider` 引用挂到 `WeaponManager` 上。

---

## 9. 存档系统

```
WorldSaveGameManager (单例 + DontDestroyOnLoad)
 │
 ├── characterSlot01..10 : CharacterSaveData     ← 启动时全部 LoadAllCharacterProfiles
 ├── currentCharacterData : CharacterSaveData    ← 当前进行中的存档
 │
 ├── AttemptToCreateNewGame() → 寻找空 Slot → NewGame() → SaveGame()
 ├── LoadGame()   → 读 JSON → PlayerManager.LoadGameDataFromCurrentCharacterData
 ├── SaveGame()   → PlayerManager.SaveGameDataToCurrentCharacterData → JSON
 └── DeleteGame(slot)

SaveFileDataWriter
 • saveDataDirectoryPath = Application.persistentDataPath
 • 序列化 / 反序列化 JSON 文件
```

### 9.1 SO 引用的可序列化包装

由于 JSON 不能直接序列化 `WeaponItem`(SO 是 Unity Object 引用),项目采用 **"只存 itemID,加载时通过 Database 反查"** 模式:

| 包装类 | 字段 |
|--------|------|
| `SerializableWeapon` | itemID, upgradeLevel, ashOfWarID |
| `SerializableQuickSlotItem` | itemID, itemAmount |
| `SerializableFlask` | itemID |
| `SerializableRangedProjectile` | itemID, itemAmount |
| `SerializableDictionary<TKey,TValue>` | 自研可序列化字典(JsonUtility 不支持原生 Dictionary) |

`CharacterSaveData` 内字段示例(节选):

```csharp
public int sceneIndex;
public float xPosition, yPosition, zPosition;
public int currentHealth, vitality, endurance, mind, ...;
public SerializableWeapon rightWeapon01, leftWeapon01;
public List<SerializableWeapon> weaponsInInventory;
public SerializableDictionary<int, bool> sitesOfGrace;      // 已激活赐福
public SerializableDictionary<int, bool> bossesDefeated;    // 已击败 Boss
public SerializableDictionary<int, bool> worldItemsLooted;  // 已拾取物品
public int namelessKnightStageID;                           // NPC 对话进度
```

---

## 10. 场景 / 关卡管理

### 10.1 主世界 + Additive Subscene

```
WorldSceneManager (NetworkBehaviour,常驻)
 ├── LoadWorldScene(buildIndex)                ← Single 模式加载主世界
 ├── LoadAdditiveScenes(List<string>)          ← 服务器排队加载子区域
 ├── UnloadAdditiveScenes(List<string>)        ← 同上 卸载
 ├── doNotUnloadList                           ← 远端玩家所在区域不能卸
 └── CheckForUnRequiredScenes()                ← 周期性卸载多余子场景

WorldSubsceneManager
 └── GenerateDoNotUnloadListBasedOnPlayerLocations()  ← 多人时基于所有玩家位置
```

### 10.2 流送触发

`EventTriggerLoadScene`(挂在场景边界 trigger)在玩家进入时调用 `LoadAdditiveScenes()`,实现无缝大世界。

### 10.3 Boss 战触发

`EventTriggerBossFight` 进入时唤醒近邻 Boss(`AIBossCharacterManager.bossFightIsActive`),并通过 `WorldAIManager.DisableAllBossFights` 在死亡复活时统一重置。

---

## 11. 网络与会话

### 11.1 网络拓扑

```
Steam(社交层)
  │ Steamworks.NET / FacePunch
  ▼
Lobby(配对)
  │ FacepunchTransport
  ▼
Unity Netcode for GameObjects (Host-Authoritative)
  │
  ├── NetworkManager.Singleton
  ├── NetworkObject + NetworkBehaviour
  └── NetworkVariable<T>(Owner write 模式)
```

### 11.2 关键流程

1. **建房**:`WorldGameSessionManager.StartGameAsHost()` → `NetworkManager.StartHost()` + 可选 `SteamMatchmaking.CreateLobbyAsync`。
2. **加房**:`OnGameLobbyJoinRequested` → 保存当前进度 → `NetworkManager.Shutdown` → `lobby.Join` → `OnLobbyEntered` → `StartGameAsClient(hostSteamId)`。
3. **进入世界**:Host 切换出主菜单后 `SetJoinable(true)` 打开大厅。
4. **角色同步**:玩家进入后 `OnClientConnectedCallback → AddPlayerToActivePlayersList`;新 client 对其它已在线玩家调用 `LoadOtherPlayerCharacterWhenJoiningServer()` 拉取装备/外观。
5. **死亡复活**:Host 死亡 → `WorldGameSessionManager.WaitThenReviveHost` 协程 → 5s 后 `ReviveCharacter` + `ResetAllCharacters` + 传送回最近赐福。

### 11.3 RPC 模式

项目用一套**对称的 ServerRpc → ClientRpc** 模板广播动作:

```csharp
[ServerRpc] NotifyTheServerOfActionAnimationServerRpc(clientID, animID, applyRootMotion)
  └─ [ClientRpc] PlayActionAnimationForAllClientsClientRpc(...)
       └─ 跳过发起者 LocalClientId,其余客户端本地播放
```

同样模式用于:动作动画、瞬时动作、攻击动作、上半身动画、Ping 受击动画、伤害、招架、背刺、处决、销毁特效。

---

## 12. UI 系统

### 12.1 层级

```
PlayerUIManager(单例,DontDestroyOnLoad)
 ├── PlayerUIHudManager           HUD(HP/FP/Stamina/快捷物品/Boss 血条)
 ├── PlayerUIPopUpManager         弹窗(你已死亡/物品拾取/对话提示)
 ├── PlayerUICharacterMenuManager 大菜单容器(Esc 键打开)
 ├── PlayerUIEquipmentManager     装备菜单(43k 字,栏位最大)
 ├── PlayerUISiteOfGraceManager   赐福菜单
 ├── PlayerUITeleportLocationManager 传送
 ├── PlayerUILoadingScreenManager 过场 Loading
 ├── PlayerUILevelUpManager       升级面板
 └── PlayerUIWeaponUpgradeManager 武器强化(铁匠)
```

### 12.2 数据绑定

UI 监听 `PlayerNetworkManager` 的 `OnValueChanged` 实时更新:

```csharp
playerNetworkManager.currentHealth.OnValueChanged += PlayerUIManager.instance.playerUIHudManager.SetNewHealthValue;
playerNetworkManager.currentStamina.OnValueChanged += PlayerUIManager.instance.playerUIHudManager.SetNewStaminaValue;
playerNetworkManager.vigor.OnValueChanged += playerNetworkManager.SetNewMaxHealthValue;
```

这种方式让 UI 与数据完全解耦,且天然支持网络同步。

### 12.3 主菜单

`TitleScreenManager`(单例) + `UI_Character_Save_Slot`(10 个槽位的 UI 项)负责主菜单 → 角色选择 → 新建/读取/删除流程。

---

## 13. 输入系统

```
PlayerControls.inputactions(Unity Input System 资产)
  └─ 自动生成 PlayerControls.cs(91k 行)

PlayerInputManager(单例)
 ├── 缓存 Vector2/bool 字段(movementInput, RB_Input, RT_Input, dodgeInput, ...)
 ├── OnSceneChange(): 进入主菜单时禁用控制
 ├── 排队输入(que_RB_Input / que_RT_Input + que_Input_Timer):
 │     用于动作后摇阶段提前缓存玩家意图
 └── Update():
     • HandleAllInputs()
     • 把 movementInput → moveAmount/horizontal/vertical
     • 把按键标志转发给 PlayerCombatManager / PlayerLocomotionManager
```

---

## 14. 渲染 / 着色器

### 14.1 自定义 RenderFeature

| RenderFeature | 用途 |
|---|---|
| `VolumetricFogFeature` / `VolumetricFogRenderFeature` | 体积雾 |
| `PerObjectShadow/` | 单物体附加阴影(常用于过场/Boss) |
| `RenderFeatureBlit/` | 通用 Blit 工具(角色渲染需要的临时 RT) |
| `ShaderIDs.cs` | 全局 Shader 属性 ID 缓存(40k 行,覆盖所有自研 Shader 属性) |

### 14.2 ClusterLight 集群光照

`Assets/Scripts/ClusterLight/Script_ClusterBasedLighting.cs`:把场景中所有动态点光源按 Cluster 切分,在 Compute Shader 内查找,降低高密度光源的 fragment 成本。

### 14.3 角色 Shader 工具链

`Assets/Scripts/Other/`:

- `RoleShaderManager` / `RoleShaderFunctions`:角色卡通渲染的统一入口,管理 SDF 面部、轮廓光、子表面散射等参数。
- `ShaderUtils` / `ShaderFunctions`:通用 Shader 工具方法(纹理打包、贴图烘焙)。
- `ShaderLightSetting`:把场景光源参数推送给 Shader 全局变量。

---

## 15. 交互(Interactable)

```
Interactable (NetworkBehaviour 基类)
 ├── PickUpItemInteractable     拾取物
 ├── PickUpRunesInteractable    拾取卢恩(死亡点)
 ├── DialogueInteractable       NPC 对话
 ├── SiteOfGraceInteractable    赐福
 ├── AnvilInteractable          铁砧(武器强化)
 └── FogWallInteractable        Boss 雾门
```

`Interactable.OnTriggerEnter` 把自己加入 `PlayerInteractionManager.interactables` 列表,玩家按交互键时 UI 弹窗 + `Interact()` 派发。

---

## 16. 跨场景常驻系统清单

| 单例 | 职责 |
|------|------|
| `WorldSaveGameManager` | 存档读写、角色槽位、对话进度 |
| `WorldSceneManager` | 主世界 + 附加子场景管理 |
| `WorldSubsceneManager` | 计算 do-not-unload 列表(多玩家流送) |
| `WorldGameSessionManager` | Steam Lobby、Host/Client、复活协程、玩家列表 |
| `WorldAIManager` | AI Spawner、巡逻路径、Boss 注册表、激活 Beacon |
| `WorldObjectManager` | 赐福 / 雾门 / 关卡物体注册表 |
| `WorldUtilityManager` | LayerMask 查询、阵营友伤判定、伤害强度分级 |
| `WorldItemDatabase` | 全局物品/武器/装备/法术 ID 注册表 |
| `WorldCharacterEffectsManager` | 共享 VFX(血溅/暴击血溅)、Damage Effect 原型 |
| `WorldSoundFXManager` | 共享 SFX(物理伤害/UI/环境音)、随机选择工具 |
| `WorldActionManager` | 全局动作/动画事件分发 |
| `PlayerUIManager` | UI 入口、本地玩家引用 |
| `PlayerInputManager` | 输入缓存与中转 |
| `PlayerCamera` | 第三人称相机 + 锁定 + 瞄准 |

---

## 17. 编辑器工具

`Assets/Scripts/Editor/Tools/`:

- `CharacterLocomotionDebugger`(20k):运行时可视化角色当前位移意图 / 网络位置 / Root Motion / KCC 状态。
- `MaterialPassManager` + `MaterialPassManagerEditor`:批量切换材质 Pass(常用于角色渲染调试)。
- `AverageNormal`:烘焙平均法线到顶点色,卡通描边专用。
- `Editor/Animation/`:动画 Clip 注册表的批量编辑工具。
- `Editor/ShaderGUI/`:自研 Shader 的 Inspector 美化。

---

## 18. 典型数据流速查

### 18.1 玩家造成伤害的完整链路

```
Input ─▶ PlayerInputManager ─▶ PlayerCombatManager
       ─▶ WeaponItemAction(SO) ─▶ PlayerAnimatorManager.Play...
       ─▶ NetworkRPC(广播动画) + NetworkVariable(isAttacking)
       ─▶ 动画事件 EnableDamageCollider
       ─▶ DamageCollider.OnTriggerEnter
       ─▶ Instantiate TakeDamageEffect(SO,填入伤害数据)
       ─▶ targetCharacter.characterEffectsManager.ProcessInstantEffect
       ─▶ Effect.ProcessEffect:扣血(NetworkVariable)/播放受击动画/VFX/SFX
       ─▶ currentHealth.OnValueChanged ─▶ UI 更新 + 死亡判定
```

### 18.2 加载存档进入世界

```
TitleScreenManager.LoadSlot
  ─▶ WorldSaveGameManager.currentCharacterSlot = X
  ─▶ WorldSaveGameManager.LoadGame()
       ─▶ SaveFileDataWriter.LoadSaveFile() → currentCharacterData
       ─▶ WorldSceneManager.LoadWorldScene(worldSceneIndex)
            ─▶ NetworkManager.SceneManager.LoadScene(Single)
            ─▶ PlayerManager.LoadGameDataFromCurrentCharacterData
                  ─▶ 写所有 NetworkVariable / 实例化武器 / 装备/外观
                  ─▶ 触发 OnValueChanged 链 → UI 同步
```

### 18.3 AI 从待机到攻击

```
IdleState.Tick
  └─ FindATargetViaLineOfSight → 找到玩家 → SwitchState(PursueTargetState)

PursueTargetState.Tick
  └─ navMeshAgent.SetDestination(target.position)
     距离 < maximumEngagementDistance → SwitchState(CombatStanceState)

CombatStanceState.Tick
  ├─ RotateTowardsAgent
  ├─ 掷骰 块/闪避决策
  ├─ 从 aiCharacterAttacks 筛选 potentialAttacks(基于角度/距离)
  ├─ 选定 chosenAttack → AttackState.currentAttack = chosenAttack
  └─ SwitchState(AttackState)

AttackState.Tick
  ├─ PerformAttack → currentAttack.AttemptToPerformAction
  ├─ 动画完成 + actionRecoveryTimer 归零
  └─ SwitchState(CombatStanceState)
```

---

## 19. 阅读路线建议

如果你是第一次接触本项目的开发者,建议按以下顺序读:

1. **入门**:`CharacterManager` → `PlayerManager` → `AICharacterManager`(理解组合模式)。
2. **移动**:`PlayerLocomotionManager` → `KCCCharacterController`(理解 KCC 桥接)。
3. **战斗**:`LightAttackWeaponItemAction` → `MeleeWeaponDamageCollider` → `TakeDamageEffect`(理解攻击链路)。
4. **AI**:`AIState` → `IdleState` → `CombatStanceState` → `AttackState`(理解状态机)。
5. **动画**:`CharacterAnimatorManager` → `CharacterAnimationData` → `AnimationClipRegistry`(理解多层 + 网络同步)。
6. **网络**:`CharacterNetworkManager` → `PlayerNetworkManager`(理解 NetworkVariable 与 RPC)。
7. **存档**:`CharacterSaveData` → `WorldSaveGameManager`(理解序列化包装)。
8. **场景**:`WorldSceneManager` → `WorldSubsceneManager` → `EventTriggerLoadScene`(理解流送)。
9. **会话**:`WorldGameSessionManager`(理解 Steam Lobby + Netcode 集成)。

---

## 20. 待办与潜在改进

> 以下是从代码 TODO 和明显技术债务中提炼出来,可作为后续迭代方向。

- `WorldSaveGameManager` 的 10 个角色槽位代码大量重复,可重构为数组循环。
- `WorldAIManager` 注释中提到的"超过 30 个 AI 改用 Reset 而非 Despawn"未实现。
- `WorldAIManager.DisableAllCharacters` 标注 TODO,远距 AI 内存优化未完成。
- `CharacterNetworkManager` 的 NetworkVariable 数量较大(~30+),后续可考虑拆分到子组件减少同步带宽。
- `PlayerEquipmentManager`(54k 行)接近巨型类,可按装备类型(头/身/手/腿/武器/弹药/法术)拆分。
- 动画 Clip 注册表的 ID 分配依赖编辑器顺序,缺少自动迁移工具,建议加 GUID 兜底。
- `WorldSceneManager.LoadAdditiveScene` 中"加载敌人/可破坏物"逻辑被注释,目前完全依赖 Spawner。

---

> 文档维护者:架构层每次大改后请同步 §2/§4/§5/§11 的对应章节。
