using System;
using System.Collections.Generic;

namespace SilkyNvg.Rendering
{
    public interface ISilkyRenderer : IDisposable
    {

        void Resize(uint width, uint height, RenderTolerances tolerances);

    }
}