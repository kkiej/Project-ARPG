using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZ
{
    public class PlayerBodyManager : MonoBehaviour
    {
        [Header("Hair Object")]
        [SerializeField] public GameObject hair;
        [SerializeField] public GameObject facialHair;

        [Header("Male")]
        [SerializeField] public GameObject maleHead;        // 卸除护甲时的默认头部模型
        [SerializeField] public GameObject[] maleBody;      // 卸除护甲时的默认上半身模型（胸部，右上臂，左上臂）
        [SerializeField] public GameObject[] maleArms;      // 卸除护甲时的默认前臂与手部模型（右下臂，右手，左下臂，左手）
        [SerializeField] public GameObject[] maleLegs;      // 卸除护甲时的默认腿部模型（右腿，左腿）
        [SerializeField] public GameObject maleEyebrows;    // 面部特征
        [SerializeField] public GameObject maleFacialHair;  // 面部特征

        [Header("Female")]
        [SerializeField] public GameObject femaleHead;
        [SerializeField] public GameObject[] femaleBody;
        [SerializeField] public GameObject[] femaleArms;
        [SerializeField] public GameObject[] femaleLegs;
        [SerializeField] public GameObject femaleEyebrows;

        //  ENABLE BODY FEATURES
        public void EnableHead()
        {
            // ENABLE HEAD OBJECT
            maleHead.SetActive(true);
            femaleHead.SetActive(true);

            // ENABLE ANY FACIAL OBJECTS (EYEBROWS, LIPS, NOSE ECT)
            maleEyebrows.SetActive(true);
            femaleEyebrows.SetActive(true);
        }

        public void DisableHead()
        {
            // DISABLE HEAD OBJECT
            maleHead.SetActive(false);
            femaleHead.SetActive(false);

            // DISABLE ANY FACIAL OBJECTS (EYEBROWS, LIPS, NOSE ECT)
            maleEyebrows.SetActive(false);
            femaleEyebrows.SetActive(false);
        }

        public void EnableHair()
        {
            hair.SetActive(true);
        }

        public void DisableHair()
        {
            hair.SetActive(false);
        }

        public void EnableFacialHair()
        {
            facialHair.SetActive(true);
        }

        public void DisableFacialHair()
        {
            facialHair.SetActive(false);
        }

        public void EnableBody()
        {
            foreach (var model in maleBody)
            {
                model.SetActive(true);
            }

            foreach (var model in femaleBody)
            {
                model.SetActive(true);
            }
        }
    }
}