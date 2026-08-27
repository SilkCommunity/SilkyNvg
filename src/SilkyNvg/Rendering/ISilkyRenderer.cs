using System;
using System.Numerics;

namespace SilkyNvg.Rendering;

public interface ISilkyRenderer : IDisposable
{
    
    ISceneContainer Container { get; }

    void Resize(float newWidth, float newHeight, RenderTolerances tolerances);

}