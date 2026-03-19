using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Shaders
{
    internal class FragmentSortingShader : ShaderProgramme
    {

        private readonly int _fragmentCountUniformLocation;
        private readonly int _strideUniformLocation;
        private readonly int _strideTrailingZerosUniformLocation;
        private readonly int _innerReminderUniformLocation;
        private readonly int _innerLastIdxUniformLocation;
        
        internal FragmentSortingShader(GL gl)
            : base(gl, (LoadShaderCode("fragmentSortingShader.comp.glsl"), ShaderType.ComputeShader))
        {
            _fragmentCountUniformLocation = Gl.GetUniformLocation(ProgrammeID, "fragmentCount");
            _strideUniformLocation = Gl.GetUniformLocation(ProgrammeID, "stride");
            _strideTrailingZerosUniformLocation = Gl.GetUniformLocation(ProgrammeID, "strideTrailingZeros");
            _innerReminderUniformLocation = Gl.GetUniformLocation(ProgrammeID, "innerReminder");
            _innerLastIdxUniformLocation = Gl.GetUniformLocation(ProgrammeID, "innerLastIdx");
        }

        internal void LoadFragmentCount(uint fragmentCount)
        {
            Gl.Uniform1(_fragmentCountUniformLocation, fragmentCount);
        }
        
        internal void LoadStride(uint stride)
        {
            Gl.Uniform1(_strideUniformLocation, stride);
        }
        
        internal void LoadStrideTrailingZeros(uint strideTrailingZeros)
        {
            Gl.Uniform1(_strideTrailingZerosUniformLocation, strideTrailingZeros);
        }
        
        internal void LoadInnerReminder(uint innerReminder)
        {
            Gl.Uniform1(_innerReminderUniformLocation, innerReminder);
        }
        
        internal void LoadInnerLastIdx(uint innerLastIdx)
        {
            Gl.Uniform1(_innerLastIdxUniformLocation, innerLastIdx);
        }
        
    }
}