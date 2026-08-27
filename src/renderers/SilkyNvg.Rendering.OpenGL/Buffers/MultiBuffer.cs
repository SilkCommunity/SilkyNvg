using System;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Buffers;

internal sealed class MultiBuffer<T> : IDisposable
    where T : unmanaged
{
    private int _frameOffset;
    
    internal int Frames { get; }
    
    internal uint OriginalSize { get; }
    
    internal string Name { get; }

    internal PersistentBuffer<T> Buffer { get; }

    internal MultiBuffer(uint size, int frames, string name, GL gl)
    {
        Frames = frames;
        OriginalSize = size;
        Name = name;

        _frameOffset = 0;

        Buffer = new PersistentBuffer<T>(size * (uint)frames, Name, gl);
    }

    internal void Advance()
    {
        _frameOffset = (_frameOffset + (int)OriginalSize) % ((int)OriginalSize * Frames);
    }

    internal void Write(Span<T> data, int offset)
    {
        Buffer.Write(data, offset + _frameOffset);
    }
    
    public void Dispose()
    {
        Buffer.Dispose();
    }
    
}