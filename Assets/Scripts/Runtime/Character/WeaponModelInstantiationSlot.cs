using UnityEngine;

namespace LZ
{
    public class WeaponModelInstantiationSlot : MonoBehaviour
    {
        public WeaponModelSlot weaponSlot;
        public GameObject currentWeaponModel;

        public void UnloadWeapon()
        {
            if (currentWeaponModel != null)
            {
                Destroy(currentWeaponModel);
            }
        }

        public void PlaceWeaponModelIntoSlot(GameObject weaponModel)
        {
            currentWeaponModel = weaponModel;
            weaponModel.transform.parent = transform;

            weaponModel.transform.localPosition = Vector3.zero;
            weaponModel.transform.localRotation = Quaternion.identity;
            weaponModel.transform.localScale = Vector3.one;
        }

        public void PlaceWeaponModelInUnequippedSlot(GameObject weaponModel, WeaponClass weaponClass, PlayerManager player)
        {
            // TO DO, MOVE WEAPON ON BACK CLOSER OR MORE OUTWARD DEPENDING ON CHEST EQUIPMENT (SO IT DOESNT APPEAR TO FLOAT)

            currentWeaponModel = weaponModel;
            weaponModel.transform.parent = transform;

            switch (weaponClass)
            {
                case WeaponClass.StraightSword:
                    weaponModel.transform.localPosition = new Vector3(0.064f, 0f, -0.06f);
                    weaponModel.transform.localRotation = Quaternion.Euler(194, 90, -0.22f);
                    break;
                case WeaponClass.Spear:
                    weaponModel.transform.localPosition = new Vector3(0.064f, 0f, -0.06f);
                    weaponModel.transform.localRotation = Quaternion.Euler(194, 90, -0.22f);
                    break;
                case WeaponClass.MediumShield:
                    weaponModel.transform.localPosition = new Vector3(0.074f, -0.002f, 0.069f);
                    weaponModel.transform.localRotation = Quaternion.Euler(-180.235f, 180.202f, -15.65601f);
                    break;
                default:
                    break;
            }
        }
    }
}