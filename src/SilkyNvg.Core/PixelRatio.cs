namespace SilkyNvg.Core
{
    internal class PixelRatio
    {

        public float TessTol { get; private set; }

        public float DistTol { get; private set; }

        public float FringeWidth { get; private set; }

        public float DevicePxRatio
        {
            get => DevicePxRatio;
            set
            {
                TessTol = 0.25f / value;
                DistTol = 0.01f / value;
                FringeWidth = 1.0f / value;
            }
        }

    }
}
