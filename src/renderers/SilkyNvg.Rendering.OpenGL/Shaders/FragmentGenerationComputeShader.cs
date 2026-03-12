using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal class FragmentGenerationComputeShader : ShaderProgramme
    {

        private const string ShaderCode =
            "#version 460 core\n" +
            "" +
            "layout(local_size_x=1, local_size_y=1, local_size_z=1) in;\n" +
            "" +
            "layout(std430, binding=1) readonly buffer input_0_buffer {\n" +
            "   uint input_0[];\n" +
            "};\n" +
            "" +
            "layout(std430, binding=2) readonly buffer input_1_buffer {\n" +
            "   uint input_1[];\n" +
            "};\n" +
            "" +
            "layout(std430, binding=3) writeonly buffer output_buffer {\n" +
            "   uint data[];\n" +
            "};\n" +
            "" +
            "uniform uint factor;\n" +
            "" +
            "void main() {\n" +
            "   uint index = gl_GlobalInvocationID.x;\n" +
            "   data[index] = input_0[index] * input_1[index] * factor;\n" +
            "}";

        private readonly int _factorUniformLocation;
        
        internal FragmentGenerationComputeShader(GL gl)
            : base(gl, (ShaderCode, ShaderType.ComputeShader))
        {
            _factorUniformLocation = Gl.GetUniformLocation(ProgrammeID, "factor");
        }

        internal void LoadFactor(uint factor)
        {
            Gl.Uniform1(_factorUniformLocation, factor);
        }
        
    }
}