using Silk.NET.OpenGL;
using System;

namespace SilkyNvg.Rendering.OpenGL.Utils
{
    internal class VAO : IDisposable
    {

        private readonly uint _vaoID;
        private readonly GL _gl;

        public VBO Vbo { get; set; }

        public VAO(GL gl)
        {
            _gl = gl;
            _vaoID = _gl.GenVertexArray();
        }

        public void Bind()
        {
            _gl.BindVertexArray(_vaoID);

            Vbo.Bind();
        }

        public void Unbind()
        {
            Vbo.Unbind();
            _gl.BindVertexArray(0);
        }

        public void Dispose()
        {
            Vbo.Dispose();
            if (_vaoID != 0)
            {
                _gl.DeleteVertexArray(_vaoID);
            }
        }

    }
}
