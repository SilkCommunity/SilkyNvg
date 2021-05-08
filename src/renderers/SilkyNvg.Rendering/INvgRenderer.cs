using SilkyNvg.Common;
using System.Numerics;

namespace SilkyNvg.Rendering
{
    public interface INvgRenderer
    {

        bool Create(LaunchParameters launchParameters);

        int CreateTexture();

        void DeleteTexture();

        void UpdateTexture();

        Vector2 GetTextureSize();

        void Viewport(Vector2 viewSize);

        void Cancel();

        void Flush();

        void Fill();

        void Stroke();

        void Triangles();

        void Delete();

    }
}
