using System;
using System.Collections.Generic;

namespace SilkyNvg.Rendering
{
    public interface ISilkyRenderer : IDisposable
    {

        void Render(Vertex[] vertexData, uint vertexCount, IReadOnlyList<PathData> pathData);

    }
}