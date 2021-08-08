using System;
using System.Collections.Generic;

namespace SilkyNvg.Rendering.Vulkan.Shaders
{
    internal class UniformManager
    {

        private readonly int _fragSize;

        private FragUniforms[] _uniforms;
        private int _count;

        public int CurrentsOffset => _count;

        public ReadOnlySpan<FragUniforms> Uniforms => _uniforms;

        public UniformManager(int fragSize)
        {
            _fragSize = fragSize;
            _uniforms = Array.Empty<FragUniforms>();
            _count = 0;
        }

        private int AllocVerts(int n)
        {
            if (_count + n > _uniforms.Length)
            {
                int cverts = Math.Max(_count + n, 128) + _uniforms.Length / 2;
                Array.Resize(ref _uniforms, cverts);
            }
            return _count * _fragSize;
        }

        public int AddUniform(FragUniforms uniforms)
        {
            int ret = AllocVerts(1);
            _uniforms[_count++] = uniforms;
            return ret;
        }

        public int AddUniforms(ICollection<FragUniforms> uniforms)
        {
            int ret = AllocVerts(uniforms.Count);
            uniforms.CopyTo(_uniforms, _count);
            _count += uniforms.Count;
            return ret;
        }

        public void Clear()
        {
            _count = 0;
        }

    }
}