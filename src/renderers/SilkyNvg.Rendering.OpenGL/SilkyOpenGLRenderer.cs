using System;
using System.Collections.Generic;
using System.Numerics;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Buffers;
using SilkyNvg.Rendering.OpenGL.Shaders;

namespace SilkyNvg.Rendering.OpenGL
{
    public sealed class SilkyOpenGLRenderer : ISilkyRenderer
    {
        
        private static readonly uint[] QuadIndices = new uint[6] { 0, 1, 2, 1, 3, 2 };

        private readonly PaintingShader _paintingShader;
        private readonly AnchorGeometryShader _anchorGeometryShader;

        private readonly Ssbo _pointsSsbo;
        
        private readonly Vao _vao;
        
        private readonly IndexBuffer _quadIndexBuffer;
        private readonly IndexBuffer _anchorGeometryIndexBuffer;
        
        private readonly GL _gl;

        private RenderTolerances _tolerances;

        private Vector2 _viewSize;
        private Scene _scene;

        public unsafe SilkyOpenGLRenderer(GL gl)
        {
            _gl = gl;
            
            _paintingShader = new PaintingShader(_gl);
            _anchorGeometryShader = new AnchorGeometryShader(_gl);

            _pointsSsbo = new Ssbo(0, _gl);
            
            _vao = new Vao(_gl);
            _vao.Bind();
            
            _quadIndexBuffer = new IndexBuffer(BufferUsageARB.StaticDraw, _gl);
            _quadIndexBuffer.Load((uint)QuadIndices.Length, QuadIndices);
            
            _anchorGeometryIndexBuffer = new IndexBuffer(BufferUsageARB.DynamicDraw, _gl);
        }

        public void Init(RenderTolerances tolerances)
        {
            _tolerances = tolerances;
        }

        public void Resize(float width, float height, float pixelRatio)
        {
            _viewSize = new Vector2(width, height);
        }

        public void PrepareRender()
        {
            
        }

        public void SetScene(Scene scene)
        {
            _scene = scene;
            
            _pointsSsbo.Store(BufferStorageMask.DynamicStorageBit, scene.PointCount, scene.Points);
            _anchorGeometryIndexBuffer.Load(scene.AnchorGeometryIndexCount, scene.AnchorGeometryIndices);
        }

        private unsafe void RenderSubPath(SubPathData subPath)
        {
            /*// ---------- ANCHOR GEOMETRY STENCIL PASS ---------- //
            if (subPath.AnchorGeometryCount > 0)
            {
                _anchorGeometryShader.Start();
                _anchorGeometryShader.LoadViewSize(_viewSize);

                _anchorGeometryIndexBuffer.Bind();

                _gl.EnableVertexAttribArray(0);
            
                _gl.DrawElements(PrimitiveType.TriangleFan, subPath.AnchorGeometryCount, DrawElementsType.UnsignedInt,
                    (void*)(subPath.AnchorGeometryIndex * sizeof(uint)));
                
                _gl.DisableVertexAttribArray(0);

                _anchorGeometryIndexBuffer.Unbind();
            
                _anchorGeometryShader.Stop();
            }
            
            // ---------- QUADRATIC DISCARD STENCIL PASS ---------- //
            if (subPath.QuadraticDiscardTrianglesCount > 0)
            {
                _quadraticDiscardShader.Start();
                _quadraticDiscardShader.LoadViewSize(_viewSize);
                
                _quadraticDiscardIndexBuffer.Bind();

                _gl.EnableVertexAttribArray(0);
                
                _gl.DrawElements(PrimitiveType.Triangles, subPath.QuadraticDiscardTrianglesCount, DrawElementsType.UnsignedInt,
                    (void*)(subPath.QuadraticDiscardTrianglesIndex * sizeof(uint)));
                
                _gl.DisableVertexAttribArray(0);
                
                _quadraticDiscardIndexBuffer.Unbind();
                
                _quadraticDiscardShader.Stop();
            }*/

            if (subPath.AnchorGeometryCount > 0)
            {
                _anchorGeometryShader.Start();
                _anchorGeometryShader.LoadViewSize(_viewSize);

                _anchorGeometryIndexBuffer.Bind();
                
                _gl.DrawElements(PrimitiveType.TriangleFan, subPath.AnchorGeometryCount, DrawElementsType.UnsignedInt,
                    (void*)(subPath.AnchorGeometryIndex * sizeof(uint)));
            }
        }

        private unsafe void RenderPath(Vector4 pathBounds, PathData path, ReadOnlySpan<SubPathData> subPaths)
        {
            // Disable colours for stencil pass
            _gl.ColorMask(false, false, false, false);
            
            _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            
            // EvenOdd Fill Rule
            if (true)
            {
                _gl.StencilOp(StencilOp.Keep, StencilOp.Invert, StencilOp.Invert);
                _gl.StencilFunc(StencilFunction.Always, 0xFF, 0xFF);
                _gl.StencilMask(0xFF);

            }
            
            for (int i = 0; i < path.SubPathCount; i++)
            {
                RenderSubPath(subPaths[(int)path.SubPathIndex + i]);
            }
            
            // ---------- DRAWING BACKGROUND QUAD ---------- //
            _gl.ColorMask(true, true, true, true);

            // EvenOdd Fill Rule
            if (true)
            {
                _gl.StencilFunc(StencilFunction.Notequal, 0, 0xFF);
                _gl.StencilMask(0xFF);
            }
            
            _paintingShader.Start();
            
            _paintingShader.LoadPathBounds(pathBounds);
            _paintingShader.LoadViewSize(_viewSize);

            _vao.Bind();
            _quadIndexBuffer.Bind();
            
            _gl.DrawElements(PrimitiveType.Triangles, (uint)QuadIndices.Length, DrawElementsType.UnsignedInt,null);
        }

        public void Render()
        {
            if (_scene is null)
            {
                return;
            }
            
            _gl.Disable(EnableCap.CullFace);
            _gl.Disable(EnableCap.DepthTest);

            // Setup stencil test
            _gl.Enable(EnableCap.StencilTest);
            _gl.ClearStencil(0);
            _gl.Clear(ClearBufferMask.StencilBufferBit);

            _vao.Bind();            
            for (int i = 0; i < _scene.PathCount; i++)
            {
                RenderPath(_scene.PathBounds[i], _scene.Paths[i], _scene.SubPaths);
            }
        }
            
        public void Dispose()
        {
            _quadIndexBuffer.Dispose();
            
            _vao.Dispose();

            _pointsSsbo.Dispose();

            _anchorGeometryShader.Dispose();
            _paintingShader.Dispose();
        }
        
    }
}