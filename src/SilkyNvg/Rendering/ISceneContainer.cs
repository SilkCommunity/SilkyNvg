using System.Numerics;

namespace SilkyNvg.Rendering;

public interface ISceneContainer
{

    uint AddLinear(Vector2 p0, Vector2 p1, float offset);

    uint AddQuadratic(Vector2 p0, Vector2 p1, Vector2 p2, float offset);

    uint AddCubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float offset);

    uint AddRational(Vector2 p0, Vector2 p1, Vector2 p2, float w, float offset);

    uint AddSubpath();

    uint AddPath();

    uint Clear();

}