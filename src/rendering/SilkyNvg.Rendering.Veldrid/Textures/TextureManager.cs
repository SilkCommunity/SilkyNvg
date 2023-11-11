using System;
using System.Collections.Generic;

namespace SilkyNvg.Rendering.Vulkan.Textures
{
    public sealed class TextureManager : IDisposable
    {
        readonly List<TextureSlot> _textures;
        readonly List<int> _availableIndexes = new List<int>();

        private int _count;

        private readonly TextureSlot _default = new TextureSlot();

        public TextureManager(VeldridRenderer renderer)
        {

            _textures = new List<TextureSlot>();
            _count = 0;
        }

        public int AddTexture(TextureSlot slot)
        {
            int id = 0;

            if (_availableIndexes.Count > 0)
            {
                id = _availableIndexes[^1];
                _textures[id] = slot;
                _availableIndexes.RemoveAt(_availableIndexes.Count - 1);
                slot.Id = id;
            }
            else
            {
                id = _count++;
                slot.Id = id;
                _textures.Add(slot);
            }

            return id;

        }

        public bool FindTexture(int id, out TextureSlot slot)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_textures[i].Id == id)
                {
                    
                    slot = _textures[i];
                    return true;
                }
            }

            slot = _default;
            return false;
        }

        public bool DeleteTexture(int id)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_textures[i].Id == id)
                {
                    _textures[i].Dispose();
                    _textures[i] = default;
                    _availableIndexes.Add(i);
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            for (int i = 0; i < _count; i++)
            {
                _textures[i].Dispose();
            }
        }

    }
}