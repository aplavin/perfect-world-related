namespace PwLib.Objects
{
    public class Pet : PwObject
    {

        /// <summary>
        /// Pet level (after selection)
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Pet HP (after selection)
        /// </summary>
        public int Hp { get; set; }

        /// <summary>
        /// Pet maximum HP (after selection)
        /// </summary>
        public int MaxHp { get; set; }

        /// <summary>
        /// Pet type (ground, water, air)
        /// </summary>
        public MobKind Kind { get; set; }

        /// <summary>
        /// Pet current action.
        /// </summary>
        public MobAction Action { get; set; }

        /// <summary>
        /// Target for pet regular attack.
        /// </summary>
        public uint PTarget { get; set; }

        /// <summary>
        /// Target for pet skills.
        /// </summary>
        public uint MTarget { get; set; }
    }
}