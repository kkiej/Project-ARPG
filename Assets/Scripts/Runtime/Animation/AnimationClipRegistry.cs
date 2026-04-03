using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    /// <summary>
    /// 全局动画片段注册表。所有需要通过网络同步的 AnimationClip 都应注册在此。
    /// 所有客户端必须引用同一份资产，以保证 clip ↔ id 映射的一致性。
    /// </summary>
    [CreateAssetMenu(menuName = "Animation/Animation Clip Registry")]
    public class AnimationClipRegistry : ScriptableObject
    {
        public static AnimationClipRegistry Instance { get; private set; }

        [Tooltip("所有需要网络同步的动画片段。数组索引即为网络传输 ID，切勿随意调换顺序。")]
        [SerializeField] private List<AnimationClip> registeredClips = new List<AnimationClip>();

        private Dictionary<int, AnimationClip> _idToClip;
        private Dictionary<AnimationClip, int> _clipToId;
        private Dictionary<string, int> _nameToId;
        private bool _initialized;

        public void Initialize()
        {
            if (_initialized) return;

            Instance = this;

            _idToClip = new Dictionary<int, AnimationClip>(registeredClips.Count);
            _clipToId = new Dictionary<AnimationClip, int>(registeredClips.Count);
            _nameToId = new Dictionary<string, int>(registeredClips.Count);

            for (int i = 0; i < registeredClips.Count; i++)
            {
                var clip = registeredClips[i];
                if (clip == null)
                {
                    Debug.LogWarning($"[AnimationClipRegistry] Index {i} is null, skipping.");
                    continue;
                }

                if (_clipToId.ContainsKey(clip))
                {
                    Debug.LogWarning($"[AnimationClipRegistry] Duplicate clip '{clip.name}' at index {i}, skipping.");
                    continue;
                }

                _idToClip[i] = clip;
                _clipToId[clip] = i;

                if (!_nameToId.ContainsKey(clip.name))
                    _nameToId[clip.name] = i;
                else
                    Debug.LogWarning($"[AnimationClipRegistry] Duplicate clip name '{clip.name}' at index {i}. Name lookup will use the first one.");
            }

            _initialized = true;
        }

        /// <summary>
        /// 通过 ID 获取 AnimationClip（用于网络接收端）
        /// </summary>
        public AnimationClip GetClip(int id)
        {
            if (_idToClip != null && _idToClip.TryGetValue(id, out var clip))
                return clip;

            Debug.LogWarning($"[AnimationClipRegistry] Clip not found for id {id}");
            return null;
        }

        /// <summary>
        /// 通过 AnimationClip 获取 ID（用于网络发送端）
        /// </summary>
        public int GetId(AnimationClip clip)
        {
            if (clip != null && _clipToId != null && _clipToId.TryGetValue(clip, out var id))
                return id;

            Debug.LogWarning($"[AnimationClipRegistry] ID not found for clip '{(clip != null ? clip.name : "null")}'");
            return -1;
        }

        /// <summary>
        /// 通过 clip 名称获取 ID（用于从旧 string 系统过渡）
        /// </summary>
        public int GetIdByName(string clipName)
        {
            if (_nameToId != null && _nameToId.TryGetValue(clipName, out var id))
                return id;

            return -1;
        }

        /// <summary>
        /// 通过 clip 名称获取 AnimationClip（用于从旧 string 系统过渡）
        /// </summary>
        public AnimationClip GetClipByName(string clipName)
        {
            int id = GetIdByName(clipName);
            return id >= 0 ? GetClip(id) : null;
        }

        public int ClipCount => registeredClips.Count;

        public bool IsInitialized => _initialized;

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: 获取内部列表引用，供 Editor 工具填充
        /// </summary>
        public List<AnimationClip> EditorGetClipList() => registeredClips;
#endif

        private void OnDisable()
        {
            _initialized = false;
            _idToClip = null;
            _clipToId = null;
            _nameToId = null;
            if (Instance == this) Instance = null;
        }
    }
}
