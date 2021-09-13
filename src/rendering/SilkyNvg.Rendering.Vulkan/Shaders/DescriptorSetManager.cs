using Silk.NET.Vulkan;
using System;

namespace SilkyNvg.Rendering.Vulkan.Shaders
{
    internal class DescriptorSetManager : IDisposable
    {

        private readonly DescriptorSetLayout _layout;
        private readonly VulkanRenderer _renderer;

        private DescriptorSet[] _descriptorSets;
        private int _count;
        private int _capacity;

        private DescriptorPool _pool;

        public DescriptorSetManager(VulkanRenderer renderer)
        {
            _layout = renderer.Shader.DescriptorSetLayout;
            _renderer = renderer;
            _descriptorSets = Array.Empty<DescriptorSet>();
            _count = 0;
            _capacity = 0;
        }

        private unsafe void CreateDescriptorPool(uint maxSets)
        {
            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            DescriptorPoolSize* descriptorPoolSizes = stackalloc DescriptorPoolSize[]
            {
                new DescriptorPoolSize()
                {
                    DescriptorCount = 2, // vertex and fragment shader
                    Type = DescriptorType.UniformBuffer
                },
                new DescriptorPoolSize()
                {
                    DescriptorCount = 1, // Image
                    Type = DescriptorType.CombinedImageSampler
                }
            };

            DescriptorPoolCreateInfo descriptorPoolCreateInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,

                MaxSets = maxSets,

                PoolSizeCount = 2,
                PPoolSizes = descriptorPoolSizes
            };

            _renderer.AssertVulkan(vk.CreateDescriptorPool(device, descriptorPoolCreateInfo, allocator, out _pool));
        }

        private unsafe void AllocDescriptorSets(uint n)
        {
            if (_count + n > _capacity)
            {
                Device device = _renderer.Params.Device;
                AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
                Vk vk = _renderer.Vk;

                int cdescriptorSets = Math.Max(_count + (int)n, 16) + (_descriptorSets.Length / 2);
                Array.Resize(ref _descriptorSets, cdescriptorSets);
                _capacity = cdescriptorSets;

                vk.DestroyDescriptorPool(device, _pool, allocator);
                CreateDescriptorPool((uint)_capacity);

                DescriptorSetAllocateInfo descriptorSetAllocateInfo = new()
                {
                    SType = StructureType.DescriptorSetAllocateInfo,

                    DescriptorPool = _pool,
                    DescriptorSetCount = 1
                };
                fixed (DescriptorSetLayout* ptr = &_layout)
                {
                    descriptorSetAllocateInfo.PSetLayouts = ptr;
                }

                for (int i = 0; i < _capacity; i++)
                {
                    _renderer.AssertVulkan(vk.AllocateDescriptorSets(device, descriptorSetAllocateInfo, out _descriptorSets[i]));
                }
            }
        }

        public void Reset(uint requireredDescriptorSetCount)
        {
            if (_capacity < requireredDescriptorSetCount)
            {
                AllocDescriptorSets(requireredDescriptorSetCount);
            }

            _count = 0;
        }

        public DescriptorSet GetDescriptorSet()
        {
            return _descriptorSets[_count++];
        }

        public unsafe void Dispose()
        {
            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            vk.DestroyDescriptorPool(device, _pool, allocator);
        }

    }
}
