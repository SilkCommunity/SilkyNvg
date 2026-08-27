using System;
using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Utils;

namespace SilkyNvg.Rendering.OpenGL.Buffers;

internal sealed unsafe class PersistentBuffer<T> : IDisposable
    where T : unmanaged
{

    private readonly uint _handle;
    private readonly GL _gl;

    internal uint Size { get; }

    internal string Name { get; }
    
    internal void* Map { get; }
    
    internal PersistentBuffer(uint size, string name, GL gl)
    {
        _gl = gl;
        Size = size;
        Name = name;

        _handle = gl.CreateBuffer();
        Errors.CheckGLError("gen buffer", _gl);

#if DEBUG
        _gl.ObjectLabel(ObjectIdentifier.Buffer, _handle, (uint)Name.Length, Name);
        Errors.CheckGLError("label buffer", _gl);
#endif
        
        _gl.NamedBufferStorage(_handle, Size * (uint)sizeof(T), null,
            BufferStorageMask.DynamicStorageBit | BufferStorageMask.MapWriteBit | BufferStorageMask.MapPersistentBit);
        Errors.CheckGLError("allocate buffer", _gl);

        Map = _gl.MapNamedBufferRange(_handle, 0, Size * (uint)sizeof(T),
            MapBufferAccessMask.WriteBit | MapBufferAccessMask.PersistentBit | MapBufferAccessMask.FlushExplicitBit);
        Errors.CheckGLError("map buffer", _gl);
    }

    internal void CopyTo(PersistentBuffer<T> dest, int srcOffset, int destOffset, uint count)
    {
        _gl.CopyNamedBufferSubData(_handle, dest._handle, srcOffset * sizeof(T), destOffset * sizeof(T),
            count * (uint)sizeof(T));
        Errors.CheckGLError("copy buffer", _gl);
    }

    internal void Write(Span<T> data, int offset)
    {
        if (Size < data.Length + offset)
        {
            Log.Error($"{Name}-Buffer too small!");
            throw new Exception("Buffer too small!");
        }

        fixed (void* ptr = data)
        {
            void* dest = Unsafe.Add<T>(Map, offset);
            Unsafe.CopyBlock(dest, ptr, (uint)(data.Length * sizeof(T)));
            _gl.FlushMappedNamedBufferRange(_handle, offset * sizeof(T), (uint)(data.Length * sizeof(T)));
            Errors.CheckGLError("flush buffer", _gl);
        }
    }
    
    public void Dispose()
    {
        _gl.UnmapNamedBuffer(_handle);
        _gl.DeleteBuffer(_handle);
    }
    
}