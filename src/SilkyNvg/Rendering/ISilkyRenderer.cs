using System;

namespace SilkyNvg.Rendering
{
    public interface ISilkyRenderer : IDisposable
    {

        void Init(RenderTolerances tolerances);

        void Resize(float width, float height, float pixelRatio);
        
        void AddScene(Scene scene);

        void Render();

    }
}