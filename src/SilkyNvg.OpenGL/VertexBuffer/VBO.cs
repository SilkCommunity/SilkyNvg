using Silk.NET.OpenGL;
using System;

namespace SilkyNvg.OpenGL.VertexBuffer
{
    internal sealed class VBO : IDisposable
    {

        private readonly uint _vboID;
        private readonly BufferTargetARB _type;
        private readonly GL _gl;

        public VBO(BufferTargetARB type, GL gl)
        {
            _type = type;
            _gl = gl;
            _vboID = _gl.GenBuffer();
        }

        public unsafe void Store<T>(T[] data)
            where T : unmanaged
        {
            Bind();
            fixed (void* d = data)
            {
                _gl.BufferData(_type, (uint)sizeof(T) * (uint)data.Length, d, BufferUsageARB.StaticDraw);
            }
            Unbind();
        }

        public void Bind()
        {
            _gl.BindBuffer(_type, _vboID);
        }

        public void Unbind()
        {
            _gl.BindBuffer(_type, 0);
        }

        public void Dispose()
        {
            Unbind();
            _gl.DeleteBuffer(_vboID);
        }

    }
}
