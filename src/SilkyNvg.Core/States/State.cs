using SilkyNvg.Common;
using System.Numerics;

namespace SilkyNvg.Core.States
{
    internal class State
    {

        public CompositeOperationState compositeOperation;
        public bool shapeAntialias;
        public Paint fill;
        public Paint stroke;
        public float strokeWidth;
        public float miterLimit;
        public LineCap lineJoin;
        public LineCap lineCap;
        public float alpha;
        public Matrix3x2 transform;
        public Scissor scissor;
        // TODO: Font

        public State Clone()
        {
            return (State)MemberwiseClone();
        }

    }
}
