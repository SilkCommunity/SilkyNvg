using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering.Vulkan.Shaders
{
    internal class Shader : IDisposable
    {

        private readonly ShaderModule _vertexShaderModule, _fragmentShaderModule;

        private readonly string _name;
        private readonly VulkanRenderer _renderer;

        private DescriptorSetLayout _descriptorSetLayout;
        private PipelineLayout _pipelineLayout;

        private ulong _align;

        public DescriptorSetLayout DescriptorSetLayout => _descriptorSetLayout;

        public PipelineLayout PipelineLayout => _pipelineLayout;

        public PipelineShaderStageCreateInfo VertexShaderStage => ShaderStageCreateInfo(_vertexShaderModule, ShaderStageFlags.ShaderStageVertexBit);

        public PipelineShaderStageCreateInfo FragmentShaderStage => ShaderStageCreateInfo(_fragmentShaderModule, ShaderStageFlags.ShaderStageFragmentBit);

        public ulong FragSize { get; private set; }

        public UniformManager UniformManager { get; private set; }

        public bool Status { get; private set; } = true;

        public Shader(string name, string vertexFile, string fragmentFile, VulkanRenderer renderer)
        {
            _renderer = renderer;
            _name = name;

            _vertexShaderModule = CreateShader(vertexFile);
            _fragmentShaderModule = CreateShader(fragmentFile);
        }

        private unsafe ShaderModule CreateShader(string file)
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

            Result result = vk.CreateShaderModule(device, shaderModuleCreateInfo, allocator, out ShaderModule module);
            if (result != Result.Success)
            {
                Status = false;
                _renderer.AssertVulkan(result);
            }
            return module;
        }

        public unsafe void CreateLayout()
        {
            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            DescriptorSetLayoutBinding* descriptorSetLayoutBindings = stackalloc DescriptorSetLayoutBinding[]
            {
                new DescriptorSetLayoutBinding() // vertex binding
                {
                    Binding = 0,
                    DescriptorCount = 1,
                    DescriptorType = DescriptorType.UniformBuffer,
                    PImmutableSamplers = null,
                    StageFlags = ShaderStageFlags.ShaderStageVertexBit
                },
                new DescriptorSetLayoutBinding() // fragment binding
                {
                    Binding = 1,
                    DescriptorCount = 1,
                    DescriptorType = DescriptorType.UniformBuffer,
                    PImmutableSamplers = null,
                    StageFlags = ShaderStageFlags.ShaderStageFragmentBit
                },
                new DescriptorSetLayoutBinding() // image sampler(2D)
                {
                    Binding = 2,
                    DescriptorCount = 1,
                    DescriptorType = DescriptorType.CombinedImageSampler,
                    PImmutableSamplers = null,
                    StageFlags = ShaderStageFlags.ShaderStageFragmentBit
                }
            };

            DescriptorSetLayoutCreateInfo descriptorSetLayoutCreateInfo = new()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,

                BindingCount = 3, // FIXME: 3 - vertex, fragment and image
                PBindings = descriptorSetLayoutBindings
            };

            _renderer.AssertVulkan(vk.CreateDescriptorSetLayout(device, descriptorSetLayoutCreateInfo, allocator, out _descriptorSetLayout));

            PipelineLayoutCreateInfo pipelineLayoutCreateInfo = new()
            {
                SType = StructureType.PipelineLayoutCreateInfo,

                PushConstantRangeCount = 0,
                PPushConstantRanges = null,
                SetLayoutCount = 1,
            };
            fixed (DescriptorSetLayout* ptr = &_descriptorSetLayout)
            {
                pipelineLayoutCreateInfo.PSetLayouts = ptr;
            }

            _renderer.AssertVulkan(vk.CreatePipelineLayout(device, pipelineLayoutCreateInfo, allocator, out _pipelineLayout));
        }

        public void InitializeFragUniformBuffers()
        {
            PhysicalDevice physicalDevice = _renderer.Params.PhysicalDevice;
            Vk vk = _renderer.Vk;

            vk.GetPhysicalDeviceProperties(physicalDevice, out PhysicalDeviceProperties properties);
            _align = properties.Limits.MinUniformBufferOffsetAlignment;

            FragSize = ((ulong)Marshal.SizeOf(typeof(FragUniforms))) + _align - (((ulong)Marshal.SizeOf(typeof(FragUniforms))) % _align);

            UniformManager = new UniformManager(FragSize);
        }

        public unsafe void SetUniforms(Frame frame, DescriptorSet descriptorSet, ulong uniformOffset, int image)
        {
            DescriptorBufferInfo vertexUniformBufferInfo = frame.VertexUniformBuffer.BufferInfo;
            vertexUniformBufferInfo.Offset = 0;
            DescriptorBufferInfo fragmentUniformBufferInfo = frame.FragmentUniformBuffer.BufferInfo;
            fragmentUniformBufferInfo.Offset = uniformOffset;
            fragmentUniformBufferInfo.Range = (uint)Marshal.SizeOf<FragUniforms>();
            DescriptorImageInfo fragmentImageInfo = Textures.Texture.FindTexture(image).ImageInfo;

            Span<WriteDescriptorSet> descriptorWrites = stackalloc WriteDescriptorSet[]
            {
                new WriteDescriptorSet() // Vertex Uniform Buffer
                {
                    SType = StructureType.WriteDescriptorSet,

                    DstBinding = 0,
                    DstArrayElement = 0,
                    DstSet = descriptorSet,

                    DescriptorCount = 1,
                    DescriptorType = DescriptorType.UniformBuffer,
                    PBufferInfo = &vertexUniformBufferInfo
                },
                new WriteDescriptorSet() // Fragment Uniform Buffer
                {
                    SType = StructureType.WriteDescriptorSet,

                    DstBinding = 1,
                    DstArrayElement = 0,
                    DstSet = descriptorSet,

                    DescriptorCount = 1,
                    DescriptorType = DescriptorType.UniformBuffer,
                    PBufferInfo = &fragmentUniformBufferInfo
                },
                new WriteDescriptorSet() // Fragment Image Sampler(2D)
                {
                    SType = StructureType.WriteDescriptorSet,

                    DstBinding = 2,
                    DstArrayElement = 0,
                    DstSet = descriptorSet,

                    DescriptorCount = 1,
                    DescriptorType = DescriptorType.CombinedImageSampler,
                    PImageInfo = &fragmentImageInfo
                }
            };

            Device device = _renderer.Params.Device;
            Vk vk = _renderer.Vk;

            vk.UpdateDescriptorSets(device, (uint)descriptorWrites.Length, in descriptorWrites[0], 0, null);
        }

        public unsafe void Dispose()
        {
            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            vk.DestroyShaderModule(device, _vertexShaderModule, allocator);
            vk.DestroyShaderModule(device, _fragmentShaderModule, allocator);

            vk.DestroyDescriptorSetLayout(device, _descriptorSetLayout, allocator);
            vk.DestroyPipelineLayout(device, _pipelineLayout, allocator);
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
