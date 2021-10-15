using Silk.NET.Maths;
using Silk.NET.Vulkan;
using SilkyNvg.Blending;
using SilkyNvg.Images;
using SilkyNvg.Rendering.Vulkan.Calls;
using SilkyNvg.Rendering.Vulkan.Shaders;
using SilkyNvg.Rendering.Vulkan.Textures;
using System;
using System.Collections.Generic;

namespace SilkyNvg.Rendering.Vulkan
{
    public sealed class VulkanRenderer : INvgRenderer
    {

        private readonly CreateFlags _flags;
        private readonly VertexCollection _vertexCollection;
        private readonly CallQueue _callQueue;

        private readonly Frame[] _frames;

        private PhysicalDeviceMemoryProperties _physicalDeviceMemoryProperties;

        private Vector2D<float> _viewSize;
        private uint _requireredDescriptorSetCount;

        internal Vk Vk { get; }

        internal VulkanRendererParams Params { get; }

        internal Shader Shader { get; private set; }

        internal CommandPool ImageTransitionPool { get; private set; }

        internal Queue ImageTransitionQueue { get; private set; }

        internal bool Debug => _flags.HasFlag(CreateFlags.Debug);

        internal bool StencilStrokes => _flags.HasFlag(CreateFlags.StencilStrokes);

        internal bool TriangleListFill => _flags.HasFlag(CreateFlags.TriangleListFill);

        internal TextureManager TextureManager { get; }

        internal int DummyTex { get; private set; }

        public uint CurrentFrameIndex { private get; set; }

        public CommandBuffer CurrentCommandBuffer { private get; set; }

        public bool EdgeAntiAlias => _flags.HasFlag(CreateFlags.Antialias);

        public VulkanRenderer(CreateFlags flags, VulkanRendererParams @params, Vk vk)
        {
            _flags = flags;
            Vk = vk;
            Params = @params;

            _vertexCollection = new VertexCollection();
            _callQueue = new CallQueue();

            _frames = new Frame[Params.FrameCount];

            CurrentFrameIndex = 0;
            CurrentCommandBuffer = Params.InitialCommandBuffer;
            _requireredDescriptorSetCount = 0;

            VulkanRenderExtensionMethodContainer.VulkanRenderer = this;

            TextureManager = new TextureManager(this);
        }

        internal void AssertVulkan(Result result)
        {
            if (!Debug)
            {
                return;
            }

            if (result != Result.Success)
            {
                Console.Error.WriteLine("Error " + result + ".");
            }
        }

        internal uint FindMemoryTypeIndex(uint typeFilter, MemoryPropertyFlags properties)
        {
            for (uint i = 0; i < _physicalDeviceMemoryProperties.MemoryTypeCount; i++)
            {
                if (((typeFilter & 1) == 1) & ((_physicalDeviceMemoryProperties.MemoryTypes[(int)i].PropertyFlags & properties) == properties))
                {
                    return i;
                }
            }

            throw new MissingMemberException("No compatible memory type found!");
        }

        private unsafe void InitializeImageTransition()
        {
            Device device = Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)Params.AllocationCallbacks.ToPointer();

            CommandPoolCreateInfo commandPoolCreateInfo = new()
            {
                SType = StructureType.CommandPoolCreateInfo,
                Flags = CommandPoolCreateFlags.CommandPoolCreateResetCommandBufferBit,

                QueueFamilyIndex = Params.ImageQueueFamily
            };

            AssertVulkan(Vk.CreateCommandPool(device, commandPoolCreateInfo, allocator, out CommandPool pool));
            ImageTransitionPool = pool;

            Vk.GetDeviceQueue(device, Params.ImageQueueFamily, Params.ImageQueueFamilyIndex, out Queue queue);
            ImageTransitionQueue = queue;
        }

        public bool Create()
        {
            Vk.GetPhysicalDeviceMemoryProperties(Params.PhysicalDevice, out _physicalDeviceMemoryProperties);

            Shader = new Shader("SilkyNvg-Vulkan-Shader", EdgeAntiAlias, this);
            if (!Shader.Status)
            {
                return false;
            }

            Shader.CreateLayout();

            for (int i = 0; i < _frames.Length; i++)
            {
                _frames[i] = new Frame(this);
            }

            Shader.InitializeFragUniformBuffers();

            InitializeImageTransition();

            DummyTex = CreateTexture(Texture.Alpha, new Vector2D<uint>(1, 1), 0, null);

            return true;
        }

        public int CreateTexture(Texture type, Vector2D<uint> size, ImageFlags imageFlags, ReadOnlySpan<byte> data)
        {
            ref var tex = ref TextureManager.AllocTexture();
            tex.Load(size, imageFlags, type, data);
            return tex.Id;
        }

        public bool UpdateTexture(int image, Rectangle<uint> bounds, ReadOnlySpan<byte> data)
        {
            ref var tex = ref TextureManager.FindTexture(image);
            if (tex.Id == 0)
            {
                return false;
            }
            tex.Update(bounds, data);
            return true;
        }

        public bool GetTextureSize(int image, out Vector2D<uint> size)
        {
            ref var tex = ref TextureManager.FindTexture(image);
            if (tex.Id == 0)
            {
                size = default;
                return false;
            }
            size = tex.Size;
            return true;
        }

        public bool DeleteTexture(int image)
        {
            return TextureManager.DeleteTexture(image);
        }

        public void Viewport(Vector2D<float> size, float _)
        {
            _viewSize = size;
        }

        public void Cancel()
        {
            _vertexCollection.Clear();
            _callQueue.Clear();
            Shader.UniformManager.Clear();
        }

        internal void AdvanceFrame()
        {
            CurrentFrameIndex++;
            if (CurrentFrameIndex >= _frames.Length)
            {
                CurrentFrameIndex = 0;
            }
        }

        public void Flush()
        {
            if (_callQueue.HasCalls)
            {
                Frame frame = _frames[CurrentFrameIndex];

                frame.VertexBuffer.Update(_vertexCollection.Vertices);

                VertUniforms vertUniforms = new()
                {
                    ViewSize = _viewSize
                };
                frame.VertexUniformBuffer.Update(vertUniforms);

                Viewport viewport = new(0.0f, 0.0f, _viewSize.X, _viewSize.Y);
                Rect2D scissor = new(new Offset2D(0, 0), new Extent2D((uint)_viewSize.X, (uint)_viewSize.Y));
                Vk.CmdSetViewport(CurrentCommandBuffer, 0, 1, viewport);
                Vk.CmdSetScissor(CurrentCommandBuffer, 0, 1, scissor);

                frame.DescriptorSetManager.Reset(_requireredDescriptorSetCount);
                frame.FragmentUniformBuffer.Update(Shader.UniformManager.Uniforms);

                Vk.CmdBindVertexBuffers(CurrentCommandBuffer, 0, 1, frame.VertexBuffer.Handle, 0);

                _callQueue.Run(frame, CurrentCommandBuffer);
            }

            _vertexCollection.Clear();
            _callQueue.Clear();
            Shader.UniformManager.Clear();
            _requireredDescriptorSetCount = 0;

            if (Params.AdvanceFrameIndexAutomatically)
            {
                AdvanceFrame();
            }
        }

        public void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, Box2D<float> bounds, IReadOnlyList<Rendering.Path> paths)
        {
            int offset = _vertexCollection.CurrentsOffset;
            Path[] renderPaths = new Path[paths.Count];
            for (int i = 0; i < paths.Count; i++)
            {
                Rendering.Path path = paths[i];
                int verticesPresentCount = _vertexCollection.CurrentsOffset;
                int fillCount;
                if (TriangleListFill)
                {
                    for (int j = 0; j < path.Fill.Count - 2; j++)
                    {
                        _vertexCollection.AddVertex(path.Fill[0]);
                        _vertexCollection.AddVertex(path.Fill[j + 1]);
                        _vertexCollection.AddVertex(path.Fill[j + 2]);
                        offset += 3;
                    }
                    fillCount = (path.Fill.Count - 2) * 3;
                }
                else
                {
                    _vertexCollection.AddVertices(path.Fill);
                    fillCount = path.Fill.Count;
                    offset += fillCount;
                }
                renderPaths[i] = new Path(
                    verticesPresentCount, fillCount,
                    verticesPresentCount + fillCount, path.Stroke.Count
                );
                _vertexCollection.AddVertices(path.Stroke);
                offset += path.Stroke.Count;
            }

            FragUniforms uniforms = new(paint, scissor, fringe, fringe, -1.0f, this);
            Call call;
            if ((paths.Count == 1) && paths[0].Convex) // Convex
            {
                _requireredDescriptorSetCount++;

                ulong uniformOffset = Shader.UniformManager.AddUniform(uniforms);
                call = new ConvexFillCall(paint.Image, renderPaths, uniformOffset, compositeOperation, this);
            }
            else
            {
                _requireredDescriptorSetCount += 2;

                _vertexCollection.AddVertex(new Vertex(bounds.Max, 0.5f, 1.0f));
                _vertexCollection.AddVertex(new Vertex(bounds.Max.X, bounds.Min.Y, 0.5f, 1.0f));
                _vertexCollection.AddVertex(new Vertex(bounds.Min.X, bounds.Max.Y, 0.5f, 1.0f));
                _vertexCollection.AddVertex(new Vertex(bounds.Min, 0.5f, 1.0f));

                FragUniforms stencilUniforms = new(-1.0f, ShaderType.Simple);
                ulong uniformOffset = Shader.UniformManager.AddUniform(stencilUniforms);
                _ = Shader.UniformManager.AddUniform(uniforms);

                call = new FillCall(paint.Image, renderPaths, (uint)offset, uniformOffset, compositeOperation, this);
            }

            _callQueue.Add(call);
        }

        public void Stroke(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, float strokeWidth, IReadOnlyList<Rendering.Path> paths)
        {
            int offset = _vertexCollection.CurrentsOffset;
            Path[] renderPaths = new Path[paths.Count];
            for (int i = 0; i < paths.Count; i++)
            {
                if (paths[i].Stroke.Count > 0)
                {
                    renderPaths[i] = new Path(0, 0, offset, paths[i].Stroke.Count);
                }
                else
                {
                    renderPaths[i] = default;
                }
                _vertexCollection.AddVertices(paths[i].Stroke);
                offset += paths[i].Stroke.Count;
            }

            FragUniforms uniforms = new(paint, scissor, strokeWidth, fringe, -1.0f, this);
            Call call;
            if (StencilStrokes)
            {
                _requireredDescriptorSetCount += 2;

                FragUniforms stencilUniforms = new(paint, scissor, strokeWidth, fringe, 1.0f - 0.5f / 255.0f, this);
                ulong uniformOffset = Shader.UniformManager.AddUniform(uniforms);
                _ = Shader.UniformManager.AddUniform(stencilUniforms);

                call = new StencilStrokeCall(paint.Image, renderPaths, uniformOffset, compositeOperation, this);
            }
            else
            {
                _requireredDescriptorSetCount++;

                ulong uniformOffset = Shader.UniformManager.AddUniform(uniforms);
                call = new StrokeCall(paint.Image, renderPaths, uniformOffset, compositeOperation, this);
            }
            _callQueue.Add(call);
        }

        public void Triangles(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, ICollection<Vertex> vertices, float fringe)
        {
            uint offset = (uint)_vertexCollection.CurrentsOffset;
            _vertexCollection.AddVertices(vertices);

            _requireredDescriptorSetCount++;

            FragUniforms uniforms = new(paint, scissor, fringe, this);
            ulong uniformOffset = Shader.UniformManager.AddUniform(uniforms);
            Call call = new TrianglesCall(paint.Image, compositeOperation, offset, (uint)vertices.Count, uniformOffset, this);
            _callQueue.Add(call);
        }

        public unsafe void Dispose()
        {
            foreach (Frame frame in _frames)
            {
                frame.Dispose();
            }

            Shader.Dispose();

            Pipelines.Pipeline.DestroyAll();

            TextureManager.Dispose();
            Vk.DestroyCommandPool(Params.Device, ImageTransitionPool, (AllocationCallbacks*)Params.AllocationCallbacks);

            _callQueue.Clear();
            _vertexCollection.Clear();
        }

    }

    public static class VulkanRenderExtensionMethodContainer
    {

        internal static VulkanRenderer VulkanRenderer { private get; set; }

        /// <summary>
        /// End drawing flushing remaining render state.
        /// Also sets the <see cref="VulkanRenderer.CurrentCommandBuffer"/> property to the specified command buffer.
        /// </summary>
        public static void EndFrame(this Nvg nvg, CommandBuffer currentCommandBuffer)
        {
            VulkanRenderer.CurrentCommandBuffer = currentCommandBuffer;
            nvg.EndFrame();
        }

        /// <summary>
        /// End drawing flushing remaining render state.
        /// Also sets the <see cref="VulkanRenderer.FrameIndex"/> property to the specified index.
        /// </summary>
        public static void EndFrame(this Nvg nvg, uint frameIndex)
        {
            VulkanRenderer.CurrentFrameIndex = frameIndex;
            nvg.EndFrame();
        }

        /// <summary>
        /// End drawing flushing remaining render state.
        /// Also sets the <see cref="VulkanRenderer.CurrentCommandBuffer"/> and <see cref="VulkanRenderer.FrameIndex"/> to the specified values.
        /// </summary>
        public static void EndFrame(this Nvg nvg, CommandBuffer currentCommandBuffer, uint frameIndex)
        {
            VulkanRenderer.CurrentCommandBuffer = currentCommandBuffer;
            VulkanRenderer.CurrentFrameIndex = frameIndex;
            nvg.EndFrame();
        }

        /// <summary>
        /// End drawing flushing remaining render state.
        /// Also sets the <see cref="VulkanRenderer.CurrentCommandBuffer"/> property to the specified command buffer and advances the <see cref="VulkanRenderer.FrameIndex"/> after rendering.
        /// </summary>
        public static void EndFrameAndAdvance(this Nvg nvg, CommandBuffer currentCommandBuffer)
        {
            VulkanRenderer.CurrentCommandBuffer = currentCommandBuffer;
            nvg.EndFrame();
            VulkanRenderer.AdvanceFrame();
        }

    }
}
