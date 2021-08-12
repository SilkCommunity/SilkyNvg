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

        internal bool TriangleListFill { get; }

        public bool EdgeAntiAlias { get; }

        public VulkanRenderer(VulkanRendererParams @params, CreateFlags flags, Queue queue, Vk vk)
        {
            Params = @params;
            Queue = queue;
            Vk = vk;

            _stencilStrokes = flags.HasFlag(CreateFlags.StencilStrokes);
            _debug = flags.HasFlag(CreateFlags.Debug);
            TriangleListFill = flags.HasFlag(CreateFlags.TriangleListFill);
            EdgeAntiAlias = flags.HasFlag(CreateFlags.Antialias) & !TriangleListFill;

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

            return true;
        }

        public int CreateTexture(Texture type, Vector2D<uint> size, ImageFlags imageFlags, ReadOnlySpan<byte> data)
        {
            Textures.Texture texture = new(size, imageFlags, type, data, this);
            return texture.ID;
        }

        public bool UpdateTexture(int image, Rectangle<uint> bounds, ReadOnlySpan<byte> data)
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
            _vertexCollection.Clear();
            _callQueue.Clear();
            Shader.UniformManager.Clear();
        }

        public unsafe void Flush()
        {
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
            Shader.UniformManager.Clear();
        }

        public void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, Box2D<float> bounds, IReadOnlyList<Rendering.Path> paths)
        {
            int offset = _vertexCollection.CurrentsOffset;
            Path[] renderPaths = new Path[paths.Count];
            for (int i = 0; i < paths.Count; i++)
            {
                Rendering.Path path = paths[i];
                renderPaths[i] = new Path(offset, TriangleListFill ? ((path.Fill.Count - 2) * 3) : path.Fill.Count, offset + path.Fill.Count, path.Stroke.Count);

                if (TriangleListFill)
                {
                    for (int j = 0; j < path.Fill.Count - 2; j++)
                    {
                        _vertexCollection.AddVertex(path.Fill[0]);
                        _vertexCollection.AddVertex(path.Fill[j + 1]);
                        _vertexCollection.AddVertex(path.Fill[j + 2]);
                        offset += 3;
                    }
                }
                else
                {
                    _vertexCollection.AddVertices(path.Fill);
                    offset += path.Fill.Count;
                }

                _vertexCollection.AddVertices(path.Stroke);
                offset += path.Stroke.Count;
            }

            FragUniforms uniforms = new(paint, scissor, fringe, fringe, -1.0f);
            Call call;
            if ((paths.Count == 1) && paths[0].Convex) // convex
            {
                int uniformOffset = Shader.UniformManager.AddUniform(uniforms);
                call = new ConvexFillCall(paint.Image, renderPaths, uniformOffset, compositeOperation, this);
            }
            else // not convex
            {
                _vertexCollection.AddVertex(new Vertex(bounds.Max, 0.5f, 1.0f));
                _vertexCollection.AddVertex(new Vertex(bounds.Max.X, bounds.Min.Y, 0.5f, 1.0f));
                _vertexCollection.AddVertex(new Vertex(bounds.Min.X, bounds.Max.Y, 0.5f, 1.0f));
                _vertexCollection.AddVertex(new Vertex(bounds.Min, 0.5f, 1.0f));

                FragUniforms stencilUniforms = new(-1.0f, ShaderType.Simple);
                int uniformOffset = Shader.UniformManager.AddUniform(stencilUniforms);
                _ = Shader.UniformManager.AddUniform(uniforms);

                call = new FillCall(paint.Image, renderPaths, offset, uniformOffset, compositeOperation, this);
            }

            _callQueue.Add(call);
        }

        public void Stroke(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, float strokeWidth, IReadOnlyList<Rendering.Path> paths)
        {
            int offset = _vertexCollection.CurrentsOffset;
            Path[] renderPaths = new Path[paths.Count];
            for (int i = 0; i < paths.Count; i++)
            {
                Rendering.Path path = paths[i];
                renderPaths[i] = new Path(0, 0, offset, path.Stroke.Count);
                _vertexCollection.AddVertices(path.Stroke);
                offset += path.Stroke.Count;
            }

            FragUniforms uniforms = new(paint, scissor, strokeWidth, fringe, -1.0f);
            Call call;
            if (_stencilStrokes)
            {
                FragUniforms stencilUniforms = new(paint, scissor, strokeWidth, fringe, 1.0f - 0.5f / 255.0f);
                int uniformOffset = Shader.UniformManager.AddUniform(uniforms);
                _ = Shader.UniformManager.AddUniform(stencilUniforms);

                call = new StencilStrokeCall(paint.Image, renderPaths, uniformOffset, compositeOperation, this);
            }
            else
            {
                int uniformOffset = Shader.UniformManager.AddUniform(uniforms);
                call = new StrokeCall(paint.Image, renderPaths, uniformOffset, compositeOperation, this);
            }

            _callQueue.Add(call);
        }

        public void Triangles(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, ICollection<Vertex> vertices, float fringeWidth)
        {
            int offset = _vertexCollection.CurrentsOffset;
            _vertexCollection.AddVertices(vertices);

            FragUniforms uniforms = new(paint, scissor, 1.0f);
            int uniformOffset = Shader.UniformManager.AddUniform(uniforms);
            Call call = new TrianglesCall(paint.Image, compositeOperation, offset, (uint)vertices.Count, uniformOffset, this);
            _callQueue.Add(call);
        }

        public unsafe void Dispose()
        {
            Device device = Params.device;
            AllocationCallbacks* allocator = Params.allocator;

            Textures.Texture.DeleteAll();

            _vertexBuffer.Dispose();

            Shader.Dispose();

            Vk.DestroyPipelineLayout(device, PipelineLayout, allocator);

            _pipelineManager.Dispose();
        }

    }
}
