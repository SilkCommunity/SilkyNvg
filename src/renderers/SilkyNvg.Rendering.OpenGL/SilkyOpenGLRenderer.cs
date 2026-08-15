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

        private Vbo _fillVbo;
        private Vao _fillVao;

        private SimpleShader _shader;
        private DebugShader _debugShader;
        
        public SilkyOpenGLRenderer(GL gl)
        {
            _gl = gl;

            _fillVao = new Vao(_gl);
            _fillVbo = new Vbo(BufferTargetARB.ArrayBuffer, _gl);
            
            _shader = new SimpleShader(_gl);
            _debugShader = new DebugShader(_gl);
        }

        public void FillPath(Vertex[] vertexData, uint vertexCount)
        {
            _fillVao.Bind();
            
            _fillVbo.Store(vertexData, vertexCount, BufferUsageARB.DynamicDraw);
            _fillVao.VertexAttributePointer<float>(0, 2, VertexAttribPointerType.Float, 5, 0);
            _fillVao.VertexAttributePointer<float>(1, 3, VertexAttribPointerType.Float, 5, 2);
            
            _shader.Start();
            _shader.LoadViewSize(new Vector2(1280f, 720f));
            
            _gl.EnableVertexAttribArray(0);
            _gl.EnableVertexAttribArray(1);

            _gl.Disable(EnableCap.DepthTest);
            _gl.Disable(EnableCap.CullFace);

            _gl.Enable(EnableCap.StencilTest);
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

            _gl.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
            
            _gl.ColorMask(true, true, true, true);
            _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            
            // EvenOdd FillRule
            if (true)
            {
                _gl.StencilFunc(StencilFunction.Notequal, 0, 0xFF);
                _gl.StencilMask(0xFF);
            }
            
            _gl.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);

            _gl.Disable(EnableCap.StencilTest);
            
            _debugShader.Start();
            _debugShader.LoadViewSize(new Vector2(1280f, 720f));
            _gl.LineWidth(15.0f);
            _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
            _gl.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
            
            _gl.DisableVertexAttribArray(0);
            _gl.DisableVertexAttribArray(1);
        }

        public void Dispose()
        {
            _fillVbo.Dispose();
            _fillVao.Dispose();
            _shader.Dispose();
        }
        
    }
}