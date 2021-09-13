using Silk.NET.Vulkan;
using SilkyNvg.Rendering.Vulkan.Pipelines;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class TrianglesCall : Call
    {

        public TrianglesCall(int image, Blending.CompositeOperationState compositeOperation, uint triangleOffset, uint triangleCount, ulong uniformOffset, VulkanRenderer renderer)
            : base(image, null, triangleOffset, triangleCount, uniformOffset, PipelineSettings.Triangles(compositeOperation), default, default, renderer) { }

        public override void Run(Frame frame, CommandBuffer cmd)
        {
            Vk vk = renderer.Vk;

            Pipelines.Pipeline pipeline = Pipelines.Pipeline.GetPipeline(renderPipeline, renderer);
            pipeline.Bind(cmd);

            DescriptorSet descriptorSet = frame.DescriptorSetManager.GetDescriptorSet();
            renderer.Shader.SetUniforms(frame, descriptorSet, uniformOffset, image);
            vk.CmdBindDescriptorSets(cmd, PipelineBindPoint.Graphics, renderer.Shader.PipelineLayout, 0, 1, descriptorSet, 0, 0);

            vk.CmdDraw(cmd, triangleCount, 1, triangleOffset, 0);
        }

    }
}
