using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [System.Serializable]
    public class SerializableFlask : ISerializationCallbackReceiver
    {
        [SerializeField] public int itemID;
        //[SerializeField] public int maxFlaskCharges;
        //[SerializeField] public int flaskHealAmount;

        public FlaskItem GetFlask()
        {
            FlaskItem flask = WorldItemDatabase.Instance.GetFlaskFromSerializedData(this);
            return flask;
        }

        public void OnAfterDeserialize()
        {

        }

        public void OnBeforeSerialize()
        {

        }
    }
}
