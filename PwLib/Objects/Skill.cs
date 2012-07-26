namespace PwLib.Objects
{
    public class Skill
    {
        public int Pointer { get; set; }
        public int Id { get; set; }
        public string Name { get { return Database.GetSkillName(Id); } }
        public int Level { get; set; }
        public int Cooldown { get; set; }
        public int MaxCooldown { get; set; }
        public bool IsCooldown { get; set; }
        public int ChiRequired { get; set; }
    }
}
