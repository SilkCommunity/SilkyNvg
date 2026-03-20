using System;
using System.Collections.Generic;
using System.Numerics;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Buffers;
using SilkyNvg.Rendering.OpenGL.Shaders;
using Buffer = SilkyNvg.Rendering.OpenGL.Buffers.Buffer;

namespace SilkyNvg.Rendering.OpenGL
{
    public sealed class SilkyOpenGLRenderer : ISilkyRenderer
    {
        
        private static readonly uint[] QuadIndices = new uint[6] { 0, 1, 2, 1, 3, 2 };

        private readonly List<Scene> _scenes = new List<Scene>();
        
        private readonly Buffer _vertexBuffer;
        private readonly Buffer _boundingBoxBuffer;

        // Anchor geometry
        private readonly AnchorGeometryShader _anchorGeometryShader;
        private readonly VAO _anchorGeometryVertexArray;
        
        // Rendering a quad behind every path to apply the paint
        private readonly PaintingShader _paintingShader;
        private readonly Buffer _quadIndexBuffer;
        private readonly VAO _quadVertexArray;
        
        private readonly GL _gl;

        private RenderTolerances _tolerances;

        private Vector2 _viewSize;

        public unsafe SilkyOpenGLRenderer(GL gl)
        {
            _gl = gl;
            
            _vertexBuffer = new Buffer(BufferTargetARB.ArrayBuffer, BufferUsageARB.DynamicDraw, _gl);
            _boundingBoxBuffer = new Buffer(BufferTargetARB.ArrayBuffer, BufferUsageARB.DynamicDraw, _gl);

            // Anchor geometry stenciling
            _anchorGeometryShader = new AnchorGeometryShader(_gl);

            _anchorGeometryVertexArray = new VAO(_gl);

            _anchorGeometryVertexArray.Bind();
            _vertexBuffer.Bind();
            _anchorGeometryVertexArray.VertexAttributePointer<float>(0, 2, VertexAttribPointerType.Float, 2, 0);
            _vertexBuffer.Unbind();
            _anchorGeometryVertexArray.Unbind();
            
            // Set up drawing of a quad to paint paths
            _paintingShader = new PaintingShader(_gl);
            
            _quadVertexArray = new VAO(_gl);
            
            _quadVertexArray.Bind();
            _quadIndexBuffer = new Buffer(BufferTargetARB.ElementArrayBuffer, BufferUsageARB.StaticDraw, _gl);
            _quadIndexBuffer.UpdateDynamicRange(0, QuadIndices);
            _quadVertexArray.Unbind();
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
            _scenes.Clear();
        }

        public void AddScene(Scene scene)
        {
            _vertexBuffer.Update(0, scene.PointCount, scene.Points);
            _boundingBoxBuffer.Update(0, scene.PathCount, scene.PathBounds);
            
            _scenes.Add(scene);
        }

        private void RenderSubPath(SubPathData subPath)
        {
            // ---------- ANCHOR GEOMETRY STENCIL PASS ---------- //
            _anchorGeometryShader.Start();

            _anchorGeometryShader.LoadViewSize(_viewSize);

            _anchorGeometryVertexArray.Bind();
            _vertexBuffer.Bind();

            _gl.EnableVertexAttribArray(0);
            
            _gl.DrawArrays(PrimitiveType.TriangleFan, (int)subPath.AnchorGeometryIndex, subPath.AnchorGeometryCount);
            
            _gl.DisableVertexAttribArray(0);
            
            _vertexBuffer.Unbind();
            _anchorGeometryVertexArray.Unbind();
            
            _anchorGeometryShader.Stop();
        }

        private unsafe void RenderPath(Vector4 pathBounds, PathData path, ReadOnlySpan<SubPathData> subPaths)
        {
            // Disable colours for stencil pass
            _gl.ColorMask(false, false, false, false);
            
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

            _quadVertexArray.Bind();
            _quadIndexBuffer.Bind();
            
            _gl.DrawElements(PrimitiveType.Triangles, (uint)QuadIndices.Length, DrawElementsType.UnsignedInt,null);

            _quadIndexBuffer.Unbind();
            _quadVertexArray.Unbind();
            
            _paintingShader.Stop();
        }

        public void Render()
        {
            if (_scenes.Count == 0)
            {
                return;
            }
            
            _gl.Disable(EnableCap.CullFace);
            _gl.Disable(EnableCap.DepthTest);

            // Setup stencil test
            _gl.Enable(EnableCap.StencilTest);
            _gl.ClearStencil(0);
            _gl.Clear(ClearBufferMask.StencilBufferBit);
            
            foreach (Scene scene in _scenes)
            {
                for (int i = 0; i < scene.PathCount; i++)
                {
                    RenderPath(scene.PathBounds[i], scene.Paths[i], scene.SubPaths);
                }
            }
        }
            
        public void Dispose()
        {
            _vertexBuffer.Dispose();
            _boundingBoxBuffer.Dispose();
            
            _quadIndexBuffer.Dispose();
            _quadVertexArray.Dispose();
            _paintingShader.Dispose();

            _anchorGeometryVertexArray.Dispose();
            _anchorGeometryShader.Dispose();
        }
        
    }
}