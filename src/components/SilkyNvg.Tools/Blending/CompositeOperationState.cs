namespace SilkyNvg.Blending
{
    public struct CompositeOperationState
    {

        public BlendFactor SrcRgb { get; }

        public BlendFactor DstRgb { get; }

        public BlendFactor SrcAlpha { get; }

        public BlendFactor DstAlpha { get; }

        public CompositeOperationState(BlendFactor srcRgb, BlendFactor dstRgb, BlendFactor srcAlpha, BlendFactor dstAlpha)
        {
            SrcRgb = srcRgb;
            DstRgb = dstRgb;
            SrcAlpha = srcAlpha;
            DstAlpha = dstAlpha;
        }

        public CompositeOperationState(BlendFactor srcFactor, BlendFactor dstFactor) : this(srcFactor, dstFactor, srcFactor, dstFactor) { }

        public CompositeOperationState(CompositeOperation op)
        {
            BlendFactor srcFactor, dstFactor;

            switch (op)
            {
                case CompositeOperation.SourceOver:
                    srcFactor = BlendFactor.One;
                    dstFactor = BlendFactor.OneMinusSrcAlpha;
                    break;
                case CompositeOperation.SourceIn:
                    srcFactor = BlendFactor.DstAlpha;
                    dstFactor = BlendFactor.Zero;
                    break;
                case CompositeOperation.SourceOut:
                    srcFactor = BlendFactor.OneMinusDstAlpha;
                    dstFactor = BlendFactor.Zero;
                    break;
                case CompositeOperation.Atop:
                    srcFactor = BlendFactor.DstAlpha;
                    dstFactor = BlendFactor.OneMinusSrcAlpha;
                    break;
                case CompositeOperation.DestinationOver:
                    srcFactor = BlendFactor.OneMinusDstAlpha;
                    dstFactor = BlendFactor.One;
                    break;
                case CompositeOperation.DestinationIn:
                    srcFactor = BlendFactor.Zero;
                    dstFactor = BlendFactor.SrcAlpha;
                    break;
                case CompositeOperation.DestinationOut:
                    srcFactor = BlendFactor.Zero;
                    dstFactor = BlendFactor.OneMinusSrcAlpha;
                    break;
                case CompositeOperation.DestinationAtop:
                    srcFactor = BlendFactor.OneMinusDstAlpha;
                    dstFactor = BlendFactor.SrcAlpha;
                    break;
                case CompositeOperation.Lighter:
                    srcFactor = BlendFactor.One;
                    dstFactor = BlendFactor.One;
                    break;
                case CompositeOperation.Copy:
                    srcFactor = BlendFactor.One;
                    dstFactor = BlendFactor.Zero;
                    break;
                case CompositeOperation.XOR:
                    srcFactor = BlendFactor.OneMinusDstAlpha;
                    dstFactor = BlendFactor.OneMinusSrcAlpha;
                    break;
                default:
                    srcFactor = BlendFactor.One;
                    dstFactor = BlendFactor.Zero;
                    break;
            }

            SrcRgb = SrcAlpha = srcFactor;
            DstRgb = DstAlpha = dstFactor;
        }

    }
}
