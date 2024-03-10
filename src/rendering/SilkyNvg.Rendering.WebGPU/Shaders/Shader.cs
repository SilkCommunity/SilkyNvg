using System;
using Silk.NET.Core.Native;
using Silk.NET.WebGPU;

namespace SilkyNvg.Rendering.WebGPU.Shaders
{
    public unsafe class Shader : IDisposable
    {
        private readonly WebGPURenderer _renderer;
        private readonly Silk.NET.WebGPU.WebGPU _wgpu;
        private readonly ShaderModule* _module;

        public ShaderModule* Module => _module;
        
        public Shader(string name, bool edgeAA, WebGPURenderer renderer)
        {
            _renderer = renderer;
            _wgpu = renderer.WebGPU;
            
            ShaderModuleWGSLDescriptor wgslDescriptor = new ShaderModuleWGSLDescriptor
            {
                Code = (byte*) SilkMarshal.StringToPtr(ShaderCode.CODE),
                Chain = new ChainedStruct
                {
                    SType = SType.ShaderModuleWgslDescriptor
                }
            };

            ShaderModuleDescriptor shaderModuleDescriptor = new ShaderModuleDescriptor
            {
                NextInChain = (ChainedStruct*) (&wgslDescriptor),
            };
            
            _module = _wgpu.DeviceCreateShaderModule(_renderer.Params.Device, in shaderModuleDescriptor);
        }

        public void Dispose()
        {
            _wgpu.ShaderModuleRelease(_module);
        }
    }
}