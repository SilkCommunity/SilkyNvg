using System;

namespace SilkyNvg.Common
{

    [Flags]
    internal enum PointFlags
    {

        Corner = 1 << 0,
        Left = 1 << 1,
        Bevel = 1 << 2,
        Innerbevel = 1 << 3

    }
}
