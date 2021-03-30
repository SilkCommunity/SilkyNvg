using Silk.NET.Maths;
using SilkyNvg;
using SpaceGame.Main.Classes;
using System;
using System.Linq;

namespace SpaceGame.Main
{
    public class Enemy : GameObject, IEntityB
    {

        private Random _r = new();

        private Textures _tex;
        private Game _game;
        private Controller _c;

        private int _speed;

        public double X => _x;
        public double Y
        {
            get => _y;
            set => _y = value;
        }

        public Enemy(double x, double y, Textures tex, Controller c, Game game) : base(x, y)
        {
            _tex = tex;
            _c = c;
            _game = game;

            _speed = _r.Next(3) + 1;
        }

        public void Tick(float d)
        {
            _y += _speed * d;

            if (_y > Game.HEIGHT * Game.SCALE)
            {
                _y = -10;
                _x = _r.Next(32, Game.WIDTH * Game.SCALE) - 32;
                _speed = _r.Next(3) + 1;
            }

            for (int i = 0; i < _game.ea.Count; i++)
            {
                var tempEnt = _game.ea.ElementAt(i);
                if (Physics.Collision(this, tempEnt))
                {
                    _c.RemoveEntity(this);
                    _c.RemoveEntity(tempEnt);
                    _game.EnemyKilled++;
                }
            }

        }

        public void Render(Nvg nvg)
        {
            nvg.BeginPath();
            nvg.Rect((float)_x, (float)_y, 32, 32);
            nvg.FillPaint(nvg.ImagePattern((float)_x, (float)_y, 32, 32, 0, _tex.Enemy, 1.0f));
            nvg.Fill();
        }

        public Rectangle<int> GetBounds()
        {
            return new Rectangle<int>(new Vector2D<int>((int)_x, (int)_y), new Vector2D<int>(32, 32));
        }

    }
}
