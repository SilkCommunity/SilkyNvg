namespace SilkyNvg
{
    public class CompositeOperationState
    {

        private readonly BlendFactor _srcRgb;
        private readonly BlendFactor _dstRgb;
        private readonly BlendFactor _srcAlpha;
        private readonly BlendFactor _dstAlpha;

        public CompositeOperationState(BlendFactor srcRgb, BlendFactor dstRgb, BlendFactor srcAlpha, BlendFactor dstAlpha)
        {
            _srcRgb = srcRgb;
            _dstRgb = dstRgb;
            _srcAlpha = srcAlpha;
            _dstAlpha = dstAlpha;
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

            _srcRgb = _srcAlpha = srcFactor;
            _dstRgb = _dstAlpha = dstFactor;
        }

    }
}
