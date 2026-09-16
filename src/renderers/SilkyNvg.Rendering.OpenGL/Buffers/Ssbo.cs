using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Utils;

namespace SilkyNvg.Rendering.OpenGL.Buffers;

internal sealed class Ssbo<T> : IDisposable
    where T : unmanaged
{

    private readonly string _debugName;
    private readonly uint _elementSize;
    private readonly BufferStorageMask _storageMask;
    
    private readonly int _framesInFlight;
    private readonly int[] _fences;
    
    private readonly GL _gl;

    private uint _regionSizeBytes;
    private int _currentRegion;
    
    private uint _bufferId;

    internal uint Capacity { get; private set; }
    
    internal Ssbo(int framesInFlight, string debugName, BufferStorageMask storageMask, GL gl)
    {
        _framesInFlight = framesInFlight;
        _debugName = debugName;
        _storageMask = storageMask;
        _gl = gl;
        
        _fences = new int[framesInFlight];
        _elementSize = (uint)Marshal.SizeOf<T>();
        
        Capacity = 0;
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

    private int RegionOffset(int region)
    {
        return region * (int)_regionSizeBytes;
    }

    private unsafe void Allocate(uint capacityElements)
    {
        _regionSizeBytes = capacityElements * _elementSize;
        uint totalSize = _regionSizeBytes * (uint)_framesInFlight;
        
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
        for (int i = 0; i < _framesInFlight; i++)
        {
            WaitForRegion(i);
        }

        uint oldBufferId = _bufferId;
        Allocate(newCapacity);

        _gl.DeleteBuffer(oldBufferId);
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

    internal void Bind(uint binding)
    {
        int offset = RegionOffset(_currentRegion);
        uint size = _regionSizeBytes;

        _gl.BindBufferRange(BufferTargetARB.ShaderStorageBuffer, binding, _bufferId, offset, size);
    }

    internal void BeginFrame()
    {
        _currentRegion = (_currentRegion + 1)  % _framesInFlight;
        WaitForRegion(_currentRegion);
    }

    internal void EndFrame()
    {
        if (_fences[_currentRegion] != 0)
        {
            _gl.DeleteSync(_fences[_currentRegion]);
            _fences[_currentRegion] = 0;
        }

        _fences[_currentRegion] = _gl.FenceSync(SyncCondition.SyncGpuCommandsComplete, (uint)0).ToInt32();
        Errors.CheckGLError("create fence", _gl);
    }
    
    public void Dispose()
    {
        for (int i = 0; i < _framesInFlight; i++)
        {
            if (_fences[i] != 0)
            {
                WaitForRegion(i);
                _fences[i] = 0;
            }
        }

        if (_bufferId != 0)
        {
            _gl.DeleteBuffer(_bufferId);
            _bufferId = 0;
        }
    }
    
}