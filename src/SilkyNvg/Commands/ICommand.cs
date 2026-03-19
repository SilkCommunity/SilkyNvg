using System.Numerics;
using SilkyNvg.Rendering;

namespace SilkyNvg.Commands
{
    internal interface ICommand
    {

        Vector2 FillToScene(Matrix3x2 transform, Vector2 p0, Vector2 subpathFirstPoint, ref Vector4 bounds, Scene scene, RenderTolerances tol);

    }
}