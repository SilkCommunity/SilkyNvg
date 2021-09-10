using Silk.NET.Vulkan;
using System;

namespace Vulkan_Example
{
    public class Swapchain : IDisposable
    {

        private readonly Device _device;
        private readonly SurfaceKHR _surface;

        private readonly SurfaceFormatKHR _surfaceFormat;
        private readonly PresentModeKHR _presentMode;
        private readonly SurfaceCapabilitiesKHR _surfaceCapabilities;
        private readonly uint _imageCount;
        private readonly uint _graphicsQueueFamily, _presentQueueFamily;

        public SwapchainKHR Handle = default;
        public Format Format;
        public Extent2D Extent;
        public Image[] Images;
        public ImageView[] ImageViews;

        public Swapchain((SurfaceCapabilitiesKHR, SurfaceFormatKHR[], PresentModeKHR[]) swapchainData, PresentModeKHR desiredPresentMode, uint desiredImageCount,
            Extent2D windowExtent, uint graphicsQueueFamily, uint presentQueueFamily, SurfaceKHR surface)
        {
            _device = VkUtil.Vk.CurrentDevice.Value;
            _surface = surface;

            _surfaceFormat = ChooseSurfaceFormat(swapchainData.Item2);
            _presentMode = ChoosePresentMode(desiredPresentMode, swapchainData.Item3);
            _surfaceCapabilities = swapchainData.Item1;
            _imageCount = CalculateImageCount(desiredImageCount, _surfaceCapabilities);
            _graphicsQueueFamily = graphicsQueueFamily;
            _presentQueueFamily = presentQueueFamily;

            Extent = GetSwapchainExtent(windowExtent, _surfaceCapabilities);

            CreateSwapchain();
        }

        private unsafe void CreateSwapchain()
        {
            SwapchainCreateInfoKHR swapchainCreateInfo = VkInit.SwapchainCreateInfo(_surface, _imageCount, _surfaceFormat,
                _presentMode, Extent, _graphicsQueueFamily, _presentQueueFamily, _surfaceCapabilities, Handle);

            Format = swapchainCreateInfo.ImageFormat;

            VkUtil.AssertVulkan(VkUtil.KhrSwapchain.CreateSwapchain(_device, swapchainCreateInfo, null, out Handle));

            uint swapchainImageCount = 0;
            VkUtil.AssertVulkan(VkUtil.KhrSwapchain.GetSwapchainImages(_device, Handle, ref swapchainImageCount, null));
            Images = new Image[swapchainImageCount];
            VkUtil.AssertVulkan(VkUtil.KhrSwapchain.GetSwapchainImages(_device, Handle, ref swapchainImageCount, out Images[0]));

            ImageViews = new ImageView[Images.Length];
            for (uint i = 0; i < Images.Length; i++)
            {
                ImageViewCreateInfo imageViewCreateInfo = VkInit.ImageViewCreateInfo(Format, Images[i], ImageAspectFlags.ImageAspectColorBit);
                VkUtil.AssertVulkan(VkUtil.Vk.CreateImageView(_device, imageViewCreateInfo, null, out ImageViews[i]));
            }
        }

        public unsafe void Recreate(Extent2D newExtent)
        {
            foreach (ImageView imageView in ImageViews)
            {
                VkUtil.Vk.DestroyImageView(_device, imageView, null);
            }
            Extent = newExtent;
            SwapchainKHR oldSwapchain = Handle;
            CreateSwapchain();
            VkUtil.KhrSwapchain.DestroySwapchain(_device, oldSwapchain, null);
        }

        public unsafe void Dispose()
        {
            foreach (ImageView imageView in ImageViews)
            {
                VkUtil.Vk.DestroyImageView(_device, imageView, null);
            }
            VkUtil.KhrSwapchain.DestroySwapchain(_device, Handle, null);
        }

        private static Extent2D GetSwapchainExtent(Extent2D windowExtent, SurfaceCapabilitiesKHR surfaceCapabilities)
        {
            static uint Clamp(uint value, uint min, uint max)
            {
                if (value < min)
                {
                    return min;
                }
                if (value > max)
                {
                    return max;
                }
                return value;
            }

            Extent2D swapchainExtent = default;
            swapchainExtent.Width = Clamp(windowExtent.Width, surfaceCapabilities.MinImageExtent.Width, surfaceCapabilities.MaxImageExtent.Width);
            swapchainExtent.Height = Clamp(windowExtent.Height, surfaceCapabilities.MinImageExtent.Height, surfaceCapabilities.MaxImageExtent.Height);

            return swapchainExtent;
        }

        private static unsafe SurfaceFormatKHR ChooseSurfaceFormat(SurfaceFormatKHR[] supportedFormats)
        {
            foreach (SurfaceFormatKHR format in supportedFormats)
            {
                if ((format.Format == Format.B8G8R8A8Unorm) && (format.ColorSpace == ColorSpaceKHR.ColorSpaceSrgbNonlinearKhr))
                {
                    return format;
                }
            }

            return supportedFormats[0];
        }

        private static PresentModeKHR ChoosePresentMode(PresentModeKHR desiredPresentMode, PresentModeKHR[] supportedPresentModes)
        {
            foreach (PresentModeKHR presentMode in supportedPresentModes)
            {
                if (presentMode == desiredPresentMode)
                {
                    return presentMode;
                }
            }

            return PresentModeKHR.PresentModeFifoKhr;
        }

        private static uint CalculateImageCount(uint desiredImageCount, SurfaceCapabilitiesKHR surfaceCapabilities)
        {
            uint imageCount = desiredImageCount;
            imageCount = Math.Max(imageCount, surfaceCapabilities.MinImageCount);
            if (surfaceCapabilities.MaxImageCount != 0)
            {
                imageCount = Math.Min(imageCount, surfaceCapabilities.MaxImageCount);
            }
            return imageCount;
        }

    }
}
