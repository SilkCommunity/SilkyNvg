using Silk.NET.Maths;
using Silk.NET.Vulkan;
using SilkyNvg.Blending;
using SilkyNvg.Images;
using SilkyNvg.Rendering.Vulkan.Calls;
using SilkyNvg.Rendering.Vulkan.Pipelines;
using SilkyNvg.Rendering.Vulkan.Shaders;
using SilkyNvg.Rendering.Vulkan.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Queue = Silk.NET.Vulkan.Queue;

namespace SilkyNvg.Rendering.Vulkan
{
    public sealed class VulkanRenderer : INvgRenderer
    {

        internal void Assert(Result result)
        {
            if (result != Result.Success && _debug)
            {
                StackFrame frame = new(1);
                Console.Error.WriteLine("Vulkan failed at " + frame.GetMethod().Name + " in " + frame.GetMethod().DeclaringType.FullName + " with result " + result + "!");
                Environment.Exit(-1);
            }
        }

        private readonly bool _stencilStrokes;
        private readonly bool _debug;

        private readonly VertexCollection _vertexCollection;
        private readonly CallQueue _callQueue;
        private readonly PipelineManager _pipelineManager;

        private PhysicalDeviceProperties _gpuProperties;
        private PhysicalDeviceMemoryProperties _memoryProperties;

        private Vector2D<float> _view;
        private Buffer<Vertex> _vertexBuffer;
        private Pipelines.Pipeline _currentPipeline;

        internal VulkanRendererParams Params { get; }

        internal Queue Queue { get; }

        internal Vk Vk { get; }

        internal PhysicalDeviceProperties GpuProperties => _gpuProperties;

        internal PhysicalDeviceMemoryProperties MemoryProperties => _memoryProperties;

        internal Shader Shader { get; private set; }

        internal PipelineLayout PipelineLayout { get; private set; }

        internal Buffer<Vertex> VertexBuffer => _vertexBuffer;

        public bool EdgeAntiAlias { get; }

        public VulkanRenderer(VulkanRendererParams @params, CreateFlags flags, Queue queue, Vk vk)
        {
            Params = @params;
            Queue = queue;
            Vk = vk;

            _stencilStrokes = flags.HasFlag(CreateFlags.StencilStrokes);
            _debug = flags.HasFlag(CreateFlags.Debug);
            EdgeAntiAlias = flags.HasFlag(CreateFlags.Antialias);

            _view = default;
            _vertexBuffer = default;
            _currentPipeline = null;

            _vertexCollection = new VertexCollection();
            _callQueue = new CallQueue();
            _pipelineManager = new PipelineManager(this);
        }

        private unsafe PipelineLayout CreatePipelineLayout(DescriptorSetLayout descLayout)
        {
            PipelineLayoutCreateInfo pipelineLayoutCreateInfo = new(StructureType.PipelineLayoutCreateInfo)
            {
                SetLayoutCount = 1,
                PSetLayouts = &descLayout
            };

            Assert(Vk.CreatePipelineLayout(Params.device, pipelineLayoutCreateInfo, Params.allocator, out PipelineLayout pipelineLayout));
            return pipelineLayout;
        }

        internal Result MemoryTypeFromProperties(uint typeBits, MemoryPropertyFlags requirementsMask, out uint typeIndex)
        {
            for (uint i = 0; i < _memoryProperties.MemoryTypeCount; i++)
            {
                if ((typeBits & 1) == 1)
                {
                    if (_memoryProperties.MemoryTypes[(int)i].PropertyFlags.HasFlag(requirementsMask))
                    {
                        typeIndex = i;
                        return Result.Success;
                    }
                }
                typeBits >>= 1;
            }

            typeIndex = 0;
            return Result.ErrorFormatNotSupported;
        }

        internal Silk.NET.Vulkan.Pipeline BindPipeline(CommandBuffer cmdBuffer, PipelineKey pipelineKey)
        {
            Pipelines.Pipeline pipeline = _pipelineManager.FindPipeline(pipelineKey);
            if (pipeline == null)
            {
                _pipelineManager.AddPipeline(pipelineKey);
                pipeline = _pipelineManager.FindPipeline(pipelineKey);
            }
            if (pipeline != _currentPipeline)
            {
                pipeline.Bind(cmdBuffer);
                _currentPipeline = pipeline;
            }
            return pipeline.Handle;
        }

        public bool Create()
        {
            Vk.GetPhysicalDeviceMemoryProperties(Params.gpu, out _memoryProperties);
            Vk.GetPhysicalDeviceProperties(Params.gpu, out _gpuProperties);

            Shader = new Shader(this);

            PipelineLayout = CreatePipelineLayout(Shader.DescLayout);

            // Dummy tex will always be at index 0.
            _ = CreateTexture(Texture.Alpha, new Vector2D<uint>(1, 1), 0, null);

            return true;
        }

        public int CreateTexture(Texture type, Vector2D<uint> size, ImageFlags imageFlags, ReadOnlySpan<byte> data)
        {
            Textures.Texture texture = new(size, imageFlags, type, data, this);
            return texture.ID;
        }

        public bool UpdateTexture(int image, Vector4D<uint> bounds, ReadOnlySpan<byte> data)
        {
            Textures.Texture tex = Textures.Texture.FindTexture(image);
            if (tex == null)
            {
                return false;
            }
            tex.Update(Params.device, bounds, data);
            return true;
        }

        public bool GetTextureSize(int image, out Vector2D<uint> size)
        {
            Textures.Texture tex = Textures.Texture.FindTexture(image);
            if (tex == null)
            {
                size = default;
                return false;
            }
            size = tex.Size;
            return true;
        }

        public bool DeleteTexture(int image)
        {
            Textures.Texture tex = Textures.Texture.FindTexture(image);
            if (tex == null)
            {
                return false;
            }
            tex.Dispose();
            return true;
        }

        public void Viewport(Vector2D<float> size, float _)
        {
            _view = size;
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public unsafe void Flush()
        {
            Device device = Params.device;
            CommandBuffer cmdBuffer = Params.cmdBuffer;
            RenderPass renderpass = Params.renderpass;
            AllocationCallbacks* allocator = Params.allocator;

            if (_callQueue.HasCalls)
            {
                Buffer<Vertex>.UpdateBuffer(ref _vertexBuffer, BufferUsageFlags.BufferUsageVertexBufferBit,
                    MemoryPropertyFlags.MemoryPropertyHostVisibleBit, _vertexCollection.Vertices, this);
                Shader.UpdateBuffers(_view);
                _currentPipeline = default;

                Shader.ValidateDescriptorPool(_callQueue.Count);

                _callQueue.Run();
            }

            _vertexCollection.Clear();
            _callQueue.Clear();
            Shader.UniformManager.Clear();
        }

        public void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, Vector4D<float> bounds, IReadOnlyList<Rendering.Path> paths)
        {
            int offset = _vertexCollection.CurrentsOffset;
            Path[] renderPaths = new Path[paths.Count];
            for (int i = 0; i < paths.Count; i++)
            {
                Rendering.Path path = paths[i];
                renderPaths[i] = new Path(offset, (path.Fill.Count - 2) * 3, offset + path.Fill.Count, path.Stroke.Count);

                for (int j = 0; j < paths[i].Fill.Count - 2; j++)
                {
                    _vertexCollection.AddVertex(paths[i].Fill[0]);
                    _vertexCollection.AddVertex(paths[i].Fill[j + 1]);
                    _vertexCollection.AddVertex(paths[i].Fill[j + 2]);
                    offset += 3;
                }

                _vertexCollection.AddVertices(path.Stroke);
                offset += path.Stroke.Count;
            }

            FragUniforms uniforms = new(paint, scissor, fringe, fringe, -1.0f);
            Call call;
            if ((paths.Count) == 1 && paths[0].Convex) // convex
            {
                int uniformOffset = Shader.UniformManager.AddUniform(uniforms);
                call = new ConvexFillCall(paint.Image, renderPaths, uniformOffset, compositeOperation, this);
            }
            else // not convex
            {
                call = null;
            }

            _callQueue.Add(call);
        }

        public void Stroke(Paint strokePaint, CompositeOperationState compositeOperation, Scissor scissor, float fringeWidth, float strokeWidth, IReadOnlyList<SilkyNvg.Rendering.Path> paths)
        {
            throw new NotImplementedException();
        }

        public void Triangles(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, ICollection<Vertex> vertices, float fringeWidth)
        {
            throw new NotImplementedException();
        }

        public unsafe void Dispose()
        {
            Device device = Params.device;
            AllocationCallbacks* allocator = Params.allocator;

            Textures.Texture.DeleteAll();

            _vertexBuffer.Dispose();

            Shader.Dispose();

            Vk.DestroyPipelineLayout(device, PipelineLayout, allocator);
        }

    }
}
