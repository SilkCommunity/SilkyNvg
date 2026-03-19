namespace SilkyNvg.Rendering.OpenGL.Fragments
{
    internal readonly struct FragmentData
    {

        public readonly int PixelX;
        public readonly int PixelY;

        public readonly int WindingNumber;
        
        public readonly uint PathID;

        public FragmentData(int pixelX, int pixelY, int windingNumber, uint pathId)
        {
            PixelX = pixelX;
            PixelY = pixelY;
            WindingNumber = windingNumber;
            PathID = pathId;
        }
        
    }
}