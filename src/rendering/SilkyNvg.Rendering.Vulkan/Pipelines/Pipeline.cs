using Silk.NET.Vulkan;

namespace SilkyNvg.Rendering.Vulkan.Pipelines
{
    internal class Pipeline
    {

        private readonly VulkanRenderer _renderer;

        public Silk.NET.Vulkan.Pipeline Handle { get; }

        public Pipeline(PipelineData pipelineData)
        {
            _renderer = VulkanRenderer.Instance;
            Handle = CreatePipeline(pipelineData);
        }

        private unsafe Silk.NET.Vulkan.Pipeline CreatePipeline(PipelineData pipelineData)
        {
            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            PipelineVertexInputStateCreateInfo vertexInputState = VertexInputState(VulkanRenderer.VertexInputBindingDescription, VulkanRenderer.VertexInputAttributeDescriptions);
            PipelineInputAssemblyStateCreateInfo inputAssemblyState = InputAssemblyState(pipelineData.Topology);
            PipelineViewportStateCreateInfo viewportState = ViewportState();
            PipelineRasterizationStateCreateInfo rasterizationState = RasterizationState();
            PipelineMultisampleStateCreateInfo multisampleState = MultisampleState();
            PipelineColorBlendAttachmentState colorBlendAttachmentState = ColourBlendAttachmentState(pipelineData.SrcRgb, pipelineData.SrcAlpha, pipelineData.DstRgb, pipelineData.DstAlpha);
            PipelineColorBlendStateCreateInfo colourBlendState = ColourBlendState(colorBlendAttachmentState);
            PipelineDynamicStateCreateInfo dynamicState = DynamicStateCreateInfo(DynamicState.Viewport, DynamicState.Scissor);

            PipelineShaderStageCreateInfo* shaderStages = stackalloc PipelineShaderStageCreateInfo[2]
            {
                _renderer.Shader.VertexShaderStage,
                _renderer.Shader.FragmentShaderStage
            };

            GraphicsPipelineCreateInfo graphicsPipelineCreateInfo = new()
            {
                SType = StructureType.GraphicsPipelineCreateInfo,

                StageCount = 2,
                PStages = shaderStages,

                PVertexInputState = &vertexInputState,
                PInputAssemblyState = &inputAssemblyState,
                PRasterizationState = &rasterizationState,
                PMultisampleState = &multisampleState,
                PColorBlendState = &colourBlendState,
                PDepthStencilState = null, // TODO
                PDynamicState = &dynamicState,
                PViewportState = &viewportState,
                PTessellationState = null,

                Layout = _renderer.Shader.Layout,

                RenderPass = _renderer.Params.RenderPass,
                Subpass = _renderer.Params.SubpassIndex,

                BasePipelineHandle = default,
                BasePipelineIndex = -1
            };

            _renderer.AssertVulkan(vk.CreateGraphicsPipelines(device, default, 1, graphicsPipelineCreateInfo, allocator, out Silk.NET.Vulkan.Pipeline pipeline));
            return pipeline;
        }

        private static unsafe PipelineVertexInputStateCreateInfo VertexInputState(VertexInputBindingDescription vertexInputBindingDescription, VertexInputAttributeDescription[] vertexInputAttributeDescriptions)
        {
            PipelineVertexInputStateCreateInfo inputState = new()
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,

                VertexBindingDescriptionCount = 1,
                PVertexBindingDescriptions = &vertexInputBindingDescription,
                VertexAttributeDescriptionCount = (uint)vertexInputAttributeDescriptions.Length
            };

            fixed (VertexInputAttributeDescription* ptr = &vertexInputAttributeDescriptions[0])
            {
                inputState.PVertexAttributeDescriptions = ptr;
            }

            return inputState;
        }

        private static PipelineInputAssemblyStateCreateInfo InputAssemblyState(PrimitiveTopology topology)
        {
            return new()
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,

                PrimitiveRestartEnable = false,
                Topology = topology
            };
        }

        private static unsafe PipelineViewportStateCreateInfo ViewportState()
        {
            return new()
            {
                SType = StructureType.PipelineViewportStateCreateInfo,

                ScissorCount = 1,
                ViewportCount = 1
            };
        }

        private static PipelineRasterizationStateCreateInfo RasterizationState()
        {
            return new()
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,

                DepthClampEnable = false,
                RasterizerDiscardEnable = false,
                DepthBiasEnable = false,

                PolygonMode = PolygonMode.Fill,
                LineWidth = 1.0f,

                CullMode = CullModeFlags.CullModeBackBit,
                FrontFace = FrontFace.CounterClockwise,

                DepthBiasConstantFactor = 0.0f,
                DepthBiasClamp = 0.0f,
                DepthBiasSlopeFactor = 0.0f
            };
        }

        private static PipelineMultisampleStateCreateInfo MultisampleState()
        {
            return new()
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,

                SampleShadingEnable = false,
                AlphaToCoverageEnable = false,
                AlphaToOneEnable = false,

                RasterizationSamples = SampleCountFlags.SampleCount1Bit,
                MinSampleShading = 0.0f,
                PSampleMask = null
            };
        }

        private static PipelineColorBlendAttachmentState ColourBlendAttachmentState(BlendFactor srcRgb, BlendFactor srcAlpha, BlendFactor dstRgb, BlendFactor dstAlpha)
        {
            return new()
            {
                ColorWriteMask = ColorComponentFlags.ColorComponentRBit | ColorComponentFlags.ColorComponentGBit |
                    ColorComponentFlags.ColorComponentBBit | ColorComponentFlags.ColorComponentABit,

                BlendEnable = false, // TODO: Change this to fit rendering
                SrcColorBlendFactor = BlendFactor.One,
                DstColorBlendFactor = BlendFactor.Zero,
                ColorBlendOp = BlendOp.Add,
                SrcAlphaBlendFactor = BlendFactor.One,
                DstAlphaBlendFactor = BlendFactor.Zero,
                AlphaBlendOp = BlendOp.Add
            };
        }

        public static unsafe PipelineColorBlendStateCreateInfo ColourBlendState(PipelineColorBlendAttachmentState colourBlendAttachmentState)
        {
            return new()
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,

                LogicOpEnable = false,

                LogicOp = LogicOp.Copy,
                AttachmentCount = 1,
                PAttachments = &colourBlendAttachmentState
            };
        }

        public static unsafe PipelineDynamicStateCreateInfo DynamicStateCreateInfo(params DynamicState[] dynamics)
        {
            PipelineDynamicStateCreateInfo dynamicStateCreateInfo = new()
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,

                DynamicStateCount = (uint)dynamics.Length
            };
            fixed (DynamicState* ptr = &dynamics[0])
            {
                dynamicStateCreateInfo.PDynamicStates = ptr;
            }
            return dynamicStateCreateInfo;
        }

    }
}
