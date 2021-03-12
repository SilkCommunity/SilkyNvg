namespace SilkyNvg.Core
{
    internal class Style
    {

        private float _tesselationTollerance;
        private float _distributionTollerance;
        private float _fringeWidth;
        private float _pixelRatio;

        public float TesselationTollerance => _tesselationTollerance;
        public float DistributionTollerance => _distributionTollerance;
        public float FringeWidth => _fringeWidth;
        public float PixelRatio => _pixelRatio;

        public Style(float pixelRatio)
        {
            CalculateForPixelRatio(pixelRatio);
        }

        public void CalculateForPixelRatio(float pixelRatio)
        {
            _tesselationTollerance = 0.25f / pixelRatio;
            _distributionTollerance = 0.01f / pixelRatio;
            _fringeWidth = 1.0f / pixelRatio;
            _pixelRatio = pixelRatio;
        }

    }
}