using System.Runtime.InteropServices;

namespace PwLib.Structs
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    internal struct LootStruct
    {
        [FieldOffset(0x03C)] public float X;
        [FieldOffset(0x044)] public float Y;
        [FieldOffset(0x040)] public float Z;
        [FieldOffset(0x10C)] public uint WorldId;
        [FieldOffset(0x110)] public int Id;
        [FieldOffset(0x14C)] public short Type;
        [FieldOffset(0x150)] public int Level;
        [FieldOffset(0x154)] public float Distance;
        [FieldOffset(0x164)] public int pName;
    }
}
