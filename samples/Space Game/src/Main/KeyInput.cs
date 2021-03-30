using Silk.NET.Input;

namespace SpaceGame.Main
{
    public class KeyInput
    {

        private Game _game;

        public KeyInput(Game game, IInputContext context)
        {
            _game = game;
            foreach (IKeyboard keyboard in context.Keyboards)
            {
                keyboard.KeyDown += KeyPressed;
                keyboard.KeyUp += KeyReleased;
            }
        }

        public void KeyPressed(IKeyboard _, Key key, int scancode)
        {
            _game.KeyPressed(key);
        }

        public void KeyReleased(IKeyboard _, Key key, int scancode)
        {
            _game.KeyReleased(key);
        }

    }
}
