using System.Collections.Generic;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.Vulkan.Pipelines;
using SilkyNvg.Rendering.Vulkan.Utils;
using Veldrid;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class StrokeCall : Call
    {

        public StrokeCall(int image, StrokePath[] paths, int uniformOffset, CompositeOperationState compositeOperation, VeldridRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset, PipelineSettings.Stroke(compositeOperation), default, default, renderer) { }

        public override void Run(NvgFrame frame, List<DrawCall> drawCalls)
        {

            Pipeline fillPipeline = frame.PipelineCache.GetPipeLine(renderPipeline, _renderer);

            DrawCall call = new DrawCall
            {
                Pipeline = fillPipeline,
                Set = new ResourceSetData
                {
                    image = image
                },
                UniformOffset = (uint)uniformOffset
            };
            

            foreach (StrokePath path in paths)
            {
                call.Offset = path.StrokeOffset;
                call.Count = path.StrokeCount;
                drawCalls.Add(call);
            }

        }

    }
}