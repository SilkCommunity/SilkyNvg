using System.Collections.Generic;
using SilkyNvg.Blending;
using SilkyNvg.Rendering.Vulkan.Pipelines;
using SilkyNvg.Rendering.Vulkan.Utils;
using Veldrid;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class FillCall : Call
    {

        public FillCall(int image, StrokePath[] paths, uint triangleOffset, int uniformOffset, CompositeOperationState compositeOperation, VeldridRenderer renderer)
            : base(image, paths, triangleOffset, 4, uniformOffset, PipelineSettings.Fill(compositeOperation), PipelineSettings.FillStencil(compositeOperation), PipelineSettings.FillEdgeAA(compositeOperation), renderer) { }

        public override void Run(NvgFrame frame, List<DrawCall> drawCalls) 
        {

            Pipeline sPipeline = frame.PipelineCache.GetPipeLine(stencilPipeline, _renderer);

            DrawCall call = new DrawCall
            {
                Pipeline = sPipeline,
                Set = new ResourceSetData
                {
                    image = image
                },
                UniformOffset = (uint)uniformOffset
            };
            
            
            foreach (StrokePath path in paths)
            {
                call.Offset = path.FillOffset;
                call.Count = path.FillCount;
                drawCalls.Add(call);
            }

            if (_renderer.EdgeAntiAlias)
            {
                Pipeline aaPipeline = frame.PipelineCache.GetPipeLine(antiAliasPipeline, _renderer);
                call.Pipeline = aaPipeline;
                foreach (StrokePath path in paths)
                {
                    call.Offset = path.StrokeOffset;
                    call.Count = path.StrokeCount;
                    drawCalls.Add(call);
                }
            }

            Pipeline fPipeline = frame.PipelineCache.GetPipeLine(renderPipeline, _renderer);
            call.Count = triangleCount;
            call.Offset = triangleOffset;
            call.Pipeline = fPipeline;
            call.Set = new ResourceSetData
            {
                image = image
            };
            call.UniformOffset = (uint) uniformOffset;

            drawCalls.Add(call);

        }

    }
}