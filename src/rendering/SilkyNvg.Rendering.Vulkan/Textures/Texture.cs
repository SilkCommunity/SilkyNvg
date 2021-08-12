using Silk.NET.Maths;
using Silk.NET.Vulkan;
using SilkyNvg.Images;
using System;
using System.Collections.Generic;

namespace SilkyNvg.Rendering.Vulkan.Textures
{
    internal class Texture : IDisposable
    {

        private static readonly IList<Texture> textures = new List<Texture>();

        private readonly Sampler _sampler;

        private readonly Image _image;
        private readonly DeviceMemory _mem;
        private readonly ImageFlags _flags;

        private readonly VulkanRenderer _renderer;

        public Vector2D<uint> Size { get; }

        public int ID { get; }

        public Rendering.Texture TextureType { get; }

        public ImageLayout ImageLayout { get; }

        public ImageView View { get; }

        public Sampler Sampler => _sampler;

        public unsafe Texture(Vector2D<uint> size, ImageFlags flags, Rendering.Texture type, ReadOnlySpan<byte> data, VulkanRenderer renderer)
        {
            _renderer = renderer;

            ImageCreateInfo imageCreateInfo = new(StructureType.ImageCreateInfo)
            {
                ImageType = ImageType.ImageType2D,
                Extent = new Extent3D(size.X, size.Y, 1),
                MipLevels = 1,
                ArrayLayers = 1,
                Samples = SampleCountFlags.SampleCount1Bit,
                Tiling = ImageTiling.Linear,
                InitialLayout = ImageLayout.Preinitialized,
                Usage = ImageUsageFlags.ImageUsageSampledBit,
                QueueFamilyIndexCount = 0,
                PQueueFamilyIndices = null,
                SharingMode = SharingMode.Exclusive,
            };
            if (type == Rendering.Texture.Rgba)
            {
                imageCreateInfo.Format = Format.R8G8B8A8Unorm;
            }
            else
            {
                imageCreateInfo.Format = Format.R8Unorm;
            }

            MemoryAllocateInfo memAlloc = new(StructureType.MemoryAllocateInfo)
            {
                AllocationSize = 0
            };

            _renderer.Assert(_renderer.Vk.CreateImage(_renderer.Params.device, imageCreateInfo, _renderer.Params.allocator, out Image mappableImage));

            _renderer.Vk.GetImageMemoryRequirements(_renderer.Params.device, mappableImage, out MemoryRequirements memReqs);

            memAlloc.AllocationSize = memReqs.Size;

            _renderer.Assert(_renderer.MemoryTypeFromProperties(memReqs.MemoryTypeBits, MemoryPropertyFlags.MemoryPropertyHostVisibleBit, out memAlloc.MemoryTypeIndex));

            _renderer.Assert(_renderer.Vk.AllocateMemory(_renderer.Params.device, memAlloc, _renderer.Params.allocator, out DeviceMemory mappableMemory));

            _renderer.Assert(_renderer.Vk.BindImageMemory(_renderer.Params.device, mappableImage, mappableMemory, 0));

            SamplerCreateInfo samplerCreateInfo = new(StructureType.SamplerCreateInfo)
            {
                MipLodBias = 0.0f,
                AnisotropyEnable = false,
                MaxAnisotropy = 1,
                CompareEnable = false,
                CompareOp = CompareOp.Never,
                MinLod = 0.0f,
                MaxLod = 0.0f,
                BorderColor = BorderColor.FloatOpaqueWhite,
                MipmapMode = SamplerMipmapMode.Nearest
            };
            if (flags.HasFlag(ImageFlags.Nearest))
            {
                samplerCreateInfo.MagFilter = Filter.Nearest;
                samplerCreateInfo.MinFilter = Filter.Nearest;
            }
            else
            {
                samplerCreateInfo.MagFilter = Filter.Linear;
                samplerCreateInfo.MinFilter = Filter.Linear;
            }
            if (flags.HasFlag(ImageFlags.RepeatX))
            {
                samplerCreateInfo.AddressModeU = SamplerAddressMode.MirroredRepeat;
                samplerCreateInfo.AddressModeV = SamplerAddressMode.MirroredRepeat;
                samplerCreateInfo.AddressModeW = SamplerAddressMode.MirroredRepeat;
            }
            else
            {
                samplerCreateInfo.AddressModeU = SamplerAddressMode.ClampToEdge;
                samplerCreateInfo.AddressModeV = SamplerAddressMode.ClampToEdge;
                samplerCreateInfo.AddressModeW = SamplerAddressMode.ClampToEdge;
            }

            _renderer.Assert(_renderer.Vk.CreateSampler(_renderer.Params.device, samplerCreateInfo, _renderer.Params.allocator, out _sampler));

            ImageViewCreateInfo viewInfo = new(StructureType.ImageViewCreateInfo)
            {
                Image = mappableImage,
                ViewType = ImageViewType.ImageViewType2D,
                Format = imageCreateInfo.Format,
                Components = new ComponentMapping(ComponentSwizzle.R, ComponentSwizzle.G, ComponentSwizzle.B, ComponentSwizzle.A),
                SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ImageAspectColorBit, 0, 1, 0, 1)
            };

            _renderer.Assert(_renderer.Vk.CreateImageView(_renderer.Params.device, viewInfo, _renderer.Params.allocator, out ImageView imageView));

            Size = size;
            _image = mappableImage;
            View = imageView;
            _mem = mappableMemory;
            ImageLayout = ImageLayout.ShaderReadOnlyOptimal;
            TextureType = type;
            _flags = flags;

            if (data != null && data.Length > 0)
            {
                Update(_renderer.Params.device, Rectangle.FromLTRB((uint)0, (uint)0, Size.X, Size.Y), data);
            }
            else
            {
                uint txFormat = 1;
                if (TextureType == Rendering.Texture.Rgba)
                {
                    txFormat = 4;
                }

                ulong textureSize = Size.X * Size.Y * txFormat * sizeof(byte);
                Span<byte> generatedTexture = new byte[(int)textureSize];
                Update(_renderer.Params.device, Rectangle.FromLTRB((uint)0, (uint)0, Size.X, Size.Y), generatedTexture);
            }

            Init(_renderer.Params.cmdBuffer, _renderer.Queue);

            ID = textures.Count + 1;
            textures.Add(this);
        }

        private unsafe void Init(CommandBuffer cmdBuffer, Queue queue)
        {
            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.CommandBufferUsageOneTimeSubmitBit
            };

            _ = _renderer.Vk.BeginCommandBuffer(cmdBuffer, beginInfo);

            ImageMemoryBarrier layoutTransitionBarrier = new()
            {
                SType = StructureType.ImageMemoryBarrier,
                SrcAccessMask = 0,
                DstAccessMask = 0,
                OldLayout = ImageLayout.Preinitialized,
                NewLayout = ImageLayout.ShaderReadOnlyOptimal,
                SrcQueueFamilyIndex = uint.MaxValue,
                DstQueueFamilyIndex = uint.MaxValue,
                Image = _image
            };
            ImageSubresourceRange resourceRange = new(ImageAspectFlags.ImageAspectColorBit, 0, 1, 0, 1);
            layoutTransitionBarrier.SubresourceRange = resourceRange;

            _renderer.Vk.CmdPipelineBarrier(cmdBuffer, PipelineStageFlags.PipelineStageTopOfPipeBit, PipelineStageFlags.PipelineStageTopOfPipeBit,
                0, 0, null, 0, null, 1, layoutTransitionBarrier);

            _ = _renderer.Vk.EndCommandBuffer(cmdBuffer);

            PipelineStageFlags* waitStageMash = stackalloc PipelineStageFlags[] { PipelineStageFlags.PipelineStageColorAttachmentOutputBit };
            SubmitInfo submitInfo = new()
            {
                SType = StructureType.SubmitInfo,

                WaitSemaphoreCount = 0,
                PWaitSemaphores = null,
                PWaitDstStageMask = waitStageMash,
                CommandBufferCount = 1,
                PCommandBuffers = &cmdBuffer,
                SignalSemaphoreCount = 0,
                PSignalSemaphores = null
            };
            _ = _renderer.Vk.QueueSubmit(queue, 1, submitInfo, default);
            _ = _renderer.Vk.QueueWaitIdle(queue);
            _ = _renderer.Vk.ResetCommandBuffer(cmdBuffer, 0);
        }

        public unsafe void Update(Device device, Rectangle<uint> bounds, ReadOnlySpan<byte> data)
        {
            _renderer.Vk.GetImageMemoryRequirements(device, _image, out MemoryRequirements memReqs);
            ImageSubresource subres = new(ImageAspectFlags.ImageAspectColorBit, 0, 0);
            void* bindptr;
            uint compSize = (TextureType == Rendering.Texture.Rgba) ? (uint)4 : (uint)1;

            _renderer.Vk.GetImageSubresourceLayout(_renderer.Params.device, _image, subres, out SubresourceLayout layout);
            _renderer.Assert(_renderer.Vk.MapMemory(device, _mem, 0, memReqs.Size, 0, &bindptr));
            for (uint y = 0; y < bounds.Max.Y; y++)
            {
                ReadOnlySpan<byte> src = data.Slice((int)(((bounds.Origin.Y + y) * (Size.X * compSize)) + bounds.Origin.X), (int)(bounds.Max.X * compSize));
                Span<byte> dest = new((byte*)bindptr + ((bounds.Origin.Y + y) * layout.RowPitch) + bounds.Origin.X, (int)(bounds.Max.X * compSize));
                src.CopyTo(dest);
            }

            _renderer.Vk.UnmapMemory(device, _mem);
        }

        public bool HasFlag(ImageFlags flags)
        {
            return _flags.HasFlag(flags);
        }

        public unsafe void Dispose()
        {
            if (View.Handle != 0)
            {
                _renderer.Vk.DestroyImageView(_renderer.Params.device, View, _renderer.Params.allocator);
            }
            if (_sampler.Handle != 0)
            {
                _renderer.Vk.DestroySampler(_renderer.Params.device, _sampler, _renderer.Params.allocator);
            }
            if (_image.Handle != 0)
            {
                _renderer.Vk.DestroyImage(_renderer.Params.device, _image, _renderer.Params.allocator);
            }
            if (_mem.Handle != 0)
            {
                _renderer.Vk.FreeMemory(_renderer.Params.device, _mem, _renderer.Params.allocator);
            }
            textures.Remove(this);
        }

        public static Texture FindTexture(int image)
        {
            image--;
            return (image >= textures.Count) || (image < 0) ? null : textures[image];
        }

        public static void DeleteAll()
        {
            int i = textures.Count - 1;
            while (textures.Count > 0)
            {
                textures[i--].Dispose();
            }
        }

    }
}
