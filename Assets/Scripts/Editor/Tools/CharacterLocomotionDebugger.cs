using System.Collections.Generic;
using System.Text;
using Animancer;
using KinematicCharacterController;
using UnityEditor;
using UnityEngine;

namespace LZ.EditorTools
{
    /// <summary>
    /// GASP 风格的角色 Locomotion 调试可视化（仅 Editor）。
    ///
    /// 显示内容：
    /// - 历史轨迹（橙色）：过去 N 帧的位置
    /// - 预测轨迹（绿色）：未来 N 帧的位置（基于当前速度 + 输入推算）
    /// - 输入方向箭头（脚下黑色圆+箭头）
    /// - 朝向参考线（品红，沿 transform.right）
    /// - 右侧 HUD：State/Gait/Direction、Bool 标志、当前 Mixer 参数、激活的子动画及权重
    ///
    /// 使用方法：
    ///   菜单 LZ/Locomotion Debugger/Add To Selection 把组件挂到角色根节点。
    ///   F1 切换 HUD 显示。
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class CharacterLocomotionDebugger : MonoBehaviour
    {
        // ── 轨迹 ──
        [Header("Trajectory")]
        public bool showTrajectory = true;
        [Range(2, 32)] public int futureSamples = 8;
        [Range(0.1f, 3f)] public float predictDuration = 1.0f;
        [Range(5, 120)] public int historyCapacity = 30;
        [Range(0.01f, 0.2f)] public float historyInterval = 0.05f;
        [Range(0.05f, 1.0f)] public float velocitySmoothing = 0.15f;
        public Color futureColor = Color.green;
        public Color historyColor = new Color(1f, 0.55f, 0.1f);
        public Color facingColor = new Color(1f, 0f, 1f, 0.9f);
        [Range(1f, 8f)] public float lineThickness = 3f;

        // ── 方向箭头 ──
        [Header("Direction Arrow")]
        public bool showDirectionArrow = true;
        public float arrowSize = 1.2f;
        public Color arrowColor = new Color(0f, 0f, 0f, 0.85f);

        // ── HUD ──
        [Header("HUD")]
        public bool showHUD = true;
        public KeyCode toggleKey = KeyCode.F1;
        public int hudWidth = 320;
        public int hudFontSize = 13;
        public int hudMarginRight = 20;
        public int hudMarginTop = 100;

        // ── 缓存的组件引用 ──
        private CharacterManager _character;
        private PlayerManager _player;
        private CharacterAnimatorManager _anim;
        private KinematicCharacterMotor _motor;

        // ── 内部状态 ──
        private struct TrajPoint
        {
            public Vector3 pos;
            public Vector3 facing;
            public Vector3 vel;
        }

        private readonly Queue<TrajPoint> _history = new Queue<TrajPoint>(64);
        private TrajPoint[] _future = new TrajPoint[8];
        private float _nextHistoryTime;
        private GUIStyle _hudStyle;
        private bool _isPivoting;
        private int _lastSector = -1;
        private float _pivotTimer;

        // ─────────────────────────────────────────────────────────────
        // Lifecycle
        // ─────────────────────────────────────────────────────────────
        private void OnEnable()
        {
            CacheRefs();
        }

        private void Reset()
        {
            CacheRefs();
        }

        private void CacheRefs()
        {
            _character = GetComponent<CharacterManager>();
            _player = GetComponent<PlayerManager>();
            _anim = GetComponent<CharacterAnimatorManager>();
            _motor = GetComponent<KinematicCharacterMotor>();
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            if (Input.GetKeyDown(toggleKey)) showHUD = !showHUD;

            SampleHistory();
            DetectPivot();

            if (_future.Length != futureSamples) _future = new TrajPoint[futureSamples];
            PredictFuture();
        }

        // ─────────────────────────────────────────────────────────────
        // 轨迹采样 & 预测
        // ─────────────────────────────────────────────────────────────
        private void SampleHistory()
        {
            if (Time.time < _nextHistoryTime) return;

            if (_history.Count >= historyCapacity) _history.Dequeue();
            _history.Enqueue(new TrajPoint
            {
                pos = transform.position,
                facing = transform.forward,
                vel = _motor != null ? _motor.BaseVelocity : Vector3.zero
            });
            _nextHistoryTime = Time.time + historyInterval;
        }

        private void PredictFuture()
        {
            Vector3 pos = transform.position;
            Vector3 vel = _motor != null ? _motor.BaseVelocity : Vector3.zero;
            Vector3 desired = GetDesiredVelocity();

            float dt = predictDuration / futureSamples;
            float tau = Mathf.Max(0.01f, velocitySmoothing);

            for (int i = 0; i < _future.Length; i++)
            {
                float lerp = 1f - Mathf.Exp(-dt / tau);
                vel = Vector3.Lerp(vel, desired, lerp);
                pos += vel * dt;
                _future[i] = new TrajPoint { pos = pos, vel = vel, facing = vel.sqrMagnitude > 0.01f ? vel.normalized : transform.forward };
            }
        }

        private Vector3 GetDesiredVelocity()
        {
            if (_player == null) return Vector3.zero;

            float h = _player.playerLocomotionManager.horizontalMovement;
            float v = _player.playerLocomotionManager.verticalMovement;
            float moveAmount = _player.playerLocomotionManager.moveAmount;
            if (moveAmount < 0.01f) return Vector3.zero;

            Vector3 camForward, camRight;
            if (PlayerCamera.instance != null && PlayerCamera.instance.cameraObject != null)
            {
                camForward = PlayerCamera.instance.cameraObject.transform.forward;
                camRight = PlayerCamera.instance.cameraObject.transform.right;
            }
            else
            {
                camForward = transform.forward;
                camRight = transform.right;
            }

            Vector3 dir = camForward * v + camRight * h;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f) dir.Normalize();

            float speed;
            bool isSprint = _player.playerNetworkManager != null && _player.playerNetworkManager.isSprinting.Value;
            if (isSprint) speed = 6.5f;
            else if (moveAmount > 0.5f) speed = 5f;
            else speed = 2f;

            return dir * speed;
        }

        private void DetectPivot()
        {
            // 简易 Pivot 检测：上半区(F)/下半区(B) 切换瞬间标记 0.3s
            if (_player == null) return;
            float v = _player.playerLocomotionManager.verticalMovement;
            float h = _player.playerLocomotionManager.horizontalMovement;
            int sector;
            if (Mathf.Abs(v) < 0.05f && Mathf.Abs(h) < 0.05f) sector = -1;
            else sector = v >= 0 ? 0 : 1;

            if (_lastSector != -1 && sector != -1 && sector != _lastSector)
            {
                _isPivoting = true;
                _pivotTimer = 0.3f;
            }
            _lastSector = sector;

            if (_isPivoting)
            {
                _pivotTimer -= Time.deltaTime;
                if (_pivotTimer <= 0f) _isPivoting = false;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 绘制：Scene 视图（Gizmos / Handles，更漂亮的抗锯齿线）
        // ─────────────────────────────────────────────────────────────
        private void OnDrawGizmos()
        {
            if (!showTrajectory && !showDirectionArrow) return;
            if (!Application.isPlaying) return;

            DrawTrajectoryHandles();
            if (showDirectionArrow) DrawDirectionHandles();
        }

        private void DrawTrajectoryHandles()
        {
            if (!showTrajectory) return;

            // 朝向参考线（品红）
            Handles.color = facingColor;
            Vector3 footPos = transform.position + Vector3.up * 0.02f;
            Handles.DrawAAPolyLine(lineThickness,
                footPos - transform.right * 0.4f, footPos + transform.right * 0.4f);

            // 历史（橙）
            Handles.color = historyColor;
            TrajPoint? prev = null;
            foreach (var h in _history)
            {
                if (prev.HasValue)
                    Handles.DrawAAPolyLine(lineThickness, prev.Value.pos, h.pos);
                prev = h;
            }
            if (prev.HasValue)
                Handles.DrawAAPolyLine(lineThickness, prev.Value.pos, transform.position);

            // 预测（绿）
            Handles.color = futureColor;
            Vector3 last = transform.position;
            for (int i = 0; i < _future.Length; i++)
            {
                Handles.DrawAAPolyLine(lineThickness, last, _future[i].pos);
                Handles.SphereHandleCap(0, _future[i].pos, Quaternion.identity, 0.04f, EventType.Repaint);
                last = _future[i].pos;
            }
        }

        private void DrawDirectionHandles()
        {
            Vector3 inputDir = GetDesiredVelocity();
            if (inputDir.sqrMagnitude < 0.01f) return;
            inputDir.Normalize();

            Handles.color = arrowColor;
            float r = arrowSize * 0.5f;
            Vector3 center = transform.position + Vector3.up * 0.02f;

            // 圆环
            Handles.DrawWireDisc(center, Vector3.up, r);

            // 箭头杆
            Vector3 tip = center + inputDir * arrowSize;
            Handles.DrawAAPolyLine(lineThickness * 1.5f, center, tip);

            // 箭头双翅
            Vector3 left = Quaternion.AngleAxis(150f, Vector3.up) * inputDir * arrowSize * 0.35f;
            Vector3 right = Quaternion.AngleAxis(-150f, Vector3.up) * inputDir * arrowSize * 0.35f;
            Handles.DrawAAPolyLine(lineThickness * 1.5f, tip, tip + left);
            Handles.DrawAAPolyLine(lineThickness * 1.5f, tip, tip + right);
        }

        // ─────────────────────────────────────────────────────────────
        // HUD（Game 视图）
        // ─────────────────────────────────────────────────────────────
        private void OnGUI()
        {
            if (!showHUD || !Application.isPlaying) return;

            EnsureStyle();

            var sb = new StringBuilder(512);
            BuildHUDText(sb);

            string content = sb.ToString();
            float lineHeight = hudFontSize + 4f;
            int lineCount = CountLines(content);
            float h = lineHeight * lineCount + 16f;

            Rect rect = new Rect(Screen.width - hudWidth - hudMarginRight, hudMarginTop, hudWidth, h);

            // 背景
            Color prevColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = prevColor;

            // 文字
            GUI.Label(new Rect(rect.x + 10, rect.y + 6, rect.width - 20, rect.height - 12),
                content, _hudStyle);
        }

        private void BuildHUDText(StringBuilder sb)
        {
            string ground = (_character != null && _character.characterLocomotionManager != null
                && _character.characterLocomotionManager.isGrounded) ? "OnGround" : "Air";
            string stance = "Stand";
            string gait = GetGait();
            string dir = GetDirectionShort();

            sb.AppendLine($"<b><color=#ffffff>{ground} | {stance} | {gait} | {dir}</color></b>");
            sb.AppendLine();

            bool isMoving = _character != null && _character.characterNetworkManager != null
                && _character.characterNetworkManager.isMoving.Value;
            bool isSprinting = _player != null && _player.playerNetworkManager != null
                && _player.playerNetworkManager.isSprinting.Value;
            bool isLockedOn = _player != null && _player.playerNetworkManager != null
                && _player.playerNetworkManager.isLockedOn.Value;

            AppendBool(sb, "Is Moving",      isMoving);
            AppendBool(sb, "Is Pivoting",    _isPivoting);
            AppendBool(sb, "Is Sprinting",   isSprinting);
            AppendBool(sb, "Is Locked On",   isLockedOn);
            sb.AppendLine();

            if (_anim != null)
            {
                Vector2 mp = _anim.GetCurrentMixerParameter();
                sb.AppendLine($"<color=#88ddff>Mixer Param:</color> ({mp.x:F2}, {mp.y:F2})");
            }
            if (_motor != null)
            {
                Vector3 v = _motor.BaseVelocity;
                v.y = 0;
                sb.AppendLine($"<color=#88ddff>Speed:</color> {v.magnitude:F2} m/s");
            }
            sb.AppendLine();

            sb.AppendLine("<b>Active Clips:</b>");
            AppendActiveClips(sb);
        }

        private void AppendActiveClips(StringBuilder sb)
        {
            var animancer = _character != null ? _character.animancer : null;
            if (animancer == null) { sb.AppendLine("   (no animancer)"); return; }

            for (int layerIdx = 0; layerIdx < animancer.Layers.Count; layerIdx++)
            {
                var layer = animancer.Layers[layerIdx];
                if (layer.Weight < 0.01f) continue;
                var current = layer.CurrentState;
                if (current == null) continue;

                sb.AppendLine($"<color=#aaaaaa>L{layerIdx} (w={layer.Weight:F2}):</color>");

                if (current is ManualMixerState mixer)
                {
                    int dominantI = -1;
                    float maxW = 0f;
                    for (int i = 0; i < mixer.ChildCount; i++)
                    {
                        var c = mixer.GetChild(i);
                        if (c == null) continue;
                        if (c.Weight > maxW) { maxW = c.Weight; dominantI = i; }
                    }

                    bool printed = false;
                    for (int i = 0; i < mixer.ChildCount; i++)
                    {
                        var c = mixer.GetChild(i);
                        if (c == null || c.Weight < 0.01f) continue;
                        string name = (c is ClipState cs && cs.Clip != null) ? cs.Clip.name : c.GetType().Name;
                        string mark = (i == dominantI) ? "  <color=#ff8888><<<</color>" : "";
                        sb.AppendLine($"   [{c.Weight:F2}] {Truncate(name, 36)}{mark}");
                        printed = true;
                    }
                    if (!printed) sb.AppendLine("   (mixer no active children)");
                }
                else if (current is ClipState clipState && clipState.Clip != null)
                {
                    sb.AppendLine($"   {Truncate(clipState.Clip.name, 36)}  <color=#ff8888><<<</color>");
                }
                else
                {
                    sb.AppendLine($"   {current.GetType().Name}");
                }
            }
        }

        private string GetGait()
        {
            if (_player == null) return "?";
            if (_player.playerNetworkManager != null && _player.playerNetworkManager.isSprinting.Value) return "Sprint";
            if (_player.playerLocomotionManager.moveAmount > 0.5f) return "Run";
            if (_player.playerLocomotionManager.moveAmount > 0.01f) return "Walk";
            return "Idle";
        }

        private string GetDirectionShort()
        {
            if (_player == null) return "-";
            float v = _player.playerLocomotionManager.verticalMovement;
            float h = _player.playerLocomotionManager.horizontalMovement;
            if (Mathf.Abs(v) < 0.01f && Mathf.Abs(h) < 0.01f) return "-";
            float ang = Mathf.Atan2(h, v) * Mathf.Rad2Deg;
            if (ang > -22.5f && ang <= 22.5f) return "F";
            if (ang > 22.5f && ang <= 67.5f) return "FR";
            if (ang > 67.5f && ang <= 112.5f) return "R";
            if (ang > 112.5f && ang <= 157.5f) return "BR";
            if (ang > 157.5f || ang <= -157.5f) return "B";
            if (ang > -157.5f && ang <= -112.5f) return "BL";
            if (ang > -112.5f && ang <= -67.5f) return "L";
            return "FL";
        }

        private static void AppendBool(StringBuilder sb, string label, bool v)
        {
            string col = v ? "#5fff5f" : "#ff5f5f";
            sb.AppendLine($"{label}: <color={col}>{v}</color>");
        }

        private static int CountLines(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            int n = 1;
            for (int i = 0; i < s.Length; i++) if (s[i] == '\n') n++;
            return n;
        }

        private static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s) || s.Length <= max) return s;
            return s.Substring(0, max - 1) + "…";
        }

        private void EnsureStyle()
        {
            if (_hudStyle == null)
            {
                _hudStyle = new GUIStyle();
                _hudStyle.normal.textColor = Color.white;
                _hudStyle.fontSize = hudFontSize;
                _hudStyle.richText = true;
                _hudStyle.wordWrap = false;
                Font f = Font.CreateDynamicFontFromOSFont("Consolas", hudFontSize)
                      ?? Font.CreateDynamicFontFromOSFont("Courier New", hudFontSize);
                if (f != null) _hudStyle.font = f;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 菜单：把组件加到选中的角色上（Editor 程序集中的 MonoBehaviour
        //   不会出现在 Inspector 的 Add Component 列表里，需要这条命令）
        // ─────────────────────────────────────────────────────────────
        [MenuItem("LZ/Locomotion Debugger/Add To Selection", priority = 100)]
        private static void AddToSelection()
        {
            int added = 0;
            foreach (var go in Selection.gameObjects)
            {
                if (go.GetComponent<CharacterManager>() == null) continue;
                if (go.GetComponent<CharacterLocomotionDebugger>() != null) continue;
                Undo.AddComponent<CharacterLocomotionDebugger>(go);
                added++;
            }
            Debug.Log($"[LocomotionDebugger] Added to {added} character(s).");
        }

        [MenuItem("LZ/Locomotion Debugger/Remove From Selection", priority = 101)]
        private static void RemoveFromSelection()
        {
            int removed = 0;
            foreach (var go in Selection.gameObjects)
            {
                var comp = go.GetComponent<CharacterLocomotionDebugger>();
                if (comp != null) { Undo.DestroyObjectImmediate(comp); removed++; }
            }
            Debug.Log($"[LocomotionDebugger] Removed from {removed} character(s).");
        }

        [MenuItem("LZ/Locomotion Debugger/Add To Selection", validate = true)]
        private static bool AddToSelectionValidate() => Selection.gameObjects.Length > 0;
    }
}
