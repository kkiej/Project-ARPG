using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class PlayerEquipmentManager : CharacterEquipmentManager
    {
        PlayerManager player;

        [Header("Weapon Model Instantiation Slots")]
        [HideInInspector] public WeaponModelInstantiationSlot rightHandWeaponSlot;
        [HideInInspector] public WeaponModelInstantiationSlot leftHandWeaponSlot;
        [HideInInspector] public WeaponModelInstantiationSlot leftHandShieldSlot;
        [HideInInspector] public WeaponModelInstantiationSlot backSlot;

        //  ER 数据驱动挂载：refID → 骨架里对应 'c0000 Dummy<n> [refID]' 挂点。
        //  由 InitializeWeaponSlots 扫描骨架构建，供按 WepAbsorpPosParam 选挂点用。
        private readonly Dictionary<int, Transform> erDummyByRefId = new Dictionary<int, Transform>();
        private static readonly System.Text.RegularExpressions.Regex DummyRefIdRegex =
            new System.Text.RegularExpressions.Regex(@"\[(\d+)\]\s*$");

        //  ER 双骨挂载所需的【绑定姿势】常量（Awake/BuildErDummyCache 时抓，动画未生效前才准确）。
        //  Dw0：dummy 绑定姿势世界矩阵；attachBone：dummy 的跟随骨(Unity 里即其父骨)及其绑定世界矩阵。
        private readonly Dictionary<int, Matrix4x4> erDummyBindWorld = new Dictionary<int, Matrix4x4>();
        private readonly Dictionary<int, Transform> erDummyAttachBone = new Dictionary<int, Transform>();
        private readonly Dictionary<int, Matrix4x4> erAttachBindWorld = new Dictionary<int, Matrix4x4>();

        //  空间骨（ER 的 ParentBoneIndex，武器 dummy 统一用 Model_Dmy_AttachWeapon）及其绑定世界矩阵。
        [Header("ER 挂载空间骨")]
        [Tooltip("ER 武器 dummy 的空间骨名（ParentBoneIndex）。卡利亚直剑 dummy[20] 实测为 Model_Dmy_AttachWeapon。")]
        public string erWeaponSpaceBoneName = "Model_Dmy_AttachWeapon";
        private Transform erWeaponSpaceBone;
        private Matrix4x4 erWeaponSpaceBoneBind = Matrix4x4.identity;
        private bool erWeaponSpaceBoneBindCaptured;

        //  ==== ER 忠实复刻挂载（DSAS dummy 公式 + 骨架自标定）====
        [Header("ER 忠实挂载 (DSAS 复刻)")]
        [Tooltip("ER 挂载数据(dummy 帧 + 标定骨)。为空则自动 Resources.Load(\"ERMountData\")。\n" +
                 "由 Tools/ER/导入挂载数据 从 c0000.flver 生成。")]
        public ERMountData erMountData;
        [Tooltip("DSAS 对 ER 武器的固定翻转(RotX180)在 Unity 轴系下的等价常量，剑刃与鞘共用。\n" +
                 "朝向不对时校准此值（可能是 (180,0,0)/(0,180,0)/(0,0,180) 之一）。")]
        public Vector3 erWeaponFlip = new Vector3(180f, 0f, 0f);
        private readonly ERWeaponMounter erMounter = new ERWeaponMounter();

        [Header("Weapon Models")]
        [HideInInspector] public GameObject rightHandWeaponModel;
        [HideInInspector] public GameObject leftHandWeaponModel;

        //  ER 剑鞘的独立实例（挂在腰间的鞘，与手持武器分开管理/销毁）。
        [HideInInspector] public GameObject rightHandSheathModel;
        [HideInInspector] public GameObject leftHandSheathModel;

        [Header("Weapon Managers")]
        public WeaponManager rightWeaponManager;
        public WeaponManager leftWeaponManager;

        [Header("General Equipment Models")]
        public GameObject hatsObject;
        [HideInInspector] public GameObject[] hats;
        public GameObject hoodsObject;
        [HideInInspector] public GameObject[] hoods;
        public GameObject faceCoversObject;
        [HideInInspector] public GameObject[] faceCovers;
        public GameObject helmetAccessoriesObject;
        [HideInInspector] public GameObject[] helmetAccessories;
        public GameObject backAccessoriesObject;
        [HideInInspector] public GameObject[] backAccessories;
        public GameObject hipAccessoriesObject;
        [HideInInspector] public GameObject[] hipAccessories;
        public GameObject rightShoulderObject;
        [HideInInspector] public GameObject[] rightShoulder;
        public GameObject rightElbowObject;
        [HideInInspector] public GameObject[] rightElbow;
        public GameObject rightKneeObject;
        [HideInInspector] public GameObject[] rightKnee;
        public GameObject leftShoulderObject;
        [HideInInspector] public GameObject[] leftShoulder;
        public GameObject leftElbowObject;
        [HideInInspector] public GameObject[] leftElbow;
        public GameObject leftKneeObject;
        [HideInInspector] public GameObject[] leftKnee;

        [Header("Male Equipment Models")]
        public GameObject maleFullHelmetObject;
        [HideInInspector] public GameObject[] maleHeadFullHelmets;
        public GameObject maleFullBodyObject;
        [HideInInspector] public GameObject[] maleBodies;
        public GameObject maleRightUpperArmObject;
        [HideInInspector] public GameObject[] maleRightUpperArms;
        public GameObject maleRightLowerArmObject;
        [HideInInspector] public GameObject[] maleRightLowerArms;
        public GameObject maleRightHandObject;
        [HideInInspector] public GameObject[] maleRightHands;
        public GameObject maleLeftUpperArmObject;
        [HideInInspector] public GameObject[] maleLeftUpperArms;
        public GameObject maleLeftLowerArmObject;
        [HideInInspector] public GameObject[] maleLeftLowerArms;
        public GameObject maleLeftHandObject;
        [HideInInspector] public GameObject[] maleLeftHands;
        public GameObject maleHipsObject;
        [HideInInspector] public GameObject[] maleHips;
        public GameObject maleRightLegObject;
        [HideInInspector] public GameObject[] maleRightLegs;
        public GameObject maleLeftLegObject;
        [HideInInspector] public GameObject[] maleLeftLegs;

        [Header("Female Equipment Models")]
        public GameObject femaleFullHelmetObject;
        [HideInInspector] public GameObject[] femaleHeadFullHelmets;
        public GameObject femaleFullBodyObject;
        [HideInInspector] public GameObject[] femaleBodies;
        public GameObject femaleRightUpperArmObject;
        [HideInInspector] public GameObject[] femaleRightUpperArms;
        public GameObject femaleRightLowerArmObject;
        [HideInInspector] public GameObject[] femaleRightLowerArms;
        public GameObject femaleRightHandObject;
        [HideInInspector] public GameObject[] femaleRightHands;
        public GameObject femaleLeftUpperArmObject;
        [HideInInspector] public GameObject[] femaleLeftUpperArms;
        public GameObject femaleLeftLowerArmObject;
        [HideInInspector] public GameObject[] femaleLeftLowerArms;
        public GameObject femaleLeftHandObject;
        [HideInInspector] public GameObject[] femaleLeftHands;
        public GameObject femaleHipsObject;
        [HideInInspector] public GameObject[] femaleHips;
        public GameObject femaleRightLegObject;
        [HideInInspector] public GameObject[] femaleRightLegs;
        public GameObject femaleLeftLegObject;
        [HideInInspector] public GameObject[] femaleLeftLegs;

        [Header("Modular Character (ER 共享骨架换装)")]
        [Tooltip("开启后，头/身/腿/手装备改用 ModularCharacterAssembler 按 itemID 动态加载部件并重绑定到 c0000 骨架；关闭则沿用旧的预置模型 SetActive 方案。")]
        public bool useModularEquipment = false;
        public ModularCharacterAssembler modularAssembler;
        public EquipmentPartCatalog partCatalog;

        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
            
            InitializeWeaponSlots();

            //  模块化模式不依赖预置模型容器，跳过旧的初始化（避免缺少 *Object 引用时报空）
            if (!useModularEquipment)
                InitializeArmorModels();

            if (useModularEquipment && modularAssembler == null)
                modularAssembler = GetComponentInChildren<ModularCharacterAssembler>(true);
        }

        /// <summary>
        /// 模块化换装入口（方案 B）：
        /// 权威数据在装备 SO 上（ArmorItem.modularPartCode），运行时按 槽位前缀 + 性别 + 编号
        /// 拼出部件名（如 BD_M_1350），到 EquipmentPartCatalog 查 prefab，交给装配器装上/卸下。
        /// owner 与联机远端都会经过 Load*Equipment，故此处统一覆盖两端。
        /// </summary>
        /// <returns>true=已用模块化处理（应跳过旧的 SetActive 流程）。</returns>
        private bool ApplyModularPart(BodySlot slot, EquipmentItem equipment)
        {
            if (!useModularEquipment) return false;

            if (modularAssembler == null)
            {
                Debug.LogWarning("[PlayerEquipmentManager] 已开启 useModularEquipment 但未设置 modularAssembler。", this);
                return false;
            }

            //  取基础编号（仅 ArmorItem 有）。为空 → 该件不走模块化，清空该槽。
            string baseCode = (equipment as ArmorItem)?.modularPartCode;
            if (equipment == null || string.IsNullOrEmpty(baseCode))
            {
                modularAssembler.UnequipPart(slot);
                return true;
            }

            string gender = player.playerNetworkManager.isMale.Value ? "M" : "F";
            string code = $"{SlotPrefix(slot)}_{gender}_{baseCode}";

            GameObject prefab = partCatalog != null ? partCatalog.GetPrefabByCode(code) : null;

            if (prefab != null)
            {
                modularAssembler.EquipPart(slot, prefab);
            }
            else
            {
                Debug.LogWarning($"[PlayerEquipmentManager] 部件目录中找不到 '{code}'（itemID={equipment.itemID}）。该槽留空。", this);
                modularAssembler.UnequipPart(slot);
            }

            return true;
        }

        //  槽位 -> ER 部件名前缀（HD 头 / BD 身 / AM 臂 / LG 腿）
        private static string SlotPrefix(BodySlot slot)
        {
            switch (slot)
            {
                case BodySlot.Head: return "HD";
                case BodySlot.Torso: return "BD";
                case BodySlot.Arms: return "AM";
                case BodySlot.Legs: return "LG";
                case BodySlot.Hair: return "HR";
                default: return "BD";
            }
        }

        protected override void Start()
        {
            base.Start();

            EquipWeapons();
        }

        public void EquipArmor()
        {
            LoadHeadEquipment(player.playerInventoryManager.headEquipment);
            LoadBodyEquipment(player.playerInventoryManager.bodyEquipment);
            LoadLegEquipment(player.playerInventoryManager.legEquipment);
            LoadHandEquipment(player.playerInventoryManager.handEquipment);
        }

        //  QUICK SLOTS
        public void SwitchQuickSlotItem()
        {
            if (!player.IsOwner)
                return;

            QuickSlotItem selectedItem = null;

            //  ADD ONE TO OUR INDEX TO SWITCH TO THE NEXT POTENTIAL WEAPON
            player.playerInventoryManager.quickSlotItemIndex += 1;

            //  IF OUR INDEX IS OUT OF BOUNDS, RESET IT TO POSITION #1 (0)
            if (player.playerInventoryManager.quickSlotItemIndex < 0 || player.playerInventoryManager.quickSlotItemIndex > 2)
            {
                player.playerInventoryManager.quickSlotItemIndex = 0;

                //  WE CHECK IF WE ARE HOLDING MORE THAN ONE WEAPON
                int itemCount = 0;
                QuickSlotItem firstItem = null;
                int firstItemPosition = 0;
                for (int i = 0; i < player.playerInventoryManager.quickSlotItemsInQuickSlots.Length; i++)
                {
                    if (player.playerInventoryManager.quickSlotItemsInQuickSlots[i] != null)
                    {
                        itemCount += 1;

                        if (firstItem == null)
                        {
                            firstItem = player.playerInventoryManager.quickSlotItemsInQuickSlots[i];
                            firstItemPosition = i;
                        }
                    }
                }

                if (itemCount <= 1)
                {
                    player.playerInventoryManager.quickSlotItemIndex = -1;
                    selectedItem = null;
                    player.playerNetworkManager.currentQuickSlotItemID.Value = -1;
                }
                else
                {
                    player.playerInventoryManager.quickSlotItemIndex = firstItemPosition;
                    player.playerNetworkManager.currentQuickSlotItemID.Value = firstItem.itemID;
                }

                return;
            }

            //  IF THE NEXT POTENTIAL WEAPON DOES NOT EQUAL THE UNARMED WEAPON
            if (player.playerInventoryManager.quickSlotItemsInQuickSlots[player.playerInventoryManager.quickSlotItemIndex] != null)
            {
                selectedItem = player.playerInventoryManager.quickSlotItemsInQuickSlots[player.playerInventoryManager.quickSlotItemIndex];
                //  ASSIGN THE NETWORK WEAPON ID SO IT SWITCHES FOR ALL CONNECTED CLIENTS
                player.playerNetworkManager.currentQuickSlotItemID.Value =
                    player.playerInventoryManager.quickSlotItemsInQuickSlots[player.playerInventoryManager.quickSlotItemIndex].itemID;
            }
            else
            {
                player.playerNetworkManager.currentQuickSlotItemID.Value = -1;
            }

            if (selectedItem == null && player.playerInventoryManager.quickSlotItemIndex <= 2)
            {
                SwitchQuickSlotItem();
            }
        }

        //  EQUIPMENT
        private void InitializeArmorModels()
        {
            //  HATS
            List<GameObject> hatsList = new List<GameObject>();

            foreach (Transform child in hatsObject.transform)
            {
                hatsList.Add(child.gameObject);
            }

            hats = hatsList.ToArray();

            //  HOODS
            List<GameObject> hoodsList = new List<GameObject>();

            foreach (Transform child in hoodsObject.transform)
            {
                hoodsList.Add(child.gameObject);
            }

            hoods = hoodsList.ToArray();

            //  FACE COVERS
            List<GameObject> faceCoversList = new List<GameObject>();

            foreach (Transform child in faceCoversObject.transform)
            {
                faceCoversList.Add(child.gameObject);
            }

            faceCovers = faceCoversList.ToArray();

            //  HELMET ACCESSORIES
            List<GameObject> helmetAccessoriesList = new List<GameObject>();

            foreach (Transform child in helmetAccessoriesObject.transform)
            {
                helmetAccessoriesList.Add(child.gameObject);
            }

            helmetAccessories = helmetAccessoriesList.ToArray();

            //  BACK ACCESSORIES
            List<GameObject> backAccessoriesList = new List<GameObject>();

            foreach (Transform child in backAccessoriesObject.transform)
            {
                backAccessoriesList.Add(child.gameObject);
            }

            backAccessories = backAccessoriesList.ToArray();

            //  HIP ACCESSORIES
            List<GameObject> hipAccessoriesList = new List<GameObject>();

            foreach (Transform child in hipAccessoriesObject.transform)
            {
                hipAccessoriesList.Add(child.gameObject);
            }

            hipAccessories = hipAccessoriesList.ToArray();

            //  RIGHT SHOULDER
            List<GameObject> rightShoulderList = new List<GameObject>();

            foreach (Transform child in rightShoulderObject.transform)
            {
                rightShoulderList.Add(child.gameObject);
            }

            rightShoulder = rightShoulderList.ToArray();

            //  RIGHT ELBOW
            List<GameObject> rightElbowList = new List<GameObject>();

            foreach (Transform child in rightElbowObject.transform)
            {
                rightElbowList.Add(child.gameObject);
            }

            rightElbow = rightElbowList.ToArray();

            //  RIGHT KNEE
            List<GameObject> rightKneeList = new List<GameObject>();

            foreach (Transform child in rightKneeObject.transform)
            {
                rightKneeList.Add(child.gameObject);
            }

            rightKnee = rightKneeList.ToArray();

            //  LEFT SHOULDER
            List<GameObject> leftShoulderList = new List<GameObject>();

            foreach (Transform child in leftShoulderObject.transform)
            {
                leftShoulderList.Add(child.gameObject);
            }

            leftShoulder = leftShoulderList.ToArray();

            //  LEFT ELBOW
            List<GameObject> leftElbowList = new List<GameObject>();

            foreach (Transform child in leftElbowObject.transform)
            {
                leftElbowList.Add(child.gameObject);
            }

            leftElbow = leftElbowList.ToArray();

            //  LEFT KNEE
            List<GameObject> leftKneeList = new List<GameObject>();

            foreach (Transform child in leftKneeObject.transform)
            {
                leftKneeList.Add(child.gameObject);
            }

            leftKnee = leftKneeList.ToArray();

            //  MALE EQUIPMENT

            List<GameObject> maleFullHelmetsList = new List<GameObject>();

            foreach (Transform child in maleFullHelmetObject.transform)
            {
                maleFullHelmetsList.Add(child.gameObject);
            }

            maleHeadFullHelmets = maleFullHelmetsList.ToArray();

            List<GameObject> maleBodiesList = new List<GameObject>();

            foreach (Transform child in maleFullBodyObject.transform)
            {
                maleBodiesList.Add(child.gameObject);
            }

            maleBodies = maleBodiesList.ToArray();

            //  MALE RIGHT UPPER ARM
            List<GameObject> maleRightUpperArmList = new List<GameObject>();

            foreach (Transform child in maleRightUpperArmObject.transform)
            {
                maleRightUpperArmList.Add(child.gameObject);
            }

            maleRightUpperArms = maleRightUpperArmList.ToArray();

            //  MALE RIGHT LOWER ARM
            List<GameObject> maleRightLowerArmList = new List<GameObject>();

            foreach (Transform child in maleRightLowerArmObject.transform)
            {
                maleRightLowerArmList.Add(child.gameObject);
            }

            maleRightLowerArms = maleRightLowerArmList.ToArray();

            //  MALE RIGHT HANDS
            List<GameObject> maleRightHandsList = new List<GameObject>();

            foreach (Transform child in maleRightHandObject.transform)
            {
                maleRightHandsList.Add(child.gameObject);
            }

            maleRightHands = maleRightHandsList.ToArray();

            //  MALE LEFT UPPER ARM
            List<GameObject> maleLeftUpperArmList = new List<GameObject>();

            foreach (Transform child in maleLeftUpperArmObject.transform)
            {
                maleLeftUpperArmList.Add(child.gameObject);
            }

            maleLeftUpperArms = maleLeftUpperArmList.ToArray();

            //  MALE LEFT LOWER ARM
            List<GameObject> maleLeftLowerArmList = new List<GameObject>();

            foreach (Transform child in maleLeftLowerArmObject.transform)
            {
                maleLeftLowerArmList.Add(child.gameObject);
            }

            maleLeftLowerArms = maleLeftLowerArmList.ToArray();

            //  MALE LEFT HANDS
            List<GameObject> maleLeftHandsList = new List<GameObject>();

            foreach (Transform child in maleLeftHandObject.transform)
            {
                maleLeftHandsList.Add(child.gameObject);
            }

            maleLeftHands = maleLeftHandsList.ToArray();

            //  MALE HIPS
            List<GameObject> maleHipsList = new List<GameObject>();

            foreach (Transform child in maleHipsObject.transform)
            {
                maleHipsList.Add(child.gameObject);
            }

            maleHips = maleHipsList.ToArray();

            //  MALE RIGHT LEG
            List<GameObject> maleRightLegList = new List<GameObject>();

            foreach (Transform child in maleRightLegObject.transform)
            {
                maleRightLegList.Add(child.gameObject);
            }

            maleRightLegs = maleRightLegList.ToArray();

            //  MALE LEFT LEG
            List<GameObject> maleLeftLegList = new List<GameObject>();

            foreach (Transform child in maleLeftLegObject.transform)
            {
                maleLeftLegList.Add(child.gameObject);
            }

            maleLeftLegs = maleLeftLegList.ToArray();

            //  FEMALE FULL HELMETS
            List<GameObject> femaleFullHelmetsList = new List<GameObject>();

            foreach (Transform child in femaleFullHelmetObject.transform)
            {
                femaleFullHelmetsList.Add(child.gameObject);
            }

            femaleHeadFullHelmets = femaleFullHelmetsList.ToArray();

            //  FEMALE BODY
            List<GameObject> femaleBodyList = new List<GameObject>();

            foreach (Transform child in femaleFullBodyObject.transform)
            {
                femaleBodyList.Add(child.gameObject);
            }

            femaleBodies = femaleBodyList.ToArray();

            //  FEMALE RIGHT UPPER ARM
            List<GameObject> femaleRightUpperArmList = new List<GameObject>();

            foreach (Transform child in femaleRightUpperArmObject.transform)
            {
                femaleRightUpperArmList.Add(child.gameObject);
            }

            femaleRightUpperArms = femaleRightUpperArmList.ToArray();

            //  FEMALE RIGHT LOWER ARM
            List<GameObject> femaleRightLowerArmList = new List<GameObject>();

            foreach (Transform child in femaleRightLowerArmObject.transform)
            {
                femaleRightLowerArmList.Add(child.gameObject);
            }

            femaleRightLowerArms = femaleRightLowerArmList.ToArray();

            //  FEMALE RIGHT HANDS
            List<GameObject> femaleRightHandsList = new List<GameObject>();

            foreach (Transform child in femaleRightHandObject.transform)
            {
                femaleRightHandsList.Add(child.gameObject);
            }

            femaleRightHands = femaleRightHandsList.ToArray();

            //  FEMALE LEFT UPPER ARM
            List<GameObject> femaleLeftUpperArmList = new List<GameObject>();

            foreach (Transform child in femaleLeftUpperArmObject.transform)
            {
                femaleLeftUpperArmList.Add(child.gameObject);
            }

            femaleLeftUpperArms = femaleLeftUpperArmList.ToArray();

            //  FEMALE LEFT LOWER ARM
            List<GameObject> femaleLeftLowerArmList = new List<GameObject>();

            foreach (Transform child in femaleLeftLowerArmObject.transform)
            {
                femaleLeftLowerArmList.Add(child.gameObject);
            }

            femaleLeftLowerArms = femaleLeftLowerArmList.ToArray();

            //  FEMALE LEFT HANDS
            List<GameObject> femaleLeftHandsList = new List<GameObject>();

            foreach (Transform child in femaleLeftHandObject.transform)
            {
                femaleLeftHandsList.Add(child.gameObject);
            }

            femaleLeftHands = femaleLeftHandsList.ToArray();

            //  FEMALE HIPS
            List<GameObject> femaleHipsList = new List<GameObject>();

            foreach (Transform child in femaleHipsObject.transform)
            {
                femaleHipsList.Add(child.gameObject);
            }

            femaleHips = femaleHipsList.ToArray();

            //  FEMALE RIGHT LEG
            List<GameObject> femaleRightLegList = new List<GameObject>();

            foreach (Transform child in femaleRightLegObject.transform)
            {
                femaleRightLegList.Add(child.gameObject);
            }

            femaleRightLegs = femaleRightLegList.ToArray();

            //  FEMALE LEFT LEG
            List<GameObject> femaleLeftLegList = new List<GameObject>();

            foreach (Transform child in femaleLeftLegObject.transform)
            {
                femaleLeftLegList.Add(child.gameObject);
            }

            femaleLeftLegs = femaleLeftLegList.ToArray();
        }

        public void LoadHeadEquipment(HeadEquipmentItem equipment)
        {
            // 1. 卸载旧的头部装备模型（如存在）
            if (!useModularEquipment)
                UnloadHeadEquipmentModels();
            
            // 2. 若装备为空，则直接将库存中的装备设为空并返回
            if (equipment == null)
            {
                if (player.IsOwner)
                    player.playerNetworkManager.headEquipmentID.Value = -1; //  -1 WILL NEVER BE AN ITEM ID, SO IT WILL ALWAYS BE NULL

                player.playerInventoryManager.headEquipment = null;
                ApplyModularPart(BodySlot.Head, null);
                return;
            }
            
            // 3. 若装备具有"OnItemEquipped"回调函数，立即执行
            
            // 4. 将传入此函数的装备设为玩家库存中的当前头部装备
            player.playerInventoryManager.headEquipment = equipment;
            
            // 5. 如需根据头部装备类型禁用特定身体特征（如头罩禁用头发，全覆式头盔禁用头部模型），在此处执行检查
			switch (equipment.headEquipmentType)
            {
                case HeadEquipmentType.FullHelmet:
                    player.playerBodyManager.DisableHair();
                    player.playerBodyManager.DisableHead();
                    break;
                case HeadEquipmentType.Hat:
                    break;
                case HeadEquipmentType.Hood:
                    player.playerBodyManager.DisableHair();
                    break;
                case HeadEquipmentType.FaceCover:
                    player.playerBodyManager.DisableFacialHair();
                    break;
                default:
                    break;
            }
			
            // 6. 加载头部装备模型
            if (!ApplyModularPart(BodySlot.Head, equipment))
            {
                foreach (var model in equipment.equipmentModels)
                {
                    model.LoadModel(player, player.playerNetworkManager.isMale.Value);
                }
            }
            
            // 7. 计算总装备负重（所有穿戴装备的重量之和，该数值会影响翻滚速度，过重时还会影响移动速度）
            
            // 8. 计算总护甲伤害吸收率
            player.playerStatsManager.CalculateTotalArmorAbsorption();
            
            if (player.IsOwner)
                player.playerNetworkManager.headEquipmentID.Value = equipment.itemID;
        }

        private void UnloadHeadEquipmentModels()
        {
            foreach (var model in maleHeadFullHelmets)
            {
                model.SetActive(false);
            }

            foreach (var model in femaleHeadFullHelmets)
            {
                model.SetActive(false);
            }

            foreach (var model in hats)
            {
                model.SetActive(false);
            }

            foreach (var model in faceCovers)
            {
                model.SetActive(false);
            }

            foreach (var model in hoods)
            {
                model.SetActive(false);
            }

            foreach (var model in helmetAccessories)
            {
                model.SetActive(false);
            }

            player.playerBodyManager.EnableHead();
            player.playerBodyManager.EnableHair();
        }

        public void LoadBodyEquipment(BodyEquipmentItem equipment)
        {
            // 1. 卸载旧装备模型（如存在）
			if (!useModularEquipment)
			    UnloadBodyEquipmentModels();
			
            // 2. 若装备为空，则直接将库存中的对应装备设为空并返回
			if (equipment == null)
            {
                if (player.IsOwner)
                    player.playerNetworkManager.bodyEquipmentID.Value = -1; //  -1 WILL NEVER BE AN ITEM ID, SO IT WILL ALWAYS BE NULL

                player.playerInventoryManager.bodyEquipment = null;
                ApplyModularPart(BodySlot.Torso, null);
                return;
            }

            //  3. IF YOU HAVE AN "ONITEMEQUIPPED" CALL ON YOUR EQUIPMENT, RUN IT NOW

            //  4. SET CURRENT HEAD EQUIPMENT IN PLAYER INVENTORY TO THE EQUIPMENT THAT IS PASSED TO THIS FUNCTION
            player.playerInventoryManager.bodyEquipment = equipment;

            //  5. IF YOU NEED TO CHECK FOR HEAD EQUIPMENT TYPE TO DISABLE CERTAIN BODY FEATURES (HOODS DISABLING HAIR ECT, FULL HELMS DISABLING HEADS) DO IT NOW
            player.playerBodyManager.DisableBody();

            //  6. LOAD HEAD EQUIPMENT MODELS
            if (!ApplyModularPart(BodySlot.Torso, equipment))
            {
                foreach (var model in equipment.equipmentModels)
                {
                    model.LoadModel(player, player.playerNetworkManager.isMale.Value);
                }
            }

            //  7. CALCULATE TOTAL EQUIPMENT LOAD (WEIGHT OF ALL YOUR WORN EQUIPMENT. THIS IMPACTS ROLL SPEED AND AT EXTREME WEIGHTS, MOVEMENT SPEED)

            //  8. CALCULATE TOTAL ARMOR ABSORPTION
            player.playerStatsManager.CalculateTotalArmorAbsorption();
			
			if (player.IsOwner)
                player.playerNetworkManager.bodyEquipmentID.Value = equipment.itemID;
        }

        private void UnloadBodyEquipmentModels()
        {
            foreach (var model in rightShoulder)
            {
                model.SetActive(false);
            }

            foreach (var model in rightElbow)
            {
                model.SetActive(false);
            }


            foreach (var model in leftShoulder)
            {
                model.SetActive(false);
            }

            foreach (var model in leftElbow)
            {
                model.SetActive(false);
            }

            foreach (var model in backAccessories)
            {
                model.SetActive(false);
            }

            //  MALE
            foreach (var model in maleBodies)
            {
                model.SetActive(false);
            }

            foreach (var model in maleRightUpperArms)
            {
                model.SetActive(false);
            }

            foreach (var model in maleLeftUpperArms)
            {
                model.SetActive(false);
            }

            //  FEMALE
            foreach (var model in femaleBodies)
            {
                model.SetActive(false);
            }

            foreach (var model in femaleRightUpperArms)
            {
                model.SetActive(false);
            }

            foreach (var model in femaleLeftUpperArms)
            {
                model.SetActive(false);
            }

            player.playerBodyManager.EnableBody();
        }

        public void LoadLegEquipment(LegEquipmentItem equipment)
        {
            //  1. UNLOAD OLD EQUIPMENT MODELS (IF ANY)
            if (!useModularEquipment)
                UnloadLegEquipmentModels();

            //  2. IF EQUIPMENT IS NULL SIMPLY SET EQUIPMENT IN INVENTORY TO NULL AND RETURN
            if (equipment == null)
            {
                if (player.IsOwner)
                    player.playerNetworkManager.legEquipmentID.Value = -1; //  -1 WILL NEVER BE AN ITEM ID, SO IT WILL ALWAYS BE NULL

                player.playerInventoryManager.legEquipment = null;
                ApplyModularPart(BodySlot.Legs, null);
                return;
            }

            //  3. IF YOU HAVE AN "ONITEMEQUIPPED" CALL ON YOUR EQUIPMENT, RUN IT NOW

            //  4. SET CURRENT HEAD EQUIPMENT IN PLAYER INVENTORY TO THE EQUIPMENT THAT IS PASSED TO THIS FUNCTION
            player.playerInventoryManager.legEquipment = equipment;

            //  5. IF YOU NEED TO CHECK FOR HEAD EQUIPMENT TYPE TO DISABLE CERTAIN BODY FEATURES (HOODS DISABLING HAIR ECT, FULL HELMS DISABLING HEADS) DO IT NOW
            player.playerBodyManager.DisableLowerBody();

            //  6. LOAD HEAD EQUIPMENT MODELS
            if (!ApplyModularPart(BodySlot.Legs, equipment))
            {
                foreach (var model in equipment.equipmentModels)
                {
                    model.LoadModel(player, player.playerNetworkManager.isMale.Value);
                }
            }

            //  7. CALCULATE TOTAL EQUIPMENT LOAD (WEIGHT OF ALL YOUR WORN EQUIPMENT. THIS IMPACTS ROLL SPEED AND AT EXTREME WEIGHTS, MOVEMENT SPEED)

            //  8. CALCULATE TOTAL ARMOR ABSORPTION
            player.playerStatsManager.CalculateTotalArmorAbsorption();

            if (player.IsOwner)
                player.playerNetworkManager.legEquipmentID.Value = equipment.itemID;
        }

        private void UnloadLegEquipmentModels()
        {
            foreach (var model in maleHips)
            {
                model.SetActive(false);
            }

            foreach (var model in femaleHips)
            {
                model.SetActive(false);
            }

            foreach (var model in leftKnee)
            {
                model.SetActive(false);
            }

            foreach (var model in rightKnee)
            {
                model.SetActive(false);
            }

            foreach (var model in maleLeftLegs)
            {
                model.SetActive(false);
            }

            foreach (var model in maleRightLegs)
            {
                model.SetActive(false);
            }

            foreach (var model in femaleLeftLegs)
            {
                model.SetActive(false);
            }

            foreach (var model in femaleRightLegs)
            {
                model.SetActive(false);
            }

            player.playerBodyManager.EnableLowerBody();
        }

        public void LoadHandEquipment(HandEquipmentItem equipment)
        {
            //  1. UNLOAD OLD EQUIPMENT MODELS (IF ANY)
            if (!useModularEquipment)
                UnloadHandEquipmentModels();

            //  2. IF EQUIPMENT IS NULL SIMPLY SET EQUIPMENT IN INVENTORY TO NULL AND RETURN
            if (equipment == null)
            {
                if (player.IsOwner)
                    player.playerNetworkManager.handEquipmentID.Value = -1; //  -1 WILL NEVER BE AN ITEM ID, SO IT WILL ALWAYS BE NULL

                player.playerInventoryManager.handEquipment = null;
                ApplyModularPart(BodySlot.Arms, null);
                return;
            }

            //  3. IF YOU HAVE AN "ONITEMEQUIPPED" CALL ON YOUR EQUIPMENT, RUN IT NOW

            //  4. SET CURRENT HEAD EQUIPMENT IN PLAYER INVENTORY TO THE EQUIPMENT THAT IS PASSED TO THIS FUNCTION
            player.playerInventoryManager.handEquipment = equipment;

            //  5. IF YOU NEED TO CHECK FOR HEAD EQUIPMENT TYPE TO DISABLE CERTAIN BODY FEATURES (HOODS DISABLING HAIR ECT, FULL HELMS DISABLING HEADS) DO IT NOW
            player.playerBodyManager.DisableArms();

            //  6. LOAD HEAD EQUIPMENT MODELS
            if (!ApplyModularPart(BodySlot.Arms, equipment))
            {
                foreach (var model in equipment.equipmentModels)
                {
                    model.LoadModel(player, player.playerNetworkManager.isMale.Value);
                }
            }

            //  7. CALCULATE TOTAL EQUIPMENT LOAD (WEIGHT OF ALL YOUR WORN EQUIPMENT. THIS IMPACTS ROLL SPEED AND AT EXTREME WEIGHTS, MOVEMENT SPEED)

            //  8. CALCULATE TOTAL ARMOR ABSORPTION
            player.playerStatsManager.CalculateTotalArmorAbsorption();

            if (player.IsOwner)
                player.playerNetworkManager.handEquipmentID.Value = equipment.itemID;
        }

        private void UnloadHandEquipmentModels()
        {
            foreach (var model in maleLeftLowerArms)
            {
                model.SetActive(false);
            }

            foreach (var model in maleRightLowerArms)
            {
                model.SetActive(false);
            }

            foreach (var model in femaleLeftLowerArms)
            {
                model.SetActive(false);
            }

            foreach (var model in femaleRightLowerArms)
            {
                model.SetActive(false);
            }

            foreach (var model in maleLeftHands)
            {
                model.SetActive(false);
            }

            foreach (var model in maleRightHands)
            {
                model.SetActive(false);
            }

            foreach (var model in femaleLeftHands)
            {
                model.SetActive(false);
            }

            foreach (var model in femaleRightHands)
            {
                model.SetActive(false);
            }

            player.playerBodyManager.EnableArms();
        }

        //  PROJECTILES
        public void LoadMainProjectileEquipment(RangedProjectileItem equipment)
        {
            //  1. IF EQUIPMENT IS NULL SIMPLY SET EQUIPMENT IN INVENTORY TO NULL AND RETURN
            if (equipment == null)
            {
                if (player.IsOwner)
                    player.playerNetworkManager.mainProjectileID.Value = -1; //  -1 WILL NEVER BE AN ITEM ID, SO IT WILL ALWAYS BE NULL

                player.playerInventoryManager.mainProjectile = null;
                return;
            }

            //  2. IF YOU HAVE AN "ONITEMEQUIPPED" CALL ON YOUR EQUIPMENT, RUN IT NOW

            //  3. SET CURRENT PROJECTILE EQUIPMENT IN PLAYER INVENTORY TO THE EQUIPMENT THAT IS PASSED TO THIS FUNCTION
            player.playerInventoryManager.mainProjectile = equipment;

            if (player.IsOwner)
                player.playerNetworkManager.mainProjectileID.Value = equipment.itemID;
        }

        public void LoadSecondaryProjectileEquipment(RangedProjectileItem equipment)
        {
            //  1. IF EQUIPMENT IS NULL SIMPLY SET EQUIPMENT IN INVENTORY TO NULL AND RETURN
            if (equipment == null)
            {
                if (player.IsOwner)
                    player.playerNetworkManager.secondaryProjectileID.Value = -1; //  -1 WILL NEVER BE AN ITEM ID, SO IT WILL ALWAYS BE NULL

                player.playerInventoryManager.secondaryProjectile = null;
                return;
            }

            //  2. IF YOU HAVE AN "ONITEMEQUIPPED" CALL ON YOUR EQUIPMENT, RUN IT NOW

            //  3. SET CURRENT PROJECTILE EQUIPMENT IN PLAYER INVENTORY TO THE EQUIPMENT THAT IS PASSED TO THIS FUNCTION
            player.playerInventoryManager.secondaryProjectile = equipment;

            if (player.IsOwner)
                player.playerNetworkManager.secondaryProjectileID.Value = equipment.itemID;
        }

        //  QUICK SLOT
        public void LoadQuickSlotEquipment(QuickSlotItem equipment)
        {
            //  1. IF EQUIPMENT IS NULL SIMPLY SET EQUIPMENT IN INVENTORY TO NULL AND RETURN
            if (equipment == null)
            {
                if (player.IsOwner)
                    player.playerNetworkManager.currentQuickSlotItemID.Value = -1; //  -1 WILL NEVER BE AN ITEM ID, SO IT WILL ALWAYS BE NULL

                player.playerInventoryManager.currentQuickSlotItem = null;
                return;
            }

            //  2. IF YOU HAVE AN "ONITEMEQUIPPED" CALL ON YOUR EQUIPMENT, RUN IT NOW

            //  3. SET CURRENT PROJECTILE EQUIPMENT IN PLAYER INVENTORY TO THE EQUIPMENT THAT IS PASSED TO THIS FUNCTION
            player.playerInventoryManager.currentQuickSlotItem = equipment;

            if (player.IsOwner)
                player.playerNetworkManager.currentQuickSlotItemID.Value = equipment.itemID;
        }

        //  WEAPONS
        private void InitializeWeaponSlots()
        {
            //  ER 数据驱动挂载：先扫描骨架里的 dummy 挂点，供按 WepAbsorpPosParam 的 refID 查表挂载。
            BuildErDummyCache();

            //  ER 忠实复刻：从 c0000.flver 的 dummy 数据 + 骨架自标定求解 FLVER→Unity 基变换。
            //  此刻(Awake)动画未生效 = 绑定姿势，标定准确。
            if (erMountData == null)
                erMountData = Resources.Load<ERMountData>("ERMountData");
            erMounter.Calibrate(transform, erMountData);

            //  权威：优先绑定 c0000 共享骨架的 ER 挂点骨(R_Weapon/L_Weapon/L_Shield)。
            //  必须先于组件扫描，否则会命中旧骨架下残留的 "* Weapon Slot" 物体，把武器挂到旧骨骼上。
            //  握持对齐由 WeaponModelInstantiationSlot 的“蒙皮骨对齐”负责（让武器根骨与挂点重合）。
            rightHandWeaponSlot = EnsureWeaponSlotOnBone("R_Weapon", WeaponModelSlot.RightHand);
            leftHandWeaponSlot = EnsureWeaponSlotOnBone("L_Weapon", WeaponModelSlot.LeftHandWeaponSlot);
            leftHandShieldSlot = EnsureWeaponSlotOnBone("L_Shield", WeaponModelSlot.LeftHandShieldSlot);

            //  回退：ER 骨名缺失（旧骨架/其它角色）或 BackSlot 无对应 dummy 时，用已存在组件补齐仍为空的槽。
            WeaponModelInstantiationSlot[] weaponSlots = GetComponentsInChildren<WeaponModelInstantiationSlot>(true);
            foreach (var weaponSlot in weaponSlots)
            {
                switch (weaponSlot.weaponSlot)
                {
                    case WeaponModelSlot.RightHand:
                        if (rightHandWeaponSlot == null) rightHandWeaponSlot = weaponSlot;
                        break;
                    case WeaponModelSlot.LeftHandWeaponSlot:
                        if (leftHandWeaponSlot == null) leftHandWeaponSlot = weaponSlot;
                        break;
                    case WeaponModelSlot.LeftHandShieldSlot:
                        if (leftHandShieldSlot == null) leftHandShieldSlot = weaponSlot;
                        break;
                    case WeaponModelSlot.BackSlot:
                        if (backSlot == null) backSlot = weaponSlot;
                        break;
                }
            }
        }

        /// <summary>
        /// 在角色骨架里按精确骨名查找挂点骨，并确保其上有 <see cref="WeaponModelInstantiationSlot"/> 组件。
        /// 找不到骨返回 null（由调用方的空安全防护处理）。
        /// </summary>
        private WeaponModelInstantiationSlot EnsureWeaponSlotOnBone(string boneName, WeaponModelSlot slotType)
        {
            Transform bone = FindDescendantByExactName(transform, boneName);
            if (bone == null)
            {
                Debug.LogWarning($"[PlayerEquipmentManager] 骨架下找不到挂点骨 '{boneName}'，{slotType} 槽位未创建。", this);
                return null;
            }

            var slot = bone.GetComponent<WeaponModelInstantiationSlot>();
            if (slot == null)
            {
                slot = bone.gameObject.AddComponent<WeaponModelInstantiationSlot>();
                slot.weaponSlot = slotType;
            }
            return slot;
        }

        /// <summary>深度优先在子层级里精确匹配骨名（避开 'Ctrl L_Weapon' 等同名前缀的控制骨）。</summary>
        private static Transform FindDescendantByExactName(Transform root, string exactName)
        {
            foreach (Transform child in root)
            {
                if (child.name == exactName) return child;
                Transform found = FindDescendantByExactName(child, exactName);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// 扫描骨架层级，把所有 'c0000 Dummy&lt;n&gt; [refID]' 挂点按 refID 建索引。
        /// 这些 dummy 在 Blender 里已骨父子到对应骨，会跟动画走，是 ER/DSAS 挂武器的正确挂点。
        /// </summary>
        private void BuildErDummyCache()
        {
            erDummyByRefId.Clear();
            erDummyBindWorld.Clear();
            erDummyAttachBone.Clear();
            erAttachBindWorld.Clear();

            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                var m = DummyRefIdRegex.Match(t.name);
                if (!m.Success) continue;
                if (t.name.IndexOf("Dummy", System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                int refId = int.Parse(m.Groups[1].Value);
                //  同一 refID 可能有多个（少数身体 dummy）；武器握持点唯一，取首个即可。
                if (erDummyByRefId.ContainsKey(refId)) continue;

                erDummyByRefId[refId] = t;

                //  绑定姿势世界矩阵：BuildErDummyCache 在 Awake 调用，此刻动画未生效 → 即 bind。
                erDummyBindWorld[refId] = t.localToWorldMatrix;

                //  跟随骨 = dummy 在 Unity 层级里的父骨（FBX 骨父 = FLVER AttachBoneIndex）。
                Transform attach = t.parent;
                if (attach != null)
                {
                    erDummyAttachBone[refId] = attach;
                    erAttachBindWorld[refId] = attach.localToWorldMatrix;
                }
            }

            //  空间骨（Model_Dmy_AttachWeapon）及其绑定世界矩阵。
            erWeaponSpaceBone = FindDescendantByExactName(transform, erWeaponSpaceBoneName);
            if (erWeaponSpaceBone != null)
            {
                erWeaponSpaceBoneBind = erWeaponSpaceBone.localToWorldMatrix;
                erWeaponSpaceBoneBindCaptured = true;
            }
            else
            {
                erWeaponSpaceBoneBindCaptured = false;
                Debug.LogWarning(
                    $"[PlayerEquipmentManager] 骨架下找不到 ER 武器空间骨 '{erWeaponSpaceBoneName}'。" +
                    "dummy 双骨挂载将回退为跟随骨刚性挂载（不含空间骨动画修正）。", this);
            }
        }

        /// <summary>按 refID 取骨架里的 dummy 挂点；找不到或 refID&lt;0 返回 null。</summary>
        private Transform ResolveErDummy(int refId)
        {
            if (refId < 0) return null;
            if (erDummyByRefId.Count == 0) BuildErDummyCache();
            return erDummyByRefId.TryGetValue(refId, out var t) ? t : null;
        }

        /// <summary>
        /// 按 ER/DSAS 方式挂载"持握中"的武器：依当前姿态(单/双手)从 WeaponItem 取 dummy refID，
        /// 在骨架里解析到对应挂点后挂上并叠加 180° 翻转（<see cref="WeaponModelInstantiationSlot.PlaceWeaponModelOnMount"/>）。
        /// refID 为 -1 或骨架里查不到该 dummy 时，回退到旧的挂点骨挂载（<see cref="WeaponModelInstantiationSlot.PlaceWeaponModelIntoSlot"/>）。
        /// </summary>
        private void MountHeldWeapon(WeaponModelInstantiationSlot slot, GameObject model, WeaponItem weapon, bool isLeftHand, bool? twoHandingOverride = null)
        {
            if (slot == null || model == null || weapon == null) return;

            //  【剑刃】刚性挂到 R_Weapon/L_Weapon 挂点骨（已验证能正确握手）。
            //  注：ER 的剑刃 dummy(ref20) 在 FLVER 参考姿势里离 R_Weapon 约 1.4m，靠“动画把 R_Weapon 从
            //  参考姿势移开”才带回手里；该机制要求 Unity 绑定姿势==FLVER 参考姿势且动画增量一致，Unity 里不成立，
            //  故剑刃不走 dummy-跟随-增量 路径（会悬浮），仍用刚性挂载 + baseRotationCorrection。
            slot.PlaceWeaponModelIntoSlot(
                model,
                weapon.weaponModelPositionOffset,
                weapon.weaponModelRotationOffset,
                weapon.weaponModelScale);
        }

        /// <summary>
        /// ER 剑鞘挂载 —— 与剑刃【完全同一套 DSAS 机制】(见 <see cref="WeaponModelInstantiationSlot.PlaceModelOnAttachBone"/>)：
        /// 剑鞘在 ER 里是武器 model index 1，由 WepAbsorpPosParam 定位。长剑(wepAbsorpPosId=23) model1 = dummy 2030，
        /// 因 2030/1000=2 在 ER 落到 body 回退 → 角色 dummy 30；c0000.flver 实测 dummy30 的 attach_bone = <c>Pelvis</c>、
        /// 世界 Z=-0.12(左侧) → 直剑鞘收于左腰。故剑鞘= 挂到 dummy30 的 attach 骨 Pelvis + 常量翻转，与剑刃(挂 R_Weapon)一致。
        ///
        /// 实现细节(Unity 特有)：鞘(WP_A_0200_1)是蒙皮网格且与剑刃同处一个 FBX，无法只 reparent 网格，
        /// 故：①手持模型隐藏鞘 renderer；②另实例化一份只显示鞘、剥掉伤害/碰撞的副本，用同一挂载核心挂到 Pelvis。
        /// 这对应 DSAS 把 model0/model1 作为两个独立模型分别放到各自 dummy 的做法。返回鞘实例(无鞘/失败返回 null)。
        /// </summary>
        private GameObject MountSheath(WeaponItem weapon, GameObject handModel)
        {
            if (weapon == null || handModel == null) return null;
            if (string.IsNullOrEmpty(weapon.sheathRendererName)) return null;

            //  仅隐藏手持模型上的鞘 renderer（不再复制一份挂到 Pelvis）。
            foreach (var r in handModel.GetComponentsInChildren<Renderer>(true))
            {
                if (r.name.IndexOf(weapon.sheathRendererName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    r.enabled = false;
            }
            return null;
        }

        public void EquipWeapons()
        {
            LoadRightWeapon();
            LoadLeftWeapon();
        }

        // 右手武器
        public void SwitchRightWeapon()
        {
            if (!player.IsOwner)
                return;

            player.playerNetworkManager.isTwoHandingWeapon.Value = false;

            // 优先 ER a000_029001（§8.7 P3，变体范围 290xx 视武器细分，先收基址），回退旧 swapRightWeapon。
            AnimationClip swapRightClip = player.playerAnimatorManager.ResolveCommonActionClip(c => c.weaponSwapRightBase)
                                          ?? (player.playerAnimatorManager.animData != null ? player.playerAnimatorManager.animData.swapRightWeapon : null);
            if (swapRightClip != null)
                player.playerAnimatorManager.PlayTargetUpperbodyAnimation(swapRightClip);
            else
                Debug.LogWarning($"{player.name}: swapRightWeapon clip 未配置", player);
            
            // 艾尔登法环武器切换
            // 1. 检查我们是否有除了主武器以外的其他武器，如果有，永远不要切换到空手，而是在武器1和2之间切换
            // 2. 如果没有其他武器，切换到空手，然后跳过另一个空的槽位并切换回来。在返回主武器之前，不要处理两个空槽位。

            WeaponItem selectedWeapon = null;
            
            // 如果我们正在双持武器，则禁用双持
            
            // 检查我们的武器索引（我们有3个槽位，所以有3个可能的数字）
            // 将索引加一以切换到下一个可能的武器
            player.playerInventoryManager.rightHandWeaponIndex += 1;

            // 如果索引超出边界，就回到位置1（0）
            if (player.playerInventoryManager.rightHandWeaponIndex < 0 || player.playerInventoryManager.rightHandWeaponIndex > 2)
            {
                player.playerInventoryManager.rightHandWeaponIndex = 0;
                
                // 我们检查是否持有不止一件武器
                float weaponCount = 0;
                WeaponItem firstWeapon = null;
                int firstWeaponPosition = 0;

                for (int i = 0; i < player.playerInventoryManager.weaponsInRightHandSlots.Length; i++)
                {
                    if (player.playerInventoryManager.weaponsInRightHandSlots[i].itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                    {
                        weaponCount += 1;

                        if (firstWeapon == null)
                        {
                            firstWeapon = player.playerInventoryManager.weaponsInRightHandSlots[i];
                            firstWeaponPosition = i;
                        }
                    }
                }

                if (weaponCount <= 1)
                {
                    player.playerInventoryManager.rightHandWeaponIndex = -1;
                    selectedWeapon = WorldItemDatabase.Instance.unarmedWeapon;
                    player.playerInventoryManager.currentRightHandWeapon = selectedWeapon;
                    player.playerNetworkManager.currentRightHandWeaponID.Value = selectedWeapon.itemID;
                }
                else
                {
                    player.playerInventoryManager.rightHandWeaponIndex = firstWeaponPosition;
                    player.playerInventoryManager.currentRightHandWeapon = selectedWeapon;
                    player.playerNetworkManager.currentRightHandWeaponID.Value = firstWeapon.itemID;
                }

                return;
            }

            foreach (WeaponItem weapon in player.playerInventoryManager.weaponsInRightHandSlots)
            {
                //  IF THE NEXT POTENTIAL WEAPON DOES NOT EQUAL THE UNARMED WEAPON
                if (player.playerInventoryManager.weaponsInRightHandSlots[player.playerInventoryManager.rightHandWeaponIndex].itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                {
                    selectedWeapon = player.playerInventoryManager.weaponsInRightHandSlots[player.playerInventoryManager.rightHandWeaponIndex];
                    //  ASSIGN THE NETWORK WEAPON ID SO IT SWITCHES FOR ALL CONNECTED CLIENTS
                    player.playerInventoryManager.currentRightHandWeapon = selectedWeapon;
                    player.playerNetworkManager.currentRightHandWeaponID.Value = selectedWeapon.itemID;
                    return;
                }
            }

            if (selectedWeapon == null && player.playerInventoryManager.rightHandWeaponIndex <= 2)
            {
                SwitchRightWeapon();
            }
        }
        
        public void LoadRightWeapon()
        {
            //  临时防护：切换到 c0000 骨架后若右手挂载槽缺失，跳过武器加载以免空引用打断后续装备加载
            if (rightHandWeaponSlot == null)
            {
                Debug.LogWarning("[PlayerEquipmentManager] rightHandWeaponSlot 为空，跳过右手武器加载。请检查角色骨架下是否存在 RightHand 的 WeaponModelInstantiationSlot。", this);
                return;
            }

            if (player.playerInventoryManager.currentRightHandWeapon != null)
            {
                // 移除旧武器
                rightHandWeaponSlot.UnloadWeapon();
                if (rightHandSheathModel != null) { Destroy(rightHandSheathModel); rightHandSheathModel = null; }
                
                // 加载新武器
                WeaponItem rightWeapon = player.playerInventoryManager.currentRightHandWeapon;
                rightHandWeaponModel = Instantiate(rightWeapon.weaponModel);
                MountHeldWeapon(rightHandWeaponSlot, rightHandWeaponModel, rightWeapon, isLeftHand: false);
                rightHandSheathModel = MountSheath(rightWeapon, rightHandWeaponModel);
                rightWeaponManager = rightHandWeaponModel.GetComponent<WeaponManager>();
                //  ER WP 模型若忘挂 WeaponManager，这里兜底避免空引用（伤害碰撞体仍需在预制体上配好）。
                if (rightWeaponManager == null)
                {
                    Debug.LogWarning($"[PlayerEquipmentManager] 右手武器 '{rightWeapon.name}' 的模型缺少 WeaponManager 组件，已临时补上。请在武器预制体上配好 WeaponManager + MeleeWeaponDamageCollider。", rightHandWeaponModel);
                    rightWeaponManager = rightHandWeaponModel.AddComponent<WeaponManager>();
                }
                rightWeaponManager.SetWeaponDamage(player, rightWeapon);
                player.playerAnimatorManager.SetActiveWeaponAnimationSet(player.playerInventoryManager.currentRightHandWeapon.weaponAnimationSet);
                // 按角色作用域注册当前武器 moveset 的攻击 clip（owner + 远端都会执行到此，
                // 由复制的 currentRightHandWeaponID 驱动）→ FSM 选片与远端 RPC 解析都能命中，不依赖全局表。
                player.playerAnimatorManager.RegisterMoveset(player.playerInventoryManager.currentRightHandWeapon.moveset);
            }
        }

        // 左手武器
        public void SwitchLeftWeapon()
        {
            if (!player.IsOwner)
                return;

            player.playerNetworkManager.isTwoHandingWeapon.Value = false;

            // 优先 ER a000_029031（§8.7 P3），回退旧 swapLeftWeapon。
            AnimationClip swapLeftClip = player.playerAnimatorManager.ResolveCommonActionClip(c => c.weaponSwapLeftBase)
                                         ?? (player.playerAnimatorManager.animData != null ? player.playerAnimatorManager.animData.swapLeftWeapon : null);
            if (swapLeftClip != null)
                player.playerAnimatorManager.PlayTargetUpperbodyAnimation(swapLeftClip);
            else
                Debug.LogWarning($"{player.name}: swapLeftWeapon clip 未配置", player);
            
            // 艾尔登法环武器切换
            // 1. 检查我们是否有除了主武器以外的其他武器，如果有，永远不要切换到空手，而是在武器1和2之间切换
            // 2. 如果没有其他武器，切换到空手，然后跳过另一个空的槽位并切换回来。在返回主武器之前，不要处理两个空槽位。

            WeaponItem selectedWeapon = null;
            
            // 如果我们正在双持武器，则禁用双持
            
            // 检查我们的武器索引（我们有3个槽位，所以有3个可能的数字）
            // 将索引加一以切换到下一个可能的武器
            player.playerInventoryManager.leftHandWeaponIndex += 1;

            // 如果索引超出边界，就回到位置1（0）
            if (player.playerInventoryManager.leftHandWeaponIndex < 0 || player.playerInventoryManager.leftHandWeaponIndex > 2)
            {
                player.playerInventoryManager.leftHandWeaponIndex = 0;
                
                // 我们检查是否持有不止一件武器
                float weaponCount = 0;
                WeaponItem firstWeapon = null;
                int firstWeaponPosition = 0;

                for (int i = 0; i < player.playerInventoryManager.weaponsInLeftHandSlots.Length; i++)
                {
                    if (player.playerInventoryManager.weaponsInLeftHandSlots[i].itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                    {
                        weaponCount += 1;

                        if (firstWeapon == null)
                        {
                            firstWeapon = player.playerInventoryManager.weaponsInLeftHandSlots[i];
                            firstWeaponPosition = i;
                        }
                    }
                }

                if (weaponCount <= 1)
                {
                    player.playerInventoryManager.leftHandWeaponIndex = -1;
                    selectedWeapon = WorldItemDatabase.Instance.unarmedWeapon;
                    player.playerInventoryManager.currentLeftHandWeapon = selectedWeapon;
                    player.playerNetworkManager.currentLeftHandWeaponID.Value = selectedWeapon.itemID;
                }
                else
                {
                    player.playerInventoryManager.leftHandWeaponIndex = firstWeaponPosition;
                    player.playerInventoryManager.currentLeftHandWeapon = selectedWeapon;
                    player.playerNetworkManager.currentLeftHandWeaponID.Value = firstWeapon.itemID;
                }

                return;
            }

            foreach (WeaponItem weapon in player.playerInventoryManager.weaponsInLeftHandSlots)
            {
                // 检查看看这是不是“非武装”武器
                // 如果下一个可能的武器不是空手
                if (player.playerInventoryManager.weaponsInLeftHandSlots[player.playerInventoryManager.leftHandWeaponIndex].itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                {
                    selectedWeapon = player.playerInventoryManager.weaponsInLeftHandSlots[player.playerInventoryManager.leftHandWeaponIndex];
                    //  ASSIGN THE NETWORK WEAPON ID SO IT SWITCHES FOR ALL CONNECTED CLIENTS
                    player.playerInventoryManager.currentLeftHandWeapon = selectedWeapon;
                    player.playerNetworkManager.currentLeftHandWeaponID.Value = selectedWeapon.itemID;
                    return;
                }
            }

            if (selectedWeapon == null && player.playerInventoryManager.leftHandWeaponIndex <= 2)
            {
                SwitchLeftWeapon();
            }
        }
        
        public void LoadLeftWeapon()
        {
            //  临时防护：切换到 c0000 骨架后若左手挂载槽缺失，跳过武器加载以免空引用打断后续装备加载
            if (leftHandWeaponSlot == null || leftHandShieldSlot == null)
            {
                Debug.LogWarning("[PlayerEquipmentManager] leftHandWeaponSlot/leftHandShieldSlot 为空，跳过左手武器加载。请检查角色骨架下是否存在对应的 WeaponModelInstantiationSlot。", this);
                return;
            }

            if (player.playerInventoryManager.currentLeftHandWeapon != null)
            {
                // 移除旧武器
                if (leftHandWeaponSlot.currentWeaponModel != null)
                    leftHandWeaponSlot.UnloadWeapon();

                if (leftHandShieldSlot.currentWeaponModel != null)
                    leftHandShieldSlot.UnloadWeapon();

                // 加载新武器
                WeaponItem leftWeapon = player.playerInventoryManager.currentLeftHandWeapon;
                leftHandWeaponModel = Instantiate(leftWeapon.weaponModel);

                switch (leftWeapon.weaponModelType)
                {
                    case WeaponModelType.Weapon:
                        MountHeldWeapon(leftHandWeaponSlot, leftHandWeaponModel, leftWeapon, isLeftHand: true);
                        break;
                    case WeaponModelType.Shield:
                        MountHeldWeapon(leftHandShieldSlot, leftHandWeaponModel, leftWeapon, isLeftHand: true);
                        break;
                    default:
                        break;
                }

                leftWeaponManager = leftHandWeaponModel.GetComponent<WeaponManager>();
                //  ER WP 模型若忘挂 WeaponManager，这里兜底避免空引用（伤害碰撞体仍需在预制体上配好）。
                if (leftWeaponManager == null)
                {
                    Debug.LogWarning($"[PlayerEquipmentManager] 左手武器 '{leftWeapon.name}' 的模型缺少 WeaponManager 组件，已临时补上。请在武器预制体上配好 WeaponManager + MeleeWeaponDamageCollider。", leftHandWeaponModel);
                    leftWeaponManager = leftHandWeaponModel.AddComponent<WeaponManager>();
                }
                leftWeaponManager.SetWeaponDamage(player, leftWeapon);
            }
        }

        //  TWO HAND
        public void UnTwoHandWeapon()
        {
            player.playerAnimatorManager.SetActiveWeaponAnimationSet(player.playerInventoryManager.currentRightHandWeapon.weaponAnimationSet);

            //  REMOVE THE STRENGTH BONUS (TWO HANDING A WEAPON MAKES YOUR STRENGTH LEVEL (STRENGTH + (STRENGTH * 0.5))

            //  UN-TWO HAND THE MODEL AND MOVE THE MODEL THAT ISNT BEING TWO HANDED BACK TO ITS HAND (IF THERE IS ANY)

            //  LEFT HAND（回到左手，单手姿态）
            if (player.playerInventoryManager.currentLeftHandWeapon.weaponModelType == WeaponModelType.Weapon)
            {
                MountHeldWeapon(leftHandWeaponSlot, leftHandWeaponModel, player.playerInventoryManager.currentLeftHandWeapon, isLeftHand: true, twoHandingOverride: false);
            }
            else if (player.playerInventoryManager.currentLeftHandWeapon.weaponModelType == WeaponModelType.Shield)
            {
                MountHeldWeapon(leftHandShieldSlot, leftHandWeaponModel, player.playerInventoryManager.currentLeftHandWeapon, isLeftHand: true, twoHandingOverride: false);
            }

            //  RIGHT HAND（回到右手，单手姿态）
            MountHeldWeapon(rightHandWeaponSlot, rightHandWeaponModel, player.playerInventoryManager.currentRightHandWeapon, isLeftHand: false, twoHandingOverride: false);

            //  REFRESH THE DAMAGE COLLIDER CALCULATIONS (STRENGTH SCALING WOULD BE EFFECTED SINCE THE STRENGTH BONUS WAS REMOVED)
            rightWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentRightHandWeapon);
            leftWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentLeftHandWeapon);
        }

        public void TwoHandRightWeapon()
        {
            // CHECK FOR UNTWOHANDABLE ITEM (Like unarmed) IF WE ARE ATTEMPTING TO TWO HAND UNARMED, RETURN
            if (player.playerInventoryManager.currentRightHandWeapon == WorldItemDatabase.Instance.unarmedWeapon)
            {
                // IF WE ARE RETURNING AND NOT TWO HANDING THE WEAPON, RESET BOOL STATUS'S
                if (player.IsOwner)
                {
                    player.playerNetworkManager.isTwoHandingRightWeapon.Value = false;
                    player.playerNetworkManager.isTwoHandingWeapon.Value = false;
                }

                return;
            }

            player.playerAnimatorManager.SetActiveWeaponAnimationSet(player.playerInventoryManager.currentRightHandWeapon.weaponAnimationSet);

            // PLACE THE NON-TWO HANDED WEAPON MODEL IN THE BACK SLOT OR HIP SLOT
            backSlot.PlaceWeaponModelInUnequippedSlot(leftHandWeaponModel, player.playerInventoryManager.currentLeftHandWeapon.weaponClass, player);

            // ADD TWO HAND STRENGTH BONUS

            // PLACE THE TWO HANDED WEAPON MODEL IN THE MAIN (RIGHT HAND)，双手姿态 → both_0 dummy
            MountHeldWeapon(rightHandWeaponSlot, rightHandWeaponModel, player.playerInventoryManager.currentRightHandWeapon, isLeftHand: false, twoHandingOverride: true);

            rightWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentRightHandWeapon);
            leftWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentLeftHandWeapon);
        }

        public void TwoHandLeftWeapon()
        {
            // CHECK FOR UNTWOHANDABLE ITEM (Like unarmed) IF WE ARE ATTEMPTING TO TWO HAND UNARMED, RETURN
            if (player.playerInventoryManager.currentLeftHandWeapon == WorldItemDatabase.Instance.unarmedWeapon)
            {
                // IF WE ARE RETURNING AND NOT TWO HANDING THE WEAPON, RESET BOOL STATUS'S
                if (player.IsOwner)
                {
                    player.playerNetworkManager.isTwoHandingLeftWeapon.Value = false;
                    player.playerNetworkManager.isTwoHandingWeapon.Value = false;
                }

                return;
            }

            player.playerAnimatorManager.SetActiveWeaponAnimationSet(player.playerInventoryManager.currentLeftHandWeapon.weaponAnimationSet);

            // PLACE THE NON-TWO HANDED WEAPON MODEL IN THE BACK SLOT OR HIP SLOT
            backSlot.PlaceWeaponModelInUnequippedSlot(rightHandWeaponModel, player.playerInventoryManager.currentRightHandWeapon.weaponClass, player);

            // ADD TWO HAND STRENGTH BONUS

            // PLACE THE TWO HANDED WEAPON MODEL IN THE MAIN (RIGHT HAND)，双手姿态 → both_0 dummy
            MountHeldWeapon(rightHandWeaponSlot, leftHandWeaponModel, player.playerInventoryManager.currentLeftHandWeapon, isLeftHand: false, twoHandingOverride: true);

            rightWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentRightHandWeapon);
            leftWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentLeftHandWeapon);
        }

        //  DAMAGE COLLIDERS
        public void OpenDamageCollider()
        {
            // 打开右手武器伤害碰撞体
            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                rightWeaponManager.meleeDamageCollider.EnableDamageCollider();
                player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(player.playerInventoryManager.currentRightHandWeapon.whooshes));
            }
            // 打开左手武器伤害碰撞体
            else if (player.playerNetworkManager.isUsingLeftHand.Value)
            {
                leftWeaponManager.meleeDamageCollider.EnableDamageCollider();
                player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(player.playerInventoryManager.currentLeftHandWeapon.whooshes));
            }
            
            // 播放音效
        }
        
        public void CloseDamageCollider()
        {
            //  OPEN RIGHT WEAPON DAMAGE COLLIDER
            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                rightWeaponManager.meleeDamageCollider.DisableDamageCollider();
            }
            //  OPEN LEFT WEAPON DAMAGE COLLIDER
            else if (player.playerNetworkManager.isUsingLeftHand.Value)
            {
                leftWeaponManager.meleeDamageCollider.DisableDamageCollider();
            }
        }

        public void OpenMainHandDamageCollider()
        {
            rightWeaponManager.meleeDamageCollider.EnableDamageCollider();
            player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(player.playerInventoryManager.currentRightHandWeapon.whooshes));
            //  PLAY WHOOSH SFX
        }

        public void CloseMainHandDamageCollider()
        {
            rightWeaponManager.meleeDamageCollider.DisableDamageCollider();
        }

        public void OpenOffHandDamageCollider()
        {
            leftWeaponManager.meleeDamageCollider.EnableDamageCollider();
            player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(player.playerInventoryManager.currentLeftHandWeapon.whooshes));
            //  PLAY WHOOSH SFX
        }

        public void CloseOffHandDamageCollider()
        {
            leftWeaponManager.meleeDamageCollider.DisableDamageCollider();
        }

        //  UNHIDE WEAPONS
        public void UnHideWeapons()
        {
            if (player.playerEquipmentManager.rightHandWeaponModel != null)
                player.playerEquipmentManager.rightHandWeaponModel.SetActive(true);

            if (player.playerEquipmentManager.leftHandWeaponModel != null)
                player.playerEquipmentManager.leftHandWeaponModel.SetActive(true);
        }
    }
}