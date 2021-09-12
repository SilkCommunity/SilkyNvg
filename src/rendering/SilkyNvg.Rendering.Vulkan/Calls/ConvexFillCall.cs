using Silk.NET.Vulkan;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.Vulkan.Pipelines;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class ConvexFillCall : Call
    {

        public ConvexFillCall(int image, Path[] paths, int uniformOffset, CompositeOperationState op, VulkanRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset, PipelineSettings.ConvexFill(op, renderer.TriangleListFill), default, PipelineSettings.ConvexFillEdgeAA(op), renderer) { }

        public unsafe override void Run(Frame frame, CommandBuffer cmd)
        {
            Vk vk = renderer.Vk;

            Pipelines.Pipeline fPipeline = Pipelines.Pipeline.GetPipeline(renderPipeline, renderer);
            fPipeline.Bind(cmd);

            DescriptorSet descriptorSet = frame.DescriptorSetManager.GetDescriptorSet();
            renderer.Shader.SetUniforms(frame, descriptorSet, uniformOffset, image);
            vk.CmdBindDescriptorSets(cmd, PipelineBindPoint.Graphics, renderer.Shader.PipelineLayout, 0, 1, descriptorSet, 0, 0);

            foreach (Path path in paths)
            {
                vk.CmdDraw(cmd, path.FillCount, 1, path.FillOffset, 0);
            }

            if (renderer.EdgeAntiAlias)
            {
                Pipelines.Pipeline aaPipeline = Pipelines.Pipeline.GetPipeline(renderPipeline, renderer);
                aaPipeline.Bind(cmd);

                foreach (Path path in paths)
                {
                    vk.CmdDraw(cmd, path.StrokeCount, 1, path.StrokeOffset, 0);
                }
            }
        }

    }
}