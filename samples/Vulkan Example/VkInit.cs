using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using System;

namespace Vulkan_Example
{
    public static class VkInit
    {

        public static unsafe InstanceCreateInfo InstanceCreateInfo(ApplicationInfo appInfo,
            byte** instanceLayers, uint instanceLayerCount, byte** instanceExtensions, uint instanceExtensionCount)
        {
            return new InstanceCreateInfo()
            {
                SType = StructureType.InstanceCreateInfo,

                PApplicationInfo = &appInfo,
                PpEnabledLayerNames = instanceLayers,
                EnabledLayerCount = instanceLayerCount,
                PpEnabledExtensionNames = instanceExtensions,
                EnabledExtensionCount = instanceExtensionCount
            };
        }

        public static unsafe DeviceQueueCreateInfo QueueCreateInfo(uint queueFamilyIndex, uint count, params float[] queuePriorities)
        {
            if (count != queuePriorities.Length)
            {
                throw new ArgumentException("Must specify a queue priority for each queue to be created!");
            }

            DeviceQueueCreateInfo queueCreateInfo = new()
            {
                SType = StructureType.DeviceQueueCreateInfo,

                QueueCount = count,
                QueueFamilyIndex = queueFamilyIndex
            };

            fixed (float* ptr = &queuePriorities[0])
            {
                queueCreateInfo.PQueuePriorities = ptr;
            }

            return queueCreateInfo;
        }

        public static unsafe DeviceCreateInfo DeviceCreateInfo(string[] deviceExtensions, params DeviceQueueCreateInfo[] queues)
        {
            DeviceCreateInfo deviceCreateInfo = new()
            {
                SType = StructureType.DeviceCreateInfo,

                EnabledLayerCount = 0,
                PpEnabledLayerNames = null,
                EnabledExtensionCount = (uint)deviceExtensions.Length,
                PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(deviceExtensions),

                QueueCreateInfoCount = (uint)queues.Length
            };
            fixed (DeviceQueueCreateInfo* ptr = &queues[0])
            {
                deviceCreateInfo.PQueueCreateInfos = ptr;
            }
            return deviceCreateInfo;
        }

        public static unsafe SwapchainCreateInfoKHR SwapchainCreateInfo(SurfaceKHR surface, uint imageCount, SurfaceFormatKHR format, PresentModeKHR presentMode,
            Extent2D extent, uint graphicsQueueFamily, uint presentQueueFamily, SurfaceCapabilitiesKHR surfaceCapabilities, SwapchainKHR oldSwapchain)
        {
            uint[] queueFamilies = new uint[] { graphicsQueueFamily, presentQueueFamily };

            SwapchainCreateInfoKHR swapchainCreateInfo = new()
            {
                SType = StructureType.SwapchainCreateInfoKhr,

                Surface = surface,

                MinImageCount = imageCount,

                ImageFormat = format.Format,
                ImageColorSpace = format.ColorSpace,

                ImageExtent = extent,

                ImageArrayLayers = 1,
                ImageUsage = ImageUsageFlags.ImageUsageColorAttachmentBit,

                ImageSharingMode = (graphicsQueueFamily == presentQueueFamily) ? SharingMode.Exclusive : SharingMode.Concurrent,
                QueueFamilyIndexCount = (uint)((graphicsQueueFamily == presentQueueFamily) ? 0 : 2),

                PreTransform = surfaceCapabilities.CurrentTransform,

                CompositeAlpha = CompositeAlphaFlagsKHR.CompositeAlphaOpaqueBitKhr,

                PresentMode = presentMode,
                Clipped = true,

                OldSwapchain = oldSwapchain
            };
            fixed (uint* ptr = queueFamilies)
            {
                swapchainCreateInfo.PQueueFamilyIndices = ptr;
            }

            return swapchainCreateInfo;
        }

        public static ImageViewCreateInfo ImageViewCreateInfo(Format format, Image image, ImageAspectFlags aspectFlags, ImageViewType viewType = ImageViewType.ImageViewType2D)
        {
            return new ImageViewCreateInfo()
            {
                SType = StructureType.ImageViewCreateInfo,

                Image = image,

                ViewType = viewType,
                Format = format,

                Components = new ComponentMapping()
                {
                    R = ComponentSwizzle.Identity,
                    G = ComponentSwizzle.Identity,
                    B = ComponentSwizzle.Identity,
                    A = ComponentSwizzle.Identity
                },

                SubresourceRange = new ImageSubresourceRange()
                {
                    AspectMask = aspectFlags,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                    BaseMipLevel = 0,
                    LevelCount = 1
                }
            };
        }

        public static CommandPoolCreateInfo CommandPoolCreateInfo(uint queueFamily, CommandPoolCreateFlags flags)
        {
            return new CommandPoolCreateInfo()
            {
                SType = StructureType.CommandPoolCreateInfo,
                Flags = flags,

                QueueFamilyIndex = queueFamily
            };
        }

        public static CommandBufferAllocateInfo CommandBufferAllocateInfo(CommandPool pool, uint count)
        {
            return new CommandBufferAllocateInfo()
            {
                SType = StructureType.CommandBufferAllocateInfo,

                CommandPool = pool,
                CommandBufferCount = count,
                Level = CommandBufferLevel.Primary
            };
        }

        public static unsafe RenderPassCreateInfo RenderPassCreateInfo(Span<AttachmentDescription> attachments, SubpassDependency dependency, SubpassDescription subpass)
        {
            RenderPassCreateInfo renderPassCreateInfo = new()
            {
                SType = StructureType.RenderPassCreateInfo,

                AttachmentCount = (uint)attachments.Length,

                DependencyCount = 1,
                PDependencies = &dependency,
                SubpassCount = 1,
                PSubpasses = &subpass
            };
            fixed (AttachmentDescription* ptr = &attachments[0])
            {
                renderPassCreateInfo.PAttachments = ptr;
            }

            return renderPassCreateInfo;
        }

        public static FramebufferCreateInfo FramebufferCreateInfo(uint attachmentCount, RenderPass renderPass, Extent2D extent)
        {
            return new FramebufferCreateInfo()
            {
                SType = StructureType.FramebufferCreateInfo,

                AttachmentCount = attachmentCount,
                RenderPass = renderPass,
                Width = extent.Width,
                Height = extent.Height,
                Layers = 1
            };
        }

        public static FenceCreateInfo FenceCreateInfo(bool signaled)
        {
            return new FenceCreateInfo()
            {
                SType = StructureType.FenceCreateInfo,
                Flags = signaled ? FenceCreateFlags.FenceCreateSignaledBit : 0
            };
        }

        public static SemaphoreCreateInfo SemaphoreCreateInfo()
        {
            return new SemaphoreCreateInfo()
            {
                SType = StructureType.SemaphoreCreateInfo
            };
        }

        public static ImageCreateInfo ImageCreateInfo(ImageType type, Format format, uint width, uint height)
        {
            return new ImageCreateInfo()
            {
                SType = StructureType.ImageCreateInfo,

                ImageType = type,
                Format = format,
                Tiling = ImageTiling.Optimal,
                Extent = new Extent3D()
                {
                    Width = width,
                    Height = height,
                    Depth = 1
                },
                MipLevels = 1,
                ArrayLayers = 1,
                Samples = SampleCountFlags.SampleCount1Bit,
                InitialLayout = ImageLayout.Undefined,
                QueueFamilyIndexCount = 0,
                PQueueFamilyIndices = null,
                SharingMode = SharingMode.Exclusive,
                Usage = ImageUsageFlags.ImageUsageDepthStencilAttachmentBit
            };
        }

        public static MemoryAllocateInfo MemoryAllocateInfo(ulong allocationSize, uint index)
        {
            return new MemoryAllocateInfo()
            {
                SType = StructureType.MemoryAllocateInfo,

                AllocationSize = allocationSize,
                MemoryTypeIndex = index
            };
        }

    }
}
