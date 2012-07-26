using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PwLib
{
    public class Rectangle
    {
        private readonly Coords _leftTop;
        private readonly Coords _rightBottom;

        public Rectangle(float x1, float y1, float x2, float y2, bool gameCoords = false, float addBorder = 10)
        {
            if (gameCoords)
            {
                addBorder /= 10;
            }
            _leftTop = new Coords(Math.Min(x1, x2) - addBorder, Math.Min(y1, y2) - addBorder, 0, gameCoords);
            _rightBottom = new Coords(Math.Max(x1, x2) + addBorder, Math.Max(y1, y2) + addBorder, 0, gameCoords);
        }

        public bool IsCoordIn(Coords coords)
        {
            return coords.X.InRange(_leftTop.X, _rightBottom.X) && coords.Y.InRange(_leftTop.Y, _rightBottom.Y);
        }
    }
}
