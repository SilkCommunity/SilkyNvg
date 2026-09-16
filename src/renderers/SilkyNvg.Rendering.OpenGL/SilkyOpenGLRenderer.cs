using System;
using System.Numerics;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Buffers;
using SilkyNvg.Rendering.OpenGL.Data;
using SilkyNvg.Rendering.OpenGL.Shaders;
using SilkyNvg.Rendering.OpenGL.Synchronization;

namespace SilkyNvg.Rendering.OpenGL
{
    public sealed class SilkyOpenGLRenderer : ISilkyRenderer
    {

        private static readonly Vector2[] Vertices =
        [
            new(-0.5f, 0.5f), new(-0.5f, -0.5f), new(0.5f, 0.5f),
            new(0.5f, 0.5f), new(-0.5f, -0.5f), new(0.5f, -0.5f)
        ];

        private readonly FrameManager _frameManager;
        private readonly SceneContainer _scene;
        
        private readonly Ssbo<int> _segmentPixelCount;
        private readonly Ssbo<SegmentMonotonicCutpoints> _monotonicCutpointCache;
        private readonly ComputeShader _makeIntersection0Shader;
        
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

            _frameManager = new FrameManager(3, _gl);
            _scene = new SceneContainer(_frameManager, gl);

            const BufferStorageMask storageMask = BufferStorageMask.MapReadBit;
            
            _segmentPixelCount = new Ssbo<int>(storageMask,
                _frameManager, "curve_pixel_count", _gl);
            _monotonicCutpointCache = new Ssbo<SegmentMonotonicCutpoints>(storageMask,
                _frameManager, "monotonic_cutpoint_cache", _gl);
            _makeIntersection0Shader = new ComputeShader(256, "shader_header.h.glsl", "make_intersection_0.comp.glsl", _gl);
            
            _vao = new Vao(_gl);
            _vao.Bind();

            _vbo = new Vbo(BufferTargetARB.ArrayBuffer, _gl);
            _vbo.Bind();
            _vbo.Store(Vertices, BufferUsageARB.StaticDraw);

            _vao.VertexAttributePointer<Vector2>(0, 2, VertexAttribPointerType.Float, 1, 0);

            _shader = new SimpleShader(_gl);
            _computeShader = new ComputeShader(0, null, "compute.comp.glsl", gl);

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
            _frameManager.BeginFrame();
            
            _scene.MakeCurrentFrameCurrent();
            _scene.Clear();
            
            _segmentPixelCount.MakeCurrentFrameCurrent();
            _monotonicCutpointCache.MakeCurrentFrameCurrent();
        }

        public void EndFrame()
        {
            _frameManager.EndFrame();
        }

        private void RasterizeImpl()
        {
            uint nVerts = _scene.VertexCount;
            uint nSegments = _scene.SegmentCount;
            uint nPaths = _scene.PathCount;
            
            // Calculate intersection times and number of intersections
            _makeIntersection0Shader.Start();
            _makeIntersection0Shader.LoadUInt(nSegments, "nSegments");
            
            _segmentPixelCount.EnsureCapacity(nSegments + 1);
            _segmentPixelCount.Bind(4);
            _monotonicCutpointCache.EnsureCapacity(nSegments);
            _monotonicCutpointCache.Bind(5);
            
            _makeIntersection0Shader.Dispatch(nSegments);
            
            // as a test, map buffers
            const MapBufferAccessMask accessMask = MapBufferAccessMask.ReadBit;
            Span<int> pcnt = _segmentPixelCount.Map(accessMask);
            Span<SegmentMonotonicCutpoints> monotonicCutpoints = _monotonicCutpointCache.Map(accessMask);
            
            _segmentPixelCount.Unmap();
            _monotonicCutpointCache.Unmap();
            
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
            _segmentPixelCount.Dispose();
            _monotonicCutpointCache.Dispose();
            _makeIntersection0Shader.Dispose();
            
            _scene.Dispose();
            _frameManager.Dispose();
            
            
            _vao.Dispose();
            _vbo.Dispose();
            _shader.Dispose();
            _computeShader.Dispose();
            _gl.DeleteTexture(_textureId);
        }
        
    }
}