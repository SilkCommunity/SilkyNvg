using SilkyNvg;
using SpaceGame.Main.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceGame.Main
{
    public class Controller
    {

        private LinkedList<IEntityA> _ea = new();
        private LinkedList<IEntityB> _eb = new();

        private IEntityA _enta;
        private IEntityB _entb;
        private Textures _tex;

        private Random _r = new();
        private Game _game;

        public LinkedList<IEntityA> EntitiesA => _ea;
        public LinkedList<IEntityB> EntitiesB => _eb;

        public Controller(Textures tex, Game game)
        {
            _tex = tex;
            _game = game;
        }

        public void CreateEnemy(int enemyCount)
        {
            for (int i = 0; i < enemyCount; i++)
            {
                AddEntity(new Enemy(_r.Next(32, 640) - 32, -10, _tex, this, _game));
            }
        }

        public void Tick(float d)
        {
            // A CLASS
            for (int i = 0; i < _ea.Count; i++)
            {
                _enta = _ea.ElementAt(i);

                _enta.Tick(d);
            }

            // B CLASS
            for (int i = 0; i < _eb.Count; i++)
            {
                _entb = _eb.ElementAt(i);

                _entb.Tick(d);
            }
        }

        public void Render(Nvg nvg)
        {
            for (int i = 0; i < _ea.Count; i++)
            {
                _enta = _ea.ElementAt(i);

                _enta.Render(nvg);
            }

            for (int i = 0; i < _eb.Count; i++)
            {
                _entb = _eb.ElementAt(i);

                _entb.Render(nvg);
            }
        }

        public void AddEntity(IEntityA block)
        {
            _ea.AddLast(block);
        }

        public void RemoveEntity(IEntityA block)
        {
            _ea.Remove(block);
        }

        public void AddEntity(IEntityB block)
        {
            _eb.AddLast(block);
        }

        public void RemoveEntity(IEntityB block)
        {
            _eb.Remove(block);
        }

    }
}
