using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using SilkyNvg.Rendering.Vulkan.Shaders;
using Veldrid;
using Veldrid.SPIRV;
using Shader = Veldrid.Shader;


namespace SilkyNvg.Rendering.Vulkan.Pipelines
{
    public class PipelineCache
    {

        NvgFrame _frame;
        public PipelineCache(NvgFrame frame)
        {
            _frame = frame;
        }

        public void SetFrame(NvgFrame frame)
        {
            _frame = frame;
            _pipelines.Clear();
        }


        readonly Dictionary<PipelineSettings, Pipeline> _pipelines = new Dictionary<PipelineSettings, Pipeline>();

    
        public Pipeline GetPipeLine(PipelineSettings renderPipeline, VeldridRenderer renderer)
        {

            if (!_pipelines.TryGetValue(renderPipeline, out Pipeline potentialPipeline))
            {
                potentialPipeline = AddPipeline(renderPipeline, renderer);
            }
            return potentialPipeline;
        }
    
        public void Clear()
        {
            foreach (KeyValuePair<PipelineSettings, Pipeline> set in _pipelines)
            {
            
                set.Value.Dispose();
            }
            _pipelines.Clear();
        }

        public Pipeline AddPipeline(PipelineSettings settings, VeldridRenderer renderer)
        {
            DepthStencilStateDescription depthStencil = DepthStencilState(settings);

            RasterizerStateDescription rasterizerState = RasterizationState(settings);
        
        
            GraphicsDevice device = renderer.Device;
            byte[] vsBytes = renderer.Shader.VertexShaderStage;
            byte[] fsBytes = renderer.Shader.FragmentShaderStage;

            if(device.BackendType != GraphicsBackend.Vulkan)
            {
                CrossCompileTarget target = device.BackendType switch
                {
                    GraphicsBackend.Direct3D11 => CrossCompileTarget.HLSL,
                    GraphicsBackend.Metal => CrossCompileTarget.MSL,
                    GraphicsBackend.OpenGL => CrossCompileTarget.GLSL,
                    GraphicsBackend.OpenGLES => CrossCompileTarget.ESSL,
                    GraphicsBackend.Vulkan => throw new NotImplementedException(),
                    _ => throw new NotImplementedException(),
                };


                VertexFragmentCompilationResult things = SpirvCompilation.CompileVertexFragment(
                    renderer.Shader.VertexShaderStage,
                    renderer.Shader.FragmentShaderStage,
                    target
                );
                vsBytes = Encoding.UTF8.GetBytes(things.VertexShader);
                fsBytes = Encoding.UTF8.GetBytes(things.FragmentShader);

            }

            Shader vs = device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex,vsBytes, "main"));
            Shader fs = device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fsBytes, "main"));

            ShaderSetDescription shaderSet = new ShaderSetDescription(new[]
            {
                new VertexLayoutDescription(new[]
                {
                    new VertexElementDescription("vertex",  VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
                    new VertexElementDescription("tcoord",  VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
                }),
                new VertexLayoutDescription((uint)Unsafe.SizeOf<FragUniforms>(), 1, new []
                    {
                        //Matrix 1
                        new VertexElementDescription("Matrix11xx", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                        new VertexElementDescription("Matrix12xx", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                        new VertexElementDescription("Matrix13xx", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                
                        //Matrix 2
                        new VertexElementDescription("Matrix21xx", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                        new VertexElementDescription("Matrix22xx", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                        new VertexElementDescription("Matrix23xx", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                
                        new VertexElementDescription("innerCol", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                        new VertexElementDescription("outerCol", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                
                        new VertexElementDescription("scissorExt", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
                        new VertexElementDescription("scissorScale", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
                        new VertexElementDescription("extent", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
                
                        new VertexElementDescription("rfss", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate),
                        new VertexElementDescription("tt", VertexElementFormat.Int2, VertexElementSemantic.TextureCoordinate),
                
                    }
                )
            }, new[]{vs, fs});

            BlendStateDescription blendState = ColourBlendState(ColorBlendAttachmentState(settings));

            Pipeline pipeline = device.ResourceFactory.CreateGraphicsPipeline(
                new GraphicsPipelineDescription(blendState, depthStencil, rasterizerState, settings.Topology, shaderSet, new []{renderer.DescriptorSetLayout}, device.MainSwapchain.Framebuffer.OutputDescription, ResourceBindingModel.Default));

            _pipelines[settings] = pipeline;

            return pipeline;
        }

        public void Dispose()
        {
            foreach (KeyValuePair<PipelineSettings, Pipeline> pipelineSet in _pipelines)
            {
                pipelineSet.Value.Dispose();
            }
        }
    
        public static unsafe BlendStateDescription ColourBlendState(BlendAttachmentDescription colourBlendAttachmentState)
        {

            BlendStateDescription Blendstate = new BlendStateDescription();
            Blendstate.AttachmentStates = new[] {colourBlendAttachmentState};
            return Blendstate;
        }
    
        private static BlendAttachmentDescription ColorBlendAttachmentState(PipelineSettings settings)
        {
            return new BlendAttachmentDescription
            {
                ColorWriteMask = settings.ColourMask,
                BlendEnabled= true,
                SourceColorFactor = settings.SrcRgb,
                SourceAlphaFactor = settings.SrcAlpha,
                ColorFunction = BlendFunction.Add,
                DestinationColorFactor = settings.DstRgb,
                DestinationAlphaFactor = settings.DstAlpha,
                AlphaFunction = BlendFunction.Add
            };
        }
    
        private static RasterizerStateDescription RasterizationState(PipelineSettings settings)
        {
            return new RasterizerStateDescription
            {
                FillMode = PolygonFillMode.Solid,
                CullMode = settings.CullMode,
                FrontFace = settings.FrontFace,

                DepthClipEnabled = false,
            };
        }

    
    
    
        public static DepthStencilStateDescription DepthStencilState(PipelineSettings settings)
        {
            DepthStencilStateDescription state = new DepthStencilStateDescription()
            {
            
                DepthWriteEnabled = false,

                DepthTestEnabled = settings.DepthTestEnabled,
                StencilTestEnabled = settings.StencilTestEnable,

                DepthComparison = ComparisonKind.LessEqual,
                StencilReference = settings.StencilRef,
                StencilWriteMask = settings.StencilWriteMask,
                StencilReadMask = settings.StencilMask,
            

                StencilFront = new StencilBehaviorDescription
                {
                    Comparison = settings.StencilFunc,
                    DepthFail = settings.FrontStencilDepthFailOp,
                    Fail = settings.FrontStencilFailOp,
                    Pass = settings.FrontStencilPassOp

                },
                StencilBack = new StencilBehaviorDescription
                {
                    Fail = settings.BackStencilFailOp,
                    DepthFail = settings.BackStencilDepthFailOp,
                    Pass = settings.BackStencilPassOp,
                    Comparison = settings.StencilFunc
                },
            };
            return state;
        }
    }
}