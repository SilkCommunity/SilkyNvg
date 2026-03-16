using System;

namespace SilkyNvg.Rendering
{
    public interface ISilkyRenderer : IDisposable
    {

        void Init(RenderTolerances tolerances);

        void PrepareFrame();

        void AddFrame(FrameContainer frame);

        void Render();

    }
}