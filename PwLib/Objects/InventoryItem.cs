namespace PwLib.Objects
{
    public class InventoryItem
    {
        public uint WorldId { get; set; }
        public int Type { get; set; }
        public int Id { get; set; }
        public int Amount { get; set; }
        public int MaxAmount { get; set; }
        public int SellPrice { get; set; }
        public int BuyPrice { get; set; }
        public int LevelReqEquip { get; set; }
        public int StrReq { get; set; }
        public int DexReq { get; set; }
        public int LevelReqUse { get; set; }
        public int MagReq { get; set; }
        public int Durability { get; set; }
        public int MaxDurability { get; set; }
        public int UpgradeLevel { get; set; }
        public int SocketsNumber { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0}, Amount: {1}", Id, Amount);
        }
    }
}