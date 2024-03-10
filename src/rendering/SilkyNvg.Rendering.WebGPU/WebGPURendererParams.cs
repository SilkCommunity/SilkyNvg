using Silk.NET.WebGPU;
using Device = Silk.NET.WebGPU.Device;

namespace SilkyNvg.Rendering.WebGPU
{
    public unsafe struct WebGPURendererParams
    {
        public Adapter* Adapter;
        public Device* Device;
        public Surface* Surface;
    }
}