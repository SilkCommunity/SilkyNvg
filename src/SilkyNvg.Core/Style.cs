namespace SilkyNvg.Core
{
    public class Style
    {

        public float TesselationTollerance { get; private set; }
        public float DistributionTollerance { get; private set; }
        public float FringeWidth { get; private set; }
        public float PixelRatio { get; private set; }

        public Style(float pixelRatio)
        {
            Update(pixelRatio);
        }

        public void Update(float pixelRatio)
        {
            TesselationTollerance = 0.25f / pixelRatio;
            DistributionTollerance = 0.01f / pixelRatio;
            FringeWidth = 1.0f / pixelRatio;
            PixelRatio = pixelRatio;
        }

    }
}
