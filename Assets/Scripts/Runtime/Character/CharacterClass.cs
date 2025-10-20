using UnityEngine;

namespace LZ
{
    [System.Serializable]
    public class CharacterClass
    {
        [Header("Class Information")]
        public string className;

        [Header("Class Stats")]
        public int vitality = 10;
        public int endurance = 10;
        public int mind = 10;
        public int strength = 10;
        public int dexterity = 10;
        public int intelligence = 10;
        public int faith = 10;
        //arcane/luck/whatever other stats you want

        [Header("Class Weapons")]
        public WeaponItem[] mainHandWeapons = new WeaponItem[3];
        public WeaponItem[] offHandWeapons = new WeaponItem[3];

        [Header("Class Armor")]
        public HeadEquipmentItem headEquipment;
        public BodyEquipmentItem bodyEquipment;
        public LegEquipmentItem legEquipment;
        public HandEquipmentItem handEquipment;

        [Header("Quick Slot Items")]
        public QuickSlotItem[] quickSlotItems = new QuickSlotItem[3];

        public void SetClass(PlayerManager player)
        {
            TitleScreenManager.instance.SetCharacterClass(player, vitality, endurance, mind, strength, dexterity, intelligence, faith,
                mainHandWeapons, offHandWeapons, headEquipment, bodyEquipment, legEquipment, handEquipment, quickSlotItems);
        }
    }
}
