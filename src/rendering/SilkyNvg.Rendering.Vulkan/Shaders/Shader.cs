using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using System;
using System.IO;

namespace SilkyNvg.Rendering.Vulkan.Shaders
{
    internal class Shader : IDisposable
    {

        private readonly VulkanRenderer _renderer;
        private readonly string _name;

        private readonly ShaderModule _vertexShaderModule;
        private readonly ShaderModule _fragmentShaderModule;

        private PipelineLayout _layout;

        public PipelineShaderStageCreateInfo VertexShaderStage { get; }

        public PipelineShaderStageCreateInfo FragmentShaderStage { get; }

        public PipelineLayout Layout => _layout;

        public bool Status { get; private set; }

        public Shader(string name, string vertexFile, string fragmentFile)
        {
            Status = true;

            _renderer = VulkanRenderer.Instance;
            _name = name;

            _vertexShaderModule = LoadShader(vertexFile);
            _fragmentShaderModule = LoadShader(fragmentFile);

            VertexShaderStage = ShaderStageCreateInfo(_vertexShaderModule, ShaderStageFlags.ShaderStageVertexBit);
            FragmentShaderStage = ShaderStageCreateInfo(_fragmentShaderModule, ShaderStageFlags.ShaderStageFragmentBit);

            if (Status == false)
            {
                return;
            }
        }

        private unsafe ShaderModule LoadShader(string file)
        {
            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            byte[] data = File.ReadAllBytes("Shaders/nanovg_" + file + ".spv");

            ShaderModuleCreateInfo shaderModuleCreateInfo = new()
            {
                SType = StructureType.ShaderModuleCreateInfo,

                CodeSize = (nuint)(data.Length * sizeof(byte))
            };
            fixed (byte* ptr = &data[0])
            {
                shaderModuleCreateInfo.PCode = (uint*)ptr;
            }

            if (vk.CreateShaderModule(device, shaderModuleCreateInfo, allocator, out ShaderModule module) != Result.Success)
            {
                Status = false;
            }
            return module;
        }

        public unsafe void CreatePipelineLayout()
        {
            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            PipelineLayoutCreateInfo pipelineLayoutCreateInfo = new()
            {
                SType = StructureType.PipelineLayoutCreateInfo,

                SetLayoutCount = 0,
                PSetLayouts = null,
                PushConstantRangeCount = 0,
                PPushConstantRanges = null
            };
            _renderer.AssertVulkan(vk.CreatePipelineLayout(device, pipelineLayoutCreateInfo, allocator, out _layout));
        }

        public unsafe void Dispose()
        {
            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            vk.DestroyPipelineLayout(device, _layout, allocator);

            vk.DestroyShaderModule(device, _vertexShaderModule, allocator);
            vk.DestroyShaderModule(device, _fragmentShaderModule, allocator);
        }

        private static unsafe PipelineShaderStageCreateInfo ShaderStageCreateInfo(ShaderModule module, ShaderStageFlags stage)
        {
            return new PipelineShaderStageCreateInfo()
            {
                SType = StructureType.PipelineShaderStageCreateInfo,

                Module = module,
                PName = (byte*)SilkMarshal.StringToPtr("main"),
                PSpecializationInfo = null,
                Stage = stage
            };
        }

    }
}
