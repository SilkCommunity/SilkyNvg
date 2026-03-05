using SilkyNvg.Rendering;

namespace SilkyNvg
{
    public class SilkyRenderingContext
    {

        private readonly SilkyRenderingContextSettings _settings;
        private readonly ISilkyRenderer _renderer;

        public uint Width { get; set; } = 600;

        public uint Height { get; set; } = 400;

        public float PixelRatio { get; set; } = 1.0f;

        public SilkyRenderingContext(SilkyRenderingContextSettings settings, ISilkyRenderer renderer)
        {
            _settings = settings;
            _renderer = renderer;
        }
        
        public SilkyRenderingContextSettings GetContextSettings()
            =>  _settings;
        
        // DIMENSIONS

        public void SetWidth(int width) => Width = (uint)width;
        
        public void SetHeight(int height) => Height = (uint)height;

        public void Resize(int newWidth, int newHeight)
        {
            SetWidth(newWidth);
            SetHeight(newHeight);
        }
        
        

    }
}