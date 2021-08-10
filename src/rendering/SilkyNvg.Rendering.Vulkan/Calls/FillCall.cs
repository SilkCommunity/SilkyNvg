using Silk.NET.Vulkan;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.Vulkan.Pipelines;
using System;
using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class FillCall : Call
    {

        public FillCall(int image, Path[] paths, int triangleOffset, int uniformOffset, CompositeOperationState compositeOperation, VulkanRenderer renderer)
            : base(image, paths, triangleOffset, 4, uniformOffset, compositeOperation, renderer) { }

        public override unsafe void Run()
        {
            ulong offset;

            Device device = renderer.Params.device;
            CommandBuffer cmdBuffer = renderer.Params.cmdBuffer;
            Vk vk = renderer.Vk;

            PipelineKey pipelineKey = new(true, false, false, renderer.EdgeAntiAlias,
                renderer.TriangleListFill ? PrimitiveTopology.TriangleList : PrimitiveTopology.TriangleFan, compositeOperation);
            _ = renderer.BindPipeline(cmdBuffer, pipelineKey);

            DescriptorSetLayout layout = renderer.Shader.DescLayout;
            DescriptorSetAllocateInfo* allocInfo = stackalloc DescriptorSetAllocateInfo[1]
            {
                new DescriptorSetAllocateInfo(StructureType.DescriptorSetAllocateInfo, null, renderer.Shader.DescPool, 1, &layout)
            };
            renderer.Assert(vk.AllocateDescriptorSets(device, allocInfo, out DescriptorSet descSet));
            renderer.Shader.SetUniforms(descSet, uniformOffset, image);
            vk.CmdBindDescriptorSets(cmdBuffer, PipelineBindPoint.Graphics, renderer.PipelineLayout, 0, 1, descSet, 0, null);

            foreach (Path path in paths)
            {
                offset = (ulong)(path.FillOffset * Marshal.SizeOf(typeof(Vertex)));
                vk.CmdBindVertexBuffers(cmdBuffer, 0, 1, renderer.VertexBuffer.Handle, offset);
                vk.CmdDraw(cmdBuffer, path.FillCount, 1, 0, 0);
            }

            renderer.Assert(vk.AllocateDescriptorSets(device, allocInfo, out DescriptorSet descSet2));
            renderer.Shader.SetUniforms(descSet2, uniformOffset + renderer.Shader.FragSize, image);
            vk.CmdBindDescriptorSets(cmdBuffer, PipelineBindPoint.Graphics, renderer.PipelineLayout, 0, 1, descSet2, 0, null);

            if (renderer.EdgeAntiAlias)
            {
                pipelineKey = new(false, true, true, renderer.EdgeAntiAlias, PrimitiveTopology.TriangleStrip, compositeOperation);
                renderer.BindPipeline(cmdBuffer, pipelineKey);

                foreach (Path path in paths)
                {
                    offset = (ulong)(path.StrokeOffset * Marshal.SizeOf(typeof(Vertex)));
                    vk.CmdBindVertexBuffers(cmdBuffer, 0, 1, renderer.VertexBuffer.Handle, offset);
                    vk.CmdDraw(cmdBuffer, path.StrokeCount, 1, 0, 0);
                }
            }

            pipelineKey = new(false, true, false, renderer.EdgeAntiAlias, PrimitiveTopology.TriangleStrip, compositeOperation);
            renderer.BindPipeline(cmdBuffer, pipelineKey);

            offset = (ulong)(triangleOffset * Marshal.SizeOf(typeof(Vertex)));
            vk.CmdBindVertexBuffers(cmdBuffer, 0, 1, renderer.VertexBuffer.Handle, offset);
            vk.CmdDraw(cmdBuffer, triangleCount, 1, 0, 0);
        }

    }
}
