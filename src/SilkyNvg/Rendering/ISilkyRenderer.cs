using System;
using System.Numerics;

namespace SilkyNvg.Rendering;

public interface ISilkyRenderer : IDisposable
{

    void Resize(float newWidth, float newHeight, RenderTolerances tolerances);

}