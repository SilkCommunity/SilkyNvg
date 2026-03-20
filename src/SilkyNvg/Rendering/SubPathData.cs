namespace SilkyNvg.Rendering
{
    public struct SubPathData
    {
        
        public readonly uint AnchorGeometryIndex;
        public readonly uint AnchorGeometryCount;

        public readonly uint QuadraticDiscardTrianglesIndex;
        public readonly uint QuadraticDiscardTrianglesCount;
        
        public readonly uint CubicDiscardTrianglesIndex;
        public readonly uint CubicDiscardTrianglesCount;
        
        internal SubPathData(uint anchorGeometryIndex, uint anchorGeometryCount, uint quadraticDiscardTrianglesIndex,
            uint quadraticDiscardTrianglesCount, uint cubicDiscardTrianglesIndex, uint cubicDiscardTrianglesCount)
        {
            AnchorGeometryIndex = anchorGeometryIndex;
            AnchorGeometryCount = anchorGeometryCount;
            QuadraticDiscardTrianglesIndex = quadraticDiscardTrianglesIndex;
            QuadraticDiscardTrianglesCount = quadraticDiscardTrianglesCount;
            CubicDiscardTrianglesIndex = cubicDiscardTrianglesIndex;
            CubicDiscardTrianglesCount = cubicDiscardTrianglesCount;
        }
        
    }
}