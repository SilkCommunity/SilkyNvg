using System;
using System.Numerics;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Buffers;
using SilkyNvg.Rendering.OpenGL.Shaders;

namespace SilkyNvg.Rendering.OpenGL
{
    public class SilkyOpenGLRenderer : ISilkyRenderer
    {

        private static readonly Vector2[] Vertices = new Vector2[]
        {
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, -0.5f),
            new Vector2(-0.5f, -0.5f)
        };

        private readonly GL _gl;

        private Vbo _vbo;
        private Vao _vao;

        private SimpleShader _shader;
        
        public SilkyOpenGLRenderer(GL gl)
        {
            _gl = gl;

            _vao = new Vao(_gl);
            _vao.Bind();

            _vbo = new Vbo(BufferTargetARB.ArrayBuffer, _gl);
            _vbo.Bind();
            _vbo.Store<Vector2>(Vertices, BufferUsageARB.StaticDraw);
            
            _vao.VertexAttributePointer<Vector2>(0, 2, VertexAttribPointerType.Float, 1, 0);

            _shader = new SimpleShader(_gl);
        }

        public void FillPath(Vector2[] points, Vector3[] curveSpacePoints, int pointCount)
        {
            _shader.Start();
            
            _vao.Bind();
            _gl.EnableVertexAttribArray(0);
            
            _gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }

        public void Dispose()
        {
            _vbo.Dispose();
            _vao.Dispose();
            _shader.Dispose();
        }
        
    }
}