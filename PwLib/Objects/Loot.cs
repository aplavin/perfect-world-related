namespace PwLib.Objects
{
    public enum LootType { Item = 0x1, Coins = 0x3, BlueItem = 0x101, GoldItem = 0x1101 };

    public class Loot : PwObject
    {
        /// <summary>
        /// Type of the loot: item, coins, blue or gold item.
        /// </summary>
        public LootType Type { get; set; }
    }
}