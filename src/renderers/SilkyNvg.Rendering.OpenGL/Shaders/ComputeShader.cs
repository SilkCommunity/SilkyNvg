using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Utils;

namespace SilkyNvg.Rendering.OpenGL.Shaders;

internal sealed class ComputeShader : ShaderProgramme
{

    private readonly uint _workGroupSize;
    
    internal ComputeShader(uint workGroupSize, string? header, string shaderSource, GL gl)
        : base(gl, (header, shaderSource, ShaderType.ComputeShader))
    {
        _workGroupSize = workGroupSize;
    }

    internal void Dispatch(uint numberOfKernels)
    {
        uint numberOfWorkGroups = Utils.Utils.DivUp(numberOfKernels, _workGroupSize);
        Gl.DispatchCompute(numberOfWorkGroups, 1, 1);
        Errors.CheckGLError("dispatch compute shader", Gl);
    }
    
}