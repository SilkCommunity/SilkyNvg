namespace SilkyNvg
{
    public struct CompositeOperationState
    {

        private readonly BlendFactor _srcRgb;
        private readonly BlendFactor _dstRgb;
        private readonly BlendFactor _srcAlpha;
        private readonly BlendFactor _dstAlpha;

        public BlendFactor SrcRgb => _srcRgb;
        public BlendFactor DstRgb => _dstRgb;
        public BlendFactor SrcAlpha => _srcAlpha;
        public BlendFactor DstAlpha => _dstAlpha;

        /// <summary>
        /// Sets both source factors to sfactor and both destination factors to dfactor.
        /// </summary>
        /// <param name="sfactor">The source factor</param>
        /// <param name="dfactor">The destination factor</param>
        public CompositeOperationState(BlendFactor sfactor, BlendFactor dfactor) : this(sfactor, dfactor, sfactor, dfactor) { }

        public CompositeOperationState(BlendFactor srcRgb, BlendFactor dstRgb, BlendFactor srcAlpha, BlendFactor dstAlpha)
        {
            _srcRgb = srcRgb;
            _dstRgb = dstRgb;
            _srcAlpha = srcAlpha;
            _dstAlpha = dstAlpha;
        }

        /// <summary>
        /// Create a CompositeOperationState from <see cref="CompositeOperation"/>
        /// </summary>
        /// <param name="op">The CompositeOperation to base this on.</param>
        public CompositeOperationState(CompositeOperation op)
        {
            BlendFactor sfactor, dfactor;
            switch (op)
            {
                case CompositeOperation.SourceOver:
                    sfactor = BlendFactor.One;
                    dfactor = BlendFactor.OneMinusSrcAlpha;
                    break;
                case CompositeOperation.SourceIn:
                    sfactor = BlendFactor.DstAlpha;
                    dfactor = BlendFactor.Zero;
                    break;
                case CompositeOperation.SourceOut:
                    sfactor = BlendFactor.OneMinusDstAlpha;
                    dfactor = BlendFactor.Zero;
                    break;
                case CompositeOperation.Atop:
                    sfactor = BlendFactor.DstAlpha;
                    dfactor = BlendFactor.OneMinusSrcAlpha;
                    break;
                case CompositeOperation.DestinationOver:
                    sfactor = BlendFactor.OneMinusDstAlpha;
                    dfactor = BlendFactor.One;
                    break;
                case CompositeOperation.DestinationIn:
                    sfactor = BlendFactor.Zero;
                    dfactor = BlendFactor.SrcAlpha;
                    break;
                case CompositeOperation.DestinationOut:
                    sfactor = BlendFactor.Zero;
                    dfactor = BlendFactor.OneMinusSrcAlpha;
                    break;
                case CompositeOperation.DestinationAtop:
                    sfactor = BlendFactor.OneMinusDstAlpha;
                    dfactor = BlendFactor.SrcAlpha;
                    break;
                case CompositeOperation.Lighter:
                    sfactor = BlendFactor.One;
                    dfactor = BlendFactor.One;
                    break;
                case CompositeOperation.Copy:
                    sfactor = BlendFactor.One;
                    dfactor = BlendFactor.Zero;
                    break;
                case CompositeOperation.XOr:
                    sfactor = BlendFactor.OneMinusDstAlpha;
                    dfactor = BlendFactor.OneMinusSrcAlpha;
                    break;
                default:
                    sfactor = BlendFactor.One;
                    dfactor = BlendFactor.Zero;
                    break;
            }

            _srcRgb = sfactor;
            _dstRgb = dfactor;
            _srcAlpha = sfactor;
            _dstAlpha = dfactor;
        }

    }
}