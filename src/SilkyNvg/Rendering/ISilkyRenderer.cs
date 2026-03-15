using System;

namespace SilkyNvg.Rendering
{
    public interface ISilkyRenderer : IDisposable
    {

        void PrepareFrame();

        void AddFrame(FrameContainer frame);

        void Render();

    }
}