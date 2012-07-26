using System.Runtime.InteropServices;

namespace PwLib.Structs
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    internal struct SkillStruct
    {
        [FieldOffset(0x08)] public int Id;
        [FieldOffset(0x0C)] public int Level;
        [FieldOffset(0x10)] public int Cooldown;
        [FieldOffset(0x14)] public int MaxCooldown;
        [FieldOffset(0x18)] public bool IsCooldown;
    }
}
