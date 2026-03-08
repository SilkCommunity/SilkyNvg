using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Silk.NET.OpenGL;
using SilkyNvg.Renderers.OpenGL.Buffers;
using SilkyNvg.Renderers.OpenGL.Shaders;
using SilkyNvg.Rendering;

namespace SilkyNvg.Renderers.OpenGL
{
    public sealed class OpenGLRenderer : ISilkyRenderer
    {

        private readonly VAO _vao, _debugVao;
        private readonly VBO<Vector2> _vbo;
        private readonly VBO<Vector4> _debugVbo;

        private readonly SilkyShader _shader;
        private readonly ComputeShader _computeShader;
        
        private readonly GL _gl;

        public OpenGLRenderer(GL gl)
        {
            _gl = gl;

            var quad = new Vector4[]
            {
                new Vector4(-1.0f, 1.0f, 0.0f, 0.0f),
                new Vector4(1.0f, -1.0f, 1.0f, 1.0f),
                new Vector4(1.0f, 1.0f, 1.0f, 0.0f),
                
                new Vector4(-1.0f, 1.0f, 0.0f, 0.0f),
                new Vector4(-1.0f, -1.0f, 0.0f, 1.0f),
                new Vector4(1.0f, -1.0f, 1.0f, 1.0f)
            };
            
            _vao = new VAO(gl);
            _debugVao = new VAO(gl);
            _vbo = new VBO<Vector2>(gl);
            _debugVbo = new VBO<Vector4>(gl);
            _debugVbo.Bind();
            _debugVbo.Store(quad, BufferUsageARB.StaticDraw);
            _debugVao.Bind();
            _debugVao.AttribPointer(0, 2, VertexAttribPointerType.Float, 16, offset: 0);
            _debugVao.AttribPointer(1, 2, VertexAttribPointerType.Float, 16, offset: 8);
            _debugVbo.Unbind();
            _debugVao.Unbind();

            _shader = new SilkyShader(_gl);
            _computeShader = new ComputeShader(gl);
        }

        private float t = 0.0f;
        
        public void Render(FrameContainer container, Vector2 viewExtent)
        {
            t += 0.1f;
            
            // Compute shader
            _computeShader.StartCompute();
            _computeShader.LoadTime(t);
            
            _gl.DispatchCompute((uint)512, (uint)512, 1);

            _gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);

            _computeShader.StopCompute();
            
            // Debug compute
            _debugVao.Bind();
            _debugVbo.Bind();
            _computeShader.StartRender();
            _gl.EnableVertexAttribArray(0);
            _gl.EnableVertexAttribArray(1);
            
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, _computeShader.TextureID);
            
            _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
            
            _gl.DisableVertexAttribArray(0);
            _gl.DisableVertexAttribArray(1);
            _computeShader.StopRender();
            _debugVao.Unbind();
            _debugVbo.Unbind();
            
            // Rendering
            _vao.Bind();
            _vbo.Bind();
            _vbo.Store(container.Vertices.ToArray(), BufferUsageARB.DynamicDraw);
            _vao.AttribPointer(0, 2, VertexAttribPointerType.Float, 8);
            
            _shader.Start();
            _shader.LoadViewExtent(viewExtent);

            _gl.EnableVertexAttribArray(0);
            
            _gl.PointSize(4.0f);
            _gl.DrawArrays(PrimitiveType.Points, 0, (uint)container.Vertices.Count);
            
            _gl.DisableVertexAttribArray(0);
            _vbo.Unbind();
            _vao.Unbind();
            
            _shader.Stop();
        }

        public void Dispose()
        {
            _shader.Dispose();
            _computeShader.Dispose();
            _debugVbo.Dispose();
            _vbo.Dispose();
            _debugVao.Dispose();
            _vao.Dispose();
        }
        
    }
}