using Silk.NET.Vulkan;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.Vulkan.Pipelines;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class FillCall : Call
    {

        public FillCall(int image, Path[] paths, uint triangleOffset, ulong uniformOffset, CompositeOperationState compositeOperation, VulkanRenderer renderer)
            : base(image, paths, triangleOffset, 4, uniformOffset, PipelineSettings.Fill(compositeOperation), PipelineSettings.FillStencil(compositeOperation, renderer.TriangleListFill), PipelineSettings.FillEdgeAA(compositeOperation), renderer) { }

        public override void Run(Frame frame, CommandBuffer cmd)
        {
            Vk vk = renderer.Vk;

            Pipelines.Pipeline sPipeline = Pipelines.Pipeline.GetPipeline(stencilPipeline, renderer);
            sPipeline.Bind(cmd);

            DescriptorSet descriptorSet = frame.DescriptorSetManager.GetDescriptorSet();
            renderer.Shader.SetUniforms(frame, descriptorSet, uniformOffset, 0);
            vk.CmdBindDescriptorSets(cmd, PipelineBindPoint.Graphics, renderer.Shader.PipelineLayout, 0, 1, descriptorSet, 0, 0);
            foreach (Path path in paths)
            {
                vk.CmdDraw(cmd, path.FillCount, 1, path.FillOffset, 0);
            }

            descriptorSet = frame.DescriptorSetManager.GetDescriptorSet();
            renderer.Shader.SetUniforms(frame, descriptorSet, uniformOffset + renderer.Shader.FragSize, image);
            vk.CmdBindDescriptorSets(cmd, PipelineBindPoint.Graphics, renderer.Shader.PipelineLayout, 0, 1, descriptorSet, 0, 0);

            if (renderer.EdgeAntiAlias)
            {
                Pipelines.Pipeline aaPipeline = Pipelines.Pipeline.GetPipeline(antiAliasPipeline, renderer);
                aaPipeline.Bind(cmd);
                foreach (Path path in paths)
                {
                    vk.CmdDraw(cmd, path.StrokeCount, 1, path.StrokeOffset, 0);
                }
            }

            Pipelines.Pipeline fPipeline = Pipelines.Pipeline.GetPipeline(renderPipeline, renderer);
            fPipeline.Bind(cmd);

            vk.CmdDraw(cmd, triangleCount, 1, triangleOffset, 0);
        }

    }
}
