using Silk.NET.Maths;
using SilkyNvg;
using SpaceGame.Main.Classes;

namespace SpaceGame.Main
{
    public class Bullet : GameObject, IEntityA
    {

        private Textures _tex;
        private Game _game;

        public double X => _x;
        public double Y => _y;

        public Bullet(double x, double y, Textures tex, Game game) : base(x, y)
        {
            _tex = tex;
            _game = game;
        }

        public void Tick(float d)
        {
            _y -= 10 * d;
        }

        public void Render(Nvg nvg)
        {
            nvg.BeginPath();
            nvg.Rect((float)_x, (float)_y, 32, 32);
            nvg.FillPaint(nvg.ImagePattern((float)_x, (float)_y, 32, 32, 0, _tex.Missile, 1.0f));
            nvg.Fill();
        }

        public Rectangle<int> GetBounds()
        {
            return new Rectangle<int>(new Vector2D<int>((int)_x, (int)_y), new Vector2D<int>(32, 32));
        }

    }
}
