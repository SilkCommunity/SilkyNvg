using System.Collections.Generic;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.Vulkan.Pipelines;
using SilkyNvg.Rendering.Vulkan.Utils;
using Veldrid;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class ConvexFillCall : Call
    {

        public ConvexFillCall(int image, StrokePath[] paths, int uniformOffset, CompositeOperationState op, VeldridRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset, PipelineSettings.ConvexFill(op), default, PipelineSettings.ConvexFillEdgeAA(op), renderer) { }

        public override void Run(NvgFrame frame, List<DrawCall> drawCalls)
        {
            
            Pipeline fPipeline = frame.PipelineCache.GetPipeLine(renderPipeline, _renderer);

            DrawCall call = new DrawCall
            {
                Pipeline = fPipeline,
                Set = new ResourceSetData
                {
                    image = image
                },
                UniformOffset = (uint)uniformOffset
            };
            
            
            
            //cmd.SetPipeline(fPipeline);
            //cmd.SetFramebuffer(renderer.Device.SwapchainFramebuffer);
            //cmd.SetGraphicsResourceSet(0, descriptorSet);

            foreach (StrokePath path in paths)
            {
                call.Count = path.FillCount;
                call.Offset = path.FillOffset;
                drawCalls.Add(call);
            }

            if (_renderer.EdgeAntiAlias)
            {
                Pipeline aaPipeline = frame.PipelineCache.GetPipeLine(renderPipeline, _renderer); 
                //cmd.SetPipeline(aaPipeline);
                call.Pipeline = aaPipeline;
                foreach (StrokePath path in paths)
                {
                    //cmd.Draw(path.StrokeCount, 1, path.StrokeOffset, 0);
                    call.Count = path.StrokeCount;
                    call.Offset = path.StrokeOffset;
                }
            }
            
        }

    }
}