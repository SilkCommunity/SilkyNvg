using System;
using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Synchronization;
using SilkyNvg.Rendering.OpenGL.Utils;

namespace SilkyNvg.Rendering.OpenGL.Buffers;

internal sealed unsafe class GpuArrayList<T> : IDisposable
    where T : unmanaged
{

    private readonly string _debugName;

    private readonly uint _alignment;
    
    private readonly FrameManager _frameManager;
    private readonly GL _gl;

    private void* _map;

    private int _currentRegion;
    private bool _disposed;
    
    internal uint BufferId { get; private set; }

    internal uint Capacity { get; private set; }

    internal uint Count { get; private set; }

    internal uint ByteCount => Count * (uint)sizeof(T);

    internal uint RegionByteSize { get; private set; }

    /// <summary>
    /// Gets the current frame's CPU-Writeable storage.
    /// </summary>
    internal Span<T> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            return GetCurrentSpan();
        }
    }
    
    /// <summary>
    /// Returns a span covering the elements currently in the array.
    /// </summary>
    internal Span<T> WrittenSpan
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ThrowIfDisposed();
            return GetCurrentSpan()[..checked((int)Count)];
        }
    }

    internal GpuArrayList(uint initialCapacity, FrameManager frameManager, string debugName, GL gl)
    {
        ArgumentOutOfRangeException.ThrowIfZero(initialCapacity);
        
        _debugName = debugName;
        _frameManager = frameManager;
        _gl = gl;

        _currentRegion = 0;
        _disposed = false;

        _alignment = (uint)_gl.GetInteger(GetPName.ShaderStorageBufferOffsetAlignment);

        Allocate(initialCapacity);
    }

    internal void Bind(uint binding)
    {
        ThrowIfDisposed();

        // would crash otherwise.
        if (Count == 0)
        {
            return;
        }
        
        int offset = RegionOffset(_currentRegion);
        uint size = Count * (uint)sizeof(T);
        
        _gl.BindBufferRange(BufferTargetARB.ShaderStorageBuffer, binding, BufferId, offset, size);
        Errors.CheckGLError($"bind array list buffer (\"{_debugName}\" at binding {binding})", _gl);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T value)
    {
        EnsureCapacity(Count + 1);
        GetCurrentSpan()[checked((int)Count)] = value;
        Count++;
    }

    public void AddRange(ReadOnlySpan<T> values)
    {
        if (values.IsEmpty) return;

        EnsureCapacity(Count + (uint)values.Length);
        
        values.CopyTo(GetCurrentSpan().Slice(checked((int)Count), values.Length));
        Count += (uint)values.Length;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Clear()
    {
        Count = 0;
    }

    internal void MakeCurrentFrameCurrent()
    {
        ThrowIfDisposed();
        _currentRegion = _frameManager.CurrentFrame;
        Count = 0;
    }

    private int RegionOffset(int region)
    {
        return region * (int)RegionByteSize;
    }

    private Span<T> GetCurrentSpan()
    {
        void* ptr = Unsafe.Add<byte>(_map, RegionOffset(_currentRegion));
        return new Span<T>(ptr, checked((int)Capacity));
    }

    private void Allocate(uint capacityElements)
    {
        uint elementSize = (uint)sizeof(T);

        RegionByteSize = Utils.Utils.Align(capacityElements * elementSize, _alignment);
        uint totalSize = RegionByteSize * (uint)_frameManager.FramesInFlight;

        Log.Info($"Allocating new {_debugName}-Buffer. region_size={RegionByteSize} B, total_size={totalSize} B");
        
        BufferId = _gl.CreateBuffer();
        Errors.CheckGLError("gen buffer", _gl);
        _gl.ObjectLabel(ObjectIdentifier.Buffer, BufferId, (uint)_debugName.Length, _debugName);
        
        const BufferStorageMask flags =
            BufferStorageMask.DynamicStorageBit |
            BufferStorageMask.MapWriteBit |
            BufferStorageMask.MapPersistentBit |
            BufferStorageMask.MapCoherentBit;

        _gl.NamedBufferStorage(BufferId, totalSize, null, flags);
        Errors.CheckGLError($"map buffer id: {BufferId}", _gl);

        _map = _gl.MapNamedBufferRange(BufferId, 0, totalSize,
            MapBufferAccessMask.WriteBit | MapBufferAccessMask.PersistentBit | MapBufferAccessMask.CoherentBit);
        Errors.CheckGLError("map buffer", _gl);

        if (_map == null)
        {
            _gl.DeleteBuffer(BufferId);
            BufferId = 0;

            throw new InvalidOperationException("Failed to persistently map OpenGL buffer.");
        }

        Capacity = capacityElements;
    }

    private void EnsureCapacity(uint requiredCapacity)
    {
        if (requiredCapacity <= Capacity)
        {
            return;
        }

        Grow(requiredCapacity);
    }

    private void Grow(uint targetCapacity)
    {
        uint newCapacity = Capacity;
        while (newCapacity < targetCapacity)
        {
            newCapacity *= 2;
        }
        
        Log.Info($"Growing {_debugName}-Buffer: {Capacity} -> {newCapacity}");

        Reallocate(newCapacity);
    }

    private void Reallocate(uint newCapacity)
    {
        // wait for all regions to complete
        _frameManager.WaitOnAll();

        uint oldBufferId = BufferId;
        Allocate(newCapacity);
        
        // no need to unmap, this happens automatically when deleting the buffer
        _gl.DeleteBuffer(oldBufferId);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }
    
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        
        _disposed = true;

        if (BufferId != 0)
        {
            // explicitly unmap to make sure everything is in order and check errors
            _gl.UnmapNamedBuffer(BufferId);
            Errors.CheckGLError("unmap buffer", _gl);
            
            _gl.DeleteBuffer(BufferId);

            BufferId = 0;
            _map = null;
        }
    }
    
}