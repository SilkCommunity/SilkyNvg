using SilkyNvg.Rendering.Vulkan.Pipelines;
using Veldrid;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    public struct DrawCall
    {
        public ResourceSetData Set;
        public Pipeline Pipeline;
        public uint Offset;
        public uint Count;
        public uint UniformOffset;
    }
}