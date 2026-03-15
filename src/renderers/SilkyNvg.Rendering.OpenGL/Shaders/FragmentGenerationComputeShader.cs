using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal class FragmentGenerationComputeShader : ShaderProgramme
    {

        private readonly int _factorUniformLocation;
        
        internal FragmentGenerationComputeShader(GL gl)
            : base(gl, (LoadShaderCode("fragmentGenerationShader.comp.glsl"), ShaderType.ComputeShader))
        {
            _factorUniformLocation = Gl.GetUniformLocation(ProgrammeID, "factor");
        }

        internal void LoadFactor(uint factor)
        {
            Gl.Uniform1(_factorUniformLocation, factor);
        }
        
    }
}