using Silk.NET.OpenGL;

namespace SilkyNvg.OpenGL.Calls
{

    internal abstract class Call
    {

        // TODO: Image
        protected readonly int _pathOffset;
        protected readonly int _pathCount;
        protected readonly int _triangleOffset;
        protected readonly int _triangleCount;
        protected readonly int _uniformOffset;
        protected readonly Blend _blendFunc;

        public Call(int pathOffset, int pathCount, int triangleOffset, int triangleCount, int uniformOffset, Blend blendFunc)
        {
            _pathOffset = pathOffset;
            _pathCount = pathCount;
            _triangleOffset = triangleOffset;
            _triangleCount = triangleCount;
            _uniformOffset = uniformOffset;
            _blendFunc = blendFunc;
        }

        public abstract void Run(GLInterface glInterface, GL gl);

    }
}