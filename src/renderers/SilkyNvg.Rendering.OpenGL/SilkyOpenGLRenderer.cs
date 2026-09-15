using System;
using System.Collections.Generic;
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

        private Vbo _fillVbo;
        private Vao _fillVao;

        private SimpleShader _shader;
        private DebugShader _debugShader;
        
        // We need to render to a framebuffer to support all required blending operations
        private uint _framebufferId;
        
        public SilkyOpenGLRenderer(GL gl)
        {
            _gl = gl;

            _fillVao = new Vao(_gl);
            _fillVbo = new Vbo(BufferTargetARB.ArrayBuffer, _gl);
            
            _shader = new SimpleShader(_gl);
            _debugShader = new DebugShader(_gl);

            _framebufferId = _gl.GenFramebuffer();
        }

        public void Resize(uint width, uint height, RenderTolerances _)
        {
            _shader.Start();
            _shader.LoadViewSize(new Vector2(width, height));
            _shader.Stop();
            _debugShader.Start();
            _debugShader.LoadViewSize(new Vector2(width, height));
            _debugShader.Stop();
        }

        public void Dispose()
        {
            _gl.DeleteFramebuffer(_framebufferId);
            
            _fillVbo.Dispose();
            _fillVao.Dispose();
            _shader.Dispose();
            _debugShader.Dispose();
        }
        
    }
}