using System;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Utils;

namespace SilkyNvg.Rendering.OpenGL.Buffers;

internal sealed class BufferList<T> : IDisposable
    where T : unmanaged
{

    private readonly int _frames;
    private readonly string _name;
    private readonly GL _gl;

    private MultiBuffer<T> _buffer;
    
    internal uint Count { get; private set; }

    internal uint Capacity => _buffer.OriginalSize;
    
    internal BufferList(int frames, uint initialSize, string name, GL gl)
    {
        _frames = frames;
        _name = name;
        _gl = gl;

        Count = 0;
        _buffer = new MultiBuffer<T>(initialSize, _frames, $"{name}-buffer", _gl);
    }

    internal void Advance()
    {
        _buffer.Advance();
    }

    private void EnsureCapacity(uint necessaryCapacity)
    {
        if (necessaryCapacity >= _buffer.OriginalSize)
        {
            uint newSize = _buffer.OriginalSize * 2;
            while (newSize < necessaryCapacity)
            {
                newSize *= 2;
            }
            
            Log.Info($"growing list buffer ({_buffer.Name}): {_buffer.OriginalSize} -> {newSize}");
            
            // OpenGL barrier in case GPU using any previous frame
            _gl.Finish();

            var newBuffer = new MultiBuffer<T>(newSize, _frames, _name, _gl);
            
            // copy data
            for (int frame = 0; frame < _frames; frame++)
            {
                int srcOffset = frame * (int)_buffer.OriginalSize;
                int destOffset = frame * (int)newSize;
                _buffer.Buffer.CopyTo(newBuffer.Buffer, srcOffset, destOffset, _buffer.OriginalSize);
            }

            _buffer.Dispose();
            _buffer = newBuffer;
        }
    }

    internal void AddRange(Span<T> elements)
    {
        uint necessaryCapacity = Count + (uint)elements.Length;
        EnsureCapacity(necessaryCapacity);
        
        _buffer.Write(elements, (int)Count);
    }

    internal void AddRange(params T[] elements)
    {
        var elementsView = new Span<T>(elements);
        AddRange(elementsView);
    }

    internal void Add(T element)
    {
        Span<T> elementView = [element];
        AddRange(elementView);
    }
    
    public void Dispose()
    {
        _buffer.Dispose();
    }
    
}