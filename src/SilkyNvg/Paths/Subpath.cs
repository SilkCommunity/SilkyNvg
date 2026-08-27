using System.Numerics;

namespace SilkyNvg.Paths
{
    internal class Subpath
    {
        
        internal bool IsClosed { get; private set; }
        
        internal Vector2 Start { get; }
        
        internal Subpath(float startX, float startY)
        {
            Start = new Vector2(startX, startY);
        }
        
        internal void AddLine(float x, float y)
        {
            
        }

        internal void AddQuadratic(float cpx, float cpy, float x, float y)
        {
            
        }

        internal void AddCubic(float cp1x, float cp1y, float cp2x, float cp2y, float x, float y)
        {
            
        }

        internal void AddArcTo(float x1, float y1, float x2, float y2, float radius)
        {
            
        }

        internal void AddEllipse(float x, float y, float radiusX, float radiusY, float startAngle, float endAngle, float rotation)
        {
            
        }

        internal void Close()
        {
            IsClosed = true;
        }

    }
}