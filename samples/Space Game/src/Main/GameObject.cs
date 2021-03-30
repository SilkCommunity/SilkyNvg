using Silk.NET.Maths;

namespace SpaceGame.Main
{
    public class GameObject
    {

        public double _x, _y;

        public GameObject(double x, double y)
        {
            _x = x;
            _y = y;
        }

        public Rectangle<int> GetBounds(int width, int height)
        {
            return new Rectangle<int>(new Vector2D<int>((int)_x, (int)_y), new Vector2D<int>(width, height));
        }

    }
}
