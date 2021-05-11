using SilkyNvg.Core.Paths;
using System.Numerics;

namespace SilkyNvg.Core.Instructions
{
    internal class CloseInstruction : IInstruction
    {

        public bool RequiresPosition => false;

        public float[] Data => null;

        public PathCache PathCache { private get; set; }

        public void Transform(Matrix3x2 _) { }

        public void BuildPath()
        {
            PathCache.ClosePath();
        }

    }
}
