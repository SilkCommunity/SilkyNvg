using Silk.NET.Vulkan;
using SilkyNvg.Rendering.Vulkan.Pipelines;
using System.Collections.Generic;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class ConvexFillCall : Call
    {

        private static readonly IDictionary<Blending.CompositeOperationState, Pipelines.Pipeline> _fillPipelines = new Dictionary<Blending.CompositeOperationState, Pipelines.Pipeline>();
        private static readonly IDictionary<Blending.CompositeOperationState, Pipelines.Pipeline> _antiAliasPipelines = new Dictionary<Blending.CompositeOperationState, Pipelines.Pipeline>();

        public static Pipelines.Pipeline GetRenderPipeline(Blending.CompositeOperationState compositeOperation)
        {
            if (!_fillPipelines.TryGetValue(compositeOperation, out _))
            {
                PipelineData data = new()
                {
                    Topology = PrimitiveTopology.TriangleFan,
                    CompositeOperation = compositeOperation
                };
                _fillPipelines.Add(compositeOperation, new Pipelines.Pipeline(data));
            }
            return _fillPipelines[compositeOperation];
        }

        public static Pipelines.Pipeline GetAntiAliasPipeline(Blending.CompositeOperationState compositeOperation)
        {
            if (!_antiAliasPipelines.TryGetValue(compositeOperation, out _))
            {
                PipelineData data = new()
                {
                    Topology = PrimitiveTopology.TriangleStrip,
                    CompositeOperation = compositeOperation
                };
                _antiAliasPipelines.Add(compositeOperation, new Pipelines.Pipeline(data));
            }
            return _antiAliasPipelines[compositeOperation];
        }

        public ConvexFillCall(int image, Path[] paths, int uniformOffset, Blending.CompositeOperationState compositeOperation)
            : base(image, paths, 0, 0, uniformOffset, GetRenderPipeline(compositeOperation), GetAntiAliasPipeline(compositeOperation)) { }

        public override void Run(CommandBuffer cmd)
        {
            Vk vk = renderer.Vk;

            vk.CmdBindPipeline(cmd, PipelineBindPoint.Graphics, renderPipeline.Handle);
            foreach (Path path in paths)
            {
                vk.CmdDraw(cmd, path.FillCount, 1, path.FillOffset, 0);
            }
            vk.CmdBindPipeline(cmd, PipelineBindPoint.Graphics, antiAliasPipeline.Handle);
            foreach (Path path in paths)
            {
                vk.CmdDraw(cmd, path.StrokeCount, 1, path.StrokeOffset, 0);
            }
        }

    }
}