using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering.Vulkan.Pipelines
{
    internal class Pipeline : IDisposable
    {

        private static readonly IDictionary<PipelineSettings, Pipeline> _pipelines = new Dictionary<PipelineSettings, Pipeline>();

        public static Pipeline GetPipeline(PipelineSettings settings, VulkanRenderer renderer)
        {
            if (!_pipelines.TryGetValue(settings, out _))
            {
                _pipelines.Add(settings, new Pipelines.Pipeline(settings, renderer));
            }
            return _pipelines[settings];
        }


        private static readonly VertexInputBindingDescription _bindingDescription = new()
        {
            Binding = 0,
            InputRate = VertexInputRate.Vertex,
            Stride = (uint)Marshal.SizeOf<Vertex>()
        };

        private static readonly VertexInputAttributeDescription[] _attributeDescriptions =
        {
            new VertexInputAttributeDescription()
            {
                Binding = 0,
                Location = 0,
                Format = Format.R32G32Sfloat,
                Offset = (uint)Marshal.OffsetOf<Vertex>("_x")
            },
            new VertexInputAttributeDescription()
            {
                Binding = 0,
                Location = 1,
                Format = Format.R32G32Sfloat,
                Offset = (uint)Marshal.OffsetOf<Vertex>("_u")
            }
        };

        private readonly Silk.NET.Vulkan.Pipeline _handle;

        private readonly VulkanRenderer _renderer;

        private unsafe Pipeline(PipelineSettings settings, VulkanRenderer renderer)
        {
            _renderer = renderer;

            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            PipelineShaderStageCreateInfo* shaderStages = stackalloc PipelineShaderStageCreateInfo[2]
            {
                _renderer.Shader.VertexShaderStage,
                _renderer.Shader.FragmentShaderStage
            };
            PipelineVertexInputStateCreateInfo vertexInputState = VertexInputState(_bindingDescription, _attributeDescriptions);
            PipelineInputAssemblyStateCreateInfo inputAssemblyState = InputAssemblyState(settings);
            PipelineRasterizationStateCreateInfo rasterizationState = RasterizationState(settings);
            PipelineMultisampleStateCreateInfo multisampleState = MultisampleState();
            PipelineColorBlendAttachmentState colourBlendAttachmentState = ColorBlendAttachmentState(settings);
            PipelineColorBlendStateCreateInfo colourBlendState = ColourBlendState(colourBlendAttachmentState);
            PipelineViewportStateCreateInfo viewportState = ViewportState();
            PipelineDynamicStateCreateInfo dynamicState = DynamicStates(DynamicState.Viewport, DynamicState.Scissor);
            PipelineDepthStencilStateCreateInfo depthStencilState = DepthStencilState(settings);

            GraphicsPipelineCreateInfo pipelineCreateInfo = new()
            {
                SType = StructureType.GraphicsPipelineCreateInfo,

                StageCount = 2,
                PStages = shaderStages,

                PVertexInputState = &vertexInputState,
                PInputAssemblyState = &inputAssemblyState,
                PRasterizationState = &rasterizationState,
                PMultisampleState = &multisampleState,
                PColorBlendState = &colourBlendState,
                PViewportState = &viewportState,
                PDynamicState = &dynamicState,
                PDepthStencilState = &depthStencilState,
                PTessellationState = null,

                Layout = _renderer.Shader.PipelineLayout,

                RenderPass = _renderer.Params.RenderPass,
                Subpass = _renderer.Params.SubpassIndex,

                BasePipelineIndex = -1,
                BasePipelineHandle = default
            };

            _renderer.AssertVulkan(vk.CreateGraphicsPipelines(device, default, 1, pipelineCreateInfo, allocator, out _handle));
        }

        public void Bind(CommandBuffer cmd)
        {
            Vk vk = _renderer.Vk;
            vk.CmdBindPipeline(cmd, PipelineBindPoint.Graphics, _handle);
        }

        public unsafe void Dispose()
        {
            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            vk.DestroyPipeline(device, _handle, allocator);
        }

        private static unsafe PipelineVertexInputStateCreateInfo VertexInputState(VertexInputBindingDescription bindingDescription, Span<VertexInputAttributeDescription> attributeDescriptions)
        {
            PipelineVertexInputStateCreateInfo vertexInputStateCreateInfo = new()
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,

                VertexBindingDescriptionCount = 1,
                PVertexBindingDescriptions = &bindingDescription,

                VertexAttributeDescriptionCount = (uint)attributeDescriptions.Length
            };
            fixed (VertexInputAttributeDescription* ptr = &attributeDescriptions[0])
            {
                vertexInputStateCreateInfo.PVertexAttributeDescriptions = ptr;
            }
            return vertexInputStateCreateInfo;
        }

        private static PipelineInputAssemblyStateCreateInfo InputAssemblyState(PipelineSettings settings)
        {
            return new()
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,

                PrimitiveRestartEnable = false,
                Topology = settings.Topology
            };
        }

        private static PipelineRasterizationStateCreateInfo RasterizationState(PipelineSettings settings)
        {
            return new()
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,

                RasterizerDiscardEnable = false,

                PolygonMode = PolygonMode.Fill,
                LineWidth = 1.0f,

                CullMode = settings.CullMode,
                FrontFace = settings.FrontFace,

                DepthClampEnable = false,
                DepthBiasEnable = false,
                DepthBiasClamp = 0.0f,
                DepthBiasConstantFactor = 0.0f,
                DepthBiasSlopeFactor = 0.0f
            };
        }

        private static PipelineMultisampleStateCreateInfo MultisampleState()
        {
            return new()
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,

                SampleShadingEnable = false,

                RasterizationSamples = SampleCountFlags.SampleCount1Bit,
                MinSampleShading = 1.0f,
                PSampleMask = null,
                AlphaToCoverageEnable = false,
                AlphaToOneEnable = false
            };
        }

        private static PipelineColorBlendAttachmentState ColorBlendAttachmentState(PipelineSettings settings)
        {
            return new()
            {
                ColorWriteMask = settings.ColourMask,

                BlendEnable = true,
                SrcColorBlendFactor = settings.SrcRgb,
                SrcAlphaBlendFactor = settings.SrcAlpha,
                ColorBlendOp = BlendOp.Add,
                DstColorBlendFactor = settings.DstRgb,
                DstAlphaBlendFactor = settings.DstAlpha,
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

        public static PipelineViewportStateCreateInfo ViewportState()
        {
            return new()
            {
                SType = StructureType.PipelineViewportStateCreateInfo,

                PScissors = null,
                PViewports = null,

                ScissorCount = 1,
                ViewportCount = 1
            };
        }

        public static unsafe PipelineDynamicStateCreateInfo DynamicStates(params DynamicState[] states)
        {
            PipelineDynamicStateCreateInfo pipelineDynamicStateCreateInfo = new()
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,

                DynamicStateCount = (uint)states.Length
            };
            fixed (DynamicState* ptr = &states[0])
            {
                pipelineDynamicStateCreateInfo.PDynamicStates = ptr;
            }
            return pipelineDynamicStateCreateInfo;
        }

        public static unsafe PipelineDepthStencilStateCreateInfo DepthStencilState(PipelineSettings settings)
        {
            return new()
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,

                DepthBoundsTestEnable = false,
                DepthWriteEnable = false,

                DepthTestEnable = settings.DepthTestEnabled,
                StencilTestEnable = settings.StencilTestEnable,

                DepthCompareOp = CompareOp.LessOrEqual,

                Front = new()
                {
                    WriteMask = settings.StencilWriteMask,
                    FailOp = settings.FrontStencilFailOp,
                    DepthFailOp = settings.FrontStencilDepthFailOp,
                    PassOp = settings.FrontStencilPassOp,
                    CompareOp = settings.StencilFunc,
                    Reference = settings.StencilRef,
                    CompareMask = settings.StencilMask
                },
                Back = new()
                {
                    WriteMask = settings.StencilWriteMask,
                    FailOp = settings.BackStencilFailOp,
                    DepthFailOp = settings.BackStencilDepthFailOp,
                    PassOp = settings.BackStencilPassOp,
                    CompareOp = settings.StencilFunc,
                    Reference = settings.StencilRef,
                    CompareMask = settings.StencilMask
                },
            };
        }

    }
}
