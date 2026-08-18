namespace SilkyNvg.Rendering;

public sealed class RenderTolerances
{

    public float FloatingPointTol { get; private set; } = 0.01f;

    public float PixelRatio { get; private set; } = 1.0f;
    
    internal void Update(float pixelRatio)
    {
        FloatingPointTol = 0.01f / pixelRatio;
        PixelRatio = pixelRatio;
    }

}