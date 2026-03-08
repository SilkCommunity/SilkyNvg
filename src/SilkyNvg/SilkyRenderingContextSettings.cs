namespace SilkyNvg
{

    public class SilkyRenderingContextSettings
    {
        
        public bool Alpha { get; }
        
        public bool Desynchronised { get; }
        
        public SilkyRenderingContextSettings(bool alpha = true, bool desynchronised = false)
        {
            Alpha = alpha;
            Desynchronised = desynchronised;
        }
        
    }
}