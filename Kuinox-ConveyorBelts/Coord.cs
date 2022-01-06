using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuinox.ConveyorBelts
{
    public struct Coord
    {
        public int X;
        public int Y;

        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public struct CoordF
    {
        public float X;
        public float Y;

        public CoordF(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}
