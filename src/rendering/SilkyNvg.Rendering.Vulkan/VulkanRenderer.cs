using Silk.NET.Maths;
using Silk.NET.Vulkan;
using SilkyNvg.Blending;
using SilkyNvg.Images;
using SilkyNvg.Rendering.Vulkan.Calls;
using SilkyNvg.Rendering.Vulkan.Shaders;
using SilkyNvg.Rendering.Vulkan.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering.Vulkan
{
    public sealed class VulkanRenderer : INvgRenderer
    {

        public static AttachmentDescription ColourAttachmentDescription(Format format)
        {
            return new AttachmentDescription()
            {
                Format = format,
                Samples = SampleCountFlags.SampleCount1Bit,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.PresentSrcKhr // FIXME: Transition to depth stencil attachment optimal!
            };
        }

        internal static VertexInputBindingDescription VertexInputBindingDescription => new()
        {
            Binding = 0,
            Stride = (uint)Marshal.SizeOf(typeof(Vertex)),
            InputRate = VertexInputRate.Vertex
        };

        internal static VertexInputAttributeDescription[] VertexInputAttributeDescriptions => new VertexInputAttributeDescription[2]
        {
            new VertexInputAttributeDescription()
            {
                Binding = 0,
                Location = 0,
                Offset = (uint)Marshal.OffsetOf<Vertex>("_x"),
                Format = Format.R32G32Sfloat
            },
            new VertexInputAttributeDescription()
            {
                Binding = 0,
                Location = 1,
                Offset = (uint)Marshal.OffsetOf<Vertex>("_u"),
                Format = Format.R32G32Sfloat
            }
        };

        internal static VulkanRenderer Instance { get; private set; }

        private readonly VertexCollection _vertexCollection;
        private readonly CallQueue _callQueue;

        private Vector2D<float> _viewSize;

        internal Vk Vk { get; }

        internal VulkanRendererParams Params { get; }

        internal Shader Shader { get; private set; }

        internal bool Debug { get; private set; }

        internal bool StencilStrokes { get; private set; }

        internal bool UseTriangleListFill { get; private set; }

        public bool EdgeAntiAlias { get; }

        public VulkanRenderer(CreateFlags flags, VulkanRendererParams @params, Vk vk)
        {
            Vk = vk;

            Debug = flags.HasFlag(CreateFlags.Debug);
            StencilStrokes = flags.HasFlag(CreateFlags.StencilStrokes);
            UseTriangleListFill = flags.HasFlag(CreateFlags.TriangleListFill);
            EdgeAntiAlias = flags.HasFlag(CreateFlags.Antialias);

            Params = @params;

            _vertexCollection = new VertexCollection();
            _callQueue = new CallQueue();

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                throw new InvalidOperationException("Vulkan renderer already initialized!");
            }
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
                
            }

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

        public void Flush()
        {

        }

        public void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, Box2D<float> bounds, IReadOnlyList<Rendering.Path> paths)
        {

        }

        public void Stroke(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringe, float strokeWidth, IReadOnlyList<Rendering.Path> paths)
        {

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

        /// <summary>
        /// End drawing flushing remaining render state.
        /// Also sets the <see cref="VulkanRenderer.CurrentCommandBuffer"/> property to the specified command buffer.
        /// </summary>
        public static void EndFrame(this Nvg nvg, CommandBuffer currentCommandBuffer)
        {
            //VulkanRenderer.Instance.CurrentCommandBuffer = currentCommandBuffer;
            nvg.EndFrame();
        }

        /// <summary>
        /// End drawing flushing remaining render state.
        /// Also sets the <see cref="VulkanRenderer.FrameIndex"/> property to the specified index.
        /// </summary>
        public static void EndFrame(this Nvg nvg, uint frameIndex)
        {
            //VulkanRenderer.Instance.FrameIndex = frameIndex;
            nvg.EndFrame();
        }

        /// <summary>
        /// End drawing flushing remaining render state.
        /// Also sets the <see cref="VulkanRenderer.CurrentCommandBuffer"/> and <see cref="VulkanRenderer.FrameIndex"/> to the specified values.
        /// </summary>
        public static void EndFrame(this Nvg nvg, CommandBuffer currentCommandBuffer, uint frameIndex)
        {
            //VulkanRenderer.Instance.CurrentCommandBuffer = currentCommandBuffer;
            //VulkanRenderer.Instance.FrameIndex = frameIndex;
            nvg.EndFrame();
        }

        /// <summary>
        /// End drawing flushing remaining render state.
        /// Also sets the <see cref="VulkanRenderer.CurrentCommandBuffer"/> property to the specified command buffer and advances the <see cref="VulkanRenderer.FrameIndex"/> after rendering.
        /// </summary>
        public static void EndFrameAndAdvance(this Nvg nvg, CommandBuffer currentCommandBuffer)
        {
            //VulkanRenderer.Instance.CurrentCommandBuffer = currentCommandBuffer;
            nvg.EndFrame();
            //VulkanRenderer.Instance.AdvanceFrame();
        }

    }
}
