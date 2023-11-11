using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering.Vulkan.Shaders
{
    internal class Shader
    {

        private readonly byte[] _vertexShaderModule, _fragmentShaderModule;

        private readonly string _name;
        private readonly VeldridRenderer _renderer;


        private ulong _align;

        public byte[] VertexShaderStage => _vertexShaderModule;

        public byte[] FragmentShaderStage => _fragmentShaderModule;

        public ulong FragSize { get; private set; }
    

        public bool Status { get; private set; } = true;

        public Shader(string name, bool edgeAA, VeldridRenderer renderer)
        {
            _renderer = renderer;
            _name = name;


            string isAAString = edgeAA ? "AA" : "NonAA";

            var vertexCompilationResult = Veldrid.SPIRV.SpirvCompilation.CompileGlslToSpirv(ShaderCode.VertexShaderCode, "NVGVertexShader", Veldrid.ShaderStages.Vertex, new Veldrid.SPIRV.GlslCompileOptions());
            _vertexShaderModule = vertexCompilationResult.SpirvBytes;


            var fragmentCompilationResult = Veldrid.SPIRV.SpirvCompilation.CompileGlslToSpirv(ShaderCode.FragmentShaderCode, $"NVGFragmentShader {isAAString}", Veldrid.ShaderStages.Fragment, new Veldrid.SPIRV.GlslCompileOptions());
            _fragmentShaderModule = fragmentCompilationResult.SpirvBytes;
        }

        public void InitializeFragUniformBuffers()
        {
            _align = _renderer.Device.UniformBufferMinOffsetAlignment;

            FragSize = ((ulong)Marshal.SizeOf(typeof(FragUniforms))) + _align - (((ulong)Marshal.SizeOf(typeof(FragUniforms))) % _align);
        
        }

    }
}