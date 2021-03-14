using Silk.NET.OpenGL;

namespace SilkyNvg.OpenGL.Calls
{
    internal class ConvexFillCall : Call
    {

        public ConvexFillCall(int pathOffset, int pathCount, int triangleOffset, int triangleCount, int uniformOffset, Blend blendFunc)
            : base(pathOffset, pathCount, triangleOffset, triangleCount, uniformOffset, blendFunc) { }


        public override void Run(GLInterface glInterface, GL gl)
        {
            glInterface.SetUniforms(_uniformOffset, 0);
            glInterface.CheckError("convex fill");

            for (int i = 0; i < _pathCount; i++)
            {
                gl.DrawArrays(PrimitiveType.TriangleFan, glInterface.Paths[_pathOffset + i].FillOffset, (uint)glInterface.Paths[_pathOffset + i].FillCount);
                if (glInterface.Paths[_pathOffset + i].StrokeCount > 0)
                {
                    gl.DrawArrays(PrimitiveType.TriangleStrip, glInterface.Paths[_pathOffset + i].StrokeOffset, (uint)glInterface.Paths[_pathOffset + i].StrokeCount);
                }
            }
        }

    }
}
