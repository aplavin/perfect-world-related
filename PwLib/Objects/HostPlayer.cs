namespace PwLib.Objects
{
    public class HostPlayer : Player
    {
        public int Experience { get; set; }
        public int Spirit { get; set; }
        public int SkillPoints { get; set; }
        public int Chi { get; set; }
        public int Vitality { get; set; }
        public int Intellect { get; set; }
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Accuracy { get; set; }
        public int MinPAtk { get; set; }
        public int MaxPAtk { get; set; }
        public int MinMAtk { get; set; }
        public int MaxMAtk { get; set; }
        public int DefMetal { get; set; }
        public int DefWood { get; set; }
        public int DefWater { get; set; }
        public int DefFire { get; set; }
        public int DefEarth { get; set; }
        public int DefPhys { get; set; }
        public int Evasion { get; set; }
        public int MaxChi { get; set; }
        public int Money { get; set; }
        public int MaxMoney { get; set; }
        public int Reputation { get; set; }
        public byte PeaceZone { get; set; }
        public byte IsAngry { get; set; }
        public int TimeMining { get; set; }
        public int TimeMiningConst { get; set; }
        public int CooldownPotHpMp { get; set; }
        public int CooldownPotHp { get; set; }
        public int CooldownPotMp { get; set; }
        public int CooldownPlayerInfo { get; set; }
        public int CooldownChemistry { get; set; }
        public int CooldownSos { get; set; }
        public int PetRecallTime { get; set; }
        public uint TargetId { get; set; }
        public uint DialogId { get; set; }
        public float GroundZ { get; set; }
        public int JumpingCnt { get; set; }

        public bool IsCasting { get { return SkillTargetId != 0; } }
    }
}