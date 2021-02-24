using System;

namespace SilkyNvg.Core.Composite
{
    public class SrcOverOperation : ICompositeOperation
    {

        public BlendFactor SrcRGB => BlendFactor.One;

        public BlendFactor DstRGB => BlendFactor.OneMinusSrcAlpha;

        public BlendFactor SrcAlpha => BlendFactor.One;

        public BlendFactor DstAlpha => BlendFactor.OneMinusSrcAlpha;

    }
}
