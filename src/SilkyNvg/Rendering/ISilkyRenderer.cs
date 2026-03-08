using System;
using System.Collections.Generic;
using System.Numerics;

namespace SilkyNvg.Rendering
{
    
    public interface ISilkyRenderer : IDisposable
    {

        void Render(FrameContainer container, Vector2 viewExtent);

    }
    
}
