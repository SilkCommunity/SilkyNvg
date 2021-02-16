using Silk.NET.OpenGL;
using SilkyNvg.OpenGL.Shaders;
using SilkyNvg.OpenGL.VertexArray;
using System;

namespace SilkyNvg.OpenGL
{
    public sealed class GLInterface
    {

        private readonly GraphicsManager _graphicsManager;
        private readonly GL _gl;

        private VAO _vao;
        private NvgShader _shader;
        private FragmentData _fragmentData;

        private void CheckError(string str)
        {
            if (!_graphicsManager.LaunchParameters.Debug)
                return;
            GLEnum error = _gl.GetError();
            if (error != GLEnum.NoError)
            {
                Console.Error.WriteLine("Error " + error + " after " + str);
            }
        }

        public GLInterface(GraphicsManager graphicsManager)
        {
            _graphicsManager = graphicsManager;
            _gl = _graphicsManager.GL;
            Init();
        }

        private void Init()
        {
            CheckError("init");
            _shader = new NvgShader("NvgShader", _graphicsManager.LaunchParameters.EdgeAntialias, _gl);
            CheckError("Uniform locations");
            _shader.GetUniforms();
            _vao = new VAO(_gl);
            _fragmentData = new FragmentData();
            CheckError("Post initialization");
            _gl.Finish();
        }

    }
}
