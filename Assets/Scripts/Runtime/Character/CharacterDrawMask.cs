using System;
using UnityEngine;

namespace LZ
{
    [Serializable]
    public struct DrawMaskGroup
    {
        public string name;
        public SkinnedMeshRenderer[] renderers;
    }

    public class CharacterDrawMask : MonoBehaviour
    {
        [SerializeField] private DrawMaskGroup[] groups;

        private uint _visibleMask = 0xFFFFFFFF;

        public void SetGroupVisible(int groupIndex, bool visible)
        {
            if (groupIndex < 0 || groupIndex >= groups.Length) return;

            if (visible)
                _visibleMask |= (1u << groupIndex);
            else
                _visibleMask &= ~(1u << groupIndex);

            var group = groups[groupIndex];
            for (int i = 0; i < group.renderers.Length; i++)
            {
                if (group.renderers[i] != null)
                    group.renderers[i].enabled = visible;
            }
        }

        public void SetGroupVisible(string groupName, bool visible)
        {
            for (int i = 0; i < groups.Length; i++)
            {
                if (groups[i].name == groupName)
                {
                    SetGroupVisible(i, visible);
                    return;
                }
            }
        }

        public bool IsGroupVisible(int groupIndex)
        {
            return (_visibleMask & (1u << groupIndex)) != 0;
        }

        public void ShowOnly(params int[] groupIndices)
        {
            for (int i = 0; i < groups.Length; i++)
                SetGroupVisible(i, false);
            for (int i = 0; i < groupIndices.Length; i++)
                SetGroupVisible(groupIndices[i], true);
        }

        public void ShowAll()
        {
            for (int i = 0; i < groups.Length; i++)
                SetGroupVisible(i, true);
        }

        public void HideAll()
        {
            for (int i = 0; i < groups.Length; i++)
                SetGroupVisible(i, false);
        }
    }
}
