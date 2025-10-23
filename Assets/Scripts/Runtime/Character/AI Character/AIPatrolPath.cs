using UnityEngine;
using System.Collections.Generic;

namespace LZ
{
    public class AIPatrolPath : MonoBehaviour
    {
        public int patrolPathID = 0;
        public List<Vector3> patrolPoints = new List<Vector3>();

        private void Awake()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                patrolPoints.Add(transform.GetChild(i).position);
            }

            WorldAIManager.instance.AddPatrolPathToList(this);
        }
    }
}
