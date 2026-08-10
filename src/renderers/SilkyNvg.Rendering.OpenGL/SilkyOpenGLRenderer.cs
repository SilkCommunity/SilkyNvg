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

        private Vbo _fillPointsVbo;
        private Vbo _fillCurveCoordsVbo;
        private Vao _fillVao;

        private SimpleShader _shader;
        
        public SilkyOpenGLRenderer(GL gl)
        {
            _gl = gl;

            _fillVao = new Vao(_gl);
            _fillVao.Bind();

            _fillPointsVbo = new Vbo(BufferTargetARB.ArrayBuffer, _gl);
            _fillCurveCoordsVbo = new Vbo(BufferTargetARB.ArrayBuffer, _gl);
            
            _shader = new SimpleShader(_gl);
        }

        public void FillPath(Vector2[] points, Vector3[] curveSpacePoints, int pointCount)
        {
            _fillVao.Bind();
            
            _fillPointsVbo.Bind();
            _fillPointsVbo.Store<Vector2>(points, (uint)pointCount, BufferUsageARB.DynamicDraw);
            _fillVao.VertexAttributePointer<Vector2>(0, 2, VertexAttribPointerType.Float, 1, 0);
            
            _fillCurveCoordsVbo.Bind();
            _fillCurveCoordsVbo.Store<Vector3>(curveSpacePoints, (uint)pointCount, BufferUsageARB.DynamicDraw);
            _fillVao.VertexAttributePointer<Vector3>(1, 3, VertexAttribPointerType.Float, 1, 0);
            
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

            _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)pointCount);
            
            _gl.ColorMask(true, true, true, true);
            
            // EvenOdd FillRule
            if (true)
            {
                _gl.StencilFunc(StencilFunction.Notequal, 0, 0xFF);
                _gl.StencilMask(0xFF);
            }
            
            _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)pointCount);
            
            _gl.DisableVertexAttribArray(0);
            _gl.DisableVertexAttribArray(1);
        }

        public void Dispose()
        {
            _fillPointsVbo.Dispose();
            _fillVao.Dispose();
            _shader.Dispose();
        }
        
    }
}