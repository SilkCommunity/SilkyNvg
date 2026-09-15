using System.Numerics;

namespace SilkyNvg.Rendering;

public interface ISceneContainer
{

    void AddLinear(Vector2 p0, Vector2 p1, float offset);

    void AddQuadratic(Vector2 p0, Vector2 p1, Vector2 p2, float offset);

    void AddCubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float offset);

    void AddRational(Vector2 p0, Vector2 p1, Vector2 p2, float w, float offset);

    void AddSubpath();

    void AddPath();

    void Clear();

}