using Silk.NET.Vulkan;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.Vulkan.Pipelines;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class StrokeCall : Call
    {

        public StrokeCall(int image, Path[] paths, ulong uniformOffset, CompositeOperationState compositeOperation, VulkanRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset, PipelineSettings.Stroke(compositeOperation), default, default, renderer) { }

        public override void Run(Frame frame, CommandBuffer cmd)
        {
            Vk vk = renderer.Vk;

            Pipelines.Pipeline fillPipeline = Pipelines.Pipeline.GetPipeline(renderPipeline, renderer);
            fillPipeline.Bind(cmd);

            DescriptorSet descriptorSet = frame.DescriptorSetManager.GetDescriptorSet();
            renderer.Shader.SetUniforms(frame, descriptorSet, uniformOffset, image);
            vk.CmdBindDescriptorSets(cmd, PipelineBindPoint.Graphics, renderer.Shader.PipelineLayout, 0, 1, descriptorSet, 0, 0);

            foreach (Path path in paths)
            {
                vk.CmdDraw(cmd, path.StrokeCount, 1, path.StrokeOffset, 0);
            }
        }

    }
}
