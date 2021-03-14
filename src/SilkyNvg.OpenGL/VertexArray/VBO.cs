using Silk.NET.OpenGL;
using System;

namespace SilkyNvg.OpenGL.VertexArray
{
    internal class VBO : IDisposable
    {

        private readonly uint _vboID;
        private readonly GL _gl;

        public uint ID => _vboID;

        public VBO(GL gl)
        {
            _gl = gl;

            _vboID = _gl.GenBuffer();
        }

        public void Bind()
        {
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vboID);
        }

        public unsafe void BufferData(float[] data)
        {
            Bind();
            fixed (void* d = data)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(data.Length * sizeof(float)), d, BufferUsageARB.StreamDraw);
            }
        }

        public void Unbind()
        {
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        }

        public void Dispose()
        {
            _gl.DeleteBuffer(_vboID);
        }
    }
}
