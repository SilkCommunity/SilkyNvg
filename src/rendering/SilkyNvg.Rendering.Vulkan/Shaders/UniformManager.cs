using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering.Vulkan.Shaders
{
    internal class UniformManager
    {

        private readonly int _fragSize;

        private byte[] _uniforms;
        private int _count;
        private int _capacity;

        public int CurrentsOffset => _count;

        public ReadOnlySpan<byte> Uniforms => _uniforms;

        public UniformManager(int fragSize)
        {
            _fragSize = fragSize;
            _uniforms = Array.Empty<byte>();
            _count = 0;
            _capacity = 0;
        }

        private int AllocUniforms(int n)
        {
            if (_count + n > _capacity)
            {
                int cuniforms = Math.Max(_count + n, 128) + _uniforms.Length / 2;
                Array.Resize(ref _uniforms, cuniforms * _fragSize);
                _capacity = cuniforms;
            }
            return _count * _fragSize;
        }

        public unsafe int AddUniform(FragUniforms uniforms)
        {
            int ret = AllocUniforms(1);
            ReadOnlySpan<byte> bytes = new(&uniforms, Marshal.SizeOf(typeof(FragUniforms)));
            Buffer.BlockCopy(bytes.ToArray(), 0, _uniforms, ret, bytes.Length);
            _count += 1;
            return ret;
        }

        public void Clear()
        {
            _count = 0;
        }

    }
}