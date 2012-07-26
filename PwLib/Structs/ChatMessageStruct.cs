using System.Runtime.InteropServices;

namespace PwLib.Structs
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    internal struct ChatMessageStruct
    {
        [FieldOffset(0x04)] public byte MsgScope;
        [FieldOffset(0x05)] public byte SmileySet;
        [FieldOffset(0x08)] public uint pMsg;
        [FieldOffset(0x0C)] public uint ItemId;
        [FieldOffset(0x10)] public uint msgId;
    }
}
