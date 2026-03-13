namespace SilkyNvg.Rendering
{
    public sealed class RenderTolerances
    {
        
        public float FloatingPointTol { get; private set; }

        public float MaxPathLength { get; } = 32.0f;
        
        internal RenderTolerances()
        {
            FloatingPointTol = 0.01f;
        }

        internal void Update(float pixelRatio)
        {
            FloatingPointTol = 0.01f / pixelRatio;
        }
        
    }
}