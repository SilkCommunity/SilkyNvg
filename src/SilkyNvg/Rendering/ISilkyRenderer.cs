using System;
using System.Numerics;

namespace SilkyNvg.Rendering
{
    public interface ISilkyRenderer : IDisposable
    {

        void FillPath(float[] vertexData, uint vertexCount);

    }
}