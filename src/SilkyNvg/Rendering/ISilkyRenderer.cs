using System;

namespace SilkyNvg.Rendering
{
    public interface ISilkyRenderer : IDisposable
    {

        void Init(RenderTolerances tolerances);

        void Resize(uint newWidth, uint newHeight);

        void PrepareFrame();

        void AddFrame(FrameContainer frame);

        void Render();

    }
}