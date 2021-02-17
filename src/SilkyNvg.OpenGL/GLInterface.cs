using Silk.NET.Maths;
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

        private Vector2D<float> _viewport = new Vector2D<float>(0.0F);

        private VAO _vao;
        private NvgShader _shader;

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
            CheckError("Post initialization");
            _gl.Finish();
        }

        public void SetupViewSize(float width, float height)
        {
            _viewport.X = width;
            _viewport.Y = height;
        }

    }
}
