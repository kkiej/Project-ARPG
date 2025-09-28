using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    [CreateAssetMenu(menuName = "Equipment Model")]
    public class EquipmentModel : ScriptableObject
    {
        public EquipmentModelType equipmentModelType;
        public string maleEquipmentName;
        public string femaleEquipmentName;

        public void LoadModel(PlayerManager player, bool isMale)
        {
            if (isMale)
            {
                LoadMaleModel(player);
            }
            else
            {
                LoadFemaleModel(player);
            }
        }

        private void LoadMaleModel(PlayerManager player)
        {
            // 1. 根据装备类型在全部装备模型中搜索（例如：若为头盔类型，则遍历所有头盔模型）
            // 2. 启用与装备名称相匹配的模型

            switch (equipmentModelType)
            {
                case EquipmentModelType.FullHelmet:

                    foreach (var model in player.playerEquipmentManager.maleHeadFullHelmets)
                    {
                        if (model.gameObject.name == maleEquipmentName)
                        {
                            model.gameObject.SetActive(true);
                            //  IF YOU WANT TO ASSIGN A MATERIAL DO IT HERE (model.gameObject.GetComponent<Renderer>().material = Instantiate(equipmentMaterial);
                        }
                    }
                    break;

                case EquipmentModelType.OpenHelmet:
                    break;
                case EquipmentModelType.Hood:
                    break;
                case EquipmentModelType.HelmetAcessorie:
                    break;
                case EquipmentModelType.FaceCover:
                    break;
                case EquipmentModelType.Torso:
                    break;
                case EquipmentModelType.Back:
                    break;
                case EquipmentModelType.RightShoulder:
                    break;
                case EquipmentModelType.RightUpperArm:
                    break;
                case EquipmentModelType.RightElbow:
                    break;
                case EquipmentModelType.RightLowerArm:
                    break;
                case EquipmentModelType.RightHand:
                    break;
                case EquipmentModelType.LeftShoulder:
                    break;
                case EquipmentModelType.LeftUpperArm:
                    break;
                case EquipmentModelType.LeftElbow:
                    break;
                case EquipmentModelType.LeftLowerArm:
                    break;
                case EquipmentModelType.LeftHand:
                    break;
                case EquipmentModelType.Hips:
                    break;
                case EquipmentModelType.HipsAttachment:
                    break;
                case EquipmentModelType.RightLeg:
                    break;
                case EquipmentModelType.RightKnee:
                    break;
                case EquipmentModelType.LeftLeg:
                    break;
                case EquipmentModelType.LeftKnee:
                    break;
                default:
                    break;
            }
        }

        private void LoadFemaleModel(PlayerManager player)
        {
            // 1. SEARCH THROUGH A LIST OF ALL EQUIPMENT MODELS BASED ON TYPE (EX. IF THIS IS A HELMET, WE LOOK THROUGH ALL HELMETS)
            // 2. ENABLE THE HELMET THAT MATCHES THE NAME
        }
    }
}
