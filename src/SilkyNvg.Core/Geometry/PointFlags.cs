namespace SilkyNvg.Core.Geometry
{
    public enum PointFlags
    {

        PointNone = 0,
        PointCorner = 1 << 0,
        PointLeft = 1 << 1,
        PointBevel = 1 << 2,
        PointInnerbevel = 1 << 3

    }
}
