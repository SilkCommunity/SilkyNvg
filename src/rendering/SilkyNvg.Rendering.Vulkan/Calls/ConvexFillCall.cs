using Silk.NET.Vulkan;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.Vulkan.Pipelines;
using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class ConvexFillCall : Call
    {

        public ConvexFillCall(int image, Path[] paths, int uniformOffset, CompositeOperationState op, VulkanRenderer renderer)
            : base(image, paths, default, default, uniformOffset, op, renderer) { }

        public override unsafe void Run()
        {
            Device device = renderer.Params.device;
            CommandBuffer cmdBuffer = renderer.Params.cmdBuffer;
            Vk vk = renderer.Vk;

            PipelineKey pipelineKey = new(false, false, false, renderer.EdgeAntiAlias,
                renderer.TriangleListFill ? PrimitiveTopology.TriangleList : PrimitiveTopology.TriangleFan, compositeOperation);
            _ = renderer.BindPipeline(cmdBuffer, pipelineKey);

            DescriptorSetLayout layout = renderer.Shader.DescLayout;
            DescriptorSetAllocateInfo* allocInfo = stackalloc DescriptorSetAllocateInfo[1]
            {
                new DescriptorSetAllocateInfo(StructureType.DescriptorSetAllocateInfo, null, renderer.Shader.DescPool, 1, &layout)
            };

            renderer.Assert(vk.AllocateDescriptorSets(device, allocInfo, out DescriptorSet descSet));
            renderer.Shader.SetUniforms(descSet, uniformOffset, image);

            vk.CmdBindDescriptorSets(cmdBuffer, PipelineBindPoint.Graphics, renderer.PipelineLayout, 0, 1, &descSet, null);

            foreach (Path path in paths)
            {
                ulong offset = (ulong)(path.FillOffset * Marshal.SizeOf(typeof(Vertex)));
                Buffer buffer = renderer.VertexBuffer.Handle;
                vk.CmdBindVertexBuffers(cmdBuffer, 0, 1, buffer, offset);
                vk.CmdDraw(cmdBuffer, path.FillCount, 1, 0, 0);
            }

            if (renderer.EdgeAntiAlias)
            {
                pipelineKey = new(false, false, false, renderer.EdgeAntiAlias, PrimitiveTopology.TriangleStrip, compositeOperation);
                _ = renderer.BindPipeline(cmdBuffer, pipelineKey);

                foreach (Path path in paths)
                {
                    ulong offset = (ulong)(path.StrokeOffset * Marshal.SizeOf(typeof(Vertex)));
                    Buffer buffer = renderer.VertexBuffer.Handle;
                    vk.CmdBindVertexBuffers(cmdBuffer, 0, 1, buffer, offset);
                    vk.CmdDraw(cmdBuffer, path.StrokeCount, 1, 0, 0);
                }
            }
        }

    }
}