using SilkyNvg.Rendering.OpenGL.Blending;

namespace SilkyNvg.Rendering.OpenGL.Calls
{
    internal abstract class Call
    {

        protected readonly int image;
        protected readonly Path[] paths;
        protected readonly int triangleOffset;
        protected readonly uint triangleCount;
        protected readonly int uniformOffset;
        protected readonly Blend blendFunc;

        protected readonly OpenGLRenderer renderer;

        protected Call(int image, Path[] paths, int triangleOffset, uint triangleCount, int uniformOffset, Blend blendFunc, OpenGLRenderer renderer)
        {
            this.image = image;
            this.paths = paths;
            this.triangleOffset = triangleOffset;
            this.triangleCount = triangleCount;
            this.uniformOffset = uniformOffset;
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