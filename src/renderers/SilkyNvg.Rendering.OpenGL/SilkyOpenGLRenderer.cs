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
        
        private static readonly uint[] QuadIndices = new uint[6] { 0, 1, 3, 1, 2, 3 };

        private readonly List<Scene> _scenes = new List<Scene>();

        private readonly PaintingShader _paintingShader;
        
        private readonly Buffer _vertexBuffer;
        private readonly Buffer _boundingBoxBuffer;

        // Rendering a quad behind every path to apply the paint
        private readonly Buffer _quadIndexBuffer;
        private readonly VAO _quadVertexArray;
        
        private readonly GL _gl;

        private RenderTolerances _tolerances;

        private Vector2 _viewSize;

        public unsafe SilkyOpenGLRenderer(GL gl)
        {
            _gl = gl;

            _paintingShader = new PaintingShader(gl);
            
            _vertexBuffer = new Buffer(BufferTargetARB.ArrayBuffer, BufferUsageARB.DynamicDraw, _gl);
            _boundingBoxBuffer = new Buffer(BufferTargetARB.ArrayBuffer, BufferUsageARB.DynamicDraw, _gl);

            // Set up drawing of a quad to paint paths
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

        private unsafe void RenderPath(Vector4 pathBounds, PathData path)
        {
            // ---------- DRAWING BACKGROUND QUAD ---------- //
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
            foreach (Scene scene in _scenes)
            {
                for (int i = 0; i < scene.PathCount; i++)
                {
                    RenderPath(scene.PathBounds[i], scene.Paths[i]);
                }
            }
        }
            
        public void Dispose()
        {
            _quadVertexArray.Dispose();
            _quadIndexBuffer.Dispose();
            
            _vertexBuffer.Dispose();
            _boundingBoxBuffer.Dispose();

            _paintingShader.Dispose();
        }
        
    }
}