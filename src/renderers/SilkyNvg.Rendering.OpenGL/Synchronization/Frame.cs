using System;
using Silk.NET.OpenGL;
using SilkyNvg.Rendering.OpenGL.Utils;

namespace SilkyNvg.Rendering.OpenGL.Synchronization;

internal sealed class Frame : IDisposable
{

    private readonly int _regionIndex;
    private readonly int _framesInFlight;
    private readonly GL _gl;

    private int _fence;
    
    internal Frame(int regionIndex, int framesInFlight, GL gl)
    {
        _regionIndex = regionIndex;
        _framesInFlight = framesInFlight;
        _gl = gl;
        
        _fence = 0;
    }

    internal void Wait()
    {
        if (_fence == 0)
        {
            return;
        }
        
        // first try a zero-timeout poll
        var result = _gl.ClientWaitSync(_fence, (uint)0, 0);
        if (result is GLEnum.AlreadySignaled or GLEnum.ConditionSatisfied)
        {
            _gl.DeleteSync(_fence);
            _fence = 0;
            return;
        }
        
        Log.Info("Waiting for region " + (_regionIndex + 1) + " / " + _framesInFlight + " to complete");
        
        // actually wait
        while (true)
        {
            // try again in 0.1 s
            result = _gl.ClientWaitSync(_fence, (uint)0, 1_000_000);
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

        _gl.DeleteSync(_fence);
        _fence = 0;
    }

    internal void Fence()
    {
        // If the previous fence should still exist, delete
        // Note that glDeleteSync waits for the fence to complete.
        if (_fence != 0)
        {
            _gl.DeleteSync(_fence);
            _fence = 0;
        }

        _fence = _gl.FenceSync(SyncCondition.SyncGpuCommandsComplete, (uint)0).ToInt32();
        if (_fence == 0)
        {
            throw new InvalidOperationException("glFenceSync failed.");
        }
    }
    
    public void Dispose()
    {
        if (_fence != 0)
        {
            Wait();
            _fence = 0;
        }
    }
    
}