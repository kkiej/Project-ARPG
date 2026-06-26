using System;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [Serializable]
    public struct TAEEvent
    {
        public int type;
        public string name;
        public string category;
        public float startTime;
        public float endTime;
        public int[] parameters;
    }

    [Serializable]
    public struct TAEAnimationEntry
    {
        public string animId;
        public int rawId;
        public TAEEvent[] events;
    }

    [CreateAssetMenu(menuName = "Animation/TAE Event Data")]
    public class TAEEventData : ScriptableObject
    {
        public string tae;
        public int taeId;
        public TAEAnimationEntry[] animations;

        // 单 SO 可含上万条（c0000 全量转储）→ 用惰性字典避免线性查找。
        [System.NonSerialized] private Dictionary<int, int> _rawIndex;
        [System.NonSerialized] private Dictionary<string, int> _animIdIndex;

        private void BuildIndex()
        {
            _rawIndex = new Dictionary<int, int>(animations.Length);
            _animIdIndex = new Dictionary<string, int>(animations.Length);
            for (int i = 0; i < animations.Length; i++)
            {
                _rawIndex[animations[i].rawId] = i;
                if (!string.IsNullOrEmpty(animations[i].animId))
                    _animIdIndex[animations[i].animId] = i;
            }
        }

        /// <summary>清空惰性索引（导入器重写 animations 后调用）。</summary>
        public void InvalidateIndex() { _rawIndex = null; _animIdIndex = null; }

        public TAEAnimationEntry? GetAnimation(string animId)
        {
            if (animations == null || animations.Length == 0) return null;
            if (_animIdIndex == null) BuildIndex();
            return _animIdIndex.TryGetValue(animId, out int i) ? animations[i] : (TAEAnimationEntry?)null;
        }

        public TAEAnimationEntry? GetAnimation(int rawId)
        {
            if (animations == null || animations.Length == 0) return null;
            if (_rawIndex == null) BuildIndex();
            return _rawIndex.TryGetValue(rawId, out int i) ? animations[i] : (TAEAnimationEntry?)null;
        }
    }

    public static class TAEEventQuery
    {
        public static bool IsInWindow(TAEEvent ev, float time)
        {
            if (ev.endTime < 0) // -1 means until animation ends
                return time >= ev.startTime;
            return time >= ev.startTime && time <= ev.endTime;
        }

        public static bool HasActiveEvent(TAEAnimationEntry entry, float time, string category)
        {
            for (int i = 0; i < entry.events.Length; i++)
            {
                if (entry.events[i].category == category && IsInWindow(entry.events[i], time))
                    return true;
            }
            return false;
        }

        public static bool HasActiveHitbox(TAEAnimationEntry entry, float time)
        {
            return HasActiveEvent(entry, time, "combat");
        }

        public static bool HasActiveIFrame(TAEAnimationEntry entry, float time)
        {
            return HasActiveEvent(entry, time, "iframe");
        }

        public static bool HasActiveCancelWindow(TAEAnimationEntry entry, float time)
        {
            return HasActiveEvent(entry, time, "cancel");
        }

        /// <summary>
        /// type 0（ChrActionFlag）在 <paramref name="time"/> 时刻是否有指定 flag（parameters[0]）激活。
        /// 无敌帧=flag 8、连招缓冲=flag 87、取消=flag 4/115 等（见 TAE_Events/README.md §5.3）。
        /// </summary>
        public static bool HasActiveActionFlag(TAEAnimationEntry entry, float time, int flag)
        {
            if (entry.events == null) return false;
            for (int i = 0; i < entry.events.Length; i++)
            {
                var ev = entry.events[i];
                if (ev.type == 0 && ev.parameters != null && ev.parameters.Length > 0
                    && ev.parameters[0] == flag && IsInWindow(ev, time))
                    return true;
            }
            return false;
        }

        /// <summary>求某个 type0 flag 在该动画里的并集开窗 [start,end]（秒）；无则返回 false。end=-1 段视为持续到结尾。</summary>
        public static bool TryGetActionFlagWindow(TAEAnimationEntry entry, int flag, out float start, out float end)
        {
            start = float.PositiveInfinity; end = float.NegativeInfinity;
            if (entry.events != null)
            {
                for (int i = 0; i < entry.events.Length; i++)
                {
                    var ev = entry.events[i];
                    if (ev.type != 0 || ev.parameters == null || ev.parameters.Length == 0 || ev.parameters[0] != flag)
                        continue;
                    float e = ev.endTime < 0f ? ev.startTime : ev.endTime;
                    if (ev.startTime < start) start = ev.startTime;
                    if (e > end) end = e;
                }
            }
            bool ok = end > start && end > 0f;
            if (!ok) { start = 0f; end = 0f; }
            return ok;
        }

        public static void GetActiveEvents(TAEAnimationEntry entry, float time, string category, List<TAEEvent> results)
        {
            results.Clear();
            for (int i = 0; i < entry.events.Length; i++)
            {
                if (entry.events[i].category == category && IsInWindow(entry.events[i], time))
                    results.Add(entry.events[i]);
            }
        }
    }
}
