using System;
using System.Collections.Generic;

namespace SilkyNvg.Rendering
{
    public interface ISilkyRenderer : IDisposable
    {

        ISceneContainer SceneContainer { get; }
        
        void Resize(uint width, uint height, RenderTolerances tolerances);

        void BeginFrame();

        void EndFrame();

    }
}