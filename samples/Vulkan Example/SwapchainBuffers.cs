using Silk.NET.Vulkan;
using System;
using System.Diagnostics;

namespace Vulkan_Example
{
    public struct SwapchainBuffers : IDisposable
    {

        private readonly ImageView _view;

        private readonly Vk _vk;

        public Image Image { get; }

        public ImageView View => _view;

        public unsafe SwapchainBuffers(VulkanDevice device, Format format, CommandBuffer cmdBuffer, Image image, Vk vk)
        {
            _vk = vk;

            ImageViewCreateInfo colourAttachmentView = new()
            {
                SType = StructureType.ImageViewCreateInfo,

                Format = format,
                Components = new(ComponentSwizzle.R, ComponentSwizzle.G, ComponentSwizzle.B, ComponentSwizzle.A)
            };

            ImageSubresourceRange subresourceRange = new()
            {
                AspectMask = ImageAspectFlags.ImageAspectColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            };

            colourAttachmentView.SubresourceRange = subresourceRange;
            colourAttachmentView.ViewType = ImageViewType.ImageViewType2D;

            Image = image;

            SetupImageLayout(cmdBuffer, image, ImageAspectFlags.ImageAspectColorBit, ImageLayout.Undefined, ImageLayout.PresentSrcKhr, _vk);

            colourAttachmentView.Image = Image;

            Result res = _vk.CreateImageView(device.Device, colourAttachmentView, null, out _view);
            Debug.Assert(res == Result.Success);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private static unsafe void SetupImageLayout(CommandBuffer cmdBuffer, Image image, ImageAspectFlags aspectMask, ImageLayout oldImageLayout, ImageLayout newImageLayout, Vk vk)
        {
            ImageMemoryBarrier imageMemoryBarrier = new()
            {
                SType = StructureType.ImageMemoryBarrier,

                OldLayout = oldImageLayout,
                NewLayout = newImageLayout,
                Image = image
            };

            ImageSubresourceRange subresourceRange = new(aspectMask, 0, 1, 0, 1);
            imageMemoryBarrier.SubresourceRange = subresourceRange;

            if (newImageLayout == ImageLayout.TransferDstOptimal)
            {
                imageMemoryBarrier.DstAccessMask = AccessFlags.AccessTransferReadBit;
            }

            if (newImageLayout == ImageLayout.ColorAttachmentOptimal)
            {
                imageMemoryBarrier.DstAccessMask = AccessFlags.AccessColorAttachmentWriteBit;
            }

            if (newImageLayout == ImageLayout.DepthStencilAttachmentOptimal)
            {
                imageMemoryBarrier.DstAccessMask = AccessFlags.AccessDepthStencilAttachmentWriteBit;
            }

            if (newImageLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                imageMemoryBarrier.DstAccessMask = AccessFlags.AccessShaderReadBit | AccessFlags.AccessInputAttachmentReadBit;
            }

            ImageMemoryBarrier* pMemoryBarrier = &imageMemoryBarrier;

            PipelineStageFlags srcStages = PipelineStageFlags.PipelineStageTopOfPipeBit;
            PipelineStageFlags destStages = PipelineStageFlags.PipelineStageTopOfPipeBit;

            vk.CmdPipelineBarrier(cmdBuffer, srcStages, destStages, 0, 0, null, 0, null, 1, pMemoryBarrier);
        }

    }
}
