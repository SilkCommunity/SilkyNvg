using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using System;
using System.Diagnostics;

namespace Vulkan_Example
{
    public struct VulkanDevice : IDisposable
    {

        private readonly PhysicalDeviceProperties _gpuProperties;
        private readonly PhysicalDeviceMemoryProperties _memoryProperties;

        private readonly QueueFamilyProperties[] _queueFamilyProperties;

        private readonly Device _device;

        private readonly CommandPool _commandPool;

        private readonly Vk _vk;

        public PhysicalDevice Gpu { get; }

        public PhysicalDeviceProperties GpuProperties => _gpuProperties;

        public PhysicalDeviceMemoryProperties MemoryProperties => _memoryProperties;

        public QueueFamilyProperties[] QueueFamilyProperties => _queueFamilyProperties;

        public uint GraphicsQueueFamilyIndex { get; }

        public Device Device => _device;

        public CommandPool CommandPool => _commandPool;

        public unsafe VulkanDevice(PhysicalDevice gpu, Vk vk)
        {
            _vk = vk;

            Gpu = gpu;
            _vk.GetPhysicalDeviceMemoryProperties(Gpu, out _memoryProperties);
            _vk.GetPhysicalDeviceProperties(gpu, out _gpuProperties);

            uint queueFamilyPropertiesCount = 0;
            _vk.GetPhysicalDeviceQueueFamilyProperties(Gpu, ref queueFamilyPropertiesCount, null);
            Debug.Assert(queueFamilyPropertiesCount >= 1);

            _queueFamilyProperties = new QueueFamilyProperties[queueFamilyPropertiesCount];

            fixed (QueueFamilyProperties* ptr = _queueFamilyProperties)
            {
                _vk.GetPhysicalDeviceQueueFamilyProperties(Gpu, ref queueFamilyPropertiesCount, ptr);
                Debug.Assert(queueFamilyPropertiesCount >= 1);
            }

            GraphicsQueueFamilyIndex = uint.MaxValue;
            for (uint i = 0; i < _queueFamilyProperties.Length; i++)
            {
                if (_queueFamilyProperties[i].QueueFlags.HasFlag(QueueFlags.QueueGraphicsBit))
                {
                    GraphicsQueueFamilyIndex = i;
                }
            }

            float* queuePriorities = stackalloc float[1] { 0.0f };
            DeviceQueueCreateInfo queueInfo = new()
            {
                SType = StructureType.DeviceQueueCreateInfo,

                QueueCount = 1,
                PQueuePriorities = queuePriorities,
                QueueFamilyIndex = GraphicsQueueFamilyIndex
            };

            byte** deviceExtensions = stackalloc byte*[]
            {
                (byte*)SilkMarshal.StringToPtr(KhrSwapchain.ExtensionName)
            };
            uint deviceExtensionsCount = 1;
            DeviceCreateInfo deviceInfo = new()
            {
                SType = StructureType.DeviceCreateInfo,

                QueueCreateInfoCount = 1,
                PQueueCreateInfos = &queueInfo,
                EnabledExtensionCount = deviceExtensionsCount,
                PpEnabledExtensionNames = deviceExtensions
            };
            Result res = _vk.CreateDevice(Gpu, deviceInfo, null, out _device);

            Debug.Assert(res == Result.Success);

            CommandPoolCreateInfo cmdPoolInfo = new()
            {
                SType = StructureType.CommandPoolCreateInfo,
                Flags = CommandPoolCreateFlags.CommandPoolCreateResetCommandBufferBit,

                QueueFamilyIndex = GraphicsQueueFamilyIndex
            };
            res = _vk.CreateCommandPool(_device, cmdPoolInfo, null, out _commandPool);
            Debug.Assert(res == Result.Success);
        }

        public unsafe void Dispose()
        {
            if (_commandPool.Handle != 0)
            {
                _vk.DestroyCommandPool(_device, _commandPool, null);
            }

            if (_device.Handle != 0)
            {
                _vk.DestroyDevice(_device, null);
            }
        }

    }
}
