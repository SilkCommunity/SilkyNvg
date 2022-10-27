using System.Collections.Generic;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.Vulkan.Pipelines;
using SilkyNvg.Rendering.Vulkan.Utils;
using Veldrid;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class StencilStrokeCall : Call
    {

        public StencilStrokeCall(int image, StrokePath[] paths, int uniformOffset, CompositeOperationState compositeOperation, VeldridRenderer renderer)
            : base(image, paths, 0, 0, uniformOffset,  PipelineSettings.StencilStroke(compositeOperation), PipelineSettings.StencilStrokeStencil(compositeOperation), PipelineSettings.StencilStrokeEdgeAA(compositeOperation), renderer) { }

        public override void Run(NvgFrame frame, List<DrawCall> drawCalls)
        {


            Pipeline sPipeline = frame.PipelineCache.GetPipeLine(stencilPipeline, _renderer);
            //cmd.SetPipeline(sPipeline);
            //cmd.SetFramebuffer(renderer.Device.SwapchainFramebuffer);

            DrawCall call = new DrawCall
            {
                Pipeline = sPipeline,
                Set = new ResourceSetData
                {
                    image = image
                },
                UniformOffset = (uint)uniformOffset
            };
            
            
            
            //cmd.SetGraphicsResourceSet(0,descriptorSet);
            foreach (StrokePath path in paths)
            {
                //cmd.Draw(path.StrokeCount, 1, path.StrokeOffset, 0);
                call.Offset = path.StrokeOffset;
                call.Count = path.StrokeCount;
                drawCalls.Add(call);
            }

            Pipeline aaPipeline = frame.PipelineCache.GetPipeLine(antiAliasPipeline, _renderer);
            //cmd.SetPipeline(aaPipeline);
            call.Pipeline = aaPipeline;


            call.Set = new ResourceSetData
            {
                image = image
            };
            call.UniformOffset = (uint) uniformOffset;
            
            //cmd.SetGraphicsResourceSet(0, descriptorSet);
            foreach (StrokePath path in paths)
            {
                //cmd.Draw(path.StrokeCount, 1, path.StrokeOffset, 0);
                
                call.Offset = path.StrokeOffset;
                call.Count = path.StrokeCount;
                drawCalls.Add(call);
            }

            Pipeline stPipeline = frame.PipelineCache.GetPipeLine(renderPipeline, _renderer);
            //cmd.SetPipeline(stPipeline);
            call.Pipeline = stPipeline;
            
            foreach (StrokePath path in paths)
            {
                //cmd.Draw(path.StrokeCount, 1, path.StrokeOffset, 0);
                call.Offset = path.StrokeOffset;
                call.Count = path.StrokeCount;
                drawCalls.Add(call);
            }
            
        }

    }
}