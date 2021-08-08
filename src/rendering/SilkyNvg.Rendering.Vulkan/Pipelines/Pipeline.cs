using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using SilkyNvg.Blending;
using System;

namespace SilkyNvg.Rendering.Vulkan.Pipelines
{
    internal class Pipeline : IDisposable
    {

        private readonly VulkanRenderer _renderer;

        public PipelineKey CreateKey { get; }

        public Silk.NET.Vulkan.Pipeline Handle { get; }

        public unsafe Pipeline(PipelineKey pipelineKey, VulkanRenderer renderer)
        {
            _renderer = renderer;

            Device device = _renderer.Params.device;
            PipelineLayout pipelineLayout = _renderer.PipelineLayout;
            RenderPass renderpass = _renderer.Params.renderpass;
            AllocationCallbacks* allocator = _renderer.Params.allocator;

            DescriptorSetLayout descLayout = _renderer.Shader.DescLayout;
            ShaderModule vertShader = _renderer.Shader.VertShader;
            ShaderModule fragShader = _renderer.Shader.FragShader;
            ShaderModule fragShaderAA = _renderer.Shader.FragShaderAA;

            VertexInputBindingDescription* viBindings = stackalloc VertexInputBindingDescription[1]
            {
                new VertexInputBindingDescription(0, (uint)sizeof(Vertex), VertexInputRate.Vertex)
            };

            VertexInputAttributeDescription* viAttrs = stackalloc VertexInputAttributeDescription[2]
            {
                new VertexInputAttributeDescription(0, 0, Format.R32G32Sfloat, 0),
                new VertexInputAttributeDescription(1, 0, Format.R32G32Sfloat, 2 * sizeof(float))
            };

            PipelineVertexInputStateCreateInfo vi = new(StructureType.PipelineVertexInputStateCreateInfo)
            {
                VertexBindingDescriptionCount = 1,
                PVertexBindingDescriptions = viBindings,
                VertexAttributeDescriptionCount = 2,
                PVertexAttributeDescriptions = viAttrs
            };

            PipelineInputAssemblyStateCreateInfo ia = new(StructureType.PipelineInputAssemblyStateCreateInfo)
            {
                Topology = pipelineKey.Topology
            };

            PipelineRasterizationStateCreateInfo rs = new(StructureType.PipelineRasterizationStateCreateInfo)
            {
                PolygonMode = PolygonMode.Fill,
                CullMode = CullModeFlags.CullModeBackBit,
                FrontFace = FrontFace.CounterClockwise,
                DepthClampEnable = false,
                RasterizerDiscardEnable = false,
                DepthBiasEnable = false,
                LineWidth = 1.0f
            };

            PipelineColorBlendAttachmentState colourBlend = CompositeOperationToColourBlendAttachmentState(pipelineKey.CompositeOperation);

            if (pipelineKey.StencilFill)
            {
                rs.CullMode = CullModeFlags.CullModeNone;
                colourBlend.ColorWriteMask = 0;
            }

            PipelineColorBlendStateCreateInfo cb = new(StructureType.PipelineColorBlendStateCreateInfo)
            {
                AttachmentCount = 1,
                PAttachments = &colourBlend
            };

            PipelineViewportStateCreateInfo vp = new(StructureType.PipelineViewportStateCreateInfo)
            {
                ViewportCount = 1,
                ScissorCount = 1
            };

            const int DYNAMIC_STATE_COUNT = 2;
            DynamicState* dynamicStateEnables = stackalloc DynamicState[DYNAMIC_STATE_COUNT]
            {
                DynamicState.Viewport,
                DynamicState.Scissor
            };

            PipelineDynamicStateCreateInfo dynamicState = new(StructureType.PipelineDynamicStateCreateInfo)
            {
                DynamicStateCount = 2,
                PDynamicStates = dynamicStateEnables
            };

            PipelineDepthStencilStateCreateInfo ds = InitializeDepthStencilCreateInfo(pipelineKey);

            PipelineMultisampleStateCreateInfo ms = new(StructureType.PipelineMultisampleStateCreateInfo)
            {
                PSampleMask = null,
                RasterizationSamples = SampleCountFlags.SampleCount1Bit
            };

            PipelineShaderStageCreateInfo* shaderStages = stackalloc PipelineShaderStageCreateInfo[2];
            shaderStages[0] = new PipelineShaderStageCreateInfo(StructureType.PipelineShaderStageCreateInfo)
            {
                Stage = ShaderStageFlags.ShaderStageVertexBit,
                Module = vertShader,
                PName = (byte*)SilkMarshal.StringToPtr("main")
            };
            shaderStages[1] = new PipelineShaderStageCreateInfo(StructureType.PipelineShaderStageCreateInfo)
            {
                Stage = ShaderStageFlags.ShaderStageFragmentBit,
                Module = fragShader,
                PName = (byte*)SilkMarshal.StringToPtr("main")
            };
            if (pipelineKey.EdgeAAShader)
            {
                shaderStages[1].Module = fragShaderAA;
            }

            GraphicsPipelineCreateInfo pipelineCreateInfo = new(StructureType.GraphicsPipelineCreateInfo)
            {
                Layout = pipelineLayout,
                StageCount = 2,
                PStages = shaderStages,
                PVertexInputState = &vi,
                PInputAssemblyState = &ia,
                PRasterizationState = &rs,
                PColorBlendState = &cb,
                PMultisampleState = &ms,
                PViewportState = &vp,
                PDepthStencilState = &ds,
                RenderPass = renderpass,
                PDynamicState = &dynamicState
            };

            renderer.Assert(renderer.Vk.CreateGraphicsPipelines(device, default, 1, pipelineCreateInfo, allocator, out Silk.NET.Vulkan.Pipeline pipeline));

            CreateKey = pipelineKey;
            Handle = pipeline;
        }

        public void Bind(CommandBuffer cmdBuffer)
        {
            _renderer.Vk.CmdBindPipeline(cmdBuffer, PipelineBindPoint.Graphics, Handle);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private static Silk.NET.Vulkan.BlendFactor BlendFactorToVkBlendFactor(Blending.BlendFactor factor)
        {
            return factor switch
            {
                Blending.BlendFactor.Zero => Silk.NET.Vulkan.BlendFactor.Zero,
                Blending.BlendFactor.One => Silk.NET.Vulkan.BlendFactor.One,
                Blending.BlendFactor.SrcColour => Silk.NET.Vulkan.BlendFactor.SrcColor,
                Blending.BlendFactor.OneMinusSrcColour => Silk.NET.Vulkan.BlendFactor.OneMinusSrcColor,
                Blending.BlendFactor.DstColour => Silk.NET.Vulkan.BlendFactor.DstColor,
                Blending.BlendFactor.OneMinusDstColour => Silk.NET.Vulkan.BlendFactor.OneMinusDstColor,
                Blending.BlendFactor.SrcAlpha => Silk.NET.Vulkan.BlendFactor.SrcAlpha,
                Blending.BlendFactor.OneMinusSrcAlpha => Silk.NET.Vulkan.BlendFactor.OneMinusSrcAlpha,
                Blending.BlendFactor.DstAlpha => Silk.NET.Vulkan.BlendFactor.DstAlpha,
                Blending.BlendFactor.OneMinusDstAlpha => Silk.NET.Vulkan.BlendFactor.OneMinusDstAlpha,
                Blending.BlendFactor.SrcAlphaSaturate => Silk.NET.Vulkan.BlendFactor.SrcAlphaSaturate,
                _ => Silk.NET.Vulkan.BlendFactor.OneMinusSrc1Alpha,
            };
        }

        private static PipelineColorBlendAttachmentState CompositeOperationToColourBlendAttachmentState(CompositeOperationState compositeOperation)
        {
            PipelineColorBlendAttachmentState state = new()
            {
                BlendEnable = true,
                ColorBlendOp = BlendOp.Add,
                AlphaBlendOp = BlendOp.Add,
                ColorWriteMask = ColorComponentFlags.ColorComponentRBit | ColorComponentFlags.ColorComponentGBit |
                ColorComponentFlags.ColorComponentBBit | ColorComponentFlags.ColorComponentABit,

                SrcColorBlendFactor = BlendFactorToVkBlendFactor(compositeOperation.SrcRgb),
                SrcAlphaBlendFactor = BlendFactorToVkBlendFactor(compositeOperation.SrcAlpha),
                DstColorBlendFactor = BlendFactorToVkBlendFactor(compositeOperation.DstRgb),
                DstAlphaBlendFactor = BlendFactorToVkBlendFactor(compositeOperation.DstAlpha)
            };

            if (state.SrcColorBlendFactor == Silk.NET.Vulkan.BlendFactor.OneMinusSrc1Alpha ||
                state.SrcAlphaBlendFactor == Silk.NET.Vulkan.BlendFactor.OneMinusSrc1Alpha ||
                state.DstColorBlendFactor == Silk.NET.Vulkan.BlendFactor.OneMinusSrc1Alpha ||
                state.DstAlphaBlendFactor == Silk.NET.Vulkan.BlendFactor.OneMinusSrc1Alpha)
            {
                state.SrcColorBlendFactor = Silk.NET.Vulkan.BlendFactor.One;
                state.SrcAlphaBlendFactor = Silk.NET.Vulkan.BlendFactor.OneMinusSrcAlpha;
                state.DstColorBlendFactor = Silk.NET.Vulkan.BlendFactor.One;
                state.DstAlphaBlendFactor = Silk.NET.Vulkan.BlendFactor.OneMinusSrcAlpha;
            }

            return state;
        }

        private static unsafe PipelineDepthStencilStateCreateInfo InitializeDepthStencilCreateInfo(PipelineKey pipelineKey)
        {
            PipelineDepthStencilStateCreateInfo ds = new(StructureType.PipelineDepthStencilStateCreateInfo)
            {
                DepthWriteEnable = false,
                DepthTestEnable = false,
                DepthCompareOp = CompareOp.LessOrEqual,
                DepthBoundsTestEnable = false,
                StencilTestEnable = false,
                Back = new StencilOpState()
                {
                    FailOp = StencilOp.Keep,
                    PassOp = StencilOp.Keep,
                    CompareOp = CompareOp.Always
                }
            };
            if (pipelineKey.StencilFill)
            {
                ds.StencilTestEnable = true;
                ds.Front = new StencilOpState()
                {
                    CompareOp = CompareOp.Always,
                    FailOp = StencilOp.Keep,
                    DepthFailOp = StencilOp.Keep,
                    PassOp = StencilOp.IncrementAndWrap,
                    Reference = 0x0,
                    CompareMask = 0xff,
                    WriteMask = 0xff
                };
                ds.Back = ds.Front;
                ds.Back.PassOp = StencilOp.DecrementAndWrap;
            }
            else if (pipelineKey.StencilTest)
            {
                ds.StencilTestEnable = true;
                if (pipelineKey.EdgeAA)
                {
                    ds.Front = new StencilOpState()
                    {
                        CompareOp = CompareOp.Equal,
                        Reference = 0x0,
                        CompareMask = 0xff,
                        WriteMask = 0xff,
                        FailOp = StencilOp.Keep,
                        DepthFailOp = StencilOp.Keep,
                        PassOp = StencilOp.Keep
                    };
                    ds.Back = ds.Front;
                }
                else
                {
                    ds.Front = new StencilOpState()
                    {
                        CompareOp = CompareOp.NotEqual,
                        Reference = 0x0,
                        CompareMask = 0xff,
                        WriteMask = 0xff,
                        FailOp = StencilOp.Zero,
                        DepthFailOp = StencilOp.Zero,
                        PassOp = StencilOp.Zero
                    };
                    ds.Back = ds.Front;
                }
            }

            return ds;
        }
    }
}
