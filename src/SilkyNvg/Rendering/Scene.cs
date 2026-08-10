using System.Numerics;
using SilkyNvg.Utils;

namespace SilkyNvg.Rendering
{
    internal class Scene
    {

        private const int InitVertexCount = 256;
        private const int InitTriangleCount = 256;
        
        // Per vertex data
        private readonly DataAccessibleArrayList<Vector2> _vertices;
        private readonly DataAccessibleArrayList<Vector3> _curveSpaceVerts;
        
        // Per triangle data
        private readonly DataAccessibleArrayList<uint> _shapeCounters;

        internal Scene()
        {
            _vertices = new DataAccessibleArrayList<Vector2>(InitVertexCount);
            _curveSpaceVerts = new DataAccessibleArrayList<Vector3>(InitVertexCount);
            
            _shapeCounters = new DataAccessibleArrayList<uint>(InitTriangleCount);
        }

        internal void PushVertices(params Vector2[] verts)
        {
            _vertices.AddRange(verts);
        }

        internal void PushCurveSpaceVerts(params Vector3[] csVerts)
        {
            _curveSpaceVerts.AddRange(csVerts);
        }

    }
}