# TAE 动画事件系统 使用文档

## 概述

本系统将 Elden Ring 的 TAE（TimeAct）动画事件数据提取为 JSON，再导入 Unity 生成 ScriptableObject，供 Animancer 运行时查询攻击判定、无敌帧、取消窗口等时间窗口。

---

## 1. 数据流

```
ER anibnd.dcx (639 个 .tae 文件)
    │
    ▼  [Python 脚本: batch_export_tae.py]
    │
JSON 文件 (632 个, 存 Assets/_ELDENRING_REF/TAE_Events/)
    │
    ▼  [Unity Editor: Tools → TAE Event Importer]
    │
ScriptableObject (632 个, 存 Assets/_ELDENRING_REF/TAE_Events_SO/)
    │
    ▼  [运行时: TAEEventQuery]
    │
战斗系统 / Animancer 播放逻辑
```

---

## 2. 提取 TAE → JSON（已完成）

在 Blender 目录下运行：

```powershell
.\blender.exe --background --python C:\Users\admin\Desktop\batch_export_tae.py
```

输出位置：`Assets/_ELDENRING_REF/TAE_Events/`

- `a00.json` ~ `a999.json`：每个 TAE 一个文件
- `_summary.json`：汇总统计

JSON 结构示例：

```json
{
  "tae": "a00",
  "taeId": 2000,
  "animationCount": 1839,
  "eventCount": 51253,
  "animations": [
    {
      "animId": "a000_003000",
      "rawId": 3000,
      "events": [
        {
          "type": 112,
          "start": 0.2667,
          "end": 0.4667,
          "name": "Hitbox_DummyPoly",
          "category": "combat",
          "params": [3000, 145, -1]
        },
        {
          "type": 120,
          "start": 0.6667,
          "end": 1.0,
          "name": "Condition_ComboAttack",
          "category": "cancel",
          "params": [1]
        }
      ]
    }
  ]
}
```

---

## 3. 导入 JSON → ScriptableObject

1. 打开 Unity，等待编译完成
2. 菜单 **Tools → TAE Event Importer**
3. 确认路径：
   - JSON Folder: `Assets/_ELDENRING_REF/TAE_Events`
   - Output Folder: `Assets/_ELDENRING_REF/TAE_Events_SO`
4. 点击 **Import All TAE JSON → SO**
5. 等待进度条完成（约 632 个文件）

生成的 SO 资产可在 Inspector 中直接查看每个动画的事件列表。

---

## 4. 运行时使用

### 4.1 引用 SO

在你的战斗/角色脚本中引用需要的 TAE 数据：

```csharp
using LZ;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private TAEEventData taeA00; // 拖入 a00.asset（基础动作组）
    
    private TAEAnimationEntry? _currentAnimEvents;
}
```

### 4.2 播放动画时绑定事件数据

```csharp
// 开始播放某个攻击动画时
public void PlayAttack(AnimationClip clip)
{
    var state = animancer.Play(clip);
    
    // 用 clip 名查找对应的 TAE 事件
    // clip 名格式: "a000_003000"
    _currentAnimEvents = taeA00.GetAnimation(clip.name);
}
```

### 4.3 每帧查询事件窗口

```csharp
private void Update()
{
    if (_currentAnimEvents == null) return;
    
    var entry = _currentAnimEvents.Value;
    float time = animancerState.Time; // 当前播放秒数
    
    // 查询攻击判定是否激活
    if (TAEEventQuery.HasActiveHitbox(entry, time))
    {
        EnableHitbox();
    }
    else
    {
        DisableHitbox();
    }
    
    // 查询无敌帧
    if (TAEEventQuery.HasActiveIFrame(entry, time))
    {
        isInvulnerable = true;
    }
    
    // 查询取消窗口（连招输入）
    if (TAEEventQuery.HasActiveCancelWindow(entry, time))
    {
        if (hasBufferedInput)
            ExecuteNextCombo();
    }
}
```

### 4.4 获取详细事件参数

```csharp
// 获取当前激活的所有战斗事件（含 params）
private List<TAEEvent> _activeEvents = new();

private void CheckCombatEvents(float time)
{
    TAEEventQuery.GetActiveEvents(_currentAnimEvents.Value, time, "combat", _activeEvents);
    
    foreach (var ev in _activeEvents)
    {
        // ev.parameters[0] 通常是 atkId（攻击参数 ID）
        // ev.parameters[1] 通常是 dummyPoly（挂点 ID）
        int atkId = ev.parameters.Length > 0 ? ev.parameters[0] : 0;
        int dummyPoly = ev.parameters.Length > 1 ? ev.parameters[1] : 0;
        
        ActivateHitbox(atkId, dummyPoly);
    }
}
```

---

## 5. 事件类型速查

### 战斗相关（category: "combat"）

| type | name | 说明 | params 含义 |
|------|------|------|-------------|
| 0 | InvokeAttackBehavior | 触发攻击行为 | [behaviorId, ...] |
| 1 | InvokeBulletBehavior | 触发弹道/飞行物 | [bulletId, dummyPoly, ...] |
| 112 | Hitbox_DummyPoly | 挂点攻击判定盒 | [atkId, dummyPoly, ...] |
| 113 | Hitbox_BodyNode | 身体节点判定盒 | [atkId, boneId, ...] |
| 116 | DamageHitbox | 伤害判定 | [atkId, ...] |

### 无敌/霸体（category: "iframe"）

| type | name | 说明 |
|------|------|------|
| 144 | Invulnerability | 无敌帧（翻滚等） |
| 145 | SuperArmor_Poise | 霸体/削韧保护 |

### 取消窗口（category: "cancel"）

| type | name | 说明 |
|------|------|------|
| 120 | Condition_ComboAttack | 可接连招输入 |
| 121 | Condition_Dodge | 可接闪避 |
| 500 | EnableComboInput | 启用连招输入缓冲 |
| 510 | RollComboWindow | 翻滚连招窗口 |

### 特效（category: "vfx"）

| type | name | 说明 |
|------|------|------|
| 16 | PlaySFX_Body | 身体特效 |
| 96 | SpawnFFX_DummyPoly | 挂点生成 FFX |
| 300 | SpawnFFX_OneShot | 一次性特效 |
| 700 | WeaponTrail | 武器拖尾/刀光 |

### 音效（category: "sound"）

| type | name | 说明 |
|------|------|------|
| 66 | PlaySound_DummyPoly | 挂点音效 |
| 67 | PlaySound_Weapon | 武器音效 |

### 动作控制（category: "motion"）

| type | name | 说明 |
|------|------|------|
| 605 | RootMotion_Mult | 根运动倍率 |
| 787 | Homing_Movement | 追踪移动 |
| 792 | MovementFlag | 移动标记 |

完整列表见 `_summary.json` 的 `eventTypeDistribution`。

---

## 6. 事件时间说明

- `startTime` / `endTime`：单位为**秒**，与帧率无关
- `endTime = -1`：表示持续到动画结束（原始值为 FLT_MAX）
- 时间直接对应 Animancer 的 `state.Time`（秒），无需换算
- 示例：`start=0.2667, end=0.4667` 表示"第 0.267 秒到第 0.467 秒之间判定激活"

---

## 7. 动画 ID 与 Clip 名的对应

TAE 中的动画 ID 格式：`a{类别:03d}_{编号:06d}`

对应关系：
- TAE `a00.json` 里的 `"animId": "a000_003000"` 
- 对应导出的 FBX clip 名 `a000_003000`

确保导入 FBX 时保留原始文件名作为 clip 名。

---

## 8. 进阶：按需加载

如果不想一次加载所有 SO，可以用 Addressables 或按需 Resources.Load：

```csharp
// 根据当前武器类型加载对应的 TAE
// 例如：武器类型 a020 → 加载 a20.asset
TAEEventData LoadTAEForWeapon(int weaponCategory)
{
    string taeFileName = $"a{weaponCategory:00}";
    return Resources.Load<TAEEventData>($"TAE/{taeFileName}");
}
```

---

## 9. 数据统计

- TAE 文件：632 个（7 个空文件已跳过）
- 动画总数：15,045 条
- 事件总数：593,876 个
- 事件类型：98 种
- 最常见事件：InvokeAttackBehavior (193,420)、SetPlayerInput (119,655)、SpawnFFX_OneShot (70,985)

---

## 10. 文件位置

| 文件 | 路径 |
|------|------|
| Python 提取脚本 | `C:\Users\admin\Desktop\batch_export_tae.py` |
| JSON 输出 | `Assets/_ELDENRING_REF/TAE_Events/*.json` |
| SO 输出 | `Assets/_ELDENRING_REF/TAE_Events_SO/*.asset` |
| Runtime 代码 | `Assets/Scripts/Runtime/Animation/TAEEventData.cs` |
| Editor 导入工具 | `Assets/Scripts/Editor/Animation/TAEEventImporter.cs` |
