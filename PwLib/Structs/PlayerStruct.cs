using System.Runtime.InteropServices;

namespace PwLib.Structs
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct PlayerStruct
    {
        [FieldOffset(0x00C)] public float SinA;
        [FieldOffset(0x02C)] public float CosA;
        [FieldOffset(0x03C)] public float X;
        [FieldOffset(0x044)] public float Y;
        [FieldOffset(0x040)] public float Z;
        [FieldOffset(0x0B8)] public byte IsAttacking;
        [FieldOffset(0x280)] public byte IsMining; // negative
        [FieldOffset(0x47C)] public int WorldId;
        [FieldOffset(0x488)] public int Level;
        [FieldOffset(0x48C)] public int Cultivation;
        [FieldOffset(0x490)] public int Hp;
        [FieldOffset(0x494)] public int Mp;
        [FieldOffset(0x498)] public int Experience;
        [FieldOffset(0x49C)] public int Spirit;
        [FieldOffset(0x4A0)] public int SkillPoints;
        [FieldOffset(0x4A4)] public int Chi;
        [FieldOffset(0x4C0)] public int Vitality;
        [FieldOffset(0x4C4)] public int Intellect;
        [FieldOffset(0x4C8)] public int Strength;
        [FieldOffset(0x4CC)] public int Dexterity;
        [FieldOffset(0x4D0)] public int MaxHp;
        [FieldOffset(0x4D4)] public int MaxMp;
        [FieldOffset(0x4F0)] public int Accuracy;
        [FieldOffset(0x4F4)] public int MinPAtk;
        [FieldOffset(0x4F8)] public int MaxPAtk;
        [FieldOffset(0x52C)] public int MinMAtk;
        [FieldOffset(0x530)] public int MaxMAtk;
        [FieldOffset(0x534)] public int DefMetal;
        [FieldOffset(0x538)] public int DefWood;
        [FieldOffset(0x53C)] public int DefWater;
        [FieldOffset(0x540)] public int DefFire;
        [FieldOffset(0x544)] public int DefEarth;
        [FieldOffset(0x548)] public int DefPhys;
        [FieldOffset(0x54C)] public int Evasion;
        [FieldOffset(0x550)] public int MaxChi;
        [FieldOffset(0x554)] public int Money;
        [FieldOffset(0x558)] public int MaxMoney;
        [FieldOffset(0x5C8)] public int Reputation;
        [FieldOffset(0x5D4)] public byte PeaceZone;
        [FieldOffset(0x5F8)] public int CatshopState;
        [FieldOffset(0x604)] public int ClanId;
        [FieldOffset(0x608)] public int ClanPost;
        [FieldOffset(0x634)] public int SpouseId;
        [FieldOffset(0x638)] public int pName;
        [FieldOffset(0x640)] public byte Class;
        [FieldOffset(0x644)] public byte Gender;
        [FieldOffset(0x64C)] public byte WalkMode;
        [FieldOffset(0x650)] public byte RunMode;
        [FieldOffset(0x698)] public byte Status;
        [FieldOffset(0x6AC)] public byte IsAngry;
        [FieldOffset(0x6F8)] public uint SkillTargetId;
        [FieldOffset(0x730)] public int pFindPartyMsg;
        [FieldOffset(0x734)] public int pCatshopName;
        [FieldOffset(0x7A8)] public float Distance;
        [FieldOffset(0x99C)] public int TimeMining;
        [FieldOffset(0x9A0)] public int TimeMiningConst;
        [FieldOffset(0x9CC)] public int CooldownPotHpMp;
        [FieldOffset(0xA0C)] public int CooldownPotHp;
        [FieldOffset(0xA14)] public int CooldownPotMp;
        [FieldOffset(0xA3C)] public int CooldownPlayerInfo;
        [FieldOffset(0xA5C)] public int CooldownChemistry;
        [FieldOffset(0xA6C)] public int CooldownSos;
        [FieldOffset(0xB60)] public int PetRecallTime;
        [FieldOffset(0xB68)] public uint TargetId;
        [FieldOffset(0xB78)] public uint DialogId;
        [FieldOffset(0xBAC)] public float GroundZ;
        [FieldOffset(0xC64)] public int JumpingCnt;
    }
}
