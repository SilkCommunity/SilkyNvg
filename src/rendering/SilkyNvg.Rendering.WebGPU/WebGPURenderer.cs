using System;
using System.Collections.Generic;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using SilkyNvg.Blending;
using SilkyNvg.Images;
using SilkyNvg.Rendering.WebGPU.Calls;
using SilkyNvg.Rendering.WebGPU.Shaders;

namespace SilkyNvg.Rendering.WebGPU
{
    public unsafe class WebGPURenderer : INvgRenderer
    {
        private readonly Silk.NET.WebGPU.WebGPU _wgpu;
        private readonly WebGPURendererParams _params;
        private CallQueue? _callQueue;
        private VertexCollection? _vertexCollection;
        private Shader? _shader;
        private SurfaceConfiguration? _surfaceConfiguration;
        private SurfaceCapabilities? _surfaceCapabilities;
        private RenderPipeline* _pipeline;

        private Vector2D<uint> _viewportSize = Vector2D<uint>.One;
        
        public Silk.NET.WebGPU.WebGPU WebGPU => _wgpu;
        public WebGPURendererParams Params => _params;
        public Shader? Shader => _shader;
        
        public bool EdgeAntiAlias { get; }
        
        public WebGPURenderer(Silk.NET.WebGPU.WebGPU wgpu, WebGPURendererParams @params)
        {
            _wgpu = wgpu;
            _params = @params;
        }
        
        public bool Create()
        {
            _callQueue = new CallQueue();
            _vertexCollection = new VertexCollection();
            _shader = new Shader("SilkyNvg-WebGPU-Shader", EdgeAntiAlias, this);
            
            SurfaceCapabilities surfaceCapabilities = new SurfaceCapabilities();
            _wgpu.SurfaceGetCapabilities(_params.Surface, _params.Adapter, ref surfaceCapabilities);
            _surfaceCapabilities = surfaceCapabilities;
            
            BlendState blendState = new BlendState
            {
                Color = new BlendComponent
                {
                    SrcFactor = Silk.NET.WebGPU.BlendFactor.One,
                    DstFactor = Silk.NET.WebGPU.BlendFactor.Zero,
                    Operation = BlendOperation.Add
                },
                Alpha = new BlendComponent
                {
                    SrcFactor = Silk.NET.WebGPU.BlendFactor.One,
                    DstFactor = Silk.NET.WebGPU.BlendFactor.Zero,
                    Operation = BlendOperation.Add
                }
            };

            ColorTargetState colorTargetState = new ColorTargetState
            {
                Format    = _surfaceCapabilities.Value.Formats[0],
                Blend     = &blendState,
                WriteMask = ColorWriteMask.All
            };

            FragmentState fragmentState = new FragmentState
            {
                Module      = _shader.Module,
                TargetCount = 1,
                Targets     = &colorTargetState,
                EntryPoint  = (byte*) SilkMarshal.StringToPtr("fs_main")
            };

            RenderPipelineDescriptor renderPipelineDescriptor = new RenderPipelineDescriptor
            {
                Vertex = new VertexState
                {
                    Module     = _shader.Module,
                    EntryPoint = (byte*) SilkMarshal.StringToPtr("vs_main"),
                },
                Primitive = new PrimitiveState
                {
                    Topology         = PrimitiveTopology.TriangleList,
                    StripIndexFormat = IndexFormat.Undefined,
                    FrontFace        = FrontFace.Ccw,
                    CullMode         = CullMode.None
                },
                Multisample = new MultisampleState
                {
                    Count                  = 1,
                    Mask                   = ~0u,
                    AlphaToCoverageEnabled = false
                },
                Fragment     = &fragmentState,
                DepthStencil = null
            };

            _pipeline = _wgpu.DeviceCreateRenderPipeline(_params.Device, renderPipelineDescriptor);
            
            CreateSwapchain();
            
            return true;
        }
        
        private void CreateSwapchain()
        {
            _surfaceConfiguration = new SurfaceConfiguration
            {
                Usage = TextureUsage.RenderAttachment, 
                Format = _surfaceCapabilities!.Value.Formats[0], 
                PresentMode = PresentMode.Fifo, 
                Device = _params.Device, 
                Width = _viewportSize.X,
                Height = _viewportSize.Y,
            };

            _wgpu.SurfaceConfigure(_params.Surface, _surfaceConfiguration.Value);
        }

        public int CreateTexture(Texture type, Vector2D<uint> size, ImageFlags imageFlags, ReadOnlySpan<byte> data)
        {
            throw new NotImplementedException();
        }

        public bool DeleteTexture(int image)
        {
            throw new NotImplementedException();
        }

        public bool UpdateTexture(int image, Rectangle<uint> bounds, ReadOnlySpan<byte> data)
        {
            throw new NotImplementedException();
        }

        public bool GetTextureSize(int image, out Vector2D<uint> size)
        {
            throw new NotImplementedException();
        }

        public void Viewport(Vector2D<float> size, float devicePixelRatio)
        {
            _viewportSize = size.As<uint>();
        }

        public void Cancel()
        {
            _vertexCollection!.Clear();
            _callQueue!.Clear();
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, Box2D<float> bounds,
            IReadOnlyList<Path> paths)
        {
            throw new NotImplementedException();
        }

        public void Stroke(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, float strokeWidth,
            IReadOnlyList<Path> paths)
        {
            throw new NotImplementedException();
        }

        public void Triangles(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, ICollection<Vertex> vertices,
            float fringeWidth)
        {
            throw new NotImplementedException();
        }
        
        public void Dispose()
        {
            _wgpu.RenderPipelineRelease(_pipeline);
            _shader?.Dispose();
            _vertexCollection?.Clear();
            _callQueue?.Clear();
        }
    }
}