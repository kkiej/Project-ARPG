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

        public TAEAnimationEntry? GetAnimation(string animId)
        {
            for (int i = 0; i < animations.Length; i++)
            {
                if (animations[i].animId == animId)
                    return animations[i];
            }
            return null;
        }

        public TAEAnimationEntry? GetAnimation(int rawId)
        {
            for (int i = 0; i < animations.Length; i++)
            {
                if (animations[i].rawId == rawId)
                    return animations[i];
            }
            return null;
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
