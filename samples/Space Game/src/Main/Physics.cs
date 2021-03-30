using Silk.NET.Maths;
using SpaceGame.Main.Classes;
using System.Collections.Generic;
using System.Linq;

namespace SpaceGame.Main
{
    public class Physics
    {

        private static bool Intersects(Rectangle<int> a, Rectangle<int> b)
        {
            var tl = b.Origin;
            var tr = new Vector2D<int>(b.Origin.X + b.Size.X, b.Origin.Y);
            var bl = new Vector2D<int>(b.Origin.X, b.Origin.Y + b.Size.Y);
            var br = new Vector2D<int>(b.Origin.X + b.Size.X, b.Origin.Y + b.Size.Y);
            if (a.Contains(tl) || a.Contains(bl) || a.Contains(tr) || a.Contains(br))
                return true;
            else
                return false;
        }

        public static bool Collision(IEntityA enta, IEntityB entb)
        {
            if (Intersects(enta.GetBounds(), entb.GetBounds()))
            {
                return true;
            }
            return false;
        }

        public static bool Collision(IEntityB entb, IEntityA enta)
        {
            if (Intersects(entb.GetBounds(), enta.GetBounds()))
            {
                return true;
            }
            return false;
        }

    }
}
