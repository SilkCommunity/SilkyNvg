using System;
using System.Runtime.CompilerServices;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Utils;

namespace SilkyNvg.Rendering.OpenGL.Buffers;

internal sealed unsafe class GpuArrayList<T> : IDisposable
    where T : unmanaged
{

    private readonly string _debugName;
    
    private readonly int _framesInFlight;
    private readonly int[] _fences;
    
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

    internal GpuArrayList(uint initialCapacity, int framesInFlight, string debugName, GL gl)
    {
        ArgumentOutOfRangeException.ThrowIfZero(initialCapacity);
        ArgumentOutOfRangeException.ThrowIfLessThan(framesInFlight, 2);
        
        _debugName = debugName;
        
        _framesInFlight = framesInFlight;
        _fences = new int[_framesInFlight];
        _gl = gl;

        _currentRegion = 0;
        _disposed = false;

        Allocate(initialCapacity);
    }

    internal void Bind(uint binding)
    {
        ThrowIfDisposed();

        int offset = RegionOffset(_currentRegion);
        uint size = Count * (uint)sizeof(T);

        _gl.BindBufferRange(BufferTargetARB.ShaderStorageBuffer, binding, BufferId, offset, size);
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

    internal void BeginFrame()
    {
        ThrowIfDisposed();

        _currentRegion = (_currentRegion + 1) % _framesInFlight;

        WaitForRegion(_currentRegion);
        Count = 0;
    }

    internal void EndFrame()
    {
        ThrowIfDisposed();

        // If the previous fence should still exist, delete
        // Note that glDeleteSync waits for the fence to complete.
        if (_fences[_currentRegion] != 0)
        {
            _gl.DeleteSync(_fences[_currentRegion]);
            _fences[_currentRegion] = 0;
        }

        _fences[_currentRegion] = _gl.FenceSync(SyncCondition.SyncGpuCommandsComplete, (uint)0).ToInt32();
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

        RegionByteSize = capacityElements * elementSize;
        uint totalSize = RegionByteSize * (uint)_framesInFlight;

        Log.Info($"Allocating new {_debugName}-Buffer. region_size={RegionByteSize} B, total_size={totalSize} B");
        
        BufferId = _gl.GenBuffer();
        Errors.CheckGLError("gen buffer", _gl);

        const BufferStorageMask flags =
            BufferStorageMask.MapWriteBit |
            BufferStorageMask.MapPersistentBit |
            BufferStorageMask.MapPersistentBit;

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
        for (int i = 0; i < _framesInFlight; i++)
        {
            WaitForRegion(i);
        }

        uint oldBufferId = BufferId;
        Allocate(newCapacity);
        
        // no need to unmap, this happens automatically when deleting the buffer
        _gl.DeleteBuffer(oldBufferId);
    }

    private void WaitForRegion(int regionIndex)
    {
        int fence = _fences[regionIndex];

        if (fence == 0)
        {
            return;
        }
        
        // first try a zero-timeout poll
        var result = _gl.ClientWaitSync(fence, (uint)0, 0);
        Errors.CheckGLError("zero-timeout wait", _gl);
        if (result is GLEnum.AlreadySignaled or GLEnum.ConditionSatisfied)
        {
            _gl.DeleteSync(fence);
            _fences[regionIndex] = 0;
            return;
        }
        
        Log.Info("Waiting for region " + (regionIndex + 1) + " / " + _framesInFlight + " to complete");
        
        // actually wait
        while (true)
        {
            // try again in 0.1 s
            result = _gl.ClientWaitSync(fence, (uint)0, 1_000_000);
            Errors.CheckGLError("fence wait", _gl);
            if (result is GLEnum.AlreadySignaled or GLEnum.ConditionSatisfied)
            {
                break;
            }
            else if(result is GLEnum.WaitFailed)
            {
                throw new InvalidOperationException("glClientWaitSync failed.");
            }
        }

        _gl.DeleteSync(fence);
        _fences[regionIndex] = 0;
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

        for (int i = 0; i < _framesInFlight; i++)
        {
            if (_fences[i] != 0)
            {
                // WaitForRegion automatically deletes the fence
                WaitForRegion(i);
                _fences[i] = 0;
            }
        }

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