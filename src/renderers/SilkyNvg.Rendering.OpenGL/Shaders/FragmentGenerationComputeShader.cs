using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal class FragmentGenerationComputeShader : ShaderProgramme
    {

        private readonly int _fpTolUniformLocation;
        
        internal FragmentGenerationComputeShader(GL gl)
            : base(gl, (LoadShaderCode("fragmentGenerationShader.comp.glsl"), ShaderType.ComputeShader))
        {
            _fpTolUniformLocation = Gl.GetUniformLocation(ProgrammeID, "fpTol");
        }

        internal void LoadFpTol(float factor)
        {
            Gl.Uniform1(_fpTolUniformLocation, factor);
        }
        
    }
}