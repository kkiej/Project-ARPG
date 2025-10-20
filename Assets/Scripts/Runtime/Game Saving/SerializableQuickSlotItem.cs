using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [System.Serializable]
    public class SerializableQuickSlotItem : ISerializationCallbackReceiver
    {
        [SerializeField] public int itemID;
        [SerializeField] public int itemAmount;

        public QuickSlotItem GetQuickSlotItem()
        {
            QuickSlotItem quickSlotItem = WorldItemDatabase.Instance.GetQuickSlotItemFromSerializedData(this);
            return quickSlotItem;
        }

        public void OnAfterDeserialize()
        {

        }

        public void OnBeforeSerialize()
        {

        }
    }
}
