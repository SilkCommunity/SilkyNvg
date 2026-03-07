using System;
using System.Collections.Generic;
using System.Numerics;

namespace SilkyNvg.Rendering;

public interface ISilkyRenderer : IDisposable
{

    public void Render(List<Vector2> vertices, Vector2 viewExtent);

}