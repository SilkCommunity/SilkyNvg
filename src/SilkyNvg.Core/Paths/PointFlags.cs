namespace SilkyNvg.Core.Paths
{
    internal enum PointFlags : short
    {

        Corner = 1 << 0,
        Left = 1 << 1,
        Bevel = 1 << 2,
        Innerbevel = 1 << 8

    }
}
