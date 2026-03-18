using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal class IntersectionFindingShader : ShaderProgramme
    {

        private readonly int _fpTolUniformLocation;
        
        internal IntersectionFindingShader(GL gl)
            : base(gl, (LoadShaderCode("intersectionFindingShader.comp.glsl"), ShaderType.ComputeShader))
        {
            _fpTolUniformLocation = Gl.GetUniformLocation(ProgrammeID, "fpTol");
        }

        internal void LoadFpTol(float factor)
        {
            Gl.Uniform1(_fpTolUniformLocation, factor);
        }
        
    }
}