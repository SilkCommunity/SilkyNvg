using Silk.NET.Vulkan;

namespace SilkyNvg.Rendering.Vulkan.Pipelines
{
    internal struct PipelineData
    {

        private static BlendFactor ToVulkan(Blending.BlendFactor factor)
        {
            return factor switch
            {
                Blending.BlendFactor.Zero => BlendFactor.Zero,
                Blending.BlendFactor.One => BlendFactor.One,
                Blending.BlendFactor.SrcColour => BlendFactor.SrcColor,
                Blending.BlendFactor.OneMinusSrcColour => BlendFactor.OneMinusSrcColor,
                Blending.BlendFactor.DstColour => BlendFactor.DstColor,
                Blending.BlendFactor.OneMinusDstColour => BlendFactor.OneMinusDstColor,
                Blending.BlendFactor.SrcAlpha => BlendFactor.SrcAlpha,
                Blending.BlendFactor.OneMinusSrcAlpha => BlendFactor.OneMinusSrcAlpha,
                Blending.BlendFactor.DstAlpha => BlendFactor.DstAlpha,
                Blending.BlendFactor.OneMinusDstAlpha => BlendFactor.OneMinusDstAlpha,
                Blending.BlendFactor.SrcAlphaSaturate => BlendFactor.SrcAlphaSaturate,
                _ => BlendFactor.ConstantColor
            };
        }

        public PrimitiveTopology Topology;
        public Blending.CompositeOperationState CompositeOperation;

        public BlendFactor SrcRgb => ToVulkan(CompositeOperation.SrcRgb);

        public BlendFactor SrcAlpha => ToVulkan(CompositeOperation.SrcAlpha);

        public BlendFactor DstRgb => ToVulkan(CompositeOperation.DstRgb);

        public BlendFactor DstAlpha => ToVulkan(CompositeOperation.DstAlpha);

    }
}
