using Silk.NET.Vulkan;
using System;

namespace Vulkan_Example
{
    public class DepthImage : IDisposable
    {
        private readonly uint _width, _height;

        private readonly Image _image;
        private readonly DeviceMemory _memory;
        private readonly ImageView _imageView;

        private readonly PhysicalDevice _physicalDevice;
        private readonly Device _device;
        private readonly Vk _vk;

        public Format Format { get; }

        public ImageView Handle => _imageView;

        public unsafe DepthImage(uint width, uint height, PhysicalDevice physicalDevice, Device device)
        {
            _width = width;
            _height = height;
            _vk = VkUtil.Vk;
            _physicalDevice = physicalDevice;
            _device = device;

            Format = FindFormat(FormatFeatureFlags.FormatFeatureDepthStencilAttachmentBit, Format.D32SfloatS8Uint, Format.D24UnormS8Uint);

            ImageCreateInfo imageCreateInfo = VkInit.ImageCreateInfo(ImageType.ImageType2D, Format, _width, _height);
            VkUtil.AssertVulkan(_vk.CreateImage(_device, imageCreateInfo, null, out _image));

            _vk.GetImageMemoryRequirements(_device, _image, out MemoryRequirements memReqs);
            _vk.GetPhysicalDeviceMemoryProperties(_physicalDevice, out PhysicalDeviceMemoryProperties memoryProperties);
            uint index = VkUtil.FindMemoryTypeIndex(memReqs.MemoryTypeBits, MemoryPropertyFlags.MemoryPropertyDeviceLocalBit, memoryProperties);

            MemoryAllocateInfo memoryAllocateInfo = VkInit.MemoryAllocateInfo(memReqs.Size, index);
            VkUtil.AssertVulkan(_vk.AllocateMemory(_device, memoryAllocateInfo, null, out _memory));
            VkUtil.AssertVulkan(_vk.BindImageMemory(_device, _image, _memory, 0));

            ImageViewCreateInfo imageViewCreateInfo = VkInit.ImageViewCreateInfo(Format, _image, ImageAspectFlags.ImageAspectDepthBit);
            VkUtil.AssertVulkan(_vk.CreateImageView(_device, imageViewCreateInfo, null, out _imageView));
        }

        private Format FindFormat(FormatFeatureFlags features, params Format[] candidates)
        {
            foreach (Format format in candidates)
            {
                _vk.GetPhysicalDeviceFormatProperties(_physicalDevice, format, out FormatProperties properties);
                if (properties.OptimalTilingFeatures.HasFlag(features))
                {
                    return format;
                }
            }

            throw new Exception("No suitable format found.");
        }

        public unsafe void Dispose()
        {
            _vk.DestroyImageView(_device, _imageView, null);
            _vk.FreeMemory(_device, _memory, null);
            _vk.DestroyImage(_device, _image, null);
        }

    }
}
