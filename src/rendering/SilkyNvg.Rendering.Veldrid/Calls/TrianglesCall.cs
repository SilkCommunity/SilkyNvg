using System.Collections.Generic;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.Vulkan.Pipelines;
using SilkyNvg.Rendering.Vulkan.Utils;
using Veldrid;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class TrianglesCall : Call
    {

        public TrianglesCall(int image, CompositeOperationState compositeOperation, uint triangleOffset, uint triangleCount, int uniformOffset, VeldridRenderer renderer)
            : base(image, null, triangleOffset, triangleCount, uniformOffset, PipelineSettings.Triangles(compositeOperation), default, default, renderer) { }

        public override void Run(NvgFrame frame, List<DrawCall> drawCalls)
        {
            
            Pipeline pipeline = frame.PipelineCache.GetPipeLine(renderPipeline, _renderer);

            DrawCall call = new DrawCall
            {
                Pipeline = pipeline,
                Set = new ResourceSetData
                {
                    image = image
                },
                Count = triangleCount,
                Offset = triangleOffset,
                UniformOffset = (uint)uniformOffset
            };
            
            drawCalls.Add(call);

        }

    }
}