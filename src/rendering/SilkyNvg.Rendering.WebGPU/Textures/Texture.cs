using System;
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using SilkyNvg.Images;

namespace SilkyNvg.Rendering.WebGPU.Textures
{
    internal unsafe class Texture : IDisposable
    {
        private readonly WebGPURenderer _renderer;
        private readonly Silk.NET.WebGPU.WebGPU _wgpu;
        private Silk.NET.WebGPU.Texture* _texture;
        private TextureView* _textureView;

        internal Texture(WebGPURenderer renderer)
        {
            _renderer = renderer;
            _wgpu = _renderer.WebGPU;
        }

        public void Load(Vector2D<uint> size, ImageFlags flags, Rendering.Texture type, ReadOnlySpan<byte> data)
        {
            TextureFormat viewFormat = TextureFormat.Rgba8Unorm;
            TextureDescriptor descriptor = new TextureDescriptor
            {
                Size            = new Extent3D(size.X, size.Y, 1),
                Format          = TextureFormat.Rgba8Unorm,
                Usage           = TextureUsage.CopyDst | TextureUsage.TextureBinding,
                MipLevelCount   = 1,
                SampleCount     = 1,
                Dimension       = TextureDimension.Dimension2D,
                ViewFormats     = &viewFormat,
                ViewFormatCount = 1
            };
            
            _texture = _wgpu.DeviceCreateTexture(_renderer.Params.Device, descriptor);

            TextureViewDescriptor viewDescriptor = new TextureViewDescriptor
            {
                Format          = TextureFormat.Rgba8Unorm,
                Dimension       = TextureViewDimension.Dimension2D,
                Aspect          = TextureAspect.All,
                MipLevelCount   = 1,
                ArrayLayerCount = 1,
                BaseArrayLayer  = 0,
                BaseMipLevel    = 0
            };

            _textureView = _wgpu.TextureCreateView(_texture, viewDescriptor);
            
            Queue* queue = _wgpu.DeviceGetQueue(_renderer.Params.Device);

            CommandEncoderDescriptor commandEncoderDescriptor = new CommandEncoderDescriptor();
            CommandEncoder* commandEncoder = _wgpu.DeviceCreateCommandEncoder(_renderer.Params.Device, commandEncoderDescriptor);


            /*for (uint x = 0; x < size.X; ++x)
            {
                for (uint y = 0; y < size.Y; ++y)
                {
                    var imageCopyTexture = new ImageCopyTexture
                    {
                        Texture  = _texture,
                        Aspect   = TextureAspect.All,
                        MipLevel = 0,
                        Origin   = new Origin3D(0, (uint) i, 0)
                    };

                    var layout = new TextureDataLayout
                    {
                        BytesPerRow  = (uint) (x.Width * sizeof(Rgba32)),
                        RowsPerImage = (uint) x.Height
                    };
                    // layout.Offset = layout.BytesPerRow * (uint) i;

                    var extent = new Extent3D
                    {
                        Width              = (uint) x.Width,
                        Height             = 1,
                        DepthOrArrayLayers = 1
                    };

                    fixed (void* pData = &data[(int)(x * size.Y + y)])
                        _wgpu.QueueWriteTexture(queue, imageCopyTexture, pData, (nuint) (sizeof(Rgba32) * imageRow.Length), layout, extent);
                }
            }*/
            /*image.ProcessPixelRows
            (
                x =>
                {
                    for (var i = 0; i < x.Height; i++)
                    {
                        var imageRow = x.GetRowSpan(i);

                        var imageCopyTexture = new ImageCopyTexture
                        {
                            Texture  = _Texture,
                            Aspect   = TextureAspect.All,
                            MipLevel = 0,
                            Origin   = new Origin3D(0, (uint) i, 0)
                        };

                        var layout = new TextureDataLayout
                        {
                            BytesPerRow  = (uint) (x.Width * sizeof(Rgba32)),
                            RowsPerImage = (uint) x.Height
                        };
                        // layout.Offset = layout.BytesPerRow * (uint) i;

                        var extent = new Extent3D
                        {
                            Width              = (uint) x.Width,
                            Height             = 1,
                            DepthOrArrayLayers = 1
                        };

                        fixed (void* dataPtr = imageRow)
                            wgpu.QueueWriteTexture(queue, imageCopyTexture, dataPtr, (nuint) (sizeof(Rgba32) * imageRow.Length), layout, extent);
                    }
                }
            );*/

            CommandBuffer* commandBuffer = _wgpu.CommandEncoderFinish(commandEncoder, new CommandBufferDescriptor());

            _wgpu.QueueSubmit(queue, 1, &commandBuffer);
        }

        public void Dispose()
        {
            
        }
    }
}