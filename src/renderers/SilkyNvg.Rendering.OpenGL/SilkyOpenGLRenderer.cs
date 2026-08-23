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

        private void FillPath(int firstVertex, uint vertexCount, int coverStart)
        {
            // _gl.Enable(EnableCap.StencilTest);
            _gl.ClearStencil(0);
            _gl.Clear(ClearBufferMask.StencilBufferBit);
            
            _gl.ColorMask(false, false, false, false);
            _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            
            // EvenOdd FillRule
            if (true)
            {
                _gl.StencilOp(StencilOp.Keep, StencilOp.Invert, StencilOp.Invert);
                _gl.StencilFunc(StencilFunction.Always, 0xFF, 0xFF);
                _gl.StencilMask(0xFF);
            }

            _gl.DrawArrays(PrimitiveType.Triangles, firstVertex, vertexCount);
            
            _gl.ColorMask(true, true, true, true);
            _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            
            // EvenOdd FillRule
            if (true)
            {
                _gl.StencilFunc(StencilFunction.Notequal, 0, 0xFF);
                _gl.StencilMask(0xFF);
            }
            
            _gl.DrawArrays(PrimitiveType.Triangles, coverStart, 6);

            /*_gl.Disable(EnableCap.StencilTest);
            
            _debugShader.Start();
            _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
            _gl.DrawArrays(PrimitiveType.Triangles, coverStart, 6);*/
            
        }

        public void Render(Vertex[] vertexData, uint vertexCount, IReadOnlyList<PathData> pathData)
        {
            _fillVao.Bind();
            
            _fillVbo.Store(vertexData, vertexCount, BufferUsageARB.DynamicDraw);
            _fillVao.VertexAttributePointer<float>(0, 2, VertexAttribPointerType.Float, 6, 0);
            _fillVao.VertexAttributePointer<float>(1, 3, VertexAttribPointerType.Float, 6, 2);
            _fillVao.VertexAttributeIPointer<int>(2, 1, VertexAttribIType.Int, 6, 5);
            
            _gl.Enable(EnableCap.StencilTest);
            _gl.Disable(EnableCap.DepthTest);
            _gl.Disable(EnableCap.CullFace);
            
            _shader.Start();
            
            _gl.EnableVertexAttribArray(0);
            _gl.EnableVertexAttribArray(1);
            _gl.EnableVertexAttribArray(2);
            
            foreach (var path in pathData)
            {
                FillPath(path.FirstVertex, path.VertexCount, path.CoverStart);
            }
            
            _gl.DisableVertexAttribArray(0);
            _gl.DisableVertexAttribArray(1);
            _gl.DisableVertexAttribArray(2);
            
            _shader.Stop();
            
            _gl.Disable(EnableCap.StencilTest);
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