using System.Runtime.InteropServices;

namespace PwLib.Structs
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    internal struct InventoryItemStruct
    {
        [FieldOffset(0x0)] public uint WorldId;
        [FieldOffset(0x4)] public int Type;
        [FieldOffset(0x8)] public int Id;
        [FieldOffset(0x10)] public int Amount;
        [FieldOffset(0x14)] public int MaxAmount;
        [FieldOffset(0x18)] public int SellPrice;
        [FieldOffset(0x1C)] public int BuyPrice;
        [FieldOffset(0x40)] public int pDescription;
        [FieldOffset(0x4C)] public int LevelReqEquip;
        [FieldOffset(0x50)] public int StrReq;
        [FieldOffset(0x54)] public int DexReq;
        [FieldOffset(0x58)] public int LevelReqUse;
        [FieldOffset(0x60)] public int MagReq;
        [FieldOffset(0x68)] public int Durability;
        [FieldOffset(0x6C)] public int MaxDurability;
        [FieldOffset(0x84)] public int UpgradeLevel;
        [FieldOffset(0x94)] public int SocketsNumber;
    }
}
