using Silk.NET.Maths;
using Silk.NET.Vulkan;
using SilkyNvg.Blending;
using SilkyNvg.Images;
using SilkyNvg.Rendering.Vulkan.Calls;
using SilkyNvg.Rendering.Vulkan.Shaders;
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

        private Vector2D<float> _viewSize;
        private uint _requireredDescriptorSetCount;

        internal Vk Vk { get; }

        internal VulkanRendererParams Params { get; }

        internal Shader Shader { get; private set; }

        internal bool Debug => _flags.HasFlag(CreateFlags.Debug);

        internal bool StencilStrokes => _flags.HasFlag(CreateFlags.StencilStrokes);

        internal bool TriangleListFill => _flags.HasFlag(CreateFlags.TriangleListFill);

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

        public bool Create()
        {
            if (EdgeAntiAlias)
            {
                Shader = new Shader("SilkyNvg-Vulkan-Shader", "vertexShader", "fragmentShaderEdgeAA", this);
            }
            else
            {
                Shader = new Shader("SilkyNvg-Vulkan-Shader", "vertexShader", "fragmentShader", this);
            }
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

            _ = CreateTexture(Texture.Alpha, new Vector2D<uint>(1, 1), 0, null);

            return true;
        }

        public int CreateTexture(Texture type, Vector2D<uint> size, ImageFlags imageFlags, ReadOnlySpan<byte> data)
        {
            return 2;
        }

        public bool UpdateTexture(int image, Rectangle<uint> bounds, ReadOnlySpan<byte> data)
        {
            return true;
        }

        public bool GetTextureSize(int image, out Vector2D<uint> size)
        {
            size = default;
            return true;
        }

        public bool DeleteTexture(int image)
        {
            return true;
        }

        public void Viewport(Vector2D<float> size, float _)
        {
            _viewSize = size;
        }

        public void Cancel()
        {

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

                Vk.CmdBindVertexBuffers(CurrentCommandBuffer, 0, 1, frame.VertexBuffer.Handle, 0);

                _callQueue.Run(frame, CurrentCommandBuffer);
            }

            _vertexCollection.Clear();
            _callQueue.Clear();
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
                renderPaths[i] = new Path(
                    _vertexCollection.CurrentsOffset, path.Fill.Count,
                    _vertexCollection.CurrentsOffset + path.Fill.Count, path.Stroke.Count
                );
                _vertexCollection.AddVertices(path.Fill);
                _vertexCollection.AddVertices(path.Stroke);
                offset += path.Fill.Count;
                offset += path.Stroke.Count;
            }

            Call call;
            if ((paths.Count == 1) && paths[0].Convex) // Convex
            {
                _requireredDescriptorSetCount++;
                call = new ConvexFillCall(paint.Image, renderPaths, 0, compositeOperation, this);
            }
            else
            {
                _vertexCollection.AddVertex(new Vertex(bounds.Max, 0.5f, 1.0f));
                _vertexCollection.AddVertex(new Vertex(bounds.Max.X, bounds.Min.Y, 0.5f, 1.0f));
                _vertexCollection.AddVertex(new Vertex(bounds.Min.X, bounds.Max.Y, 0.5f, 1.0f));
                _vertexCollection.AddVertex(new Vertex(bounds.Min, 0.5f, 1.0f));

                _requireredDescriptorSetCount += 2;
                call = new FillCall(paint.Image, renderPaths, (uint)offset, 0, compositeOperation, this);
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

            Call call;
            if (StencilStrokes)
            {
                _requireredDescriptorSetCount += 2;
                call = new StencilStrokeCall(paint.Image, renderPaths, 0, compositeOperation, this);
            }
            else
            {
                _requireredDescriptorSetCount += 1;
                call = new StrokeCall(paint.Image, renderPaths, 0, compositeOperation, this);
            }
            _callQueue.Add(call);
        }

        public void Triangles(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, ICollection<Vertex> vertices, float fringeWidth)
        {

        }

        public unsafe void Dispose()
        {

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
