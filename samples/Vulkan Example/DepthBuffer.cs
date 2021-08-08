using Silk.NET.Vulkan;
using System;
using System.Diagnostics;

namespace Vulkan_Example
{
    public struct DepthBuffer
    {

        private readonly Image _image;
        private readonly DeviceMemory _mem;
        private readonly ImageView _view;

        private readonly Vk _vk;

        public Format Format { get; }

        public Image Image => _image;

        public DeviceMemory Mem => _mem;

        public ImageView View => _view;

        public unsafe DepthBuffer(VulkanDevice device, int width, int height, Vk vk)
        {
            _vk = vk;

            Format = Format.D24UnormS8Uint;

            Format[] depthFormats = new Format[] { Format.D32SfloatS8Uint, Format.D24UnormS8Uint, Format.D16UnormS8Uint };
            ImageTiling imageTiling = default;
            for (uint i = 0; i < depthFormats.Length; i++)
            {
                _vk.GetPhysicalDeviceFormatProperties(device.Gpu, depthFormats[i], out FormatProperties fprops);

                if (fprops.LinearTilingFeatures.HasFlag(FormatFeatureFlags.FormatFeatureDepthStencilAttachmentBit))
                {
                    Format = depthFormats[i];
                    imageTiling = ImageTiling.Linear;
                    break;
                }
                else if (fprops.OptimalTilingFeatures.HasFlag(FormatFeatureFlags.FormatFeatureDepthStencilAttachmentBit))
                {
                    Format = depthFormats[i];
                    imageTiling = ImageTiling.Optimal;
                    break;
                }

                if (i == depthFormats.Length - 1)
                {
                    Console.Error.WriteLine("Failed to find supported depth format!");
                    Environment.Exit(-1);
                }
            }

            Format depthFormat = Format;

            ImageCreateInfo imageInfo = new()
            {
                SType = StructureType.ImageCreateInfo,

                ImageType = ImageType.ImageType2D,
                Format = depthFormat,
                Tiling = imageTiling,
                Extent = new((uint)width, (uint)height, 1),
                MipLevels = 1,
                ArrayLayers = 1,
                Samples = SampleCountFlags.SampleCount1Bit,
                InitialLayout = ImageLayout.Undefined,
                QueueFamilyIndexCount = 0,
                PQueueFamilyIndices = null,
                SharingMode = SharingMode.Exclusive,
                Usage = ImageUsageFlags.ImageUsageDepthStencilAttachmentBit
            };

            MemoryAllocateInfo memAlloc = new(StructureType.MemoryAllocateInfo);

            ImageViewCreateInfo viewInfo = new()
            {
                SType = StructureType.ImageViewCreateInfo,

                Format = depthFormat,
                Components = new(ComponentSwizzle.R, ComponentSwizzle.G, ComponentSwizzle.B, ComponentSwizzle.A),
                SubresourceRange = new()
                {
                    AspectMask = ImageAspectFlags.ImageAspectDepthBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                },
                ViewType = ImageViewType.ImageViewType2D
            };

            if (depthFormat == Format.D16UnormS8Uint || depthFormat == Format.D24UnormS8Uint ||
                depthFormat == Format.D32SfloatS8Uint)
            {
                viewInfo.SubresourceRange.AspectMask |= ImageAspectFlags.ImageAspectStencilBit;
            }

            Result res = _vk.CreateImage(device.Device, imageInfo, null, out _image);
            Debug.Assert(res == Result.Success);

            _vk.GetImageMemoryRequirements(device.Device, _image, out MemoryRequirements memReqs);

            memAlloc.AllocationSize = memReqs.Size;

            bool pass = MemoryTypeFromProperties(device.MemoryProperties, memReqs.MemoryTypeBits, MemoryPropertyFlags.MemoryPropertyDeviceLocalBit, out memAlloc.MemoryTypeIndex);
            Debug.Assert(pass);

            res = _vk.AllocateMemory(device.Device, memAlloc, null, out _mem);
            Debug.Assert(res == Result.Success);

            res = _vk.BindImageMemory(device.Device, _image, _mem, 0);
            Debug.Assert(res == Result.Success);

            viewInfo.Image = _image;
            res = _vk.CreateImageView(device.Device, viewInfo, null, out _view);
            Debug.Assert(res == Result.Success);
        }

        private static bool MemoryTypeFromProperties(PhysicalDeviceMemoryProperties memoryProps, uint typeBits, MemoryPropertyFlags requirementsMask, out uint typeIndex)
        {
            for (uint i = 0; i < memoryProps.MemoryTypeCount; i++)
            {
                if ((typeBits & i) == 1)
                {
                    if ((memoryProps.MemoryTypes[(int)i].PropertyFlags.HasFlag(requirementsMask)))
                    {
                        typeIndex = i;
                        return true;
                    }
                }
                typeBits >>= 1;
            }

            typeIndex = 0;
            return false;
        }

    }
}
