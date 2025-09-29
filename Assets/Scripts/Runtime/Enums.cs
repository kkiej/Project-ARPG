using UnityEngine;

public class Enums : MonoBehaviour
{
        
}

//  USED FOR CHARACTER DATA SAVING
public enum CharacterSlot
{
    CharacterSlot_01,
    CharacterSlot_02,
    CharacterSlot_03,
    CharacterSlot_04,
    CharacterSlot_05,
    CharacterSlot_06,
    CharacterSlot_07,
    CharacterSlot_08,
    CharacterSlot_09,
    CharacterSlot_10,
    NO_SLOT
}

//  USED FOR TO PROCESS DAMAGE, AND CHARACTER TARGETING
public enum CharacterGroup
{
    Team01,
    Team02
}

//  USED AS A TAG FOR EACH WEAPON MODEL INSTANTIATION SLOT
public enum WeaponModelSlot
{
    RightHand,
    LeftHandWeaponSlot,
    LeftHandShieldSlot,
    BackSlot
    //Right Hips
    //Left Hips
}

//  USED TO KNOW WHERE TO INSTANTIATE THE WEAPON MODEL BASED ON MODEL TYPE
public enum WeaponModelType
{
    Weapon,
    Shield
}

//  USED FOR ANY INFORMATION SPECIFIC TO A WEAPONS CLASS, SUCH AS BEING ABLE TO RIPOSTE ECT
public enum WeaponClass
{
    StraightSword,
    Spear,
    MediumShield,
    Fist
}

//  USED TO TAG EQUIPMENT MODELS WITH SPECIFIC BODY PARTS THAT THEY WILL COVER
public enum EquipmentModelType
{
    FullHelmet,     // WOULD ALWAYS HIDE FACE, HAIR ECT
    Hat,     // WOULD ALWAYS HIDE HAIR
    Hood,           // WOULD ALWAYS HIDE HAIR
    HelmetAcessorie,
    FaceCover,
    Torso,
    Back,
    RightShoulder,
    RightUpperArm,
    RightElbow,
    RightLowerArm,
    RightHand,
    LeftShoulder,
    LeftUpperArm,
    LeftElbow,
    LeftLowerArm,
    LeftHand,
    Hips,
    HipsAttachment,
    RightLeg,
    RightKnee,
    LeftLeg,
    LeftKnee
}

//  USED TO TAG HELMET TYPE, SO SPECIFIC HEAD PORTIONS CAN BE HIDDEN DURING EQUIP PROCESS (HAIR, BEARD, ECT)
public enum HeadEquipmentType
{
    FullHelmet, // HIDE ENTIRE HEAD + FEATURES
    Hat,        // DOES NOT HIDE ANYTHING
    Hood,       // HIDES HAIR
    FaceCover   // HIDES BEARD
}

//  USED TO CALCULATE DAMAGE BASED ON ATTACK TYPE
public enum AttackType
{
    LightAttack01,
    LightAttack02,
    HeavyAttack01,
    HeavyAttack02,
    ChargedAttack01,
    ChargedAttack02,
    RunningAttack01,
    RollingAttack01,
    BackstepAttack01
}

//  USED TO CALCULATE DAMAGE ANIMATION INTENSITY
public enum DamageIntensity
{
    Ping,
    Light,
    Medium,
    Heavy,
    Colossal
}

//  USED TO DETERMINE ITEM PICKUP TYPE
public enum ItemPickUpType
{
    WorldSpawn,
    CharacterDrop
}
