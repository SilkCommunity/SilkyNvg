// Veldrid backend implementation (C)opyright 2026 by David Jeske <davidj@gmail.com>
//   co-development with Claude Sonnet 4.5 and Claude Opus 4.6
// Released under the MIT License.

using SilkyNvg.Images;
using System;
using System.Collections.Generic;
using System.Drawing;
using Veldrid;

namespace SilkyNvg.Rendering.Veldrid
{
    /// <summary>
    /// Veldrid NanoVG renderer backend.
    /// Supports solid fill, stroke, and textured (font atlas) rendering via separate GPU pipelines.
    /// Split across partial files:
    ///   VeldridRenderer.cs          - Core fields, constructor, lifecycle
    ///   VeldridRenderer_Pipelines.cs - Pipeline/shader/buffer creation
    ///   VeldridRenderer_DrawCalls.cs - Fill(), Stroke(), Triangles() batching
    ///   VeldridRenderer_Flush.cs     - Flush() draw dispatch
    ///   TextureRegistry.cs           - Texture CRUD + ResourceSet caching (separate class)
    ///   Shaders/                     - GLSL source + C# struct layouts per shader
    /// </summary>
    ///
    /// <remarks>
    /// ╔══════════════════════════════════════════════════════════════════════════╗
    /// ║  CRITICAL: graphicsDevice.UpdateBuffer() vs commandList.UpdateBuffer()  ║
    /// ╚══════════════════════════════════════════════════════════════════════════╝
    ///
    /// Veldrid has TWO ways to update a GPU buffer. They look identical but have
    /// completely different timing semantics. Using the wrong one causes silent
    /// data corruption that is extremely hard to diagnose.
    ///
    /// ┌─────────────────────────────────────────────────────────────────────────┐
    /// │  _graphicsDevice.UpdateBuffer(buffer, offset, data)                    │
    /// │                                                                         │
    /// │  IMMEDIATE / GLOBAL — executes NOW, before any command list.            │
    /// │  The GPU sees the new data immediately. If you call this twice on the   │
    /// │  same buffer before submitting the command list, the GPU only sees the  │
    /// │  LAST value for ALL draw calls that reference this buffer.              │
    /// │                                                                         │
    /// │  ✅ USE FOR: One-time-per-frame data (viewSize, vertex upload)          │
    /// │  ❌ NEVER FOR: Per-draw-call uniforms (paint params, textures)          │
    /// └─────────────────────────────────────────────────────────────────────────┘
    ///
    /// ┌─────────────────────────────────────────────────────────────────────────┐
    /// │  commandList.UpdateBuffer(buffer, offset, data)                         │
    /// │                                                                         │
    /// │  SEQUENCED / PER-DRAW — recorded into the command list at this point.   │
    /// │  The GPU sees the data AT THIS POSITION in the command stream.          │
    /// │  Multiple updates to the same buffer are properly sequenced with draws. │
    /// │                                                                         │
    /// │  ✅ USE FOR: Per-draw-call uniforms (paint params that change each draw)│
    /// │  ❌ AVOID FOR: Large uploads (vertex buffers) — less efficient          │
    /// └─────────────────────────────────────────────────────────────────────────┘
    /// </remarks>
    public sealed partial class VeldridRenderer : INvgRenderer
    {
        // Adaptive buffer downsizing: evaluate every N frames to prevent permanent bloat from rare large draws
        private const int FLUSHES_BEFORE_DOWNSIZE_EVAL = 3000;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly bool _edgeAntiAlias;

        // Pipeline resources - solid fill (shapes)
        private Pipeline? _solidFillPipeline;
        private DeviceBuffer? _vertexBuffer;
        private DeviceBuffer? _viewSizeUniformBuffer;
        private ResourceLayout? _viewSizeOnlyResourceLayout;  // Shared: solidFill + stencilFill + stencilCover
        private ResourceSet? _viewSizeOnlyResourceSet;        // Shared: solidFill + stencilFill + stencilCover
        private Shader[]? _vertexColorShaders;                // Shared: solidFill + stencilFill + stencilCover

        // Pipeline resources - textured (font atlas text rendering)
        private Pipeline? _texturedPipeline;
        private ResourceLayout? _texturedResourceLayout;
        private Shader[]? _texturedShaders;
        private Sampler? _fontAtlasSampler;

        // Pipeline resources - gradient (linear, radial, box gradients)
        private Pipeline? _gradientPipeline;
        private ResourceLayout? _gradientResourceLayout;
        private ResourceSet? _gradientResourceSet;
        private Shader[]? _gradientShaders;
        private DeviceBuffer? _paintUniformBuffer;

        // Pipeline resources - stencil non-convex fill (two-pass stencil-then-cover)
        private Pipeline? _stencilFillPipeline;              // Pass 1: write stencil, no color
        private Pipeline? _stencilCoverSolidPipeline;         // Pass 2 (solid): fill where stencil != 0, clear stencil
        private Pipeline? _stencilCoverGradientPipeline;      // Pass 2 (gradient): gradient fill where stencil != 0
        private Pipeline? _stencilCoverImagePatternPipeline;  // Pass 2 (image): image pattern fill where stencil != 0

        // Pipeline resources - image pattern (RGBA texture fill with paintMat UV transform)
        private Pipeline? _imagePatternPipeline;
        private ResourceLayout? _imagePatternResourceLayout;
        private Shader[]? _imagePatternShaders;
        private Sampler? _imagePatternSampler;
        private readonly Dictionary<int, ResourceSet> _imagePatternResourceSetCache = new();

        // Texture management (font atlas, images, ResourceSet caching)
        private TextureRegistry _textureRegistry = null!; // Initialized in CreateBuffers()

        // Batching - zero-alloc vertex buffer (same strategy as OpenGL backend)
        private ShaderLayouts.NvgVertex[] _vertexBatchArray = new ShaderLayouts.NvgVertex[4096];
        private int _vertexBatchCount = 0;
        private readonly List<DrawCall> _drawCalls = new(64);

        // Adaptive buffer downsizing tracking (prevents permanent bloat from rare large draws)
        private int _flushesSinceLastBufferResize = 0;
        private int _peakVertexCountSinceLastResize = 0;
        private SizeF _viewportSize;
        private bool _isInitialized;

        // Active command list for rendering - MUST be set before BeginFrame/EndFrame!
        // Veldrid requires explicit CommandList management (unlike OpenGL's immediate mode).
        private CommandList? _activeRenderCommandList;

        public bool EdgeAntiAlias => _edgeAntiAlias;

        public VeldridRenderer(GraphicsDevice graphicsDevice, bool edgeAntiAlias = true)
        {
            _graphicsDevice = graphicsDevice;
            _edgeAntiAlias = edgeAntiAlias;
        }

        public bool Create()
        {
            if (_isInitialized)
            {
                return true;
            }

            try
            {
                Console.WriteLine($"VeldridRenderer.Create: Starting initialization for {_graphicsDevice.BackendType} backend");
                
                try
                {
                    Console.WriteLine("VeldridRenderer.Create: Creating shaders");
                    CreateShaders();
                }
                catch (Exception shaderEx)
                {
                    Console.Error.WriteLine($"VeldridRenderer.Create: Shader creation failed: {shaderEx.GetType().Name}: {shaderEx.Message}");
                    Console.Error.WriteLine($"Stack trace: {shaderEx.StackTrace}");
                    throw new InvalidOperationException($"Failed to create shaders for {_graphicsDevice.BackendType} backend", shaderEx);
                }
                
                try
                {
                    Console.WriteLine("VeldridRenderer.Create: Creating pipeline");
                    CreatePipeline();
                }
                catch (Exception pipelineEx)
                {
                    Console.Error.WriteLine($"VeldridRenderer.Create: Pipeline creation failed: {pipelineEx.GetType().Name}: {pipelineEx.Message}");
                    Console.Error.WriteLine($"Stack trace: {pipelineEx.StackTrace}");
                    throw new InvalidOperationException($"Failed to create pipeline for {_graphicsDevice.BackendType} backend", pipelineEx);
                }
                
                try
                {
                    Console.WriteLine("VeldridRenderer.Create: Creating buffers");
                    CreateBuffers();
                }
                catch (Exception bufferEx)
                {
                    Console.Error.WriteLine($"VeldridRenderer.Create: Buffer creation failed: {bufferEx.GetType().Name}: {bufferEx.Message}");
                    Console.Error.WriteLine($"Stack trace: {bufferEx.StackTrace}");
                    throw new InvalidOperationException($"Failed to create buffers for {_graphicsDevice.BackendType} backend", bufferEx);
                }
                
                _isInitialized = true;
                Console.WriteLine($"VeldridRenderer.Create: Successfully initialized for {_graphicsDevice.BackendType} backend");
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"VeldridRenderer.Create failed: {ex.GetType().Name}: {ex.Message}");
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Check for common issues based on backend type
                if (_graphicsDevice.BackendType == GraphicsBackend.Vulkan)
                {
                    Console.Error.WriteLine("Vulkan backend failure - possible causes:");
                    Console.Error.WriteLine("- Device may not support Vulkan or have proper drivers");
                    Console.Error.WriteLine("- Required Vulkan extensions may be missing");
                    Console.Error.WriteLine("- Memory allocation may have failed");
                }
                else if (_graphicsDevice.BackendType == GraphicsBackend.OpenGLES)
                {
                    Console.Error.WriteLine("OpenGLES backend failure - possible causes:");
                    Console.Error.WriteLine("- EGL context creation may have failed");
                    Console.Error.WriteLine("- Required OpenGLES version may not be supported");
                    Console.Error.WriteLine("- Shader compilation may have failed due to GLSL version");
                }
                
                return false;
            }
        }

        public void Viewport(SizeF size, float devicePixelRatio)
        {
            _viewportSize = size;
        }

        /// <summary>
        /// Sets the active CommandList for rendering. MUST be called before BeginFrame/EndFrame!
        /// This is Veldrid-specific - unlike OpenGL's immediate mode, Veldrid requires
        /// explicit CommandList management to ensure proper draw ordering.
        /// </summary>
        public void SetActiveCommandList(CommandList commandList)
        {
            _activeRenderCommandList = commandList;
        }

        public void Cancel()
        {
            // Track peak vertex usage for adaptive downsizing
            if (_vertexBatchCount > _peakVertexCountSinceLastResize)
            {
                _peakVertexCountSinceLastResize = _vertexBatchCount;
            }
            _flushesSinceLastBufferResize++;

            _vertexBatchCount = 0;
            _drawCalls.Clear();
        }

        // --- INvgRenderer texture methods delegate to TextureRegistry ---

        public int CreateTexture(Texture type, Size size, ImageFlags imageFlags, ReadOnlySpan<byte> data)
            => _textureRegistry.CreateTexture(type, size, imageFlags, data);

        public bool DeleteTexture(int textureId)
            => _textureRegistry.DeleteTexture(textureId);

        public bool UpdateTexture(int textureId, Rectangle bounds, ReadOnlySpan<byte> data)
            => _textureRegistry.UpdateTexture(textureId, bounds, data);

        public bool GetTextureSize(int textureId, out Size size)
            => _textureRegistry.GetTextureSize(textureId, out size);

        // --- Dispose ---

        public void Dispose()
        {
            _textureRegistry?.Dispose();

            // Dispose solid fill pipeline resources
            _solidFillPipeline?.Dispose();
            _viewSizeOnlyResourceSet?.Dispose();
            _viewSizeOnlyResourceLayout?.Dispose();
            if (_vertexColorShaders != null)
            {
                foreach (var shader in _vertexColorShaders)
                {
                    shader.Dispose();
                }
            }

            // Dispose textured pipeline resources
            _texturedPipeline?.Dispose();
            _texturedResourceLayout?.Dispose();
            _fontAtlasSampler?.Dispose();
            if (_texturedShaders != null)
            {
                foreach (var shader in _texturedShaders)
                {
                    shader.Dispose();
                }
            }

            // Dispose gradient pipeline resources
            _gradientPipeline?.Dispose();
            _gradientResourceSet?.Dispose();
            _gradientResourceLayout?.Dispose();
            _paintUniformBuffer?.Dispose();
            if (_gradientShaders != null)
            {
                foreach (var shader in _gradientShaders)
                {
                    shader.Dispose();
                }
            }

            // Dispose image pattern pipeline resources
            _imagePatternPipeline?.Dispose();
            _imagePatternResourceLayout?.Dispose();
            _imagePatternSampler?.Dispose();
            if (_imagePatternShaders != null)
            {
                foreach (var shader in _imagePatternShaders)
                {
                    shader.Dispose();
                }
            }
            foreach (var kvp in _imagePatternResourceSetCache)
            {
                kvp.Value.Dispose();
            }
            _imagePatternResourceSetCache.Clear();

            // Dispose stencil pipelines
            _stencilFillPipeline?.Dispose();
            _stencilCoverSolidPipeline?.Dispose();
            _stencilCoverGradientPipeline?.Dispose();
            _stencilCoverImagePatternPipeline?.Dispose();

            // Dispose shared resources
            _vertexBuffer?.Dispose();
            _viewSizeUniformBuffer?.Dispose();
        }
    }
}