using Silk.NET.Vulkan;

namespace SilkyNvg.Rendering.Vulkan.Pipelines
{
    internal struct PipelineSettings
    {

        public CullModeFlags CullMode;
        public FrontFace FrontFace;

        public bool DepthTestEnabled;

        public ColorComponentFlags ColourMask;

        public uint StencilWriteMask;

        public StencilOp FrontStencilFailOp;
        public StencilOp FrontStencilDepthFailOp;
        public StencilOp FrontStencilPassOp;
        public StencilOp BackStencilFailOp;
        public StencilOp BackStencilDepthFailOp;
        public StencilOp BackStencilPassOp;

        public StencilOp StencilFailOp
        {
            set => FrontStencilFailOp = BackStencilFailOp = value;
        }

        public StencilOp StencilDepthFailOp
        {
            set => FrontStencilDepthFailOp = BackStencilDepthFailOp = value;
        }

        public StencilOp StencilPassOp
        {
            set => FrontStencilPassOp = BackStencilPassOp = value;
        }

        public CompareOp StencilFunc;
        public uint StencilRef;
        public uint StencilMask;

        public bool StencilTestEnable;

        public PrimitiveTopology Topology;

        public Blending.CompositeOperationState CompositeOperation;

        public BlendFactor SrcRgb => ConvertBlend(CompositeOperation.SrcRgb);

        public BlendFactor SrcAlpha => ConvertBlend(CompositeOperation.SrcAlpha);

        public BlendFactor DstRgb => ConvertBlend(CompositeOperation.DstRgb);

        public BlendFactor DstAlpha => ConvertBlend(CompositeOperation.DstAlpha);

        private static BlendFactor ConvertBlend(Blending.BlendFactor factor)
        {
            return factor switch
            {
                Blending.BlendFactor.Zero => BlendFactor.Zero,
                Blending.BlendFactor.One => BlendFactor.One,
                Blending.BlendFactor.SrcColour => BlendFactor.SrcColor,
                Blending.BlendFactor.OneMinusSrcColour => BlendFactor.OneMinusSrcColor,
                Blending.BlendFactor.DstColour => BlendFactor.DstColor,
                Blending.BlendFactor.OneMinusDstColour => BlendFactor.OneMinusDstColor,
                Blending.BlendFactor.SrcAlpha => BlendFactor.SrcAlpha,
                Blending.BlendFactor.OneMinusSrcAlpha => BlendFactor.OneMinusSrcAlpha,
                Blending.BlendFactor.DstAlpha => BlendFactor.DstAlpha,
                Blending.BlendFactor.OneMinusDstAlpha => BlendFactor.OneMinusDstAlpha,
                Blending.BlendFactor.SrcAlphaSaturate => BlendFactor.SrcAlphaSaturate,
                _ => BlendFactor.ConstantColor
            };
        }

        private static PipelineSettings Default()
        {
            return new()
            {
                CullMode = CullModeFlags.CullModeBackBit,
                FrontFace = FrontFace.CounterClockwise,
                DepthTestEnabled = false,
                ColourMask = ColorComponentFlags.ColorComponentRBit | ColorComponentFlags.ColorComponentGBit | ColorComponentFlags.ColorComponentBBit | ColorComponentFlags.ColorComponentABit,
                StencilMask = 0xffffffff,
                StencilFailOp = StencilOp.Keep,
                StencilDepthFailOp = StencilOp.Keep,
                StencilPassOp = StencilOp.Keep,
                StencilFunc = CompareOp.Always,
                StencilRef = 0,
                StencilWriteMask = 0xffffffff,
            };
        }

        public static PipelineSettings ConvexFill(Blending.CompositeOperationState compositeOperation, bool dontUseFan)
        {
            PipelineSettings settings = Default();
            settings.CompositeOperation = compositeOperation;
            settings.StencilTestEnable = false;
            settings.Topology = dontUseFan ? PrimitiveTopology.TriangleList : PrimitiveTopology.TriangleFan;
            return settings;
        }

        public static PipelineSettings ConvexFillEdgeAA(Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = ConvexFill(compositeOperation, true);
            settings.Topology = PrimitiveTopology.TriangleStrip;
            return settings;
        }

        public static PipelineSettings FillStencil(Blending.CompositeOperationState compositeOperation, bool dontUseFan)
        {
            PipelineSettings settings = Default();

            settings.StencilTestEnable = true;

            settings.StencilWriteMask = 0xff;

            settings.StencilFunc = CompareOp.Always;
            settings.StencilRef = 0;
            settings.StencilMask = 0xff;

            settings.ColourMask = 0;

            settings.FrontStencilFailOp = StencilOp.Keep;
            settings.FrontStencilDepthFailOp = StencilOp.Keep;
            settings.FrontStencilPassOp = StencilOp.IncrementAndWrap;

            settings.BackStencilFailOp = StencilOp.Keep;
            settings.BackStencilDepthFailOp = StencilOp.Keep;
            settings.BackStencilPassOp = StencilOp.DecrementAndWrap;

            settings.CullMode = CullModeFlags.CullModeNone;

            settings.CompositeOperation = compositeOperation;
            settings.Topology = dontUseFan ? PrimitiveTopology.TriangleList : PrimitiveTopology.TriangleFan;

            return settings;
        }

        public static PipelineSettings FillEdgeAA(Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = FillStencil(compositeOperation, true);

            settings.CullMode = CullModeFlags.CullModeBackBit;

            settings.ColourMask = ColorComponentFlags.ColorComponentRBit | ColorComponentFlags.ColorComponentGBit | ColorComponentFlags.ColorComponentBBit | ColorComponentFlags.ColorComponentABit;

            settings.StencilFunc = CompareOp.Equal;
            settings.StencilRef = 0x0;
            settings.StencilMask = 0xff;

            settings.StencilFailOp = StencilOp.Keep;
            settings.StencilDepthFailOp = StencilOp.Keep;
            settings.StencilPassOp = StencilOp.Keep;

            settings.CompositeOperation = compositeOperation;
            settings.Topology = PrimitiveTopology.TriangleStrip;

            return settings;
        }

        public static PipelineSettings Fill(Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = FillEdgeAA(compositeOperation);

            settings.StencilFunc = CompareOp.NotEqual;
            settings.StencilRef = 0x0;
            settings.StencilMask = 0xff;

            settings.StencilFailOp = StencilOp.Zero;
            settings.StencilDepthFailOp = StencilOp.Zero;
            settings.StencilPassOp = StencilOp.Zero;

            settings.CompositeOperation = compositeOperation;
            settings.Topology = PrimitiveTopology.TriangleStrip;

            return settings;
        }

        public static PipelineSettings Stroke(Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = Default();

            settings.CompositeOperation = compositeOperation;
            settings.Topology = PrimitiveTopology.TriangleStrip;

            return settings;
        }

        public static PipelineSettings StencilStrokeStencil(Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = Default();

            settings.StencilTestEnable = true;

            settings.StencilMask = 0xff;

            settings.StencilFunc = CompareOp.Equal;
            settings.StencilRef = 0x0;
            settings.StencilMask = 0xff;

            settings.StencilFailOp = StencilOp.Keep;
            settings.StencilDepthFailOp = StencilOp.Keep;
            settings.StencilPassOp = StencilOp.IncrementAndClamp;

            settings.CompositeOperation = compositeOperation;
            settings.Topology = PrimitiveTopology.TriangleStrip;

            return settings;
        }

        public static PipelineSettings StencilStrokeEdgeAA(Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = StencilStrokeStencil(compositeOperation);

            settings.StencilFunc = CompareOp.Equal;
            settings.StencilRef = 0x0;
            settings.StencilMask = 0xff;

            settings.StencilFailOp = StencilOp.Keep;
            settings.StencilDepthFailOp = StencilOp.Keep;
            settings.StencilPassOp = StencilOp.Keep;

            settings.CompositeOperation = compositeOperation;
            settings.Topology = PrimitiveTopology.TriangleStrip;

            return settings;
        }

        public static PipelineSettings StencilStroke(Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = StencilStrokeEdgeAA(compositeOperation);

            settings.ColourMask = ColorComponentFlags.ColorComponentRBit | ColorComponentFlags.ColorComponentGBit | ColorComponentFlags.ColorComponentGBit | ColorComponentFlags.ColorComponentABit;

            settings.StencilFunc = CompareOp.Always;
            settings.StencilRef = 0x0;
            settings.StencilMask = 0xff;

            settings.StencilFailOp = StencilOp.Zero;
            settings.StencilDepthFailOp = StencilOp.Zero;
            settings.StencilPassOp = StencilOp.Zero;

            settings.CompositeOperation = compositeOperation;
            settings.Topology = PrimitiveTopology.TriangleStrip;

            return settings;
        }

        public static PipelineSettings Triangles(Blending.CompositeOperationState compositeOperation)
        {
            PipelineSettings settings = Default();

            settings.CompositeOperation = compositeOperation;
            settings.Topology = PrimitiveTopology.TriangleList;

            return settings;
        }

    }
}
