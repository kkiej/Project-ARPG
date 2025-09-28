using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class PlayerEquipmentManager : CharacterEquipmentManager
    {
        PlayerManager player;

        [Header("Weapon Model Instantiation Slots")]
        public WeaponModelInstantiationSlot rightHandWeaponSlot;
        public WeaponModelInstantiationSlot leftHandWeaponSlot;
        public WeaponModelInstantiationSlot leftHandShieldSlot;
        public WeaponModelInstantiationSlot backSlot;

        [Header("Weapon Models")]
        public GameObject rightHandWeaponModel;
        public GameObject leftHandWeaponModel;

        [Header("Weapon Managers")]
        [SerializeField] WeaponManager rightWeaponManager;
        [SerializeField] WeaponManager leftWeaponManager;

        [Header("DEBUG DELETE LATER")]
        [SerializeField] bool equipNewItems = false;

        [Header("Male Equipment Models")]
        public GameObject maleFullHelmetObject;
        public GameObject[] maleHeadFullHelmets;

        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
            
            InitializeWeaponSlots();

            List<GameObject> maleFullHelmetsList = new List<GameObject>();

            foreach (Transform child in maleFullHelmetObject.transform)
            {
                maleFullHelmetsList.Add(child.gameObject);
            }

            maleHeadFullHelmets = maleFullHelmetsList.ToArray();
        }

        protected override void Start()
        {
            base.Start();
            
            LoadWeaponsOnBothHands();
        }
        
        private void Update()
        {
            if (equipNewItems)
            {
                equipNewItems = false;
                DebugEquipNewItems();
            }
        }

        private void DebugEquipNewItems()
        {
            Debug.Log("EQUIPPING NEW ITEMS");

            LoadHeadEquipment(player.playerInventoryManager.headEquipment);

            if (player.playerInventoryManager.bodyEquipment != null)
                LoadBodyEquipment(player.playerInventoryManager.bodyEquipment);

            if (player.playerInventoryManager.legEquipment != null)
                LoadLegEquipment(player.playerInventoryManager.legEquipment);

            if (player.playerInventoryManager.handEquipment != null)
                LoadHandEquipment(player.playerInventoryManager.handEquipment);
        }
        
        //  EQUIPMENT
        public void LoadHeadEquipment(HeadEquipmentItem equipment)
        {
            // 1. 卸载旧的头部装备模型（如存在）
            UnloadHeadEquipmentModels();
            
            // 2. 若装备为空，则直接将库存中的装备设为空并返回
            if (equipment == null)
            {
                if (player.IsOwner)
                    player.playerNetworkManager.headEquipmentID.Value = -1; //  -1 WILL NEVER BE AN ITEM ID, SO IT WILL ALWAYS BE NULL

                player.playerInventoryManager.headEquipment = null;
                return;
            }
            
            // 3. 若装备具有"OnItemEquipped"回调函数，立即执行
            
            // 4. 将传入此函数的装备设为玩家库存中的当前头部装备
            player.playerInventoryManager.headEquipment = equipment;
            
            // 5. 如需根据头部装备类型禁用特定身体特征（如头罩禁用头发，全覆式头盔禁用头部模型），在此处执行检查
            // 6. 加载头部装备模型
            foreach (var model in equipment.equipmentModels)
            {
                model.LoadModel(player, true);
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

            //  RE-ENABLE HEAD
            //  RE-ENABLE HAIR
        }

        public void LoadBodyEquipment(BodyEquipmentItem equipment)
        {
            // 1. 卸载旧装备模型（如存在）
            // 2. 若装备为空，则直接将库存中的对应装备设为空并返回
            // 3. 若装备具有"OnItemEquipped"回调函数，立即执行
            // 4. 将传入的装备设为玩家库存中的当前装备
            // 5. 加载新装备模型
            // 6. 计算总装备负重（所有穿戴装备的重量之和，影响翻滚速度，超重时还会降低移动速度）
            // 7. 计算总护甲伤害吸收率
            player.playerStatsManager.CalculateTotalArmorAbsorption();
        }

        public void LoadLegEquipment(LegEquipmentItem equipment)
        {
            // 1. 卸载旧装备模型（如存在）
            // 2. 若装备为空，则直接将库存中的对应装备设为空并返回
            // 3. 若装备具有"OnItemEquipped"回调函数，立即执行
            // 4. 将传入的装备设为玩家库存中的当前装备
            // 5. 加载新装备模型
            // 6. 计算总装备负重（所有穿戴装备的重量之和，影响翻滚速度，超重时还会降低移动速度）
            // 7. 计算总护甲伤害吸收率
            player.playerStatsManager.CalculateTotalArmorAbsorption();
        }

        public void LoadHandEquipment(HandEquipmentItem equipment)
        {
            // 1. 卸载旧装备模型（如存在）
            // 2. 若装备为空，则直接将库存中的对应装备设为空并返回
            // 3. 若装备具有"OnItemEquipped"回调函数，立即执行
            // 4. 将传入的装备设为玩家库存中的当前装备
            // 5. 加载新装备模型
            // 6. 计算总装备负重（所有穿戴装备的重量之和，影响翻滚速度，超重时还会降低移动速度）
            // 7. 计算总护甲伤害吸收率
            player.playerStatsManager.CalculateTotalArmorAbsorption();
        }

        private void InitializeWeaponSlots()
        {
            WeaponModelInstantiationSlot[] weaponSlots = GetComponentsInChildren<WeaponModelInstantiationSlot>();

            foreach (var weaponSlot in weaponSlots)
            {
                if (weaponSlot.weaponSlot == WeaponModelSlot.RightHand)
                {
                    rightHandWeaponSlot = weaponSlot;
                }
                else if (weaponSlot.weaponSlot == WeaponModelSlot.LeftHandWeaponSlot)
                {
                    leftHandWeaponSlot = weaponSlot;
                }
                else if (weaponSlot.weaponSlot == WeaponModelSlot.LeftHandShieldSlot)
                {
                    leftHandShieldSlot = weaponSlot;
                }
                else if (weaponSlot.weaponSlot == WeaponModelSlot.BackSlot)
                {
                    backSlot = weaponSlot;
                }
            }
        }

        public void LoadWeaponsOnBothHands()
        {
            LoadRightWeapon();
            LoadLeftWeapon();
        }

        // 右手武器
        public void SwitchRightWeapon()
        {
            if (!player.IsOwner)
                return;

            player.playerAnimatorManager.PlayTargetActionAnimation("Swap_Right_Weapon_01", false, false, true, true);
            
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
                    player.playerNetworkManager.currentRightHandWeaponID.Value = selectedWeapon.itemID;
                }
                else
                {
                    player.playerInventoryManager.rightHandWeaponIndex = firstWeaponPosition;
                    player.playerNetworkManager.currentRightHandWeaponID.Value = firstWeapon.itemID;
                }

                return;
            }

            foreach (WeaponItem weapon in player.playerInventoryManager.weaponsInRightHandSlots)
            {
                // 检查看看这是不是“非武装”武器
                // 如果下一个可能的武器不是空手
                if (player.playerInventoryManager.weaponsInRightHandSlots[player.playerInventoryManager.rightHandWeaponIndex].itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                {
                    selectedWeapon =
                        player.playerInventoryManager.weaponsInRightHandSlots[
                            player.playerInventoryManager.rightHandWeaponIndex];
                    // 分配网络武器ID，方便它为所有连接的客户端切换
                    player.playerNetworkManager.currentRightHandWeaponID.Value = player.playerInventoryManager
                        .weaponsInRightHandSlots[player.playerInventoryManager.rightHandWeaponIndex].itemID;
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
            if (player.playerInventoryManager.currentRightHandWeapon != null)
            {
                // 移除旧武器
                rightHandWeaponSlot.UnloadWeapon();
                
                // 加载新武器
                rightHandWeaponModel = Instantiate(player.playerInventoryManager.currentRightHandWeapon.weaponModel);
                rightHandWeaponSlot.PlaceWeaponModelIntoSlot(rightHandWeaponModel);
                rightWeaponManager = rightHandWeaponModel.GetComponent<WeaponManager>();
                rightWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentRightHandWeapon);
                player.playerAnimatorManager.UpdateAnimatorController(player.playerInventoryManager
                    .currentRightHandWeapon.weaponAnimator);
            }
        }

        // 左手武器
        public void SwitchLeftWeapon()
        {
            if (!player.IsOwner)
                return;

            player.playerAnimatorManager.PlayTargetActionAnimation("Swap_Left_Weapon_01", false, false, true, true);
            
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
                    player.playerNetworkManager.currentLeftHandWeaponID.Value = selectedWeapon.itemID;
                }
                else
                {
                    player.playerInventoryManager.leftHandWeaponIndex = firstWeaponPosition;
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
                    selectedWeapon =
                        player.playerInventoryManager.weaponsInLeftHandSlots[
                            player.playerInventoryManager.leftHandWeaponIndex];
                    // 分配网络武器ID，方便它为所有连接的客户端切换
                    player.playerNetworkManager.currentLeftHandWeaponID.Value = player.playerInventoryManager
                        .weaponsInLeftHandSlots[player.playerInventoryManager.leftHandWeaponIndex].itemID;
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
            if (player.playerInventoryManager.currentLeftHandWeapon != null)
            {
                // 移除旧武器
                if (leftHandWeaponSlot.currentWeaponModel != null)
                    leftHandWeaponSlot.UnloadWeapon();

                if (leftHandShieldSlot.currentWeaponModel != null)
                    leftHandShieldSlot.UnloadWeapon();

                // 加载新武器
                leftHandWeaponModel = Instantiate(player.playerInventoryManager.currentLeftHandWeapon.weaponModel);

                switch (player.playerInventoryManager.currentLeftHandWeapon.weaponModelType)
                {
                    case WeaponModelType.Weapon:
                        leftHandWeaponSlot.PlaceWeaponModelIntoSlot(leftHandWeaponModel);
                        break;
                    case WeaponModelType.Shield:
                        leftHandShieldSlot.PlaceWeaponModelIntoSlot(leftHandWeaponModel);
                        break;
                    default:
                        break;
                }

                leftWeaponManager = leftHandWeaponModel.GetComponent<WeaponManager>();
                leftWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentLeftHandWeapon);
            }
        }

        //  TWO HAND
        public void UnTwoHandWeapon()
        {
            //  UPDATE ANIMATOR CONTROLLER TO CURRENT MAIN HAND WEAPON
            player.playerAnimatorManager.UpdateAnimatorController(player.playerInventoryManager.currentRightHandWeapon.weaponAnimator);

            //  REMOVE THE STRENGTH BONUS (TWO HANDING A WEAPON MAKES YOUR STRENGTH LEVEL (STRENGTH + (STRENGTH * 0.5))

            //  UN-TWO HAND THE MODEL AND MOVE THE MODEL THAT ISNT BEING TWO HANDED BACK TO ITS HAND (IF THERE IS ANY)

            //  LEFT HAND
            if (player.playerInventoryManager.currentLeftHandWeapon.weaponModelType == WeaponModelType.Weapon)
            {
                leftHandWeaponSlot.PlaceWeaponModelIntoSlot(leftHandWeaponModel);
            }
            else if (player.playerInventoryManager.currentLeftHandWeapon.weaponModelType == WeaponModelType.Shield)
            {
                leftHandShieldSlot.PlaceWeaponModelIntoSlot(leftHandWeaponModel);
            }

            //  RIGHT HAND
            rightHandWeaponSlot.PlaceWeaponModelIntoSlot(rightHandWeaponModel);

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

            // UPDATE ANIMATOR
            player.playerAnimatorManager.UpdateAnimatorController(player.playerInventoryManager.currentRightHandWeapon.weaponAnimator);

            // PLACE THE NON-TWO HANDED WEAPON MODEL IN THE BACK SLOT OR HIP SLOT
            backSlot.PlaceWeaponModelInUnequippedSlot(leftHandWeaponModel, player.playerInventoryManager.currentLeftHandWeapon.weaponClass, player);

            // ADD TWO HAND STRENGTH BONUS

            // PLACE THE TWO HANDED WEAPON MODEL IN THE MAIN (RIGHT HAND)
            rightHandWeaponSlot.PlaceWeaponModelIntoSlot(rightHandWeaponModel);

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

            // UPDATE ANIMATOR
            player.playerAnimatorManager.UpdateAnimatorController(player.playerInventoryManager.currentLeftHandWeapon.weaponAnimator);

            // PLACE THE NON-TWO HANDED WEAPON MODEL IN THE BACK SLOT OR HIP SLOT
            backSlot.PlaceWeaponModelInUnequippedSlot(rightHandWeaponModel, player.playerInventoryManager.currentRightHandWeapon.weaponClass, player);

            // ADD TWO HAND STRENGTH BONUS

            // PLACE THE TWO HANDED WEAPON MODEL IN THE MAIN (RIGHT HAND)
            rightHandWeaponSlot.PlaceWeaponModelIntoSlot(leftHandWeaponModel);

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
                player.characterSoundFXManager.PlaySoundFX(
                    WorldSoundFXManager.instance.ChooseRandomSFXFromArray(player.playerInventoryManager
                        .currentRightHandWeapon.whooshes));
            }
            // 打开左手武器伤害碰撞体
            else if (player.playerNetworkManager.isUsingLeftHand.Value)
            {
                leftWeaponManager.meleeDamageCollider.EnableDamageCollider();
                player.characterSoundFXManager.PlaySoundFX(
                    WorldSoundFXManager.instance.ChooseRandomSFXFromArray(player.playerInventoryManager
                        .currentLeftHandWeapon.whooshes));
            }
            
            // 播放音效
        }
        
        public void CloseDamageCollider()
        {
            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                rightWeaponManager.meleeDamageCollider.DisableDamageCollider();
            }
            else if (player.playerNetworkManager.isUsingLeftHand.Value)
            {
                leftWeaponManager.meleeDamageCollider.DisableDamageCollider();
            }
        }
    }
}