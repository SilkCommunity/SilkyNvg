using System;
using System.Collections.Generic;

namespace SilkyNvg.Rendering.Vulkan.Pipelines
{
    internal class PipelineManager : IDisposable
    {

        private readonly IDictionary<PipelineKey, Pipeline> _pipelines = new Dictionary<PipelineKey, Pipeline>();

        private readonly VulkanRenderer _renderer;

        public PipelineManager(VulkanRenderer renderer)
        {
            _renderer = renderer;
        }

        public void AddPipeline(PipelineKey pipelineKey)
        {
            _pipelines.Add(pipelineKey, new Pipeline(pipelineKey, _renderer));
        }

        public Pipeline FindPipeline(PipelineKey pipelineKey)
        {
            bool hasValue = _pipelines.TryGetValue(pipelineKey, out Pipeline value);
            if (!hasValue)
            {
                return null;
            }

            return value;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}
