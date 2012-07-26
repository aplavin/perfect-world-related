namespace PwLib.Objects
{

    public class PwObject
    {
        /// <summary>
        /// Object coordinates (Z is height).
        /// </summary>
        public Coords Coords { get; set; }
        /// <summary>
        /// Id of object, is the same for all such objects.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Unique object id - world id.
        /// </summary>
        public uint WorldId { get; set; }
        /// <summary>
        /// Distance from host player to the object.
        /// </summary>
        public float Distance { get; set; }
        /// <summary>
        /// Object name.
        /// </summary>
        public string Name { get; set; }

        public static implicit operator uint(PwObject obj)
        {
            return obj.WorldId;
        }
    }
}