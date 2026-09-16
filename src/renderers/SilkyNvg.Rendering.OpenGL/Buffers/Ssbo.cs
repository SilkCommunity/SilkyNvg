using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Synchronization;
using SilkyNvg.Rendering.OpenGL.Utils;

namespace SilkyNvg.Rendering.OpenGL.Buffers;

internal sealed class Ssbo<T> : IDisposable
    where T : unmanaged
{
    
    private readonly string _debugName;
    
    private readonly uint _elementSize;
    private readonly uint _alignment;
    private readonly BufferStorageMask _storageMask;

    private readonly FrameManager _frameManager;
    
    private readonly GL _gl;

    private uint _regionSizeBytes;
    private int _currentRegion;
    
    private uint _bufferId;

    internal uint Capacity { get; private set; }
    
    internal Ssbo(BufferStorageMask storageMask, FrameManager frameManager, string debugName, GL gl)
    {
        _storageMask = storageMask;
        _frameManager = frameManager;
        _debugName = debugName;
        _gl = gl;
        
        _elementSize = (uint)Marshal.SizeOf<T>();

        _alignment = (uint)_gl.GetInteger(GetPName.ShaderStorageBufferOffsetAlignment);
    }

    private int RegionOffset()
    {
        return _currentRegion * (int)_regionSizeBytes;
    }

    private unsafe void Allocate(uint capacityElements)
    {
        _regionSizeBytes = Utils.Utils.Align(capacityElements * _elementSize, _alignment);
        uint totalSize = _regionSizeBytes * (uint)_frameManager.FramesInFlight;
        
        Log.Info($"Allocating new {_debugName}-SSBO. region_size={_regionSizeBytes} B, total_size={totalSize} B");

        _bufferId = _gl.CreateBuffer();
        Errors.CheckGLError("create buffer", _gl);
        _gl.ObjectLabel(ObjectIdentifier.Buffer, _bufferId, (uint)_debugName.Length, _debugName);

        _gl.NamedBufferStorage(_bufferId, totalSize, null, _storageMask);
        Errors.CheckGLError("buffer storage", _gl);

        Capacity = capacityElements;
    }
    
    private void Reallocate(uint newCapacity)
    {
        _frameManager.WaitOnAll();

        uint oldBufferId = _bufferId;
        Allocate(newCapacity);

        if (oldBufferId != 0)
        {
            _gl.DeleteBuffer(oldBufferId);
        }
    }

    internal void EnsureCapacity(uint requiredCapacity)
    {
        if (requiredCapacity <= Capacity)
        {
            return;
        }

        Log.Info($"Growing {_debugName}-SSBO: {Capacity} -> {requiredCapacity}");
        Reallocate(requiredCapacity);
    }

    internal void Store(ReadOnlySpan<T> data)
    {
        EnsureCapacity((uint)data.Length);
        _gl.NamedBufferSubData(_bufferId, 0, data);
    }

    internal unsafe Span<T> Map(MapBufferAccessMask accessMask)
    {
        int offset = RegionOffset();
        uint size = _regionSizeBytes;
        
        var map = _gl.MapNamedBufferRange(_bufferId, offset, size, accessMask);
        Errors.CheckGLError("map buffer", _gl);

        if (map == null)
        {
            _gl.DeleteBuffer(_bufferId);
            _bufferId = 0;

            throw new InvalidOperationException("Failed to persistently map OpenGL buffer.");
        }

        var ptr = (T*)map;
        return new Span<T>(ptr, (int)Capacity);
    }

    internal void Unmap()
    {
        _gl.UnmapNamedBuffer(_bufferId);
        Errors.CheckGLError("unmap buffer", _gl);
    }

    internal void Bind(uint binding)
    {
        int offset = RegionOffset();
        uint size = _regionSizeBytes;

        _gl.BindBufferRange(BufferTargetARB.ShaderStorageBuffer, binding, _bufferId, offset, size);
        Errors.CheckGLError($"bind SSBO (\"{_debugName}\")", _gl);
    }

    internal void MakeCurrentFrameCurrent()
    {
        _currentRegion = _frameManager.CurrentFrame;
    }
    
    public void Dispose()
    {
        if (_bufferId != 0)
        {
            _gl.DeleteBuffer(_bufferId);
            _bufferId = 0;
        }
    }
    
}