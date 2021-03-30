using Silk.NET.Maths;
using SilkyNvg;
using SpaceGame.Main.Classes;
using System.Linq;

namespace SpaceGame.Main
{
    public class Player : GameObject, IEntityA
    {

        private double _velX = 0;
        private double _velY = 0;

        private Textures _tex;
        private Controller _controller;
        private Game _game;

        public double X
        {
            get => _x;
            set => _x = value;
        }

        public double Y
        {
            get => _y;
            set => _y = value;
        }

        public double VelX { set => _velX = value; }
        public double VelY { set => _velY = value; }

        public Player(double x, double y, Textures tex, Game game, Controller controller) : base(x, y)
        {
            _tex = tex;
            _game = game;
            _controller = controller;
        }

        public void Tick(float d)
        {
            _x += _velX * d;
            _y += _velY * d;

            if (_x <= 0)
                _x = 0;
            if (_x >= Game.WIDTH * Game.SCALE - 32)
                _x = Game.WIDTH * Game.SCALE - 32;
            if (_y < 0)
                _y = 0;
            if (_y > Game.HEIGHT * Game.SCALE - 32)
                _y = Game.HEIGHT * Game.SCALE - 32;

            for (int i = 0; i < _game.eb.Count; i++)
            {
                var tempEnt = _game.eb.ElementAt(i);

                if (Physics.Collision(this, tempEnt))
                {
                    _controller.RemoveEntity(tempEnt);
                    Game.health -= 10;
                    _game.EnemyKilled++;

                    if (Game.health <= 0)
                    {
                        Game.health = 100 * 2;
                        _game.EnemyKilled = 0;
                        _game.EnemyCount = 5;
                        _game.eb.Clear();
                        _game.ea.Clear();
                        _controller.CreateEnemy(_game.EnemyCount);
                    }
                }
            }

        }

        public void Render(Nvg nvg)
        {
            nvg.BeginPath();
            nvg.Rect((float)_x, (float)_y, 32, 32);
            nvg.FillPaint(nvg.ImagePattern((float)_x, (float)_y, 32, 32, 0, _tex.Player, 1.0f));
            nvg.Fill();
        }

        public Rectangle<int> GetBounds()
        {
            return new Rectangle<int>(new Vector2D<int>((int)_x, (int)_y), new Vector2D<int>(32, 32));
        }

    }
}
