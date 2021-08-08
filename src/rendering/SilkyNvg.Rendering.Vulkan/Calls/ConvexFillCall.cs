using Silk.NET.Vulkan;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.Vulkan.Pipelines;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class ConvexFillCall : Call
    {

        public ConvexFillCall(int image, Path[] paths, int uniformOffset, CompositeOperationState op, VulkanRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset, op, renderer) { }

        public override unsafe void Run()
        {
            Device device = renderer.Params.device;
            CommandBuffer cmdBuffer = renderer.Params.cmdBuffer;
            Vk vk = renderer.Vk;

            PipelineKey pipelineKey = new(false, false, false, renderer.EdgeAntiAlias, PrimitiveTopology.TriangleList, compositeOperation);
            _ = renderer.BindPipeline(cmdBuffer, pipelineKey);

            DescriptorSetLayout layout = renderer.Shader.DescLayout;
            DescriptorSetAllocateInfo* allocInfo = stackalloc DescriptorSetAllocateInfo[1]
            {
                new DescriptorSetAllocateInfo(StructureType.DescriptorSetAllocateInfo, null, renderer.Shader.DescPool, 1, &layout)
            };

            renderer.Assert(renderer.Vk.AllocateDescriptorSets(device, allocInfo, out DescriptorSet descSet));
            renderer.Shader.SetUniforms(descSet, uniformOffset, image);

            vk.CmdBindDescriptorSets(cmdBuffer, PipelineBindPoint.Graphics, renderer.PipelineLayout, 0, 1, &descSet, null);

            for (uint i = 0; i < paths.Length; i++)
            {
                ulong offset = (ulong)(paths[i].FillOffset * sizeof(Vertex));
                Buffer buffer = renderer.VertexBuffer.Handle;
                vk.CmdBindVertexBuffers(cmdBuffer, 0, 1, buffer, offset);
                vk.CmdDraw(cmdBuffer, paths[i].FillCount, 1, 0, 0);
            }
        }

    }
}