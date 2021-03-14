using Silk.NET.OpenGL;
using SilkyNvg.OpenGL.Shaders;

namespace SilkyNvg.OpenGL.Calls
{

    internal abstract class Call
    {

        // TODO: Image
        protected readonly int _triangleOffset;
        protected readonly int _triangleCount;
        protected readonly Blend _blendFunc;

        protected readonly FragmentDataUniforms _uniforms;
        protected readonly Path[] _paths;

        public Call(int triangleOffset, int triangleCount, Blend blendFunc, FragmentDataUniforms uniforms, Path[] paths)
        {
            _triangleOffset = triangleOffset;
            _triangleCount = triangleCount;
            _blendFunc = blendFunc;

            _uniforms = uniforms;
            _paths = paths;
        }

        public abstract void Run(GLInterface glInterface, GL gl);

    }
}