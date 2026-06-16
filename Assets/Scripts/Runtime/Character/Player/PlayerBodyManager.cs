using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class PlayerBodyManager : MonoBehaviour
    {
        PlayerManager player;
        [Header("Hair Object")]
        [SerializeField] public GameObject hair;
        [SerializeField] private GameObject[] hairObjects;
        [SerializeField] public GameObject facialHair;

        [Header("Male")]
		[SerializeField] public GameObject maleObject;      // THE MASTER MALE GAMEOBJECT PARENT
        [SerializeField] public GameObject maleHead;        // 卸除护甲时的默认头部模型
        [SerializeField] public GameObject[] maleBody;      // 卸除护甲时的默认上半身模型（胸部，右上臂，左上臂）
        [SerializeField] public GameObject[] maleArms;      // 卸除护甲时的默认前臂与手部模型（右下臂，右手，左下臂，左手）
        [SerializeField] public GameObject[] maleLegs;      // 卸除护甲时的默认腿部模型（右腿，左腿）
        [SerializeField] public GameObject maleEyebrows;    // 面部特征
        [SerializeField] public GameObject maleFacialHair;  // 面部特征

        [Header("Female")]
        [SerializeField] public GameObject femaleObject;
        [SerializeField] public GameObject femaleHead;
        [SerializeField] public GameObject[] femaleBody;
        [SerializeField] public GameObject[] femaleArms;
        [SerializeField] public GameObject[] femaleLegs;
        [SerializeField] public GameObject femaleEyebrows;

        private void Awake()
        {
            player = GetComponent<PlayerManager>();
        }

        //  空安全工具：模块化换装模式下，默认身体引用可能为空（部件已由 ModularCharacterAssembler 接管），
        //  因此所有显隐操作都需先判空，避免移除旧 Synty 身体后空引用崩溃。
        private static void SetActiveSafe(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }

        private static void SetActiveSafe(GameObject[] models, bool active)
        {
            if (models == null) return;
            foreach (var model in models)
                if (model != null) model.SetActive(active);
        }

        //  ENABLE BODY FEATURES
        public void EnableHead()
        {
            // ENABLE HEAD OBJECT
            SetActiveSafe(maleHead, true);
            SetActiveSafe(femaleHead, true);

            // ENABLE ANY FACIAL OBJECTS (EYEBROWS, LIPS, NOSE ECT)
            SetActiveSafe(maleEyebrows, true);
            SetActiveSafe(femaleEyebrows, true);
        }

        public void DisableHead()
        {
            // DISABLE HEAD OBJECT
            SetActiveSafe(maleHead, false);
            SetActiveSafe(femaleHead, false);

            // DISABLE ANY FACIAL OBJECTS (EYEBROWS, LIPS, NOSE ECT)
            SetActiveSafe(maleEyebrows, false);
            SetActiveSafe(femaleEyebrows, false);
        }

        public void EnableHair()
        {
            SetActiveSafe(hair, true);
        }

        public void DisableHair()
        {
            SetActiveSafe(hair, false);
        }

        public void EnableFacialHair()
        {
            SetActiveSafe(facialHair, true);
        }

        public void DisableFacialHair()
        {
            SetActiveSafe(facialHair, false);
        }

        public void EnableBody()
        {
            SetActiveSafe(maleBody, true);
            SetActiveSafe(femaleBody, true);
        }

        public void EnableArms()
        {
            SetActiveSafe(maleArms, true);
            SetActiveSafe(femaleArms, true);
        }

        public void EnableLowerBody()
        {
            SetActiveSafe(maleLegs, true);
            SetActiveSafe(femaleLegs, true);
        }

        public void DisableBody()
        {
            SetActiveSafe(maleBody, false);
            SetActiveSafe(femaleBody, false);
        }

        public void DisableArms()
        {
            SetActiveSafe(maleArms, false);
            SetActiveSafe(femaleArms, false);
        }

        public void DisableLowerBody()
        {
            SetActiveSafe(maleLegs, false);
            SetActiveSafe(femaleLegs, false);
        }

        public void ToggleBodyType(bool isMale)
        {
            SetActiveSafe(maleObject, isMale);
            SetActiveSafe(femaleObject, !isMale);

            player.playerEquipmentManager.EquipArmor();
        }

        public void ToggleHairType(int hairType)
        {
            if (hairObjects == null || hairObjects.Length == 0)
                return;

            //  DISABLE ALL HAIR
            for (int i = 0; i < hairObjects.Length; i++)
            {
                SetActiveSafe(hairObjects[i], false);
            }

            //  ENABLE CHOOSEN HAIR
            if (hairType >= 0 && hairType < hairObjects.Length)
                SetActiveSafe(hairObjects[hairType], true);
        }

        public void SetHairColor()
        {
            // 1. IF YOU ARE USING A REGULAR MATERIAL AS A HAIR MATERIAL, SIMPLY CHANGE ITS COLOR
            //  IF YOU ARE USING A MATERIAL WITH MULITPLE COLOR VARIABLES, SIMPLY SET THE CORRECT COLOR

            Color32 hairColor;

            byte red = (byte)player.playerNetworkManager.hairColorRed.Value;
            byte green = (byte)player.playerNetworkManager.hairColorGreen.Value;
            byte blue = (byte)player.playerNetworkManager.hairColorBlue.Value;

            hairColor = new Color32(red, green, blue, 255);

            if (hairObjects == null)
                return;

            for (int i = 0; i < hairObjects.Length; i++)
            {
                if (hairObjects[i] == null)
                    continue;

                SkinnedMeshRenderer skinMeshRenderer = hairObjects[i].GetComponent<SkinnedMeshRenderer>();

                if (skinMeshRenderer != null)
                    skinMeshRenderer.material.SetColor("_Color_Hair", hairColor);
            }
        }
    }
}