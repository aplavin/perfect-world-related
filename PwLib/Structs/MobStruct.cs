using System.Runtime.InteropServices;

namespace PwLib.Structs
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct MobStruct
    {
        [FieldOffset(0x3C)] public float X;
        [FieldOffset(0x44)] public float Y;
        [FieldOffset(0x40)] public float Z;
        [FieldOffset(0xB4)] public int Type;
        [FieldOffset(0x11C)] public uint WorldId;
        [FieldOffset(0x120)] public int Id;
        [FieldOffset(0x124)] public int Level;
        [FieldOffset(0x12C)] public int Hp;
        [FieldOffset(0x16C)] public int MaxHp;
        [FieldOffset(0x248)] public int Feature;
        [FieldOffset(0x254)] public int pName;
        [FieldOffset(0x278)] public float Distance;
        [FieldOffset(0x2AC)] public int Kind;
        [FieldOffset(0x2B0)] public byte MoveFlag;
        [FieldOffset(0x2B8)] public int Action;
        [FieldOffset(0x2DC)] public uint PTarget;
        [FieldOffset(0x2E0)] public uint MTarget;
        [FieldOffset(0x2C4)] public int Attack;
    }
}
