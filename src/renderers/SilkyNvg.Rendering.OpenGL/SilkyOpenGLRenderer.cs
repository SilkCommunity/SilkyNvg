using System.Numerics;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Buffers;
using SilkyNvg.Rendering.OpenGL.Shaders;

namespace SilkyNvg.Rendering.OpenGL
{
    public class SilkyOpenGLRenderer : ISilkyRenderer
    {

        private static readonly Vector2[] Vertices =
        [
            new(-0.5f, 0.5f), new(-0.5f, -0.5f), new(0.5f, 0.5f),
            new(0.5f, 0.5f), new(-0.5f, -0.5f), new(0.5f, -0.5f)
        ];
        
        private readonly SceneContainer _scene;
        private readonly GL _gl;

        private Vbo _vbo;
        private Vao _vao;

        private SimpleShader _shader;
        
        public ISceneContainer SceneContainer => _scene;

        public SilkyOpenGLRenderer(GL gl)
        {
            _gl = gl;

            _scene = new SceneContainer(3, gl);

            _vao = new Vao(_gl);
            _vao.Bind();

            _vbo = new Vbo(BufferTargetARB.ArrayBuffer, _gl);
            _vbo.Bind();
            _vbo.Store(Vertices, BufferUsageARB.StaticDraw);

            _vao.VertexAttributePointer<Vector2>(0, 2, VertexAttribPointerType.Float, 1, 0);

            _shader = new SimpleShader(_gl);
        }

        public void Resize(uint width, uint height, RenderTolerances _)
        {
            
        }

        public void BeginFrame()
        {
            _scene.BeginFrame();
        }

        public void EndFrame()
        {
            _scene.EndFrame();

            _scene.Clear();
        }

        private void RasterizeImpl()
        {
            _shader.Start();

            _vao.Bind();
            _gl.EnableVertexAttribArray(0);
            _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)Vertices.Length);
        }

        public void Rasterize()
        {
            RasterizeImpl();
        }
        
        public void Dispose()
        {
            _scene.Dispose();

            _vao.Dispose();
            _vbo.Dispose();
            _shader.Dispose();
        }
        
    }
}