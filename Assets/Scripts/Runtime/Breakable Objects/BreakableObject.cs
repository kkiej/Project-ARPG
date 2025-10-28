using UnityEngine;
using Unity.Netcode;
using System.Collections;

namespace LZ
{
    public class BreakableObject : NetworkBehaviour
    {
        [Header("Position")]
        public NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [Header("Status")]
        public NetworkVariable<bool> isBroken = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        //  THIS LOCAL FLAG IS USED FOR CLIENTS/NON OWNERS, WHO BREAK IT LOCALLY ON THEIR END AND INSTEAD OF HAVING TO WAIT FOR THE IS BROKEN TO UPDATE, THEY BREAK IT INSTANTLY
        //  AND LET THE SERVER KNOW TO BREAK IT FOR EVERYBODY ELSE
        [HideInInspector] public bool isBrokenLocal = false;

        //  WHEN THE OBJECT BREAKS WE WILL NOT DISABLE THE ENTIRE GAMEOBJECT, ONLY THE MESH RENDERERS AND COLLIDERS, SO ANY NETWORK INFO THAT CHANGES CAN STILL BE PASSED
        [Header("Mesh Renderers")]
        [SerializeField] private MeshRenderer[] meshRenderers;

        [Header("Collision")]
        [SerializeField] Collider[] meshColliders;

        [Header("SFX")]
        private AudioSource audioSource;
        [SerializeField] AudioClip[] brokenSFX;

        [Header("Instantiated Broken Object")]
        private GameObject brokenObjectPrefab;
        private GameObject instantiatedBrokenObject;

        //  TO DO, ADD AN "ACTIVATION BEACON" SIMILAR TO A.I CHARACTERS WHERE WHEN THE LOCAL PLAYER IS FAR ENOUGH AWAY MESHES ARE HIDDEN TO SAVE MEMORY

        [Header("On Break Settings")]
        [SerializeField] bool addForceOnBreak = false;
        [SerializeField] float addedExplosionDebrisForce = 350;
        [SerializeField] float adddedForceDebrisRadius = 5;
        [SerializeField] float addedTorqueForceMinimum = 250;
        [SerializeField] float addedTorqueForceMaximum = 500;

        private void Awake()
        {
            meshRenderers = GetComponentsInChildren<MeshRenderer>();
            meshColliders = GetComponentsInChildren<Collider>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            //  CREATE THE BEACON HERE
            //  ADD TO WORLD OBJECT LIST IF LOADING/UNLOADING BREAKABLES DEPENDING ON SCENES LOADED
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            isBroken.OnValueChanged += OnIsBrokenChanged;
            networkPosition.OnValueChanged += OnNetworkPositionChanged;
            networkRotation.OnValueChanged += OnNetworkRotationChanged;

            //  IF WE ARE NOT THE HOST, THERE IS A POSSIBILITY WE ARE LOADING INTO A GAME WORLD WHERE THIS OBJECT IS ALREADY BROKEN, SO CHECK
            if (!NetworkManager.Singleton.IsHost)
                OnIsBrokenChanged(false, isBroken.Value);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            //  DESTROY BEACON

            //  DESTROY BROKEN OBJECT
            if (instantiatedBrokenObject != null)
                Destroy(instantiatedBrokenObject);

            isBroken.OnValueChanged -= OnIsBrokenChanged;
            networkPosition.OnValueChanged -= OnNetworkPositionChanged;
            networkRotation.OnValueChanged -= OnNetworkRotationChanged;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void OnTriggerEnter(Collider other)
        {
            //  SINCE BREAKABLES ARE NOT STATIC, NAV MESH WILL NOT MAKE THE A.I GO AROUND THEM (UNLESS USING OBSTACLE COMPONENT)
            //  WE CAN MAKE THE BREAKABLE BREAK ON CONTACT WITH THE A.I
            //  OPTIONALLY, MAKE THE AI PLAY AN ATTACK ANIMATION HERE TO REALLY SELL IT
            AICharacterManager aiCharacter = other.GetComponent<AICharacterManager>();

            if (aiCharacter != null)
                BreakObject();

            PlayerManager player = other.GetComponent<PlayerManager>();

            if (player != null)
            {
                //  CHECK FOR ROLLING AND JUMPING (TO DO: ROLLING)
                if (player.playerNetworkManager.isJumping.Value)
                    BreakObject();
            }

            DamageCollider damageCollider = other.GetComponent<DamageCollider>();

            if (damageCollider != null)
                BreakObject();
        }

        private void BreakObject()
        {
            if (isBroken.Value || isBrokenLocal)
                return;

            PlayBreakFX();
            BreakObjectServerRpc();
        }

        [ServerRpc]
        private void BreakObjectServerRpc()
        {

        }

        private void OnIsBrokenChanged(bool oldStatus, bool newStatus)
        {

        }

        private void PlayBreakFX()
        {
            isBrokenLocal = true;

            if (!gameObject.activeInHierarchy)
                return;

            instantiatedBrokenObject = Instantiate(brokenObjectPrefab, transform);

            if (addForceOnBreak)
            {
                Rigidbody[] rigidbodies = instantiatedBrokenObject.GetComponentsInChildren<Rigidbody>();

                for (int i = 0; i < rigidbodies.Length; i++)
                {
                    rigidbodies[i].AddExplosionForce(addedExplosionDebrisForce, rigidbodies[i].transform.position, adddedForceDebrisRadius);
                    Vector3 torqueDirection = Random.onUnitSphere;
                    rigidbodies[i].AddTorque(torqueDirection * Random.Range(addedTorqueForceMinimum, addedTorqueForceMaximum), ForceMode.Impulse);
                }
            }
        }

        private void OnNetworkPositionChanged(Vector3 oldPosition, Vector3 newPosition)
        {

        }

        private void OnNetworkRotationChanged(Quaternion oldPosition, Quaternion newPosition)
        {

        }

        private void ToggleMeshRenderers(bool status)
        {

        }
    }
}
