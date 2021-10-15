using Silk.NET.Maths;
using Silk.NET.Vulkan;
using SilkyNvg.Images;
using SilkyNvg.Rendering.Vulkan.Utils;
using System;

namespace SilkyNvg.Rendering.Vulkan.Textures
{
    internal struct Texture : IDisposable
    {

        private static int _idCounter = 0;

        private readonly VulkanRenderer _renderer;

        private Image _image;
        private ImageView _imageView;
        private Sampler _sampler;

        private DeviceMemory _memory;

        private Format _format;
        private uint _mipLevelCount;
        private Rendering.Texture _type;

        private ImageFlags _flags;

        public int Id { get; private set; }

        public Vector2D<uint> Size { get; private set; }

        public Rendering.Texture TextureType { get; private set; }

        public DescriptorImageInfo ImageInfo => new()
        {
            ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
            ImageView = _imageView,
            Sampler = _sampler
        };

        public unsafe Texture(VulkanRenderer renderer)
            : this()
        {
            _renderer = renderer;
        }

        private uint CalculateMipLevelCount()
        {
            uint mipWidth = Size.X;
            uint mipHeight = Size.Y;

            uint levelCount = 0;
            while ((mipWidth > 0) && (mipHeight > 0))
            {
                levelCount++;
                mipWidth /= 2;
                mipHeight /= 2;
            }

            return Math.Max(levelCount, 1);
        }

        private unsafe Image CreateImage(Rendering.Texture type)
        {
            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            ImageCreateInfo imageCreateInfo = new()
            {
                SType = StructureType.ImageCreateInfo,

                ImageType = ImageType.ImageType2D,
                Extent = new Extent3D()
                {
                    Width = Size.X,
                    Height = Size.Y,
                    Depth = 1
                },
                MipLevels = _mipLevelCount,
                ArrayLayers = 1,
                Format = _format,
                Tiling = ImageTiling.Optimal,
                InitialLayout = ImageLayout.Undefined,
                Usage = ImageUsageFlags.ImageUsageTransferDstBit | ImageUsageFlags.ImageUsageTransferSrcBit | ImageUsageFlags.ImageUsageSampledBit,
                SharingMode = SharingMode.Exclusive,
                Samples = SampleCountFlags.SampleCount1Bit,
                QueueFamilyIndexCount = 0,
                PQueueFamilyIndices = null
            };

            _renderer.AssertVulkan(vk.CreateImage(device, imageCreateInfo, allocator, out Image image));
            return image;
        }

        private unsafe DeviceMemory BindImageMemory()
        {
            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            vk.GetImageMemoryRequirements(device, _image, out MemoryRequirements memReqs);

            MemoryAllocateInfo memoryAllocateInfo = new()
            {
                SType = StructureType.MemoryAllocateInfo,

                AllocationSize = memReqs.Size,
                MemoryTypeIndex = _renderer.FindMemoryTypeIndex(memReqs.MemoryTypeBits, MemoryPropertyFlags.MemoryPropertyDeviceLocalBit)
            };

            _renderer.AssertVulkan(vk.AllocateMemory(device, memoryAllocateInfo, allocator, out DeviceMemory memory));
            _renderer.AssertVulkan(vk.BindImageMemory(device, _image, memory, 0));
            return memory;
        }

        private unsafe CommandBuffer BeginImageCommandBuffer()
        {
            Device device = _renderer.Params.Device;
            Vk vk = _renderer.Vk;

            CommandBufferAllocateInfo commandBufferAllocateInfo = new()
            {
                SType = StructureType.CommandBufferAllocateInfo,

                CommandBufferCount = 1,
                CommandPool = _renderer.ImageTransitionPool
            };

            _renderer.AssertVulkan(vk.AllocateCommandBuffers(device, commandBufferAllocateInfo, out CommandBuffer commandBuffer));

            CommandBufferBeginInfo commandBufferBeginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.CommandBufferUsageOneTimeSubmitBit,

                PInheritanceInfo = null
            };

            _renderer.AssertVulkan(vk.BeginCommandBuffer(commandBuffer, commandBufferBeginInfo));

            return commandBuffer;
        }

        private unsafe void EndImageCommandBuffer(CommandBuffer commandBuffer)
        {
            Device device = _renderer.Params.Device;
            Vk vk = _renderer.Vk;

            _renderer.AssertVulkan(vk.EndCommandBuffer(commandBuffer));

            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,

                CommandBufferCount = 1,
                PCommandBuffers = &commandBuffer,

                SignalSemaphoreCount = 0,
                PSignalSemaphores = null,
                WaitSemaphoreCount = 0,
                PWaitSemaphores = null,
                PWaitDstStageMask = null
            };

            _renderer.AssertVulkan(vk.QueueSubmit(_renderer.ImageTransitionQueue, 1, submitInfo, default));
            _renderer.AssertVulkan(vk.QueueWaitIdle(_renderer.ImageTransitionQueue));

            vk.FreeCommandBuffers(device, _renderer.ImageTransitionPool, 1, commandBuffer);
        }

        private unsafe void TransitionImageLayout(ImageLayout oldLayout, ImageLayout newLayout)
        {
            Vk vk = _renderer.Vk;

            CommandBuffer commandBuffer = BeginImageCommandBuffer();

            ImageMemoryBarrier imageMemoryBarrier = new()
            {
                SType = StructureType.ImageMemoryBarrier,

                OldLayout = oldLayout,
                NewLayout = newLayout,

                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,

                Image = _image,

                SubresourceRange = new ImageSubresourceRange()
                {
                    AspectMask = ImageAspectFlags.ImageAspectColorBit,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                    BaseMipLevel = 0,
                    LevelCount = _mipLevelCount
                },

                SrcAccessMask = 0,
                DstAccessMask = 0
            };

            if ((oldLayout == ImageLayout.Undefined) && (newLayout == ImageLayout.TransferDstOptimal))
            {
                imageMemoryBarrier.SrcAccessMask = 0;
                imageMemoryBarrier.DstAccessMask = AccessFlags.AccessTransferWriteBit;

                vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.PipelineStageTopOfPipeBit, PipelineStageFlags.PipelineStageTransferBit, 0, 0, null, 0, null, 1, imageMemoryBarrier);
            }
            else if ((oldLayout == ImageLayout.TransferDstOptimal) && (newLayout == ImageLayout.ShaderReadOnlyOptimal))
            {
                imageMemoryBarrier.SrcAccessMask = AccessFlags.AccessTransferWriteBit;
                imageMemoryBarrier.DstAccessMask = AccessFlags.AccessShaderReadBit;

                vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.PipelineStageTransferBit, PipelineStageFlags.PipelineStageFragmentShaderBit, 0, 0, null, 0, null, 1, imageMemoryBarrier);
            }
            else if ((oldLayout == ImageLayout.ShaderReadOnlyOptimal) && (newLayout == ImageLayout.TransferDstOptimal))
            {
                imageMemoryBarrier.SrcAccessMask = AccessFlags.AccessShaderReadBit;
                imageMemoryBarrier.DstAccessMask = AccessFlags.AccessTransferWriteBit;

                vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.PipelineStageFragmentShaderBit, PipelineStageFlags.PipelineStageTransferBit, 0, 0, null, 0, null, 1, imageMemoryBarrier);
            }

            EndImageCommandBuffer(commandBuffer);
        }

        private void CopyBufferToImage(Buffer<byte> source)
        {
            Vk vk = _renderer.Vk;

            CommandBuffer commandBuffer = BeginImageCommandBuffer();

            BufferImageCopy bufferImageCopy = new()
            {
                BufferOffset = 0,
                BufferRowLength = 0,
                BufferImageHeight = 0,

                ImageSubresource = new ImageSubresourceLayers()
                {
                    AspectMask = ImageAspectFlags.ImageAspectColorBit,
                    MipLevel = 0,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                },

                ImageOffset = new Offset3D()
                {
                    X = 0,
                    Y = 0,
                    Z = 0
                },
                ImageExtent = new Extent3D()
                {
                    Width = Size.X,
                    Height = Size.Y,
                    Depth = 1
                }
            };

            vk.CmdCopyBufferToImage(commandBuffer, source.Handle, _image, ImageLayout.TransferDstOptimal, 1, bufferImageCopy);

            EndImageCommandBuffer(commandBuffer);
        }

        private ReadOnlySpan<byte> FakeData()
        {
            uint format = (_type == Rendering.Texture.Rgba) ? (uint)4 : (uint)1;
            uint textureSize = Size.X * Size.Y * format;
            ReadOnlySpan<byte> data = new byte[textureSize];
            return data;
        }

        private void Upload(ReadOnlySpan<byte> data)
        {
            Buffer<byte> stagingBuffer = new(BufferUsageFlags.BufferUsageTransferSrcBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit, _renderer);
            stagingBuffer.Update(data);

            TransitionImageLayout(ImageLayout.Undefined, ImageLayout.TransferDstOptimal);
            CopyBufferToImage(stagingBuffer);
            if (HasFlag(ImageFlags.GenerateMimpas))
            {
                Mipmaps();
            }
            else
            {
                TransitionImageLayout(ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal);
            }

            stagingBuffer.Dispose();
        }

        private unsafe ImageView CreateImageView()
        {
            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            ImageViewCreateInfo imageViewCreateInfo = new()
            {
                SType = StructureType.ImageViewCreateInfo,

                Image = _image,
                ViewType = ImageViewType.ImageViewType2D,
                Format = _format,

                SubresourceRange = new ImageSubresourceRange()
                {
                    AspectMask = ImageAspectFlags.ImageAspectColorBit,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                    BaseMipLevel = 0,
                    LevelCount = _mipLevelCount
                },
                Components = new ComponentMapping()
                {
                    R = ComponentSwizzle.Identity,
                    G = ComponentSwizzle.Identity,
                    B = ComponentSwizzle.Identity,
                    A = ComponentSwizzle.Identity
                }
            };

            _renderer.AssertVulkan(vk.CreateImageView(device, imageViewCreateInfo, allocator, out ImageView imageView));
            return imageView;
        }

        private unsafe Sampler CreateSampler()
        {
            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            SamplerCreateInfo imageSampler = new()
            {
                SType = StructureType.SamplerCreateInfo
            };

            imageSampler.MipmapMode = SamplerMipmapMode.Nearest;
            if (HasFlag(ImageFlags.Nearest))
            {
                imageSampler.MinFilter = Filter.Nearest;
                imageSampler.MagFilter = Filter.Nearest;
            }
            else
            {
                imageSampler.MinFilter = Filter.Linear;
                imageSampler.MagFilter = Filter.Linear;
                if (HasFlag(ImageFlags.GenerateMimpas))
                {
                    imageSampler.MipmapMode = SamplerMipmapMode.Linear;
                }
            }

            if (HasFlag(ImageFlags.RepeatX))
            {
                imageSampler.AddressModeU = SamplerAddressMode.Repeat;
            }
            else
            {
                imageSampler.AddressModeU = SamplerAddressMode.ClampToEdge;
            }

            if (HasFlag(ImageFlags.RepeatY))
            {
                imageSampler.AddressModeV = SamplerAddressMode.Repeat;
            }
            else
            {
                imageSampler.AddressModeV = SamplerAddressMode.ClampToEdge;
            }

            imageSampler.AddressModeW = SamplerAddressMode.Repeat;

            _renderer.AssertVulkan(vk.CreateSampler(device, imageSampler, allocator, out Sampler sampler));
            return sampler;
        }

        public void Load(Vector2D<uint> size, ImageFlags flags, Rendering.Texture type, ReadOnlySpan<byte> data)
        {
            _format = (type == Rendering.Texture.Rgba) ? Format.R8G8B8A8Unorm : Format.R8Unorm;

            Id = ++_idCounter;
            Size = size;
            _type = type;
            _mipLevelCount = CalculateMipLevelCount();
            _image = CreateImage(type);
            TextureType = type;
            _flags = flags;

            _memory = BindImageMemory();
            if ((data == null) || (data.Length == 0))
            {
                data = FakeData();
            }
            Upload(data);
            _imageView = CreateImageView();
            _sampler = CreateSampler();
        }

        public unsafe void Mipmaps()
        {
            PhysicalDevice physicalDevice = _renderer.Params.PhysicalDevice;
            Vk vk = _renderer.Vk;

            vk.GetPhysicalDeviceFormatProperties(physicalDevice, _format, out FormatProperties formatProperties);
            if (!formatProperties.OptimalTilingFeatures.HasFlag(FormatFeatureFlags.FormatFeatureSampledImageFilterLinearBit)) // bitting not supported.
            {
                return;
            }

            CommandBuffer commandBuffer = BeginImageCommandBuffer();

            uint mipWidth = Size.X;
            uint mipHeight = Size.Y;

            ImageMemoryBarrier barrier = new()
            {
                SType = StructureType.ImageMemoryBarrier,

                Image = _image,
                SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
                DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
                SubresourceRange = new ImageSubresourceRange()
                {
                    AspectMask = ImageAspectFlags.ImageAspectColorBit,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                    BaseMipLevel = 0,
                    LevelCount = 1
                }
            };

            for (uint i = 1; i < _mipLevelCount; i++)
            {
                barrier.SubresourceRange.BaseMipLevel = i - 1;
                barrier.OldLayout = ImageLayout.TransferDstOptimal;
                barrier.NewLayout = ImageLayout.TransferSrcOptimal;
                barrier.SrcAccessMask = AccessFlags.AccessTransferWriteBit;
                barrier.DstAccessMask = AccessFlags.AccessTransferReadBit;

                vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.PipelineStageTransferBit, PipelineStageFlags.PipelineStageTransferBit, 0, 0, null, 0, null, 1, barrier);

                ImageBlit blit = new()
                {
                    SrcOffsets = new ImageBlit.SrcOffsetsBuffer()
                    {
                        Element0 = new Offset3D()
                        {
                            X = 0,
                            Y = 0,
                            Z = 0,
                        },
                        Element1 = new Offset3D()
                        {
                            X = (int)mipWidth,
                            Y = (int)mipHeight,
                            Z = 1
                        }
                    },
                    SrcSubresource = new ImageSubresourceLayers()
                    {
                        AspectMask = ImageAspectFlags.ImageAspectColorBit,
                        MipLevel = i - 1,
                        BaseArrayLayer = 0,
                        LayerCount = 1
                    },
                    DstOffsets = new ImageBlit.DstOffsetsBuffer()
                    {
                        Element0 = new Offset3D()
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        },
                        Element1 = new Offset3D()
                        {
                            X = (int)((mipWidth > 1) ? mipWidth / 2 : 1),
                            Y = (int)((mipHeight > 1) ? mipHeight / 2 : 1),
                            Z = 1
                        }
                    },
                    DstSubresource = new ImageSubresourceLayers()
                    {
                        AspectMask = ImageAspectFlags.ImageAspectColorBit,
                        MipLevel = i,
                        BaseArrayLayer = 0,
                        LayerCount = 1
                    }
                };

                vk.CmdBlitImage(commandBuffer, _image, ImageLayout.TransferSrcOptimal,
                    _image, ImageLayout.TransferDstOptimal, 1, blit, HasFlag(ImageFlags.Nearest) ? Filter.Nearest : Filter.Linear);

                barrier.OldLayout = ImageLayout.TransferSrcOptimal;
                barrier.NewLayout = ImageLayout.ShaderReadOnlyOptimal;
                barrier.SrcAccessMask = AccessFlags.AccessTransferReadBit;
                barrier.DstAccessMask = AccessFlags.AccessShaderReadBit;

                vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.PipelineStageTransferBit, PipelineStageFlags.PipelineStageFragmentShaderBit, 0, 0, null, 0, null, 1, barrier);

                mipWidth /= 2;
                mipHeight /= 2;
            }

            barrier.SubresourceRange.BaseMipLevel = _mipLevelCount - 1;
            barrier.OldLayout = ImageLayout.TransferDstOptimal;
            barrier.NewLayout = ImageLayout.ShaderReadOnlyOptimal;
            barrier.SrcAccessMask = AccessFlags.AccessTransferWriteBit;
            barrier.DstAccessMask = AccessFlags.AccessShaderReadBit;

            vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.PipelineStageTransferBit, PipelineStageFlags.PipelineStageFragmentShaderBit, 0, 0, null, 0, null, 1, barrier);

            EndImageCommandBuffer(commandBuffer);
        }

        public unsafe void Update(Rectangle<uint> bounds, ReadOnlySpan<byte> data)
        {
            Vk vk = _renderer.Vk;

            Buffer<byte> stagingBuffer = new(BufferUsageFlags.BufferUsageTransferSrcBit,
                MemoryPropertyFlags.MemoryPropertyHostVisibleBit | MemoryPropertyFlags.MemoryPropertyHostCoherentBit, _renderer);
            stagingBuffer.Update(data);

            TransitionImageLayout(ImageLayout.ShaderReadOnlyOptimal, ImageLayout.TransferDstOptimal);

            CommandBuffer commandBuffer = BeginImageCommandBuffer();

            BufferImageCopy bufferImageCopy = new()
            {
                BufferOffset = 0,
                BufferRowLength = 0,
                BufferImageHeight = 0,

                ImageSubresource = new ImageSubresourceLayers()
                {
                    AspectMask = ImageAspectFlags.ImageAspectColorBit,
                    MipLevel = 0,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                },

                ImageOffset = new Offset3D()
                {
                    X = 0,
                    Y = 0,
                    Z = 0
                },
                ImageExtent = new Extent3D()
                {
                    Width = Size.X,
                    Height = Size.Y,
                    Depth = 1
                }
            };

            vk.CmdCopyBufferToImage(commandBuffer, stagingBuffer.Handle, _image, ImageLayout.TransferDstOptimal, 1, bufferImageCopy);

            EndImageCommandBuffer(commandBuffer);
            TransitionImageLayout(ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal);

            stagingBuffer.Dispose();

        }

        public bool HasFlag(ImageFlags flag)
        {
            return _flags.HasFlag(flag);
        }

        public unsafe void Dispose()
        {
            if (Id == 0)
            {
                return;
            }

            Device device = _renderer.Params.Device;
            AllocationCallbacks* allocator = (AllocationCallbacks*)_renderer.Params.AllocationCallbacks.ToPointer();
            Vk vk = _renderer.Vk;

            _renderer.AssertVulkan(vk.DeviceWaitIdle(device));

            vk.DestroySampler(device, _sampler, allocator);
            vk.DestroyImageView(device, _imageView, allocator);
            vk.FreeMemory(device, _memory, allocator);
            vk.DestroyImage(device, _image, allocator);
        }

    }
}
