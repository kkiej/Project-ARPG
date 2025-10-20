using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [System.Serializable]
    public class SerializableRangedProjectile : ISerializationCallbackReceiver
    {
        [SerializeField] public int itemID;
        [SerializeField] public int itemAmount;

        public RangedProjectileItem GetProjectile()
        {
            RangedProjectileItem projectile = WorldItemDatabase.Instance.GetRangedProjectileFromSerializedData(this);
            return projectile;
        }

        public void OnAfterDeserialize()
        {

        }

        public void OnBeforeSerialize()
        {

        }
    }
}
