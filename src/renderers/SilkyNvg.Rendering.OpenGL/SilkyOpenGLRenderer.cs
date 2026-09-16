using System;
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
        private ComputeShader _computeShader;

        private uint _textureId;
        
        public ISceneContainer SceneContainer => _scene;

        public unsafe SilkyOpenGLRenderer(GL gl)
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
            _computeShader = new ComputeShader(gl);

            _textureId = _gl.GenTexture();
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, _textureId);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba32f, 512, 512, 0, GLEnum.Rgba, GLEnum.Float,
                null);
            _gl.BindImageTexture(0, _textureId, 0, false, 0, BufferAccessARB.ReadWrite, InternalFormat.Rgba32f);

            var colourData = new float[512 * 512 * 4];
            _gl.ClearTexImage(_textureId, 0, PixelFormat.Rgba, PixelType.Float, colourData);
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
            _computeShader.Start();
            _gl.DispatchCompute(512, 512, 1);
            _gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
            
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
            _computeShader.Dispose();
            _gl.DeleteTexture(_textureId);
        }
        
    }
}