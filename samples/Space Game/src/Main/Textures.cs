using StbImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Main
{
    public class Textures
    {

        public int Player, Missile, Enemy;

        private SpriteSheet _ss = null;

        public Textures(Game game)
        {
            _ss = new SpriteSheet(game.SpriteSheet, game.Nvg);
            GetTextures();
        }

        private void GetTextures()
        {
            Player = _ss.GrabImage(1, 1, 32, 32);
            Missile = _ss.GrabImage(2, 1, 32, 32);
            Enemy = _ss.GrabImage(3, 1, 32, 32);
        }

    }
}
