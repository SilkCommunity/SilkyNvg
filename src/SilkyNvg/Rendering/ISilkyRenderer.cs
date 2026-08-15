using System;

namespace SilkyNvg.Rendering
{
    public interface ISilkyRenderer : IDisposable
    {

        void FillPath(Vertex[] vertexData, uint vertexCount);

    }
}