using System;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL.Utils;

internal static class Errors
{

    internal static void CheckGLError(string location, GL gl)
    {
        GLEnum err;
        while ((err = gl.GetError()) != GLEnum.NoError)
        {
            Log.Error($"GL Error {err} at {location}");
            throw new Exception($"GL Error {err} at {location}");
        }
    }
    
}