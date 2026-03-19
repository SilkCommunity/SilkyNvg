namespace SilkyNvg.Rendering
{
    public readonly struct PathData
    {

        public readonly uint PointIndex;
        public readonly uint PointCount;

        public readonly uint QuadraticDiscardTrianglesIndex;
        public readonly uint QuadraticDiscardTrianglesCount;
        
        public readonly uint CubicDiscardTrianglesIndex;
        public readonly uint CubicDiscardTrianglesCount;

        internal PathData(uint pointIndex, uint pointCount, uint quadraticDiscardTrianglesIndex,
            uint quadraticDiscardTrianglesCount, uint cubicDiscardTrianglesIndex, uint cubicDiscardTrianglesCount)
        {
            PointIndex = pointIndex;
            PointCount = pointCount;
            QuadraticDiscardTrianglesIndex = quadraticDiscardTrianglesIndex;
            QuadraticDiscardTrianglesCount = quadraticDiscardTrianglesCount;
            CubicDiscardTrianglesIndex = cubicDiscardTrianglesIndex;
            CubicDiscardTrianglesCount = cubicDiscardTrianglesCount;
        }
        
    }
}