using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkyNvg;
using SilkyNvg.Base;
using SilkyNvg.Image;
using SpaceGame.Main.Classes;
using StbImageSharp;
using System;
using System.Collections.Generic;

namespace SpaceGame.Main
{
    public class Game
    {

        public const int WIDTH = 320;
        public const int HEIGHT = WIDTH / 12 * 9;
        public const int SCALE = 2;

        public readonly string TITLE = "2D Space Game";

        // FPS
        private int _ticks = 0;
        private int _frames = 0;
        private double _time = 0;

        private Nvg _nvg;
        private GL _gl;

        private ImageResult _spriteSheet = null;
        private int _background = 0;

        private Player _p;
        private Controller _c;
        private Textures _tex;

        private int _enemyCount = 5;
        private int _enemyKilled = 0;

        public LinkedList<IEntityA> ea;
        public LinkedList<IEntityB> eb;

        public static int health = 100 * 2;

        public ImageResult SpriteSheet => _spriteSheet;
        public Nvg Nvg => _nvg;

        public int EnemyCount
        {
            get => _enemyCount;
            set => _enemyCount = value;
        }

        public int EnemyKilled
        {
            get => _enemyKilled;
            set => _enemyKilled = value;
        }

        private void Init(IWindow window)
        {
            var input = window.CreateInput();
            _ = new KeyInput(this, input);

            _gl = GL.GetApi(window);
            _nvg = Nvg.Create((uint)CreateFlag.Antialias | (uint)CreateFlag.StencilStrokes | (uint)CreateFlag.Debug, _gl);

            ImageLoader loader = new();
            try
            {
                _spriteSheet = loader.LoadImage("./res/sprite_sheet.png");
                _background = _nvg.CreateImage("./res/background.png", (uint)ImageFlags.Premultiplied);
            } catch
            {
                throw;
            }

            _tex = new Textures(this);

            _c = new Controller(_tex, this);
            _p = new Player(200, 200, _tex, this, _c);

            ea = _c.EntitiesA;
            eb = _c.EntitiesB;

            _c.CreateEnemy(_enemyCount);
        }

        private void Tick(double d)
        {
            // FPS
            _time += d;
            if (_time >= 1.0f)
            {
                Console.WriteLine(_ticks + " Ticks, FPS: " + _frames);
                _ticks = 0;
                _frames = 0;
                _time = 0;
            }
            //////////////////////////

            _p.Tick((float)d * 40);
            _c.Tick((float)d * 40);

            if (_enemyKilled >= _enemyCount)
            {
                _enemyCount += 2;
                _enemyKilled = 0;
                _c.CreateEnemy(_enemyCount);
            }

            //////////////////////////
            _ticks++;
        }

        private void Render(double _)
        {
            _gl.ClearColor(1.0f, 0.0f, 0.0f, 1.0f);
            _gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.StencilBufferBit);
            _nvg.BeginFrame(WIDTH * SCALE, HEIGHT * SCALE, 1.0f);
            //////////////////////////////////////
            _nvg.BeginPath();
            _nvg.Rect(0, 0, WIDTH * SCALE, HEIGHT * SCALE);
            _nvg.FillPaint(_nvg.ImagePattern(0, 0, WIDTH * SCALE, HEIGHT * SCALE, 0, _background, 1.0f));
            _nvg.Fill();

            _p.Render(_nvg);
            _c.Render(_nvg);

            _nvg.BeginPath();
            _nvg.FillColour(_nvg.Rgb(64, 64, 64));
            _nvg.Rect(5, 5, 200, 50);
            _nvg.Fill();
            _nvg.StrokeColour(_nvg.Rgb(255, 255, 255));
            _nvg.StrokeWidth(3);
            _nvg.Stroke();
            _nvg.BeginPath();
            _nvg.FillColour(_nvg.Rgb(0, 255, 0));
            _nvg.Rect(5, 5, health, 50);
            _nvg.Fill();
            //////////////////////////////////////
            _nvg.EndFrame();
            _frames++;
        }

        public void KeyPressed(Key key)
        {
            if (key == Key.Right)
            {
                _p.VelX = 5;
            }
            else if (key == Key.Left)
            {
                _p.VelX = -5;
            }
            else if(key == Key.Down)
            {
                _p.VelY = 5;
            }
            else if(key == Key.Up)
            {
                _p.VelY = -5;
            } else if (key == Key.Space)
            {
                _c.AddEntity(new Bullet(_p.X, _p.Y, _tex, this));
            }
        }

        public void KeyReleased(Key key)
        {
            if (key == Key.Right)
            {
                _p.VelX = 0;
            }
            else if (key == Key.Left)
            {
                _p.VelX = 0;
            }
            else if (key == Key.Down)
            {
                _p.VelY = 0;
            }
            else if (key == Key.Up)
            {
                _p.VelY = 0;
            }
        }

        static void Main()
        {
            Game game = new();

            var options = WindowOptions.Default;
            options.Size = new Silk.NET.Maths.Vector2D<int>(WIDTH * SCALE, HEIGHT * SCALE);
            options.UpdatesPerSecond = 60;
            options.FramesPerSecond = 60;
            options.WindowBorder = WindowBorder.Fixed;
            options.IsVisible = true;
            var window = Window.Create(options);
            window.Load += () => game.Init(window);
            window.Update += game.Tick;
            window.Render += game.Render;
            window.Closing += () => Environment.Exit(0);
            window.Run();
        }

    }
}
