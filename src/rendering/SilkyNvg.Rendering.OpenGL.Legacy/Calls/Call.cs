using Silk.NET.OpenGL.Legacy;
using SilkyNvg.Rendering.OpenGL.Legacy.Blending;
using SilkyNvg.Rendering.OpenGL.Legacy.Paths;
using SilkyNvg.Rendering.OpenGL.Legacy.Shaders;
using System;

namespace SilkyNvg.Rendering.OpenGL.Legacy.Calls
{
    internal abstract class Call
    {

        protected readonly Uniforms uniforms;
        protected readonly Blend blend;
        protected readonly int image;
        protected readonly Path[] paths;

        protected Call(Uniforms uniforms, Blend blend, int image, Path[] paths)
        {
            this.uniforms = uniforms;
            this.blend = blend;
            this.image = image;
            this.paths = paths;
        }

        protected void CheckError(string str, LegacyOpenGLRenderer renderer)
        {
            if (!renderer.Debug)
            {
                return;
            }

            GLEnum err = renderer.Gl.GetError();
            if (err != GLEnum.NoError)
            {
                Console.Error.WriteLine("Error " + err + " after" + Environment.NewLine + str);
                return;
            }
        }

        public abstract void Run(LegacyOpenGLRenderer renderer);

    }
}
