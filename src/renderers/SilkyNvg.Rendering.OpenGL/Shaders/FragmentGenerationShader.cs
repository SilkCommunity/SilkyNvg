using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal class FragmentGenerationShader : ShaderProgramme
    {

        private readonly int _intersectionCountUniformLocation;
        private readonly int _screenDimensionsUniformLocation;
        
        internal FragmentGenerationShader(GL gl)
            : base(gl, (LoadShaderCode("fragmentGenerationShader.comp.glsl"), ShaderType.ComputeShader))
        {
            _intersectionCountUniformLocation = Gl.GetUniformLocation(ProgrammeID, "intersectionCount");
            _screenDimensionsUniformLocation = Gl.GetUniformLocation(ProgrammeID, "screenDimensions");
        }

        internal void LoadIntersectionCount(uint intersectionCount)
        {
            Gl.Uniform1(_intersectionCountUniformLocation, intersectionCount);
        }

        internal void LoadScreenDimensions(uint screenWidth, uint screenHeight)
        {
            Gl.Uniform2(_screenDimensionsUniformLocation, screenWidth, screenHeight);
        }
        
    }
}