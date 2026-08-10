using System;
using System.Numerics;

namespace SilkyNvg.Rendering
{
    public interface ISilkyRenderer : IDisposable
    {

        void FillPath(Vector2[] points, Vector3[] curveSpacePoints, int pointCount);

    }
}