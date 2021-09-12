using Silk.NET.Vulkan;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.Vulkan.Pipelines;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class StencilStrokeCall : Call
    {

        public StencilStrokeCall(int image, Path[] paths, int uniformOffset, CompositeOperationState compositeOperation, VulkanRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset, PipelineSettings.StencilStroke(compositeOperation), PipelineSettings.StencilStrokeStencil(compositeOperation), PipelineSettings.StencilStrokeEdgeAA(compositeOperation), renderer) { }

        public override void Run(Frame frame, CommandBuffer cmd)
        {
            Vk vk = renderer.Vk;

            Pipelines.Pipeline sPipeline = Pipelines.Pipeline.GetPipeline(stencilPipeline, renderer);
            sPipeline.Bind(cmd);

            DescriptorSet descriptorSet = frame.DescriptorSetManager.GetDescriptorSet();
            renderer.Shader.SetUniforms(frame, descriptorSet, uniformOffset /* + fragSize */, image);
            vk.CmdBindDescriptorSets(cmd, PipelineBindPoint.Graphics, renderer.Shader.PipelineLayout, 0, 1, descriptorSet, 0, 0);
            foreach (Path path in paths)
            {
                vk.CmdDraw(cmd, path.StrokeCount, 1, path.StrokeOffset, 0);
            }

            Pipelines.Pipeline aaPipeline = Pipelines.Pipeline.GetPipeline(antiAliasPipeline, renderer);
            aaPipeline.Bind(cmd);

            descriptorSet = frame.DescriptorSetManager.GetDescriptorSet();
            renderer.Shader.SetUniforms(frame, descriptorSet, uniformOffset, image);
            vk.CmdBindDescriptorSets(cmd, PipelineBindPoint.Graphics, renderer.Shader.PipelineLayout, 0, 1, descriptorSet, 0, 0);
            foreach (Path path in paths)
            {
                vk.CmdDraw(cmd, path.StrokeCount, 1, path.StrokeOffset, 0);
            }

            Pipelines.Pipeline stPipeline = Pipelines.Pipeline.GetPipeline(renderPipeline, renderer);
            stPipeline.Bind(cmd);
            foreach (Path path in paths)
            {
                vk.CmdDraw(cmd, path.StrokeCount, 1, path.StrokeOffset, 0);
            }
        }

    }
}
