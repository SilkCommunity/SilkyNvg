using Silk.NET.Vulkan;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.Vulkan.Pipelines;
using System;
using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class StencilStrokeCall : Call
    {

        public StencilStrokeCall(int image, Path[] paths, int uniformOffset, CompositeOperationState compositeOperation, VulkanRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset, compositeOperation, renderer) { }

        public override unsafe void Run()
        {
            Device device = renderer.Params.device;
            CommandBuffer cmdBuffer = renderer.Params.cmdBuffer;
            Vk vk = renderer.Vk;

            DescriptorSetLayout layout = renderer.Shader.DescLayout;
            DescriptorSetAllocateInfo* allocInfo = stackalloc DescriptorSetAllocateInfo[1]
            {
                new DescriptorSetAllocateInfo(StructureType.DescriptorSetAllocateInfo, null, renderer.Shader.DescPool, 1, &layout)
            };
            renderer.Assert(vk.AllocateDescriptorSets(device, allocInfo, out DescriptorSet descSet));
            renderer.Shader.SetUniforms(descSet, uniformOffset, image);
            vk.CmdBindDescriptorSets(cmdBuffer, PipelineBindPoint.Graphics, renderer.PipelineLayout, 0, 1, descSet, 0, null);

            PipelineKey pipelineKey = new(false, false, false, renderer.EdgeAntiAlias, PrimitiveTopology.TriangleStrip, compositeOperation);
            _ = renderer.BindPipeline(cmdBuffer, pipelineKey);

            foreach (Path path in paths)
            {
                ulong offset = (ulong)(path.StrokeOffset * Marshal.SizeOf(typeof(Vertex)));
                vk.CmdBindVertexBuffers(cmdBuffer, 0, 1, renderer.VertexBuffer.Handle, offset);
                vk.CmdDraw(cmdBuffer, path.StrokeCount, 1, 0, 0);
            }

            pipelineKey = new(false, true, true, renderer.EdgeAntiAlias, PrimitiveTopology.TriangleStrip, compositeOperation);
            _ = renderer.BindPipeline(cmdBuffer, pipelineKey);

            foreach (Path path in paths)
            {
                ulong offset = (ulong)(path.StrokeOffset * Marshal.SizeOf(typeof(Vertex)));
                vk.CmdBindVertexBuffers(cmdBuffer, 0, 1, renderer.VertexBuffer.Handle, offset);
                vk.CmdDraw(cmdBuffer, path.StrokeCount, 1, 0, 0);
            }

            pipelineKey = new(true, true, false, false, PrimitiveTopology.LineStrip, compositeOperation);

            foreach (Path path in paths)
            {
                ulong offset = (ulong)(path.StrokeOffset * Marshal.SizeOf(typeof(Vertex)));
                vk.CmdBindVertexBuffers(cmdBuffer, 0, 1, renderer.VertexBuffer.Handle, offset);
                vk.CmdDraw(cmdBuffer, path.StrokeCount, 1, 0, 0);
            }
        }

    }
}
