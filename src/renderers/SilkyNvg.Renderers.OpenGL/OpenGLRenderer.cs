using System.Numerics;
using Silk.NET.OpenGL;
using SilkyNvg.Renderers.OpenGL.Buffers;
using SilkyNvg.Renderers.OpenGL.Shaders;
using SilkyNvg.Rendering;

namespace SilkyNvg.Renderers.OpenGL
{
    public sealed class OpenGLRenderer : ISilkyRenderer
    {

        private readonly VAO _vao;
        private readonly VBO<Vector2> _vbo;

        private readonly SilkyShader _shader;
        
        private readonly GL _gl;

        public OpenGLRenderer(GL gl)
        {
            _gl = gl;

            var quad = new Vector2[]
            {
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, -0.5f),
                new Vector2(-0.5f, 0.5f),
                
                new Vector2(0.5f, -0.5f),
                new Vector2(-0.5f, -0.5f),
                new Vector2(-0.5f, 0.5f)
            };
            
            _vao = new VAO(gl);
            _vbo = new VBO<Vector2>(gl);
            _vbo.Bind();
            _vbo.Store(quad, BufferUsageARB.StaticDraw);
            _vao.Bind();
            _vao.AttribPointer(0, 2, VertexAttribPointerType.Float, 8);
            _vbo.Unbind();
            _vao.Unbind();

            _shader = new SilkyShader(_gl);
        }

        public void Render()
        {
            _shader.Start();

            _vao.Bind();
            _vbo.Bind();
            _gl.EnableVertexAttribArray(0);
            
            _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
            
            _gl.DisableVertexAttribArray(0);
            _vbo.Unbind();
            _vao.Unbind();
            
            _shader.Stop();
        }

        public void Dispose()
        {
            _shader.Dispose();
            _vbo.Dispose();
            _vao.Dispose();
        }
        
    }
}