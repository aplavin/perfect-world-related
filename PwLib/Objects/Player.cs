using System;

namespace PwLib.Objects
{
    public enum Cultivation { None = 0, Lvl9 = 1, Lvl19 = 2, Lvl29 = 3, Lvl39 = 4, Lvl49 = 5, Lvl59 = 6, Lvl69 = 7, Lvl79 = 8, Lvl89Holy = 20, Lvl99Holy = 21, Lvl100Holy = 23, Lvl89Evil = 30, Lvl99Evil = 31, Lvl100Evil = 32 }
    public enum ClanPost { Master = 2, Marshal = 3, Major = 4, Captain = 5, Member = 6 }
    public enum CatshopState { None, Initiating, Catshop, Wathing };
    public enum Class { Blademaster, Wizard, Psychic, Venomancer, Barbarian, Assasin, Archer, Cleric, Seeker, Mystic }
    public enum Gender { Male, Female }
    public enum WalkMode { Ground, Water, Air }
    public enum RunMode { Walk, Run }

    public class Player : PwObject
    {
        public float SinA { get; set; }
        public float CosA { get; set; }
        public float Angle { get { return (float)Math.Atan2(SinA, CosA); } }
        public bool IsAttacking { get; set; }
        public bool IsMining { get; set; }
        public int Level { get; set; }
        public Cultivation Cultivation { get; set; }
        public int Hp { get; set; }
        public int Mp { get; set; }
        public int MaxHp { get; set; }
        public int MaxMp { get; set; }
        public CatshopState CatshopState { get; set; }
        public int ClanId { get; set; }
        public ClanPost ClanPost { get; set; }
        public int SpouseId { get; set; }
        public Class Class { get; set; }
        public Gender Gender { get; set; }
        public WalkMode WalkMode { get; set; }
        public RunMode RunMode { get; set; }
        public byte Status { get; set; }
        public uint SkillTargetId { get; set; }
        public string FindPartyMsg { get; set; }
        public string CatshopName { get; set; }

        public new int Id { get { return (int)WorldId; } }
    }
}