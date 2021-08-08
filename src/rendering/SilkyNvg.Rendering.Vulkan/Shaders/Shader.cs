using Silk.NET.Maths;
using Silk.NET.Vulkan;
using SilkyNvg.Rendering.Vulkan.Utils;
using System;
using System.IO;

namespace SilkyNvg.Rendering.Vulkan.Shaders
{
    internal class Shader : IDisposable
    {

        private readonly int _align;
        private readonly int _fragSize;

        private readonly VulkanRenderer _renderer;

        private Buffer<Vector2D<float>> _vertUniformBuffer;
        private Buffer<FragUniforms> _fragUniformBuffer;

        private DescriptorPool _descriptorPool;
        private uint _descriptorPoolCapacity;

        public DescriptorPool DescPool => _descriptorPool;

        public DescriptorSetLayout DescLayout { get; }

        public ShaderModule VertShader { get; }

        public ShaderModule FragShader { get; }

        public ShaderModule FragShaderAA { get; }

        public UniformManager UniformManager { get; }

        public unsafe Shader(VulkanRenderer renderer)
        {
            _renderer = renderer;

            VertShader = CreateShaderModule("fill.vert");
            FragShader = CreateShaderModule("fill.frag");
            FragShaderAA = CreateShaderModule("fill_edge_aa.frag");

            _align = (int)_renderer.GpuProperties.Limits.MinUniformBufferOffsetAlignment;

            _fragSize = sizeof(FragUniforms) + _align - (sizeof(FragUniforms) % _align);

            UniformManager = new UniformManager(_fragSize);
            _vertUniformBuffer = default;
            _fragUniformBuffer = default;
            _descriptorPool = default;
            _descriptorPoolCapacity = 0;

            DescLayout = CreateDescriptorSetLayout();
        }

        private unsafe ShaderModule CreateShaderModule(string fileName)
        {
            byte[] code = File.ReadAllBytes("./Shaders/" + fileName + ".spv");
            ShaderModuleCreateInfo moduleCreateInfo = new()
            {
                SType = StructureType.ShaderModuleCreateInfo,

                CodeSize = (nuint)code.Length
            };
            fixed (byte* ptr = code)
            {
                moduleCreateInfo.PCode = (uint*)ptr;
            }

            _renderer.Assert(_renderer.Vk.CreateShaderModule(_renderer.Params.device, moduleCreateInfo, _renderer.Params.allocator, out ShaderModule module));
            return module;
        }

        private unsafe DescriptorSetLayout CreateDescriptorSetLayout()
        {
            DescriptorSetLayoutBinding* layoutBindings = stackalloc DescriptorSetLayoutBinding[3]
            {
                new DescriptorSetLayoutBinding(0, DescriptorType.UniformBuffer, 1, ShaderStageFlags.ShaderStageVertexBit, null),
                new DescriptorSetLayoutBinding(1, DescriptorType.UniformBuffer, 1, ShaderStageFlags.ShaderStageFragmentBit, null),
                new DescriptorSetLayoutBinding(2, DescriptorType.CombinedImageSampler, 1, ShaderStageFlags.ShaderStageFragmentBit, null)
            };

            DescriptorSetLayoutCreateInfo descriptorLayout = new(StructureType.DescriptorSetLayoutCreateInfo, null, 0, 3, layoutBindings);

            _renderer.Assert(_renderer.Vk.CreateDescriptorSetLayout(_renderer.Params.device, descriptorLayout, _renderer.Params.allocator, out DescriptorSetLayout descLayout));
            return descLayout;
        }

        public unsafe void UpdateBuffers(Vector2D<float> view)
        {
            Buffer<Vector2D<float>>.UpdateBuffer(ref _vertUniformBuffer, BufferUsageFlags.BufferUsageUniformBufferBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit, new(&view, 1), _renderer);
            Buffer<FragUniforms>.UpdateBuffer(ref _fragUniformBuffer, BufferUsageFlags.BufferUsageUniformBufferBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit, UniformManager.Uniforms, _renderer);
        }

        private unsafe DescriptorPool CreateDescriptorPool(Device device, uint count, AllocationCallbacks* allocator)
        {
            DescriptorPoolSize* typeCount = stackalloc DescriptorPoolSize[3]
            {
                new DescriptorPoolSize(DescriptorType.InputAttachment, 2 * count),
                new DescriptorPoolSize(DescriptorType.UniformBuffer, 4 * count),
                new DescriptorPoolSize(DescriptorType.CombinedImageSampler, 2 * count)
            };

            DescriptorPoolCreateInfo descriptorPool = new(StructureType.DescriptorPoolCreateInfo, null, 0, count * 2, 3, typeCount);
            _renderer.Assert(_renderer.Vk.CreateDescriptorPool(device, descriptorPool, allocator, out DescriptorPool descPool));
            return descPool;
        }

        public unsafe void ValidateDescriptorPool(uint count)
        {
            if (count > _descriptorPoolCapacity)
            {
                if (_descriptorPool.Handle != 0)
                {
                    _renderer.Vk.DestroyDescriptorPool(_renderer.Params.device, _descriptorPool, _renderer.Params.allocator);
                }
                _descriptorPool = CreateDescriptorPool(_renderer.Params.device, count, _renderer.Params.allocator);
                _descriptorPoolCapacity = count;
            }
            else
            {
                _renderer.Vk.ResetDescriptorPool(_renderer.Params.device, _descriptorPool, 0);
            }
        }

        public unsafe void SetUniforms(DescriptorSet descSet, int uniformOffset, int image)
        {
            Device device = _renderer.Params.device;

            WriteDescriptorSet* writes = stackalloc WriteDescriptorSet[3];

            DescriptorBufferInfo vertUniformBufferInfo = new()
            {
                Buffer = _vertUniformBuffer.Handle,
                Offset = 0,
                Range = (ulong)sizeof(Vector2D<float>) // view size is Vector2D
            };

            writes[0] = new(StructureType.WriteDescriptorSet)
            {
                DstSet = descSet,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.UniformBuffer,
                PBufferInfo = &vertUniformBufferInfo,
                DstArrayElement = 0,
                DstBinding = 0
            };

            DescriptorBufferInfo uniformBufferInfo = new()
            {
                Buffer = _fragUniformBuffer.Handle,
                Offset = (ulong)uniformOffset,
                Range = (ulong)sizeof(FragUniforms)
            };

            writes[1] = new(StructureType.WriteDescriptorSet)
            {
                DstSet = descSet,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.UniformBuffer,
                PBufferInfo = &uniformBufferInfo,
                DstBinding = 1
            };

            Textures.Texture tex;
            if (image != 0)
            {
                tex = Textures.Texture.FindTexture(image);
            }
            else
            {
                tex = Textures.Texture.FindTexture(1);
            }

            DescriptorImageInfo imageInfo = new()
            {
                ImageLayout = tex.ImageLayout,
                ImageView = tex.View,
                Sampler = tex.Sampler
            };

            writes[2] = new(StructureType.WriteDescriptorSet)
            {
                DstSet = descSet,
                DstBinding = 2,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.CombinedImageSampler,
                PImageInfo = &imageInfo
            };

            _renderer.Vk.UpdateDescriptorSets(device, 3, writes, null);
        }

        public unsafe void Dispose()
        {
            _vertUniformBuffer.Dispose();
            _fragUniformBuffer.Dispose();

            _renderer.Vk.DestroyShaderModule(_renderer.Params.device, VertShader, _renderer.Params.allocator);
            _renderer.Vk.DestroyShaderModule(_renderer.Params.device, FragShader, _renderer.Params.allocator);
            _renderer.Vk.DestroyShaderModule(_renderer.Params.device, FragShaderAA, _renderer.Params.allocator);

            _renderer.Vk.DestroyDescriptorPool(_renderer.Params.device, _descriptorPool, _renderer.Params.allocator);
            _renderer.Vk.DestroyDescriptorSetLayout(_renderer.Params.device, DescLayout, _renderer.Params.allocator);
        }

    }
}
