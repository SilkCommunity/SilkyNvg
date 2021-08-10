using Silk.NET.Vulkan;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.Vulkan.Pipelines;
using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class TrianglesCall : Call
    {

        public TrianglesCall(int image, CompositeOperationState compositeOperation, int triangleOffset, uint triangleCount, int uniformOffset, VulkanRenderer renderer)
            : base(image, null, triangleOffset, triangleCount, uniformOffset, compositeOperation, renderer) { }

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
            renderer.Assert(vk.AllocateDescriptorSets(device, allocInfo, out DescriptorSet descSet));
            renderer.Shader.SetUniforms(descSet, uniformOffset, image);
            vk.CmdBindDescriptorSets(cmdBuffer, PipelineBindPoint.Graphics, renderer.PipelineLayout, 0, 1, descSet, 0, null);

            ulong offset = (ulong)(triangleOffset * Marshal.SizeOf(typeof(Vertex)));
            vk.CmdBindVertexBuffers(cmdBuffer, 0, 1, renderer.VertexBuffer.Handle, offset);

            vk.CmdDraw(cmdBuffer, triangleCount, 1, 0, 0);
        }

    }
}
