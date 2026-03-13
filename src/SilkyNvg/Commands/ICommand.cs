using System.Numerics;
using SilkyNvg.Rendering;

namespace SilkyNvg.Commands
{
    internal interface ICommand
    {

        Vector2 Fill(Matrix3x2 transform, Vector2 p0, FrameContainer frame, RenderTolerances tol);

    }
}