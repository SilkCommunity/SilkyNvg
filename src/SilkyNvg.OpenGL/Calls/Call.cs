using Silk.NET.OpenGL;
using SilkyNvg.OpenGL.Shaders;

namespace SilkyNvg.OpenGL.Calls
{

    internal abstract class Call
    {

        protected readonly int _image;
        protected readonly Blend _blendFunc;

        protected readonly FragmentDataUniforms _uniforms;
        protected readonly Path[] _paths;

        public Call(int image, Blend blendFunc, FragmentDataUniforms uniforms, Path[] paths)
        {
            _image = image;
            _blendFunc = blendFunc;

            _uniforms = uniforms;
            _paths = paths;
        }

        public abstract void Run(GLInterface glInterface, GL gl);

    }
}