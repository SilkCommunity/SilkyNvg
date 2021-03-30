using Silk.NET.Maths;
using SilkyNvg;

namespace SpaceGame.Main.Classes
{
    public interface IEntityA
    {

        public double X { get; }
        public double Y { get; }

        Rectangle<int> GetBounds();

        void Tick(float d);
        void Render(Nvg nvg);

    }
}
