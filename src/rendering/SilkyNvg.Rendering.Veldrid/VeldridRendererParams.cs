using Veldrid;

namespace SilkyNvg.Rendering.Vulkan
{
    public struct VeldridRendererParams
    {
        public GraphicsDevice Device;
        public CommandList InitialCommandBuffer;
        public bool AdvanceFrameIndexAutomatically;

    }
}