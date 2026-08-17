using System.Numerics;

namespace SilkyNvg.Rendering;

public readonly record struct PathData(int FirstVertex, uint VertexCount, int CoverStart);