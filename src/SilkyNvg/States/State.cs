using System.Numerics;

namespace SilkyNvg.States
{
    internal class State
    {

        internal Matrix3x2 Transform;

        internal State()
        {
            Transform = Matrix3x2.Identity;
        }

        internal State(State state)
        {
            Transform = state.Transform;
        }
    
    }
}
