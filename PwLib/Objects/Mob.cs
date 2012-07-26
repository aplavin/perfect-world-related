namespace PwLib.Objects
{
    public enum MobKind { Ground, Water, Air }
    public enum MobAction { None, Passive, PAtk, MAtk, Death, Moving }
    public enum MobFeature { None, Speed, Monolog, PDef, MDef, PAtk, MAtk, Berserk, Hp, Weak }

    public class Mob : PwObject
    {
        /// <summary>
        /// Mob level.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Mob HP (after selection)
        /// </summary>
        public int Hp { get; set; }

        /// <summary>
        /// Mob maximum HP (after selection)
        /// </summary>
        public int MaxHp { get; set; }

        /// <summary>
        /// Mob additional feature.
        /// </summary>
        public MobFeature Feature { get; set; }

        /// <summary>
        /// Kind of mob (ground, water, air)
        /// </summary>
        public MobKind Kind { get; set; }

        /// <summary>
        /// Mob current action.
        /// </summary>
        public MobAction Action { get; set; }

        /// <summary>
        /// Target for mob regular attack.
        /// </summary>
        public uint PTarget { get; set; }

        /// <summary>
        /// Target for mob skills.
        /// </summary>
        public uint MTarget { get; set; }
    }
}