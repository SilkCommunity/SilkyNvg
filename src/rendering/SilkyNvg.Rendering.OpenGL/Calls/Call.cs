using SilkyNvg.Rendering.OpenGL.Blending;
using SilkyNvg.Rendering.OpenGL.Shaders;
using System;

namespace SilkyNvg.Rendering.OpenGL.Calls
{
    internal abstract class Call
    {

        protected readonly int image;
        protected readonly Path[] paths;
        protected readonly int triangleOffset;
        protected readonly uint triangleCount;
        protected readonly FragUniforms uniforms;
        protected readonly Blend blendFunc;

        protected readonly OpenGLRenderer renderer;

        protected Call(int image, Span<Path> paths, int triangleOffset, uint triangleCount, FragUniforms uniforms, Blend blendFunc, OpenGLRenderer renderer)
        {
            this.image = image;
            this.paths = paths.ToArray();
            this.triangleOffset = triangleOffset;
            this.triangleCount = triangleCount;
            this.uniforms = uniforms;
            this.blendFunc = blendFunc;
            this.renderer = renderer;
        }

        public void Blend()
        {
            blendFunc.BlendFuncSeperate();
        }

        public abstract void Run();

    }
}
