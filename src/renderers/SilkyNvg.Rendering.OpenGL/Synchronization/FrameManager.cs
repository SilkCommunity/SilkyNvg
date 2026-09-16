using System;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Synchronization;

internal sealed class FrameManager : IDisposable
{
    
    private readonly Frame[] _frames;

    public int FramesInFlight { get; }

    internal int CurrentFrame { get; private set; }
    
    internal FrameManager(int framesInFlight, GL gl)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(framesInFlight, 2);
        
        FramesInFlight = framesInFlight;
        
        _frames = new Frame[framesInFlight];
        for (int i = 0; i < FramesInFlight; i++)
        {
            _frames[i] = new Frame(i, FramesInFlight, gl);
        }

        CurrentFrame = 0;
    }

    public void BeginFrame()
    {
        CurrentFrame = (CurrentFrame + 1) % FramesInFlight;
        _frames[CurrentFrame].Wait();
    }

    internal void EndFrame()
    {
        _frames[CurrentFrame].Fence();
    }

    internal void WaitOnAll()
    {
        for (int i = 0; i < FramesInFlight; i++)
        {
            _frames[i].Wait();
        }
    }

    public void Dispose()
    {
        for (int i = 0; i < FramesInFlight; i++)
        {
            _frames[i].Dispose();
        }
    }
    
}