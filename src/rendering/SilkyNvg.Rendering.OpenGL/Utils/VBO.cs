using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;

namespace SilkyNvg.Rendering.OpenGL.Utils
{
    internal class VBO : IDisposable
    {

        private readonly uint _vboID;
        private readonly GL _gl;

        public VBO(GL gl)
        {
            _gl = gl;
            _vboID = _gl.GenBuffer();
        }

        public void Bind()
        {
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vboID);
        }

        public unsafe void Update(ReadOnlySpan<Vertex> vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(vertices.Length * sizeof(Vertex)), vertices, BufferUsageARB.StreamDraw);
            _gl.EnableVertexAttribArray(0);
            _gl.EnableVertexAttribArray(1);
            _gl.VertexAttribPointer(0, 2, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)0);
            _gl.VertexAttribPointer(1, 2, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(0 + (2 * sizeof(float))));
        }

        public void Unbind()
        {
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        }

        public void Dispose()
        {
            if (_vboID != 0)
            {
                _gl.DeleteBuffer(_vboID);
            }
        }

    }
}
