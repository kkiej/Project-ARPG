using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace LZ
{
    public class NetworkObjectSpawner : MonoBehaviour
    {
        [Header("Object")]
        [SerializeField] protected GameObject networkGameObject;
        [SerializeField] protected GameObject instantiatedGameObject;

        private void Awake()
        {

        }

        private void Start()
        {
            WorldObjectManager.instance.SpawnObject(this);
            gameObject.SetActive(false);
        }

        public virtual void AttemptToSpawnObject()
        {
            if (networkGameObject != null)
            {
                instantiatedGameObject = Instantiate(networkGameObject);
                instantiatedGameObject.transform.position = transform.position;
                instantiatedGameObject.transform.rotation = transform.rotation;
                instantiatedGameObject.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}