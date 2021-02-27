using Silk.NET.OpenGL;
using SilkyNvg.OpenGL.Shaders;
using System;
using System.Runtime.InteropServices;
using Shader = SilkyNvg.OpenGL.Shaders.Shader;

namespace SilkyNvg.OpenGL
{
    public sealed class GLInterface
    {

        private readonly Shader _shader;

        private readonly uint _vao;
        private readonly uint _vertexVBO, _fragmentVBO;

        private readonly GraphicsManager _graphicsManager;
        private readonly GL _gl;

        internal void CheckError(string str)
        {
            if (_graphicsManager.LaunchParameters.Debug)
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

            CheckError("init");
            _shader = new Shader("SilkyNvg-Shader", _graphicsManager.LaunchParameters.EdgeAntialias, _gl);
            CheckError("loaded shaders");
            _shader.GetUniforms();

            _vao = _gl.GenVertexArray();
            _vertexVBO = _gl.GenBuffer();

            _shader.BindBlock();
            _fragmentVBO = _gl.GenBuffer();
            _gl.GetInteger(GetPName.UniformBufferOffsetAlignment, out int align);

            _shader.UniformSize = Marshal.SizeOf(typeof(FragmentDataUniforms)) + align - Marshal.SizeOf(typeof(FragmentDataUniforms)) % align;

            CheckError("create done!");
            _gl.Finish();
        }

        public void RenderViewport(float width, float height)
        {

        }

    }
}
