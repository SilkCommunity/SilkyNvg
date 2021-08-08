using Silk.NET.Vulkan;
using SilkyNvg.Blending;

namespace SilkyNvg.Rendering.Vulkan.Pipelines
{
    internal struct PipelineKey
    {

        public bool StencilFill { get; }

        public bool StencilTest { get; }

        public bool EdgeAA { get; }

        public bool EdgeAAShader { get; }

        public PrimitiveTopology Topology { get; }

        public CompositeOperationState CompositeOperation { get; }

        public PipelineKey(bool stencilFill, bool stencilTest, bool edgeAA, bool edgeAAShader, PrimitiveTopology topology, CompositeOperationState compositeOperation)
        {
            StencilFill = stencilFill;
            StencilTest = stencilTest;
            EdgeAA = edgeAA;
            EdgeAAShader = edgeAAShader;
            Topology = topology;
            CompositeOperation = compositeOperation;
        }

    }
}
