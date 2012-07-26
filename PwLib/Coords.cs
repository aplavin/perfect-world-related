using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PwLib
{
    [Serializable]
    public class Coords
    {
        public Coords(float x, float y, float z)
            : this(x, y, z, false)
        { }

        public Coords(float x, float y, float z, bool gameCoords)
        {
            if (gameCoords)
            {
                GameX = x;
                GameY = y;
                GameZ = z;
            }
            else
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        public string ToGameString()
        {
            return string.Format("({0:0.#}, {1:0.#}) ↑{2:0.#}", GameX, GameY, GameZ);
        }

        public override string ToString()
        {
            return ToGameString();
            return string.Format("({0:0.#}, {1:0.#}) ↑{2:0.#}", X, Y, Z);
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public float GameX
        {
            get { return X / 10 + 400; }
            set { X = (value - 400) * 10; }
        }

        public float GameY
        {
            get { return Y / 10 + 550; }
            set { Y = (value - 550) * 10; }
        }

        public float GameZ
        {
            get { return Z / 10; }
            set { Z = value * 10; }
        }

        public float Distance(Coords coords)
        {
            return (float)Math.Sqrt((X - coords.X) * (X - coords.X) + (Y - coords.Y) * (Y - coords.Y) + (Z - coords.Z) * (Z - coords.Z));
        }

        public float DistancePlanar(Coords coords)
        {
            return (float)Math.Sqrt((X - coords.X) * (X - coords.X) + (Y - coords.Y) * (Y - coords.Y));
        }

        public float GameDistance(Coords coords)
        {
            return Distance(coords) / 10;
        }

        public float GameDistancePlanar(Coords coords)
        {
            return DistancePlanar(coords) / 10;
        }

        public bool Equals(Coords other)
        {
            if (other == null)
                return false;
            return Distance(other) <= 1e-8;
        }
    }
}
