namespace SilkyNvg.Core.Composite
{
    public interface ICompositeOperation
    {

        BlendFactor SrcRGB { get; }
        BlendFactor DstRGB { get; }
        BlendFactor SrcAlpha { get; }
        BlendFactor DstAlpha { get; }

    }
}
